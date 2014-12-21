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

        /* 5-bit bitmap with each bit referring to a row in the player's inventory
         with top row (the hotbar) being the right-most (least-significant) bit,
         and the 5th row on the bottom represented by the left-most (most significant) bit.
         A locked row will have its bit switched to 1.

        This can viewed as assigning a "value" to each row:
            Row    | Dec. | Binary
            ----   |------| ------
            hotbar | 1    | 00001
            row2   | 2    | 00010
            row3   | 4    | 00100
            row4   | 8    | 01000
            row5   | 16   | 10000

        Thus each possible combination of locked rows can be represented as a unique
        decimal number from 0 (no locked rows) to 31 (all rows locked).

        The state of an individual row's lock status can be extracted using
        bit-masks and the bitwise &.

        Example:    byte lockMap = 19; // 10011

                    byte[] masks = {1,2,4,8,16}; // mask is the same as the row's binary value

                    for (m=0; m<masks.Length; m++) {
                        if (lockmap & masks[m] == masks[m]) {
                            Rows[m].locked=true; // or something to this effect
                        }
                    }

        This could theoretically be expanded to a 50-bit map stored in a long to accomodate locking
        individual slots rather than just rows.
        */
        public static int LockedRowsOption = 1;
        public const byte[] ROW_BIT_MASKS = {1,2,4,8,16};

        public static bool[] LockedRows = new bool[5];

        public override void OnLoad()
        {
            self = this;
            InventoryManager.Initialize();

            // key_sort        = (Keys)options["sort"].Value;
            // key_cleanStacks = (Keys)options["cleanStacks"].Value;
            // key_quickStack  = (Keys)options["quickStack"].Value;
            // key_depositAll  = (Keys)options["depositAll"].Value;
            // key_lootAll     = (Keys)options["lootAll"].Value;

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
                case "rowLock":
                    LockedRowsOption = (int)option.Value;
                    SetLockedRows((byte)LockedRowsOption);
                    break;
            }
        }

        public static void SetLockedRows(byte rowMap)
        {
            for (int m=0; m<ROW_BIT_MASKS.Length; m++)
            {
                LockedRows[m]=((rowMap & ROW_BIT_MASKS[m]) == ROW_BIT_MASKS[m]);
            }
        }
    }


}
