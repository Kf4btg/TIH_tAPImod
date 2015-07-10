using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    /// A ButtonLayer is a container that holds one or more Buttons (ButtonBase class).
    /// It occupies a rectangular area on the screen, with its size and shape
    /// defined by the buttons that it holds. It will be just big enough to hold them
    /// all--but it must do so within a single rectangle, so its buttons should be grouped
    /// near each other to prevent it taking up a large section of the screen. Buttons
    /// that need to be placed in a different area of the screen should be placed in
    /// separate ButtonLayer.
    ///
    /// The ButtonLayer is responsible for calling Draw() on each of its components.
    /// It also defines the area in which the mouse will be considered "hovered"
    /// over this group of buttons.
    ///
    /// ButtonLayer can be utilized by subclassing it and customizing its constructor
    /// for each group of buttons you would like displayed.
    public abstract class ButtonLayer : InterfaceLayer
    {
        public readonly Dictionary<TIH, ButtonBase> Buttons;

        /// Area on screen containing all the buttons in Buttons within
        /// its boundaries. Use this to determine MouseInterface
        public Rectangle ButtonFrame { get; protected set;}
        public bool IsHovered {
            get
            {
                return ButtonFrame.Contains(Main.mouseX, Main.mouseY);
            }
        }

        protected float opacity_inactive = 0.45f;
        protected float opacity_active = 1.0f;

        //TODO: have this fade in/out?
        public float LayerOpacity { get; protected set; }

        /// Constructor
        protected ButtonLayer(string name) : base(IHBase.Instance.mod.InternalName + ":" + name)
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
                Main.localPlayer.mouseInterface = true;
                LayerOpacity=opacity_active;
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
