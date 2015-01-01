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

        // Shift + Left Click on item slot to move it between inventory and chest
        public override bool PreItemSlotLeftClick(ItemSlot slot, ref bool release)
        {
            if (!(bool)IHBase.self.options["enableShiftMove"].Value) return true;
            if (!(slot.modBase == null && release && KState.Special.Shift.Down())) return true;

            if (Main.localPlayer.chestItems != null)
            {
                if (slot.type == "Inventory" || slot.type == "Coin" || slot.type == "Ammo")
                {
                    MovePlayerSlotItem(ref slot);

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
                else if (slot.type == "Chest")
                {
                    MoveSlotItem(ref slot);
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
                if (Main.guideItem.IsBlank() && (slot.type == "Inventory" || slot.type == "Coin" || slot.type == "Ammo"))
                {
                    Main.PlaySound(7, -1, -1, 1);
                    Main.guideItem = slot.MyItem.Clone();
                    slot.MyItem = new Item();
                    return false;
                }
                if (!Main.guideItem.IsBlank() && (slot.type == "CraftGuide"))
                {
                    if (MoveSlotItem(ref slot)) Recipe.FindRecipes();
                }
                return false;
            }

            return true;
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

        /**************************************************************
        *   returns true if item moved/itemstack emptied
        */

        // move item from player inventory slot to chest
        public static bool MovePlayerSlotItem(ref ItemSlot slot)
        {
            return MoveSlotItem(ref slot, Main.localPlayer.chestItems, 0, Chest.maxItems);
        }

        // move craft guide item to player inventory
        // public static bool MoveGuideSlotItem(ref ItemSlot slot)
        // {
        //     return MoveSlotItem
        // }

        // MoveChestSlotItem - moves item from chest/guide slot to player inventory
        public static bool MoveSlotItem(ref ItemSlot slot)
        {
            Item cItem = slot.MyItem;

            // MoveItem returns true if original item ends up empty
            if (cItem.IsBlank() || (cItem.Matches(ItemCat.COIN) &&
            MoveSlotItem(ref slot, Main.localPlayer.inventory, 50, 53, true))) return false;

            if (cItem.Matches(ItemCat.AMMO) &&
            MoveSlotItem(ref slot, Main.localPlayer.inventory, 54, 57, true)) return false;
            //else:
            MoveSlotItem(ref slot, Main.localPlayer.inventory,  0, 49, true);


            // return MoveSlotItem(ref slot, Main.localPlayer.inventory, ixStart, ixStop, true);
        }

        public static bool MoveSlotItem(ref ItemSlot slot, Item[] container, int ixStart, int ixStop, bool desc=false)
        {
            int? retIdx = IHUtils.MoveItem(ref slot.MyItem, container, ixStart, ixStop, desc);
            if (retIdx.HasValue)
            {   //some movement occurred
                Main.PlaySound(7, -1, -1, 1);

                // if whole stack moved, empty item slot
                if ((int)retIdx>=0) {
                    slot.MyItem = new Item();
                    return true;
                }
            }
            return false;

            // if (IHUtils.DoMoveItem())
            // return false;
        }

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
