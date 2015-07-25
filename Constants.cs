using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using TAPI;
using Terraria.ID;

namespace InvisibleHand
{
    /// All the actions that the mod can perform
    public enum TIH
    {
        None,

        Sort,
        ReverseSort,

        QuickStack,
        DepositAll,
        LootAll,

        CleanStacks,

        SmartDeposit,
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

        public static readonly ButtonPlot IconReplacersPlot
            = new ButtonPlot(506, API.main.invBottom + 22,
                               0, ButtonH + 2);

        public static readonly ButtonPlot TextReplacersPlot
             = new ButtonPlot(506, API.main.invBottom + 40, 0, 26);

        public static readonly ButtonPlot InventoryButtonsPlot
            = new ButtonPlot(496, 28, ButtonW + 4, 0);

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


        // ///////////////////////////////// //
        //          Dictionaries             //
        // ///////////////////////////////// //


        /// holds the index in Terraria.Lang.inter[] corresponding
        /// to the paired actions; used to get original button lobels.
        public static readonly Dictionary<TIH, int> LangInterIndices;

        public static readonly Dictionary<TIH, string> DefaultButtonLabels;
        public static readonly Dictionary<TIH, Action> DefaultClickActions;

        public static readonly Dictionary<string, int> ButtonGridIndex;
        public static readonly Dictionary<TIH, int>    ButtonGridIndexByActionType;

        /// maps actions to the modoption defining their keybind
        public static readonly Dictionary<TIH, string>    ButtonActionToKeyBindOption;

        static Constants()
        {
            LangInterIndices = new Dictionary<TIH, int>
            {
                {TIH.LootAll,    29},
                {TIH.DepositAll, 30},
                {TIH.QuickStack, 31},
                {TIH.Rename,     61},
                {TIH.SaveName,   47},
                {TIH.CancelEdit, 63}
            };


            DefaultButtonLabels = new Dictionary<TIH, string>
            {
                // Player Inventory
                {TIH.None, ""},
                {TIH.Sort,         "Sort"},                     // 0
                {TIH.ReverseSort,  "Sort (Reverse)"},           // 1

                {TIH.CleanStacks,     "Clean Stacks"},             // 2
                // Chests
                {TIH.SmartLoot,    "Restock"},                  // 5
                {TIH.QuickStack,   "Quick Stack"},              // 6
                {TIH.SmartDeposit, "Smart Deposit"},            // 8
                {TIH.DepositAll,   "Deposit All"},              // 9
                {TIH.LootAll,      "Loot All"},                 // 11
                {TIH.Rename,       "Rename"},
                {TIH.SaveName,     "Save"}
            };

            DefaultClickActions = new Dictionary<TIH, Action>
            {
                // Player Inventory
                // a couple overloads could easily allow for using
                // plain functions like most of the other actions,
                // if I care enough.
                {TIH.None, None},

                {TIH.Sort,         () => IHPlayer.Sort()},
                {TIH.ReverseSort,  () => IHPlayer.Sort(true)},

                {TIH.CleanStacks,     IHPlayer.CleanStacks},
                // Chests
                {TIH.SmartLoot,    IHSmartStash.SmartLoot},
                {TIH.QuickStack,   IHUtils.DoQuickStack},
                {TIH.SmartDeposit, IHSmartStash.SmartDeposit},
                {TIH.DepositAll,   IHUtils.DoDepositAll},
                {TIH.LootAll,      IHUtils.DoLootAll},
                {TIH.Rename,       EditChest.DoChestEdit},
                {TIH.SaveName,     EditChest.DoChestEdit},
                {TIH.CancelEdit,   EditChest.CancelRename}
            };

            /************************************************
            * Make getting a button's texture (texels) easier
            */
            ButtonGridIndexByActionType = new Dictionary<TIH, int>
            {
                {TIH.Sort,         0},

                {TIH.ReverseSort,  1},

                {TIH.LootAll,      2},

                {TIH.DepositAll,   3},

                {TIH.SmartDeposit, 4},

                {TIH.CleanStacks,     5},

                {TIH.QuickStack,   6},

                {TIH.SmartLoot,    7},

                {TIH.Rename,       8},
                {TIH.SaveName,     8}
            };

            /*************************************************
            * Map action types to the string used for the corresponding
            * keybind in Modoptions.json
            */
            ButtonActionToKeyBindOption = new Dictionary<TIH, string>()
            {
                {TIH.CleanStacks,   "cleanStacks"},

                {TIH.DepositAll,     "depositAll"},
                {TIH.SmartDeposit,   "depositAll"},

                {TIH.LootAll,    "lootAll"},

                {TIH.QuickStack, "quickStack"},
                {TIH.SmartLoot,  "quickStack"},

                {TIH.Sort,       "sort"},
                {TIH.ReverseSort,"sort"},
                // edit chest doesn't get a keyboard shortcut. So there.
            };
        }

        // a hack; don't know if it's necessary
        public static void None() { }
    }

    public struct ButtonPlot
    {
        public Vector2 Origin;
        public Vector2 Offset;

        public ButtonPlot(Vector2 origin, Vector2 offset)
        {
            this.Origin = origin;
            this.Offset = offset;
        }

        public ButtonPlot(float origin_x, float origin_y, float offset_x, float offset_y)
        {
            this.Origin = new Vector2(origin_x, origin_y);
            this.Offset = new Vector2(offset_x, offset_y);
        }
    }


    // /// because there's no void/none/null type
    // public sealed class None
    // {
    //     private None() { }
    //     private readonly static None _none = new None();
    //     public static None none { get { return _none; } }
    // }
}
