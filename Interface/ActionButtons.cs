using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{

    public class InventoryButtons : ButtonLayer
    {
        public InventoryButtons(ModBase mbase) : base("InventoryButtons")
        {
            // float posX = 73 - (Main.inventoryBackTexture.Width * Main.inventoryScale);
            // float posY = API.main.invBottom + (Main.inventoryBackTexture.Height * Main.inventoryScale)/2;

            //let's try to put these above the coins/ammo slot, instead. Should be less intrusive.
            float posX = 496;
            float posY = 40;

            /** --Create Sort Button-- **/
            //default state
            ButtonState bsD = new ButtonState("Sort", mbase.textures["resources/btn_sort"],
            () => IHOrganizer.SortPlayerInv(Main.localPlayer, IHBase.ModOptions["ReverseSortPlayer"]));

            //altState
            ButtonState bsA = new ButtonState("Sort (Reverse)", mbase.textures["resources/btn_sort_reverse"],
            () => IHOrganizer.SortPlayerInv(Main.localPlayer, !IHBase.ModOptions["ReverseSortPlayer"]));

            Buttons.Add(    IHAction.Sort,
                            new ButtonBase(
                                new IHContextButton(
                                    bsD,
                                    bsA,
                                    KState.Special.Shift,
                                    new Vector2(
                                        posX,
                                        posY
                                    )
                                )
                            )
                        );

            /** --Create Stack Button-- **/

            // posX += (Main.inventoryBackTexture.Width * Main.inventoryScale);
            posX = 532;

            bsD = new ButtonState("Clean Stacks", mbase.textures["resources/btn_clean"],
            () => IHOrganizer.ConsolidateStacks(Main.localPlayer.inventory, 0, 50));

            Buttons.Add(IHAction.Stack, new ButtonBase(new IHButton(bsD, new Vector2(posX, posY))));
        }
    }

    public class ChestButtons : ButtonLayer
    {
        /*if this is true (will likely be a mod-option), replace the text buttons to the right of chests with multifunctional icons*/
        // public readonly bool replaceVanilla = false;

        public ChestButtons(ModBase mbase) : base("ChestButtons")
        {
            // this should put the Buttons to the left of the chest inventory
            float posX = 73 - (Main.inventoryBackTexture.Width * Main.inventoryScale);
            float posY = API.main.invBottom + 4;
            // float posY = API.main.invBottom + (Main.inventoryBackTexture.Height * Main.inventoryScale)/2;

        #region sort_and_reverse
            // --Create Sort Button-- //
            //default state
            ButtonState bsD = new ButtonState("Sort Chest", mbase.textures["resources/btn_sort"],
                () => IHOrganizer.SortChest(Main.localPlayer.chestItems, IHBase.ModOptions["ReverseSortChest"]));

            //altState
            ButtonState bsA = new ButtonState("Sort Chest (Reverse)", mbase.textures["resources/btn_sort_reverse"],
            () => IHOrganizer.SortChest(Main.localPlayer.chestItems, !IHBase.ModOptions["ReverseSortChest"]));

            Buttons.Add(IHAction.Sort, new ButtonBase(new IHContextButton(bsD, bsA, KState.Special.Shift, new Vector2(posX,posY))));
        #endregion

            // if (replaceVanilla){}
            // else{

        #region refill_quickstack
            // --Create Refill/QuickStack Button-- //

            //restock is simple single-state button
            bsD = new ButtonState();
            bsD.label = "Quick Restock";
            bsD.texture = mbase.textures["resources/btn_restock"];
            bsD.onClick = IHSmartStash.SmartLoot;

            //create button
            posY += (Main.inventoryBackTexture.Height);
            IHButton QRbutton = new IHButton(bsD, new Vector2(posX, posY));

            //add it as the default context to a new ButtonBase
            Buttons.Add(IHAction.Refill, new ButtonBase(QRbutton));

            //quickstack will be 2-state toggle button (locked/unlocked)
            // that toggles on right click

            // create default unlocked state, setup name and texture
            bsA = new ButtonState();
            bsA.label = "Quick Stack (Unlocked)";
            bsA.texture = mbase.textures["resources/btn_stack"];

            // create locked state, setup name and texture
            ButtonState bsL = new ButtonState();
            bsL.label = "Quick Stack (Locked)";
            bsL.texture = mbase.textures["resources/btn_stack"];

            // being a toggle, onClick (the quickstack action) and
            // onRightClick (the state-toggle action) will be the same
            bsL.onClick = bsA.onClick = IHUtils.DoQuickStack;
            bsL.onRightClick = bsA.onRightClick = () => {
                Main.PlaySound(22, -1, -1, 1); //lock sound
                IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.QS); };

            // create the button, setting its state from ActionLocked()
            IHToggle QSbutton = new IHToggle(bsA, bsL,
                () => IHPlayer.ActionLocked(Main.localPlayer, IHAction.QS), //IsActive()
                new Vector2(posX, posY), true);

            //now create keywatchers to toggle the button from restock to quickstack when Shift is pressed
            Buttons[IHAction.Refill].RegisterKeyToggle(KState.Special.Shift, QRbutton, QSbutton);
        #endregion

        #region smart_and_all_deposit
            // --Create SmartStash/DepositAll Button-- //
            posY += (Main.inventoryBackTexture.Height);

            bsD = new ButtonState();
            bsD.label = "Smart Deposit";
            bsD.texture = mbase.textures["resources/btn_smart_deposit"];
            bsD.onClick = IHSmartStash.SmartDeposit;

            IHButton SDbutton = new IHButton(bsD, new Vector2(posX, posY));

            Buttons.Add(IHAction.Deposit, new ButtonBase(SDbutton));

            bsA = new ButtonState();
            bsA.label = "Deposit All (Unlocked)";
            bsA.texture = mbase.textures["resources/btn_depositAll"];

            bsL = new ButtonState();
            bsL.label = "Deposit All (Locked)";
            bsL.texture = mbase.textures["resources/btn_deposit"];

            bsL.onClick = bsA.onClick = IHUtils.DoDepositAll;
            bsL.onRightClick = bsA.onRightClick = () => {
                Main.PlaySound(22, -1, -1, 1); //lock sound
                IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.DA); };

            IHToggle DAbutton = new IHToggle(bsA, bsL,
                () => IHPlayer.ActionLocked(Main.localPlayer, IHAction.DA), //IsActive()
                null, true); //can leave the position null since the position for this button has already been established

            //create keywatchers
            Buttons[IHAction.Deposit].RegisterKeyToggle(KState.Special.Shift, SDbutton, DAbutton);
        #endregion

        #region loot_all_button
            // --Create LootAll Button-- //
            posY += (Main.inventoryBackTexture.Height);

            bsD = new ButtonState();
            bsD.label = "Loot All";
            bsD.texture = mbase.textures["resources/btn_loot"];
            bsD.onClick = IHUtils.DoLootAll;

            Buttons.Add(IHAction.LA, new ButtonBase( new IHButton(bsD, new Vector2(posX, posY))));
        #endregion


            // if (replaceVanilla)
            // {
            //     /// --Create Deposit Button-- ///
            //     bsD = new ButtonState("DepositAll", mbase.textures["resources/btn_depositAll"],
            //     () => { Main.localPlayer.chest == -1 ?
            //         IHOrganizer.SortPlayerInv(Main.localPlayer, IHBase.ModOptions["ReverseSortPlayer"]) :
            //         IHOrganizer.SortChest(Main.localPlayer.chestItems, IHBase.ModOptions["ReverseSortChest"]);} );
            //
            //         //altState
            //     bsA = new ButtonState("SmartDeposit", mbase.textures["resources/btn_smartDeposit"],
            //     () => { Main.localPlayer.chest == -1 ?
            //         IHOrganizer.SortPlayerInv(Main.localPlayer, !IHBase.ModOptions["ReverseSortPlayer"]) :
            //         IHOrganizer.SortChest(Main.localPlayer.chestItems, !IHBase.ModOptions["ReverseSortChest"]);} );
            // }

        // }
        }

    }
}
