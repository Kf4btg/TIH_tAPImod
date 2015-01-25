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
                sb.Draw(state.texture, bBase.Position, state.sourceRect, state.tint*bBase.Alpha, 0f, default(Vector2), bBase.Scale, SpriteEffects.None, 0f);
        }

        public static void DrawIHButton(this SpriteBatch sb, ButtonBase bBase, ButtonState state, Color overrideColor)
        {
            if (state.texture==null)
            sb.DrawString(Main.fontMouseText, state.label, bBase.Position, overrideColor*bBase.Alpha);
            else
            sb.Draw(state.texture, bBase.Position, state.sourceRect, overrideColor*bBase.Alpha, 0f, default(Vector2), bBase.Scale, SpriteEffects.None, 0f);
        }

        public static void DrawButtonBG(this SpriteBatch sb, ButtonBase bb, Texture2D bgTex, Color bgColor)
        {
            sb.Draw(bgTex, bb.Position, null, bgColor*bb.Alpha, 0f, default(Vector2), bb.Scale, SpriteEffects.None, 0f);
        }



    #endregion

    #region colorandpointstuff
    /*
        public static Color Invert(this Color c)
        {
            return new Color(255-c.R, 255-c.G, 255-c.B, 255);
        }

        public static Color Rotate(this Color c)
        {
            return new Color(c.B, c.R, c.G, 255);
        }

        public static Color GetMapColor(this Point p, bool applyLighting=false)
        {
            return GetMapColor(new TilePoint(p), applyLighting);
        }

        public static Color GetMapColor(this TilePoint pTile, bool applyLighting=false)
        {
            Color mColor;
            try
            {
                //this is sometimes null
                Main.map[pTile.X,pTile.Y].getColor(out mColor, pTile.Y);
            }
            catch
            {
                mColor = Color.White;
            }

            if (applyLighting)
            {
                Color lighting = Lighting.GetColor(pTile.X, pTile.Y);
                float scale = (float)(lighting.R+lighting.G+lighting.B)/3/255;

                mColor*=scale;
            }
            return mColor;
        }
    }

    public struct TilePoint
    {
        public int X;
        public int Y;

        public int scrX;
        public int scrY;

        public TilePoint(int x, int y)
        {
            X=x;
            Y=y;

            scrX = (x - API.main.firstTileX)*16;
            scrY = (y - API.main.firstTileY)*16;
        }

        // public TilePoint(Vector2 coords)
        // {
        //     X=(int)(coords.X/16);
        //     Y=(int)(coords.Y/16);
        // }

        public TilePoint(Point screenPoint)
        {
            scrX=screenPoint.X;
            scrY=screenPoint.Y;
            X=ConvertToTileCoords(screenPoint.X, true);
            Y=ConvertToTileCoords(screenPoint.Y, false);
        }

        public static int ConvertToTileCoords(int num, bool onXaxis)
        {
            return onXaxis ? (int)(API.main.firstTileX + (float)num/16) : (int)(API.main.firstTileY + (float)num/16);
        }*/
    }
    #endregion

}
