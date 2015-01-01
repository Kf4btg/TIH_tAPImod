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

            // iterate through the player's inventory in reverse
            for (int i=R_START; i >= R_END; i--)
            {
                // returned index
                int? retIdx = IHUtils.MoveItem(ref player.inventory[i], player.chestItems);
                if (retIdx.HasValue)
                {
                    RingBell(); //some movement occurred
                    // if whole stack moved, empty item slot
                    if ((int)retIdx>=0) player.inventory[i] = new Item();
                    //only for non-bank chest
                    if (player.chest>=0) SendNetMessage((int)retIdx);
                }
            }//\inv iteration
            player.chest < -1 ? Main.BankCoins() : Main.ChestCoins();
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

            // welp
            LootAll(player, player.chestItems, player.chest >= 0);
        } // \DoLootAll()

		/********************************************************
        *   LootAll
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
        */
        public static void DoQuickStack(Player player)
        {
            if (player.chest == -1) return;
            QuickStack(player, player.chestItems, player.chest >= 0);
            player.chest >= 0 ? Main.ChestCoins() : Main.BankCoins();
        }//\DoQuickStack()

        private static void QuickStack(Player player, Item[] container, bool sendMessage)
        {
            for (int i=0; i<Chest.maxItems; i++)
            {
                //if chest item is not blank && not a full stack:
                if (!container[i].IsBlank() && container[i].stack < container[i].maxStack)
                {
                    //for each item in inventory (including coins, ammo, hotbar):
                    for (int j=0; j<58; j++) {
                        //if chest item matches inv. item, then...
                        if (container[i].IsTheSameAs(player.inventory[j])
                            // ...merge inv. item stack to chest item stack
                        && StackMerge(ref player.inventory[j], ref container[i]))
                        {   // reset slot if all stack removed
                            player.inventory[j] = new Item();
                            RingBell();
                            if (sendMessage) SendNetMessage(i);
                        }}}}
        }//\QuickStack()
#endregion

#region helperfunctions
        /******************************************************
        *   MoveItem - moves a single item to a different container
        *
        *    @param item : the item to move
        *    @param container: where to move the item
        *
        *    @param rangeStart, rangeEnd : first and last index to consider eligigible
        *        move destinations in the target container.
        *    @param desc : whether to move item to end of container rather than beginning
        *
        *    @return >=0 : index in container where item was placed/stacked
        *             <0 : some stack remains (returned value is difference between initial
        *                    stack and current stack)
        *             null : item passed was blank or failed to move item
        */
        public static int? MoveItem(ref Item item, Item[] container, bool desc = false)
        {
            return MoveItem(ref item, container, 0, container.Length -1, desc);
        }

        public static int? MoveItem(ref Item item, Item[] container, int rangeStart, int rangeEnd, bool desc = false)
        {
            if (item.IsBlank()) return null;

            int iStart = rangeStart;
            Func<int,bool> iCheck = i => i <= rangeEnd;
            Func<int,int> iNext = i => i+1;

            if (desc) {
                iStart = rangeEnd;
                iCheck = i => i >= rangeStart;
                iNext  = i => i-1; }

            int preStack = item.stack;
            //search container for matching non-maxed stacks
            for (int j=iStart; iCheck(j); j=iNext(j)){
                Item item2 = container[j];

                // found a non-empty slot containing a <full stack of the same item type
                if (!item2.IsBlank() && item2.IsTheSameAs(item) && item2.stack < item2.maxStack
                && //then
                StackMerge(ref item, ref item2))
                    return j;  //if item's stack was reduced to 0

                //move remainder of stack to first empty slot
                for (int k=iStart; iCheck(k); k=iNext(k))
                {
                    if (container[k].IsBlank())
                    {
                        container[k] = item.Clone();
                        // item = new Item();
                        return k;
                    }}}

            //unable to move item/entire stack to container
            return preStack==item.stack ? null : (int?)item.stack-preStack ;
        }//\MoveItem()

        /********************************************************
        *   MoveItemHandler
        *   @param source
            @param iSource : index of the item in source
            @param dest
            @param destIsBank : is the destination container a regular chest (i.e. not
                one of the global "banks", and not the player inventory)?

            @return : True if item was (entirely) removed from source; otherwise false.
        */

        public static bool DoMoveItem(Item[] source, int iSource, Item[] dest, bool destIsChest = false, bool desc = false)
        {
            return DoMoveItem(source, iSource, dest, 0, dest.Length -1, destIsChest, desc);
        }


        public static bool DoMoveItem(Item[] source, int iSource, Item[] dest, int rangeStart, int rangeEnd, bool destIsChest = false, bool desc = false)
        {
            int? retIdx = IHUtils.MoveItem(ref source[iSource], dest, rangeStart, rangeEnd, desc);
            if (retIdx.HasValue)
            {   //some movement occurred
                RingBell();
                //only for non-bank chest
                if (destIsChest) SendNetMessage((int)retIdx);

                // if whole stack moved, empty item slot
                if ((int)retIdx>=0) {
                    source[iSource] = new Item();
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
        */
        public static bool StackMerge(ref Item itemSrc, ref Item itemDest)
        {
            int diff = Math.Min(itemDest.maxStack - itemDest.stack, itemSrc.stack);
            itemDest.stack += diff;
            itemSrc.stack  -= diff;

            // return true to indicate stack has been emptied
            return itemSrc.IsBlank();
        }

        //plays the "item moved" sound
        public static void RingBell(int o1 = -1, int o2 = -1, int o3 = 1)
        {
            Main.PlaySound(7, o1, o2, o3);
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
