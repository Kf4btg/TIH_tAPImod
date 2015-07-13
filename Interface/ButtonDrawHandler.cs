using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public interface IButtonDrawHandler
    {
        void Draw(SpriteBatch batch, ButtonRebase bBase );
    }



    public class VanillaTextButtonDrawHandler : IButtonDrawHandler
    {
        
        public void Draw(SpriteBatch batch, ButtonRebase bBase)
        {

        }
    }
}
