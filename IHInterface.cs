using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
                        if (!slot.MyItem.IsBlank() && MovePlayerSlotItem(ref slot))
                            Recipe.FindRecipes(); // !ref:Main:#22640.36#

                        // Item pItem = slot.MyItem;
                        // int? retIdx = IHUtils.MoveItem(ref pItem, Main.localPlayer.chestItems);
                        // if (retIdx.HasValue)
                        // {//some movement occurred
                        //     Main.PlaySound(7, -1, -1, 1);
                        //     if ((int)retIdx>=0) slot.MyItem = new Item();
                        // }
                        // Main.PlaySound(7, -1, -1, 1);
                        // slot.MyItem = myItem;
                    }
                    else if (slot.type == "Chest" && !slot.MyItem.IsBlank())
                    {
                        // seriously is this all it takes aaaarrrhghghghaghggggghgghhh
                        slot.MyItem = Main.localPlayer.GetItem(Main.myPlayer, slot.MyItem);
                        if (Main.localPlayer.chest > -1) IHUtils.SendNetMessage(slot.index);
                        // MoveSlotItem(ref slot, Main.localPlayer.chest >- 1);

                        // Item cItem = slot.MyItem;
                        //
                        // // MoveItem returns true if original item ends up empty
                        // if (cItem.IsBlank() || (cItem.Matches(ItemCat.COIN) &&
                        //     MoveChestSlotItem(ref slot, 50, 53))) return false;
                        //
                        // if (cItem.Matches(ItemCat.AMMO) &&
                        //     MoveChestSlotItem(ref slot, 54, 57)) return false;
                        //
                        // MoveChestSlotItem(ref slot, 0, 49);

                        // Main.PlaySound(7, -1, -1, 1);
                        // slot.MyItem = cItem;
                    }
                    return false;
                }
                if (Main.craftGuide)
                {
                    if (Main.guideItem.IsBlank() && !slot.MyItem.IsBlank() && (slot.type == "Inventory" || slot.type == "Coin" || slot.type == "Ammo") )
                    {
                        Main.PlaySound(7, -1, -1, 1);
                        Main.guideItem = slot.MyItem.Clone();
                        slot.MyItem = new Item();
                    }
                    else if (!Main.guideItem.IsBlank() && slot.type == "CraftGuide")
                    {
                        Main.guideItem = Main.localPlayer.GetItem(Main.myPlayer, Main.guideItem);
                        // MoveGuideItem(ref slot);
                        // if (MoveSlotItem(ref slot)) Recipe.FindRecipes();
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
        public static bool MovePlayerSlotItem(ref ItemSlot slot)
        {
            // if regular chest
            return (Main.localPlayer.chest > -1) ?
                 IHUtils.MoveItemToChest(slot.index, Main.ChestCoins, true) :
            //else banks
                IHUtils.MoveItemToChest(slot.index, Main.BankCoins, false);
        }
        // public static bool MoveGuideItem(ref ItemSlot slot)
        // {
        //
        // }
        // // MoveChestSlotItem - moves item from chest/guide slot to player inventory
        // public static bool MoveSlotItem(ref ItemSlot slot, bool sendMessage)
        // {
        //     Item cItem = slot.MyItem;
        //
        //     if (cItem.IsBlank()) return false;
        //
        //     // MoveItem returns true if original item ends up empty
        //     if (cItem.Matches(ItemCat.COIN) &&
        //     MoveSlotItem(ref slot, Main.localPlayer.inventory, 50, 53, sendMessage, true)) return true;
        //
        //     if (cItem.Matches(ItemCat.AMMO) &&
        //     MoveSlotItem(ref slot, Main.localPlayer.inventory, 54, 57, sendMessage, true)) return true;
        //
        //     return MoveSlotItem(ref slot, Main.localPlayer.inventory,  0, 49, true);
        // }
        //
        // public static bool MoveSlotItem(ref ItemSlot slot, int ixStart, int ixStop, bool sendMessage, bool desc=false)
        // {
        //     int retIdx = IHUtils.MoveItemC2P(ref slot.MyItem, Main.localPlayer.inventory, ixStart, ixStop, desc);
        //     if (retIdx > -2)
        //     {   //some movement occurred
        //         Main.PlaySound(7, -1, -1, 1);
        //
        //         // if whole stack moved, empty item slot
        //         if (retIdx > -1) {
        //             slot.MyItem = new Item();
        //             if (sendMessage) IHUtils.SendNetMessage(retIdx);
        //                 return true;
        //         }
        //     }
        //     return false;
        // }
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
