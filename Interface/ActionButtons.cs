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
        private float posX = 496;
        private const float posY = 28; //maintain height

        // these dictionaries are all keyed on the state's label
        private readonly Dictionary<String,ButtonState> States = new Dictionary<String,ButtonState>();
        private readonly Dictionary<String,Rectangle?> Rects   = new Dictionary<String,Rectangle?>(); // source rectangle for the button's inactive appearance
        private readonly Dictionary<String,Rectangle?> MRects  = new Dictionary<String,Rectangle?>(); // source rect for mouseover appearance

        private readonly Color bgColor = Constants.InvSlotColor*0.8f; //make the bg translucent even at max button opacity

        public InventoryButtons(IHBase mbase) : base("InventoryButtons")
        {
            /** --Create Sort Button-- **/
            //default state
            String label = "Sort";
            SetUpStateBasics(label);
            States[label].onClick = () => IHOrganizer.SortPlayerInv(Main.localPlayer, IHBase.ModOptions["ReverseSortPlayer"]);

            //altState
            label = "Sort (Reverse)";
            SetUpStateBasics(label);

            States[label].onClick = () => IHOrganizer.SortPlayerInv(Main.localPlayer, !IHBase.ModOptions["ReverseSortPlayer"]);

            mbase.ButtonRepo.Add("Sort",
                new IHDynamicButton(
                    States["Sort"],
                    States["Sort (Reverse)"],
                    KState.Special.Shift,
                    new Vector2(posX, posY)
                    ) );

            Buttons.Add(IHAction.Sort,
                        new ButtonBase(this, mbase.ButtonRepo["Sort"]) );

            /** --Create Stack Button-- **/
            posX = 532;

            label = "Clean Stacks";
            SetUpStateBasics(label);

            States[label].onClick = () => IHOrganizer.ConsolidateStacks(Main.localPlayer.inventory, 0, 50);

            mbase.ButtonRepo.Add(label,  new IHButton(States[label], new Vector2(posX, posY)) );

            Buttons.Add(IHAction.Stack, new ButtonBase(this,mbase.ButtonRepo[label]));
        }

        /*************************************************************************
         * Consolidate the operations that are similar for every new button state
         */
        private void SetUpStateBasics(String label)
        {
            Rects.Add(label,IHUtils.GetSourceRect(label)); //inactive appearance
            MRects.Add(label, IHUtils.GetSourceRect(label,true)); //mouse-over

            var bState = new ButtonState(label);

            bState.texture       = IHBase.ButtonGrid;
            bState.defaultTexels = Rects[label];
            bState.altTexels     = MRects[label];

            States.Add(label, bState);
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
        private float posX;
        private readonly float posY = API.main.invBottom + (224*Constants.CHEST_INVENTORY_SCALE) + 4;

        //position offset for the "locked" icon on QS/DA
        private readonly Vector2 lockOffset = new Vector2((float)(int)((float)Constants.ButtonW/2), -(float)(int)((float)Constants.ButtonH/2));

        //just use this index internally...
        private readonly string[] label = {
            "Sort Chest",               //0
            "Sort Chest (Reverse)",     //1
            "Restock",                  //2
            "Quick Stack",              //3
            "Quick Stack (Locked)",     //4
            "Smart Deposit",            //5
            "Deposit All",              //6
            "Deposit All (Locked)"      //7
        };

        private readonly ButtonState[] States = new ButtonState[8];
        private readonly Rectangle?[] Rects   = new Rectangle?[8];
        private readonly Rectangle?[] MRects  = new Rectangle?[8];

        private readonly Color bgColor = Constants.ChestSlotColor*0.8f;

        public ChestButtons(IHBase mbase) : base("ChestButtons")
        {
            //*******************************//
            // --Create Sort Chest Button--  //
            //*******************************//
            posX = 453 - (3*Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE); //leftmost

            //  label   = "Sort Chest";
            SetUpStateBasics(0);
            States[0].onClick = () => IHOrganizer.SortChest(Main.localPlayer.chestItems, IHBase.ModOptions["ReverseSortChest"]);

            // label   = "Sort Chest (Reverse)";
            SetUpStateBasics(1);
            States[1].onClick = () => IHOrganizer.SortChest(Main.localPlayer.chestItems, !IHBase.ModOptions["ReverseSortChest"]);

            mbase.ButtonRepo.Add(label[0],  new IHDynamicButton(
               States[0], States[1],
               KState.Special.Shift,
               new Vector2(posX,posY)) );

            Buttons.Add(IHAction.Sort, new ButtonBase(this, mbase.ButtonRepo[label[0]]));

            //*********************************//
            // Create Refill/QuickStack Button //
            //*********************************//
            posX = 453 - (2*Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE);

            // label   = "Quick Restock";
            SetUpStateBasics(2);
            States[2].onClick = IHSmartStash.SmartLoot;

            mbase.ButtonRepo[label[2]] = new IHButton(States[2], new Vector2(posX, posY));

            Buttons.Add(IHAction.Refill, new ButtonBase(this, mbase.ButtonRepo[label[2]]));

            // label = "Quick Stack";
            SetUpStateBasics(3);

            //quickstack is 2-state toggle button (locked/unlocked) that toggles on right click
            // create default unlocked state
            States[3].onClick = IHUtils.DoQuickStack;
            States[3].onRightClick = () => {
                Main.PlaySound(22); //lock sound
                IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.QS); };

            // create locked state
            // label = "Quick Stack (Locked)";
            SetUpStateBasics(4);

            States[4].onClick      = States[3].onClick;
            States[4].onRightClick = States[3].onRightClick;

            //draw the lock icon
            States[4].PostDraw =
                (sb, bBase) => sb.Draw(IHBase.lockedIcon, bBase.Position + lockOffset, Color.Firebrick*this.LayerOpacity*bBase.Alpha);

            // create the button, setting its state from ActionLocked()
            mbase.ButtonRepo[label[3]] = new IHToggle(
                States[3],
                States[4],
                () => IHPlayer.ActionLocked(Main.localPlayer, IHAction.QS), //IsActive()
                null, //can leave position null since position for this button has already been set
                true); //toggleOnRightClick=true

            mbase.ButtonUpdates.Push(label[3]); //set its state to be initialized on World Load

            //now create keywatchers to toggle the button from restock to quickstack when Shift is pressed
            Buttons[IHAction.Refill].RegisterKeyToggle(KState.Special.Shift, label[2], label[3]);

            //******************************************//
            // --Create SmartStash/DepositAll Button--  //
            //******************************************//
            posX = 453 - (Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE); //right beside trash

            // label   = "Smart Deposit";
            SetUpStateBasics(5);
            States[5].onClick = IHSmartStash.SmartDeposit;

            mbase.ButtonRepo[label[5]] = new IHButton(States[5], new Vector2(posX, posY));
            Buttons.Add(IHAction.Deposit, new ButtonBase(this, mbase.ButtonRepo[label[5]]));

            // label   = "Deposit All (Unlocked)";  --> InactiveState
            SetUpStateBasics(6);
            States[6].onClick = IHUtils.DoDepositAll;
            States[6].onRightClick = () => {
                Main.PlaySound(22); //lock sound
                IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.DA); };

            // label   = "Deposit All (Locked)";  --> ActiveState
            SetUpStateBasics(7);

            States[7].onClick      = States[6].onClick;
            States[7].onRightClick = States[6].onRightClick;

            States[7].PostDraw =
                (sb, bBase) => sb.Draw(IHBase.lockedIcon, bBase.Position + lockOffset, Color.Firebrick*this.LayerOpacity*bBase.Alpha);

            mbase.ButtonRepo[label[6]] = new IHToggle(States[6], States[7],
                () => IHPlayer.ActionLocked(Main.localPlayer, IHAction.DA), //IsActive()
                null, true);
            mbase.ButtonUpdates.Push(label[6]);

            //create keywatchers
            Buttons[IHAction.Deposit].RegisterKeyToggle(KState.Special.Shift, label[5], label[6]);

            //increase base alpha of chest buttons TODO: test if this is still necessary
            // Buttons[IHAction.Deposit].Alpha = Buttons[IHAction.Refill].Alpha = Buttons[IHAction.Sort].Alpha = 0.8f;
        }

        /*************************************************************************
         * Consolidate the operations that are similar for every new button state
         */
        private void SetUpStateBasics(int index)
        {
            Rects[index] = IHUtils.GetSourceRect(label[index]); //inactive appearance
            MRects[index] = IHUtils.GetSourceRect(label[index], true); //mouse-over

            var bState = new ButtonState(label[index]);

            bState.texture       = IHBase.ButtonGrid;
            bState.defaultTexels = Rects[index];
            bState.altTexels     = MRects[index];

            States[index] = bState;
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
