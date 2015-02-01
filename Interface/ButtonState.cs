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
        // public Rectangle? sourceRect;
        // public Rectangle? altSourceRect;
        public TexSources texSource;
        public Action onClick;
        public Action onRightClick;
        public Func<ButtonBase,bool> onMouseEnter;
        public Func<ButtonBase,bool> onMouseLeave;
        public Color tint;      //How to tint the texture when this state is active



        public ButtonState()
        {
            label        = "Button";
            texture      = null;
            // sourceRect   = null;
            // altSourceRect= null;
            // texSource  = null;    --this is unnecessary
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
            // this.sourceRect   = sourceRect;
            // this.altSourceRect=null;
            texSource.Main = sourceRect;
            this.onClick      = onClick;
            this.onRightClick = onRightClick;
            this.onMouseEnter = null;
            this.onMouseLeave = null;
            this.tint         = tintColor ?? Color.White;
        }

        public void SetSourceRect(int x, int y, int w, int h)
        {
            // sourceRect = new Rectangle(x,y,w,h);
            texSource.Main = new Rectangle(x,y,w,h);
        }

        public void SetAltSourceRect(int x, int y, int w, int h)
        {
            texSource.Alt = new Rectangle(x,y,w,h);
        }

        public ButtonState Duplicate()
        {
            var bsNew          = new ButtonState();
            bsNew.label        = label;
            bsNew.texture      = texture;
            // bsNew.sourceRect   = sourceRect;
            // bsNew.altSourceRect= altSourceRect;
            bsNew.texSource = texSource;
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
            // sourceRect   = bsCopy.sourceRect;
            // altSourceRect= bsCopy.altSourceRect;
            texSource = bsCopy.texSource;
            onClick      = bsCopy.onClick;
            onRightClick = bsCopy.onRightClick;
            tint         = bsCopy.tint;
            onMouseEnter = bsCopy.onMouseEnter;
            onMouseLeave = bsCopy.onMouseLeave;
        }

        /**
         * Data Structures
         */
        public struct TexSources
        {
            //immutable once set
            public Rectangle? Main { get;
                set { if (!this.Main.HasValue) this.Main=value; } }
            public Rectangle? Alt { get;
                set { if (!this.Alt.HasValue) this.Alt=value; } }
        }

    }
}
