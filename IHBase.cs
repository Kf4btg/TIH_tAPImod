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
        public static IHBase Instance { get; private set; }

        // holds the various T/F options, e.g. enable-locking
        public static Dictionary<string, bool> ModOptions;
        // associates the default or player-assigned keybind with
        // a string representing its corresponding game action.
        public static Dictionary<string, Keys> ActionKeys;
        // holds string-version of the keybinds for use in button tooltips
        public static Dictionary<string, string> ButtonKeyTips;

        private static Texture2D lockedIcon;
        private static Texture2D buttonGrid;
        private static Texture2D buttonBG;

        // lazy-loading the textures prevents errors initialization errors
        // on the dedicated server.
        public static Texture2D LockedIcon
        {
            get { return lockedIcon ?? (lockedIcon = Instance.textures["resources/LockIndicator"]); }
        }
        public static Texture2D ButtonGrid
        {
            get { return buttonGrid ?? (buttonGrid = Instance.textures["resources/ButtonGrid"]); }
        }
        public static Texture2D ButtonBG
        {
            get { return buttonBG ?? (buttonBG = Instance.textures["resources/button_bg"]); }
        }

        public static KeyEventProvider KEP;

        public ButtonLayer invButtons;
        public ButtonLayer chestButtons;

        //keep track of ALL existing button contexts here.
        public Dictionary<string, IHButton> ButtonRepo;
        //the ids of those that need a state-update:
        public Stack<string> ButtonUpdates;

        public override void OnLoad()
        {
            Instance = this;
            ModOptions = new Dictionary<string, bool>();
            ActionKeys = new Dictionary<string, Keys>();
            ButtonKeyTips = new Dictionary<string, string>();
        }

        public override void OnAllModsLoaded()
        {
            // this should prevent dictionary-key exceptions if mod-options page not visited
            foreach (Option o in options)
            {
                OptionChanged(o);
            }
            InitButtons();
            CategoryDef.Initialize();
        }

        private void InitButtons()
        {
            KEP           = new KeyEventProvider();
            ButtonRepo    = new Dictionary<string, IHButton>();
            ButtonUpdates = new Stack<string>();

            // TODO: does doing this here also make the mp-server freak out?
            invButtons   = ButtonMaker.GetButtons("Inventory");
            chestButtons = ButtonMaker.GetButtons("Chest");
        }

        public override void OptionChanged(Option option)
        {
            switch(option.name)
            {
                // keybinds
                case "sort":
                case "cleanStacks":
                case "quickStack":
                case "depositAll":
                case "lootAll":
                    ActionKeys[option.name] = (Keys)option.Value;
                    ButtonKeyTips[option.name] = " (" + option.Value.ToString() + ")";
                    break;

                // slot-locking
                case "enableLocking":
                    ModOptions["LockingEnabled"] = (bool)option.Value;
                    break;

                //sort from end for...?
                case "rearSort":
                    switch ((string)option.Value)
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

                // reverse sort order of...?
                case "reverseSort":
                    switch ((string)option.Value)
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
