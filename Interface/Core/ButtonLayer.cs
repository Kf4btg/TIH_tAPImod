using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    // TODO: transfer over documentation later
    public abstract class ButtonLayer : InterfaceLayer
    {
        protected bool handleMouseInterface;
        protected float opacity_inactive = 1.0f, opacity_active = 1.0f;

        public readonly Dictionary<string,ICoreButton> Buttons;
        public readonly Dictionary<TIH, IButtonSlot> ButtonBases;

        public Rectangle ButtonFrame { get; protected set; }
        public virtual float LayerOpacity { get; protected set; }

        public bool IsHovered
        {
            get { return ButtonFrame.Contains(Main.mouse); }
        }

        protected ButtonLayer(string name, bool handle_mouse_interface = true) : base(IHBase.Instance.mod.InternalName + ":" + name)
        {
            ButtonBases = new Dictionary<TIH, IButtonSlot>();
            Buttons     = new Dictionary<string, ICoreButton>();
            ButtonFrame = Rectangle.Empty;
            handleMouseInterface = handle_mouse_interface;

            LayerOpacity = opacity_inactive;
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

        // receive button from base //
        public void AddButton(ICoreButton b)
        {
            Buttons[b.ID] = b;
            //bubble up
            IHBase.Instance.ButtonStore.Add(b.ID, b);
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
    public abstract class FadingButtonLayer : ButtonLayer
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

            LayerOpacity = opacity_inactive;
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
