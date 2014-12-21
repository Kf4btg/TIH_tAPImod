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
        private Texture2D lockedMarker = null, unlockedMarker = null;

        public override void PostDrawItemSlotBackground(SpriteBatch sb, ItemSlot slot)
        {
            if (slot.type == "Inventory" && slot.index >=10)
            {
                if (lockedMarker == null)
                {
                    lockedMarker = IHBase.self.textures["resources/Lock1"];
                    unlockedMarker = IHBase.self.textures["resources/Lock0"];
                }

                Texture2D tex = IsSlotLocked(slot) ? lockedMarker : unlockedMarker;

                // TODO: replace the separate textures with one, colored by Color.xxxxx options
                sb.Draw(tex,                // the texture to draw
                        slot.pos,           // (Vector2) location in screen coords to draw sprite
                        null,               // Rectangle to specifies source texels from texture; null draws whole texture
                        Color.White,        // color to tint sprite; color.white=full color, no tint
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
            if (slot.modBase == null && Main.playerInventory && release && KState.Special.Shift.Down())
            {
                if (slot.type == "Inventory" && slot.index >= 10) //not in the hotbar
                {
                    Item myItem = slot.MyItem;
                    IHBase.LockedSlots[slot.index-10]=!IHBase.LockedSlots[slot.index-10]; //toggle lock state
                    Main.PlaySound(7, -1, -1, 1);
                }
                return false;
            }
            return true;
        }

        //TODO: decide if this should check slot type or just assume that the caller
        //      knows what they're doing
        public static bool IsSlotLocked(ItemSlot slot)
        {
            return IHBase.LockedSlots[slot.index-10];
        }

    }
}
