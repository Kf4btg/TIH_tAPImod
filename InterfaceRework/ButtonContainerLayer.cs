using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public abstract class ButtonContainerLayer : InterfaceLayer
    {
        public readonly Dictionary<TIH, ButtonBase> Buttons;

        /// Area on screen containing all the buttons in Buttons within
        /// its boundaries. Use this to determine MouseInterface
        public Rectangle ButtonFrame { get; protected set;}
        public bool IsHovered {
            get {
                return ButtonFrame.Contains(Main.mouseX, Main.mouseY);
            }
        }

        /// whether or not to set MouseInterface=true
        /// when the mouse is ANYwhere over this layer,
        /// including blank space; some buttons may need
        /// to handle this themselves, so set it false in
        /// those cases. Also disables the opacity stuff.
        public bool handleMouseInterface = true;

        protected float opacity_inactive, opacity_active = 1.0f;

        //TODO: have this fade in/out?
        public float LayerOpacity { get; protected set; }

        /// Constructor
        protected ButtonContainerLayer(string name) : base(IHBase.Instance.mod.InternalName + ":" + name)
        {
            Buttons = new Dictionary<TIH, ButtonBase>();
            ButtonFrame = Rectangle.Empty;
        }

        internal void UpdateFrame()
        {
            // initialize this here so it doesn't somehow get stuck at 0
            LayerOpacity = opacity_inactive;
            // ButtonFrame = rectangle big enough to contain all the buttons assigned to this layer
            foreach (var kvp in Buttons)
            {
                ButtonFrame = (ButtonFrame.IsEmpty) ? kvp.Value.ButtonBounds : Rectangle.Union(ButtonFrame, kvp.Value.ButtonBounds);
            }
        }

        protected virtual void DrawButtons(SpriteBatch sb)
        {
            //KeyValuePair<TIH, ButtonBase>
            foreach (var kvp in Buttons)
            {
                kvp.Value.Draw(sb);
            }
        }

        protected override void OnDraw(SpriteBatch sb)
        {
            if (!parentLayer.visible) return;

            if (IsHovered)
            {
                if (handleMouseInterface)
                {
                    Main.localPlayer.mouseInterface = true;
                    LayerOpacity = opacity_active;
                }
                DrawButtons(sb);
                return;
            }
            // these two calls are down here so we can use the return statement
            // above to avoid setting the opacity twice on each call to OnDraw
            LayerOpacity=opacity_inactive;
            DrawButtons(sb);
        }
    }
}
