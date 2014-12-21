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

        //TODO: make this detect current lock status of slot
        public static bool IsSlotLocked(ItemSlot slot)
        {
            if (slot!=null) return true;
            return false;
        }

    }
}
