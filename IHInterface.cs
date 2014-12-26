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
        private Texture2D lockedMarker = null;

        public override void PostDrawItemSlotBackground(SpriteBatch sb, ItemSlot slot)
        {
            if (IHBase.oLockingEnabled && slot.type == "Inventory" && slot.index >=10 && IHPlayer.SlotLocked(slot.index))
            {
                    if (lockedMarker == null)
                    {
                        lockedMarker = IHBase.self.textures["resources/LockIndicator"];
                    }

                    sb.Draw(lockedMarker,                // the texture to draw
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
            if (KState.Special.Shift.Down() && IHBase.oLockingEnabled && slot.modBase == null && Main.playerInventory && release )
            {
                if (slot.type == "Inventory" && slot.index >= 10) //not in the hotbar
                {
                    IHPlayer.ToggleLock(slot.index); //toggle lock state
                    Main.PlaySound(7, -1, -1, 1);
                }
                return false;
            }
            return true;
        }
    }
}
