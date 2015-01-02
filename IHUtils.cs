using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{

    public static class IHUtils
    {
        // !ref:Main:#22215.29##22643.29#
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
        // !ref:Main:#22314.0#
        */
        public static void DoDepositAll(Player player)
        {
            //this shouldn't happen if method is called correctly
            if (player.chest == -1) return;

            // if regular chest
            if (player.chest > -1)
            {
                for (int i=R_START; i >= R_END; i--) // iterate through the player's inventory in reverse
                {
                    if (!player.inventory[i].IsBlank()) MoveItemToChest(i, Main.ChestCoins, true);
                }
                Recipe.FindRecipes(); // !ref:Main:#22640.36#
                return;
            }
            //else banks
            for (int i=R_START; i >= R_END; i--)
            {
                if (!player.inventory[i].IsBlank()) MoveItemToChest(i, Main.BankCoins, false);
            }
            Recipe.FindRecipes(); // !ref:Main:#22640.36#

        } //\DoDepositAll()
    #endregion

    #region lootall
        /********************************************************
        *   DoLootAll
        */

        public static void DoLootAll(Player player)
        {
            //this shouldn't happen if method is called correctly
            if (player.chest == -1) return;

            LootAll(player, player.chestItems, player.chest >= 0);

            Recipe.FindRecipes(); // !ref:Main:#22640.36#
        } // \DoLootAll()

		/********************************************************
        *   LootAll
        *   !ref:Main:#22272.00#
        */
        private static void LootAll(Player player, Item[] container, bool sendMessage)
        {
            for (int i=0; i<Chest.maxItems; i++)
            {
                if (!container[i].IsBlank())
                {
                    container[i] = player.GetItem(player.whoAmI, container[i]);

                    // ok I have no idea what this does but it's part of the original
                    // loot-all code so I added it as well.
                    if (sendMessage) SendNetMessage(i);
                }}
        } // \LootAll()
    #endregion

#region quickstack
        /********************************************************
        *   DoQuickStack
        *   !ref:Main:#22476.44##22637.44#

            FIXME: If there's a stack of any type of coin in the destination
            container, ALL coins (regardless of type) will go to the container.
            Vanilla only stacks that type. (NOTE: this appears to be a TAPI bug.)
        */
        public static void DoQuickStack(Player player)
        {
            if (player.chest == -1) return;
            if (player.chest > -1 )
                QuickStack(player.inventory, player.chestItems, true, Main.ChestCoins);
            else
                QuickStack(player.inventory, player.chestItems, false, Main.BankCoins);

            Recipe.FindRecipes(); // !ref:Main:#22640.36#
        }//\DoQuickStack()

        private static void QuickStack(Item[] inventory, Item[] container, bool sendMessage, Action doCoins)
        {
            for (int iC = 0; iC < Chest.maxItems; iC++)                                         // go through entire chest inventory.
            {                                                                                   //if chest item is not blank && not a full stack, then
                if (!container[iC].IsBlank() && container[iC].stack < container[iC].maxStack)
                {                                                                               //for each item in inventory (including coins, ammo, hotbar),
                    for (int iP=0; iP<58; iP++)
                    {                                                                           //if chest item matches inv. item...
                        if (container[iC].IsTheSameAs(inventory[iP]))
                        {
                            RingBell();                                                         //...play "item-moved" sound and...
                                                                                                // ...merge inv. item stack to chest item stack
                            if (StackMerge(ref inventory[iP], ref container[iC], doCoins))
                            {                                                                   // do merge & check return (inv stack empty) status
                                inventory[iP] = new Item();                                     // reset slot if all inv stack moved
                            }
                            else if (container[iC].IsBlank())
                            {                                                                   // else, inv stack not empty after merge, but (because of DoCoins() call),
                                                                                                // chest stack could be.
                                container[iC] = inventory[iP].Clone();                          // move inv item to chest slot
                                inventory[iP] = new Item();                                     // and reset inv slot
                            }
                            if (sendMessage) SendNetMessage(iC);                                //send net message if regular chest
                        }
                    }
                }
            }
        }//\QuickStack()
#endregion

    #region helperfunctions
        /******************************************************
        *   MoveItem - moves a single item to a different container
        *    !ref:Main:#22320.00##22470.53#
        *    @param item : the item to move
        *    @param container: where to move @item
        *    @param doCoins: either Main.ChestCoins or Main.BankCoins
        *    @param desc : whether to move @item to end of @container rather than beginning
        *
        *    @return >=0 : index in @container where @item was placed/stacked
                      -1 : some stacked was moved, but some remains in @item
                      -2 : failed to move @item in any fashion (stack value unchanged)
                      -3 : @item was passed as blank
        */

        //for player->chest
        public static int MoveItemP2C(ref Item item, Item[] container, Action doCoins, bool sendMessage=true, bool desc = false)
        {
            if (item.stack<=0) return -3;

            int iStart; Func<int,bool> iCheck; Func<int,int> iNext;

            if (desc) { iStart = Chest.maxItems - 1; iCheck = i => i >= 0; iNext  = i => i-1; }
            else      { iStart = 0;      iCheck = i => i < Chest.maxItems; iNext  = i => i+1; }

            int j=-1;
            int stackB4 = item.stack;
            if (item.maxStack > 1)
            {   //search container for matching non-maxed stacks
                j=TryStackMerge(ref item, container, doCoins, sendMessage, iStart, iCheck, iNext);
            }
            if (j<0) //remaining stack or non-stackable
            {
                j=MoveToFirstEmpty(item, container, iStart, iCheck, iNext);
                if (j<0) //no empty slots
                {
                    return stackB4==item.stack ? -2 : -1; //exit status
                }
            }
            return j;
        }

        /********************************************************
        *   Helper Helpers?
        */
        // @return: >=0 if move succeeded; -1 if failed
        public static int MoveToFirstEmpty(Item item, Item[] dest, int iStart, Func<int, bool> iCheck, Func<int,int> iNext)
        {
            for (int i=iStart; iCheck(i); i=iNext(i)) //!ref:Main:#22416.00#
            {
                if (dest[i].IsBlank())
                {
                    dest[i] = item.Clone();
                    return i; //return index of destination slot
                }
            }
            return -1;
        }

        // @return: >=0 if entire stack moved; -1 if failed to move or some remains
        public static int TryStackMerge(ref Item item, Item[] dest, Action doCoins, bool sendMessage, int iStart, Func<int, bool> iCheck, Func<int,int> iNext)
        {
            //search inventory for matching non-maxed stacks
            for (int i=iStart; iCheck(i); i=iNext(i))
            {
                Item item2 = dest[i];
                // found a non-empty slot containing a < full stack of the same item type
                if (!item2.IsBlank() && item2.IsTheSameAs(item) && item2.stack < item2.maxStack)
                {
                    if (StackMerge(ref item, ref item2, doCoins)) return i;  //if item's stack was reduced to 0
                    if (dest[i].IsBlank())  //now check container slot to see if doCoins emptied it
                    {
                        dest[i] = item.Clone(); // move inv item to chest slot
                        return i;  // return index to indicate that item slot should be reset
                    }
                    if (sendMessage) SendNetMessage(i); //still have to send this apparently
                }
            } // if we don't return in this loop, there is still some stack remaining
            return -1;
        }

        /********************************************************
        *   MoveItemToChest
            @param iPlayer : index of the item in source
            @param doCoins: either Main.ChestCoins or Main.BankCoins
            @param sendMessage : should ==true if regular chest, false for banks
            @param desc : whether to place item towards end of @dest rather than beginning

            @return : True if item was (entirely) removed from source; otherwise false.
        */

        // player main inventory->chest/bank
        public static bool MoveItemToChest(int iPlayer, Action doCoins, bool sendMessage, bool desc = false)
        {
            int retIdx = MoveItemP2C (
            ref Main.localPlayer.inventory[iPlayer],                        // item in inventory
            Main.localPlayer.chestItems,                                    // destination container
            doCoins,                                                        // chest or bank doCoins
            sendMessage,                                                    // if true, sendMessage
            desc);                                                          // check container indices descending?

            if (retIdx > -2) // >=partial success
            {
                RingBell();
                if (retIdx > -1) // =full success
                {
                    Main.localPlayer.inventory[iPlayer] = new Item();
                    if (sendMessage) SendNetMessage(retIdx);
                    return true;
                }
            }
            return false;
        }

        /******************************************************
        // Moves as much of itemSrc.stack to itemDest.stack as possible.
        // Returns true if itemSrc.stack is reduced to 0; false otherwise.
        // Does not check for item equality or existence of passed items;
        // that must be ensured by the calling method.
        //
        // @param doCoins: either Main.ChestCoins 0or Main.BankCoins
                            (or neither, if absent)
        */
        public static bool StackMerge(ref Item itemSrc, ref Item itemDest)
        {
            int diff = Math.Min(itemDest.maxStack - itemDest.stack, itemSrc.stack);
            itemDest.stack += diff;
            itemSrc.stack  -= diff;
            // return true to indicate stack has been emptied
            return itemSrc.IsBlank();
        }

        public static bool StackMerge(ref Item itemSrc, ref Item itemDest, Action doCoins )
        {
            int diff = Math.Min(itemDest.maxStack - itemDest.stack, itemSrc.stack);
            itemDest.stack += diff;
            itemSrc.stack  -= diff;
            doCoins();
            // return true to indicate stack has been emptied
            return itemSrc.IsBlank();
        }

        //this one returns the amount transferred
        public static int StackMergeD(ref Item itemSrc, ref Item itemDest)
        {
            int diff = Math.Min(itemDest.maxStack - itemDest.stack, itemSrc.stack);
            itemDest.stack += diff;
            itemSrc.stack  -= diff;

            return diff;
        }

        //plays the "item moved" sound
        public static void RingBell(int o1 = -1, int o2 = -1, int o3 = 1)
        {
            Main.PlaySound(7, o1, o2, o3);
        }

        // calls the NetMessage.sendData method for the current chest
        // at the given index. Called on all of loot, deposit, stack
        public static void SendNetMessage(int index)
        {
            if (Main.netMode == 1)
            {
                NetMessage.SendData(32, -1, -1, "", Main.localPlayer.chest, (float)index, 0, 0, 0);
            }
        }
    #endregion

    #region oldcode

        // public static bool GetItem(Item item, int itemIndex)
        // {
        //     //...check every item in chest...
        //     Item[] chestItems = Main.localPlayer.chestItems;
        //     // quit if we max out this stack or reach the end of the chest;
        //     // also note that the DoCoins() call may reduce this stack to 0, so check that too
        //     int j=-1;
        //     while ( item.stack<item.maxStack && ++j<Chest.maxItems && item.stack>0 )
        //     {   //...for a matching item stack...
        //         if (GetItemStackChecker(chestItems, j, ref item, itemIndex, Main.localPlayer.chest>-1)) break;
        //
        //     }// </while>
        // }
        //
        // public static bool GetItemStackChecker(Item[] chestItems, int cIndex, ref Item pItem, int pIndex, bool sendMessage)
        // {
        //     if (!chestItems[cIndex].IsBlank() && chestItems[cIndex].IsTheSameAs(pItem))
        //     {
        //         RingBell();
        //         //...and merge it to the Player's inventory
        //
        //         // I *think* this ItemText.NewText command just makes the text pulse...
        //         // I don't entirely grok how it works, but included for parity w/ vanilla
        //         ItemText.NewText(chestItems[cIndex], StackMergeD(ref chestItems[cIndex], ref pItem));
        //         Main.localPlayer.DoCoins(pIndex);
        //         if (chestItems[cIndex].stack<=0)
        //         {
        //             chestItems[cIndex] = new Item(); //reset this item if all stack transferred
        //             if (sendMessage) SendNetMessage(cIndex); //only for non-bank chest
        //             return true;
        //         }
        //         if (sendMessage) SendNetMessage(cIndex);
        //         return false;
        //     }
        // }

    #endregion


    }// \class
}
