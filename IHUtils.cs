using System;
using TAPI;
using TAPI.UIKit;
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

        //------------------------------------------------------//
        //-----------CALLABLES FOR VANILLA FUNCTIONS------------//
        //------------------------------------------------------//
        //  Call these methods to perform the action(s)         //
        //  associated with their GUI-dependent vanilla         //
        //  counterparts.                                       //
        //------------------------------------------------------//
        //  Members:                                            //
        //      DoDepositAll(Player)                            //
        //      DoLootAll(Player)                               //
        //      DoQuickStack(Player)                            //
        //------------------------------------------------------//

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
            bool sendNetMsg = player.chest > -1;

            for (int i=R_START; i >= R_END; i--)
            {
                if (!player.inventory[i].IsBlank()) MoveItemToChest(i, sendNetMsg);
            }
            Recipe.FindRecipes(); // !ref:Main:#22640.36#

        } //\DoDepositAll()
    #endregion

    #region lootall
        /********************************************************
        *   DoLootAll !ref:Main:#22272.00#
        */
        public static void DoLootAll(Player player)
        {
            //this shouldn't happen if method is called correctly
            if (player.chest == -1) return;

            bool sendNetMsg = player.chest > -1;
            Item[] container = player.chestItems;

            for (int i=0; i<Chest.maxItems; i++)
            {
                if (!container[i].IsBlank())
                {
                    container[i] = player.GetItem(player.whoAmI, container[i]);

                    // ok I have no idea what this does but it's part of the original
                    // loot-all code so I added it as well.
                    if (sendNetMsg) SendNetMessage(i);
                }
            }
            Recipe.FindRecipes(); // !ref:Main:#22640.36#
        } // \DoLootAll()
    #endregion

    #region quickstack
        /********************************************************
        *   DoQuickStack
        *   !ref:Main:#22476.44##22637.44#
        */
        public static void DoQuickStack(Player player)
        {
            if (player.chest == -1) return;
            QuickStack(player.inventory, player.chestItems,  player.chest > -1);

            Recipe.FindRecipes(); // !ref:Main:#22640.36#
        }//\DoQuickStack()

        private static void QuickStack(Item[] inventory, Item[] container, bool sendMessage)
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
                            if (StackMerge(ref inventory[iP], container, iC))
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

        //------------------------------------------------------//
        //-----------CALLABLES FOR ITEM SLOT ACTIONS------------//
        //------------------------------------------------------//
        //  These methods are intended for use when             //
        //  "shifting" the item contained in an ItemSlot to a   //
        //  different container, as via the capability bound to //
        //  Shift+Right-click in IHInterface.                   //
        //------------------------------------------------------//
        //  Members:                                            //
        //    bool ShiftToChest(ref ItemSlot)                   //
        //    bool ShiftToPlayer(ref ItemSlot, bool)            //
        //    bool ShiftToPlayer(ref ItemSlot, int, int,        //
        //          bool, bool)                                 //
        //------------------------------------------------------//

        /**************************************************************
        *   returns true if item moved/itemstack emptied
        */

        // move item from player inventory slot to chest
        public static bool ShiftToChest(ref ItemSlot slot)
        {
            bool sendMessage = Main.localPlayer.chest > -1;

            Item pItem = slot.MyItem;

            int retIdx = -1;
            if (pItem.stack == pItem.maxStack) //move non-stackable items or full stacks to empty slot.
            {
                retIdx = MoveToFirstEmpty( pItem, Main.localPlayer.chestItems, 0,
                new Func<int,bool>( i => i<Chest.maxItems ),
                new Func<int,int> ( i => i+1 ) );
            }

            // if we didn't find an empty slot...
            if (retIdx < 0)
            {
                if (pItem.maxStack == 1) return false; //we can't stack it, so we already know there's no place for it.

                retIdx = MoveItemP2C(ref pItem, Main.localPlayer.chestItems, sendMessage);

                if (retIdx < 0)
                {
                    if (retIdx == -1)  // partial success (stack amt changed), but we don't want to reset the item.
                    {
                        RingBell();
                        Recipe.FindRecipes();
                    }
                    return false;
                }
            }
            //else, success!
            RingBell();
            slot.MyItem = new Item();
            if (sendMessage) SendNetMessage(retIdx);
            return true;
        }

        // MoveChestSlotItem - moves item from chest/guide slot to player inventory
        public static bool ShiftToPlayer(ref ItemSlot slot, bool sendMessage)
        {
            //TODO: check for quest fish (item.uniqueStack && player.HasItem(item.type))
            Item cItem = slot.MyItem;

            if (cItem.IsBlank()) return false;

            if (cItem.Matches(ItemCat.COIN)) {
                // don't bother with "shifting", just move it as usual
                slot.MyItem = Main.localPlayer.GetItem(Main.myPlayer, slot.MyItem);
                return (slot.MyItem.IsBlank());
            }

            // ShiftToPlayer returns true if original item ends up empty
            if (cItem.Matches(ItemCat.AMMO)) {
                //ammo goes top-to-bottom
                if (cItem.maxStack > 1 && cItem.stack==cItem.maxStack && ShiftToPlayer(ref slot, 54, 57, sendMessage, false)) return true;
            }

            // if it's a stackable item and the stack is *full*, just shift it.
            else if (cItem.maxStack > 1 && cItem.stack==cItem.maxStack){
                if (ShiftToPlayer(ref slot,  0,  9, sendMessage, false) //try hotbar first, ascending order (vanilla parity)
                ||  ShiftToPlayer(ref slot, 10, 49, sendMessage,  true)) return true; //the other slots, descending
            }

            //if all of the above failed, then we have no empty slots.
            // Let's save some work and get traditional:
            slot.MyItem = Main.localPlayer.GetItem(Main.myPlayer, slot.MyItem);
            return (slot.MyItem.IsBlank());
        }

        // attempts to move an item to an empty slot
        private static bool ShiftToPlayer(ref ItemSlot slot, int ixStart, int ixStop, bool sendMessage, bool desc)
        {
            int iStart; Func<int,bool> iCheck; Func<int,int> iNext;

            if (desc) { iStart =  ixStop; iCheck = i => i >= ixStart; iNext = i => i-1; }
            else      { iStart = ixStart; iCheck = i => i <=  ixStop; iNext = i => i+1; }

            int retIdx = MoveToFirstEmpty( slot.MyItem, Main.localPlayer.inventory, iStart, iCheck, iNext );
            if (retIdx >= 0)
            {
                RingBell();
                slot.MyItem = new Item();
                if (sendMessage) SendNetMessage(retIdx);
                return true;
            }
            return false;
        }


    #region helperfunctions

        //------------------------------------------------------//
        //--------------------HELPER METHODS--------------------//
        //------------------------------------------------------//
        //  Common pieces of the callables broken down into     //
        //  smaller functions.                                  //
        //------------------------------------------------------//
        //  Members:                                            //
        //    int MoveItemP2C(ref Item, Item[], bool, bool)     //
        //    int MoveToFirstEmpty(Item, Item[], int,           //
        //          Func<int,bool>, Func<bool,bool>)            //
        //    int TryStackMerge(ref Item, Item[], bool, int,    //
        //          Func<int,bool>, Func<bool,bool>)            //
        //------------------------------------------------------//

        /********************************************************
        *   MoveItemToChest
        @param iPlayer : index of the item in inventory
        @param sendMessage : should ==true if regular chest, false for banks
        @param desc : whether to place item towards end of chest rather than beginning

        @return : True if item was (entirely) removed from source; otherwise false.
        */

        // player main inventory->chest/bank
        public static bool MoveItemToChest(int iPlayer, bool sendMessage, bool desc = false)
        {
            int retIdx = MoveItemP2C (
            ref Main.localPlayer.inventory[iPlayer],    // item in inventory
            Main.localPlayer.chestItems,                // destination container
            sendMessage,                                // if true, sendMessage
            desc);                                      // check container indices descending?

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
        *   MoveItem - moves a single item to a different container
        *    !ref:Main:#22320.00##22470.53#
        *    @param item : the item to move
        *    @param container: where to move @item
        *    @param desc : whether to move @item to end of @container rather than beginning
        *
        *    @return >=0 : index in @container where @item was placed/stacked
        *             -1 : some stacked was moved, but some remains in @item
        *             -2 : failed to move @item in any fashion (stack value unchanged)
        *             -3 : @item was passed as blank
        */

        //for player->chest
        public static int MoveItemP2C(ref Item item, Item[] container, bool sendMessage=true, bool desc = false)
        {
            if (item.stack<=0) return -3;

            int iStart; Func<int,bool> iCheck; Func<int,int> iNext;

            if (desc) { iStart = Chest.maxItems - 1; iCheck = i => i >= 0; iNext  = i => i-1; }
            else      { iStart = 0;      iCheck = i => i < Chest.maxItems; iNext  = i => i+1; }

            int j=-1;
            int stackB4 = item.stack;
            if (item.maxStack > 1) //search container for matching non-maxed stacks
                j = TryStackMerge(ref item, container, sendMessage, iStart, iCheck, iNext);

            if (j<0) //remaining stack or non-stackable
            {
                j = MoveToFirstEmpty(item, container, iStart, iCheck, iNext);

                if (j<0) //no empty slots
                    return stackB4==item.stack ? -2 : -1; //exit status
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
        public static int TryStackMerge(ref Item item, Item[] dest, bool sendMessage, int iStart, Func<int, bool> iCheck, Func<int,int> iNext)
        {
            //search inventory for matching non-maxed stacks
            for (int i=iStart; iCheck(i); i=iNext(i))
            {
                Item item2 = dest[i];
                // found a non-empty slot containing a < full stack of the same item type
                if (!item2.IsBlank() && item2.IsTheSameAs(item) && item2.stack < item2.maxStack)
                {
                    if (StackMerge(ref item, dest, i)) return i;  //if item's stack was reduced to 0
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



        /******************************************************
        // Moves as much of itemSrc.stack to itemDest.stack as possible.
        // Returns true if itemSrc.stack is reduced to 0; false otherwise.
        // Does not check for item equality or existence of passed items;
        // that must be ensured by the calling method.
        */
        public static bool StackMerge(ref Item itemSrc, ref Item itemDest)
        {
            int diff = Math.Min(itemDest.maxStack - itemDest.stack, itemSrc.stack);
            itemDest.stack += diff;
            itemSrc.stack  -= diff;
            // return true to indicate stack has been emptied
            return itemSrc.IsBlank();
        }

        public static bool StackMerge(ref Item itemSrc, Item[] dest, int dIndex )
        {
            int diff = Math.Min(dest[dIndex].maxStack - dest[dIndex].stack, itemSrc.stack);
            dest[dIndex].stack += diff;
            itemSrc.stack  -= diff;
            DoContainerCoins(dest, dIndex);
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

        /**********************************************************
        *   DoContainerCoins
        *
        *   Adapted from the Player.DoCoins(int i) method because Main.MoveCoins() wasn't doing what I wanted.
        */
        public static void DoContainerCoins(Item[] container, int i)
        {
            if (container[i].stack == 100 && (container[i].type == 71 || container[i].type == 72 || container[i].type == 73))
            {
                container[i].SetDefaults(container[i].type + 1);
                for (int j = 0; j < container.Length; j++)
                {
                    if (container[j].IsTheSameAs(container[i]) && j != i && container[j].type == container[i].type && container[j].stack < container[j].maxStack)
                    {
                        container[j].stack++;
                        container[i] = new Item();
                        DoContainerCoins(container, j);
                    }
                }
            }
        }

        /**********************************************************
        *   Wrapper functions
        */
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

    }// \class
}
