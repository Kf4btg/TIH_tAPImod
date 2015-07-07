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
        public readonly Dictionary<IHAction, ButtonBase> Buttons;

        public Rectangle ButtonFrame { get; protected set;}  //use this to determine MouseInterface
        public bool IsHovered { get { return ButtonFrame.Contains(Main.mouseX, Main.mouseY); } }

        protected float opacity_inactive = 0.45f;
        protected float opacity_active = 1.0f;

        //TODO: have this fade in/out?
        public float LayerOpacity { get; private set; }

        protected ButtonLayer(string name) : base(IHBase.Instance.mod.InternalName + ":" + name)
        {
            Buttons = new Dictionary<IHAction, ButtonBase>();
            ButtonFrame = Rectangle.Empty;
        }

        internal void UpdateFrame()
        {
            foreach (var kvp in Buttons)
            {
                ButtonFrame = (ButtonFrame.IsEmpty) ? kvp.Value.ButtonBounds : Rectangle.Union(ButtonFrame, kvp.Value.ButtonBounds);
            }
        }

        protected virtual void DrawButtons(SpriteBatch sb)
        {
            foreach (KeyValuePair<IHAction, ButtonBase> kvp in Buttons)
            {
                kvp.Value.Draw(sb);
            }
        }

        protected override void OnDraw(SpriteBatch sb)
        {
            if (!parentLayer.visible) return;

            LayerOpacity=opacity_inactive;
            if (IsHovered)
            {
                Main.localPlayer.mouseInterface = true;
                LayerOpacity=opacity_active;
            }
            DrawButtons(sb);
        }
    }
}
