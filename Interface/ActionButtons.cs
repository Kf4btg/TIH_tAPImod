using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class IHActionButtons : InterfaceLayer
    {
        public readonly Dictionary<IHAction, IHButton> Buttons;

        public IHActionButtons(ModBase mbase) : base("InvisibleHand:IHActionButtons")
        {
            // this should put the Buttons to the left of the chest inventory
            float posX = 73 - (Main.inventoryBackTexture.Width * Main.inventoryScale);
            float posY = API.main.invBottom + (Main.inventoryBackTexture.Height * Main.inventoryScale)/2;

            Buttons = new Dictionary<IHAction, IHButton>();

            Func<bool>[] checks = new Func<bool>[] { () =>  }
            ButtonState bs;
            bs.tint = Color.White;



            Buttons.Add(IHAction.Sort, new IHButton("Sort",
                mbase.textures["resources/btn_sort"],
                () =>
                {
                    if ( Main.localPlayer.chest == -1 ) // no valid chest open, sort player inventory
                        IHOrganizer.SortPlayerInv(Main.localPlayer, KState.Special.Shift.Down() ^ IHBase.ModOptions["ReverseSortPlayer"]);
                    else
                        IHOrganizer.SortChest(Main.localPlayer.chestItems, KState.Special.Shift.Down() ^ IHBase.ModOptions["ReverseSortChest"]);
                },
                new Vector2(posX, posY) ));

            // Buttons.Add(IHAction.SmartDep, new IHButton("Smart Deposit",
            //     mbase.textures["resources/btn_smartDeposit"],
            //     () =>
            //     {
            //         if ( Main.localPlayer.chest == -1 ) // no valid chest open, sort player inventory
            //             IHOrganizer.SortPlayerInv(Main.localPlayer, KState.Special.Shift.Down() ^ IHBase.ModOptions["ReverseSortPlayer"]);
            //         else
            //             IHOrganizer.SortChest(Main.localPlayer.chestItems, KState.Special.Shift.Down() ^ IHBase.ModOptions["ReverseSortChest"]);
            //     },
            //     new Vector2(posX, posY) ));
            //
            // Buttons.Add(IHAction.SmartDep, new IHButton("Quick Refill",
            //     mbase.textures["resources/btn_quickRefill"],
            //     () =>
            //     {
            //         if ( Main.localPlayer.chest == -1 ) // no valid chest open, sort player inventory
            //             IHOrganizer.SortPlayerInv(Main.localPlayer, KState.Special.Shift.Down() ^ IHBase.ModOptions["ReverseSortPlayer"]);
            //         else
            //             IHOrganizer.SortChest(Main.localPlayer.chestItems, KState.Special.Shift.Down() ^ IHBase.ModOptions["ReverseSortChest"]);
            //     },
            //     new Vector2(posX, posY) ));

            // lockQS = new IHToggle("qslock", "Quick Stack Locked", "Quick Stack Unlocked", null, () => {return IHPlayer.qsLocked;}, () =>
            // Buttons[VAction.QS] = new IHToggle("QS", "qs", null, () => IHPlayer.ActionLocked(Main.localPlayer, VAction.QS), () => //onToggle
            // Buttons[VAction.QS] = new IHToggle("Quick Stack Locked", "Quick Stack Unlocked", mbase.textures["resources/btn_quickstack"], () => IHPlayer.ActionLocked(Main.localPlayer, VAction.QS), () => //onToggle
            // {
            //     Main.PlaySound(22, -1, -1, 1);
            //     IHPlayer.ToggleActionLock(Main.localPlayer, VAction.QS);
            //     Buttons[VAction.QS].Update();
            // },
            // new Vector2(posX, (Main.inventoryBackTexture.Height * Main.inventoryScale) + posY) );

            // lockLA = new IHToggle("lalock", "Loot All Locked", "Loot All Unlocked", null, () => {return IHPlayer.laLocked;}, () =>
            // Buttons[VAction.LA] =  new IHToggle("LA", "la", null, () => IHPlayer.ActionLocked(Main.localPlayer, VAction.LA), () => //onToggle
            //     {
            //         Main.PlaySound(22, -1, -1, 1);
            //         IHPlayer.ToggleActionLock(Main.localPlayer, VAction.LA);
            //         Buttons[VAction.LA].Update();
            //     },
            //     new Vector2(posX, posY + (Main.inventoryBackTexture.Height * Main.inventoryScale * 2)) );
        }


        protected override void OnDraw(SpriteBatch sb)
        {
            foreach (KeyValuePair<IHAction, IHButton> kvp in Buttons)
            {
                kvp.Value.Draw(sb);
            }
        }

    }

    public class IHActionAlts : InterfaceLayer
    {

        public IHActionAlts(ModBase mbase) : base("InvisibleHand:IHActionAlts")
        {
            // this should put the Buttons to the left of the chest inventory
            float posX = 73 - (Main.inventoryBackTexture.Width * Main.inventoryScale);
            float posY = API.main.invBottom + (Main.inventoryBackTexture.Height * Main.inventoryScale)/2;

            Buttons = new Dictionary<IHAction, IHButton>();

    }
}
