using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class InventoryButtons : ButtonLayer
    {
        //put these above the coins/ammo slot to be less intrusive.
        private readonly float[] posX = { 496, 532 }; //X-pos of the two buttons
        private const float posY = 28; //maintain height

        private readonly Color bgColor = Constants.InvSlotColor*0.8f; //make the bg translucent even at max button opacity

        public InventoryButtons(IHBase mbase) : base("InventoryButtons")
        {
            string[] labels = {"Sort", "Sort (Reverse)", "Clean Stacks"};

            /** --Create Sort and Stack Buttons-- **/
            int i=-1;
            string L;
            ButtonState[] states = {

                // "Sort"
                new ButtonState( L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = () => IHOrganizer.SortPlayerInv(Main.localPlayer, IHBase.ModOptions["ReverseSortPlayer"]) },

                //"Sort (Reverse)"
                new ButtonState( L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = () => IHOrganizer.SortPlayerInv(Main.localPlayer, !IHBase.ModOptions["ReverseSortPlayer"]) },

                //Clean Stacks
                new ButtonState( L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = () => IHOrganizer.ConsolidateStacks(Main.localPlayer.inventory, 0, 50) }
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
            Buttons.Add(IHAction.Sort,  new ButtonBase(this, mbase.ButtonRepo[sortButton.label]));
            Buttons.Add(IHAction.Stack, new ButtonBase(this, mbase.ButtonRepo[stackButton.label]));
        }

        /*************************************************************************
         * Draw each button in this layer (bg first, then button)
         */
        protected override void DrawButtons(SpriteBatch sb)
        {
            foreach (KeyValuePair<IHAction, ButtonBase> kvp in Buttons)
            {
                sb.DrawButtonBG(kvp.Value, IHBase.ButtonBG, bgColor);
                kvp.Value.Draw(sb);
            }
        }
    }

    public class ChestButtons : ButtonLayer
    {
        // private float[] posX = {
        //     453 - (3*Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE),   //leftmost
        //     453 - (2*Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE),   //middle
        //     453 - (Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE)      //right beside trash
        // };

        // X-val of Trash slot = 453; calc positions of the buttons from that
        private float PosX(int posIndex)
        {
            return 453 - ((posIndex+1)*Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE);
        }
        private readonly float posY = API.main.invBottom + (224*Constants.CHEST_INVENTORY_SCALE) + 4;

        //position offset for the "locked" icon on QS/DA
        private readonly Vector2 lockOffset = new Vector2((float)(int)((float)Constants.ButtonW/2), -(float)(int)((float)Constants.ButtonH/2));

        private readonly Color bgColor = Constants.ChestSlotColor*0.8f;

        public ChestButtons(IDictionary<String, IHButton> buttonRepo, Queue<String> updateQueue) : base("ChestButtons")
        {
            //create differently depending on state of "use-locks" options
            if (IHBase.ModOptions["LockingEnabled"])
                CB_Locks(buttonRepo, updateQueue);
            else
                CB_NoLocks(buttonRepo, updateQueue);
        }

        private void CB_Locks(IDictionary<String, IHButton> buttonRepo, Queue<String> updateQueue)
        {
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
                //   "Sort Chest"
                new ButtonState( L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = () => IHOrganizer.SortChest(Main.localPlayer.chestItems, IHBase.ModOptions["ReverseSortChest"]) },

                //  "Sort Chest (Reverse)"
                new ButtonState( L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = () => IHOrganizer.SortChest(Main.localPlayer.chestItems, !IHBase.ModOptions["ReverseSortChest"]) },

                // "Restock",                  //2
                new ButtonState( L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = IHSmartStash.SmartLoot },

                // "Quick Stack",              //3
                new ButtonState( L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = IHUtils.DoQuickStack,
                    onRightClick = () => {
                        Main.PlaySound(22); //lock sound
                        IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.QS); } },

                // "Quick Stack (Locked)",     //4
                new ButtonState( L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = IHUtils.DoQuickStack,
                    onRightClick = () => {
                        Main.PlaySound(22); //lock sound
                        IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.QS); },
                    PostDraw = (sb, bBase) => sb.Draw(IHBase.lockedIcon, bBase.Position + lockOffset, Color.Firebrick*this.LayerOpacity*bBase.Alpha) }, //draw lock

                // "Smart Deposit",            //5
                new ButtonState( L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = IHSmartStash.SmartDeposit },

                // "Deposit All",              //6
                new ButtonState( L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = IHUtils.DoDepositAll,
                    onRightClick = () => {
                        Main.PlaySound(22); //lock sound
                        IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.DA); } },

                // "Deposit All (Locked)"      //7
                new ButtonState( L=labels[++i], IHBase.ButtonGrid, IHUtils.GetSourceRect(L), IHUtils.GetSourceRect(L,true) )
                {
                    onClick = IHUtils.DoDepositAll,
                    onRightClick = () => {
                        Main.PlaySound(22); //lock sound
                        IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.DA); },
                    PostDraw = (sb, bBase) => sb.Draw(IHBase.lockedIcon, bBase.Position + lockOffset, Color.Firebrick*this.LayerOpacity*bBase.Alpha) },
            };

            // make some anonymous types to help with readability
            var sort  = new { label=labels[0], forward = states[0], reversed = states[1], toggleKey = KState.Special.Shift, position = new Vector2(PosX(0), posY) };

            var restock    = new { label = labels[2], face     = states[2], position = new Vector2(PosX(1), posY) };
            var quickStack = new { label = labels[3], unlocked = states[3], locked   = states[4], toggleOnRC = true };

            var smartDep   = new { label = labels[5], face     = states[5], position = new Vector2(PosX(2), posY) };
            var depositAll = new { label = labels[6], unlocked = states[6], locked   = states[7], toggleOnRC = true };

            //add all the buttons to the repo
            mbase.ButtonRepo.Add ( sort.label,
                new IHDynamicButton( sort.forward, sort.reversed, sort.toggleKey, sort.position ) );

            mbase.ButtonRepo.Add ( restock.label,
                new IHButton(restock.face, restock.position) );
            mbase.ButtonRepo.Add ( quickStack.label,
                new IHToggle(quickStack.unlocked, quickStack.locked,
                            () => IHPlayer.ActionLocked(Main.localPlayer, IHAction.QS), //IsActive()
                            restock.position, quickStack.toggleOnRC) ); //position is same as restock

            mbase.ButtonRepo.Add ( smartDep.label,
                new IHButton(smartDep.face, smartDep.position) );
            mbase.ButtonRepo.Add ( depositAll.label,
                new IHToggle(depositAll.unlocked, depositAll.locked,
                            () => IHPlayer.ActionLocked(Main.localPlayer, IHAction.DA),
                            smartDep.position, depositAll.toggleOnRC) );

            // set QS & DA to have their state initialized on world load
            mbase.ButtonUpdates.Push(quickStack.label);
            mbase.ButtonUpdates.Push(depositAll.label);

            // add the three ButtonBases to this layer
            Buttons.Add(IHAction.Sort,    new ButtonBase(this, mbase.ButtonRepo[sort.label]));

            //different setup depending on whether we're showing Vanilla or TIH buttons by default
            if (IHBase.ModOptions["ShowVanillaButtons"]){
                Buttons.Add(IHAction.QS, new ButtonBase(this, mbase.ButtonRepo[quickStack.label]));
                Buttons.Add(IHAction.DA, new ButtonBase(this, mbase.ButtonRepo[depositAll.label]));

                //now create keywatchers to toggle Restock/QS & SD/DA
                Buttons[IHAction.QS].RegisterKeyToggle( KState.Special.Shift,  quickStack.label, restock.label);
                Buttons[IHAction.DA].RegisterKeyToggle(KState.Special.Shift, depositAll.label, smartDep.label);
            } else {
                Buttons.Add(IHAction.Refill,  new ButtonBase(this, mbase.ButtonRepo[restock.label]));
                Buttons.Add(IHAction.Deposit, new ButtonBase(this, mbase.ButtonRepo[smartDep.label]));

                //now create keywatchers to toggle Restock/QS & SD/DA
                Buttons[IHAction.Refill].RegisterKeyToggle( KState.Special.Shift, restock.label,  quickStack.label);
                Buttons[IHAction.Deposit].RegisterKeyToggle(KState.Special.Shift, smartDep.label, depositAll.label);
            }
        }

        private void CB_NoLocks(IDictionary<String, IHButton> buttonRepo, Queue<String> updateQueuembase)
        {
            string[] labels = {
                "Sort Chest",               //0
                "Sort Chest (Reverse)",     //1
                "Restock",                  //2
                "Quick Stack",              //3
                "Smart Deposit",            //5
                "Deposit All",              //6
            };

            int i=-1;
            string L;

            ButtonState[] states = {
        }

        /*************************************************************************
         * Draw each button in this layer (bg first, then button)
         */
        protected override void DrawButtons(SpriteBatch sb)
        {
            foreach (KeyValuePair<IHAction, ButtonBase> kvp in Buttons)
            {
                sb.DrawButtonBG(kvp.Value, IHBase.ButtonBG, bgColor);
                kvp.Value.Draw(sb);
            }
        }
    }
}
