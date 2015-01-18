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

            Buttons.Add(IHAction.Sort, new IHContextButton(bsD, bsA, KState.Special.Shift, new Vector2(posX, posY)));

            /** --Create Stack Button-- **/

            posX += (Main.inventoryBackTexture.Width * Main.inventoryScale);

            bsD = new ButtonState("Clean Stacks", mbase.textures["resources/btn_cleanStacks"],
            () => IHOrganizer.ConsolidateStacks(Main.localPlayer.inventory, 0, 50));

            Buttons.Add(IHAction.Stack, new IHButton(bsD, new Vector2(posX, posY)));
        }
    }

    public class ChestButtons : ButtonLayer
    {
        // public readonly Dictionary<IHAction, IHButton> Buttons;

        /*if this is true (will likely be a mod-option), replace the text buttons to the right of chests with my multifunctional icons*/
        // public bool replaceVanilla;

        public ChestButtons(ModBase mbase) : base("ChestButtons")
        {
            // this should put the Buttons to the left of the chest inventory
            float posX = 73 - (Main.inventoryBackTexture.Width * Main.inventoryScale);
            float posY = API.main.invBottom + (Main.inventoryBackTexture.Height * Main.inventoryScale)/2;

            // Buttons = new Dictionary<IHAction, IHButton>();

            // --Create Sort Button-- //
            //default state
            ButtonState bsD = new ButtonState("Sort Chest", mbase.textures["resources/btn_sort"],
                () => IHOrganizer.SortChest(Main.localPlayer.chestItems, IHBase.ModOptions["ReverseSortChest"]));

            //altState
            ButtonState bsA = new ButtonState("Sort Chest (Reverse)", mbase.textures["resources/btn_sort_reverse"],
            () => IHOrganizer.SortChest(Main.localPlayer.chestItems, !IHBase.ModOptions["ReverseSortChest"]));

            Buttons.Add(IHAction.Sort, new IHContextButton(bsD, bsA, KState.Special.Shift));

            // --Create Refill Button-- //

            bsD = new ButtonState();
            bsD.label = "Refill";
            bsD.texture = mbase.textures["resources/btn_quickRefill"];
            bsD.onClick = IHSmartStash.SmartLoot;

            Buttons.Add(IHAction.Refill, new IHButton(bsD, new Vector2(posX, posY)));

            // --Create Smart Deposit Button-- //

            bsD = new ButtonState();
            bsD.label = "Smart Deposit";
            bsD.texture = mbase.textures["resources/btn_smartDeposit"];
            bsD.onClick = IHSmartStash.SmartDeposit;

            Buttons.Add(IHAction.Deposit, new IHButton(bsD, new Vector2(posX, posY)));




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
