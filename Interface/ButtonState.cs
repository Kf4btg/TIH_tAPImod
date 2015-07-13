using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

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
        public string tooltip;

        /// The game-affecting action this state represents
        /// (not necessarily unique amongst all buttonstates).
        public TIH action;

        public Texture2D texture;

        ///texture source rectangle for base, non-mouseover appearance
        public Rectangle? defaultTexels;
        ///texture source rect for mouseover appearance
        public Rectangle? altTexels;

        public Action onClick;
        public Action onRightClick;

        // hooks (plugged into the corresponding hooks in IHButton)
        public Func<ButtonBase,bool> onMouseEnter;
        public Func<ButtonBase,bool> onMouseLeave;

        /// NOTE: currently not called in ButtonBase
        public Func<SpriteBatch, ButtonBase, bool> PreDraw;
        /// Called after all drawing and mouse-interaction checking is done
        public Action<SpriteBatch, ButtonBase> PostDraw;

        /// to change tint of the button texture/text
        public Color tint;
        /// color of button background, if any;
        public Color bgColor;
        /// color of lockIndicator, if used
        public Color lockColor;

        public OperationCollection operations;

        //make sure it at least has a related action and a label on construction
        public ButtonState(TIH action, string label="")
        {
            this.action = action;
            this.label  = label == "" ? Constants.DefaultButtonLabels[action] : label;
            this.tint   = Color.White;

            this.tooltip = this.label + IHUtils.GetKeyTip(action);
        }

        // found myself commonly creating series of states where all these were
        // consistent; this overload eases that process
        public ButtonState(TIH action, string label, Texture2D tex, Rectangle? defaultTexels, Rectangle? altTexels, Color? tintColor = null)
        {
            this.label         = label;
            this.texture       = tex;
            this.defaultTexels = defaultTexels;
            this.altTexels     = altTexels;
            this.tint          = tintColor ?? Color.White;

            this.tooltip = this.label + IHUtils.GetKeyTip(action);

        }

        public ButtonState Duplicate()
        {
            var bsNew          = new ButtonState(this.action, this.label);
            bsNew.tooltip      = tooltip;
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
            action       = bsCopy.action;
            label        = bsCopy.label;
            tooltip      = bsCopy.tooltip;
            texture      = bsCopy.texture;
            altTexels    = bsCopy.altTexels;
            defaultTexels= bsCopy.defaultTexels;
            onClick      = bsCopy.onClick;
            onRightClick = bsCopy.onRightClick;
            tint         = bsCopy.tint;
            onMouseEnter = bsCopy.onMouseEnter;
            onMouseLeave = bsCopy.onMouseLeave;
        }

        // TODO: make Iterable. Enumerable. I don't knowable, one of those.
        public class OperationCollection
        {
            public Action onClick { get; set;}
            public Action onRightClick { get; set;}

            public Func<ButtonBase,bool> onMouseEnter { get; set;}
            public Func<ButtonBase,bool> onMouseLeave { get; set;}

            public Func<SpriteBatch, ButtonBase, bool> preDraw { get; set;}
            public Action<SpriteBatch, ButtonBase> postDraw { get; set;}

            public OperationCollection(
                Action onClick = null,
                Action onRightClick = null,
                Func<ButtonBase,bool> onMouseEnter = null,
                Func<ButtonBase,bool> onMouseLeave = null,
                Func<SpriteBatch, ButtonBase, bool> preDraw = null,
                Action<SpriteBatch, ButtonBase> postDraw = null)
                {
                    this.onClick = onClick;
                    this.onRightClick = onRightClick;
                    this.onMouseEnter = onMouseEnter;
                    this.onMouseLeave = onMouseLeave;
                    this.preDraw = preDraw;
                    this.postDraw = postDraw;
                }

            public OperationCollection(OperationCollection other) :
                this(other.onClick, other.onRightClick, other.onMouseEnter,
                     other.onMouseLeave, other.preDraw, other.postDraw) {}
        }
    }
}
