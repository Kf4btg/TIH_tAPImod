using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using Terraria.ID;

namespace InvisibleHand
{
    /// All the actions that the mod can perform
    public enum TIH
    {
        None,

        // smarted button creation should
        // eliminate the need for separate
        // inventory/chest sort actions.
        Sort,
        ReverseSort,
        // The same is likely true for Clean... as well

        QuickStack,
        DepAll,
        LootAll,

        SortInv,
        RSortInv,
        CleanInv,

        SortChest,
        RSortChest,
        CleanChest,

        SmartDep,
        SmartLoot,

        // simply for replacing the button with a visually-consistent
        // tiled-graphic version when someone chooses non-text
        // replacements
        Rename,
        SaveName,
        CancelEdit
    }

    ///the ItemCat Enum defines the actual Sort Order of the categories
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
        SPECIAL,    // Boss summoning items, heart containers, mana crystals
        OTHER
    }

    public enum Sound
    {
        // just because it causes issues when there's no value for 0
        // in an enum; 0 is the dig sound, but this mod doesn't use it
        Dig       =  0,
        ItemMoved =  7,
        MouseOver = 12,
        Coins     = 18,
        Lock      = 22
    }

    public static class Constants
    {
        // pulled from Main.DrawInventory  !ref:Main:#22137.0#
        public const float CHEST_INVENTORY_SCALE    = 0.755f;
        public const float PLAYER_INVENTORY_SCALE   = 0.85f;

        public static readonly Color InvSlotColor   = new Color( 63,  65, 151, 255);  // bluish
        public static readonly Color ChestSlotColor = new Color(104,  52,  52, 255);  // reddish
        public static readonly Color BankSlotColor  = new Color(130,  62, 102, 255);  // pinkish
        public static readonly Color EquipSlotColor = new Color( 50, 106,  46, 255);  // greenish

        // width and height of button
        public const int ButtonW = 32;
        public const int ButtonH = 32;

        ///the ItemCat Enum defines the actual Sort Order of the categories,
        /// but this defines in which order an item will be checked against
        /// the category matching rules. This is important due to a kind
        /// of "sieve" or "cascade" effect, where items that would have matched
        /// a certain category were instead caught by an earlier one.
        public static readonly ItemCat[] CheckOrder =
        {
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
        /** ***********************************************************************
        Create several hashsets to quickly check values of "item.createTile" to aid in categorization/sorting.
        Initialize them here with an anonymous array to avoid the resizing penalty of .Add()
        */
        public static readonly HashSet<int>
            TileGroupFurniture = new HashSet<int>(new int[] {
                    TileID.ClosedDoor,
                    TileID.Tables,
                    TileID.Chairs,
                    TileID.Platforms,
                    TileID.Beds,
                    TileID.Pianos,
                    TileID.Dressers,
                    TileID.Benches,
                    TileID.Bathtubs,
                    TileID.Bookcases,
                    TileID.GrandfatherClocks,
                    TileID.Containers,
                    TileID.PiggyBank,
                    TileID.Signs,
                    TileID.Safes,
                    TileID.Thrones,
                    TileID.WoodenPlank,
                    TileID.Mannequin,
                    TileID.Womannequin
            });

        public static readonly HashSet<int>
            TileGroupLighting = new HashSet<int>(new int[] {
                    TileID.Torches,
                    TileID.Candles,
                    TileID.Chandeliers,
                    TileID.HangingLanterns,
                    TileID.Lamps,
                    TileID.Candelabras,
                    TileID.Jackolanterns,
                    TileID.ChineseLanterns,
                    TileID.SkullCandles,
                    TileID.Campfire,
                    TileID.FireflyinaBottle,
                    TileID.LightningBuginaBottle,
                    TileID.WaterCandle
            });

        public static readonly HashSet<int>
            TileGroupStatue = new HashSet<int>(new int[] {
                    TileID.Tombstones,
                    TileID.Statues,
                    TileID.WaterFountain,
                    TileID.AlphabetStatues,
                    TileID.BubbleMachine
            });

        public static readonly HashSet<int>
            TileGroupWallDeco = new HashSet<int>(new int[] {
                    TileID.Painting2x3,
                    TileID.Painting3x2,
                    TileID.Painting3x3,
                    TileID.Painting4x3,
                    TileID.Painting6x4
            });

        public static readonly HashSet<int>
            TileGroupClutter = new HashSet<int>(new int[] {
                    TileID.Bottles,
                    TileID.Bowls,
                    TileID.BeachPiles,
                    TileID.Books,
                    TileID.Coral,
                    TileID.ShipInABottle,
                    TileID.BlueJellyfishBowl,
                    TileID.GreenJellyfishBowl,
                    TileID.PinkJellyfishBowl,
                    TileID.SeaweedPlanter,
                    TileID.ClayPot,
                    TileID.BunnyCage,
                    TileID.SquirrelCage,
                    TileID.MallardDuckCage,
                    TileID.DuckCage,
                    TileID.BirdCage,
                    TileID.BlueJay,
                    TileID.CardinalCage,
                    TileID.FishBowl,
                    TileID.SnailCage,
                    TileID.GlowingSnailCage,
                    TileID.MonarchButterflyJar,
                    TileID.PurpleEmperorButterflyJar,
                    TileID.RedAdmiralButterflyJar,
                    TileID.UlyssesButterflyJar,
                    TileID.SulphurButterflyJar,
                    TileID.TreeNymphButterflyJar,
                    TileID.ZebraSwallowtailButterflyJar,
                    TileID.JuliaButterflyJar,
                    TileID.ScorpionCage,
                    TileID.BlackScorpionCage,
                    TileID.FrogCage,
                    TileID.MouseCage,
                    TileID.PenguinCage,
                    TileID.WormCage,
                    TileID.GrasshopperCage
            }); // blergh

        public static readonly HashSet<int>
            TileGroupCrafting = new HashSet<int>(new int[] {
                    TileID.WorkBenches,
                    TileID.Anvils,
                    TileID.MythrilAnvil,
                    TileID.AdamantiteForge,
                    TileID.CookingPots,
                    TileID.Furnaces,
                    TileID.Hellforge,
                    TileID.Loom,
                    TileID.Kegs,
                    TileID.Sawmill,
                    TileID.TinkerersWorkbench,
                    TileID.CrystalBall,
                    TileID.Blendomatic,
                    TileID.MeatGrinder,
                    TileID.Extractinator,
                    TileID.Solidifier,
                    TileID.DyeVat,
                    TileID.ImbuingStation,
                    TileID.Autohammer,
                    TileID.HeavyWorkBench,
                    TileID.BoneWelder,
                    TileID.FleshCloningVaat,
                    TileID.GlassKiln,
                    TileID.LihzahrdFurnace,
                    TileID.LivingLoom,
                    TileID.SkyMill,
                    TileID.IceMachine,
                    TileID.SteampunkBoiler,
                    TileID.HoneyDispenser
            }); // also blergh

        public static readonly HashSet<int>
            TileGroupOre = new HashSet<int>(new int[] {
                    TileID.Meteorite,
                    TileID.Obsidian,
                    TileID.Hellstone
            }); // (get others by name)

        public static readonly HashSet<int>
            TileGroupCoin = new HashSet<int>(new int[] {
                    TileID.CopperCoinPile,
                    TileID.SilverCoinPile,
                    TileID.GoldCoinPile,
                    TileID.PlatinumCoinPile
            });

        public static readonly HashSet<int>
            TileGroupSeed = new HashSet<int>(new int[] {
                    TileID.ImmatureHerbs,
                    TileID.Saplings, /*Acorn*/
                    TileID.Pumpkins  /*Pumpkin Seed*/
            });                      // get the rest by EndsWith("Seeds")


        public static readonly string[] ButtonLabels =
        {
            // Player Inventory
            "Sort",                     // 0
            "Sort (Reverse)",           // 1
            "Clean Stacks",             // 2
            // Chests
            "Sort Chest",               // 3
            "Sort Chest (Reverse)",     // 4
            "Restock",                  // 5
            "Quick Stack",              // 6
            "Quick Stack (Locked)",     // 7
            "Smart Deposit",            // 8
            "Deposit All",              // 9
            "Deposit All (Locked)",     // 10
            "Loot All"                  // 11
        };

        public static readonly Dictionary<TIH, string> DefaultButtonLabels;
        public static readonly Dictionary<TIH, Action> DefaultClickActions;

        public static readonly Dictionary<string, int> ButtonGridIndex;
        public static readonly Dictionary<TIH, int>    ButtonGridIndexByActionType;

        /// maps labels to the modoption defining their keybind
        public static readonly Dictionary<string, string> ButtonLabelToKBOption;
        public static readonly Dictionary<TIH, string>    ButtonActionToKeyBindOption;

        static Constants()
        {
            DefaultButtonLabels = new Dictionary<TIH, string>
            {
                // Player Inventory
                {TIH.None, ""},
                {TIH.Sort,       "Sort"},                     // 0
                {TIH.ReverseSort,"Sort (Reverse)"},           // 1

                {TIH.SortInv,    "Sort"},                     // 0
                {TIH.RSortInv,   "Sort (Reverse)"},           // 1
                {TIH.CleanInv,   "Clean Stacks"},             // 2
                {TIH.CleanChest, "Clean Chest Stacks"},       // 2
                // Chests
                {TIH.SortChest,  "Sort Chest"},               // 3
                {TIH.RSortChest, "Sort Chest (Reverse)"},     // 4
                {TIH.SmartLoot,  "Restock"},                  // 5
                {TIH.QuickStack, "Quick Stack"},              // 6
                {TIH.SmartDep,   "Smart Deposit"},            // 8
                {TIH.DepAll,     "Deposit All"},              // 9
                {TIH.LootAll,    "Loot All"},                 // 11
                {TIH.Rename,     "Rename"},
                {TIH.SaveName,   "Save"}
            };

            DefaultClickActions = new Dictionary<TIH, Action>
            {
                // Player Inventory
                // a couple overloads could easily allow for using
                // plain functions like most of the other actions,
                // if I care enough.
                {TIH.None, None},

                {TIH.Sort,       () => IHPlayer.Sort()},
                {TIH.ReverseSort,() => IHPlayer.Sort(true)},

                {TIH.SortInv,    () => IHPlayer.SortInventory()},
                {TIH.RSortInv,   () => IHPlayer.SortInventory(true)},
                {TIH.CleanInv,   IHPlayer.CleanInventoryStacks},
                {TIH.CleanChest, IHPlayer.CleanChestStacks},
                // Chests
                {TIH.SortChest,  () => IHPlayer.SortChest()},
                {TIH.RSortChest, () => IHPlayer.SortChest(true)},
                {TIH.SmartLoot,  IHSmartStash.SmartLoot},
                {TIH.QuickStack, IHUtils.DoQuickStack},
                {TIH.SmartDep,   IHSmartStash.SmartDeposit},
                {TIH.DepAll,     IHUtils.DoDepositAll},
                {TIH.LootAll,    IHUtils.DoLootAll},
                {TIH.Rename,     EditChest.DoChestEdit},
                {TIH.SaveName,   EditChest.DoChestEdit},
                {TIH.CancelEdit, EditChest.CancelRename}
            };

            /************************************************
            * Make getting a button's texture (texels) easier
            */
            ButtonGridIndexByActionType = new Dictionary<TIH, int>
            {
                {TIH.Sort,       0},
                {TIH.SortInv,    0},
                {TIH.SortChest,  0},

                {TIH.ReverseSort,1},
                {TIH.RSortInv,   1},
                {TIH.RSortChest, 1},

                {TIH.LootAll,    2},

                {TIH.DepAll,     3},

                {TIH.SmartDep,   4},

                {TIH.CleanInv,   5},
                {TIH.CleanChest, 5},

                {TIH.QuickStack, 6},

                {TIH.SmartLoot,  7},

                {TIH.Rename,     8},
                {TIH.SaveName,   8}
            };

            /*************************************************
            * Map action types to the string used for the corresponding
            * keybind in Modoptions.json
            */
            ButtonActionToKeyBindOption = new Dictionary<TIH, string>()
            {
                {TIH.CleanInv,   "cleanStacks"},
                {TIH.CleanChest, "cleanStacks"},

                {TIH.DepAll,     "depositAll"},
                {TIH.SmartDep,   "depositAll"},

                {TIH.LootAll,    "lootAll"},

                {TIH.QuickStack, "quickStack"},
                {TIH.SmartLoot,  "quickStack"},

                {TIH.Sort,       "sort"},
                {TIH.ReverseSort,"sort"},
                {TIH.SortInv,    "sort"},
                {TIH.SortChest,  "sort"},
                {TIH.RSortInv,   "sort"},
                {TIH.RSortChest, "sort"}
                // edit chest doesn't get a keyboard shortcut. So there.
            };
        }

        /// this lets buttons go through the ButtonFactory
        /// methods even if they don't have a simple static
        /// Action for onClick(); obviously the real action
        /// should be added afterwards.
        public static void None() { }
        // yes it's a hack and I'll try to fix it later
    }


    // /// because there's no void/none/null type
    // public sealed class None
    // {
    //     private None() { }
    //     private readonly static None _none = new None();
    //     public static None none { get { return _none; } }
    // }
}
