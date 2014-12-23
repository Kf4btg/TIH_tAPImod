using System;
using System.Collections.Generic;
using TAPI;
using Terraria;
using Microsoft.Xna.Framework.Input;

namespace InvisibleHand
{
    public class IHBase : ModBase
    {

        public static Keys
            key_sort, key_cleanStacks, key_quickStack, key_depositAll, key_lootAll;

        public static bool lockingEnabled;

        /* Whether to place items at end of inventory or beginning
            when sorting.
            Possible values: */
        public const int RS_DISABLE = 0;    //0 = disabled (sort at beginning)
        public const int RS_PLAYER = 1;    //1 = enabled for player inventory
        public const int RS_CHEST = 2;   //2 = enabled for chest inventories
        public const int RS_BOTH = 3;    //3 = enabled both chest and player inventories

        public static int opt_reverseSort;

        public static ModBase self { get; private set; }

        public override void OnLoad()
        {
            self = this;

            InventoryManager.Initialize();
        }

        public override void OptionChanged(Option option)
        {
            switch(option.name)
            {
                case "sort":
                    key_sort = (Keys)option.Value;
                    break;
                case "cleanStacks":
                    key_cleanStacks = (Keys)option.Value;
                    break;
                case "quickStack":
                    key_quickStack = (Keys)option.Value;
                    break;
                case "depositAll":
                    key_depositAll = (Keys)option.Value;
                    break;
                case "lootAll":
                    key_lootAll = (Keys)option.Value;
                    break;
                case "enableLocking":
                    lockingEnabled = (bool)option.Value;
                    break;
                case "reverseSort":
                    switch ((String)option.Value)
                    {
                        case "Player Inventory":
                            opt_reverseSort=RS_PLAYER;
                            break;
                        case "Chests":
                            opt_reverseSort=RS_CHEST;
                            break;
                        case "Both":
                            opt_reverseSort=RS_BOTH;
                            break;
                        case "Disabled":
                            opt_reverseSort=RS_DISABLE;
                            break;
                    }
                    break;


            }
        }
    }


}
