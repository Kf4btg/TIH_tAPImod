using System;
using Microsoft.Xna.Framework.Input;
using TAPI;
using Terraria;


namespace InvisibleHand
{

    public class IHPlayer : ModPlayer
    {

        public bool control_sort;
        public bool control_clean;      //combine stacks

        public bool control_qStack;
        public bool control_depositAll;
        public bool control_lootAll;

        //alt functions
        public bool control_rSort;      // shift+sort=reverse sort

        public static bool[] lockedSlots = new bool[40]; //not the hotbar

        public override void Save(BinBuffer bb)
        {
            if (!IHBase.lockingEnabled) return;
            for (int i=0; i<lockedSlots.Length; i++)
            {
                bb.Write(lockedSlots[i]);
            }
        }

        public override void Load(BinBuffer bb)
        {
            if (bb.IsEmpty) return;
            lockedSlots = new bool[40];

            for (int i=0; i<lockedSlots.Length; i++)
            {
                lockedSlots[i]=bb.ReadBool();
            }
        }

        public override void PreUpdate()
        {
            //activate only if:
                // not typing
                // inventory is open
                // not shopping
                // not talking to an npc
            if (!API.KeyboardInputFocused() && Main.playerInventory && Main.npcShop==0 && Main.localPlayer.talkNPC==-1)
            {
                control_sort = control_clean = control_qStack = control_depositAll = control_lootAll = false;
                control_rSort = false;

                if (KState.Special.Shift.Down()) //alt functions
                {
                    control_rSort       = IHBase.key_sort.Pressed();
                }
                else
                {
                    control_sort       = IHBase.key_sort.Pressed();
                    control_clean      = IHBase.key_cleanStacks.Pressed();
                    control_qStack     = IHBase.key_quickStack.Pressed();
                    control_depositAll = IHBase.key_depositAll.Pressed();
                    control_lootAll    = IHBase.key_lootAll.Pressed();
                }

                if (control_sort || control_rSort)
                {
                    if ( player.chestItems == null ) // no valid chest open, sort player inventory
                    {
                        InventoryManager.SortPlayerInv(player, control_rSort);
                        return;
                    }
                    // else call sort on the Item[] array returned by chestItems
                    InventoryManager.SortChest(player.chestItems, control_rSort);
                    return;
                }

                if (control_clean)
                {
                    if ( player.chestItems == null )
                    {
                        InventoryManager.ConsolidateStacks(player.inventory, 0, 50);
                        return;
                    }
                    InventoryManager.ConsolidateStacks(player.chestItems);
                    return;
                }

                if (control_qStack)
                {
                    if ( player.chestItems == null ) return;
                    IHUtils.DoQuickStack(player);
                    return;
                }
                if (control_lootAll)
                {
                    if ( player.chestItems == null ) return;
                    IHUtils.DoLootAll(player);
                    return;
                }
                if (control_depositAll)
                {
                    if ( player.chestItems == null ) return;
                    IHUtils.DoDepositAll(player);
                    return;
                }

            }
        }

        public static bool SlotLocked(int slotIndex)
        {
            return lockedSlots[slotIndex-10];
        }

        public static void ToggleLock(int slotIndex)
        {
            lockedSlots[slotIndex-10]=!lockedSlots[slotIndex-10];
        }
    }
}
