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
            while (IHBase.self.ButtonUpdates.Count>0)
            {
                string btn = IHBase.self.ButtonUpdates.Pop();
                IHBase.self.ButtonRepo[btn].OnUpdate();
            }
        }
    }
}
