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

        public readonly ButtonLayer Container; //interface layer this button belongs to

        // this defines the location and size of this button.
        // even if other buttons have differently-sized textures/strings,
        // the button will not move from the coordinates defined here.
        public readonly IHButton DefaultContext;

        // defines the current appearance and functionality of the button
        private IHButton currentContext;
        public IHButton CurrentContext { get { return currentContext; } }
        public ButtonState CurrentState { get { return currentContext.DisplayState; } }

        // will be set at construction
        public readonly Vector2 Position;
        public readonly Vector2 Size;
        public readonly Rectangle ButtonBounds;
        public bool IsHovered { get { return ButtonBounds.Contains(Main.mouseX, Main.mouseY); } }

        // for determining when the mouse moves on and off the button
        private bool hasMouseFocus;

        public const float SCALE_FULL = 1.0f;
        private float scale = ButtonBase.SCALE_FULL;
        public float Scale { get { return scale; } set { scale = value < 0.5f ? 0.5f : value; } }

        // affects the alpha component of the current tint
        private float alphaBase = 0.85f;
        private float alphaMult = 0.85f;
        // gets current alpha (modified by container opacity), sets current and base alpha values
        public float Alpha { get { return alphaMult*Container.LayerOpacity; } set { alphaMult = alphaBase = value; }}

        // this will actively set the Source Texels based on whether or not the mouse is currently over this button.
        // If both rects are null, then the entire texture will be drawn as per default
        public Rectangle? SourceRect { get { return hasMouseFocus ? currentContext.ActiveRect : currentContext.InactiveRect; }}

        /**************************************************************************
        * Construct the ButtonBase with just the reference to its default Context.
        * Handle changing contexts externally and effect it with ChangeContext
        */
        public ButtonBase(ButtonLayer container, IHButton defaultContext)
        {
            Container = container;
            DefaultContext = currentContext = defaultContext;

            Name     = defaultContext.Label;
            Position = defaultContext.pos;
            Size     = defaultContext.Size;

            ButtonBounds = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        }

        // switch to a new context
        public void ChangeContext(IHButton newContext)
        {
            currentContext = newContext;
        }

        public void RegisterKeyToggle(KState.Special key, String context1, String context2)
        {
            RegisterKeyToggle(key, IHBase.Instance.ButtonRepo[context1], IHBase.Instance.ButtonRepo[context2]);
        }

        // set up key-event-subscribers that will toggle btw 2 contexts
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
                alphaMult = 1.0f;
        }

        public void OnMouseLeave()
        {
            if (currentContext.OnMouseLeave(this))
                alphaMult = alphaBase;
        }

        public void OnHover()
        {
            DrawTooltip();
            if (Main.mouseLeft && Main.mouseLeftRelease) currentContext.OnClick();
            if (Main.mouseRight && Main.mouseRightRelease && currentContext.OnRightClick!=null) currentContext.OnRightClick();
        }

        //hooks into the current context's onDraw function
        public void Draw(SpriteBatch sb)
        {
            /** Disable this hook until something actually uses it... **/
            // if the context doesn't override this default draw function
            // if (currentContext.OnDraw(sb, this))
            // {
                sb.DrawIHButton(this, currentContext.DisplayState);

                if (IsHovered)
                {
                    if (!hasMouseFocus) { hasMouseFocus=true; OnMouseEnter(); }
                    OnHover();

                    currentContext.PostDraw(sb, this);
                    return;
                }
                if (hasMouseFocus) { OnMouseLeave(); }
                hasMouseFocus=false;
            // }
            currentContext.PostDraw(sb, this);
        }

        // rare affects the color of the text (0 is default);
        // I'm not quite sure what diff does...
        // FIXME: currently displays _under_ the chest item slots
        public void DrawTooltip(int rare=0, byte diff=0)
        {
            //only draw if displaying texture
            if (currentContext.Texture!=null) {
                API.main.MouseText(currentContext.Label, rare, diff);
                Main.mouseText = true;
            }
        }
    }
}
