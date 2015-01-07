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
            shows up and can be toggled by clicking on it.
            FIXME: Initially, the display text is the button name, rather than its corresponding
                    active/inactive label.
            FIXME: The default left click action still occurs when clicking on the buttons (e.g. weapon used, etc.)
            FIXME: No visual or audible feedback when bringing the mouse to hover over the button.
            FIXME: These should show up only when a chest is open
            TODO: On that note, they should be moved down to the left of the chest item slots.
            TODO: Actually test to make sure the BinBuffer saving/loading in IHPlayer works.
            TODO: Actually implement the check for these options in the IHUtils code

            FIXME: this function (this entire *class*) is an absolute mess.
                   Get rid of those stupid arrays and fields.
                   Which of these FlagUpdate() calls is really necessary? Isn't there still one in the IHToggle ctor?
                   And FlagUpdate/MarkUpdate are pretty much redundant. Figure out which one to keep.

        */
        public LockOptions() : base("InvisibleHand:LockOptions")
        {
            lockDA = new IHToggle("dalock", "DA", "da", null, () => {return IHPlayer.daLocked;}, () =>
//            lockDA = new IHToggle("dalock", "Deposit All Locked", "Deposit All Unlocked", null, () => {return IHPlayer.daLocked;}, () =>
            {
                IHPlayer.daLocked=!IHPlayer.daLocked;
                lockDA.FlagUpdate();
            });
            lockDA.FlagUpdate();

            buttons[0]=lockDA;

            lockLA = new IHToggle("lalock", "LA", "la", null, () => {return IHPlayer.laLocked;}, () =>
            // lockLA = new IHToggle("lalock", "Loot All Locked", "Loot All Unlocked", null, () => {return IHPlayer.laLocked;}, () =>
            {
                IHPlayer.laLocked=!IHPlayer.laLocked;
                lockLA.FlagUpdate();
            });
            lockLA.FlagUpdate();

            buttons[1]=lockLA;

            lockQS = new IHToggle("qslock", "QS", "qs", null, () => {return IHPlayer.qsLocked;}, () =>
            // lockQS = new IHToggle("qslock", "Quick Stack Locked", "Quick Stack Unlocked", null, () => {return IHPlayer.qsLocked;}, () =>
            {
                IHPlayer.qsLocked=!IHPlayer.qsLocked;
                lockQS.FlagUpdate();
            });
            lockQS.FlagUpdate();

            buttons[2]=lockQS;

            float posX = 2;
            float posY = 30 + Main.inventoryBackTexture.Height;

            PosDA = new Vector2(posX, posY);
            positions[0]=PosDA;
            PosLA = new Vector2(posX, posY + Main.inventoryBackTexture.Height);
            positions[1]=PosLA;
            PosQS = new Vector2(posX, 2*Main.inventoryBackTexture.Height + posY);
            positions[2]=PosQS;

        }

        protected override void OnDraw(SpriteBatch sb)
        {
            for (int i=0; i<3; i++)
            {
                buttons[i].Draw(sb, positions[i]);
            }
        }
    }
}
