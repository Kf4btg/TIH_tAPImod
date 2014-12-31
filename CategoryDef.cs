using System.Collections.Generic;
using System;
using System.Linq;
// using System.Linq.Expressions;
// using System.Linq.Dynamic;
// using System.Reflection;
using TAPI;
using Terraria;
using Terraria.ID;

namespace InvisibleHand
{
    public static class ItemExtension
    {
        public static ItemCat GetCategory(this Item item)
        {
            foreach (ItemCat catID in CategoryDef.CheckOrder)
            {
                if (CategoryDef.Categories[catID].Invoke(item)) { return catID; }
            }
            return ItemCat.OTHER;
        }

        public static bool IsHook(this Item item)
        {
            return ProjDef.byType.ContainsKey(item.shoot) && (ProjDef.byType[item.shoot].hook || ProjDef.byType[item.shoot].aiStyle==7);
        }

        public static bool IsBomb(this Item item)
        {            //grenades, bombs, etc
            return ProjDef.byType.ContainsKey(item.shoot) && ProjDef.byType[item.shoot].aiStyle==16;
        }

        public static bool IsTool(this Item item)
        {
            return item.createTile == TileID.Rope || item.createTile == TileID.Chain || item.name.EndsWith("Bucket") ||
            item.fishingPole > 1 || item.tileWand != -1 || item.IsHook() || ItemDef.autoSelect["Glowstick"].Contains(item.type) ||
            item.type == 1991 || item.type == 50 || item.type == 1326 || ItemDef.autoSelect["Flaregun"].Contains(item.type) ||
            item.name.Contains("Paintbrush") || item.name.Contains("Paint Roller") || item.name.Contains("Paint Scraper") ||
            (item.type >= 1543 && item.type <= 1545);
            //bucket, bug net, magic mirror, rod of discord, spectre paint tools
        }
    }


    public static class CategoryDef
    {
        // pass initial capacity as ItemCat.OTHER -- this trick should work so long as OTHER remains the last member of the Enum
        public static readonly Dictionary<ItemCat, List<String>> ItemSortRules = new Dictionary<ItemCat, List<String>>((Int32)ItemCat.OTHER+1);
        public static readonly Dictionary<ItemCat, Func<Item, bool>> Categories = new Dictionary<ItemCat, Func<Item, bool>>((Int32)ItemCat.OTHER+1);

        //the ItemCat Enum defines the actual Sort Order of the categories,
        // but this defines in which order an item will be checked against
        // the category matching rules. This is important due to a kind
        // of "sieve" or "cascade" effect, where items that would have matched
        // a certain category were instead caught by an earlier one.
        public static readonly ItemCat[] CheckOrder = new ItemCat[]  {
            ItemCat.COIN,
            ItemCat.MECH,
            ItemCat.TOOL,
            ItemCat.PICK,
            ItemCat.AXE,
            ItemCat.HAMMER,
            ItemCat.HEAD,
            ItemCat.BODY,
            ItemCat.LEGS,
            ItemCat.ACCESSORY,
            ItemCat.VANITY,
            ItemCat.MELEE,
            ItemCat.BOMB,
            ItemCat.RANGED,
            ItemCat.AMMO,
            ItemCat.MAGIC,
            ItemCat.SUMMON,
            // ItemCat.HOOK,
            ItemCat.CONSUME,
            ItemCat.POTION,
            ItemCat.BAIT,
            ItemCat.DYE,
            ItemCat.PAINT,
            ItemCat.TILE,
            ItemCat.ORE,
            ItemCat.BAR,
            ItemCat.GEM,
            ItemCat.SEED,
            ItemCat.CRAFT,
            ItemCat.LIGHT,
            ItemCat.FURNITURE,
            ItemCat.STATUE,
            ItemCat.WALLDECO,
            ItemCat.BANNER,
            ItemCat.CLUTTER,
            ItemCat.WOOD,
            ItemCat.BLOCK,
            ItemCat.BRICK,
            ItemCat.WALL,
            ItemCat.PET,
            ItemCat.SPECIAL,
            ItemCat.MISC_MAT,
            ItemCat.OTHER
        };

        // A large number of "Tile"-type items will share a .createTile attribute with items that fulfill a similar purpose.
        // This gives us a handy way to sort and possibly even categorize these item types.

        /*************************************************************************
        Create several hashsets to quickly check values of "item.createTile" to aid in categorization/sorting.
        Initialize them here with an anonymous array to avoid the resizing penalty of .Add()
        */
        public static readonly
            HashSet<int> TileGroupFurniture  = new HashSet<int>( new int[] {
                TileID.ClosedDoor, TileID.Tables, TileID.Chairs, TileID.Platforms,
                TileID.Beds, TileID.Pianos, TileID.Dressers, TileID.Benches,
                TileID.Bathtubs, TileID.Bookcases, TileID.GrandfatherClocks,
                TileID.Containers, TileID.PiggyBank, TileID.Signs, TileID.Safes,
                TileID.Thrones, TileID.WoodenPlank, TileID.Mannequin, TileID.Womannequin } );

        public static readonly
            HashSet<int> TileGroupLighting   = new HashSet<int>( new int[] {
                TileID.Torches, TileID.Candles, TileID.Chandeliers, TileID.HangingLanterns,
                TileID.Lamps, TileID.Candelabras, TileID.Jackolanterns, TileID.ChineseLanterns,
                TileID.SkullCandles, TileID.Campfire, TileID.FireflyinaBottle, TileID.LightningBuginaBottle, TileID.WaterCandle } );

        public static readonly
            HashSet<int> TileGroupStatue     = new HashSet<int>( new int[] {
                TileID.Tombstones, TileID.Statues, TileID.WaterFountain,
                TileID.AlphabetStatues, TileID.BubbleMachine } );

        public static readonly
            HashSet<int> TileGroupWallDeco   = new HashSet<int>( new int[] {
                TileID.Painting2x3, TileID.Painting3x2, TileID.Painting3x3,
                TileID.Painting4x3, TileID.Painting6x4 } );

        public static readonly
            HashSet<int> TileGroupClutter    = new HashSet<int>( new int[] {
                TileID.Bottles, TileID.Bowls, TileID.BeachPiles, TileID.Books, TileID.Coral,
                TileID.ShipInABottle, TileID.BlueJellyfishBowl, TileID.GreenJellyfishBowl,
                TileID.PinkJellyfishBowl, TileID.SeaweedPlanter, TileID.ClayPot,
                TileID.BunnyCage, TileID.SquirrelCage, TileID.MallardDuckCage, TileID.DuckCage,
                TileID.BirdCage, TileID.BlueJay, TileID.CardinalCage, TileID.FishBowl,
                TileID.SnailCage, TileID.GlowingSnailCage, TileID.MonarchButterflyJar,
                TileID.PurpleEmperorButterflyJar, TileID.RedAdmiralButterflyJar,
                TileID.UlyssesButterflyJar, TileID.SulphurButterflyJar, TileID.TreeNymphButterflyJar,
                TileID.ZebraSwallowtailButterflyJar, TileID.JuliaButterflyJar, TileID.ScorpionCage,
                TileID.BlackScorpionCage, TileID.FrogCage, TileID.MouseCage, TileID.PenguinCage,
                TileID.WormCage, TileID.GrasshopperCage } );

        public static readonly
            HashSet<int> TileGroupCrafting   = new HashSet<int>( new int[] {
                TileID.WorkBenches, TileID.Anvils, TileID.MythrilAnvil, TileID.AdamantiteForge,
                TileID.CookingPots, TileID.Furnaces, TileID.Hellforge, TileID.Loom, TileID.Kegs,
                TileID.Sawmill, TileID.TinkerersWorkbench, TileID.CrystalBall, TileID.Blendomatic,
                TileID.MeatGrinder, TileID.Extractinator, TileID.Solidifier, TileID.DyeVat,
                TileID.ImbuingStation, TileID.Autohammer, TileID.HeavyWorkBench, TileID.BoneWelder,
                TileID.FleshCloningVaat, TileID.GlassKiln, TileID.LihzahrdFurnace, TileID.LivingLoom,
                TileID.SkyMill, TileID.IceMachine, TileID.SteampunkBoiler, TileID.HoneyDispenser } ); //blergh

        public static readonly
            HashSet<int> TileGroupOre        = new HashSet<int>( new int[] {
                TileID.Meteorite, TileID.Obsidian, TileID.Hellstone }); //(get others by name)

        public static readonly
            HashSet<int> TileGroupCoin       = new HashSet<int>( new int[] {
                TileID.CopperCoinPile, TileID.SilverCoinPile, TileID.GoldCoinPile, TileID.PlatinumCoinPile });

        public static readonly
            HashSet<int> TileGroupSeed       = new HashSet<int>( new int[] {
                TileID.ImmatureHerbs, TileID.Saplings /*Acorn*/, TileID.Pumpkins /*Pumpkin Seed*/ } );
                // get the rest by EndsWith("Seeds")

        public static readonly
                HashSet<int> TileGroupTool       = new HashSet<int>( new int[] {
                    TileID.Rope, TileID.Chain });


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
            Categories.Add( ItemCat.COIN,       item    => TileGroupCoin.Contains(item.createTile));
            Categories.Add( ItemCat.PICK, 		item   	=> item.pick > 0);
            Categories.Add( ItemCat.AXE, 		item   	=> item.axe > 0);
            Categories.Add( ItemCat.HAMMER,		item   	=> item.hammer > 0);
            Categories.Add( ItemCat.HEAD,		item 	=> item.headSlot != -1 && item.defense>0); //13=empty bucket
            Categories.Add( ItemCat.BODY,		item 	=> item.bodySlot != -1 && item.defense>0);
            Categories.Add( ItemCat.LEGS,		item 	=> item.legSlot  != -1 && item.defense>0);
            Categories.Add( ItemCat.ACCESSORY,	item   	=> item.accessory && !item.vanity);
            Categories.Add( ItemCat.VANITY,		item 	=> (item.vanity || item.headSlot!=-1 || item.bodySlot!=-1 || item.legSlot!=-1) && item.defense==0 ); //catch the non-armor
            Categories.Add( ItemCat.MELEE,		item 	=> item.damage > 0 && item.melee);
            Categories.Add( ItemCat.RANGED,		item 	=> item.damage > 0 && item.ranged && (item.ammo == 0));
            Categories.Add( ItemCat.BOMB,       item    => item.IsBomb());
            Categories.Add( ItemCat.AMMO, 		item   	=> item.damage > 0 && item.ranged && item.ammo != 0 && !item.notAmmo);
            Categories.Add( ItemCat.MAGIC, 		item   	=> item.damage > 0 && item.magic);
            Categories.Add( ItemCat.SUMMON,		item   	=> item.damage > 0 && item.summon);
            Categories.Add( ItemCat.MECH,       item    => item.mech || item.cartTrack );

            // Categories.Add( ItemCat.HOOK,       item    => ProjDef.byType.ContainsKey(item.shoot) && (ProjDef.byType[item.shoot].hook || ProjDef.byType[item.shoot].aiStyle==7) );

            Categories.Add( ItemCat.TOOL,       item    => item.IsTool() );

            Categories.Add( ItemCat.CONSUME, 	item   	=> item.consumable && item.bait == 0 && item.damage <= 0 && item.createTile == -1
                                                        && item.tileWand == -1 && item.createWall == -1 && item.ammo == 0 && item.name != "Xmas decorations");

            // for some reason, all vanilla potions have w=14, h=24. Food, ale, etc. are all different.
            Categories.Add( ItemCat.POTION, 	item   	=> Categories[ItemCat.CONSUME].Invoke(item) && item.width==14 && item.height==24 );

            Categories.Add( ItemCat.BAIT, 		item   	=> item.bait > 0 && item.consumable);
            Categories.Add( ItemCat.DYE, 		item   	=> item.dye != 0);
            Categories.Add( ItemCat.PAINT, 		item   	=> item.paint != 0);
            Categories.Add( ItemCat.TILE, 		item   	=> (item.createTile != -1 && item.createTile!=TileID.DyePlants) || item.tileWand != -1 || item.name == "Xmas decorations" );
            Categories.Add( ItemCat.ORE, 		item   	=> item.createTile != -1 && (item.name.EndsWith("Ore") || TileGroupOre.Contains(item.createTile)) );
            Categories.Add( ItemCat.BAR, 		item   	=> item.createTile==TileID.MetalBars );
            Categories.Add( ItemCat.GEM, 		item   	=> item.createTile==TileID.ExposedGems );
            Categories.Add( ItemCat.SEED, 		item   	=> TileGroupSeed.Contains(item.createTile) || (item.createTile != -1 && item.name.EndsWith("Seeds") ));
            Categories.Add( ItemCat.CRAFT, 		item   	=> TileGroupCrafting.Contains(item.createTile) );
            Categories.Add( ItemCat.LIGHT, 		item   	=> TileGroupLighting.Contains(item.createTile) );
            Categories.Add( ItemCat.FURNITURE, 	item   	=> TileGroupFurniture.Contains(item.createTile) );
            Categories.Add( ItemCat.STATUE, 	item   	=> TileGroupStatue.Contains(item.createTile) );
            Categories.Add( ItemCat.WALLDECO, 	item   	=> TileGroupWallDeco.Contains(item.createTile) );
            Categories.Add( ItemCat.BANNER, 	item   	=> item.createTile==TileID.Banners );
            Categories.Add( ItemCat.CLUTTER, 	item   	=> TileGroupClutter.Contains(item.createTile) );
            Categories.Add( ItemCat.WOOD,       item    => ItemDef.itemGroups["g:Wood"].Contains(item) );
            Categories.Add( ItemCat.BLOCK,   	item   	=> item.createTile != -1 && item.width==12 && item.height==12 && item.value==0 );
            Categories.Add( ItemCat.BRICK,   	item   	=> Categories[ItemCat.BLOCK].Invoke(item) && (item.name.EndsWith("Brick")
                                                            || item.name.EndsWith("Slab") || item.name.EndsWith("Plating")) );


            Categories.Add( ItemCat.WALL, 		item   	=> item.createWall != -1);
            Categories.Add( ItemCat.PET, 		item   	=> item.damage <= 0 && ((item.shoot > 0 && Main.projPet[item.shoot])
                                                        || (item.buffType > 0 && (Main.vanityPet[item.buffType] || Main.lightPet[item.buffType]))));
            Categories.Add( ItemCat.MISC_MAT,	item   	=> item.material && !item.notMaterial);
            Categories.Add( ItemCat.SPECIAL,	item   	=> item.useStyle == 4 ); //Boss summon, hearts, mana crystals
            Categories.Add( ItemCat.OTHER, 		item    => true);

    		// though i wanted to avoid this, I'm afraid there are some items that will need to be assigned to
    		// categories manually, by type.  These would include:
    			// bucket = OTHER
    			// acorn, pumpkin seed = SEED
    			//

    		// TODO: divide the "OTHER" category into OTHER_MATERIALS & OTHER (stuff like umbrella, )
    		// TODO: make bombs/dynamite & boss summoning items be separate from food and other
    		//		  buffing consumables.
    		// TODO: see if possible to read some NPCdef file to figure out if an item is an NPC or an environment
    		//		(entity or tile) drop.

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
            ItemSortRules.Add( ItemCat.MELEE,    new List<String> { "maxStack", "damage", "type", "rare", "value", "stack desc"});
            // consumable to sort throwing weapons separately
            ItemSortRules.Add( ItemCat.RANGED,   new List<String> { "consumable", "damage", "type", "rare", "value", "stack desc"});
            ItemSortRules.Add( ItemCat.MAGIC,    new List<String> { "damage", "rare", "type", "value"});
            ItemSortRules.Add( ItemCat.SUMMON,   new List<String> { "damage", "rare", "type", "value"});
            ItemSortRules.Add( ItemCat.AMMO,     new List<String> { "rare", "damage", "type", "value", "stack desc"});
            ItemSortRules.Add( ItemCat.HEAD,     new List<String> { "rare", "defense", "value", "type"});
            ItemSortRules.Add( ItemCat.BODY,     new List<String> { "rare", "defense", "value", "type"});
            ItemSortRules.Add( ItemCat.LEGS,     new List<String> { "rare", "defense", "value", "type"});
            ItemSortRules.Add( ItemCat.ACCESSORY,new List<String> { "handOffSlot", "handOnSlot", "backSlot", "frontSlot", "shoeSlot", "waistSlot",
                                                                    "wingSlot", "balloonSlot", "faceSlot", "neckSlot", "shieldSlot",
                                                                    "rare", "value", "type", "prefix.id"});
            // ItemSortRules.Add( ItemCat.HOOK,     new List<String> { "shoot" , "type"});
            ItemSortRules.Add( ItemCat.TOOL,     new List<String> { "consumable", "fishingPole", "shoot", "type", "stack desc" }
            // stack because of those fishbowls...
            ItemSortRules.Add( ItemCat.VANITY,   new List<String> { "headSlot!=-1 desc", "bodySlot!=-1 desc", "legSlot!=-1 desc", "name", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.PET,      new List<String> { "buffType", "type"});
            ItemSortRules.Add( ItemCat.MECH,     new List<String> { "cartTrack", "tileBoost", "createTile desc", "value", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.CONSUME,  new List<String> { "potion desc", "buffType", "type", "stack desc"});
            ItemSortRules.Add( ItemCat.POTION,   new List<String> { "healLife desc", "healMana desc", "buffType!=0 desc", "buffType", "type", "stack desc"});
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
            ItemSortRules.Add( ItemCat.WOOD,     new List<String> { "createTile", "type", "name", "stack desc" });
            ItemSortRules.Add( ItemCat.BLOCK,    new List<String> { "createTile", "type", "name", "stack desc" });
            ItemSortRules.Add( ItemCat.BRICK,    new List<String> { "createTile", "type", "name", "stack desc" });
            ItemSortRules.Add( ItemCat.TILE,     new List<String> { "tileWand", "createTile", "type", "name", "stack desc"});
            ItemSortRules.Add( ItemCat.WALL,     new List<String> { "createWall", "type", "stack desc"});
            // generic stuff
            ItemSortRules.Add( ItemCat.MISC_MAT, new List<String> { "type", "netID", "stack desc"});
            ItemSortRules.Add( ItemCat.SPECIAL,  new List<String> { "maxStack", "type", "stack desc"}); 
            // quest fish: uniquestack=true, rare=-11
            ItemSortRules.Add( ItemCat.OTHER,    new List<String> { "uniqueStack", "rare", "type", "netID", "stack desc"});
        }//end setup sorting rules



        private static bool IsBomb(Item item)
        {
            //grenades, bombs, etc
            return (ProjDef.byType.ContainsKey(item.shoot) && ProjDef.byType[item.shoot].aiStyle==16);
        }

    }

}
