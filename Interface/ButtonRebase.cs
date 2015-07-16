using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    /// Implementations need to override at least DrawButtonContent;
    /// everything else has a default impl. to use if applicable
    /// Scale and Alpha properties are present, but aren't used by default
    public abstract class ButtonRebase
    {
        /// interface layer this button belongs to
        public readonly ButtonLayer parentLayer;
        // public IButtonDrawHandler Drawer;  //just subclass
        public Vector2 Position { get; protected set; }

        public readonly CoreButton DefaultContent;
        public CoreButton CurrentContent { get; protected set;}


        public Rectangle ButtonBounds { get; protected set; }

        public virtual Vector2 Size
        {
            get { return ButtonBounds.Size() * this.Scale; }
        }

        public virtual bool Hovered
        {
            get { return IsHovered(Main.mouse); }
        }
        public virtual bool HasMouseFocus { get; set; }

        public const float SCALE_FULL = 1.0f;
        protected float _scale = SCALE_FULL;
        public virtual float Scale
        {
            get { return _scale; }
            set { _scale = clamp(value, 0.5f, SCALE_FULL); }
        }

        protected float _baseAlpha = 0.85f;
        /// This is the minimum Alpha value this ButtonBase can achieve,
        /// not accounting for the opacity of its parent layer.
        public float BaseAlpha
        {
            get { return _baseAlpha; }
            set { _baseAlpha = clamp(value); }
        }

        protected float _alpha = 1.0f;
        // no longer including the parent container opacity in the return value.
        /// Get current alpha value or set alpha to the given value (constrained
        /// by the value of BaseAlpha)
        public virtual float Alpha
        {
            get { return _alpha; }
            set {  _alpha = clamp(value, BaseAlpha); }
        }

        public ButtonRebase(ButtonLayer parent, CoreButton content, Vector2 position) //, IButtonDrawHandler drawer)
        {
            parentLayer = parent;
            // Drawer = drawer;
            this.DefaultContent = this.CurrentContent = content;

            ButtonBounds = new Rectangle((int)position.X, (int)position.Y, (int)content.Size.X, (int)content.Size.Y);
            // this.IsHovered = defaultHoverCheck;
        }

        /// switch to a new button content
        public void ChangeContent(CoreButton newContent)
        {
            CurrentContent = newContent;
        }
        /// return this ButtonBase to its default context
        public void Reset()
        {
            ChangeContent(DefaultContent);
        }

        public void Draw(SpriteBatch sb)
        {
            if (CurrentContent.PreDraw(sb))
            {
                DrawButtonContent(sb);
                DrawBase(sb);
            }
            CurrentContent.PostDraw(sb);
        }

        /// handles hover-check, hover events, etc.
        /// subclass and override this to change these aspects.
        protected virtual void DrawBase(SpriteBatch sb)
        {
            if (Hovered)
            {
                if (!HasMouseFocus)
                {
                    HasMouseFocus=true;
                    OnMouseEnter();
                }

                HandleClicks();
                return;
            }
            if (HasMouseFocus) OnMouseLeave();
            HasMouseFocus=false;
        }

        public virtual bool IsHovered(Vector2 mouse)
        {
            // this.Size takes current scale into account
            // (ButtonBounds defines normal size)
            var s = this.Size;
            return new Rectangle((int)Position.X, (int)Position.Y, (int)s.X, (int)s.Y).Contains(mouse);
        }

        // protected virtual bool otherHover(Vector2 mouse, Vector2 offset, Vector2 origin)
        // {
        //     var pos = this.Position + offset;
        //     return true;
        // }

        public virtual void OnMouseEnter()
        {
            if (CurrentContent.OnMouseEnter())
                Sound.MouseOver.Play();
        }
        public virtual void OnMouseLeave()
        {
            // no checking return value because...we don't do anything here
            CurrentContent.OnMouseLeave();
        }

        /// Checks for both left and right clicks
        public virtual void HandleClicks()
        {
            if (Main.mouseLeft && Main.mouseLeftRelease)
                CurrentContent.OnClick();

            if (Main.mouseRight && Main.mouseRightRelease)
                CurrentContent.OnRightClick();
        }

        /// Should handle the actual SpriteBatch command which draws the button
        protected abstract void DrawButtonContent(SpriteBatch sb);

        ///<returns>given value bound by specified minumum and maximum
        /// values (inclusive)</returns>
        protected float clamp(float value, float min = 0, float max = 1)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
