using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class IHWorld : ModWorld
    {
        public override void Initialize()
        {
            // finding a place to do this where the buttons are actually *set correctly*
            // upon initial load has been the bitchiest of bitches.
            foreach (KeyValuePair<VAction, IHToggle> kvp in IHInterface.self.lockButtons.Buttons)
            {
                kvp.Value.Update();
            }
        }
    }
}
