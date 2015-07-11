using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
        TODO 2: ...what code was that, again?
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
        protected IHButton currentContext;
        public IHButton CurrentContext { get { return currentContext; } }
        public ButtonState CurrentState { get { return currentContext.DisplayState; } }

        // will be set at construction
        public readonly Vector2 Position;
        public readonly Vector2 Size;
        public readonly Rectangle ButtonBounds;

        public virtual bool IsHovered
        {
            get { return ButtonBounds.Contains(Main.mouseX, Main.mouseY); }
        }

        /// for determining when the mouse moves on and off the button
        // protected bool hasMouseFocus;
        public virtual bool HasMouseFocus { get; set; }

    public const float SCALE_FULL = 1.0f;
        protected float scale = SCALE_FULL;
        public virtual float Scale {get { return scale; } set { scale = value < 0.5 ? 0.5f : value; } }

        // affects the alpha component of the current tint
        public float alphaBase { get; protected set; }
        protected float alphaMult;
        public float AlphaMult { get { return alphaMult; }
            set {alphaMult = value < alphaBase ? alphaBase :
                             value > 1.0f ? 1.0f : value;
                }}
        public readonly float alphaStep;
        /// gets current alpha (modified by container opacity), sets current and base alpha values
        public virtual float Alpha { get { return alphaMult*Container.LayerOpacity; } set { AlphaMult = alphaBase = value; }}

        // public virtual float Alpha {
        //     get { return alphaMult*Container.LayerOpacity; }
        //     set { AlphaMult = value; } }



        /// this will actively set the Source Texels based on whether or not the mouse is currently over this button.
        /// If both rects are null, then the entire texture will be drawn as per default
        public Rectangle? SourceRect { get { return HasMouseFocus ? currentContext.ActiveRect : currentContext.InactiveRect; }}

        /** **********************************************************************
        * Construct the ButtonBase with just the reference to its default Context.
        * Handle changing contexts externally and effect it with ChangeContext
        */
        public ButtonBase(ButtonLayer container, IHButton defaultContext, float base_alpha = 0.85f, float alpha_step = 0.05f)
        {
            Container = container;
            DefaultContext = currentContext = defaultContext;

            Name     = defaultContext.Label;
            Position = defaultContext.pos;
            Size     = defaultContext.Size;

            alphaBase = alphaMult = base_alpha;
            alphaStep = alpha_step;

            ButtonBounds = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        }

        /// switch to a new context (button function)
        public void ChangeContext(IHButton newContext)
        {
            currentContext = newContext;
        }

        /// return this ButtonBase to its default context
        public void Reset()
        {
            ChangeContext(DefaultContext);
        }

        /// allows registering key toggle w/ just the context (button) labels
        public void RegisterKeyToggle(KState.Special key, string context1, string context2)
        {
            RegisterKeyToggle(key, IHBase.Instance.ButtonRepo[context1], IHBase.Instance.ButtonRepo[context2]);
        }

        /// register a key toggle for this base's default context
        public void RegisterKeyToggle(KState.Special key, IHButton context2)
        {
            RegisterKeyToggle(key, this.DefaultContext, context2);
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

        public virtual void OnMouseEnter()
        {
            // Main.PlaySound(12); // "mouse-over" sound
            if (currentContext.OnMouseEnter(this))
                Sound.MouseOver.Play();

                // alphaMult = 1.0f;
        }

        public virtual void OnMouseLeave()
        {
            currentContext.OnMouseLeave(this);
            // alphaMult = alphaBase;
        }

        public virtual void OnHover()
        {
            DrawTooltip();
            if (Main.mouseLeft && Main.mouseLeftRelease)
                currentContext.OnClick();

            if (Main.mouseRight && Main.mouseRightRelease && currentContext.OnRightClick!=null)
                currentContext.OnRightClick();
        }

        ///hooks into the current context's onDraw function
        public virtual void Draw(SpriteBatch sb)
        {
            /** Disable this hook until something actually uses it... **/
            // if the context doesn't override this default draw function
            if (currentContext.PreDraw(sb, this))
            {
                sb.DrawIHButton(this, currentContext.DisplayState);

                if (IsHovered)
                {
                    if (!HasMouseFocus) { HasMouseFocus=true; OnMouseEnter(); }

                    if (AlphaMult!=1.0f)
                        AlphaMult += alphaStep;

                    OnHover();

                    currentContext.PostDraw(sb, this);
                    return;
                }
                if (HasMouseFocus) { OnMouseLeave(); }
                if (AlphaMult!=alphaBase)
                    AlphaMult -= alphaStep;
                HasMouseFocus=false;

            }
            currentContext.PostDraw(sb, this);
        }

        /// rare affects the color of the text (0 is default);
        /// I'm not quite sure what diff does...
        /// FIXME: currently displays _under_ other UI items
        public void DrawTooltip(int rare=0, byte diff=0)
        {
            //only draw if displaying texture
            if (currentContext.Texture!=null) {

                //TODO: is this the best place to do this? It could go lots of places.
                //Depends on whether the keybind-reminder should be considered a "core" part of the
                //button (part of the label) or something added on, just for this implementation; I'm
                //leaning towards the latter. But still, constructing the string on each draw
                //seems inefficient since we can't change the keybind in-game anyway.
                var labelDisplay = currentContext.Label + IHUtils.GetKeyTip(currentContext.Action);

                API.main.MouseText(labelDisplay, rare, diff);
                Main.mouseText = true;
            }
        }
    }
}
