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


        protected ButtonLayer(string name) : base("InvisibleHand:" + name)
        {
            Buttons = new Dictionary<IHAction, ButtonBase>();
            ButtonFrame = Rectangle.Empty;
            FrameCenter = Point.Zero;
            onDraw=_onDraw;
        }


        protected void UpdateFrame(bool useInvertedBGColor=false)
        {
            foreach (var kvp in Buttons)
            {
                ButtonFrame = (ButtonFrame.IsEmpty) ? kvp.Value.ButtonBounds : Rectangle.Union(ButtonFrame, kvp.Value.ButtonBounds);
            }

            if (!ButtonFrame.IsEmpty && useInvertedBGColor)
            {
                onDraw=_onDrawOverrideColor;
                FrameCenter = ButtonFrame.Center;
            }
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
            Color oc = FrameCenter.GetColorBehind().Invert();
            // sbc.DrawString(Main.fontMouseText, FrameCenter.ToString(), new Vector2((float)FrameCenter.X, (float)FrameCenter.Y), Color.White);
            // oc.A=(byte)255;
            // oc.Invert();
            foreach (KeyValuePair<IHAction, ButtonBase> kvp in Buttons)
            {
                kvp.Value.Draw(sbc, oc);
            }
        }

        protected override void OnDraw(SpriteBatch sb)
        {
            onDraw(sb);
            // foreach (KeyValuePair<IHAction, ButtonBase> kvp in Buttons)
            // {
            //     kvp.Value.Draw(sb);
            // }
        }
    }

}
