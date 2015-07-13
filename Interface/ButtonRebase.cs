using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{

    public class ButtonRebase
    {

        /// interface layer this button belongs to
        public readonly ButtonLayer parentLayer;
        public IButtonDrawHandler Drawer;
        protected Vector2 Position;

        public readonly CoreButton defaultContent;
        public CoreButton currentContent { get; protected set;}

        // public bool IsHovered { get; protected set; }

        public virtual bool HasMouseFocus { get; set; }

        public Rectangle ButtonBounds { get; protected set; }

        public virtual Vector2 Size { get { return ButtonBounds.Size() * this.Scale; } }

        public const float SCALE_FULL = 1.0f;
        protected float _scale = SCALE_FULL;
        public virtual float Scale {get { return _scale; } set { _scale = clamp(value, 0.5f, SCALE_FULL); } }


        protected float _baseAlpha = 0.85f;
        public float baseAlpha { get { return _baseAlpha; } protected set { _baseAlpha = clamp(value); } }
        protected float _alpha;
        /// no longer including the parent container opacity in the return value
        public virtual float Alpha { get { return _alpha; } set {  _alpha = clamp(value, this.baseAlpha); }}

        public Func<Vector2, bool> IsHovered;

        // public ButtonRebase(ButtonLayer container, IHButton defaultContext, float base_alpha = 0.85f, float alpha_step = 0.01f)
        //     : base(container, defaultContext, base_alpha, alpha_step)
        // {
        // }

        public ButtonRebase(ButtonLayer parent, CoreButton content, Vector2 position, IButtonDrawHandler drawer)
        {
            parentLayer = parent;
            Drawer = drawer;
            this.defaultContent = this.currentContent = content;

            ButtonBounds = new Rectangle((int)position.X, (int)position.Y, (int)content.Size.X, (int)content.Size.Y);
            this.IsHovered = defaultHover;

        }



        public virtual void Draw(SpriteBatch sb)
        {
            if (currentContent.PreDraw(sb))
            {
                // current.Draw();
                // sb.DrawIHButton(this, currentContext.DisplayState);
                Drawer.Draw(sb, this);
            }
        }


        protected virtual bool defaultHover(Vector2 mouse)
        {
            var size = this.Size;
            return new Rectangle((int)Position.X, (int)Position.Y, (int)size.X, (int)size.Y).Contains(mouse);
        }

        // IDEA: can we affect the reported mouse position instead of recalculating
        // the size and position of the button? that's probably silly and would be
        // way more complicated
        protected virtual bool otherHover(Vector2 mouse, Vector2 offset, Vector2 origin)
        {
            var pos = this.Position + offset;
            return true;
        }


        public virtual void OnMouseEnter(SpriteBatch sb)
        {
            if (currentContent.OnMouseEnter(sb))
                Sound.MouseOver.Play();
        }

        public virtual void OnHover()
        {
            if (Main.mouseLeft && Main.mouseLeftRelease)
                currentContent.OnClick();

            if (Main.mouseRight && Main.mouseRightRelease)
                currentContent.OnRightClick();
        }


        /// this is intended to allow services to target aspects of the
        /// Button-drawing process that can't or shouldn't be handled
        /// on the core-button level; pretty much anything that shouldn't
        /// be affected by this base switching CoreButtons in the middle
        /// of the operation. This could include things like fading,
        /// scaling, or modifying the "hovered" area.
        public class BaseHooks
        {
            public Func<bool> onCheckIsHovered;
            public Func<bool> onHover;
        }

        protected float clamp(float value, float min = 0, float max = 1)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

    }


}
