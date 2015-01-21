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
                sb.DrawString(Main.fontMouseText, state.label, bBase.Position, state.tint*bBase.Alpha);
            else
                sb.Draw(state.texture, bBase.Position, null, state.tint*bBase.Alpha, 0f, default(Vector2), bBase.Scale, SpriteEffects.None, 0f);
        }

        public static void DrawIHButton(this SpriteBatch sb, ButtonBase bBase, ButtonState state, Color overrideColor)
        {
            if (state.texture==null)
            sb.DrawString(Main.fontMouseText, state.label, bBase.Position, overrideColor*bBase.Alpha);
            else
            sb.Draw(state.texture, bBase.Position, null, overrideColor*bBase.Alpha, 0f, default(Vector2), bBase.Scale, SpriteEffects.None, 0f);
        }

        // get the color of whatever is behind the given point, pulled from the backbuffer
        // FIXME: this doesn't work... It Always returns transparent White??
        // public static Color GetColorBehind(this Point p)
        // {
        //     var backBufferData = new Texture2D(
        //     API.main.GraphicsDevice,
        //     API.main.GraphicsDevice.PresentationParameters.BackBufferWidth,
        //     API.main.GraphicsDevice.PresentationParameters.BackBufferHeight);
        //
        //     var retrievedColor = new Color[1];
        //     backBufferData.GetData<Color>(0, new Rectangle(p.X,p.Y,1,1), retrievedColor, 0, 1);
        //     backBufferData.Dispose();
        //
        //     return retrievedColor[0];
        // }

        public static Color GetColorBehind(this Point p)
        {
            var pTile = new TilePoint(p);

            Color lighting = Lighting.GetColor(pTile.X, pTile.Y);

            return lighting;
        }

        public static Color GetColorBehind(this TilePoint pTile)
        {
            // var pTile = new TilePoint(p);

            Color lighting = Lighting.GetColor(pTile.X, pTile.Y);

            return lighting;
        }

        // public static byte fullColor = (byte)255;
        public static Color Invert(this Color c)
        {
            return new Color(255-c.R, 255-c.G, 255-c.B, 255);

            // c.R=(byte)(255-c.R);
            // c.G=(byte)(255-c.G);
            // c.B=(byte)(255-c.B); //, c.A);
        }
    #endregion

    }

    public struct TilePoint
    {
        public int X;
        public int Y;

        public TilePoint(int x, int y)
        {
            X=x;
            Y=y;
        }

        public TilePoint(Vector2 coords)
        {
            X=(int)(coords.X/16);
            Y=(int)(coords.Y/16);
        }

        public TilePoint(Point screenPoint)
        {
            // X=(int)(Main.screenPosition.X/16 + (float)screenPoint.X/16);
            // Y=(int)(Main.screenPosition.Y/16 + (float)screenPoint.Y/16);
            X=(int)(API.main.firstTileX + (float)screenPoint.X/16);
            Y=(int)(API.main.firstTileY + (float)screenPoint.Y/16);
        }

        public static int ConvertToTileCoords(int num)
        {
            return (int)((float)num/16);
        }
    }

}
