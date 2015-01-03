using System;
// using System.Collections.Generic;
using TAPI;
using Terraria;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


namespace InvisibleHand
{
    public class IHBase : ModBase
    {
        public static Texture2D lockedIcon;

        public static Keys
            key_sort, key_cleanStacks, key_quickStack, key_depositAll, key_lootAll;

        public static bool oLockingEnabled;

        /* Whether to place items at end of inventory or beginning
            when sorting. */
        public static bool oRearSortPlayer;
        public static bool oRearSortChest;

        //default sort = reversed ?
        public static bool oRevSortPlayer;
        public static bool oRevSortChest;

        public static ModBase self { get; private set; }

        public override void OnLoad()
        {
            self = this;
            lockedIcon = self.textures["resources/LockIndicator"];

            CategoryDef.Initialize();
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
                    oLockingEnabled = (bool)option.Value;
                    break;
                case "rearSort":
                    switch ((String)option.Value)
                    {
                        case "Inventory":
                            oRearSortPlayer = true;
                            oRearSortChest  = false;
                            break;
                        case "Chests":
                            oRearSortPlayer = false;
                            oRearSortChest  = true;
                            break;
                        case "Both":
                            oRearSortPlayer = oRearSortChest = true;
                            break;
                        case "Disabled":
                            oRearSortPlayer = oRearSortChest = false;
                            break;
                    }
                    break;
                case "reverseSort":
                    switch ((String)option.Value)
                    {
                        case "Inventory":
                            oRevSortPlayer = true;
                            oRevSortChest  = false;
                            break;
                        case "Chests":
                            oRevSortPlayer = false;
                            oRevSortChest  = true;
                            break;
                        case "Both":
                            oRevSortPlayer = oRevSortChest = true;
                            break;
                        case "Disabled":
                            oRevSortPlayer = oRevSortChest = false;
                            break;
                    }
                break;
            }
        }
    }


}
