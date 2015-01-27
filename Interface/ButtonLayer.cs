using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
// using Terraria;

namespace InvisibleHand
{
    public abstract class ButtonLayer : InterfaceLayer
    {
        public readonly Dictionary<IHAction, ButtonBase> Buttons;

        //this will contain ALL the possible button-contexts, even those grouped onto a single base
        public readonly Dictionary<IHAction, IHButton> Contexts;

        // public Rectangle ButtonFrame { get; protected set;}
        // public Point FrameCenter { get; private set;}

        protected ButtonLayer(string name) : base("InvisibleHand:" + name)
        {
            Buttons = new Dictionary<IHAction, ButtonBase>();
            // ButtonFrame = Rectangle.Empty;
            // FrameCenter = Point.Zero;
        }

        // protected void UpdateFrame()
        // {
        //     foreach (var kvp in Buttons)
        //     {
        //         ButtonFrame = (ButtonFrame.IsEmpty) ? kvp.Value.ButtonBounds : Rectangle.Union(ButtonFrame, kvp.Value.ButtonBounds);
        //     }
        //     if (!ButtonFrame.IsEmpty) FrameCenter = ButtonFrame.Center;
        //
        // }

        protected override void OnDraw(SpriteBatch sb)
        {
            if (!parentLayer.visible) return;
            foreach (KeyValuePair<IHAction, ButtonBase> kvp in Buttons)
            {
                kvp.Value.Draw(sb);
            }
        }
    }

}
