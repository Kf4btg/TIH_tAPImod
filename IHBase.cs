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
        public static IHBase self { get; private set; }

        public static Dictionary<String, Keys> ActionKeys;
        public static Dictionary<String, bool> ModOptions;

        public static Texture2D lockedIcon;
        public LockOptions lockOptions;

        public override void OnLoad()
        {
            self = this;
            ActionKeys = new Dictionary<String, Keys>();
            ModOptions = new Dictionary<String, bool>();
        }

        public override void OnAllModsLoaded()
        {
            lockedIcon = self.textures["resources/LockIndicator"];
            CategoryDef.Initialize();
            lockOptions = new LockOptions(self);
        }

        public override void OptionChanged(Option option)
        {
            switch(option.name)
            {
                case "sort":        ActionKeys["Sort"]        = (Keys)option.Value;
                    break;
                case "cleanStacks": ActionKeys["CleanStacks"] = (Keys)option.Value;
                    break;
                case "quickStack":  ActionKeys["QuickStack"]  = (Keys)option.Value;
                    break;
                case "depositAll":  ActionKeys["DepositAll"]  = (Keys)option.Value;
                    break;
                case "lootAll":     ActionKeys["LootAll"]     = (Keys)option.Value;
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
            }
        }
    }
}
