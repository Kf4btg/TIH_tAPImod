using System;
using Microsoft.Xna.Framework;
using TAPI.UIKit;
using Terraria;

namespace InvisibleHand
{

    public static class IHUtils
    {
        // !ref:Main:#22215.29##22643.29#
        // The methods here are pretty much just cleaned up and somewhat
        // refactored versions of the vanilla code (which did not wrap
        // these functionalities in method-calls, but rather placed
        // them inline with the DrawInventory code).


        //------------------------------------------------------//
        //------------CALLABLES FOR VANILLA ACTIONS-------------//
        //------------------------------------------------------//
        //  Call these methods to perform the action(s)         //
        //  associated with their vanilla counterparts.         //
        //------------------------------------------------------//
        //  Members:                                            //
        //      DoDepositAll()                                  //
        //      DoLootAll()                                     //
        //      DoQuickStack()                                  //
        //------------------------------------------------------//

    #region depositall
        private const int R_START = 49;   //start from end of main inventory
        private const int R_END   = 10;     //don't include hotbar

        /********************************************************
        *   DoDepositAll
        // !ref:Main:#22314.0#
        */
        public static void DoDepositAll()
        {
            DoDepositAll(Main.localPlayer);
        }
        public static void DoDepositAll(Player player)
        {
            //this shouldn't happen if method is called correctly
            if (player.chest == -1) return;
            bool sendNetMsg = player.chest > -1;

            if (IHPlayer.ActionLocked(player, TIH.DepAll))
            {
                for (int i=R_START; i >= R_END; i--)
                {
                    if (IHPlayer.SlotLocked(player, i) ||  player.inventory[i].IsBlank()) continue;
                    MoveItemToChest(i, sendNetMsg);
                }
                Recipe.FindRecipes(); // !ref:Main:#22640.36#
                return;
            }

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
        public static void DoLootAll()
        {
            DoLootAll(Main.localPlayer);
        }
        public static void DoLootAll(Player player)
        {
            //this shouldn't happen if method is called correctly
            if (player.chest == -1) return;

            var sendNetMsg = player.chest > -1;
            var container = player.chestItems;

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
        }
    #endregion

    #region quickstack
        /********************************************************
        *   DoQuickStack
        *   !ref:Main:#22476.44##22637.44#
        */
        public static void DoQuickStack()
        {
            DoQuickStack(Main.localPlayer);
        }
        public static void DoQuickStack(Player player)
        {
            if (player.chest == -1) return;

            var inventory = player.inventory;
            var container = player.chestItems;
            bool sendMessage = player.chest > -1;
            var checkLocks  = IHPlayer.ActionLocked(player, TIH.QuickStack);  //boolean


            for (int iC = 0; iC < Chest.maxItems; iC++)                                         // go through entire chest inventory.
            {                                                                                   //if chest item is not blank && not a full stack, then
                if (!container[iC].IsBlank() && container[iC].stack < container[iC].maxStack)
                {                                                                               //for each item in inventory (including coins, ammo, hotbar),
                    for (int iP=0; iP<58; iP++)
                    {
                        if (checkLocks && IHPlayer.SlotLocked(player, iP)) continue;            // if we're checking locks ignore the locked ones

                        if (container[iC].IsTheSameAs(inventory[iP]))                           //if chest item matches inv. item...
                        {
                            // RingBell();                                                       //...play "item-moved" sound and...
                            Sound.ItemMoved.Play();
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
            Recipe.FindRecipes(); // !ref:Main:#22640.36#
        }
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
        *   move item from player inventory slot to chest
        *   returns true if item moved/itemstack emptied
        */
        public static bool ShiftToChest(ref ItemSlot slot)
        {
            bool sendMessage = Main.localPlayer.chest > -1;

            var pItem = slot.MyItem;

            int retIdx = -1;
            if (pItem.stack == pItem.maxStack) //move non-stackable items or full stacks to empty slot.
            {
                retIdx = MoveToFirstEmpty( pItem, Main.localPlayer.chestItems, 0,
                                            i => i<Chest.maxItems,
                                            i => i+1
                                        );
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
                        // RingBell();
                        Sound.ItemMoved.Play();
                        Recipe.FindRecipes();
                    }
                    return false;
                }
            }
            //else, success!
            // RingBell();
            Sound.ItemMoved.Play();
            slot.MyItem = new Item();
            if (sendMessage) SendNetMessage(retIdx);
            return true;
        }

        // MoveChestSlotItem - moves item from chest/guide slot to player inventory
        public static bool ShiftToPlayer(ref ItemSlot slot, bool sendMessage)
        {
            //TODO: check for quest fish (item.uniqueStack && player.HasItem(item.type))
            var cItem = slot.MyItem;

            if (cItem.IsBlank()) return false;

            if (cItem.Matches(ItemCat.COIN)) {
                // don't bother with "shifting", just move it as usual
                slot.MyItem = Main.localPlayer.GetItem(Main.myPlayer, slot.MyItem);
                return (slot.MyItem.IsBlank());
            }

            // reminder: ShiftToPlayer returns true if original item ends up empty
            if (cItem.Matches(ItemCat.AMMO)) {
                if (cItem.maxStack > 1
                && cItem.stack==cItem.maxStack
                && ShiftToPlayer(ref slot, 54, 57, sendMessage, false)) //ammo goes top-to-bottom
                    return true;
            }

            // if it's a stackable item and the stack is *full*, just shift it.
            else if (cItem.maxStack > 1 && cItem.stack==cItem.maxStack){
                if (ShiftToPlayer(ref slot,  0,  9, sendMessage, false) //try hotbar first, ascending order (vanilla parity)
                ||  ShiftToPlayer(ref slot, 10, 49, sendMessage,  true)) return true; //then the other slots, descending
            }

            //if all of the above failed, then we have no empty slots.
            // Let's save some work and go traditional:
            slot.MyItem = Main.localPlayer.GetItem(Main.myPlayer, slot.MyItem);
            return (slot.MyItem.IsBlank());
        }

        // attempts to move an item to an empty slot (returns success status)
        private static bool ShiftToPlayer(ref ItemSlot slot, int ixStart, int ixStop, bool sendMessage, bool desc)
        {
            int iStart;
            Func<int,bool> iCheck;
            Func<int,int> iNext;

            if (desc) {
                iStart =  ixStop;
                iCheck = i => i >= ixStart;
                iNext = i => i-1;
            }
            else {
                iStart = ixStart;
                iCheck = i => i <=  ixStop;
                iNext = i => i+1;
            }

            int retIdx = MoveToFirstEmpty( slot.MyItem, Main.localPlayer.inventory, iStart, iCheck, iNext );
            if (retIdx >= 0)
            {
                // RingBell();
                Sound.ItemMoved.Play();
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
        *    @param iPlayer : index of the item in inventory
        *    @param sendMessage : should ==true if regular chest, false for banks
        *    @param desc : whether to place item towards end of chest rather than beginning
        *
        *    @return : True if item was (entirely) removed from inventory; otherwise false.
        */
        public static bool MoveItemToChest(int iPlayer, bool sendMessage, bool desc = false)
        {
            int retIdx = MoveItemP2C (
            ref Main.localPlayer.inventory[iPlayer],    // item in inventory
            Main.localPlayer.chestItems,                // destination container
            sendMessage,                                // if true, sendMessage
            desc);                                      // check container indices descending?

            if (retIdx > -2) // >=partial success
            {
                // RingBell();
                Sound.ItemMoved.Play();
                if (retIdx > -1) // =full success!
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
        *             -1 : some stack was moved, but some still remains in @item
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
        *   Helper Helpers? - functions used in the above helper methods
        */

        // @return: >=0 (index) if move succeeded; -1 if failed
        /// <summary>
        /// Will move the given item into a destination array.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="dest"></param>
        /// <param name="iStart"></param>
        /// <param name="iCheck"></param>
        /// <param name="iNext"></param>
        /// <returns>bigger than -1 if move succeeded; -1 if failed</returns>
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

        // @return: >=0 (index) if entire stack moved; -1 if failed to move or some remains
        public static int TryStackMerge(ref Item item, Item[] dest, bool sendMessage, int iStart, Func<int, bool> iCheck, Func<int,int> iNext)
        {
            //search inventory for matching non-maxed stacks
            for (int i=iStart; iCheck(i); i=iNext(i))
            {
                var item2 = dest[i];
                // found a non-empty slot containing a not-full stack of the same item type
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
            } // if we manage to exit this loop, there is still some stack remaining:
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
            // return true if stack has been emptied
            return itemSrc.IsBlank();
        }

        // takes the entire destination container as a parameter so as to check coin stacks
        public static bool StackMerge(ref Item itemSrc, Item[] dest, int dIndex )
        {
            int diff = Math.Min(dest[dIndex].maxStack - dest[dIndex].stack, itemSrc.stack);
            dest[dIndex].stack += diff;
            itemSrc.stack  -= diff;
            DoContainerCoins(dest, dIndex);
            // return true if stack has been emptied
            return itemSrc.IsBlank();
        }

        // as the first StackMerge above, but returns the amount transferred
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
            // stack contains 100 items && item matches copper/silver/gold item-type id
            if (container[i].stack == 100 && (container[i].type == 71 || container[i].type == 72 || container[i].type == 73))
            {
                container[i].SetDefaults(container[i].type + 1); //replace stack with 1 coin of next-higher type
                for (int j = 0; j < container.Length; j++)
                {   // search the rest of the container for more coins of this type and move it there if found.
                    if (container[j].IsTheSameAs(container[i]) && j != i && container[j].type == container[i].type && container[j].stack < container[j].maxStack)
                    {
                        container[j].stack++;
                        container[i] = new Item();
                        DoContainerCoins(container, j); //recursive call as move may have maxed new stack
                    }
                }
            }
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

    #region gui_stuff

        /* ************************************************
        *	 Unrelated to any of the above, these methods assist in the GUI/buttons part of the mod.
        */

        /// get source texels based on an index (defined in Constants class).
        /// the "+2" is due to the 2-pixel gap between the button faces in the grid png
        public static Rectangle? RectFromGridIndex(int gIndex, bool active=false)
        {
            return active ?
                new Rectangle(Constants.ButtonW+2,(Constants.ButtonH+2)*gIndex,Constants.ButtonW, Constants.ButtonH) : //mouse-over
                new Rectangle(0, (Constants.ButtonH+2)*gIndex,Constants.ButtonW, Constants.ButtonH); //inactive
        }

        ///Get source Texels for the button based what @action it performs
        /// @param action
        /// @param active - false = default/inactive button appearance;
        ///                  true = focused/mouseover/active appearance
        public static Rectangle? GetSourceRect(TIH action, bool active=false)
        {
            return RectFromGridIndex( Constants.ButtonGridIndexByActionType[action], active );
        }

        /// returns the key-bind (as a string) for the button with the given action.
        /// return value will be something like "(X)"
        public static string GetKeyTip(TIH action)
        {
            string kbopt;
            if (Constants.ButtonActionToKeyBindOption.TryGetValue(action, out kbopt))
                return IHBase.ButtonKeyTips[kbopt];

            return "";
        }

        public static void AddToButtonStore(ICoreButton btn)
        {
            IHBase.Instance.ButtonStore.Add(btn.ID, btn);
        }
    #endregion

    }// \class
}
