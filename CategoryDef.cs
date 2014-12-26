using System.Collections.Generic;
using System;
// using System.Linq;
// using System.Linq.Expressions;
// using System.Linq.Dynamic;
using System.Reflection;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public static class CategoryDef
    {
        public static List<IHCategory<Item>> Categories;// = new List<IHCategory<Item>>();
        public static Dictionary<ItemCat, List<String>> ItemSortRules;// = new Dictionary<ItemCat, List<String>>();

        public static IHCategory<Item>
        catHead, catBody, catLegs, catVanity, catMelee, catRanged, catAmmo, catMagic, catSummon, catAccessory,
        catPick, catAxe, catHammer, catConsume, catDye, catPaint, catOre, catTile, catWall, catPet, catOther, catBait;


        public static void Initialize()
        {
            Categories = new List<IHCategory<Item>>();
            ItemSortRules = new Dictionary<ItemCat, List<String>>();

            SetupCategories();
            SetupSortingRules();
        }

        /*************************************************************************
        *  Define the categories and their matching rules, then put them in the list.
        */
        public static void SetupCategories()
        {
            // Item matching functions unceremoniously stolen from Shockah's Fancy Cheat Menu mod,
            // who graciously did all the hard work figuring these out so I didn't have to!
            // Although I did end up editing a few to make them mutually more exclusive
            // (e.g. adding the !vanity check to the armor items)
            catPick		= new IHCategory<Item>( ItemCat.PICK, 		item   	=> item.pick > 0);
            catAxe		= new IHCategory<Item>( ItemCat.AXE, 		item   	=> item.axe > 0);
            catHammer	= new IHCategory<Item>( ItemCat.HAMMER,		item   	=> item.hammer > 0);
            catHead		= new IHCategory<Item>( ItemCat.HEAD,		item 	=> item.headSlot != -1 && !item.vanity);
            catBody		= new IHCategory<Item>( ItemCat.BODY,		item 	=> item.bodySlot != -1 && !item.vanity);
            catLegs		= new IHCategory<Item>( ItemCat.LEGS,		item 	=> item.legSlot  != -1 && !item.vanity);
            catAccessory= new IHCategory<Item>( ItemCat.ACCESSORY,	item   	=> item.accessory && !item.vanity);
            catVanity	= new IHCategory<Item>( ItemCat.VANITY,		item 	=> item.vanity);
            catMelee	= new IHCategory<Item>( ItemCat.MELEE,		item 	=> item.damage > 0 && item.melee);
            catRanged	= new IHCategory<Item>( ItemCat.RANGED,		item 	=> item.damage > 0 && item.ranged && (item.ammo == 0));
            catAmmo		= new IHCategory<Item>( ItemCat.AMMO, 		item   	=> item.damage > 0 && item.ranged && item.ammo != 0 && !item.notAmmo);
            catMagic	= new IHCategory<Item>( ItemCat.MAGIC, 		item   	=> item.damage > 0 && item.magic);
            catSummon	= new IHCategory<Item>( ItemCat.SUMMON,		item   	=> item.damage > 0 && item.summon);
            catConsume	= new IHCategory<Item>( ItemCat.CONSUME, 	item   	=> item.consumable && item.bait == 0 && item.damage <= 0 && item.createTile == -1 && item.tileWand == -1 && item.createWall == -1 && item.ammo == 0 && item.name != "Xmas decorations");
            catBait 	= new IHCategory<Item>( ItemCat.BAIT, 		item   	=> item.bait > 0 && item.consumable);
            catDye		= new IHCategory<Item>( ItemCat.DYE, 		item   	=> item.dye != 0);
            catPaint	= new IHCategory<Item>( ItemCat.PAINT, 		item   	=> item.paint != 0);
            catTile		= new IHCategory<Item>( ItemCat.TILE, 		item   	=> item.createTile != -1 || item.tileWand != -1 || item.name == "Xmas decorations");
            catOre 		= new IHCategory<Item>( ItemCat.ORE, 		item   	=> catTile.matches(item) && item.name.EndsWith("Ore")  );
            catWall		= new IHCategory<Item>( ItemCat.WALL, 		item   	=> item.createWall != -1);
            catPet		= new IHCategory<Item>( ItemCat.PET, 		item   	=> item.damage <= 0 && ((item.shoot > 0 && Main.projPet[item.shoot]) || (item.buffType > 0 && (Main.vanityPet[item.buffType] || Main.lightPet[item.buffType]))));
            catOther	= new IHCategory<Item>( ItemCat.OTHER, 		null);

            Categories.AddRange(new IHCategory<Item>[]
            {
                catPick,
                catAxe,
                catHammer,
                catMelee,
                catRanged,
                catMagic,
                catSummon,
                catAmmo,
                catHead,
                catBody,
                catLegs,
                catAccessory,
                catVanity,
                catPet,
                catConsume,
                catBait,
                catDye,
                catPaint,
                catOre,
                catTile,
                catWall
                });
                //catOther NOT added to the list

            } //end initialize()

        /*************************************************************************
            This is where we define how the items within each category will be
            sorted re: each other. Each category will be associated with a list
            of strings corresponding to properties of an Item instance. This list,
            through the magic of Dynamic LINQ, will be transformed into the
            OrderBy() arguments for that category.

            Order of the items in the list matters (first item is primary sort field,
            second is secondary, and so on), and each property word may be followed by
            a space and the letters "desc", representing that that field should be
            sorted in descending order.
        */
        public static void SetupSortingRules()
        {
            foreach (var category in Categories)
            {
                List<String> sortFields;
                switch((ItemCat)category.catID)
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

                    default: // catOther/there's a bug and this shouldn't be reached
                        sortFields=new List<String>() {"material desc", "type", "netID", "stack desc"};
                        break;
                }
                ItemSortRules.Add((ItemCat)category.catID, sortFields);
            }
            // because this is not in the category list
            ItemSortRules.Add(ItemCat.OTHER, new List<String>() {"material desc", "type", "netID", "stack desc"});
        }//end setup sorting rules

    }

}
