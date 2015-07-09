using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TAPI;
using Terraria;
using Terraria.ID;

namespace InvisibleHand
{
    public static class IHExtensions
    {

    #region itemExensions
        public static ItemCat GetCategory(this Item item)
        {
            foreach (ItemCat catID in Constants.CheckOrder)
            {
                if (CategoryDef.Categories[catID].Invoke(item)) return catID;
            }
            return ItemCat.OTHER;
        }

        public static bool Matches(this Item item, ItemCat isCategory)
        {
            return CategoryDef.Categories[isCategory].Invoke(item);
        }

        public static bool IsHook(this Item item)
        {
            return ProjDef.byType.ContainsKey(item.shoot) && (ProjDef.byType[item.shoot].hook || ProjDef.byType[item.shoot].aiStyle==7);
        }

        public static bool IsBomb(this Item item)
        {   //grenades, bombs, etc
            return ProjDef.byType.ContainsKey(item.shoot) && ProjDef.byType[item.shoot].aiStyle==16;
        }

        public static bool IsTool(this Item item)
        {
            return item.createTile == TileID.Rope || item.createTile == TileID.Chain || item.name.EndsWith("Bucket") ||
            item.fishingPole > 1 || item.tileWand != -1 || item.IsHook() || ItemDef.autoSelect["Glowstick"].Contains(item.type) ||
            item.type == 1991 || item.type == 50 || item.type == 1326 || ItemDef.autoSelect["Flaregun"].Contains(item.type) ||
            item.name.Contains("Paintbrush") || item.name.Contains("Paint Roller") || item.name.Contains("Paint Scraper") ||
            (item.type >= 1543 && item.type <= 1545);
            //bucket, bug net, magic mirror, rod of discord, spectre paint tools
        }
    #endregion

    #region buttonExtensions

        public static void DrawIHButton(this SpriteBatch sb, ButtonBase bBase, ButtonState state)
        {


            if (state.texture == null)
            {
                //doing this here to see if this enables the "pulse" effect all the vanilla text has
                // Result: YES!
                Color textColor = Main.mouseTextColor.toScaledColor(bBase.Scale);

                // Do these put the text in the proper place and cause it to expand like vanilla?
                // Result: YES!
                Vector2 origin = bBase.CurrentContext.Size / 2;
                Vector2 pos = new Vector2(bBase.Position.X + (int)(origin.X * bBase.Scale), bBase.Position.Y);

                sb.DrawString(
                        Main.fontMouseText,     //font
                        state.label,            //string
                        // bBase.Position,         //position
                        pos,
                        // state.tint*bBase.Alpha, //color
                        textColor,
                        0f,                     //rotation
                        // default(Vector2),       //origin
                        origin,
                        bBase.Scale,            //scale
                        SpriteEffects.None,     //effects
                        0f                      //layerDepth
                     );
            }
            else
                sb.Draw(state.texture, bBase.Position, bBase.SourceRect, state.tint * bBase.Alpha, 0f, default(Vector2), bBase.Scale, SpriteEffects.None, 0f);
        }

        public static void DrawIHButton(this SpriteBatch sb, ButtonBase bBase, ButtonState state, Color overrideColor)
        {
            if (state.texture==null)
                sb.DrawString(
                        Main.fontMouseText,     //font
                        state.label,            //string
                        bBase.Position,         //position
                        overrideColor*bBase.Alpha, //color
                        0f,                     //rotation
                        default(Vector2),       //origin
                        bBase.Scale,            //scale
                        SpriteEffects.None,     //effects
                        0f                      //layerDepth
                     );
            else
                sb.Draw(state.texture, bBase.Position, bBase.SourceRect, overrideColor*bBase.Alpha, 0f, default(Vector2), bBase.Scale, SpriteEffects.None, 0f);
        }

        public static void DrawButtonBG(this SpriteBatch sb, ButtonBase bb, Texture2D bgTex, Color bgColor)
        {
            sb.Draw(bgTex, bb.Position, null, bgColor*bb.Alpha, 0f, default(Vector2), bb.Scale, SpriteEffects.None, 0f);
        }

        public static bool IsHovered(this Rectangle frame)
        {
            return frame.Contains(Main.mouseX, Main.mouseY);
        }

        /// Convert a byte to an rgba Color, using the
        /// value of the byte for each field unless a value > 1
        /// is provided for alpha. If a mult. factor is provided, each
        /// color component will be multiplied by that factor.
        public static Color toColor(this byte b, float mult = 1, float alpha = -1)
        {
            byte a;
            if (mult != 1) {
                // casting to byte is basically just taking modulus 256:
                //  (b*mult)%256
                byte c = (byte)((float)b * mult);
                a = alpha > 0 ? (byte)(alpha * mult) : c;
                return new Color((int)c, (int)c, (int)c, (int)a);
            }
            a = alpha > 0 ? (byte)alpha : b ;
            return new Color((int)b, (int)b, (int)b, (int)a);
        }

        /// This is a simplified version of toColor specifically tailored
        /// for being called each frame when drawing text-only buttons.
        /// This enables the pulse effect seen on the vanilla text.
        public static Color toScaledColor(this byte b, float mult)
        {
            var c = (int)((byte)((float)b * mult));
            return new Color(c, c, c, c);
        }

        public static Color toScaledColor(this byte b, float mult, Color tint)
        {
            var c = (int)((byte)((float)b * mult));
            return TAPI.Extensions.Multiply(new Color(c, c, c, c), tint);
            // Terraria.Utils.Multiply(textColor, tint),

        }


    #endregion

    }
}
