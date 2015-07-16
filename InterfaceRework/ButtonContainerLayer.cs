using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    //TODO: transfer over documentation later
    public abstract class ButtonContainerLayer : InterfaceLayer
    {
        public readonly Dictionary<TIH, ButtonRebase<CoreButton>> Buttons;

        public Rectangle ButtonFrame { get; protected set;}
        public bool IsHovered {
            get {
                return ButtonFrame.Contains(Main.mouse);
            }
        }

        public bool handleMouseInterface;

        protected float opacity_inactive, opacity_active = 1.0f;

        //TODO: have this fade in/out?
        public virtual float LayerOpacity { get; protected set; }

        protected ButtonContainerLayer(string name, bool handleMouseInterface = true) : base(IHBase.Instance.mod.InternalName + ":" + name)
        {
            Buttons = new Dictionary<TIH, ButtonRebase<CoreButton>>();
            ButtonFrame = Rectangle.Empty;
            this.handleMouseInterface = handleMouseInterface;
        }

        internal void UpdateFrame()
        {
            LayerOpacity = opacity_inactive;
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

            if (handleMouseInterface && IsHovered)
                    Main.localPlayer.mouseInterface = true;
            DrawButtons(sb);
        }
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
        protected FadingButtonLayer(string name, float min_opacity, float max_opacity, float fade_step = 0, bool handleMouseInterface = true) : base(name, handleMouseInterface)
        {
            opacity_inactive = min_opacity.Clamp();
            opacity_active = max_opacity.Clamp();
            fadeStep = fade_step == 0 ? opacity_active - opacity_inactive : fade_step;
        }

        protected override void OnDraw(SpriteBatch sb)
        {
            if (!parentLayer.visible) return;

            if (IsHovered)
            {
                if (handleMouseInterface)
                    Main.localPlayer.mouseInterface = true;
                if (LayerOpacity!=opacity_active) LayerOpacity += fadeStep;
            }
            else
                if (LayerOpacity!=opacity_inactive) LayerOpacity -= fadeStep;
            DrawButtons(sb);
        }


    }
}
