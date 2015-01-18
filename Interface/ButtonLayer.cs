using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;


namespace InvisibleHand
{
    public abstract class ButtonLayer : InterfaceLayer
    {
        public readonly Dictionary<IHAction, ButtonBase> Buttons;

        protected ButtonLayer(string name) : base("InvisibleHand:" + name)
        {
            Buttons = new Dictionary<IHAction, ButtonBase>();
        }

        protected override void OnDraw(SpriteBatch sb)
        {
            foreach (KeyValuePair<IHAction, ButtonBase> kvp in Buttons)
            {
                kvp.Value.Draw(sb);
            }
        }
    }

}
