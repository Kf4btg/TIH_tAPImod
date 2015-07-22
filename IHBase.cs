using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class IHBase : ModBase
    {
        private static Texture2D _lockedIcon;
        private static Texture2D _buttonGrid;
        private static Texture2D _buttonBG;

        public static IHBase Instance { get; private set; }

        /// holds the various T/F options, e.g. enable-locking
        public static Dictionary<string, bool> ModOptions { get; private set; }

        /// associates the default or player-assigned keybind with
        /// a string representing its corresponding game action.
        public static Dictionary<string, Keys> ActionKeys { get; private set; }

        // lazy-loading the textures prevents initialization errors
        // on the dedicated server.
        public static Texture2D LockedIcon
        {
            get { return _lockedIcon ?? (_lockedIcon = Instance.textures["resources/LockIndicator"]); }
        }
        public static Texture2D ButtonGrid
        {
            get { return _buttonGrid ?? (_buttonGrid = Instance.textures["resources/ButtonGrid"]); }
        }
        public static Texture2D ButtonBG
        {
            get { return _buttonBG ?? (_buttonBG = Instance.textures["resources/button_bg"]); }
        }

        /// provides notifications to buttons listening for a key to be pressed/released
        /// (just shift right now, I believe) in order to change what function they perform
        public static KeyEventProvider KEP { get; private set; }

        /// holds string-version of the keybinds for use in button labels/tooltips
        public static Dictionary<string, string> ButtonKeyTips { get; private set; }

        /// holds the game's original strings for loot-all, dep-all, quick-stack;
        /// we're going to be removing these later on, but will use their
        /// original values to replace them with newer, better buttons.
        public static Dictionary<TIH, string> OriginalButtonLabels { get; private set; }

        public ButtonLayer invButtons;
        public ButtonLayer chestButtons;
        public ButtonLayer replacerButtons;

        public ButtonLayer ReplacerButtons { get; private set; }

        /// keep track of ALL existing button contexts here.
        /// TODO: there has to be a better way to key these things
        /// than with their label.
        public Dictionary<string, IHButton> ButtonRepo { get; private set; }
        /// updated version, stores by unique id
        public Dictionary<string, ICoreButton> ButtonStore { get; private set; }
        ///the ids of those that need a state-update:
        public Stack<string> ButtonUpdates { get; private set; }

        // indices in Lang.inter[]
        internal const int iLA = 29;
        internal const int iDA = 30;
        internal const int iQS = 31;
        // rename, save, cancel edit chest
        internal const int iRC = 61;
        internal const int iSC = 47;
        internal const int iCE = 63;

        public override void OnLoad()
        {
            Instance      = this;
            ModOptions    = new Dictionary<string, bool>();
            ActionKeys    = new Dictionary<string, Keys>();
            ButtonKeyTips = new Dictionary<string, string>();

            // TODO: put this behind a modoption
            OriginalButtonLabels = new Dictionary<TIH, string>
            {
                { TIH.LootAll,    Lang.inter[iLA] },
                { TIH.DepAll,     Lang.inter[iDA] },
                { TIH.QuickStack, Lang.inter[iQS] },

                { TIH.Rename,     Lang.inter[iRC] },
                { TIH.SaveName,   Lang.inter[iSC] },
                { TIH.CancelEdit, Lang.inter[iCE] }
            };
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
            ButtonStore   = new Dictionary<string, ICoreButton>();
            ButtonUpdates = new Stack<string>();

            // TODO: does doing this here also make the mp-server freak out (since it'll be loading textures)?
            invButtons   = ButtonFactory.BuildButtons("Inventory");
            chestButtons = ButtonFactory.BuildButtons("Chest");

            if (ModOptions["TextReplacers"])
                replacerButtons = ButtonFactory.BuildButtons("TextReplacer");
            else if (ModOptions["IconReplacers"])
                replacerButtons = ButtonFactory.BuildButtons("IconReplacer");
        }

        /// Store the Key assigned for each action as a hint
        /// that can be tacked on to a button label or tooltip.
        private void SetKeyHint(string actionType, string assignedKey, string prefix = " (", string suffix = ")")
        {
            string keyHint = prefix + assignedKey + suffix;
            ButtonKeyTips[actionType] = keyHint;
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
                    SetKeyHint(option.name, option.Value.ToString());
                    break;

                // show letter of key bind in button names/tooltips
                case "showKeyBind":
                    ModOptions["ShowKeyBind"] = (bool)option.Value;
                    break;

                // slot-locking
                case "enableLocking":
                    ModOptions["LockingEnabled"] = (bool)option.Value;
                    break;

                // sort from end for...?
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
                            ModOptions["RearSortPlayer"]
                            = ModOptions["RearSortChest"] = true;
                            break;
                        case "Disabled":
                            ModOptions["RearSortPlayer"]
                            = ModOptions["RearSortChest"] = false;
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
                            ModOptions["ReverseSortPlayer"]
                            = ModOptions["ReverseSortChest"] = true;
                            break;
                        case "Disabled":
                            ModOptions["ReverseSortPlayer"]
                            = ModOptions["ReverseSortChest"] = false;
                            break;
                    }
                    break;
                case "replaceButtons":
                    switch((string)option.Value)
                    {
                        case "Text":
                            ModOptions["UseReplacers"]
                            = ModOptions["TextReplacers"] = true;
                            ModOptions["IconReplacers"]   = false;
                            break;
                        case "Buttons":
                            ModOptions["UseReplacers"]
                            = ModOptions["IconReplacers"] = true;
                            ModOptions["TextReplacers"]   = false;
                            break;
                        case "None":
                            ModOptions["UseReplacers"]
                            = ModOptions["TextReplacers"]
                            = ModOptions["IconReplacers"] = false;
                            break;
                    }
                    break;
            }
        }
    }
}
