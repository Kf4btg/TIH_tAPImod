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
            if (category.CompareTo(other.category) != 0) return category.CompareTo(other.category);

            // improve sorting within certain categories
            switch (category.catID)
            {
                case InventoryManager.ID_ORE:
                    if (item.value!=other.item.value) return item.value.CompareTo(other.item.value);
                    break;
                // default:
                //     break;
            }

            if (item.type!=other.item.type) return item.type.CompareTo(other.item.type);
            if (item.rare!=other.item.rare) return item.rare.CompareTo(other.item.rare);
            if (item.stack!=other.item.stack) return item.stack.CompareTo(other.item.stack);
            if (item.netID!=other.item.netID) return item.netID.CompareTo(other.item.netID);
            if (item.prefix.id!=other.item.prefix.id) return item.prefix.id.CompareTo(other.item.prefix.id);
            return 0;
            //name?

        }
    }

}
