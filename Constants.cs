using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace InvisibleHand
{
    /// All the actions that the mod can perform
    public enum TIH
    {
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
        SmartLoot
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
        SPECIAL,    //Boss summoning items, heart containers, mana crystals
        OTHER
    }

    public static class Constants
    {
        //pulled from Main.DrawInventory  !ref:Main:#22137.0#
        public const float CHEST_INVENTORY_SCALE = 0.755f;
        public const float PLAYER_INVENTORY_SCALE = 0.85f;

        public static readonly Color InvSlotColor   = new Color(63,65,151,255);  //bluish
        public static readonly Color ChestSlotColor = new Color(104,52,52,255);  //reddish
        public static readonly Color BankSlotColor  = new Color(130,62,102,255); //pinkish
        public static readonly Color EquipSlotColor = new Color(50,106,46,255); //greenish

        //width and height of button
        public const int ButtonW = 32;
        public const int ButtonH = 32;

        ///the ItemCat Enum defines the actual Sort Order of the categories,
        /// but this defines in which order an item will be checked against
        /// the category matching rules. This is important due to a kind
        /// of "sieve" or "cascade" effect, where items that would have matched
        /// a certain category were instead caught by an earlier one.
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
        /** ***********************************************************************
        Create several hashsets to quickly check values of "item.createTile" to aid in categorization/sorting.
        Initialize them here with an anonymous array to avoid the resizing penalty of .Add()
        */
        public static readonly
        HashSet<int> TileGroupFurniture  = new HashSet<int>( new int[] {
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

        public static readonly
        HashSet<int> TileGroupLighting   = new HashSet<int>( new int[] {
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

        public static readonly
        HashSet<int> TileGroupStatue     = new HashSet<int>( new int[] {
            TileID.Tombstones,
            TileID.Statues,
            TileID.WaterFountain,
            TileID.AlphabetStatues,
            TileID.BubbleMachine
            });

        public static readonly
        HashSet<int> TileGroupWallDeco   = new HashSet<int>( new int[] {
            TileID.Painting2x3,
            TileID.Painting3x2,
            TileID.Painting3x3,
            TileID.Painting4x3,
            TileID.Painting6x4
            });

        public static readonly
        HashSet<int> TileGroupClutter    = new HashSet<int>( new int[] {
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
            }); //blergh

        public static readonly
        HashSet<int> TileGroupCrafting = new HashSet<int>( new int[] {
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
            }); //also blergh

        public static readonly
        HashSet<int> TileGroupOre      = new HashSet<int>( new int[] {
            TileID.Meteorite,
            TileID.Obsidian,
            TileID.Hellstone
            }); //(get others by name)

        public static readonly
        HashSet<int> TileGroupCoin     = new HashSet<int>( new int[] {
            TileID.CopperCoinPile,
            TileID.SilverCoinPile,
            TileID.GoldCoinPile,
            TileID.PlatinumCoinPile
            });

        public static readonly
        HashSet<int> TileGroupSeed     = new HashSet<int>( new int[] {
            TileID.ImmatureHerbs,
            TileID.Saplings, /*Acorn*/
            TileID.Pumpkins /*Pumpkin Seed*/
            }); // get the rest by EndsWith("Seeds")


        public static readonly string[] ButtonLabels = {
            //Player Inventory
            "Sort",                     //0
            "Sort (Reverse)",           //1
            "Clean Stacks",             //2
            //Chests
            "Sort Chest",               //3
            "Sort Chest (Reverse)",     //4
            "Restock",                  //5
            "Quick Stack",              //6
            "Quick Stack (Locked)",     //7
            "Smart Deposit",            //8
            "Deposit All",              //9
            "Deposit All (Locked)",     //10
            "Loot All"                  //11
        };

        public static readonly Dictionary<TIH, string> DefaultButtonLabels;
        public static readonly Dictionary<TIH, Action> DefaultClickActions;

        public static readonly Dictionary<string, int> ButtonGridIndex;
        public static readonly Dictionary<TIH, int> ButtonGridIndexByActionType;

        /// maps labels to the modoption defining their keybind
        public static readonly Dictionary<string, string> ButtonLabelToKBOption;
        public static readonly Dictionary<TIH, string> ButtonActionToKeyBindOption;

        static Constants()
        {
            DefaultButtonLabels = new Dictionary<TIH, string>
            {
                //Player Inventory
                {TIH.SortInv,    "Sort"},                     //0
                {TIH.RSortInv,   "Sort (Reverse)"},           //1
                {TIH.CleanInv,   "Clean Stacks"},             //2
                {TIH.CleanChest, "Clean Chest Stacks"},             //2
                //Chests
                {TIH.SortChest,  "Sort Chest"},               //3
                {TIH.RSortChest, "Sort Chest (Reverse)"},     //4
                {TIH.SmartLoot,  "Restock"},                  //5
                {TIH.QuickStack, "Quick Stack"},              //6
                {TIH.SmartDep,   "Smart Deposit"},            //8
                {TIH.DepAll,     "Deposit All"},              //9
                {TIH.LootAll,    "Loot All"}                  //11
            };

            DefaultClickActions = new Dictionary<TIH, Action>
            {
                //Player Inventory
                {TIH.SortInv,    () => IHPlayer.SortInventory()},
                {TIH.RSortInv,   () => IHPlayer.SortInventory(true)},
                {TIH.CleanInv,   IHPlayer.CleanInventoryStacks},
                {TIH.CleanChest, IHPlayer.CleanChestStacks},
                //Chests
                {TIH.SortChest,  () => IHPlayer.SortChest()},
                {TIH.RSortChest, () => IHPlayer.SortChest(true)},
                {TIH.SmartLoot,  IHSmartStash.SmartLoot},
                {TIH.QuickStack, IHUtils.DoQuickStack},
                {TIH.SmartDep,   IHSmartStash.SmartDeposit},
                {TIH.DepAll,     IHUtils.DoDepositAll},
                {TIH.LootAll,    IHUtils.DoLootAll}
            };

            /************************************************
            * Make getting a button's texture (texels) easier
            */
            ButtonGridIndex = new Dictionary<string,int>(12);
            //sort, sort chest
            ButtonGridIndex.Add(ButtonLabels[0],0);
            ButtonGridIndex.Add(ButtonLabels[3],0);
            //sort reverse, sort chest reverse
            ButtonGridIndex.Add(ButtonLabels[1],1);
            ButtonGridIndex.Add(ButtonLabels[4],1);
            //loot all
            ButtonGridIndex.Add(ButtonLabels[11],2);
            //deposit all, DA+locked
            ButtonGridIndex.Add(ButtonLabels[9],3);
            ButtonGridIndex.Add(ButtonLabels[10],3);
            //smart deposit
            ButtonGridIndex.Add(ButtonLabels[8],4);
            //clean stacks
            ButtonGridIndex.Add(ButtonLabels[2],5);
            //quick stack, QS+locked
            ButtonGridIndex.Add(ButtonLabels[6],6);
            ButtonGridIndex.Add(ButtonLabels[7],6);
            // restock/smartloot
            ButtonGridIndex.Add(ButtonLabels[5],7);

            // and now do it with the action enum
            ButtonGridIndexByActionType = new Dictionary<TIH, int>
            {
                {TIH.SortInv,    0},
                {TIH.SortChest,  0},

                {TIH.RSortInv,   1},
                {TIH.RSortChest, 1},

                {TIH.LootAll,    2},

                {TIH.DepAll,     3},

                {TIH.SmartDep,   4},

                {TIH.CleanInv,   5},
                {TIH.CleanChest, 5},

                {TIH.QuickStack, 6},

                {TIH.SmartLoot,  7}
            };

            /*************************************************
            * Map labels to the string used for the corresponding
            * keybind in Modoptions.json
            */
            ButtonLabelToKBOption = new Dictionary<string, string>
            {
                //s, s-r, sc, sc-r
                { ButtonLabels[0], "sort" },
                { ButtonLabels[1], "sort" },
                { ButtonLabels[3], "sort" },
                { ButtonLabels[4], "sort" },
                //c.stacks
                { ButtonLabels[2], "cleanStacks" },
                //restock, qs, qs-l
                { ButtonLabels[5], "quickStack" },
                { ButtonLabels[6], "quickStack" },
                { ButtonLabels[7], "quickStack" },
                //sd, da, da-l
                { ButtonLabels[8], "depositAll" },
                { ButtonLabels[9], "depositAll" },
                { ButtonLabels[10],"depositAll" },
                //lootall
                { ButtonLabels[11],"lootAll" }
            };

            // and now do it with the action enum
            ButtonActionToKeyBindOption = new Dictionary<TIH, string>()
            {
                {TIH.CleanInv,   "cleanStacks"},
                {TIH.CleanChest, "cleanStacks"},

                {TIH.DepAll,     "depositAll"},
                {TIH.SmartDep,   "depositAll"},

                {TIH.LootAll,    "lootAll"},

                {TIH.QuickStack, "quickStack"},
                {TIH.SmartLoot,  "quickStack"},

                {TIH.SortInv,    "sort"},
                {TIH.SortChest,  "sort"},
                {TIH.RSortInv,   "sort"},
                {TIH.RSortChest, "sort"},
            };
        }
    }
}
