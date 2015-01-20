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
        public readonly Rectangle ButtonArea;

        // private float scale = Main.inventoryScale;
        public const float SCALE_FULL = 1.0f;
        private float scale = ButtonBase.SCALE_FULL;
        public float Scale { get { return scale; } set { scale = value < 0.5f ? 0.5f : value; } }

        // affects the alpha component of the current tint
        public const float ALPHA_DIMMED = 0.65f;
        private float alphaMult = ButtonBase.ALPHA_DIMMED;
        public float Alpha { get { return alphaMult; } }
        // public Color Tint { get {
        //     // return new Color(   (int)((float)currentContext.tint.R * alphaMult),
        //     //                     (int)((float)currentContext.tint.G * alphaMult),
        //     //                     (int)((float)currentContext.tint.B * alphaMult),
        //     //                     (int)((float)currentContext.tint.A * alphaMult)
        //     //                 );
        //
        //     return currentContext.tint * alphaMult;
        //                     }}

        // defines the current appearance and functionality of the button
        private IHButton currentContext;

        // other contexts must be added later
        public ButtonBase(IHButton defaultContext)
        {
            DefaultContext = currentContext = defaultContext;
            Name = defaultContext.displayLabel;
            Position = defaultContext.pos;
            Size = defaultContext.Size;

            ButtonArea = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        }

        // switch to a new context
        public void ChangeContext(IHButton newContext)
        {
            currentContext = newContext;
        }

        // set up key-event-subscibers that will toggle b/t 2 contexts
        public void RegisterKeyToggle(KState.Special key, IHButton context1, IHButton context2)
        {
            //have to initialize this to prevent compile-time error in kw1 declaration
            KeyWatcher kw2 = new KeyWatcher(KState.Special.Shift, KeyEventProvider.Event.Released, null);

            KeyWatcher kw1 = new KeyWatcher(key, KeyEventProvider.Event.Pressed,
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

        // for determining when the mouse moves on and off the button
        private bool wasHovered;

        // returns true if mouse currently over the button space
        public bool IsHovered()
        {
            // return (new Rectangle((int)b.Position.X, (int)b.Position.Y, (int)b.Size.X, (int)b.Size.Y).Contains(Main.mouseX, Main.mouseY));
            return ButtonArea.Contains(Main.mouseX, Main.mouseY);
        }

        public void OnMouseEnter()
        {
            Main.PlaySound(12); // "mouse-over" sound
            wasHovered = true;
            // scale *= 1.1f;
            alphaMult = 1.0f;
        }

        public void OnMouseLeave()
        {
            wasHovered = false;
            // scale = 1;
            alphaMult = ButtonBase.ALPHA_DIMMED;
        }

        //hooks into the current context's onDraw function
        public void Draw(SpriteBatch sb)
        {
            // if the context doesn't override this default draw function
            if (currentContext.OnDraw(sb, this))
            {
                sb.DrawIHButton(this, currentContext.displayState);

            //     if (currentContext.texture==null)
            //     sb.DrawString(Main.fontMouseText, displayState.label, Position, displayState.tint);
            //     else
            //     sb.Draw(displayState.texture, pos, displayState.tint);

                if (IsHovered())
                {
                    if (!wasHovered) { OnMouseEnter(); }

                    Main.localPlayer.mouseInterface = true;
                    DrawTooltip(0,0);
                    if (Main.mouseLeft && Main.mouseLeftRelease) currentContext.onClick();
                    if (Main.mouseRight && Main.mouseRightRelease && currentContext.onRightClick!=null) currentContext.onRightClick();
                }
                else if (wasHovered) { OnMouseLeave(); }
            }
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

    public static class SBExtensions
    {
        public static void DrawIHButton(this SpriteBatch sb, ButtonBase bBase, ButtonState state)
        {
            if (state.texture==null)
                sb.DrawString(Main.fontMouseText, state.label, bBase.Position, state.tint*bBase.Alpha);
            else
                sb.Draw(state.texture, bBase.Position, null, state.tint*bBase.Alpha, 0f, default(Vector2), bBase.Scale, SpriteEffects.None, 0f);
        }
    }
}
