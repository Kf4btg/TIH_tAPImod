using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using System.Reflection;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public static class ItemExtension
    {
        public static IMCategory<Item> GetCategory(this Item item)
        {
            foreach (var category in InventoryManager.Categories)
            {
                if (category.matches(item)) { return category; }
            }
            return InventoryManager.catOther;
        }
    }

    public static class IHOrganizer
    {
        // this will sort the categorized items first by category, then by
        // more specific traits.
        public static List<Item> OrganizeItems(List<Item> source)
        {
            // returns an IEnumerable<IGrouping<IMCategory<Item>,CategorizedItem>>
            var byCategory =
                from item in source
                group item by item.GetCategory() into category
                orderby category.Key
                select category;

            /*  create a Category -> list_of_items indexer using ToLookup()
                Returns an ILookup<IMCategory<Item>,List<Item>> which is really an
                    IEnumerable<IGrouping<IMCategory<Item>, List<Item>> */
            // var byCategory = source.ToLookup( item => GetCategory(item) );

            /* This Lookup can possibly later be used for matching categories between chests and inventories
                by using chestCategories.contains(itemCategory), or even chestCats[itemCat]
            */

            List<Item> sortedList = new List<Item>();

            //  Now we can dynamically construct Queries using Dynamic LINQ
            // expression methods with arbitrary (maybe later user-defined) sorting parameters.
            foreach (var category in byCategory)
            {   // category.Key is IMCategory<Item>
                List<String> sortFields;
                switch((ItemCat)category.Key.catID)
                {
                    case ItemCat.PICK:
                        sortFields=new List<String>() {"rare", "pick", "type", "value"};
                        break;
                    case ItemCat.AXE:
                        sortFields=new List<String>() {"rare", "axe", "type", "value"};
                        break;
                    case ItemCat.HAMMER:
                        sortFields=new List<String>() {"rare", "hammer", "type", "value"};
                        break;
                    case ItemCat.MELEE:
                        // stack to sort the stackable boomerangs separately
                        sortFields=new List<String>() {"maxStack", "stack desc", "damage", "type", "rare", "value"};
                        break;
                    case ItemCat.RANGED:
                        // consumable to sort throwing weapons separately
                        sortFields=new List<String>() {"consumable", "stack desc", "damage", "type", "rare", "value"};
                        break;
                    case ItemCat.MAGIC:
                        sortFields=new List<String>() {"damage", "rare", "type", "value"};
                        break;
                    case ItemCat.SUMMON:
                        sortFields=new List<String>() {"damage", "rare", "type", "value"};
                        break;
                    case ItemCat.AMMO:
                        sortFields=new List<String>() {"rare", "damage", "type", "value", "stack desc"};
                        break;
                    case ItemCat.HEAD:
                        sortFields=new List<String>() {"rare", "defense", "value", "type"};
                        break;
                    case ItemCat.BODY:
                        sortFields=new List<String>() {"rare", "defense", "value", "type"};
                        break;
                    case ItemCat.LEGS:
                        sortFields=new List<String>() {"rare", "defense", "value", "type"};
                        break;
                    case ItemCat.ACCESSORY:
                        sortFields=new List<String>() {"type", "rare", "value", "prefix.id"};
                        break;
                    case ItemCat.VANITY:
                        // stack because of those fishbowls...
                        sortFields=new List<String>() {"name", "type", "stack desc"};
                        break;
                    case ItemCat.PET:
                        sortFields=new List<String>() {"buffType", "type"};
                        break;
                    case ItemCat.CONSUME:
                        // first option will include fish, shrooms, etc.
                        sortFields=new List<String>() {"potion desc", "name.EndsWith(\"Potion\") desc", "buffType", "type", "stack desc"};
                        break;
                    case ItemCat.BAIT:
                        sortFields=new List<String>() {"bait", "type", "stack desc"};
                        break;
                    case ItemCat.DYE:
                        sortFields=new List<String>() {"dye", "type", "stack desc"};
                        break;
                    case ItemCat.PAINT:
                        sortFields=new List<String>() {"paint", "type", "stack desc"};
                        break;
                    case ItemCat.ORE:
                        sortFields=new List<String>() {"rare", "value", "type", "stack desc"};
                        break;
                    case ItemCat.TILE:
                        // gems have alpha==50, cobwebs==100
                        sortFields=new List<String>() {"name.EndsWith(\"Bar\") desc", "name.EndsWith(\"Seeds\") desc", "alpha desc",
                                                        "tileWand", "createTile", "type", "stack desc"};
                        break;
                    case ItemCat.WALL:
                        sortFields=new List<String>() {"createWall", "type", "stack desc"};
                        break;

                    default: // catOther
                        sortFields=new List<String>() {"material desc", "type", "netID", "stack desc"};
                        break;
                }

                // var result=BuildDynamicQuery(category.AsQueryable(), sortFields);

                var result = category.AsQueryable().OrderBy(String.Join(", ", sortFields));

                foreach (Item i in result)
                {
                    sortedList.Add(i);
                }
            }
            return sortedList;
        }

        public static IEnumerable<Item> BuildDynamicQuery(IQueryable<Item> source, List<String> sortFields)
        {
            String orderString = String.Join(", ", sortFields);

            return source.OrderBy(orderString);

        }


        /*************************************************************************
        *  Sort Container
        *
        *  @param container: The container whose contents to sort.
        *  @param checkLocks: whether to check for & exclude locked slots
        *  @param chest : whether the container is a chest (otherwise the player inventory)
        *  @param rangeStart: starting index of the sort operation
        *  @param rangeEnd: end index of the sort operation
        *
        *  Omitting both range arguments will sort the entire container.
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
            bool checkLocks = IHBase.lockingEnabled;

            // initialize the list that will hold the items to sort
            var itemSorter = new List<Item>();

            // get copies of sortable items from container
            // will need a different list to pass to the sorter if locking is enabled
            if (!chest && checkLocks)
            {
                for (int i=range.Item1; i<=range.Item2; i++)
                {
                    if (IHPlayer.SlotLocked(i) || container[i].IsBlank()) continue;

                    itemSorter.Add(container[i].Clone());
                }
            }
            else
            {
                for (int i=range.Item1; i<=range.Item2; i++)
                {
                    if (!container[i].IsBlank()) itemSorter.Add(container[i].Clone());
                }
            }

            /* now this seems too easy... */
            // itemSorter.Sort();  // sort using the CategorizedItem.CompareTo() method
            // Well it was easy, then I had to screw it up.
            itemSorter = OrganizeItems(itemSorter);

            if (reverse) itemSorter.Reverse();

            // depending on user settings, decide if we re-copy items to end or beginning of container
            bool fillFromEnd = false;
            switch(IHBase.opt_rearSort)
            {
                case RearSort.DISABLE:		//normal sort to beginning
                    break;
                case RearSort.PLAYER:		//copy to end for player inventory only
                fillFromEnd = !chest;
                    break;
                case RearSort.CHEST:		//copy to end for chests only
                fillFromEnd = chest;
                    break;
                case RearSort.BOTH:		//copy to end for all containers
                fillFromEnd=true;
                    break;
            }

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

            int filled = 0;
            if (!chest && checkLocks) // move these checks out of the loop
            {
                // copy the sorted items back to the original container
                foreach (Item item in itemSorter)
                {
                    // find the first unlocked slot.
                    // this would throw an exception if range.Item1+filled somehow went over 49,
                    // but if the categorizer and slot-locker are functioning correctly,
                    // that _shouldn't_ be possible. Shouldn't. Probably.
                    while (IHPlayer.SlotLocked(getIndex(filled))) { filled++; }
                    container[getIndex(filled++)] = item.Clone();
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
                foreach ( Item item in itemSorter)
                {
                    container[getIndex(filled++)] = item.Clone();
                    // filled++;
                }
                // and the rest of the slots should be empty
                for (int i=getIndex(filled); getCond(i); i=getIter(i))
                {
                    container[i] = new Item();
                }
            }
        } // sort()

        //
        // public static List<String> GetPropNames(ISP p)
        // {
        //     //split on commas and spaces, remove empty strings
        //     List<String> props = new List<String>();
        //     foreach (String s in (p.ToString().Split(new Char[] {',',' '}, StringSplitOptions.None)))
        //     {
        //         // first letter to lowercase, add to list
        //         props.Add(String.Concat((s[0].ToString().ToLower()),s.Substring(1)));
        //     }
        //     return props;
        // }

    }


}
