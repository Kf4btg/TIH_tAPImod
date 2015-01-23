using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
// using Terraria;

namespace InvisibleHand
{
    public abstract class ButtonLayer : InterfaceLayer
    {
        public readonly Dictionary<IHAction, ButtonBase> Buttons;

        public Rectangle ButtonFrame { get; protected set;}
        public Point FrameCenter { get; private set;}

        //nonsense
        private Action<SpriteBatch> onDraw;
        private Func<Color> overrideButtonColor;

        protected ButtonLayer(string name) : base("InvisibleHand:" + name)
        {
            Buttons = new Dictionary<IHAction, ButtonBase>();
            ButtonFrame = Rectangle.Empty;
            FrameCenter = Point.Zero;
            onDraw=_onDraw;
        }

        protected void UpdateFrame(Func<Color> btnColor = null)
        {
            foreach (var kvp in Buttons)
            {
                ButtonFrame = (ButtonFrame.IsEmpty) ? kvp.Value.ButtonBounds : Rectangle.Union(ButtonFrame, kvp.Value.ButtonBounds);
            }
            if (!ButtonFrame.IsEmpty) FrameCenter = ButtonFrame.Center;
            if (btnColor!=null)
            {
                overrideButtonColor = btnColor;
                onDraw=_onDrawOverrideColor;
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
            foreach (KeyValuePair<IHAction, ButtonBase> kvp in Buttons)
            {
                kvp.Value.Draw(sbc, overrideButtonColor());
            }
        }

        protected override void OnDraw(SpriteBatch sb)
        {
            if (!parentLayer.visible) return;
            this.onDraw(sb);
            // foreach (KeyValuePair<IHAction, ButtonBase> kvp in Buttons)
            // {
            //     kvp.Value.Draw(sb);
            // }
        }
    }

}
