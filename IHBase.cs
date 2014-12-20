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

        public override void OnLoad()
        {
            InventoryManager.Initialize();

            key_sort        = (Keys)options["sort"].Value;
            key_cleanStacks = (Keys)options["sort"].Value;
            key_quickStack  = (Keys)options["sort"].Value;
            key_depositAll  = (Keys)options["sort"].Value;
            key_lootAll     = (Keys)options["sort"].Value;

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
