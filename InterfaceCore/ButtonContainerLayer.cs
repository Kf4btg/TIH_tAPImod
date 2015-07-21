using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    // TODO: transfer over documentation later
    public abstract class ButtonContainerLayer : InterfaceLayer
    {
        protected bool handleMouseInterface;
        protected float opacity_inactive, opacity_active = 1.0f;

        public readonly Dictionary<string,CoreButton> Buttons;
        public readonly Dictionary<TIH, ButtonSocket<CoreButton>> ButtonBases;

        public Rectangle ButtonFrame { get; protected set; }
        public virtual float LayerOpacity { get; protected set; }

        public bool IsHovered
        {
            get { return ButtonFrame.Contains(Main.mouse); }
        }

        protected ButtonContainerLayer(string name, bool handle_mouse_interface = true) : base(IHBase.Instance.mod.InternalName + ":" + name)
        {
            ButtonBases = new Dictionary<TIH, ButtonSocket<CoreButton>>();
            Buttons     = new Dictionary<string, CoreButton>();
            ButtonFrame = Rectangle.Empty;
            handleMouseInterface = handle_mouse_interface;
        }

        public void Initialize()
        {
            AddBasesToLayer();
            AddButtonsToBases();
            UpdateFrame();
        }

        internal void UpdateFrame()
        {
            LayerOpacity = opacity_inactive;
            foreach (var kvp in ButtonBases)
            {
                ButtonFrame = (ButtonFrame.IsEmpty) ? kvp.Value.ButtonBounds : Rectangle.Union(ButtonFrame, kvp.Value.ButtonBounds);
            }
        }

        protected virtual void DrawButtons(SpriteBatch sb)
        {
            // KeyValuePair<TIH, ButtonBase>
            foreach (var kvp in ButtonBases)
            {
                kvp.Value.Draw(sb);
            }
        }

        protected override void OnDraw(SpriteBatch sb)
        {
            if (!parentLayer.visible) return;

            if (handleMouseInterface && IsHovered)
                Main.localPlayer.mouseInterface = true;
            DrawButtons(sb);
        }


        // Abstract Methods //

        protected abstract void AddBasesToLayer();

        protected abstract void AddButtonsToBases();
    }


    /// this Class will fade ALL buttons in the loyer in/out as the
    /// LAYER itself is hovered/unhovered;
    public abstract class FadingButtonLayer : ButtonContainerLayer
    {
        protected readonly float fadeStep;

        private float _opacity;
        public override float LayerOpacity
        {
            get { return _opacity; }
            protected set { _opacity = value.Clamp(opacity_inactive, opacity_active ); }
        }

        /// fade step of 0 will cause instant opacity change;
        protected FadingButtonLayer(string name, float min_opacity, float max_opacity, float fade_step = 0, bool handle_mouse_interface = true) : base(name, handle_mouse_interface)
        {
            opacity_inactive = min_opacity.Clamp();
            opacity_active   = max_opacity.Clamp();
            fadeStep = fade_step == 0 ? opacity_active - opacity_inactive : fade_step;
        }

        protected override void OnDraw(SpriteBatch sb)
        {
            if (!parentLayer.visible) return;

            if (IsHovered)
            {
                if (handleMouseInterface)
                    Main.localPlayer.mouseInterface = true;
                if (LayerOpacity != opacity_active) LayerOpacity += fadeStep;
            }
            else
                if (LayerOpacity != opacity_inactive) LayerOpacity -= fadeStep;
            DrawButtons(sb);
        }
    }
}
