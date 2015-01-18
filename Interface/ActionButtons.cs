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
            float posX = 73 - (Main.inventoryBackTexture.Width * Main.inventoryScale);
            float posY = API.main.invBottom + (Main.inventoryBackTexture.Height * Main.inventoryScale)/2;

            /** --Create Sort Button-- **/
            //default state
            ButtonState bsD = new ButtonState("Sort", mbase.textures["resources/btn_sort"],
            () => IHOrganizer.SortPlayerInv(Main.localPlayer, IHBase.ModOptions["ReverseSortPlayer"]));

            //altState
            ButtonState bsA = new ButtonState("Sort (Reverse)", mbase.textures["resources/btn_sort_reverse"],
            () => IHOrganizer.SortPlayerInv(Main.localPlayer, !IHBase.ModOptions["ReverseSortPlayer"]));

            Buttons.Add(IHAction.Sort, new ButtonBase(new IHContextButton(bsD, bsA, KState.Special.Shift, new Vector2(posX, posY))));

            /** --Create Stack Button-- **/

            posX += (Main.inventoryBackTexture.Width * Main.inventoryScale);

            bsD = new ButtonState("Clean Stacks", mbase.textures["resources/btn_cleanStacks"],
            () => IHOrganizer.ConsolidateStacks(Main.localPlayer.inventory, 0, 50));

            Buttons.Add(IHAction.Stack, new ButtonBase(new IHButton(bsD, new Vector2(posX, posY))));
        }
    }

    public class ChestButtons : ButtonLayer
    {
        // public readonly Dictionary<IHAction, IHButton> Buttons;

        /*if this is true (will likely be a mod-option), replace the text buttons to the right of chests with multifunctional icons*/
        // public readonly bool replaceVanilla = false;

        public ChestButtons(ModBase mbase) : base("ChestButtons")
        {
            // this should put the Buttons to the left of the chest inventory
            float posX = 73 - (Main.inventoryBackTexture.Width * Main.inventoryScale);
            // float posY = API.main.invBottom + (Main.inventoryBackTexture.Height * Main.inventoryScale)/2;
            float posY = API.main.invBottom + 4;

            // Buttons = new Dictionary<IHAction, IHButton>();

            // --Create Sort Button-- //
            //default state
            ButtonState bsD = new ButtonState("Sort Chest", mbase.textures["resources/btn_sort"],
                () => IHOrganizer.SortChest(Main.localPlayer.chestItems, IHBase.ModOptions["ReverseSortChest"]));

            //altState
            ButtonState bsA = new ButtonState("Sort Chest (Reverse)", mbase.textures["resources/btn_sort_reverse"],
            () => IHOrganizer.SortChest(Main.localPlayer.chestItems, !IHBase.ModOptions["ReverseSortChest"]));

            Buttons.Add(IHAction.Sort, new ButtonBase(new IHContextButton(bsD, bsA, KState.Special.Shift, new Vector2(posX,posY))));

            // if (replaceVanilla){}
            // else{

            // --Create Refill/QuickStack Button-- //

            posY += (Main.inventoryBackTexture.Height * Main.inventoryScale);
            //restock is simple single-state button
            bsD = new ButtonState();
            bsD.label = "Quick Restock";
            bsD.texture = mbase.textures["resources/btn_quickRefill"];
            bsD.onClick = IHSmartStash.SmartLoot;

            //create button
            IHButton QRbutton =new IHButton(bsD, new Vector2(posX, posY));

            //add it as the default context to a new ButtonBase
            Buttons.Add(IHAction.Refill, new ButtonBase(QRbutton));

            //quickstack will be 2-state toggle button (locked/unlocked)
            // that toggles on right click

            // create default unlocked state, setup name and texture
            bsA = new ButtonState();
            bsA.label = "Quick Stack (Unlocked)";
            bsA.texture = mbase.textures["resources/btn_quickStack"];
            // bsA.onClick = () => IHUtils.DoQuickStack();
            // bsA.onRightClick = () => {
            //     Main.PlaySound(22, -1, -1, 1); //lock sound
            //     IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.QS); }

            // create locked state, setup name and texture
            ButtonState bsL = new ButtonState();
            bsL.label = "Quick Stack (Locked)";
            bsL.texture = mbase.textures["resources/btn_quickStack_locked"];

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

            // --Create SmartStash/DepositAll Button-- //
            posY += (Main.inventoryBackTexture.Height * Main.inventoryScale);

            bsD = new ButtonState();
            bsD.label = "Smart Deposit";
            bsD.texture = mbase.textures["resources/btn_smartDeposit"];
            bsD.onClick = IHSmartStash.SmartDeposit;

            IHButton SDbutton = new IHButton(bsD, new Vector2(posX, posY));

            Buttons.Add(IHAction.Deposit, new ButtonBase(SDbutton));

            bsA = new ButtonState();
            bsA.label = "Deposit All (Unlocked)";
            bsA.texture = mbase.textures["resources/btn_depositAll"];

            bsL = new ButtonState();
            bsL.label = "Deposit All (Locked)";
            bsL.texture = mbase.textures["resources/btn_depositAll_locked"];

            bsL.onClick = bsA.onClick = IHUtils.DoDepositAll;
            bsL.onRightClick = bsA.onRightClick = () => {
                Main.PlaySound(22, -1, -1, 1); //lock sound
                IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.DA); };

            IHToggle DAbutton = new IHToggle(bsA, bsL,
                () => IHPlayer.ActionLocked(Main.localPlayer, IHAction.DA), //IsActive()
                null, true); //can leave the position null since the position for this button has already been established

            //create keywatchers
            Buttons[IHAction.Deposit].RegisterKeyToggle(KState.Special.Shift, SDbutton, DAbutton);

            // --Create LootAll Button-- //
            posY += (Main.inventoryBackTexture.Height * Main.inventoryScale);
            
            bsD = new ButtonState();
            bsD.label = "Loot All";
            bsD.texture = mbase.textures["resources/btn_lootAll"];
            bsD.onClick = IHUtils.DoLootAll;

            Buttons.Add(IHAction.LA, new ButtonBase( new IHButton(bsD, new Vector2(posX, posY))));


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


        // protected override void OnDraw(SpriteBatch sb)
        // {
        //     foreach (KeyValuePair<IHAction, IHButton> kvp in Buttons)
        //     {
        //         kvp.Value.Draw(sb);
        //     }
        // }

    }
}
