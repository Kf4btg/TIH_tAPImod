using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{

    public static class IHUtils
    {
        // The methods here are pretty much just cleaned up and somewhat
        // refactored versions of the vanilla code. It really surprised
        // me that these weren't given function-calls in the original code...

        // Overall, these don't interact or rely much on the rest of the mod.
        // TODO: make them recognize locked slots in some fashion.

#region depositall
        private const int R_START=49;   //start from end of main inventory
        private const int R_END=10;     //don't include hotbar

        /********************************************************
        *   DoDepositAll
        */
        public static void DoDepositAll(Player player)
        {
            //this shouldn't happen if method is called correctly
            if (player.chest == -1) return;

            //flag for playing the sound
            bool moveSuccess = false;

            //regular chest
            if (player.chest >= 0)
            {
                // iterate through the player's inventory in reverse
                for (int i=R_START; i >= R_END; i--)
                {
                    // returned index
                    int retIdx = MoveItem(ref player.inventory[i], Main.chest[player.chest].item);
                    if (retIdx >= 0 ) // item/entire stack successfully moved
                    {
                        player.inventory[i] = new Item();
                        if (!moveSuccess) moveSuccess=true; //toggle success flag

                        SendNetMessage(retIdx); //only for non-bank chest
                    }
                }//\inv iteration

                Main.ChestCoins();
            }

            // one of the banks
            else
            {
                for (int i=R_START; i >= R_END; i--)
                {
                    if (MoveItem(ref player.inventory[i],
                                player.chest == -3 ? player.bank2.item : player.bank.item) >= 0 )
                    {
                        player.inventory[i] = new Item();
                        if (!moveSuccess) moveSuccess=true; //toggle success flag
                    }
                }//\inv iteration

                Main.BankCoins();

            } //\depositing items

            // play sound if deposit was at least somewhat successful
            if (moveSuccess) {
                RingBell(); }

        } //\DoDepositAll()
#endregion

#region lootall
        /********************************************************
        *   DoLootAll
        */
        public static void DoLootAll(Player player)
        {
            //this shouldn't happen if method is called correctly
            if (player.chest == -1) {return;}

            //regular chest
            if (player.chest >= 0 )
            {
                LootAll(player, Main.chest[player.chest].item, true);
            }
            else //should be one of the banks
            {
                LootAll(player, player.chest == -3 ? player.bank2.item : player.bank.item);
            }
        } // \DoLootAll()

        private static void LootAll(Player player, Item[] container, bool sendMessage = false)
        {
            for (int i=0; i<Chest.maxItems; i++)
            {
                if (!container[i].IsBlank())
                {
                    container[i] = player.GetItem(player.whoAmI, container[i]);

                    // ok I have no idea what this does but it's part of the original
                    // loot-all code so I added it as well.
                    if (sendMessage) SendNetMessage(i);
                }
            }
        } // \LootAll()
#endregion

#region quickstack
        /********************************************************
        *   DoQuickStack
        */
        public static void DoQuickStack(Player player)
        {
            //regular chest
            if (player.chest >= 0)
            {
                QuickStack(player, Main.chest[player.chest].item, true);
                Main.ChestCoins();
            }
            else
            {  //do the same for the banks
                QuickStack(player, player.chest == -3 ? player.bank2.item : player.bank.item);
                Main.BankCoins();
            }
        }//\DoQuickStack()

        private static void QuickStack(Player player, Item[] container, bool sendMessage = false)
        {

            for (int i=0; i<Chest.maxItems; i++)
            {
                //if chest item is not blank && not a full stack:
                if (!container[i].IsBlank() && container[i].stack < container[i].maxStack)
                {
                    //for each item in inventory (including coins, ammo, hotbar):
                    for (int j=0; j<58; j++)
                    {
                        //if chest item matches inv. item:
                        if (container[i].IsTheSameAs(player.inventory[j]))
                        {
                            // merge inv. item stack to chest item stack
                            if (StackMerge(ref player.inventory[j], ref container[i]))
                            {
                                player.inventory[j] = new Item();

                                RingBell();
                                if (sendMessage) SendNetMessage(i);
                            }
                        }
                    }
                }
            }
        }//\QuickStack()
#endregion

#region helperfunctions
        /******************************************************
        *   MoveItem - moves a single item to a different container
        *
        *   @return >=0 : index in container where item was placed/stacked
        *            -1 : failed to move item or some stack remains
        *            -2 : item passed was blank
        */
        public static int MoveItem(ref Item item, Item[] container)
        {
            return MoveItem(ref item, container, 0, container.Length -1);
        }

        public static int MoveItem(ref Item item, Item[] container, int rangeStart, int rangeEnd)
        {
            if (item.IsBlank()) return -2;

            //search container for matching non-maxed stacks
            for (int j=rangeStart; j<=rangeEnd; j++)
            {
                Item item2 = container[j];

                // found a non-empty slot containing a <full stack of the same item type
                if (!item2.IsBlank() && item2.IsTheSameAs(item) && item2.stack < item2.maxStack)
                {
                    if (StackMerge(ref item, ref item2))
                    {
                        // item = new Item();
                        return j;  //if item's stack was reduced to 0
                    }
                }

                //move remainder of stack to first empty slot
                for (int k=rangeStart; k<=rangeEnd; k++)
                {
                    if (container[k].IsBlank())
                    {
                        container[k] = item.Clone();
                        // item = new Item();
                        return k;
                    }
                }
            }

            //unable to move item/entire stack to container
            return -1;

        }//\MoveItem()

        // Moves as much of itemSrc.stack to itemDest.stack as possible.
        // Returns true if itemSrc.stack is reduced to 0; false otherwise.
        // Does not check for item equality or existence of passed items;
        // that must be ensured by the calling method.
        public static bool StackMerge(ref Item itemSrc, ref Item itemDest)
        {
            int diff = Math.Min(itemDest.maxStack - itemDest.stack, itemSrc.stack);
            itemDest.stack += diff;
            itemSrc.stack  -= diff;

            // return true to indicate stack has been emptied
            return itemSrc.IsBlank();
        }

        //plays the "item moved" sound
        public static void RingBell()
        {
            Main.PlaySound(7, -1, -1, 1);
        }

        // calls the NetMessage.sendData method for the current chest
        // at the given index.
        public static void SendNetMessage(int index)
        {
            if (Main.netMode == 1)
            {
                NetMessage.SendData(32, -1, -1, "", Main.localPlayer.chest, (float)index, 0, 0, 0);
            }

        }
#endregion

    }// \class

}
