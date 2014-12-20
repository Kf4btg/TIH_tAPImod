using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{

    /**
    *  Simply bundles an item with its category and provides the ability to
    *  sort the item first by category (catID), then type, and finally
    *  netID. Could possibly be simplified to contain just the integer category
    *  ID and not the full IMCategory<Item> object.
    */
    public class CategorizedItem : IComparable<CategorizedItem>
    {
        public IMCategory<Item> category { get; private set; }
        public Item item { get; private set; }

        public CategorizedItem(Item item, IMCategory<Item> category)
        {
            this.item=item;
            this.category=category;
        }

        /*  Comparison Priority:
                Category
                Type
                netID
                //Rarity
                //Stack Size
        */
        public int CompareTo(CategorizedItem other)
        {
            return (category.CompareTo(other.category) == 0 ?
                    (item.type.CompareTo(other.item.type) == 0 ?
                            item.netID.CompareTo(other.item.netID) :
                            item.type.CompareTo(other.item.type)) :
                        category.CompareTo(other.category)
                    );
        }
    }

}
