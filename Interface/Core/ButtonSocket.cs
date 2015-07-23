using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public interface IButtonSlot
    {
        ButtonLayer ParentLayer { get; }
        Vector2 Position { get; }
        Vector2 Size { get; }
        Rectangle ButtonBounds { get; }

        bool IsHovered { get; }
        float Scale { get; set; }
        float Alpha { get; set; }

        void RegisterKeyToggle(KState.Special key, string button_id);
        void RegisterKeyToggle(KState.Special key, string button_a_id, string button_b_id);

        void ChangeContent(String button_id);
        void Reset();

        void Draw(SpriteBatch sb);

    }

    public interface IButtonSocket<T> : IButtonSlot where T: ICoreButton
    {
        T CurrentContent { get; }
        T DefaultContent { get; }

        void ChangeContent(T new_content);

        // void RegisterKeyToggle(KState.Special key, T context2);
        // void RegisterKeyToggle(KState.Special key, T context1, T context2);
        // void SetDefault(T default_content);
        void AddButton(T newButton);

    }


    /// Implementations need to override at least DrawButtonContent;
    /// everything else has a default impl. to use if applicable
    /// Scale and Alpha properties are present, but aren't used by default
    public abstract class ButtonSocket<T> : IButtonSocket<T> where T: ICoreButton
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


        public Dictionary<string, T> AssociatedButtons { get; protected set; }

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
        protected ButtonSocket(ButtonLayer parent, Vector2 position)
        {
            ParentLayer = parent;
            Position = position;

            AssociatedButtons = new Dictionary<string, T>();
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

        public void ChangeContent(string new_content_id)
        {
            ChangeContent(AssociatedButtons[new_content_id]);
        }

        /// <summary>
        /// return this Socket to its default configuration
        /// </summary>
        public void Reset()
        {
            ChangeContent(DefaultContent);
        }


        /// <summary>
        /// register a key toggle for this base's default context
        /// </summary>
        /// <param name="key">Activation key, e.g. Shift</param>
        /// <param name="button_id">ID of button to swap with</param>
        public void RegisterKeyToggle(KState.Special key, string button_id)
        {
            RegisterKeyToggle(key, DefaultContent, AssociatedButtons[button_id]);
        }

        /// <summary>
        /// Set up key-event-subscribers that will toggle between the 2 contexts
        /// when the player holds or releases the button.
        /// </summary>
        /// <param name="key">Key (ctrl | shift | alt) on which to toggle content</param>
        /// <param name="button_a_id">ID of default button to display</param>
        /// <param name="button_b_id">ID of button displayed while <paramref name="key "/> is held down.</param>
        public void RegisterKeyToggle(KState.Special key, string button_a_id, string button_b_id)
        {
            RegisterKeyToggle(key, AssociatedButtons[button_a_id], AssociatedButtons[button_b_id]);
        }

        /// <summary>
        /// register a key toggle for this base's default context
        /// </summary>
        /// <param name="key">Activation key, e.g. Shift</param>
        /// <param name="context2">Button to swap with</param>
        public void RegisterKeyToggle(KState.Special key, T context2)
        {
            RegisterKeyToggle(key, DefaultContent, context2);
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
            var kw2 = new KeyWatcher(key, KeyEventProvider.Event.Released, null);

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
            // FIXME: it occurs to me that there might currently be no
            // way to Un-register this key-toggle
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
        public void SetDefault(T default_content)
        {
            AssociatedButtons[default_content.ID] = default_content;
            DefaultContent = CurrentContent = default_content;

            ButtonBounds = new Rectangle((int)Position.X, (int)Position.Y, (int)default_content.Size.X, (int)default_content.Size.Y);

            // return this;
        }

        /// Associate a new button with this base.
        /// If no other buttons have previously been associated,
        /// make it the default content.
        public void AddButton(T button)
        {
            if (AssociatedButtons.Count == 0)
                SetDefault(button);
            else
                AssociatedButtons[button.ID] = button;

            // bubble up the stack
            ParentLayer.AddButton(button);
        }

        /// Associate multiple buttons with this base.
        /// The first button in the parameter list becomes
        /// the default content if no other buttons have
        /// yet been added.
        public void AddButtons(params T[] buttons)
        {
            foreach (T button in buttons)
                AddButton(button);
        }

        public void RemoveButtonAssociation(string buttonID)
        {
            AssociatedButtons.Remove(buttonID);
        }

        //virtually abstract methods (no default implementation)

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

}
