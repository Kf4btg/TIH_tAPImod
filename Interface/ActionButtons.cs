using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    // FIXME: Port this file over to the new ButtonFactory functions for button creation.
    public class InventoryButtons : ButtonLayer
    {
        //put these above the coins/ammo slot to be less intrusive.
        private readonly float[] posX = { 496, 532 }; //X-pos of the two buttons
        private const float posY = 28; //maintain height

        private static readonly Dictionary<TIH,float> PosX = new Dictionary<TIH, float> {
            {TIH.SortInv, 496},
            {TIH.RSortInv, 496},
            {TIH.CleanInv, 532}
        };

        /// make the bg slightly translucent even at max button opacity
        private readonly Color bgColor = Constants.InvSlotColor*0.8f;

        public InventoryButtons(IHBase mbase) : base("InventoryButtons")
        {
            // TODO: use the ButtonLabels array from Constants.cs
            string[] labels = {"Sort", "Sort (Reverse)", "Clean Stacks"};

            /** --Create Sort and Stack Buttons-- **/
            int i=-1;
            string L;
            ButtonState[] states =
            {
                // "Sort"
                new ButtonState(TIH.SortInv, L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = () => IHOrganizer.SortPlayerInv(Main.localPlayer, IHBase.ModOptions["ReverseSortPlayer"])
                },
                //"Sort (Reverse)"
                new ButtonState(TIH.RSortInv, L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = () => IHOrganizer.SortPlayerInv(Main.localPlayer, !IHBase.ModOptions["ReverseSortPlayer"])
                },
                //Clean Stacks
                new ButtonState(TIH.CleanInv, L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = () => IHOrganizer.ConsolidateStacks(Main.localPlayer.inventory, 0, 50)
                }
            };

            //create anon types w/ named fields, simply for clarity's sake
            var sortButton  = new { label=labels[0], forward = states[0], reversed = states[1], toggleKey = KState.Special.Shift, position = new Vector2(posX[0], posY) };
            var stackButton = new { label=labels[2], face = states[2], position = new Vector2(posX[1], posY) };

            // add buttons to main button collection
            mbase.ButtonRepo.Add (
                sortButton.label,
                new IHDynamicButton( sortButton.forward, sortButton.reversed, sortButton.toggleKey, sortButton.position )
            );

            mbase.ButtonRepo.Add (
                stackButton.label,
                new IHButton( stackButton.face, stackButton.position )
            );

            // and now add to this ButtonLayer's Buttons collection
            Buttons.Add(TIH.SortInv,  new ButtonBase(this, mbase.ButtonRepo[sortButton.label]));
            Buttons.Add(TIH.CleanInv, new ButtonBase(this, mbase.ButtonRepo[stackButton.label]));
        }

        public InventoryButtons(IHBase mbase, bool replace) : base("InventoryButtons")
        {
            var actions = new TIH[] { TIH.SortInv, TIH.RSortInv, TIH.CleanInv };

            // create a button for each action & add it to the main ButtonRepo.
            // also create a button base and add it to the Buttons
            // collection for each action other than RSort,
            // which is instead registered as a toggle for Sort
            // (this actually eliminates the need for IHDynamicButton)
            foreach (var a in actions)
            {
                // uses default label for the action
                var button = ButtonFactory.GetSimpleButton(a, new Vector2(PosX[a], posY));
                mbase.ButtonRepo.Add(button.Label, button);

                if (a != TIH.RSortInv)
                    Buttons.Add(a, new ButtonBase(this, button));
                else
                    Buttons[TIH.SortInv].RegisterKeyToggle(KState.Special.Shift, button);
            }
        }

        /*************************************************************************/

        /// Draw each button in this layer (bg first, then button)
        protected override void DrawButtons(SpriteBatch sb)
        {
            foreach (KeyValuePair<TIH, ButtonBase> kvp in Buttons)
            {
                sb.DrawButtonBG(kvp.Value, IHBase.ButtonBG, bgColor);
                kvp.Value.Draw(sb);
            }
        }
    }

    public class ChestButtons : ButtonLayer
    {
        private static readonly float[] posX = {
            453 - (3*Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE),   //leftmost
            453 - (2*Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE),   //middle
            453 - (Main.inventoryBackTexture.Width   * Constants.CHEST_INVENTORY_SCALE)    //right beside trash
        };

        private static readonly Dictionary<TIH,float> PosX = new Dictionary<TIH,float>{
            {TIH.SortChest, posX[0]},   //leftmost
            {TIH.RSortChest, posX[0]},   //leftmost
            {TIH.SmartLoot, posX[1]},   //middle
            {TIH.QuickStack, posX[1]},   //middle
            {TIH.SmartDep, posX[2]},    //right beside trash
            {TIH.DepAll, posX[2]}    //right beside trash
        };

        private static readonly Dictionary<TIH, TIH> togglesWith = new Dictionary<TIH, TIH>{
            {TIH.RSortChest, TIH.SortChest},
            {TIH.QuickStack, TIH.SmartLoot},
            {TIH.DepAll, TIH.SmartDep}
        };

        private readonly float posY = API.main.invBottom + (224*Constants.CHEST_INVENTORY_SCALE) + 4;

        //position offset for the "locked" icon on QS/DA
        private static readonly Vector2 lockOffset = new Vector2((float)(int)((float)Constants.ButtonW/2),
                                                         -(float)(int)((float)Constants.ButtonH/2));

        private static readonly Color bgColor = Constants.ChestSlotColor*0.8f;

        public ChestButtons(IHBase mbase) : base("ChestButtons")
        {
            // TODO: use the ButtonLabels array from Constants.cs
            string[] labels = {
                "Sort Chest",               //0
                "Sort Chest (Reverse)",     //1
                "Restock",                  //2
                "Quick Stack",              //3
                "Quick Stack (Locked)",     //4
                "Smart Deposit",            //5
                "Deposit All",              //6
                "Deposit All (Locked)"      //7
            };

            int i=-1;
            string L;

            ButtonState[] states = {
                // 0 -- "Sort Chest"
                new ButtonState(TIH.SortChest, L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = () => IHOrganizer.SortChest(Main.localPlayer.chestItems, IHBase.ModOptions["ReverseSortChest"])
                },
                // 1 -- "Sort Chest (Reverse)"
                new ButtonState(TIH.RSortChest, L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = () => IHOrganizer.SortChest(Main.localPlayer.chestItems, !IHBase.ModOptions["ReverseSortChest"])
                },
                // 2 -- "Restock",
                new ButtonState(TIH.SmartLoot, L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = IHSmartStash.SmartLoot
                },
                // 3 -- "Quick Stack",
                new ButtonState(TIH.QuickStack, L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = IHUtils.DoQuickStack,
                    onRightClick = () =>
                    {
                        Main.PlaySound(22); //lock sound
                        IHPlayer.ToggleActionLock(Main.localPlayer, TIH.QuickStack);
                    }
                },
                // 4 -- "Quick Stack (Locked)",
                new ButtonState(TIH.QuickStack, L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = IHUtils.DoQuickStack,
                    onRightClick = () =>
                    {
                        Main.PlaySound(22); //lock sound
                        IHPlayer.ToggleActionLock(Main.localPlayer, TIH.QuickStack);
                    },
                    // use buttonstate's PostDraw hook to draw the lock indicator
                    PostDraw = (sb, bBase) => sb.Draw(IHBase.LockedIcon, bBase.Position + lockOffset,
                                    Color.Firebrick*this.LayerOpacity*bBase.Alpha) //draw lock
                },
                // 5 -- "Smart Deposit",
                new ButtonState(TIH.SmartDep, L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = IHSmartStash.SmartDeposit
                },
                // 6 -- "Deposit All",
                new ButtonState(TIH.DepAll, L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = IHUtils.DoDepositAll,
                    onRightClick = () =>
                    {
                        Main.PlaySound(22); //lock sound
                        IHPlayer.ToggleActionLock(Main.localPlayer, TIH.DepAll);
                    }
                },
                // 7 -- "Deposit All (Locked)"
                new ButtonState(TIH.DepAll, L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = IHUtils.DoDepositAll,
                    onRightClick = () =>
                    {
                        Main.PlaySound(22); //lock sound
                        IHPlayer.ToggleActionLock(Main.localPlayer, TIH.DepAll);
                    },
                    // use buttonstate's PostDraw hook to draw the lock indicator
                    PostDraw = (sb, bBase) => sb.Draw(IHBase.LockedIcon, bBase.Position + lockOffset,
                                    Color.Firebrick*this.LayerOpacity*bBase.Alpha) },
            };

            // make some anonymous types to help with readability
            var sort  = new { label=labels[0], forward = states[0], reversed = states[1],
                            toggleKey = KState.Special.Shift, position = new Vector2(posX[0], posY) };

            var restock    = new { label = labels[2], face     = states[2], position = new Vector2(posX[1], posY) };
            var quickStack = new { label = labels[3], unlocked = states[3], locked   = states[4], toggleOnRC = true };

            var smartDep   = new { label = labels[5], face     = states[5], position = new Vector2(posX[2], posY) };
            var depositAll = new { label = labels[6], unlocked = states[6], locked   = states[7], toggleOnRC = true };

            //add all the buttons to the repo
            mbase.ButtonRepo.Add (
                sort.label,
                new IHDynamicButton( sort.forward, sort.reversed, sort.toggleKey, sort.position )
                );

            mbase.ButtonRepo.Add (
                restock.label,
                new IHButton(restock.face, restock.position)
                );
            mbase.ButtonRepo.Add (
                quickStack.label,
                new IHToggle(quickStack.unlocked, quickStack.locked,
                            () => IHPlayer.ActionLocked(Main.localPlayer, TIH.QuickStack),
                            null, quickStack.toggleOnRC)
                );

            mbase.ButtonRepo.Add (
                smartDep.label,
                new IHButton(smartDep.face, smartDep.position)
                );
            mbase.ButtonRepo.Add (
                depositAll.label,
                new IHToggle(depositAll.unlocked, depositAll.locked,
                            () => IHPlayer.ActionLocked(Main.localPlayer, TIH.DepAll),
                            null, depositAll.toggleOnRC)
                );

            // set QS & DA to have their state initialized on world load
            mbase.ButtonUpdates.Push(quickStack.label);
            mbase.ButtonUpdates.Push(depositAll.label);

            // add the three ButtonBases to this layer
            Buttons.Add(TIH.SortChest, new ButtonBase(this, mbase.ButtonRepo[sort.label]));
            Buttons.Add(TIH.SmartLoot, new ButtonBase(this, mbase.ButtonRepo[restock.label]));
            Buttons.Add(TIH.SmartDep,  new ButtonBase(this, mbase.ButtonRepo[smartDep.label]));

            //now create keywatchers to toggle Restock/QS & SD/DA
            Buttons[TIH.SmartLoot].RegisterKeyToggle( KState.Special.Shift, restock.label,  quickStack.label );
            Buttons[TIH.SmartDep]. RegisterKeyToggle( KState.Special.Shift, smartDep.label, depositAll.label );
        }

        public ChestButtons(IHBase mbase, bool replace) : base("ChestButtons")
        {
            var simpleActions = new TIH[] { TIH.SortChest, TIH.RSortChest, TIH.SmartDep, TIH.SmartLoot };
            var lockingActions = new TIH[] { TIH.QuickStack, TIH.DepAll };

            foreach (var a in simpleActions)
            {
                // uses default label for the action
                var button = ButtonFactory.GetSimpleButton(a, new Vector2(PosX[a], posY));
                mbase.ButtonRepo.Add(button.Label, button);

                if (a != TIH.RSortChest)
                    Buttons.Add(a, new ButtonBase(this, button));
                else
                    Buttons[TIH.SortChest].RegisterKeyToggle(KState.Special.Shift, button);
            }

            foreach (var a in lockingActions)
            {
                var button = ButtonFactory.GetLockableButton(a, new Vector2(PosX[a], posY), this, lockOffset);
                mbase.ButtonRepo.Add(button.Label, button);
                // set QS & DA to have their state initialized on world load
                mbase.ButtonUpdates.Push(button.Label);

                Buttons[togglesWith[a]].RegisterKeyToggle(KState.Special.Shift, button);
            }
        }

        /*************************************************************************
         * Draw each button in this layer (bg first, then button)
         */
        protected override void DrawButtons(SpriteBatch sb)
        {
            foreach (KeyValuePair<TIH, ButtonBase> kvp in Buttons)
            {
                sb.DrawButtonBG(kvp.Value, IHBase.ButtonBG, bgColor);
                kvp.Value.Draw(sb);
            }
        }
    }
}
