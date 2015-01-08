using System;
using System.Collections.Generic;
using TAPI;
using Terraria;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


namespace InvisibleHand
{
    public class IHBase : ModBase
    {
        public static Texture2D lockedIcon;
        private static Stack<IHUpdateable> toUpdate; // = new Stack<IHUpdateable>();

        // private static Keys
        //     key_sort, key_cleanStacks, key_quickStack, key_depositAll, key_lootAll;

        public static Dictionary<String, Keys> ActionKeys;

        // public static bool oLockingEnabled,
        //         /* Whether to place items at end of inventory or beginning when sorting. */
        //         oRearSortPlayer, oRearSortChest,
        //         //default sort = reversed ?
        //         oRevSortPlayer, oRevSortChest;

        public static Dictionary<String, bool> ModOptions;

        public static IHBase self { get; private set; }

        public override void OnLoad()
        {
            self = this;
            toUpdate = new Stack<IHUpdateable>();
            toUpdate.Push(null); //null will mark the end of the elements needing update
            ActionKeys = new Dictionary<String, Keys>();
            ModOptions = new Dictionary<String, bool>();
        }

        public override void OnAllModsLoaded()
        {
            lockedIcon = self.textures["resources/LockIndicator"];

            CategoryDef.Initialize();
        }

        public override void OptionChanged(Option option)
        {
            switch(option.name)
            {
                case "sort":
                    // key_sort = (Keys)option.Value;
                    ActionKeys["Sort"] = (Keys)option.Value;
                    break;
                case "cleanStacks":
                    // key_cleanStacks = (Keys)option.Value;
                    ActionKeys["CleanStacks"] = (Keys)option.Value;
                    break;
                case "quickStack":
                    // key_quickStack = (Keys)option.Value;
                    ActionKeys["QuickStack"] = (Keys)option.Value;
                    break;
                case "depositAll":
                    // key_depositAll = (Keys)option.Value;
                    ActionKeys["DepositAll"] = (Keys)option.Value;
                    break;
                case "lootAll":
                    // key_lootAll = (Keys)option.Value;
                    ActionKeys["LootAll"] = (Keys)option.Value;
                    break;

                case "enableLocking":
                    ModOptions["LockingEnabled"] = (bool)option.Value;
                break;

                    case "rearSort":
                    switch ((String)option.Value)
                    {
                        case "Inventory":
                            ModOptions["RearSortPlayer"] = true;
                            ModOptions["RearSortChest"]  = false;
                            break;
                        case "Chests":
                            ModOptions["RearSortPlayer"] = false;
                            ModOptions["RearSortChest"]  = true;
                            break;
                        case "Both":
                            ModOptions["RearSortPlayer"] = ModOptions["RearSortChest"] = true;
                            break;
                        case "Disabled":
                            ModOptions["RearSortPlayer"] = ModOptions["RearSortChest"] = false;
                            break;
                    }
                    break;
                    case "reverseSort":
                        switch ((String)option.Value)
                        {
                            case "Inventory":
                                ModOptions["ReverseSortPlayer"] = true;
                                ModOptions["ReverseSortChest"]  = false;
                                break;
                            case "Chests":
                                ModOptions["ReverseSortPlayer"] = false;
                                ModOptions["ReverseSortChest"]  = true;
                                break;
                            case "Both":
                                ModOptions["ReverseSortPlayer"] = ModOptions["ReverseSortChest"] = true;
                                break;
                            case "Disabled":
                                ModOptions["ReverseSortPlayer"] = ModOptions["ReverseSortChest"] = false;
                                break;
                        }
                    break;

                // case "enableLocking":
                //     oLockingEnabled = (bool)option.Value;
                //     break;
                //
                // case "rearSort":
                //     switch ((String)option.Value)
                //     {
                //         case "Inventory":
                //             oRearSortPlayer = true;
                //             oRearSortChest  = false;
                //             break;
                //         case "Chests":
                //             oRearSortPlayer = false;
                //             oRearSortChest  = true;
                //             break;
                //         case "Both":
                //             oRearSortPlayer = oRearSortChest = true;
                //             break;
                //         case "Disabled":
                //             oRearSortPlayer = oRearSortChest = false;
                //             break;
                //     }
                //     break;
                // case "reverseSort":
                //     switch ((String)option.Value)
                //     {
                //         case "Inventory":
                //             oRevSortPlayer = true;
                //             oRevSortChest  = false;
                //             break;
                //         case "Chests":
                //             oRevSortPlayer = false;
                //             oRevSortChest  = true;
                //             break;
                //         case "Both":
                //             oRevSortPlayer = oRevSortChest = true;
                //             break;
                //         case "Disabled":
                //             oRevSortPlayer = oRevSortChest = false;
                //             break;
                //     }
                // break;
            }
        }

        public static void FlagUpdate(IHUpdateable u)
        {
            toUpdate.Push(u);
        }

        public static List<IHUpdateable> GetUpdateables()
        {
            var uList = new List<IHUpdateable>();
            while (toUpdate.Peek()!=null)
            {
                uList.Add(toUpdate.Pop());
            }
            return uList;
        }
    }

    // public enum KeyID
    // {
    //     Sort,
    //     Clean,
    //     QuickStack,
    //     DepositAll,
    //     LootAll
    // }

}
