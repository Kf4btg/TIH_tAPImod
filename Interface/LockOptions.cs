using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class LockOptions : InterfaceLayer
    {
        // protected readonly IHToggle lockDA, lockLA, lockQS;
        // private readonly IHToggle[] buttons = new IHToggle[3];
        public readonly List<IHToggle> buttons;
        internal int buttonCount;

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
                   Get rid of those stupid arrays.

        */
        public LockOptions() : base("InvisibleHand:LockOptions")
        {
            // this should put the buttons to the left of the chest inventory
            float posX = 73 - (Main.inventoryBackTexture.Width * Main.inventoryScale);
            float posY = API.main.invBottom + (Main.inventoryBackTexture.Height * Main.inventoryScale)/2;

            buttons = new List<IHToggle>();
            // buttonCount = 0;

            buttons.Add(new IHToggle("QS", "qs", null, () => IHPlayer.ActionLocked(Main.localPlayer, IHPlayer.VACTION_QS), () => //onToggle
            // lockQS = new IHToggle("qslock", "Quick Stack Locked", "Quick Stack Unlocked", null, () => {return IHPlayer.qsLocked;}, () =>
            {
                IHPlayer.ToggleActionLock(Main.localPlayer, IHPlayer.VACTION_QS);
                buttons[IHPlayer.VACTION_QS].Update();
                },
            new Vector2(posX, 2*(Main.inventoryBackTexture.Height * Main.inventoryScale) + posY)));

            // IHBase.FlagUpdate(buttons[buttonCount]);
            // buttonCount++;
            // buttons[2]=lockQS;

            buttons.Add(new IHToggle("DA", "da", null, () => IHPlayer.ActionLocked(Main.localPlayer, IHPlayer.VACTION_DA), () => //onToggle
//            lockDA = new IHToggle("dalock", "Deposit All Locked", "Deposit All Unlocked", null, () => {return IHPlayer.daLocked;}, () =>
            {
                IHPlayer.ToggleActionLock(Main.localPlayer, IHPlayer.VACTION_DA);
                buttons[IHPlayer.VACTION_DA].Update();
            },
            new Vector2(posX, posY)));

            // IHBase.FlagUpdate(buttons[buttonCount]);
            // buttonCount++;

            // buttons[0]=lockDA;

            buttons.Add(new IHToggle("LA", "la", null, () => IHPlayer.ActionLocked(Main.localPlayer, IHPlayer.VACTION_LA), () => //onToggle
            // lockLA = new IHToggle("lalock", "Loot All Locked", "Loot All Unlocked", null, () => {return IHPlayer.laLocked;}, () =>
            {
                IHPlayer.ToggleActionLock(Main.localPlayer, IHPlayer.VACTION_LA);
                buttons[IHPlayer.VACTION_LA].Update();
                },
            new Vector2(posX, posY + (Main.inventoryBackTexture.Height * Main.inventoryScale))));

            // IHBase.FlagUpdate(buttons[buttonCount]);
            buttonCount=buttons.Count;
            // buttons[1]=lockLA;


        }

        protected override void OnDraw(SpriteBatch sb)
        {
            for (int i=0; i<buttonCount; i++)
            {
                buttons[i].Draw(sb);
            }
        }
    }
}
