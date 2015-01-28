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
            // upon initial load has beensdjnkdsjnklsakjnasdfjk
            foreach (String btn in IHBase.self.ButtonUpdates)
            {
                IHBase.self.ButtonRepo[btn].OnUpdate();
            }
        }
    }
}
