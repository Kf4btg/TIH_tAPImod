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
            }
        }
    }


}
