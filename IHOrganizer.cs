using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Dynamic;
using Terraria;

namespace InvisibleHand
{
    public static class IHOrganizer
    {
        /// this will sort the categorized items first by category, then by
        /// more specific traits. Sorting Rules defined in CategoryDef class.
        public static List<Item> OrganizeItems(List<Item> source)
        {
            if (source == null) return null;

            // returns an IEnumerable<IGrouping<ItemCat,Item>>
            var byCategory =
                from item in source
                group item by item.GetCategory() into category
                orderby category.Key
                select category;

            var sortedList = new List<Item>();

            //  Now we can dynamically construct Queries using Dynamic LINQ
            // expression methods with arbitrary (maybe later user-defined) sorting parameters.
            foreach (var category in byCategory)
            {
                // pull the sorting rules for this category from the ItemSortRules dictionary, convert them to a
                // single string using "String.Join()", and pass it to the Dynamic LINQ OrderBy() method.
                var result = category.AsQueryable().OrderBy(String.Join(", ", CategoryDef.ItemSortRules[category.Key]));

                // execute the query and put the result in a list to return
                foreach (var s_item in result)
                    sortedList.Add(s_item);
            }
            return sortedList;
        }

       /// <summary>
       /// Construct a list containing cloned copies of items in the given container, skipping blank (and optionally locked) slots.
       /// </summary>
       /// <param name="source_container">The Item[] array of the container</param>
       /// <param name="source_is_chest">Is the source container a chest? </param>
       /// <param name="rangeStart">index in source to start looking for items </param>
       /// <param name="rangeEnd">index in source to stop looking for items </param>
       /// <returns> The new list of copied items, or null if no items were
       /// applicable to be copied (NOT an empty list!).</returns>
        public static List<Item> GetItemCopies(Item[] source_container, bool source_is_chest, int rangeStart, int rangeEnd)
        {
            return GetItemCopies(source_container, source_is_chest, new Tuple<int, int>(rangeStart, rangeEnd));
        }

        /// <summary>
        /// Construct a list containing cloned copies of items in the given
        /// container, skipping blank (and optionally locked) slots.
        /// </summary>
        /// <param name="source_container">The Item[] array of the container</param>
        /// <param name="source_is_chest">Is the source container a chest? </param>
        /// <param name="range">Starting and ending indices defining the subset of the
        /// source's slots to be searched for items.</param>
        /// <returns> The new list of copied items, or null if no items were
        /// applicable to be copied (NOT an empty list!).</returns>
        public static List<Item> GetItemCopies(Item[] source_container, bool source_is_chest, Tuple<int,int> range = null)
        {
            if (range == null) range = new Tuple<int,int>(0, source_container.Length -1);

            // initialize the list that will hold the copied items
            var itemList = new List<Item>();

            int count = 0; //having trouble with empty lists...

            // get copies of viable items from container.
            // will need a different list if locking is enabled
            if (!source_is_chest && IHBase.ModOptions["LockingEnabled"])
            {
                for (int i=range.Item1; i<=range.Item2; i++)
                {
                    if (IHPlayer.SlotLocked(i) || source_container[i].IsBlank()) continue;

                    itemList.Add(source_container[i].Clone());
                    count++;
                }
            }
            else //only skip blank slots
            {
                for (int i=range.Item1; i<=range.Item2; i++)
                {
                    if (!source_container[i].IsBlank())
                        itemList.Add(source_container[i].Clone());
                    count++;
                }
            }
            // return null if no items were copied to new list
            return count > 0 ? itemList : null;
        }

        /**
          SortPlayerInv - perform the sort operation on the items in the player's
        	inventory, excluding the hotbar and optionally any slots marked as locked

          @param player: The player whose inventory to sort.
          @param reverse: whether to reverse the list of item once it's sorted
        */
        public static void SortPlayerInv(Player player, bool reverse=false)
        {
            ConsolidateStacks(player.inventory, 0, 49); //include hotbar in this step

            Sort(player.inventory, false, reverse, 10, 49);
        }

        // as above, but for the Item[] array of a chest
        public static void SortChest(Item[] chestitems, bool reverse=false)
        {
            ConsolidateStacks(chestitems);

            Sort(chestitems, true, reverse);
        }

        // as above, but given the actual chest object, pull out the array
        public static void SortChest(Chest chest, bool reverse=false)
        {
            SortChest(chest.item, reverse);
        }

        /**
          Sort Container

          @param container: The container whose contents to sort.
          @param chest : whether the container is a chest (otherwise the player inventory)
          @param rangeStart: starting index of the sort operation
          @param rangeEnd: end index of the sort operation

          Omitting both range arguments will sort the entire container.

        FIXME (maybe?): the "item-moved" sound plays even if the order doesn't change.
        */
        public static void Sort(Item[] container, bool chest, bool reverse, int rangeStart, int rangeEnd)
        {
            Sort(container, chest, reverse, new Tuple<int,int>(rangeStart, rangeEnd));
        }

        public static void Sort(Item[] container, bool chest, bool reverse, Tuple<int,int> range = null)
        {
            // if range param not specified, set it to whole container
            if (range == null) range = new Tuple<int,int>(0, container.Length -1);

            // for clarity
            var checkLocks = IHBase.ModOptions["LockingEnabled"]; //boolean

            // get copies of the items and send them off to be sorted
            var sortedItemList = OrganizeItems(GetItemCopies(container, chest, range));
            if (sortedItemList == null) return;

            if (reverse) sortedItemList.Reverse(); //reverse on user request

            // depending on user settings, decide if we copy items to end or beginning of container
            var fillFromEnd = chest ? IHBase.ModOptions["RearSortChest"] : IHBase.ModOptions["RearSortPlayer"]; //boolean

            // set up the functions that will be used in the iterators ahead
            Func<int,int> getIndex, getIter;
            Func<int,bool> getCond, getWhileCond;

            if (fillFromEnd)	// use decrementing iterators
            {
                getIndex = x => range.Item2 - x;
                getIter = x => x-1;
                getCond = x => x >= range.Item1;
                getWhileCond = x => x>range.Item1 && IHPlayer.SlotLocked(x);
            }
            else 	// use incrementing iterators
            {
                getIndex = y => range.Item1 + y;
                getIter = y => y+1;
                getCond = y => y <= range.Item2;
                getWhileCond = y => y<range.Item2 && IHPlayer.SlotLocked(y);
            }

            int filled = 0; // num of slots filled (or locked) so far
            if (!chest && checkLocks) // player inv with locking enabled
            {
                // copy the sorted items back to the original container
                // (overwriting the current, unsorted contents)
                foreach (var item in sortedItemList)
                {
                    // find the first unlocked slot. this would throw an
                    // exception if range.Item1+filled somehow went over 49, but
                    // if the categorizer and slot-locker are functioning
                    // correctly, that _shouldn't_ be possible. Shouldn't.
                    // Probably.
                    while (IHPlayer.SlotLocked(getIndex(filled)))
                        filled++;

                    // now that we've found an unlocked slot, clone
                    // the next sorted item into it.
                    container[getIndex(filled++)] = item.Clone();
                    Sound.ItemMoved.Play();
                }
                // and the rest of the slots should be empty
                for (int i=getIndex(filled); getCond(i); i=getIter(i))
                {
                    // find the first unlocked slot.
                    if (IHPlayer.SlotLocked(i)) continue;

                    container[i] = new Item();
                }
            }
            else // just run through 'em all
            {
                foreach ( var item in sortedItemList)
                {
                    container[getIndex(filled++)] = item.Clone();
                    Sound.ItemMoved.Play();
                }
                // and the rest of the slots should be empty
                for (int i=getIndex(filled); getCond(i); i=getIter(i))
                {
                    container[i] = new Item();
                }
            }
        } // sort()

        /*************************************************************************
        *  Adapted from "PutItem()" in ShockahBase.SBase
        *  @params container, rangeStart, rangeEnd
        */
        public static void ConsolidateStacks(Item[] container, int rangeStart, int rangeEnd)
        {
            ConsolidateStacks(container, new Tuple<int,int>(rangeStart, rangeEnd));
        }

        public static void ConsolidateStacks(Item[] container, Tuple<int, int> range = null)
        {
            if (range == null) range = new Tuple<int,int>(0, container.Length -1);

            for (int i = range.Item2; i>=range.Item1; i--) //iterate in reverse
            {
                var item = container[i];

                //found non-blank item in a <full stack
                if (!item.IsBlank() && item.stack < item.maxStack)
                {
                    // search the remaining slots for other stacks of this item
                    StackItems(ref item, container, range.Item1, i-1);
                }
            }
        }

        /// called by ConsolidateStacks, this takes a single item and searches a
        /// subset of the original range for other non-max stacks of that item
        private static void StackItems(ref Item item, Item[] container, int rangeStart, int rangeEnd)
        {
            for (int j=rangeEnd; j>=rangeStart; j--) //iterate in reverse
            {
                var item2 = container[j];
                // found another <full stack of a matching item
                if (!item2.IsBlank() && item2.IsTheSameAs(item) && item2.stack < item2.maxStack)
                {
                    int diff = Math.Min(item2.maxStack - item2.stack, item.stack);
                    item2.stack += diff;
                    item.stack -= diff;

                    Sound.ItemMoved.Play();

                    if (item.IsBlank())
                    {
                        item = new Item();
                        return;
                    }//\item blank
                }//\matching stack
            }//\go through container range
        }//\StackItems()
    }
}
