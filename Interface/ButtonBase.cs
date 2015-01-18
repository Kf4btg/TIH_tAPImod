using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
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
    */
    public class ButtonBase
    {
        // a _unique_ name that can identify this button
        public readonly string Name;

        //this defines the location and size of this button
        // even if other buttons have differently-sized textures/strings,
        // the button will not move from the coordinates defined here.
        public readonly IHButton DefaultContext;

        // will be set at construction
        public readonly Vector2 Position;
        public readonly Vector2 Size;

        // defines the current appearance and functionality of the button
        private IHButton currentContext;

        // other contexts must be added later
        public ButtonBase(IHButton defaultContext)
        {
            DefaultContext = currentContext = defaultContext;
            Position = defaultContext.pos;
            Size = defaultContext.Size;
        }

        // switch to a new context
        public void ChangeContext(IHButton newContext)
        {
            this.currentContext = newContext;
        }

        // set up key-event-subscibers that will toggle b/t 2 contexts
        public void RegisterKeyToggle(KState.Special key, IHButton context1, IHButton context2)
        {
            KeyWatcher kw1, kw2;

            KeyWatcher kw1 = new KeyWatcher(key, KeyEventProvider.Pressed,
            () => {
                ChangeContext(context2);
                kw2.Subscribe();
                } );

            KeyWatcher kw2 = new KeyWatcher(KState.Special.Shift, KeyEventProvider.Released,
            () => {
                ChangeContext(context1);
                kw1.Subscribe();
                } );

            kw1.Subscribe();
        }

        // returns true if mouse currently over the button space
        public bool IsHovered()
        {
            return (new Rectangle((int)b.Position.X, (int)b.Position.Y, (int)b.Size.X, (int)b.Size.Y).Contains(Main.mouseX, Main.mouseY));
        }

        // for determining when the mouse moves on and off the button
        private bool wasHovered;

        //hooks into the current context's onDraw function
        public void Draw(SpriteBatch sb)
        {
            // if the context doesn't override this default draw function
            if (currentContext.OnDraw(sb, Position))
            {
                sb.DrawIHButton(currentContext, Position);

            //     if (currentContext.texture==null)
            //     sb.DrawString(Main.fontMouseText, displayState.label, Position, displayState.tint);
            //     else
            //     sb.Draw(displayState.texture, pos, displayState.tint);

                if (IsHovered())
                {
                    if (!wasHovered)
                    {
                        Main.PlaySound(12, -1, -1, 1); // "mouse-over" sound
                        wasHovered = true;
                    }

                    Main.localPlayer.mouseInterface = true;
                    if (Main.mouseLeft && Main.mouseLeftRelease) currentContext.onClick();
                    if (Main.mouseRight && Main.mouseRightRelease && currentContext.onRightClick!=null) currentContext.onRightClick();
                }
                else wasHovered = false;
            }
        }

    }

    public static class SBExtensions
    {
        public static void DrawIHButton(this SpriteBatch sb, IHButton button, Vector2 pos)
        {
            if (button.texture==null)
                sb.DrawString(Main.fontMouseText, button.label, pos, button.tint);
            else
                sb.Draw(button.texture, pos, button.tint);
        }
    }
}
