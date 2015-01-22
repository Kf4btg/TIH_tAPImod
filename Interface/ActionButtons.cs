using Microsoft.Xna.Framework;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{

    public class InventoryButtons : ButtonLayer
    {
        private float posX = 496;
        private const float posY = 28; //doesn't change

        public InventoryButtons(ModBase mbase) : base("InventoryButtons")
        {
            /*
            Main.inventoryScale=0.85)
            */
            // float posX = 73 - (Main.inventoryBackTexture.Width * Main.inventoryScale);
            // float posY = API.main.invBottom + (Main.inventoryBackTexture.Height * Main.inventoryScale)/2;

            //let's try to put these above the coins/ammo slot, instead. Should be less intrusive.


            /** --Create Sort Button-- **/
            //default state
            var bsD = new ButtonState("Sort", mbase.textures["resources/btn_sort"],
            () => IHOrganizer.SortPlayerInv(Main.localPlayer, IHBase.ModOptions["ReverseSortPlayer"]), null,
            Main.inventoryBack);

            //altState
            var bsA = new ButtonState("Sort (Reverse)", mbase.textures["resources/btn_sort_reverse"],
            () => IHOrganizer.SortPlayerInv(Main.localPlayer, !IHBase.ModOptions["ReverseSortPlayer"]), null,
            Main.inventoryBack);

            Buttons.Add(    IHAction.Sort,
                            new ButtonBase( this,
                                new IHContextButton(
                                    bsD,
                                    bsA,
                                    KState.Special.Shift,
                                    new Vector2(posX, posY)
                                )
                            )
                        );

            /** --Create Stack Button-- **/

            // posX += (Main.inventoryBackTexture.Width * Main.inventoryScale);
            posX = 532;

            bsD = new ButtonState("Clean Stacks", mbase.textures["resources/btn_clean"],
            () => IHOrganizer.ConsolidateStacks(Main.localPlayer.inventory, 0, 50));

            Buttons.Add(IHAction.Stack, new ButtonBase(this, new IHButton(bsD, new Vector2(posX, posY))));

            //try making it a bit bigger?
            // Buttons[IHAction.Stack].Scale = 1.1f;

            UpdateFrame();
        }
    }

    public class ChestButtons : ButtonLayer
    {
        /*if this is true (will likely be a mod-option), replace the text buttons to the right of chests with multifunctional icons*/
        // public readonly bool replaceVanilla = false;

        private float posX; // = 453 - (Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE);
        private readonly float posY = API.main.invBottom + (224*Constants.CHEST_INVENTORY_SCALE) + 4; //doesn't change

        public ChestButtons(ModBase mbase) : base("ChestButtons")
        {
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

            // this should put the Buttons to the left of the chest inventory
            // float posX = 73 - (Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE);
            // float posY = API.main.invBottom + 4;
            // float posY = API.main.invBottom + (Main.inventoryBackTexture.Height * Main.inventoryScale)/2;

        #region sort_and_reverse
            // --Create Sort Button-- //
            posX = 453 - (3*Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE); //leftmost
            //default state
            var bsD = new ButtonState("Sort Chest", mbase.textures["resources/btn_sort"],
                () => IHOrganizer.SortChest(Main.localPlayer.chestItems, IHBase.ModOptions["ReverseSortChest"]));

            //altState
            var bsA = new ButtonState("Sort Chest (Reverse)", mbase.textures["resources/btn_sort_reverse"],
            () => IHOrganizer.SortChest(Main.localPlayer.chestItems, !IHBase.ModOptions["ReverseSortChest"]));

            Buttons.Add(IHAction.Sort, new ButtonBase(this, new IHContextButton(bsD, bsA, KState.Special.Shift, new Vector2(posX,posY))));
        #endregion

            // if (replaceVanilla){}
            // else{

        #region refill_quickstack
            // --Create Refill/QuickStack Button-- //
            posX = 453 - (2*Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE);
            // posY += (Main.inventoryBackTexture.Height);

            //restock is simple single-state button
            bsD = new ButtonState();
            bsD.label = "Quick Restock";
            bsD.texture = mbase.textures["resources/btn_restock"];
            bsD.onClick = IHSmartStash.SmartLoot;

            //create button
            var QRbutton = new IHButton(bsD, new Vector2(posX, posY));

            //add it as the default context to a new ButtonBase
            Buttons.Add(IHAction.Refill, new ButtonBase(this, QRbutton));

            //quickstack will be 2-state toggle button (locked/unlocked)
            // that toggles on right click

            // create default unlocked state, setup name and texture
            bsA = new ButtonState();
            bsA.label = "Quick Stack (Unlocked)";
            bsA.texture = mbase.textures["resources/btn_stack"];

            // create locked state, setup name and texture
            var bsL = new ButtonState();
            bsL.label = "Quick Stack (Locked)";
            bsL.texture = mbase.textures["resources/btn_stack"];

            // being a toggle, onClick (the quickstack action) and
            // onRightClick (the state-toggle action) will be the same
            bsL.onClick = bsA.onClick = IHUtils.DoQuickStack;
            bsL.onRightClick = bsA.onRightClick = () => {
                Main.PlaySound(22); //lock sound
                IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.QS); };

            // create the button, setting its state from ActionLocked()
            var QSbutton = new IHToggle(bsA, bsL,
                () => IHPlayer.ActionLocked(Main.localPlayer, IHAction.QS), //IsActive()
                null, true); //can leave the position null since the position for this button has already been established

            //now create keywatchers to toggle the button from restock to quickstack when Shift is pressed
            Buttons[IHAction.Refill].RegisterKeyToggle(KState.Special.Shift, QRbutton, QSbutton);
        #endregion

        #region smart_and_all_deposit
            // --Create SmartStash/DepositAll Button-- //
            posX = 453 - (Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE); //right beside trash

            // posY += (Main.inventoryBackTexture.Height);

            bsD = new ButtonState();
            bsD.label = "Smart Deposit";
            bsD.texture = mbase.textures["resources/btn_smart_deposit"];
            bsD.onClick = IHSmartStash.SmartDeposit;

            var SDbutton = new IHButton(bsD, new Vector2(posX, posY));

            Buttons.Add(IHAction.Deposit, new ButtonBase(this, SDbutton));

            bsA = new ButtonState();
            bsA.label = "Deposit All (Unlocked)";
            bsA.texture = mbase.textures["resources/btn_deposit"];

            bsL = new ButtonState();
            bsL.label = "Deposit All (Locked)";
            bsL.texture = mbase.textures["resources/btn_deposit"];

            bsL.onClick = bsA.onClick = IHUtils.DoDepositAll;
            bsL.onRightClick = bsA.onRightClick = () => {
                Main.PlaySound(22); //lock sound
                IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.DA); };

            var DAbutton = new IHToggle(bsA, bsL,
                () => IHPlayer.ActionLocked(Main.localPlayer, IHAction.DA), //IsActive()
                null, true); //can leave the position null since the position for this button has already been established

            //create keywatchers
            Buttons[IHAction.Deposit].RegisterKeyToggle(KState.Special.Shift, SDbutton, DAbutton);
        #endregion

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
        UpdateFrame();
        }

    }
}
