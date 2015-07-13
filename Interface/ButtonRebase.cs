using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{

    public class ButtonRebase : ButtonBase
    {

        ///interface layer this button belongs to
        // public readonly ButtonLayer Container;

        protected Vector2 position;

        public ButtonRebase(ButtonLayer container, IHButton defaultContext, float base_alpha = 0.85f, float alpha_step = 0.01f)
            : base(container, defaultContext, base_alpha, alpha_step)
        {
        }

    }


}
