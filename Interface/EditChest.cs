using System;
using Terraria;

namespace InvisibleHand
{
    public static class EditChest
    {
        /// Edit a chest's label. Essentially copied verbatim from vanilla code.
        /// Luckily most everything in it is a static variable.
        /// Can be called for either save or rename buttons
        /// and will perform the appropriate action.
        public static void DoChestEdit()
        {
            if (!Main.editChest)  //enter rename phase
            {
                Main.npcChatText = Main.chest[Main.localPlayer.chest].name;
                Main.defaultChestName = Lang.chestType [ (int)(
                            Main.tile[ Main.localPlayer.chestX, Main.localPlayer.chestY ].frameX / 36
                        ) ];
                if (Main.npcChatText == "")
                {
                    Main.npcChatText = Main.defaultChestName;
                }
                Main.editChest = true;
                Main.clrInput();
            }
            else  //save name
            {
                // even though this happens on click, vanilla code
                // specifies the mouse-over sound to play.
                Sound.MouseOver.Play();
                Main.editChest = false;
                int current = Main.localPlayer.chest;
                if (Main.npcChatText == Main.defaultChestName)
                {
                    Main.npcChatText = "";
                }
                if (Main.chest[current].name != Main.npcChatText)
                {
                    Main.chest[current].name = Main.npcChatText;
                    if (Main.netMode == 1)
                    {
                        Main.localPlayer.editedChestName = true;
                    }
                }
            }
        }

        // public static void CancelRename(ButtonLayer container, TIH defaultAction)
        public static void CancelRename()
        {
            Sound.MouseOver.Play();
            Main.editChest = false;
		    Main.npcChatText = string.Empty;

            // container.Buttons[defaultAction].Reset();
        }
    }
}
