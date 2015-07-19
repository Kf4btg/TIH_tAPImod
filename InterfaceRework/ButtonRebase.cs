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
    public abstract class ButtonSocket<T> where T:CoreButton
    {
        //backing stores & default values
        protected float _minScale = 0.5f;
        protected float _maxScale = 1.0f;
        protected float _scale = 1.0f;
        protected float _baseAlpha = 0.85f;
        protected float _alpha = 1.0f;

        /// interface layer this button belongs to
        public ButtonLayer ParentLayer { get; protected set; }

        /// Get this button's screem coordinates
        public Vector2 Position { get; protected set; }
        /// Get the bounding rectangle for this socket.
        public Rectangle ButtonBounds { get; protected set; }

        /// Get an indication of whether this button is currently
        /// focused by the mouse. Differs from IsHovered in that
        /// this is a switch which is set when the mouse first enters
        /// or leaves the button, rather than calculating the
        /// hover status on each call. Used to activate OnMouseEnter/Leave,
        /// and is faster to call from an external class.
        public bool HasMouseFocus { get; protected set; }

        /// Get whether or not this button is currently hovered by the mouse
        public bool IsHovered
        {
            get { return GetIsHovered(Main.mouse); }
        }

        /// Get or set current scaling factor of this button
        public float Scale
        {
            get { return _scale; }
            set { _scale = value.Clamp(_minScale, _maxScale); }
        }

        /// Get or set the minimum Alpha value this ButtonBase can achieve,
        /// not accounting for the opacity of its parent layer.
        public float BaseAlpha
        {
            get { return _baseAlpha; }
            set { _baseAlpha = value.Clamp(); }
        }

        /// Get current alpha value or set alpha to the given value (constrained
        /// by the value of BaseAlpha)
        public float Alpha
        {
            // no longer including the parent container opacity in the return value.
            get { return _alpha; }
            set { _alpha = value.Clamp(BaseAlpha); }
        }

        //virtual properties//

        ///Get the original button for this socket;
        ///can return socket to this configuration with Reset()
        public virtual T DefaultContent { get; protected set; }

        /// Get the currently socketed button; at button creation,
        /// this is the same as DefaultContent.
        public virtual T CurrentContent { get; protected set; }

        /// Get the unscaled size of this button
        public virtual Vector2 Size
        {
            get { return ButtonBounds.Size(); }
        }

        //Constructor//

        /// initialize a new, empty socket with blank content;
        /// calls the derived-class specific version of InitEmptySocket()
        public ButtonSocket(ButtonLayer parent, Vector2 position)
        {
            ParentLayer = parent;
            Position = position;

            InitEmptySocket();
        }

        // public ButtonSocket(ButtonLayer parent, T content, Vector2 position)
        // {
        //     parentLayer = parent;
        //     this.DefaultContent = this.CurrentContent = content;
        //
        //     ButtonBounds = new Rectangle((int)position.X, (int)position.Y, (int)content.Size.X, (int)content.Size.Y);
        // }


        /// <summary>
        /// Replace current button configuration
        /// </summary>
        /// <param name="new_content">The button to swap into this socket</param>
        public void ChangeContent(T new_content)
        {
            CurrentContent = new_content;
        }
        /// <summary>
        /// return this Socket to its default configuration
        /// </summary>
        public void Reset()
        {
            ChangeContent(DefaultContent);
        }

        // allows registering key toggle w/ just the context (button) IDs
        // public void RegisterKeyToggle(KState.Special key, string context1ID, string context2ID)
        // {
        //     RegisterKeyToggle(key, IHBase.Instance.ButtonRepo[context1ID], IHBase.Instance.ButtonRepo[context2ID]);
        // }

        /// <summary>
        /// register a key toggle for this base's default context
        /// </summary>
        /// <param name="key">Activation key, e.g. Shift</param>
        /// <param name="context2">Corebutton to swap with</param>
        public void RegisterKeyToggle(KState.Special key, T context2)
        {
            RegisterKeyToggle(key, this.DefaultContent, context2);
        }

        /// <summary>
        /// Set up key-event-subscribers that will toggle between the 2 contexts
        /// when the player holds or releases the button.
        /// </summary>
        /// <param name="key">Key (ctrl | shift | alt) on which to toggle content</param>
        /// <param name="context1">Default button displayed</param>
        /// <param name="context2">Button displayed while <paramref name="key "/> is held down.</param>
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

        /// <summary>
        /// Called by ButtonLayer each frame.
        /// </summary>
        /// <param name="sb">Spritebatch passed down from containing layer</param>
        public void Draw(SpriteBatch sb)
        {
            if (CurrentContent.PreDraw(sb))
            {
                DrawButtonContent(sb);
                OnDrawBase();
            }
            CurrentContent.PostDraw(sb);
        }

        #region virtual methods

        /// <summary>
        /// Handles hover-check, hover events, etc.
        /// </summary><remarks>
        /// Most of these events can be changed individually
        /// by overriding their respective hooks, but, for more
        /// fine-grained control, you can subclass and override
        /// this entire method. Be careful to make sure all the
        /// necessary hooks are called from the derived version.</remarks>
        protected virtual void OnDrawBase()
        {
            if (IsHovered)
            {
                if (!HasMouseFocus)
                {
                    HasMouseFocus = true;
                    OnMouseEnter();
                }
                WhenFocused();
                HandleClicks();
                return;
            }
            if (HasMouseFocus) OnMouseLeave();
            HasMouseFocus = false;
            WhenNotFocused();
        }

        /// <summary>
        /// Determines whether the mouse is hovered over this button's
        /// screen-area, accounting for the button's current scale.
        /// </summary>
        /// <param name="mouse">Current position of mouse cursor</param>
        /// <returns>True if mouse currently over button</returns>
        protected virtual bool GetIsHovered(Vector2 mouse)
        {
            // get Size taking current scale into account
            var s = this.Size * Scale;
            return new Rectangle((int)Position.X, (int)Position.Y, (int)s.X, (int)s.Y).Contains(mouse);
        }

        /// <summary>
        /// Called when the mouse first enters this button's space.
        /// Also calls the OnMouseEnter hook of the currently socketed button.
        /// </summary>
        protected virtual void OnMouseEnter()
        {
            if (CurrentContent.OnMouseEnter())
                Sound.MouseOver.Play();
        }
        /// <summary>
        /// Called when the mouse leaves this button's space.
        /// Also calls the OnMouseLeave hook of the currently socketed button.
        /// </summary>
        protected virtual void OnMouseLeave()
        {
            // no checking return value because...we don't do anything here
            CurrentContent.OnMouseLeave();
        }

        /// <summary>
        /// Checks for both left and right clicks and calls
        /// the associated button hook if detected.
        /// </summary>
        protected virtual void HandleClicks()
        {
            if (Main.mouseLeft && Main.mouseLeftRelease)
                CurrentContent.OnClick();

            if (Main.mouseRight && Main.mouseRightRelease)
                CurrentContent.OnRightClick();
        }

        /// <summary>
        /// Make this button usable by adding a default button configuration
        /// </summary>
        /// <param name="default_content">Default button</param>
        public virtual void SetupDefault(T default_content)
        {
            this.DefaultContent = this.CurrentContent = default_content;

            ButtonBounds = new Rectangle((int)Position.X, (int)Position.Y, (int)default_content.Size.X, (int)default_content.Size.Y);
        }

        //virtually abstract methods (no default implementation)

        /// <summary>
        /// Setup this socket in a stable but uninitialized manner
        /// </summary>
        protected virtual void InitEmptySocket() {}

        ///<summary>
        /// Called every frame while the mouse is hovered over this button
        ///</summary>
        protected virtual void WhenFocused() {}

        ///<summary>
        /// Called during every frame that the button is visible
        /// but does not have direct mouse focus.
        ///</summary>
        protected virtual void WhenNotFocused() {}

        #endregion

        //abstract methods//

        /// <summary>
        /// Should handle the actual SpriteBatch command which draws the button
        /// </summary>
        /// <param name="sb">Spritebatch which performs the drawing</param>
        protected abstract void DrawButtonContent(SpriteBatch sb);
    }

    // ------------------------------------------------------------
    // some subclasses; maybe put these in separate file?

    public class IconButtonBase : ButtonSocket<TexturedButton>
    {

        /// Texture resource that will be drawn in the background of this buttonbase.
        /// BgColor property of current button content object is used for tint.
        public Texture2D ButtonBackground { get; set; }

        /// this will actively set the Source Texels based on whether or not the mouse is currently over this button.
        /// If both rects are null, then the entire texture will be drawn as per default
        public Rectangle? SourceRect
        {
            get { return HasMouseFocus ?
                            CurrentContent.ActiveRect :
                            CurrentContent.InactiveRect;
                }
        }



        public IconButtonBase(ButtonLayer parent, TexturedButton content, Vector2 position, Texture2D button_bg ) : base(parent, position)
        {
            ButtonBackground = button_bg;
            base.SetupDefault(content);
        }

        protected override void InitEmptySocket()
        {
            ButtonBackground = IHBase.ButtonBG;
            DefaultContent = CurrentContent = new TexturedButton();
        }

        protected override void DrawButtonContent(SpriteBatch sb)
        {
            var opacity = ParentLayer.LayerOpacity*Alpha;
            //draw button background first
            // (otherwise button content will be below bg!)
            sb.Draw(ButtonBackground,
                    Position,
                    null,
                    CurrentContent.BackgroundColor*opacity,
                    0f,
                    default(Vector2),
                    Scale,
                    SpriteEffects.None,
                    0f);

            // and now the real button stuff
            sb.Draw(CurrentContent.Texture,
                    Position,
                    SourceRect,
                    CurrentContent.Tint*opacity,
                    0f,
                    default(Vector2),
                    Scale,
                    SpriteEffects.None,
                    0f);
        }

    }

    public class TextButtonBase : ButtonSocket<TextButton>
    {

        //class fields//

        /// modified position used in scaling/hover calculations
        private Vector2 posMod;

        /// makes the button smoothly grow and shrink as the mouse moves on and off
        private float scaleStep = float.MaxValue;

        //Properties//

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

        /// get current color of the text
        public Color TextColor
        {
            // basing textColor on Main.mouseTextColor enables
            // the "pulse" effect all the vanilla text has;
            // Combining it with the current scale makes the
            // intensity of the text color fade up and down as the
            // button zooms in or out.
            get { return Main.mouseTextColor.toScaledColor(Scale); }
        }



        ///<summary>
        /// Create an empty socket at the given position</summary>
        public TextButtonBase
        (   ButtonLayer parent, Vector2 position,
            float base_scale = 0.75f,
            float focus_scale = 1.0f,
            float scale_step = 0.05f
        ) : base(parent, position)
        {
            posMod = Position;
            // 30 is honestly kind of a ridiculously high limit;
            // I don't think I saw any text-scaling values go higher
            // than 4 in the vanilla code.
            _minScale = base_scale.Clamp(0.5f, 30.0f);
            _maxScale = focus_scale.Clamp(_minScale, 30.0f);


            scaleStep = (_minScale == _maxScale) ? 0 :
             scale_step;
        }

        public TextButtonBase
        (   ButtonLayer parent, TextButton content, Vector2 position,
            float base_scale = 0.75f, float focus_scale = 1.0f, float scale_step = 0.05f
        ) : this(parent, position, base_scale, focus_scale, scale_step)
        {
            base.SetupDefault(content);
        }

        protected override bool GetIsHovered(Vector2 mouse)
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
                TextColor,                 //color
                0f,                        //rotation
                origin,
                Scale,
                SpriteEffects.None,        //effects
                0f                         //layerDepth
            );
        }

        /// Handle mouseInterface, Scale up
        protected override void WhenFocused()
        {
            // handling mouseInterface individually rather than by
            // the ButtonFrame so that the buttons will act like the
            // vanilla versions.
            Main.localPlayer.mouseInterface = true;
            if (Scale!=_maxScale)
                Scale += scaleStep;
        }

        /// Scale down
        protected override void WhenNotFocused()
        {
            if (Scale!=_minScale)
                Scale -= scaleStep;
        }

    }
}
