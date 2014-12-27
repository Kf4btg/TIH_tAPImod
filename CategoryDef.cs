using System.Collections.Generic;
using System;
using System.Linq;
// using System.Linq.Expressions;
// using System.Linq.Dynamic;
// using System.Reflection;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public static class CategoryDef
    {
        // pass initial capacity as ItemCat.OTHER -- this trick will work so long as OTHER remains the last member of the Enum
        public static readonly Dictionary<ItemCat, List<String>> ItemSortRules = new Dictionary<ItemCat, List<String>>((Int32)ItemCat.OTHER+1);
        public static readonly Dictionary<ItemCat, Func<Item, bool>> Categories = new Dictionary<ItemCat, Func<Item, bool>>((Int32)ItemCat.OTHER+1);

        public static void Initialize()
        {
            SetupCategories();
            SetupSortingRules();
        }

        /*************************************************************************
        *  Define the categories and their matching rules
        */
        public static void SetupCategories()
        {
            // Item matching functions unceremoniously stolen from Shockah's Fancy Cheat Menu mod,
            // who graciously did all the hard work figuring these out so I didn't have to!
            // Although I did end up editing a few to make them mutually more exclusive
            // (e.g. adding the !vanity check to the armor items)
            Categories.Add( ItemCat.PICK, 		item   	=> item.pick > 0);
            Categories.Add( ItemCat.AXE, 		item   	=> item.axe > 0);
            Categories.Add( ItemCat.HAMMER,		item   	=> item.hammer > 0);
            Categories.Add( ItemCat.HEAD,		item 	=> item.headSlot != -1 && !item.vanity);
            Categories.Add( ItemCat.BODY,		item 	=> item.bodySlot != -1 && !item.vanity);
            Categories.Add( ItemCat.LEGS,		item 	=> item.legSlot  != -1 && !item.vanity);
            Categories.Add( ItemCat.ACCESSORY,	item   	=> item.accessory && !item.vanity);
            Categories.Add( ItemCat.VANITY,		item 	=> item.vanity);
            Categories.Add( ItemCat.MELEE,		item 	=> item.damage > 0 && item.melee);
            Categories.Add( ItemCat.RANGED,		item 	=> item.damage > 0 && item.ranged && (item.ammo == 0));
            Categories.Add( ItemCat.AMMO, 		item   	=> item.damage > 0 && item.ranged && item.ammo != 0 && !item.notAmmo);
            Categories.Add( ItemCat.MAGIC, 		item   	=> item.damage > 0 && item.magic);
            Categories.Add( ItemCat.SUMMON,		item   	=> item.damage > 0 && item.summon);
            Categories.Add( ItemCat.CONSUME, 	item   	=> item.consumable && item.bait == 0 && item.damage <= 0 && item.createTile == -1
                                                        && item.tileWand == -1 && item.createWall == -1 && item.ammo == 0 && item.name != "Xmas decorations");
            Categories.Add( ItemCat.BAIT, 		item   	=> item.bait > 0 && item.consumable);
            Categories.Add( ItemCat.DYE, 		item   	=> item.dye != 0);
            Categories.Add( ItemCat.PAINT, 		item   	=> item.paint != 0);
            Categories.Add( ItemCat.TILE, 		item   	=> item.createTile != -1 || item.tileWand != -1 || item.name == "Xmas decorations");
            Categories.Add( ItemCat.ORE, 		item   	=> Categories[ItemCat.TILE].Invoke(item) && item.name.EndsWith("Ore")  );
            Categories.Add( ItemCat.WALL, 		item   	=> item.createWall != -1);
            Categories.Add( ItemCat.PET, 		item   	=> item.damage <= 0 && ((item.shoot > 0 && Main.projPet[item.shoot]) ||
                                                        (item.buffType > 0 && (Main.vanityPet[item.buffType] || Main.lightPet[item.buffType]))));
            Categories.Add( ItemCat.OTHER, 		item    => true);


        } //end setupCategories()

        /*************************************************************************
        *    This is where we define how the items within each category will be
        *    sorted re: each other. Each category will be associated with a list
        *    of strings corresponding to properties of an Item instance. This list,
        *    through the magic of Dynamic LINQ, will be transformed into the
        *    OrderBy() arguments for that category.
        *
        *    Order of the items in the list matters (first item is primary sort field,
        *    second is secondary, and so on), and each property word may be followed by
        *    a space and the letters "desc", representing that that field should be
        *    sorted in descending order rather than the default ascending.
        */
        public static void SetupSortingRules()
        {
            ItemSortRules.Add( ItemCat.PICK,     new List<String> {"rare", "pick", "type", "value"});
            ItemSortRules.Add( ItemCat.AXE,      new List<String> {"rare", "axe", "type", "value"});
            ItemSortRules.Add( ItemCat.HAMMER,   new List<String> {"rare", "hammer", "type", "value"});
            // stack to sort the stackable boomerangs separately
            ItemSortRules.Add( ItemCat.MELEE,    new List<String> {"maxStack", "stack desc", "damage", "type", "rare", "value"});
            // consumable to sort throwing weapons separately
            ItemSortRules.Add( ItemCat.RANGED,   new List<String> {"consumable", "stack desc", "damage", "type", "rare", "value"});
            ItemSortRules.Add( ItemCat.MAGIC,    new List<String> {"damage", "rare", "type", "value"});
            ItemSortRules.Add( ItemCat.SUMMON,   new List<String> {"damage", "rare", "type", "value"});
            ItemSortRules.Add( ItemCat.AMMO,     new List<String> {"rare", "damage", "type", "value", "stack desc"});
            ItemSortRules.Add( ItemCat.HEAD,     new List<String> {"rare", "defense", "value", "type"});
            ItemSortRules.Add( ItemCat.BODY,     new List<String> {"rare", "defense", "value", "type"});
            ItemSortRules.Add( ItemCat.LEGS,     new List<String> {"rare", "defense", "value", "type"});
            ItemSortRules.Add( ItemCat.ACCESSORY,new List<String> {"type", "rare", "value", "prefix.id"});
            // stack because of those fishbowls...
            ItemSortRules.Add( ItemCat.VANITY,   new List<String> {"name", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.PET,      new List<String> {"buffType", "type"});
            ItemSortRules.Add( ItemCat.CONSUME,  new List<String> {"potion desc", "name.EndsWith(\"Potion\") desc", "buffType", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.BAIT,     new List<String> {"bait", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.DYE,      new List<String> {"dye", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.PAINT,    new List<String> {"paint", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.ORE,      new List<String> {"rare", "value", "type", "stack desc"});
            // gems have alpha==50, cobwebs==100
            ItemSortRules.Add( ItemCat.TILE,     new List<String> {"name.EndsWith(\"Bar\") desc", "alpha desc", "name.EndsWith(\"Seeds\") desc",
                                                                   "tileWand", "createTile", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.WALL,     new List<String> {"createWall", "type", "stack desc"});
            // generic stuff
            ItemSortRules.Add( ItemCat.OTHER,    new List<String> {"material desc", "type", "netID", "stack desc"});
        }//end setup sorting rules

    }

}
