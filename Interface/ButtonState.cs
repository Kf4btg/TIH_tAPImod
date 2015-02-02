using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    /**
    * 	This class holds all the technical details (appearance, function, etc.) of a specific button
    * 	implementation. This does not define "a button" in this project, though, as a single conceptual button
    * 	(as it appears to the player) can exhibit different states depending on various circumstances.
    * 	TODO: make the constructors more thorough and/or more standardized
    */
    public class ButtonState
    {
        public string label;
        public Texture2D texture;

        public Rectangle? defaultTexels;  //texture source rectangle for base, non-mouseover appearance
        public Rectangle? altTexels;    //texture source rect for mouseover appearance

        public Action onClick;
        public Action onRightClick;

        public Func<ButtonBase,bool> onMouseEnter;
        public Func<ButtonBase,bool> onMouseLeave;

        public Func<SpriteBatch, ButtonBase, bool> PreDraw;
        public Action<SpriteBatch, ButtonBase> PostDraw;

        public Color tint;

        public ButtonState()
        {
            label         = "Button";
            texture       = null;
            altTexels     = null;
            defaultTexels = null;
            onClick       = null;
            onRightClick  = null;
            onMouseEnter  = null;
            onMouseLeave  = null;
            tint          = Color.White;
        }

        public ButtonState(string label, Texture2D tex=null, Rectangle? defaultTexels=null, Action onClick=null, Action onRightClick=null, Color? tintColor = null)
        {
            this.label         = label;
            this.texture       = tex;
            this.defaultTexels = defaultTexels;
            this.altTexels     = null;
            this.onClick       = onClick;
            this.onRightClick  = onRightClick;
            this.onMouseEnter  = null;
            this.onMouseLeave  = null;
            this.tint          = tintColor ?? Color.White;
        }

        public ButtonState Duplicate()
        {
            var bsNew          = new ButtonState();
            bsNew.label        = label;
            bsNew.texture      = texture;
            bsNew.altTexels    = altTexels;
            bsNew.defaultTexels= defaultTexels;
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
            altTexels    = bsCopy.altTexels;
            defaultTexels= bsCopy.defaultTexels;
            onClick      = bsCopy.onClick;
            onRightClick = bsCopy.onRightClick;
            tint         = bsCopy.tint;
            onMouseEnter = bsCopy.onMouseEnter;
            onMouseLeave = bsCopy.onMouseLeave;
        }
    }
}
