using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
// using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    /*
        Conceptually, this can be thought of as the "actual" button: it defines the location
        on the screen where the button appears and how much space it takes up. This is the
        object that will receive mouse clicks, which will then be passed on to its members.

        Technically, though, this is more of a button "frame" or "platform."  It contains
        ButtonContexts (which define what the button _can_ do) that in turn contain
        ButtonStates (which define what the button _actually_ does).  All of these
        can be swapped out and around to create some fairly dynamic interactivity.

        TODO: stop duplicating all the code from UIComponent et al...
    */
    public class ButtonBase
    {
        // a _unique_ name that can identify this button
        public readonly string Name;

        public readonly ButtonLayer Container;

        //this defines the location and size of this button
        // even if other buttons have differently-sized textures/strings,
        // the button will not move from the coordinates defined here.
        public readonly IHButton DefaultContext;

        // defines the current appearance and functionality of the button
        private IHButton currentContext;
        public IHButton CurrentContext { get { return currentContext; } }
        public ButtonState CurrentState { get { return currentContext.displayState; } }

        //and here's our choices
        private Dictionary<String, IHButton> contexts; //welp.

        // will be set at construction
        public readonly Vector2 Position;
        public readonly Vector2 Size;
        public readonly Rectangle ButtonBounds;
        public bool IsHovered { get { return ButtonBounds.Contains(Main.mouseX, Main.mouseY); } }

        // private readonly Point center;
        // for determining when the mouse moves on and off the button
        private bool hasMouseFocus;

        public const float SCALE_FULL = 1.0f;
        private float scale = ButtonBase.SCALE_FULL;
        public float Scale { get { return scale; } set { scale = value < 0.5f ? 0.5f : value; } }

        // affects the alpha component of the current tint
        private float alphaBase = 0.65f;
        private float alphaMult = 0.65f;
        // gets current alpha (modified by container opacity), sets current and base alpha values
        public float Alpha { get { return alphaMult*Container.LayerOpacity; } set { alphaMult = alphaBase = value; }}

        //this will actively set the Source Texels based on whether or not the mouse is currently over this button.
        // If both rects are null, then the entire texture will be drawn as per default
        // This should hopefully solve the issue of incorrect display states when toggling contexts w/ Shift
        public Rectangle? SourceRect { get { return hasMouseFocus ? currentContext.ActiveRect : currentContext.InactiveRect; }}


        /**
         * Construct the ButtonBase with just the reference to its default Context.
         * Add more contexts with Add
         */
        public ButtonBase(ButtonLayer container, IHButton defaultContext)
        {
            Container = container;

            contexts = new Dictionary<String, IHButton>();
            contexts.Add(defaultContext.Name, defaultContext);

            DefaultContext = currentContext = defaultContext;
            Name = defaultContext.Name;
            Position = defaultContext.pos;
            Size = defaultContext.Size;

            ButtonBounds = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            // center = ButtonBounds.Center;
        }

        public void Add(IHButton newContext)
        {
            if (!contexts.ContainsKey(newContext.Name)) contexts.Add(newContext.Name, newContext);
        }

        // this makes sure we don't switch to a context not associated with this button
        public void ChangeContext(String newContext)
        {
            if (contexts.ContainsKey(newContext)) ChangeContext(contexts[newContext]);
        }

        // switch to a new context
        private void ChangeContext(IHButton newContext)
        {
            //set previous context un-hovered, hover new one:
            // if (hasMouseFocus) {
            //     currentContext.OnMouseLeave(this);
            //     newContext.OnMouseEnter(this);
            // }
            currentContext = newContext;
        }

        public void RegisterKeyToggle(KState.Special key, String context1, String context2)
        {
            if (contexts.ContainsKey(context1) && contexts.ContainsKey(context2))
                RegisterKeyToggle(key, contexts[context1], contexts[context2]);
        }

        // set up key-event-subscribers that will toggle btw 2 contexts
        private void RegisterKeyToggle(KState.Special key, IHButton context1, IHButton context2)
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
            // hasMouseFocus = true;

            if (currentContext.OnMouseEnter(this))
            {
                alphaMult = 1.0f;
            }
        }

        public void OnMouseLeave()
        {
            // hasMouseFocus = false;
            if (currentContext.OnMouseLeave(this))
                alphaMult = alphaBase;
        }

        public void OnHover()
        {
            //if (currentContext.OnHover(this))  //future hook?
            DrawTooltip();
            if (Main.mouseLeft && Main.mouseLeftRelease) currentContext.onClick();
            if (Main.mouseRight && Main.mouseRightRelease && currentContext.onRightClick!=null) currentContext.onRightClick();
        }

        //hooks into the current context's onDraw function
        public void Draw(SpriteBatch sb)
        {
            /** Disable this hook until something actually uses it... **/
            // if the context doesn't override this default draw function
            // if (currentContext.OnDraw(sb, this))
            // {
                sb.DrawIHButton(this, currentContext.displayState);

                if (IsHovered)
                {
                    if (!hasMouseFocus) { hasMouseFocus=true; OnMouseEnter(); }
                    OnHover();
                    return;
                }
                if (hasMouseFocus) { OnMouseLeave(); }
                hasMouseFocus=false;
            // }
        }

        // rare affects the color of the text (0 is default);
        // I'm not quite sure what diff does...
        // FIXME: currently displays _under_ the chest item slots
        public void DrawTooltip(int rare=0, byte diff=0)
        {
            //only draw if displaying texture
            if (currentContext.texture!=null) {
                API.main.MouseText(currentContext.displayLabel, rare, diff);
                Main.mouseText = true;
            }
        }
    }
}
