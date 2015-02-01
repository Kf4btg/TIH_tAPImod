using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class ButtonState
    {
        public string label;
        public Texture2D texture;
        public Rectangle? sourceRect;
        public Action onClick;
        public Action onRightClick;
        public Func<ButtonBase,bool> onMouseEnter;
        public Func<ButtonBase,bool> onMouseLeave;
        public Color tint;      //How to tint the texture when this state is active

        public ButtonState()
        {
            label        = "Button";
            texture      = null;
            sourceRect   = null;
            onClick      = null;
            onRightClick = null;
            onMouseEnter = null;
            onMouseLeave = null;
            tint         = Color.White;
        }

        public ButtonState(string label, Texture2D tex=null, Rectangle? sourceRect=null, Action onClick=null, Action onRightClick=null, Color? tintColor = null)
        {
            this.label        = label;
            this.texture      = tex;
            this.sourceRect   = sourceRect;
            this.onClick      = onClick;
            this.onRightClick = onRightClick;
            this.onMouseEnter = null;
            this.onMouseLeave = null;
            this.tint         = tintColor ?? Color.White;
        }

        public void SetSourceRect(int x, int y, int w, int h)
        {
            sourceRect = new Rectangle(x,y,w,h);
        }

        public ButtonState Duplicate()
        {
            var bsNew          = new ButtonState();
            bsNew.label        = label;
            bsNew.texture      = texture;
            bsNew.sourceRect   = sourceRect;
            bsNew.onClick      = onClick;
            bsNew.onRightClick = onRightClick;
            bsNew.tint         = tint;
            bsNew.onMouseEnter = onMouseEnter;
            bsNew.onMouseLeave = onMouseLeave;

            return bsNew;
        }

        public void CopyFrom(ButtonState bsCopy)
        {
            label        = bsCopy.label;
            texture      = bsCopy.texture;
            sourceRect   = bsCopy.sourceRect;
            onClick      = bsCopy.onClick;
            onRightClick = bsCopy.onRightClick;
            tint         = bsCopy.tint;
            onMouseEnter = bsCopy.onMouseEnter;
            onMouseLeave = bsCopy.onMouseLeave;
        }

    }
}
