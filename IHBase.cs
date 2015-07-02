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
        public static Texture2D ButtonGrid { get; private set; }
        public static Texture2D ButtonBG { get; private set; }
        public static KeyEventProvider KEP;

        public ButtonLayer invButtons;
        public ButtonLayer chestButtons;

        //keep track of ALL existing button contexts here.
        public Dictionary<String, IHButton> ButtonRepo;
        //the ids of those that need a state-update:
        public Stack<String> ButtonUpdates;

        public override void OnLoad()
        {
            self = this;
            ActionKeys = new Dictionary<String, Keys>();
            ModOptions = new Dictionary<String, bool>();
        }

        public override void OnAllModsLoaded()
        {
            lockedIcon = self.textures["resources/LockIndicator"];

            // this should prevent dictionary-key exceptions if mod-options page not visited
            // This also calls the button initializer
            foreach (Option o in options)
            {
                OptionChanged(o);
            }
            // InitButtons();

            CategoryDef.Initialize();
        }

        private void InitButtons()
        {
            KEP           = new KeyEventProvider();
            ButtonRepo    = new Dictionary<String, IHButton>();
            ButtonUpdates = new Stack<String>();

            //Add some actions to the ButtonParser
            ButtonParser.RegisterActions( new Dictionary<String,Action>
            {
                { "pSort", () => IHOrganizer.SortPlayerInv(Main.localPlayer, IHBase.ModOptions["ReverseSortPlayer"]) },
                { "pSortRev", () => IHOrganizer.SortPlayerInv(Main.localPlayer, !IHBase.ModOptions["ReverseSortPlayer"]) },

                { "cSort", () => IHOrganizer.SortChest(Main.localPlayer.chestItems, IHBase.ModOptions["ReverseSortChest"]) },
                { "cSortRev", () => IHOrganizer.SortChest(Main.localPlayer.chestItems, !IHBase.ModOptions["ReverseSortChest"]) },



                { "toggleLockDA", () => {
                     Main.PlaySound(22); //lock sound
                     IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.DA); }},
                {"toggleLockQS", () => {
                     Main.PlaySound(22); //lock sound
                     IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.DA); }},

                {"SmartLoot", IHSmartStash.SmartLoot},
                {"SmartDeposit", IHSmartStash.SmartDeposit},

                {"QuickStack", IHUtils.DoQuickStack},
                {"DepositAll", IHUtils.DoDepositAll},
                {"LootAll", IHUtils.DoLootAll}
             }
            );





            ButtonGrid   = textures["resources/ButtonGrid"];
            ButtonBG     = textures["resources/button_bg"];
            invButtons   = ButtonMaker.GetButtons("Inventory");
            chestButtons = ButtonMaker.GetButtons("Chest");
        }

        /**
         * For Inter-Mod communication.
         * Currently only to support loading buttons from external configuration files.
         * And SPECIFICALLY to support adding actions to events on said buttons.
         */
        public override object OnModCall(ModBase mod, params object[] arguments)
        {
            if (arguments.Length == 0 || !(arguments[0] is string)) {
                throw new ArgumentException("First argument for mod calls to 'InvisibleHand' must be a string.");
            }
            switch((string)arguments[0])
            {
                case "AddButtonEventAction":
                    return ButtonParser.GetAction((string)arguments[2]);
                    // switch((string)arguments[1])
                    // {
                    //     case "onClick":
                    //         break;
                    //     case "onRightClick":
                    //         break;
                    // }
                    // break;
            }
        }

        public override void OptionChanged(Option option)
        {
            switch(option.name)
            {
                case "sort":
                case "cleanStacks":
                case "quickStack":
                case "depositAll":
                case "lootAll":
                    ActionKeys[option.name] = (Keys)option.Value;
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

                case "buttonDisplay":
                    ModOptions["ShowVanillaButtons"] = ((string)option.Value)=="Vanilla";
                    InitButtons(); //create or reset the button layers
                    break;
            }
        }
    }
}
