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

        public ButtonLayer ReplacerButtons { get; private set; }
        public ButtonLayer InventoryButtons { get; private set; }

        /// keep track of ALL existing button contexts here, by unique ID
        public Dictionary<string, ICoreButton> ButtonStore { get; private set; }

        /// the ids of buttons needing a state-update (i.e. OnWorldLoad call)
        // TODO: probably should just call OnWorldLoad for every button in the ButtonStore
        // rather than relying on this collection to get calls to them. It won't matter
        // to the ones that don't have any hooks for it, and there shouldn't ever be so many
        // buttons that this would call a huge slowdown.
        public Stack<string> ButtonUpdates { get; private set; }

        public override void OnLoad()
        {
            Instance      = this;
            ModOptions    = new Dictionary<string, bool>();
            ActionKeys    = new Dictionary<string, Keys>();
            ButtonKeyTips = new Dictionary<string, string>();

            OriginalButtonLabels = new Dictionary<TIH, string>();

            // TODO: put this behind a modoption
            // pull values out of Lang.inter to populate OBL
            foreach (var kvp in Constants.LangInterIndices)
            {
                OriginalButtonLabels[kvp.Key] = Lang.inter[kvp.Value];
            }

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
            ButtonStore   = new Dictionary<string, ICoreButton>();
            ButtonUpdates = new Stack<string>();

            // TODO: does doing this here also make the mp-server freak out (since it'll be loading textures)?
            InventoryButtons = PlayerInventoryButtons.New();

            // if (ModOptions["TextReplacers"])
                ReplacerButtons = ChestButtonReplacerLayer.New(ModOptions["TextReplacers"]);
            // else if (ModOptions["IconReplacers"])

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

                // replace the vanilla LA/DA/QS buttons with new...
                case "replaceButtons":
                    switch((string)option.Value)
                    {
                        // Text buttons, like the originals
                        case "Text":
                            ModOptions["UseReplacers"]
                            = ModOptions["TextReplacers"] = true;
                            ModOptions["IconReplacers"]   = false;
                            break;
                        // Icon buttons, with pictures
                        case "Buttons":
                            ModOptions["UseReplacers"]
                            = ModOptions["IconReplacers"] = true;
                            ModOptions["TextReplacers"]   = false;
                            break;
                        // Don't replace them at all
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
