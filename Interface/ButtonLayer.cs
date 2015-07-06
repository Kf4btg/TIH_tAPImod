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

    // The Button Factory (tm)
    // Use this to get instances of the button-layer types rather than
    // directly calling their constructor.  This ensures that the layer
    // frame will be properly updated.
    public static class ButtonMaker
    {
        public static ButtonLayer GetButtons(String type)
        {
            ButtonLayer btns;
            switch(type)
            {
                case "Inventory":
                    btns = new InventoryButtons(IHBase.Instance);
                    break;
                case "Chest":
                    btns = new ChestButtons(IHBase.Instance);
                    break;
                default:
                    throw new ArgumentException("Invalid ButtonLayer type \"" + type + "\"; valid types are \"Inventory\" and \"Chest\".");
            }
            btns.UpdateFrame();
            return btns;
        }
    }
}
