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
        //let's try to put these above the coins/ammo slot, instead. Should be less intrusive.
        private float posX = 496;
        private const float posY = 28; //doesn't change

        private readonly Dictionary<String,ButtonState> States = new Dictionary<String,ButtonState>();
        private readonly Dictionary<String,Rectangle?> Rects   = new Dictionary<String,Rectangle?>();
        private readonly Dictionary<String,Rectangle?> MRects  = new Dictionary<String,Rectangle?>();
        private readonly Color bgColor = Constants.InvSlotColor*0.8f;

        public InventoryButtons() : base("InventoryButtons")
        {
            /*
            Main.inventoryScale=0.85)
            */
            /** --Create Sort Button-- **/
            //default state
            String label = "Sort";
            SetUpStateBasics(label);
            States[label].onClick = () => IHOrganizer.SortPlayerInv(Main.localPlayer, IHBase.ModOptions["ReverseSortPlayer"]);

            //altState

            label = "Sort (Reverse)";
            SetUpStateBasics(label);

            States[label].onClick = () => IHOrganizer.SortPlayerInv(Main.localPlayer, !IHBase.ModOptions["ReverseSortPlayer"]);

            Buttons.Add(    IHAction.Sort,
                            new ButtonBase( this,
                                new IHContextButton(
                                    States["Sort"],
                                    States["Sort (Reverse)"],
                                    KState.Special.Shift,
                                    new Vector2(posX, posY)
                                )
                            )
                        );

            /** --Create Stack Button-- **/

            posX = 532;

            label = "Clean Stacks";
            SetUpStateBasics(label);

            States[label].onClick = () => IHOrganizer.ConsolidateStacks(Main.localPlayer.inventory, 0, 50);

            Buttons.Add(IHAction.Stack, new ButtonBase(this, new IHButton(States[label], new Vector2(posX, posY))));

            // UpdateFrame();
        }

        private void SetUpStateBasics(String label)
        {
            States.Add(label, new ButtonState(label));

            Rects.Add(label,IHUtils.GetSourceRect(label)); //inactive appearance
            MRects.Add(label, IHUtils.GetSourceRect(label,true)); //mouse-over

            States[label].texture      = IHBase.ButtonGrid;
            States[label].sourceRect   = Rects[label];
            States[label].onMouseEnter = bb => { bb.CurrentState.sourceRect = MRects[label]; return true;};
            States[label].onMouseLeave = bb => { bb.CurrentState.sourceRect = Rects[label]; return true;};
            // States[label].tint         = btnTint;
        }

        protected override void OnDraw(SpriteBatch sb)
        {
            if (!parentLayer.visible) return;
            // foreach (KeyValuePair<IHAction, ButtonBase> kvp in Buttons)
            // {
            //     sb.DrawButtonBG(kvp.Value, IHBase.ButtonBG, bgColor);
            // }

            foreach (KeyValuePair<IHAction, ButtonBase> kvp in Buttons)
            {
                sb.DrawButtonBG(kvp.Value, IHBase.ButtonBG, bgColor);
                kvp.Value.Draw(sb);
            }
        }
    }

    public class ChestButtons : ButtonLayer
    {
        /*if this is true (will likely be a mod-option), replace the text buttons to the right of chests with multifunctional icons*/
        // public readonly bool replaceVanilla = false;

        private float posX; // = 453 - (Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE);
        private readonly float posY = API.main.invBottom + (224*Constants.CHEST_INVENTORY_SCALE) + 4; //doesn't change

        private readonly Dictionary<String,ButtonState> States = new Dictionary<String,ButtonState>();
        private readonly Dictionary<String,Rectangle?> Rects   = new Dictionary<String,Rectangle?>();
        private readonly Dictionary<String,Rectangle?> MRects  = new Dictionary<String,Rectangle?>();
        private readonly Color bgColor = Constants.ChestSlotColor*0.8f;

        public ChestButtons() : base("ChestButtons")
        {

            // --Create Sort Button-- //
            posX = 453 - (3*Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE); //leftmost

            String label   = "Sort Chest";
            SetUpStateBasics(label);
            States[label].onClick = () => IHOrganizer.SortChest(Main.localPlayer.chestItems, IHBase.ModOptions["ReverseSortChest"]);

            label   = "Sort Chest (Reverse)";
            SetUpStateBasics(label);
            States[label].onClick = () => IHOrganizer.SortChest(Main.localPlayer.chestItems, !IHBase.ModOptions["ReverseSortChest"]);

            Buttons.Add(IHAction.Sort, new ButtonBase(this,
                new IHContextButton(
                    States["Sort Chest"], States["Sort Chest (Reverse)"],
                    KState.Special.Shift,
                    new Vector2(posX,posY))
                ));


            // if (replaceVanilla){}
            // else{

            // --Create Refill/QuickStack Button-- //
            posX = 453 - (2*Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE);

            label   = "Quick Restock";
            SetUpStateBasics(label);
            States[label].onClick = IHSmartStash.SmartLoot;

            //create button
            var QRbutton = new IHButton(States[label], new Vector2(posX, posY));

            Buttons.Add(IHAction.Refill, new ButtonBase(this, QRbutton));

            //quickstack will be 2-state toggle button (locked/unlocked) that toggles on right click

            // create default unlocked state
            label = "Quick Stack (Unlocked)";
            SetUpStateBasics(label);

            States[label].onClick = IHUtils.DoQuickStack;
            States[label].onRightClick = () => {
                Main.PlaySound(22); //lock sound
                IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.QS); };

            // create locked state
            label = "Quick Stack (Locked)";
            SetUpStateBasics(label);

            States[label].onClick      = States["Quick Stack (Unlocked)"].onClick;
            States[label].onRightClick = States["Quick Stack (Unlocked)"].onRightClick;

            // create the button, setting its state from ActionLocked()
            //can leave the position null since the position for this button has already been established

            var QSbutton = new IHToggle(
                States["Quick Stack (Unlocked)"],
                States["Quick Stack (Locked)"],
                () => IHPlayer.ActionLocked(Main.localPlayer, IHAction.QS), //IsActive()
                null, true);

            //now create keywatchers to toggle the button from restock to quickstack when Shift is pressed
            Buttons[IHAction.Refill].RegisterKeyToggle(KState.Special.Shift, QRbutton, QSbutton);

            // --Create SmartStash/DepositAll Button-- //
            posX = 453 - (Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE); //right beside trash

            label   = "Smart Deposit";
            SetUpStateBasics(label);
            States[label].onClick = IHSmartStash.SmartDeposit;

            var SDbutton = new IHButton(States[label], new Vector2(posX, posY));
            Buttons.Add(IHAction.Deposit, new ButtonBase(this, SDbutton));

            //deposit all
            label   = "Deposit All (Unlocked)";
            SetUpStateBasics(label);
            States[label].onClick = IHUtils.DoDepositAll;
            States[label].onRightClick = () => {
                Main.PlaySound(22); //lock sound
                IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.DA); };

            //locked
            label   = "Deposit All (Locked)";
            SetUpStateBasics(label);

            States[label].onClick      = States["Deposit All (Unlocked)"].onClick;
            States[label].onRightClick = States["Deposit All (Unlocked)"].onRightClick;

            var DAbutton = new IHToggle(States["Deposit All (Unlocked)"], States["Deposit All (Locked)"],
                () => IHPlayer.ActionLocked(Main.localPlayer, IHAction.DA), //IsActive()
                null, true);

            //create keywatchers
            Buttons[IHAction.Deposit].RegisterKeyToggle(KState.Special.Shift, SDbutton, DAbutton);

            //increase base alpha of chest buttons
            Buttons[IHAction.Deposit].Alpha = Buttons[IHAction.Refill].Alpha = Buttons[IHAction.Sort].Alpha = 0.8f;

        #region loot_all_button
            // --Create LootAll Button-- //
            // posY += (Main.inventoryBackTexture.Height);
            //
            // bsD = new ButtonState();
            // bsD.label = "Loot All";
            // bsD.texture = mbase.textures["resources/btn_loot"];
            // bsD.onClick = IHUtils.DoLootAll;
            //
            // Buttons.Add(IHAction.LA, new ButtonBase( new IHButton(bsD, new Vector2(posX, posY))));
        #endregion
        // }
        // UpdateFrame();

        #region calcs
            /*
            (all the following multiplied by Main.inventoryScale=0.755)
            chest slots begin at X=73px, Y=Main.invBottom
            Chest slots take up 56x56px
            chest inventory width = 560px
            chest inv height = 224 px
            bottom of chest slots = API.main.invBottom + (224*Constants.CHEST_INVENTORY_SCALE)

            trash.X = 448+5   = 453
            trash.Y = 258+168 = 426

            Positions (right->left from trash location)
            Button1 = ( 453-56, 426 ) = (397,426)
            Button2 = ( 397-56, 426 ) = (341,426)
            Button3 = ...

            */
        #endregion

        }

        private void SetUpStateBasics(String label)
        {
            States.Add(label, new ButtonState(label));

            Rects.Add(label,  IHUtils.GetSourceRect(label)); //inactive appearance
            MRects.Add(label, IHUtils.GetSourceRect(label,true)); //mouse-over

            States[label].texture      = IHBase.ButtonGrid;
            States[label].sourceRect   = Rects[label];
            States[label].onMouseEnter = bb => { bb.CurrentState.sourceRect = MRects[label]; return true;};
            States[label].onMouseLeave = bb => { bb.CurrentState.sourceRect = Rects[label]; return true;};
            // States[label].tint         = btnTint;
        }

        protected override void OnDraw(SpriteBatch sb)
        {
            if (!parentLayer.visible) return;
            // foreach (KeyValuePair<IHAction, ButtonBase> kvp in Buttons)
            // {
            //     sb.DrawButtonBG(kvp.Value, IHBase.ButtonBG, bgColor);
            // }

            foreach (KeyValuePair<IHAction, ButtonBase> kvp in Buttons)
            {
                sb.DrawButtonBG(kvp.Value, IHBase.ButtonBG, bgColor);
                kvp.Value.Draw(sb);
            }
        }

    }
}
