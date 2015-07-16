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
    public abstract class ButtonRebase<T> where T:CoreButton
    {
        /// interface layer this button belongs to
        public readonly ButtonLayer parentLayer;
        // public IButtonDrawHandler Drawer;  //just subclass
        public Vector2 Position { get; protected set; }

        public virtual T DefaultContent { get; protected set; }
        public virtual T CurrentContent { get; protected set; }


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

        protected float _min_scale = 0.5f;
        protected float _max_scale = 1.0f;
        protected float _scale = 1.0f;
        public virtual float Scale
        {
            get { return _scale; }
            set { _scale = clamp(value, _min_scale, _max_scale); }
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
            set { _alpha = clamp(value, BaseAlpha); }
        }

        public ButtonRebase(ButtonLayer parent, T content, Vector2 position)
        {
            parentLayer = parent;
            // Drawer = drawer;
            this.DefaultContent = this.CurrentContent = content;

            ButtonBounds = new Rectangle((int)position.X, (int)position.Y, (int)content.Size.X, (int)content.Size.Y);
            // this.IsHovered = defaultHoverCheck;
        }

        /// switch to a new button content
        public void ChangeContent(T newContent)
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
                OnDrawBase(sb);
            }
            CurrentContent.PostDraw(sb);
        }

        /// handles hover-check, hover events, etc.
        /// subclass and override this to change these aspects.
        protected virtual void OnDrawBase(SpriteBatch sb)
        {
            if (Hovered)
            {
                if (!HasMouseFocus)
                {
                    HasMouseFocus = true;
                    OnMouseEnter();
                }
                WhenHovered();
                HandleClicks();
                return;
            }
            if (HasMouseFocus) OnMouseLeave();
            HasMouseFocus = false;
            WhenNotHovered();
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
        protected virtual void HandleClicks()
        {
            if (Main.mouseLeft && Main.mouseLeftRelease)
                CurrentContent.OnClick();

            if (Main.mouseRight && Main.mouseRightRelease)
                CurrentContent.OnRightClick();
        }

        /// Should handle the actual SpriteBatch command which draws the button
        protected abstract void DrawButtonContent(SpriteBatch sb);

        /// hook for derived classes to add extra functionality
        /// to OnDrawBase without having to reimplement the
        /// whole method.
        protected virtual void WhenHovered() {}

        protected virtual void WhenNotHovered() {}


        ///<returns>given value bound by specified minumum and maximum
        /// values (inclusive)</returns>
        protected float clamp(float value, float min = 0, float max = 1)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// allows registering key toggle w/ just the context (button) IDs
        // public void RegisterKeyToggle(KState.Special key, string context1ID, string context2ID)
        // {
        //     RegisterKeyToggle(key, IHBase.Instance.ButtonRepo[context1ID], IHBase.Instance.ButtonRepo[context2ID]);
        // }

        /// register a key toggle for this base's default context
        public void RegisterKeyToggle(KState.Special key, T context2)
        {
            RegisterKeyToggle(key, this.DefaultContent, context2);
        }

        /// set up key-event-subscribers that will toggle btw 2 contexts
        public void RegisterKeyToggle(KState.Special key, T context1, T context2)
        {
            //have to initialize (rather than just declare) this to prevent compile-time error in kw1 declaration
            var kw2 = new KeyWatcher(KState.Special.Shift, KeyEventProvider.Event.Released, null);

            var kw1 = new KeyWatcher(key, KeyEventProvider.Event.Pressed,
            () => {
                ChangeContent(context2);
                kw2.Subscribe();
                } );

            // assign kw2 onkeyevent
            kw2.OnKeyEvent = () => {
                ChangeContent(context1);
                kw1.Subscribe();
                };

            // subscribe to default watcher
            kw1.Subscribe();
        }
    }

    // ------------------------------------------------------------
    // some subclasses; maybe put these in separate file?

    public class IconButtonBase : ButtonRebase<TexturedButton>
    {

        /// this will actively set the Source Texels based on whether or not the mouse is currently over this button.
        /// If both rects are null, then the entire texture will be drawn as per default
        public Rectangle? SourceRect
        {
            get { return HasMouseFocus ?
                            CurrentContent.ActiveRect :
                            CurrentContent.InactiveRect;
                }
        }

        public IconButtonBase(ButtonLayer parent, TexturedButton content, Vector2 position) : base(parent, content, position)
        {

        }

        protected override void DrawButtonContent(SpriteBatch sb)
        {
            sb.Draw(CurrentContent.Texture,
                    Position,
                    SourceRect,
                    CurrentContent.Tint*Alpha,
                    0f,
                    default(Vector2),
                    Scale,
                    SpriteEffects.None,
                    0f);
        }

    }

    public class TextButtonBase : ButtonRebase<TextButton>
    {

        /// Get the (relative) center of the full-size button
        private Vector2 origin
        {
            get { return CurrentContent.Size / 2; }
        }
        /// shift origin up-right or down-left as button is scaled
        private Vector2 scaledOrigin
        {
            get { return origin * Scale; }
        }

        ///doing this enables the "pulse" effect all the vanilla text has
        private Color textColor
        {
            get { return Main.mouseTextColor.toScaledColor(Scale); }
        }

        /// modified position used in scaling/hover calculations
        private Vector2 posMod;

        /// makes the button smoothly grow and shrink as the mouse moves on and off
        private readonly float scaleStep;
        public TextButtonBase(ButtonLayer parent, TextButton content, Vector2 position,
                                float base_scale = 0.75f,
                                float focus_scale = 1.0f,
                                float scale_step = 0.05f ) : base(parent, content, position)
        {
            posMod = Position;
            _min_scale = base_scale;
            _max_scale = focus_scale;
            scaleStep = scale_step;
        }

        public override bool IsHovered(Vector2 mouse)
        {
            var o = scaledOrigin; //cache it
            return (float)mouse.X > (float)posMod.X - o.X &&
                    (float)mouse.X < (float)posMod.X + o.X &&
                    (float)mouse.Y > (float)posMod.Y - o.Y &&
                    (float)mouse.Y < (float)posMod.Y + o.Y;
        }

        protected override void DrawButtonContent(SpriteBatch sb)
        {
            // var textColor = Main.mouseTextColor.toScaledColor(Scale, CurrentState.tint);

            posMod = Position; //reset
            posMod.X += (int)(origin.X * Scale);

            sb.DrawString(
                Main.fontMouseText,        //font
                CurrentContent.Label,        //string
                new Vector2(posMod.X, posMod.Y), //position
                textColor,                 //color
                0f,                        //rotation
                origin,
                Scale,
                SpriteEffects.None,        //effects
                0f                         //layerDepth
            );
        }

        /// Handle mouseInterface, Scale up
        protected override void WhenHovered()
        {
            // handling mouseInterface individually rather than by
            // the ButtonFrame so that the buttons will act like the
            // vanilla versions.
            Main.localPlayer.mouseInterface = true;
            if (Scale!=_max_scale)
                Scale += scaleStep;
        }

        /// Scale down
        protected override void WhenNotHovered()
        {
            if (Scale!=_min_scale)
                Scale -= scaleStep;
        }

    }
}
