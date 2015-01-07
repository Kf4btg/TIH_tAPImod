using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class LockOptions : InterfaceLayer
    {
        protected readonly IHToggle lockDA, lockLA, lockQS;

        public LockOptions() : base("InvisibleHand:LockOptions")
        {
            lockDA = new IHToggle("Deposit All Locked", "Deposit All Unlocked", null, () => {return IHPlayer.daLocked;}, () =>
            {
                IHPlayer.daLocked=!IHPlayer.daLocked;
            });

            lockDA = new IHToggle("Loot All Locked", "Loot All Unlocked", null, () => {return IHPlayer.laLocked;}, () =>
            {
                IHPlayer.laLocked=!IHPlayer.laLocked;
            });

            lockDA = new IHToggle("Quick Stack Locked", "Quick Stack Unlocked", null, () => {return IHPlayer.qsLocked;}, () =>
            {
                IHPlayer.qsLocked=!IHPlayer.qsLocked;
            });
        }

        protected override void OnDraw(SpriteBatch sb)
        {
            
        }
    }
}
