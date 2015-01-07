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
        private readonly Vector2 PosDA, PosLA, PosQS;
        private readonly IHToggle[] buttons = new IHToggle[3];
        private readonly Vector2[] positions = new Vector2[3];

        /*****************************************************************
        *   Create the buttons that will be used to toggle the states of
        *   the variables controlling how the vanilla functions interact
        *   with locked slots.

            At the moment, the text (placeholder for future button textures) actually
            shows up and can be toggled by clicking on it. Save/load w/ player works
            FIXME: Initially, the display text is the disabled label regardles of its actual state, though the coloring is correct
            FIXME: No visual feedback when bringing the mouse to hover over the button.
            TODO: Actually implement the check for these options in the IHUtils code

            FIXME: this function (this entire *class*) is an absolute mess.
                   Get rid of those stupid arrays and fields.
                   Which of these FlagUpdate() calls is really necessary? Isn't there still one in the IHToggle ctor?
                   And FlagUpdate/MarkUpdate are pretty much redundant. Figure out which one to keep.

        */
        public LockOptions() : base("InvisibleHand:LockOptions")
        {
            // this should put the buttons to the left of the chest inventory
            float posX = 73 - (Main.inventoryBackTexture.Width * Main.inventoryScale);
            float posY = API.main.invBottom + (Main.inventoryBackTexture.Height * Main.inventoryScale)/2;

            PosDA = new Vector2(posX, posY);
            PosLA = new Vector2(posX, posY + (Main.inventoryBackTexture.Height * Main.inventoryScale));
            PosQS = new Vector2(posX, 2*(Main.inventoryBackTexture.Height * Main.inventoryScale) + posY);


            lockDA = new IHToggle("DA", "da", null, () => IHPlayer.daLocked, () => //onToggle
//            lockDA = new IHToggle("dalock", "Deposit All Locked", "Deposit All Unlocked", null, () => {return IHPlayer.daLocked;}, () =>
            {
                IHPlayer.daLocked=!IHPlayer.daLocked;
                lockDA.Update();
            }, PosDA);
            IHBase.FlagUpdate(lockDA);

            buttons[0]=lockDA;

            lockLA = new IHToggle("LA", "la", null, () => IHPlayer.laLocked, () => //onToggle
            // lockLA = new IHToggle("lalock", "Loot All Locked", "Loot All Unlocked", null, () => {return IHPlayer.laLocked;}, () =>
            {
                IHPlayer.laLocked=!IHPlayer.laLocked;
                lockLA.Update();
            }, PosLA);
            IHBase.FlagUpdate(lockLA);

            buttons[1]=lockLA;

            lockQS = new IHToggle("QS", "qs", null, () => IHPlayer.qsLocked, () => //onToggle
            // lockQS = new IHToggle("qslock", "Quick Stack Locked", "Quick Stack Unlocked", null, () => {return IHPlayer.qsLocked;}, () =>
            {
                IHPlayer.qsLocked=!IHPlayer.qsLocked;
                lockQS.Update();
            }, PosQS);
            IHBase.FlagUpdate(lockQS);

            buttons[2]=lockQS;

        }

        protected override void OnDraw(SpriteBatch sb)
        {
            for (int i=0; i<3; i++)
            {
                buttons[i].Draw(sb);
            }
        }
    }
}
