using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public abstract class ButtonLayer : InterfaceLayer
    {
        public readonly Dictionary<IHAction, ButtonBase> Buttons;

        public Rectangle ButtonFrame { get; protected set;}
        public Point FrameCenter { get; private set;}
        private Action<SpriteBatch> onDraw;
        private Color? prevColor = null;
        private Color currColor;


        protected ButtonLayer(string name) : base("InvisibleHand:" + name)
        {
            Buttons = new Dictionary<IHAction, ButtonBase>();
            ButtonFrame = Rectangle.Empty;
            FrameCenter = Point.Zero;
            onDraw=_onDraw;
        }

        //nope
        // protected void UpdateFrame(bool useInvertedBGColor=false)
        protected void UpdateFrame(bool useBGColor = true)
        {
            foreach (var kvp in Buttons)
            {
                ButtonFrame = (ButtonFrame.IsEmpty) ? kvp.Value.ButtonBounds : Rectangle.Union(ButtonFrame, kvp.Value.ButtonBounds);
            }
            if (useBGColor)
            {
                FrameCenter = ButtonFrame.Center;
                onDraw=_onDrawOverrideColor;
            }
            // if (!ButtonFrame.IsEmpty && useInvertedBGColor)
            // {
            //     onDraw=_onDrawOverrideColor;
            //     FrameCenter = ButtonFrame.Center;
            // }
        }


        private void _onDraw(SpriteBatch sb)
        {
            foreach (KeyValuePair<IHAction, ButtonBase> kvp in Buttons)
            {
                kvp.Value.Draw(sb);
            }
        }

        private void _onDrawOverrideColor(SpriteBatch sbc)
        {
            // Color ovColor = Main.bgColor; //TODO: process this to brighten up the color and make it stand out
            // if (prevColor==null) prevColor = Main.bgColor;
            // Color bg = Main.bgColor;
            // // currColor = (bg.R==0 || bg.G==0 || bg.B==0 || bg.R==255 || bg.G==255 || bg.B==255) ? prevColor : bg;
            // int s = bg.R+bg.G+bg.B;
            // currColor = s<255 || s>600 ? prevColor ?? Color.Gray : bg;
            // prevColor = currColor;

            // TilePoint p = new TilePoint(FrameCenter);
            // Color c;
            // Main.map[p.X,p.Y].getColor(out c, p.Y);


            foreach (KeyValuePair<IHAction, ButtonBase> kvp in Buttons)
            {
                // kvp.Value.Draw(sbc, Color.Lerp(Main.bgColor, Main.bgColor.Invert(), 0.5f));
                // kvp.Value.Draw(sbc, Color.Lerp(Main.bgColor.Rotate(), Color.White, 0.5f));
                // kvp.Value.Draw(sbc, currColor.Rotate().Rotate());
                // kvp.Value.Draw(sbc, currColor);
                kvp.Value.Draw(sbc, FrameCenter.GetMapColor(true).Invert());


            }
        }


        // private void _onDrawOverrideColor(SpriteBatch sbc)
        // {
        //     // Color oc = FrameCenter.GetColorBehind().Invert();
        //     // sbc.DrawString(Main.fontMouseText, FrameCenter.ToString(), new Vector2((float)FrameCenter.X, (float)FrameCenter.Y), Color.White);
        //     // oc.A=(byte)255;
        //     // oc.Invert();
        //     sbc.End();
        //     sbc.Begin(SpriteSortMode.Deferred, Constants.InverseBlendState);
        //
        //     foreach (KeyValuePair<IHAction, ButtonBase> kvp in Buttons)
        //     {
        //         kvp.Value.Draw(sbc);
        //     }
        //     sbc.End();
        //     sbc.Begin();//reset
        // }

        protected override void OnDraw(SpriteBatch sb)
        {
            if (!parentLayer.visible) return;
            onDraw(sb);
            // foreach (KeyValuePair<IHAction, ButtonBase> kvp in Buttons)
            // {
            //     kvp.Value.Draw(sb);
            // }
        }
    }

}
