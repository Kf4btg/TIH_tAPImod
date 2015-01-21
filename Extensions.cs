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
        {            //grenades, bombs, etc
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
            if (state.texture==null)
                sb.DrawString(Main.fontMouseText, state.label, bBase.Position, bBase.Tint*bBase.Alpha);
            else
                sb.Draw(state.texture, bBase.Position, null, bBase.Tint*bBase.Alpha, 0f, default(Vector2), bBase.Scale, SpriteEffects.None, 0f);
        }

        // get the color of whatever is behind the given point, pulled from the backbuffer
        public static Color GetColorBehind(this Point p)
        {
            var backBufferData = new Texture2D(
            GraphicsDevice,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight);

            var retrievedColor = new Color[1];
            backBufferData.GetData<Color>(0, new Rectangle(p.X,p.Y,1,1), retrievedColor, 0, 1);

            return retrievedColor[0];
        }

        public static byte fullColor = (byte)255;
        public static Color Invert(this Color c)
        {
            return new Color(fullColor-c.R, fullColor-c.G, fullColor-c.B, c.A);
        }
    #endregion

    }

}
