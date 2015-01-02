using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using TAPI;
using TAPI.UIKit;
using Terraria;

namespace InvisibleHand
{
    public class IHInterface : ModInterface
    {
        // private Texture2D lockedIcon = null;

        // public override void ModifyInterfaceLayerList(List<InterfaceLayer>list)
        // {
        //     if (!Main.playerInventory) return;
        // }
        //
        // public InterfaceLayer lockMarkers = new InterfaceLayer.Action("InvisibleHand:lockMarkers", (layer, sb) =>
        // {
        //     if (!Main.playerInventory) return;
        //
        //
        //
        //
        //     })

        public override void PostDrawItemSlotBackground(SpriteBatch sb, ItemSlot slot)
        {
            if (IHBase.oLockingEnabled && slot.type == "Inventory" && IHPlayer.SlotLocked(slot.index))
            {
                sb.Draw(IHBase.lockedIcon,                // the texture to draw
                            slot.pos,           // (Vector2) location in screen coords to draw sprite
                            null,               // Rectangle to specifies source texels from texture; null draws whole texture
                            Color.Firebrick,        // color to tint sprite; color.white=full color, no tint
                            0f,                 // angle in radians to rotate sprite around its center
                            default(Vector2),   // (Vector2) sprite origin, default=(0,0) i.e. upper left corner
                            slot.scale,         // (Vector2) scale factor
                            SpriteEffects.None, // effects to apply
                            0f                  // layer depth; 0=front layer, 1=backlayer; SpriteSortMode can sort sprites
                            );
            }
        }

        // Shift + Right Click on inventory slot toggles the lock state
        public override bool PreItemSlotRightClick(ItemSlot slot, ref bool release)
        {
            if (!KState.Special.Shift.Down()) return true;

            if (IHBase.oLockingEnabled && slot.modBase == null && Main.playerInventory && release )
            {
                if (slot.type == "Inventory" && slot.index >= 10) //not in the hotbar
                {
                    IHPlayer.ToggleLock(slot.index); //toggle lock state
                    Main.PlaySound(7, -1, -1, 1);
                }
            }
            return false;
        }

    #region shiftmove

        /************************************************************************
        *   An implementation of the "Shift-click to move item between containers"
        *   concept. Possibly temporary, though it includes the feature of working
        *   with the craft-guide slot, which I really like.
        *
        *   Known Issues:
            FIXME: Items should go to empty hotbar slot first
            FIXME: fill ammo from top down (ascending)
            FIXME: shift-moving coins should stack them properly
        */
        // Shift + Left Click on item slot to move it between inventory and chest
        public override bool PreItemSlotLeftClick(ItemSlot slot, ref bool release)
        {
            if (!(bool)IHBase.self.options["enableShiftMove"].Value) return true;

            if (slot.modBase == null && release && KState.Special.Shift.Down())
            {
                if (Main.localPlayer.chestItems != null)
                {
                    if (slot.type == "Inventory" || slot.type == "Coin" || slot.type == "Ammo")
                    {
                        if (!slot.MyItem.IsBlank() && ShiftToChest(ref slot))
                            Recipe.FindRecipes(); // !ref:Main:#22640.36#
                    }
                    else if (slot.type == "Chest" && !slot.MyItem.IsBlank())
                    {

                        if (ShiftToPlayer(ref slot, Main.localPlayer.chest>-1))
                            Recipe.FindRecipes();
                        // seriously is this all it takes aaaarrrhghghghaghggggghgghhh
                        // slot.MyItem = Main.localPlayer.GetItem(Main.myPlayer, slot.MyItem);
                        // if (Main.localPlayer.chest > -1) IHUtils.SendNetMessage(slot.index);
                        // MoveSlotItem(ref slot, Main.localPlayer.chest >- 1);
                    }
                    return false;
                }
                if (Main.craftGuide)
                {
                    if (Main.guideItem.IsBlank() && !slot.MyItem.IsBlank() && (slot.type == "Inventory" || slot.type == "Coin" || slot.type == "Ammo") )
                    {
                        if (slot.MyItem.material && !slot.MyItem.notMaterial)
                        {
                            Main.PlaySound(7, -1, -1, 1);
                            Main.guideItem = slot.MyItem.Clone();
                            slot.MyItem = new Item();
                            Recipe.FindRecipes();
                        }
                    }
                    else if (!Main.guideItem.IsBlank() && slot.type == "CraftGuide")
                    {
                        // Main.localPlayer.GetItem(Main.myPlayer, Main.guideItem);
                        // if ( IHUtils.ShiftItemToPlayer(ref Main.guideItem, Main.localPlayer.inventory, 0, 57, true) >= 0 )
                        if (ShiftToPlayer(ref slot, false))
                            Main.guideItem = new Item();
                        // if (MoveSlotItem(ref slot))
                        Recipe.FindRecipes();
                    }
                    return false;
                }
            }
            return true;
        }

        /**************************************************************
        *   returns true if item moved/itemstack emptied
        */

        // move item from player inventory slot to chest
        public static bool ShiftToChest(ref ItemSlot slot)
        {
            bool sendMessage = Main.localPlayer.chest > -1;
            Item pItem = slot.MyItem;

            int retIdx = IHUtils.MoveToFirstEmpty( pItem, Main.localPlayer.chestItems, 0,
            new Func<int,bool>( i => i<Chest.maxItems ),
            new Func<int,int>( i => i+1 ) );

            // if we didn't find an empty slot...
            if (retIdx < 0)
            { //return IHUtils.MoveItemToChest(slot.index, Main.ChestCoins, true);

                Action doCoins;
                doCoins = sendMessage ? (Action)Main.ChestCoins : (Action)Main.BankCoins;

                int stackB4 = pItem.stack;
                if (pItem.maxStack > 1)  //try to stack the item if possible
                {
                    retIdx = IHUtils.TryStackMerge(ref pItem, Main.localPlayer.chestItems,
                        doCoins, sendMessage, 0,
                        new Func<int,bool>( i => i<Chest.maxItems ),
                        new Func<int,int>( i => i+1 ) );
                }

                if (retIdx < 0)  //stack failed/was incomplete
                {
                    if (stackB4!=pItem.stack) //some movement occurred
                    {
                        Main.PlaySound(7, -1, -1, 1);
                        Recipe.FindRecipes();
                    }
                    return false;
                }
            }
            //else, success!
            slot.MyItem = new Item();
            if (sendMessage) IHUtils.SendNetMessage(retIdx);
            return true;
            // }
            //else banks

            // if (retIdx < 0)
            // { //return IHUtils.MoveItemToChest(slot.index, Main.ChestCoins, true);
            //     int stackB4 = slot.MyItem.stack;
            //     if (slot.MyItem.maxStack > 1)  //try to stack the item if possible
            //     {
            //         retIdx = IHUtils.TryStackMerge(ref slot.MyItem, Main.localPlayer.chestItems, Main.BankCoins, false, 0,
            //             new Func<int,bool>( i => i<Chest.maxItems ),
            //             new Func<int,int>( i => i+1 ) );
            //     }
            //     if (retIdx < 0)  //stack failed/was incomplete
            //     {
            //         if (stackB4!=slot.MyItem.stack) //some movement occurred
            //         {
            //             Main.PlaySound(7, -1, -1, 1);
            //             Recipe.FindRecipes();
            //         }
            //         return false;
            //     }
            // }
            // return true;

            // return (retIdx < 0) ? IHUtils.MoveItemToChest(slot.index, Main.BankCoins, false) : true;


            // return (Main.localPlayer.chest > -1) ?
            //      IHUtils.MoveItemToChest(slot.index, Main.ChestCoins, true) :
            // //else banks
            //     IHUtils.MoveItemToChest(slot.index, Main.BankCoins, false);
        }
        // public static bool MoveGuideItem(ref ItemSlot slot)
        // {
        //
        // }
        // MoveChestSlotItem - moves item from chest/guide slot to player inventory
        public static bool ShiftToPlayer(ref ItemSlot slot, bool sendMessage)
        {
            Item cItem = slot.MyItem;
            // IHUtils.ShiftItemToPlayer(ref Main.guideItem, Main.localPlayer.inventory, 0, 57, true) >= 0 )

            if (cItem.IsBlank()) return false;

            // MoveItem returns true if original item ends up empty
            if (cItem.Matches(ItemCat.COIN) &&
            ShiftToPlayer(ref slot, 50, 53, sendMessage, true)) return true;

            if (cItem.Matches(ItemCat.AMMO) &&
            ShiftToPlayer(ref slot, 54, 57, sendMessage, true)) return true;

            if (ShiftToPlayer(ref slot,  0, 49, sendMessage, true)) return true;

            //if all of the above failed, then we have no empty slots.
            // Let's save some work and just get traditional:
            slot.MyItem = Main.localPlayer.GetItem(Main.myPlayer, slot.MyItem);
            return (slot.MyItem.IsBlank());

        }
        // attempts to move an item to an empty slot
        public static bool ShiftToPlayer(ref ItemSlot slot, int ixStart, int ixStop, bool sendMessage, bool desc=false)
        {
            int iStart; Func<int,bool> iCheck; Func<int,int> iNext;

            if (desc) { iStart =  ixStop; iCheck = i => i >= ixStart; iNext = i => i-1; }
            else      { iStart = ixStart; iCheck = i => i <=  ixStop; iNext = i => i+1; }

            int retIdx = IHUtils.MoveToFirstEmpty( slot.MyItem, Main.localPlayer.inventory, iStart, iCheck, iNext );
            if (retIdx >= 0)
            {
                Main.PlaySound(7, -1, -1, 1);
                slot.MyItem = new Item();
                if (sendMessage) IHUtils.SendNetMessage(retIdx);
                return true;
            }
            return false;
        }
    #endregion

        // public override bool? ItemSlotAllowsItem(ItemSlot slot, Item item)
        // {
        //     if (IHBase.oLockingEnabled && slot.type == "Inventory" && slot.index >=10 && IHPlayer.SlotLocked(slot.index))
        //     {
        //
        //     }
        //     return null;
        // }

    }
}
