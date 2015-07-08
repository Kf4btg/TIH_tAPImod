using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
// using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    /**
        Conceptually, this can be thought of as the "actual" button: it defines the location
        on the screen where the button appears and how much space it takes up. This is the
        object that will receive mouse clicks, which will then be passed on to its members.

        Technically, though, this is more of a button "frame" or "platform."  It contains
        ButtonContexts (which define what the button _can_ do) that in turn contain
        ButtonStates (which define what the button _actually_ does).  All of these
        can be swapped out and around to create some fairly dynamic interactivity.

        TODO: stop duplicating all the code from UIComponent et al...
        TODO 2: what code was that, exactly?
    */
    public class ButtonBase
    {
        /// a _unique_ name that can identify this button
        public readonly string Name;

        ///interface layer this button belongs to
        public readonly ButtonLayer Container;

        /// this defines the location and size of this button.
        /// even if other buttons have differently-sized textures/strings,
        /// the button will not move from the coordinates defined here.
        public readonly IHButton DefaultContext;

        // defines the current appearance and functionality of the button
        private IHButton currentContext;
        public IHButton CurrentContext { get { return currentContext; } }
        public ButtonState CurrentState { get { return currentContext.DisplayState; } }

        // will be set at construction
        public readonly Vector2 Position;
        public readonly Vector2 Size;
        public readonly Func<Rectangle> CurrentBounds;
        public readonly Rectangle MaxBounds;
        public bool IsHovered {
             get { return CurrentBounds().Contains(Main.mouseX, Main.mouseY); }
        }

        /// for determining when the mouse moves on and off the button
        private bool hasMouseFocus;

        private bool useScaleEffect = false;

        public const float SCALE_FULL = 1.0f;
        private float scale = ButtonBase.SCALE_FULL;
        private readonly float baseScale;
        private readonly float hoverScale;
        private readonly float scaleStep;
        public float Scale {
            get { return scale; }
            set {
                scale = value < baseScale ? baseScale :
                            value > hoverScale ? hoverScale : value;
            } }

        // affects the alpha component of the current tint
        private float alphaBase = 0.85f;
        private float alphaMult = 0.85f;
        /// gets current alpha (modified by container opacity), sets current and base alpha values
        public float Alpha { get { return alphaMult*Container.LayerOpacity; } set { alphaMult = alphaBase = value; }}

        /// this will actively set the Source Texels based on whether or not the mouse is currently over this button.
        /// If both rects are null, then the entire texture will be drawn as per default
        public Rectangle? SourceRect { get { return hasMouseFocus ? currentContext.ActiveRect : currentContext.InactiveRect; }}

        /** **********************************************************************
        * Construct the ButtonBase with just the reference to its default Context.
        * Handle changing contexts externally and effect it with ChangeContext
        *
        * Enable scale effect on the button by providing a base scaling value (values less than
        * 0.5 will be set to 0.5) and optionally one for on-mouse-hover (defaults to 1.0).
        * Values larger than 1.0 are allowed. Provide a scale_step larger than 0 to control
        * how quickly the scale effect eases in and out (default is instantly).
        *
        * Enabling the scale effect automatically sets base alpha of the button to 1.0 (still
        * modified by the opacity of it's containing ButtonLayer); usually the alpha changes
        * from 0.85 to 1.0 on mouse hover. Provide a new base_alpha (unfocused transparency)
        * if you would like to reenable the opacity change.
        */
        public ButtonBase(ButtonLayer container, IHButton defaultContext,
                float base_scale = 0, float focused_scale = 1.0f, float scale_step = 0,
                float base_alpha = 0 )
        {
            Container = container;
            DefaultContext = currentContext = defaultContext;

            Name     = defaultContext.Label;
            Position = defaultContext.pos;
            Size     = defaultContext.Size;

            MaxBounds = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

            if (base_scale > 0)
            {
                this.useScaleEffect = true;
                this.baseScale = base_scale < 0.5f ? 0.5f : base_scale;
                Scale = this.baseScale;
                this.hoverScale = focused_scale;
                this.scaleStep = scale_step;

                if (base_alpha > 0 && base_alpha < 1)
                    Alpha = base_alpha;
                else
                    Alpha = 1.0f;

                //dynamic bounds that changes with the scale
                var origin = Size / 2;
                CurrentBounds = () =>
                {
                    var px = Position.X + (int)(origin.X * Scale);
                    var o = origin * Scale;

                    //WHAAAAAAAATTTTTTT
                    // FIXME: the hitbox for the string buttons is
                    // still WAY too big. Also, they freak out when
                    // the cursor is on the very edge of their area.
                    // And they may be scaling too fast? Hard to tell.
                    // Maybe we need to stop using a rectangle and
                    // just check float bounds (rather than these ints)
                    return new Rectangle(
                        (int)(px - o.X),
                        (int)(Position.Y - o.Y),
                        (int)(Size.X + o.X),
                        (int)(Size.Y + o.Y));
                };
            }
            else
            {
                //static size, static bounds
                CurrentBounds = () => MaxBounds;
            }
        }

        /// switch to a new context (button function)
        public void ChangeContext(IHButton newContext)
        {
            currentContext = newContext;
        }

        /// allows registering key toggle w/ just the context (button) labels
        public void RegisterKeyToggle(KState.Special key, string context1, string context2)
        {
            RegisterKeyToggle(key, IHBase.Instance.ButtonRepo[context1], IHBase.Instance.ButtonRepo[context2]);
        }

        /// set up key-event-subscribers that will toggle btw 2 contexts
        public void RegisterKeyToggle(KState.Special key, IHButton context1, IHButton context2)
        {
            //have to initialize (rather than just declare) this to prevent compile-time error in kw1 declaration
            var kw2 = new KeyWatcher(KState.Special.Shift, KeyEventProvider.Event.Released, null);

            var kw1 = new KeyWatcher(key, KeyEventProvider.Event.Pressed,
            () => {
                ChangeContext(context2);
                kw2.Subscribe();
                } );

            // assign kw2 onkeyevent
            kw2.OnKeyEvent = () => {
                ChangeContext(context1);
                kw1.Subscribe();
                };

            // subscribe to default watcher
            kw1.Subscribe();
        }

        public void OnMouseEnter()
        {
            Main.PlaySound(12); // "mouse-over" sound

            if (currentContext.OnMouseEnter(this))
            {

                alphaMult = 1.0f;
            }
        }

        public void OnMouseLeave()
        {
            if (currentContext.OnMouseLeave(this))
            {
                alphaMult = alphaBase;
            }
        }

        public void OnHover()
        {
            DrawTooltip();
            if (Main.mouseLeft && Main.mouseLeftRelease)
                currentContext.OnClick();

            if (Main.mouseRight && Main.mouseRightRelease && currentContext.OnRightClick!=null)
                currentContext.OnRightClick();
        }

        ///hooks into the current context's onDraw function
        public void Draw(SpriteBatch sb)
        {
            /** Disable this hook until something actually uses it... **/
            // if the context doesn't override this default draw function
            // if (currentContext.PreDraw(sb, this))
            // {
                sb.DrawIHButton(this, currentContext.DisplayState);

                if (IsHovered)
                {
                    if (!hasMouseFocus) { hasMouseFocus=true; OnMouseEnter(); }

                    if (useScaleEffect) Scale += scaleStep;
                    OnHover();

                    currentContext.PostDraw(sb, this);
                    return;
                }
                if (hasMouseFocus) { OnMouseLeave(); }
                hasMouseFocus=false;
                if (useScaleEffect) Scale -= scaleStep;

            // }
            currentContext.PostDraw(sb, this);
        }

        /// rare affects the color of the text (0 is default);
        /// I'm not quite sure what diff does...
        /// FIXME: currently displays _under_ the chest item slots
        public void DrawTooltip(int rare=0, byte diff=0)
        {
            //only draw if displaying texture
            if (currentContext.Texture!=null) {

                //TODO: is this the best place to do this? It could go lots of places.
                //Depends on whether the keybind-reminder should be considered a "core" part of the
                //button (part of the label) or something added on, just for this implementation; I'm
                //leaning towards the latter. But still, constructing the string on each draw
                //seems inefficient since we can't change the keybind in-game anyway.
                var labelDisplay = currentContext.Label + IHUtils.GetKeyTip(currentContext.Label);

                API.main.MouseText(labelDisplay, rare, diff);
                Main.mouseText = true;
            }
        }
    }
}
