using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class IHWorld : ModWorld
    {

        // Dictionary<VAction, IHToggle> lockButtons = IHBase.self.lockOptions.Buttons;

        public override void Initialize()
        {
            // finding a place to do this where the buttons are actually *set correctly*
            // upon initial load has beensdjnkdsjnklsakjnasdfjk
            // foreach (KeyValuePair<IHAction, IHButton> kvp in IHBase.self.lockOptions.Buttons)
            // {
            //     ((IHToggle)kvp.Value).Init();
            // }
        }
    }
}
