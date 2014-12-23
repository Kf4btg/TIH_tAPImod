using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Expressions;
using TAPI;
using Terraria;

namespace InvisibleHand
{

    public static class IHOrganizer
    {

        private static readonly Dictionary<ItemSortProperty,String> sortParams;

        public static void Init(){
            sortParams=new Dictionary<ItemSortProperty,String>();

            // sortParams.Add(ItemSortProperty.Value, "value");
            foreach (ItemSortProperty isp in ItemSortProperty)
            {

            }
        }


        // this will sort the categorized items first by category, then by
        // more specific traits.
        public static List<Item> OrganizeItems(List<CategorizedItem> source)
        {
            // returns an IEnumerable<IGrouping<IMCategory<Item>,CategorizedItem>>
            var byCategory =
                from item in source
                group item by item.category into category
                orderby category.Key
                select category;

            foreach (var category in byCategory)
            {
                switch(category.Key.catID)
                {
                    case ItemCat.ORE:
                        oreSort(category);
                        break;
                }
            }


        }

        public static List<String> GetPropNames(ItemSortProperty p)
        {
            //split on commas and spaces, remove empty strings
            List<String> props = new List<String>();
            foreach (String s in (p.ToString().Split(new Char[] {',',' '}, StringSplitOptions.None)))
            {
                props.Add(String.Concat((s[0].ToString().ToLower()),s.Substring(1)));
            }
            return props;
        }

        public static void BuildQuery(IEnumerable<IComparable<Object>> preds)
        {

        }

        public static List<CategorizedItem> oreSort(IGrouping<IMCategory<Item>,CategorizedItem> ores)
        {
            // List<CategorizedItem> sorted = new List<CategorizedItem>();


        }

        public static IEnumerable<CategorizedItem> sortBy(IEnumerable<CategorizedItem> items, IEnumerable<ItemSortProperty> props)
        {

        }


    }


}
