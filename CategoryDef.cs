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
        // pass initial capacity as ItemCat.OTHER -- this trick should work so long as OTHER remains the last member of the Enum
        public static readonly Dictionary<ItemCat, List<String>> ItemSortRules = new Dictionary<ItemCat, List<String>>((Int32)ItemCat.OTHER+1);
        public static readonly Dictionary<ItemCat, Func<Item, bool>> Categories = new Dictionary<ItemCat, Func<Item, bool>>((Int32)ItemCat.OTHER+1);

        // A large number of "Tile"-type items will share a .createTile attribute with items that fulfill a similar purpose.
        // This gives us a handy way to sort and possibly even categorize these item types.
        // Here are few, constant-ified.
/**/    public const int TILE_TORCH             =     4;  // NOTE: torches/candles etc also have noWet=true
/**/    public const int TILE_DOOR              =    10;
/**/    public const int TILE_CUPS              =    13; // pink vase, wine glass, bottles, &c.
/**/    public const int TILE_TABLE             =    14;
/**/    public const int TILE_CHAIR             =    15;
/**/    public const int TILE_ANVIL_BASIC       =    16; // Iron & Lead only
/**/    public const int TILE_WORKBENCH         =    18;
/**/    public const int TILE_PLATFORM          =    19;
/**/    public const int TILE_CHEST             =    21;
/**/    public const int TILE_CANDLE            =    33; // not water candle
/**/    public const int TILE_CHANDELIER        =    34;
/**/    public const int TILE_LANTERN           =    42;
/**/    public const int TILE_BED               =    79;
/**/    public const int TILE_SEED              =    82; // THE "POTION-FARM" PLANTS
/**/    public const int TILE_GRAVE             =    85;
/**/    public const int TILE_PIANO             =    87;
/**/    public const int TILE_DRESSER           =    88;
/**/    public const int TILE_SOFA              =    89; //ALSO, the two Benches
/**/    public const int TILE_BATHTUB           =    90;
        public const int TILE_BANNER            =    91;
/**/    public const int TILE_LAMP              =    93;
/**/    public const int TILE_COOKING_POT       =    96; //only includes cooking pot & cauldron
/**/    public const int TILE_CANDELABRA        =   100;
/**/    public const int TILE_BOOKCASE          =   101;
/**/    public const int TILE_CLOCK             =   104;
/**/    public const int TILE_BOWL              =   103; //wooden and glass?
/**/    public const int TILE_STATUE            =   105; //ALSO VASES
/**/    public const int TILE_FURNACE3          =   133; //Adamant, Orich forge
/**/    public const int TILE_ANVIL_ADV         =   134; //Mythril-quality
        public const int TILE_MUSIC_BOX         =   139;
/**/    public const int TILE_GEM               =   178;
/**/    public const int TILE_FOUNTAIN          =   207;
        public const int TILE_DYE_MATERIAL      =   227; // consecutive ids, 1107-1114, 1115-119 are husks, mucus, ink ##kick these out of the TILE group
/**/    public const int TILE_BAR               =   239; // SEEMS TO HAVE THEM ALL
/**/    public const int TILE_TROPHY            =   240; // also some paintings and decorative wall hangings
/**/    public const int TILE_PAINTING          =   242; //some in 240, "Catacomb" is 241, two in 245
/**/    public const int TILE_BEACH_STUFF       =   324;
/**/    public const int TILE_TEXT_STATUE       =   337;

        // public readonly int[] TILE_BUTTERFLY_JAR = new int[] {288-297};
        // public readonly int[] TILE_JELLYFISH_JAR = new int[] {316, 317, 318};
        //
        // public readonly int[] TILE_CRITTER_CAGE  = new int[] {298,299};

        //some groups
        // public readonly int[] TILEGROUP_FURNITURE   = new int[] { TILE_TABLE, TILE_CHAIR, TILE_BED, TILE_PIANO, TILE_DRESSER,
        //                                                         TILE_SOFA, TILE_BATHTUB, TILE_BOOKCASE, TILE_CLOCK, TILE_CHEST, 29, 55, 97,102, 124, 128, 269 }; //piggy, sign, safe, throne, wooden beam, wo/mannequin
        // public readonly int[] TILEGROUP_LIGHTING    = new int[] { TILE_TORCH, TILE_CANDLE, TILE_CHANDELIER, TILE_LANTERN, TILE_LAMP, TILE_CANDELABRA, 35, 95, 98, 215, 270, 271 }; //jackolantern, chinese, skull, campfire, bugs in bottles
        // public readonly int[] TILEGROUP_STATUE      = new int[] { TILE_GRAVE, TILE_STATUE, TILE_FOUNTAIN, TILE_TEXT_STATUE, 244 }; //bubble machine
        //
        // public static readonly int[] TILEGROUP_WALLDECO    = new int[] { TILE_TROPHY, TILE_PAINTING, 241, 245 } //"Catacomb" in 241, amurrca stuff in 245
        // public static readonly int[] TILEGROUP_CLUTTER     = new int[] { TILE_CUPS, TILE_BOWL, TILE_BEACH_STUFF, 50 /*Book*/, 81/*Coral*/, 319/*Ship in bottle*/ } + TILE_BUTTERFLY_JAR + TILE_CRITTER_CAGE + TILE_JELLYFISH_JAR
        // public static readonly int[] TILEGROUP_CRAFTING    = new int[] { TILE_WORKBENCH, TILE_ANVIL_BASIC, TILE_ANVIL_ADV, TILE_FURNACE3, TILE_COOKING_POT,
        //                                                                 17, 77, 86, 94, 106, 114, 125, [217-220], 228, 243, 247, 283, [300-308] }
        //                                                                 //furnace, hellforge
                                                                        //300-308=all the new custom stuff

        //also, items with width=8, height=10: all woods, platforms, tile-wands (rare), pumpkin, pumpkin seed, hay
        // quest fish: uniquestack=true, rare=-11

        /*************************************************************************
        Create a number of hashsets to quickly check values of "item.createTile" to aid in categorization/sorting
        */
        public static readonly HashSet<int> TileGroupFurniture  = new HashSet<int>
            ( new int[] { TILE_DOOR, TILE_TABLE, TILE_CHAIR, TILE_PLATFORM, TILE_BED, TILE_PIANO, TILE_DRESSER, TILE_SOFA, TILE_BATHTUB, TILE_BOOKCASE, TILE_CLOCK, TILE_CHEST,
                29, 55, 97,102, 124, 128, 269 } ); //piggy, sign, safe, throne, wooden beam, wo/mannequin
        public static readonly HashSet<int> TileGroupLighting   = new HashSet<int>
            ( new int[] { TILE_TORCH, TILE_CANDLE, TILE_CHANDELIER, TILE_LANTERN, TILE_LAMP, TILE_CANDELABRA, 35, 95, 98, 215, 270, 271 } ); //jackolantern, chinese, skull, campfire, bugs in bottles

        public static readonly HashSet<int> TileGroupStatue     = new HashSet<int>
            ( new int[] { TILE_GRAVE, TILE_STATUE, TILE_FOUNTAIN, TILE_TEXT_STATUE, 244 } ); //bubble machine

        public static readonly HashSet<int> TileGroupWallDeco   = new HashSet<int>
            ( new int[] { TILE_TROPHY, TILE_PAINTING, 241, 245 } );

        public static readonly HashSet<int> TileGroupClutter    = new HashSet<int>
            ( new int[] { TILE_CUPS, TILE_BOWL, TILE_BEACH_STUFF, 50 /*Book*/, 81/*Coral*/, 319/*Ship in bottle*/, 316, 317, 318 } ); //jellyfish jars

        public static readonly HashSet<int> TileGroupCrafting   = new HashSet<int>
            ( new int[] { TILE_WORKBENCH, TILE_ANVIL_BASIC, TILE_ANVIL_ADV, TILE_FURNACE3, TILE_COOKING_POT,
            17, 77, 86, 94, 106, 114, 125, 217, 218, 219, 220, 228, 243, 247, 283, 300, 301, 302, 303, 304, 305, 306, 307, 308 } ); //blergh

        public static readonly HashSet<int> TileGroupOre  = new HashSet<int>(new int[] { 37, 56, 58 }); //meteorite, obsidian, hellstone (get others by name)


        public static void Initialize()
        {
            // CreateTileGroups();
            for (int i=288; i<=299; i++){   //butterfly jars & critter cages
                TileGroupClutter.Add(i);
            }

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
            Categories.Add( ItemCat.HEAD,		item 	=> item.headSlot != -1 && item.defense>0);// && item.headSlot != 13); //13=empty bucket
            Categories.Add( ItemCat.BODY,		item 	=> item.bodySlot != -1 && item.defense>0);
            Categories.Add( ItemCat.LEGS,		item 	=> item.legSlot  != -1 && item.defense>0);
            Categories.Add( ItemCat.ACCESSORY,	item   	=> item.accessory && !item.vanity);
            Categories.Add( ItemCat.VANITY,		item 	=> item.vanity || item.headSlot!=-1 || item.bodySlot!=-1 || item.legSlot!=-1 ); //catch the non-armor
            Categories.Add( ItemCat.MELEE,		item 	=> item.damage > 0 && item.melee);
            Categories.Add( ItemCat.RANGED,		item 	=> item.damage > 0 && item.ranged && (item.ammo == 0));
            Categories.Add( ItemCat.AMMO, 		item   	=> item.damage > 0 && item.ranged && item.ammo != 0 && !item.notAmmo);
            Categories.Add( ItemCat.MAGIC, 		item   	=> item.damage > 0 && item.magic);
            Categories.Add( ItemCat.SUMMON,		item   	=> item.damage > 0 && item.summon);
            Categories.Add( ItemCat.MECH,       item    => item.mech || item.cartTrack );
            Categories.Add( ItemCat.CONSUME, 	item   	=> item.consumable && item.bait == 0 && item.damage <= 0 && item.createTile == -1
                                                        && item.tileWand == -1 && item.createWall == -1 && item.ammo == 0 && item.name != "Xmas decorations");
            Categories.Add( ItemCat.BAIT, 		item   	=> item.bait > 0 && item.consumable);
            Categories.Add( ItemCat.DYE, 		item   	=> item.dye != 0);
            Categories.Add( ItemCat.PAINT, 		item   	=> item.paint != 0);
            Categories.Add( ItemCat.TILE, 		item   	=> (item.createTile != -1 && item.createTile!=TILE_DYE_MATERIAL) || item.tileWand != -1 || item.name == "Xmas decorations" );
            Categories.Add( ItemCat.ORE, 		item   	=> Categories[ItemCat.TILE].Invoke(item) && (item.name.EndsWith("Ore") || TileGroupOre.Contains(item.createTile)) );
            Categories.Add( ItemCat.BAR, 		item   	=> item.createTile==TILE_BAR );
            Categories.Add( ItemCat.GEM, 		item   	=> item.createTile==TILE_GEM );
            Categories.Add( ItemCat.SEED, 		item   	=> item.createTile==TILE_SEED ); //leaves out crimson, hallowed, etc
            Categories.Add( ItemCat.CRAFT, 		item   	=> TileGroupCrafting.Contains(item.createTile) );
            Categories.Add( ItemCat.LIGHT, 		item   	=> TileGroupLighting.Contains(item.createTile) );
            Categories.Add( ItemCat.FURNITURE, 	item   	=> TileGroupFurniture.Contains(item.createTile) );
            Categories.Add( ItemCat.STATUE, 	item   	=> TileGroupStatue.Contains(item.createTile) );
            Categories.Add( ItemCat.WALLDECO, 	item   	=> TileGroupWallDeco.Contains(item.createTile) );
            Categories.Add( ItemCat.BANNER, 	item   	=> item.createTile==TILE_BANNER );
            Categories.Add( ItemCat.CLUTTER, 	item   	=> TileGroupClutter.Contains(item.createTile) );

            Categories.Add( ItemCat.WALL, 		item   	=> item.createWall != -1);
            Categories.Add( ItemCat.PET, 		item   	=> item.damage <= 0 && ((item.shoot > 0 && Main.projPet[item.shoot]) ||
                                                        (item.buffType > 0 && (Main.vanityPet[item.buffType] || Main.lightPet[item.buffType]))));
            Categories.Add( ItemCat.OTHER, 		item    => true);
            // Categories.Add( ItemCat.BAR, 		item   	=> Categories[ItemCat.TILE].Invoke(item) && item.name.EndsWith("Bar")  );
            // pretty sure only gems will match the next category
            // Categories.Add( ItemCat.GEM, 		item   	=> Categories[ItemCat.TILE].Invoke(item) && item.alpha==50  );


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
            ItemSortRules.Add( ItemCat.PICK,     new List<String> { "rare", "pick", "type", "value"});
            ItemSortRules.Add( ItemCat.AXE,      new List<String> { "rare", "axe", "type", "value"});
            ItemSortRules.Add( ItemCat.HAMMER,   new List<String> { "rare", "hammer", "type", "value"});
            // stack to sort the stackable boomerangs separately
            ItemSortRules.Add( ItemCat.MELEE,    new List<String> { "maxStack", "stack desc", "damage", "type", "rare", "value"});
            // consumable to sort throwing weapons separately
            ItemSortRules.Add( ItemCat.RANGED,   new List<String> { "consumable", "stack desc", "damage", "type", "rare", "value"});
            ItemSortRules.Add( ItemCat.MAGIC,    new List<String> { "damage", "rare", "type", "value"});
            ItemSortRules.Add( ItemCat.SUMMON,   new List<String> { "damage", "rare", "type", "value"});
            ItemSortRules.Add( ItemCat.AMMO,     new List<String> { "rare", "damage", "type", "value", "stack desc"});
            ItemSortRules.Add( ItemCat.HEAD,     new List<String> { "rare", "defense", "value", "type"});
            ItemSortRules.Add( ItemCat.BODY,     new List<String> { "rare", "defense", "value", "type"});
            ItemSortRules.Add( ItemCat.LEGS,     new List<String> { "rare", "defense", "value", "type"});
            ItemSortRules.Add( ItemCat.ACCESSORY,new List<String> { "handOffSlot", "handOnSlot", "backSlot", "frontSlot", "shoeSlot", "waistSlot",
                                                                    "wingSlot", "balloonSlot", "faceSlot", "neckSlot", "shieldSlot",
                                                                    "rare", "value", "type", "prefix.id"});
            // stack because of those fishbowls...
            ItemSortRules.Add( ItemCat.VANITY,   new List<String> { "headSlot!=-1 desc", "bodySlot!=-1 desc", "legSlot!=-1 desc", "name", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.PET,      new List<String> { "buffType", "type"});
            ItemSortRules.Add( ItemCat.MECH,     new List<String> { "cartTrack", "tileBoost", "createTile desc", "value", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.CONSUME,  new List<String> { "potion desc", "name.EndsWith(\"Potion\") desc", "buffType", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.BAIT,     new List<String> { "bait", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.DYE,      new List<String> { "dye", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.PAINT,    new List<String> { "paint", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.ORE,      new List<String> { "rare", "value", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.BAR,      new List<String> { "rare", "value", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.GEM,      new List<String> { "rare", "value", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.SEED,     new List<String> { "name", "type", "stack desc"});

            ItemSortRules.Add( ItemCat.CRAFT,    new List<String> { "createTile", "type", "name", "stack desc"});
            ItemSortRules.Add( ItemCat.LIGHT,    new List<String> { "createTile", "type", "name", "stack desc"});
            ItemSortRules.Add( ItemCat.FURNITURE,new List<String> { "createTile", "type", "name", "stack desc"});
            ItemSortRules.Add( ItemCat.STATUE,   new List<String> { "createTile", "type", "name", "stack desc"});
            ItemSortRules.Add( ItemCat.WALLDECO, new List<String> { "createTile", "type", "name", "stack desc"});
            ItemSortRules.Add( ItemCat.BANNER,   new List<String> { "type", "stack desc"});
            ItemSortRules.Add( ItemCat.CLUTTER,  new List<String> { "createTile", "type", "name", "stack desc"});
            // had to hard-code my IsWoodLike check b/c dynamic linq doesn't like extension methods...
            ItemSortRules.Add( ItemCat.TILE,     new List<String> { "(width==8 && height==10 && maxStack==999) desc", "name.EndsWith(\"Block\") desc", "name.EndsWith(\"Brick\") desc",
                                                                    "tileWand", "createTile", "type", "name", "stack desc"});
            ItemSortRules.Add( ItemCat.WALL,     new List<String> { "createWall", "type", "stack desc"});
            // generic stuff
            ItemSortRules.Add( ItemCat.OTHER,    new List<String> { "material desc", "type", "netID", "stack desc"});
        }//end setup sorting rules


    }

}
