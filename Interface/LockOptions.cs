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
        public readonly Dictionary<VAction, IHToggle> Buttons;

        /*****************************************************************
        *   Create the Buttons that will be used to toggle the states of
        *   the variables controlling how the vanilla functions interact
        *   with locked slots.

            At the moment, the text (placeholder for future button textures) actually
            shows up and can be toggled by clicking on it. Save/load w/ player works
            FIXME: No visual feedback when bringing the mouse to hover over the button.
            TODO: This class could still use some cleanup.
        */
        public LockOptions() : base("InvisibleHand:LockOptions")
        {
            // this should put the Buttons to the left of the chest inventory
            float posX = 73 - (Main.inventoryBackTexture.Width * Main.inventoryScale);
            float posY = API.main.invBottom + (Main.inventoryBackTexture.Height * Main.inventoryScale)/2;

            Buttons = new Dictionary<VAction, IHToggle>();

            // lockQS = new IHToggle("qslock", "Quick Stack Locked", "Quick Stack Unlocked", null, () => {return IHPlayer.qsLocked;}, () =>
            Buttons[VAction.QS] = new IHToggle("QS", "qs", null, () => IHPlayer.ActionLocked(Main.localPlayer, VAction.QS), () => //onToggle
                {
                    Main.PlaySound(22, -1, -1, 1);
                    IHPlayer.ToggleActionLock(Main.localPlayer, VAction.QS);
                    Buttons[VAction.QS].Update();
                },
                new Vector2(posX, 2*(Main.inventoryBackTexture.Height * Main.inventoryScale) + posY) );

            // lockDA = new IHToggle("dalock", "Deposit All Locked", "Deposit All Unlocked", null, () => {return IHPlayer.daLocked;}, () =>
            Buttons[VAction.DA] = new IHToggle("DA", "da", null, () => IHPlayer.ActionLocked(Main.localPlayer, VAction.DA), () => //onToggle
                {
                    Main.PlaySound(22, -1, -1, 1);
                    IHPlayer.ToggleActionLock(Main.localPlayer, VAction.DA);
                    Buttons[VAction.DA].Update();
                },
                new Vector2(posX, posY) );

            // lockLA = new IHToggle("lalock", "Loot All Locked", "Loot All Unlocked", null, () => {return IHPlayer.laLocked;}, () =>
            Buttons[VAction.LA] =  new IHToggle("LA", "la", null, () => IHPlayer.ActionLocked(Main.localPlayer, VAction.LA), () => //onToggle
                {
                    Main.PlaySound(22, -1, -1, 1);
                    IHPlayer.ToggleActionLock(Main.localPlayer, VAction.LA);
                    Buttons[VAction.LA].Update();
                },
                new Vector2(posX, posY + (Main.inventoryBackTexture.Height * Main.inventoryScale)) );

        }

        protected override void OnDraw(SpriteBatch sb)
        {
            foreach (KeyValuePair<VAction, IHToggle> kvp in Buttons)
            {
                kvp.Value.Draw(sb);
            }

        }
    }
}
