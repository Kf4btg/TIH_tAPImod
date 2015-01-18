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
    /*
    public class ChestButtons : ButtonLayer
    {
        // public readonly Dictionary<IHAction, IHButton> Buttons;

        public bool replaceVanilla;

        public ChestButtons(ModBase mbase) : base("ChestButtons")
        {
            // this should put the Buttons to the left of the chest inventory
            float posX = 73 - (Main.inventoryBackTexture.Width * Main.inventoryScale);
            float posY = API.main.invBottom + (Main.inventoryBackTexture.Height * Main.inventoryScale)/2;

            // Buttons = new Dictionary<IHAction, IHButton>();

            // --Create Sort Button-- //
            //default state
            ButtonState bsD = new ButtonState("Sort", mbase.textures["resources/btn_sort"],
                () => { Main.localPlayer.chest == -1 ?
                        IHOrganizer.SortPlayerInv(Main.localPlayer, IHBase.ModOptions["ReverseSortPlayer"]) :
                        IHOrganizer.SortChest(Main.localPlayer.chestItems, IHBase.ModOptions["ReverseSortChest"]);} );

            //altState
            ButtonState bsA = new ButtonState("Sort (Reverse)", mbase.textures["resources/btn_sort_reverse"],
            () => { Main.localPlayer.chest == -1 ?
                IHOrganizer.SortPlayerInv(Main.localPlayer, !IHBase.ModOptions["ReverseSortPlayer"]) :
                IHOrganizer.SortChest(Main.localPlayer.chestItems, !IHBase.ModOptions["ReverseSortChest"]);} );

            Buttons.Add(IHAction.Sort, new IHContextButton(bsD, bsA, KState.Special.Shift));

            if (replaceVanilla)
            {
                /// --Create Deposit Button-- ///
                bsD = new ButtonState("DepositAll", mbase.textures["resources/btn_depositAll"],
                () => { Main.localPlayer.chest == -1 ?
                    IHOrganizer.SortPlayerInv(Main.localPlayer, IHBase.ModOptions["ReverseSortPlayer"]) :
                    IHOrganizer.SortChest(Main.localPlayer.chestItems, IHBase.ModOptions["ReverseSortChest"]);} );

                    //altState
                bsA = new ButtonState("SmartDeposit", mbase.textures["resources/btn_smartDeposit"],
                () => { Main.localPlayer.chest == -1 ?
                    IHOrganizer.SortPlayerInv(Main.localPlayer, !IHBase.ModOptions["ReverseSortPlayer"]) :
                    IHOrganizer.SortChest(Main.localPlayer.chestItems, !IHBase.ModOptions["ReverseSortChest"]);} );
            }

        }


        // protected override void OnDraw(SpriteBatch sb)
        // {
        //     foreach (KeyValuePair<IHAction, IHButton> kvp in Buttons)
        //     {
        //         kvp.Value.Draw(sb);
        //     }
        // }

    }
    */
}
