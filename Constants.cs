using System;
using System.Collections.Generic;
using TAPI;
using Terraria;
using Terraria.ID;

namespace InvisibleHand
{
    public enum VAction { QS, DA, LA }

    public enum IHAction { Sort, Stack, Deposit, Refill }

    public enum ItemCat
    {
        COIN,
        PICK,
        AXE,
        HAMMER,
        TOOL,
        MECH,
        MELEE,
        RANGED,
        BOMB,
        AMMO,
        MAGIC,
        SUMMON,
        PET,
        HEAD,
        BODY,
        LEGS,
        ACCESSORY,
        VANITY,
        POTION,
        CONSUME,
        BAIT,
        DYE,
        PAINT,
        ORE,
        BAR,
        GEM,
        SEED,
        LIGHT,
        CRAFT,
        FURNITURE,
        STATUE,
        WALLDECO,
        BANNER,
        CLUTTER,
        WOOD,
        BLOCK,
        BRICK,
        TILE,
        WALL,
        MISC_MAT,
        SPECIAL,    //Boss summoning items, heart containers, mana crystals
        OTHER
    }

    public static class Constants
    {
        //the ItemCat Enum defines the actual Sort Order of the categories,
        // but this defines in which order an item will be checked against
        // the category matching rules. This is important due to a kind
        // of "sieve" or "cascade" effect, where items that would have matched
        // a certain category were instead caught by an earlier one.
        public static readonly ItemCat[] CheckOrder =  {
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
            ItemCat.SPECIAL,
            ItemCat.PET,
            ItemCat.VANITY,
            ItemCat.MELEE,
            ItemCat.BOMB,
            ItemCat.RANGED,
            ItemCat.AMMO,
            ItemCat.MAGIC,
            ItemCat.SUMMON,
            ItemCat.POTION,
            ItemCat.CONSUME,
            ItemCat.BAIT,
            ItemCat.DYE,
            ItemCat.PAINT,
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
            ItemCat.BRICK,
            ItemCat.BLOCK,
            ItemCat.TILE,
            ItemCat.WALL,
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
            TileID.WormCage, TileID.GrasshopperCage } ); //blergh

        public static readonly
        HashSet<int> TileGroupCrafting = new HashSet<int>( new int[] {
            TileID.WorkBenches, TileID.Anvils, TileID.MythrilAnvil, TileID.AdamantiteForge,
            TileID.CookingPots, TileID.Furnaces, TileID.Hellforge, TileID.Loom, TileID.Kegs,
            TileID.Sawmill, TileID.TinkerersWorkbench, TileID.CrystalBall, TileID.Blendomatic,
            TileID.MeatGrinder, TileID.Extractinator, TileID.Solidifier, TileID.DyeVat,
            TileID.ImbuingStation, TileID.Autohammer, TileID.HeavyWorkBench, TileID.BoneWelder,
            TileID.FleshCloningVaat, TileID.GlassKiln, TileID.LihzahrdFurnace, TileID.LivingLoom,
            TileID.SkyMill, TileID.IceMachine, TileID.SteampunkBoiler, TileID.HoneyDispenser } ); //also blergh

        public static readonly
        HashSet<int> TileGroupOre      = new HashSet<int>( new int[] {
            TileID.Meteorite, TileID.Obsidian, TileID.Hellstone }); //(get others by name)

        public static readonly
        HashSet<int> TileGroupCoin     = new HashSet<int>( new int[] {
            TileID.CopperCoinPile, TileID.SilverCoinPile, TileID.GoldCoinPile, TileID.PlatinumCoinPile });

        public static readonly
        HashSet<int> TileGroupSeed     = new HashSet<int>( new int[] {
            TileID.ImmatureHerbs, TileID.Saplings /*Acorn*/, TileID.Pumpkins /*Pumpkin Seed*/ } );
            // get the rest by EndsWith("Seeds")
    }
}