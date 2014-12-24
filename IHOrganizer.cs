using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TAPI;
using Terraria;

namespace InvisibleHand
{

    public static class IHOrganizer
    {

        // private static readonly Dictionary<ItemSortProperty,String> sortParams;
        //
        // public static void Init(){
        //     sortParams=new Dictionary<ItemSortProperty,String>();
        //
        //     // sortParams.Add(ItemSortProperty.Value, "value");
        //     foreach (ItemSortProperty isp in ItemSortProperty)
        //     {
        //
        //     }
        // }


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

            // and that was the easy part. Now need to dynamically construct Queries using
            // LINQ expression methods with arbitrary (maybe later user-defined) sorting parameters.``
            foreach (var category in byCategory)
            {   // category.Key is IMCategory<Item>

                switch(category.Key.catID)
                {
                    case ItemCat.ORE:
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
                // first letter to lowercase, add to list
                props.Add(String.Concat((s[0].ToString().ToLower()),s.Substring(1)));
            }
            return props;
        }

        // public static void BuildQuery(IEnumerable<IComparable<Object>> preds)
        // public static void BuildQuery(IGrouping<IMCategory<Item>,CategorizedItem> source, List<String> sortFields)
        public static IQueryable<Item> BuildQuery(IQueryable<Item> source, List<String> sortFields)
                {
            //category type is IGrouping<IMCategory<Item>,CategorizedItem>
            /* Build an expression of the form
                from item in category
                    orderby item.property1,
                            item.property2,
                            ...
                    select item
                OR
                items.OrderBy(item => item.property1).ThenBy(item => item.property2)....
            */

            //Create the Parameter Expression for the query
            // Expressions.ParameterExpression
            var eachItemAsParam = Expression.Parameter(typeof(Item), "item");

            //get the first orderBy expression
            MemberExpression orderByProperty;

            // Prepare Expression var
            MethodCallExpression orderByExp;

            bool first = true;
            foreach (String s in sortFields)
            {
                orderByProperty = Expression.Property(eachItemAsParam, s);

                if (first)
                {   //create expression to represent source.OrderBy(...)
                    first=false;
                    orderByExp = Expression.Call(
                        typeof(Queryable),
                        "OrderBy",
                        new Type[] { source.ElementType, typeof(IComparable) },
                        source.Expression,
                        Expression.Lambda<Func<Item, IComparable>>(orderByProperty, new ParameterExpression[] { eachItemAsParam })
                        );
                }
                else
                {
                    // Create similar expression trees for any other specified properties, using a ThenBy call
                    // (could make this recursive)

                    orderByExp = Expression.Call(
                        typeof(Queryable),
                        "ThenBy",
                        new Type[] { source.ElementType, typeof(IComparable) },
                        orderByExp,
                        Expression.Lambda<Func<Item, IComparable>>(orderByProperty, new ParameterExpression[] { eachItemAsParam })
                        );
                }
            }

            // create and return the query
            return source.Provider.CreateQuery<Item>(orderByExp);

        }

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
