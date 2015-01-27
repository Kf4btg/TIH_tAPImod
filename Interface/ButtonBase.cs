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
        // private readonly ButtonLayer container;

        //this defines the location and size of this button
        // even if other buttons have differently-sized textures/strings,
        // the button will not move from the coordinates defined here.
        public readonly IHButton DefaultContext;

        // will be set at construction
        public readonly Vector2 Position;
        public readonly Vector2 Size;
        public readonly Rectangle ButtonBounds;
        // private readonly Point center;
        // for determining when the mouse moves on and off the button
        private bool isHovered;

        public const float SCALE_FULL = 1.0f;
        private float scale = ButtonBase.SCALE_FULL;
        public float Scale { get { return scale; } set { scale = value < 0.5f ? 0.5f : value; } }

        // affects the alpha component of the current tint
        private float alphaBase = 0.65f;
        private float alphaMult = 0.65f;
        // gets current alpha, sets current and base alpha
        public float Alpha { get { return alphaMult; } set { alphaMult = alphaBase = value; }}

        // defines the current appearance and functionality of the button
        private IHButton currentContext;
        public IHButton CurrentContext { get { return currentContext; } }
        public ButtonState CurrentState { get { return currentContext.displayState; } }


        // change contexts externally using ChangeContext()
        public ButtonBase(ButtonLayer container, IHButton defaultContext)
        {
            // this.container = container;
            DefaultContext = currentContext = defaultContext;
            Name = defaultContext.displayLabel;
            Position = defaultContext.pos;
            Size = defaultContext.Size;

            ButtonBounds = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
            // center = ButtonBounds.Center;
        }

        // switch to a new context
        public void ChangeContext(IHButton newContext)
        {
            //set previous context un-hovered, hover new one:
            if (isHovered) {
                currentContext.OnMouseLeave(this);
                newContext.OnMouseEnter(this);
            }
            currentContext = newContext;

        }

        // set up key-event-subscibers that will toggle b/t 2 contexts
        public void RegisterKeyToggle(KState.Special key, IHButton context1, IHButton context2)
        {
            //have to initialize this to prevent compile-time error in kw1 declaration
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

        // returns true if mouse currently over the button space
        public bool IsHovered()
        {
            // return (new Rectangle((int)b.Position.X, (int)b.Position.Y, (int)b.Size.X, (int)b.Size.Y).Contains(Main.mouseX, Main.mouseY));
            return ButtonBounds.Contains(Main.mouseX, Main.mouseY);
        }

        public void OnMouseEnter()
        {
            Main.PlaySound(12); // "mouse-over" sound
            isHovered = true;

            if (currentContext.OnMouseEnter(this))
            {
                alphaMult = 1.0f;
            }
        }

        public void OnMouseLeave()
        {
            isHovered = false;
            if (currentContext.OnMouseLeave(this))
                alphaMult = alphaBase;
        }

        public void OnHover()
        {
            //if (currentContext.OnHover(this))  //future hook?
            Main.localPlayer.mouseInterface = true;
            DrawTooltip(0,0);
            if (Main.mouseLeft && Main.mouseLeftRelease) currentContext.onClick();
            if (Main.mouseRight && Main.mouseRightRelease && currentContext.onRightClick!=null) currentContext.onRightClick();
        }

        public void Draw(SpriteBatch sb, Color overrideColor)
        {
            sb.DrawIHButton(this, currentContext.displayState, overrideColor);

            if (IsHovered())
            {
                if (!isHovered) { OnMouseEnter(); }
                OnHover();
            }
            else if (isHovered) { OnMouseLeave(); }
        }

        //hooks into the current context's onDraw function
        public void Draw(SpriteBatch sb)
        {
            /** Disable this hook until something actually uses it... **/
            // if the context doesn't override this default draw function
            // if (currentContext.OnDraw(sb, this))
            // {
                sb.DrawIHButton(this, currentContext.displayState);

                if (IsHovered())
                {
                    if (!isHovered) { OnMouseEnter(); }
                    OnHover();
                }
                else if (isHovered) { OnMouseLeave(); }
            // }
        }

        // rare affects the color of the text (0 is default);
        // I'm not quite sure what diff does...
        // FIXME: currently displays _under_ the chest item slots
        public void DrawTooltip(int rare, byte diff)
        {
            //only draw if displaying texture
            if (currentContext.texture!=null) {
                API.main.MouseText(currentContext.displayLabel, rare, diff);
                Main.mouseText = true;
            }
        }
    }
}
