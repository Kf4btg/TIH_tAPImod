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
        // public static List<Item> OrganizeItems(List<CategorizedItem> source)
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
            // and that was the easy part. Now need to dynamically construct Queries using
            // LINQ expression methods with arbitrary (maybe later user-defined) sorting parameters.
            foreach (var category in byCategory)
            {   // category.Key is IMCategory<Item>
                // IQueryable<Item> query;
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
                        sortFields=new List<String>() {"maxStack", "stack", "damage", "type", "rare", "value"};
                        break;
                    case ItemCat.RANGED:
                        // consumable to sort throwing weapons separately
                        sortFields=new List<String>() {"consumable", "stack", "damage", "type", "rare", "value"};
                        break;
                    case ItemCat.MAGIC:
                        sortFields=new List<String>() {"damage", "rare", "type", "value"};
                        break;
                    case ItemCat.SUMMON:
                        sortFields=new List<String>() {"damage", "rare", "type", "value"};
                        break;
                    case ItemCat.AMMO:
                        sortFields=new List<String>() {"maxStack", "stack", "damage", "type", "rare", "value"};
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
                        sortFields=new List<String>() {"name", "type", "stack"};
                        break;
                    case ItemCat.PET:
                        sortFields=new List<String>() {"buffType", "type"};
                        break;
                    case ItemCat.CONSUME:
                        // first option will include fish, shrooms, etc.
                        sortFields=new List<String>() {"potion", "name.EndsWith(\"Potion\") desc", "buffType", "type", "stack"};
                        break;
                    case ItemCat.BAIT:
                        sortFields=new List<String>() {"bait", "type", "stack"};
                        break;
                    case ItemCat.DYE:
                        sortFields=new List<String>() {"dye", "type", "stack"};
                        break;
                    case ItemCat.PAINT:
                        sortFields=new List<String>() {"paint", "type", "stack"};
                        break;
                    case ItemCat.ORE:
                        sortFields=new List<String>() {"rare", "value", "type", "stack"};
                        break;
                    case ItemCat.TILE:
                        // gems have alpha==50, cobwebs==100
                        sortFields=new List<String>() {"name.EndsWith(\"Bar\") desc", "name.EndsWith(\"Seeds\") desc", "alpha desc",
                                                        "tileWand", "createTile", "type", "stack"};
                        break;
                    case ItemCat.WALL:
                        sortFields=new List<String>() {"createWall", "type", "stack"};
                        break;

                    default: // catOther
                        sortFields=new List<String>() {"material desc", "type", "netID", "stack"};
                        break;
                }

                var result=BuildDynamicQuery(category.AsQueryable(), sortFields);

                // var result = category.AsQueryable().OrderBy("type, stack");

                foreach (Item i in result)
                {
                    sortedList.Add(i);
                }

                // sortedList.AddRange( BuildDynamicQuery(category.AsQueryable(), sortFields).ToList() );

                // sortedList.AddRange( (category.AsQueryable().OrderBy(String.Join(", ",sortFields))).ToArray() );
            }
            return sortedList;
        }

        public static IEnumerable<Item> BuildDynamicQuery(IQueryable<Item> source, List<String> sortFields)
        {
            // bool first = true;
            // String orderString = "";
            // for (int i=0; i<sortFields.Count-1; i++)
            // foreach (String s in sortFields)
            // {
            //     orderString+=s;
            //     // orderString+=", "
            //     // orderString
            // }

            String orderString = String.Join(", ", sortFields);

            return source.OrderBy(orderString);

        }



        /************************************************************
         Build an expression tree to represent the query:
            from item in category
            orderby item.property1,
            item.property2,
            ...
            select item
        AKA:
            category.OrderBy(item => item.property1).ThenBy(item => item.property2)....

            source type is IGrouping<IMCategory<Item>,Item>

        public static IQueryable<Item> BuildQuery(IQueryable<Item> source, List<String> sortFields)
        {

            //Create the Parameter Expression for the query
            // Expressions.ParameterExpression
            var eachItemAsParam = Expression.Parameter(typeof(Item), "item");

            // Prepare Expression vars
            MethodCallExpression orderByExp = null;
            MemberExpression orderByProperty;

            Expression baseExp = source.Expression; //was having troubles with "unassigned variable" errors...
            String opString = "OrderBy";    //create expression to represent source.OrderBy(...)
            bool first = true;
            foreach (String s in sortFields)
            {
                orderByProperty = Expression.Property(eachItemAsParam, s);

                orderByExp = Expression.Call(
                    typeof(Queryable),
                    opString,
                    new Type[] { source.ElementType, typeof(IComparable) },
                    baseExp,
                    Expression.Lambda<Func<Item, IComparable>>(orderByProperty, new ParameterExpression[] { eachItemAsParam })
                );

                baseExp = orderByExp;
                if (first)
                {  // Create similar expression trees for any other specified properties, using a ThenBy call
                    opString = "ThenBy";
                    first=false;
                }

                    // orderByExp = Expression.Call(
                    //     typeof(Queryable),
                    //     "OrderBy",
                    //     new Type[] { source.ElementType, typeof(IComparable) },
                    //     source.Expression,
                    //     Expression.Lambda<Func<Item, IComparable>>(orderByProperty, new ParameterExpression[] { eachItemAsParam })
                    //     );

                // else
                // {

                    // orderByExp = Expression.Call(
                    //     typeof(Queryable),
                    //     "ThenBy",
                    //     new Type[] { source.ElementType, typeof(IComparable) },
                    //     orderByExp,
                    //     Expression.Lambda<Func<Item, IComparable>>(orderByProperty, new ParameterExpression[] { eachItemAsParam })
                    //     );
                // }
            }

            // create and return the query
            return source.Provider.CreateQuery<Item>(orderByExp);
        }
        */

        // go through category list, checking the item against the match parameters
        // for each until the item either matches a category or fails all
        // checks (except for the last category, Other, which isn't checked and
        // into which all unmatched items will fall).
        public static IMCategory<Item> GetCategory(Item item)
        {
            foreach (var category in InventoryManager.Categories)
            {
                if (category.matches(item)) { return category; }
            }
            return InventoryManager.catOther;
        }

        // go through category-list, checking the item against the match parameters
        // for each category until the item either matches a category or fails all
        // the checks (except for the last category, Other, which isn't checked and
        // into which all unmatched items will fall).
        // public static List<Item> MatchCategory(List<Item> items, IMCategory<Item> category)
        // {
        //     List<Item> matchedItems = new List<Item>();
        //     foreach (Item item in items)
        //     {
        //
        //         if (category.matches(item)){ matchedItems.Add(item); }
        //         // int cid = 0;
        //         // while ( cid<(InventoryManager.Categories.Count - 1) &&
        //         //     !InventoryManager.Categories[cid++].match_params(item) ) { }
        //     }
        // }


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
            switch(IHBase.opt_reverseSort)
            {
                case IHBase.RS_DISABLE:		//normal sort to beginning
                    break;
                case IHBase.RS_PLAYER:		//copy to end for player inventory only
                fillFromEnd = !chest;
                    break;
                case IHBase.RS_CHEST:		//copy to end for chests only
                fillFromEnd = chest;
                    break;
                case IHBase.RS_BOTH:		//copy to end for all containers
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
        // public static List<CategorizedItem> oreSort(IGrouping<IMCategory<Item>,CategorizedItem> ores)
        // {
        //     // List<CategorizedItem> sorted = new List<CategorizedItem>();
        //
        //
        // }
        //
        // public static IEnumerable<CategorizedItem> sortBy(IEnumerable<CategorizedItem> items, IEnumerable<ItemSortProperty> props)
        // {
        //
        // }


    }


}
