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

        public bool control_qStack, control_depositAll, control_lootAll;

        //alt functions
        public bool control_rSort,  control_sDeposit,   control_rStack;

        public static bool[] lockedSlots = new bool[40]; //not the hotbar

        public static bool daLocked, laLocked, qsLocked;


        // save locked-slot state with player
        public override void Save(BinBuffer bb)
        {
            if (!IHBase.oLockingEnabled) return;
            for (int i=0; i<lockedSlots.Length; i++)
            {
                bb.Write(lockedSlots[i]);
            }
            bb.Write(daLocked);
            bb.Write(laLocked);
            bb.Write(qsLocked);
        }

        //load back locked-slot state
        public override void Load(BinBuffer bb)
        {
            if (bb.IsEmpty) return;
            lockedSlots = new bool[40];

            for (int i=0; i<lockedSlots.Length; i++)
            {
                lockedSlots[i]=bb.ReadBool();
            }
            if (bb.IsEmpty) return;
            daLocked=bb.ReadBool();
            laLocked=bb.ReadBool();
            qsLocked=bb.ReadBool();
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
                    control_rSort      = IHBase.key_sort.Pressed();         // reverse sort
                    control_sDeposit   = IHBase.key_depositAll.Pressed();   // smart-deposit
                    control_rStack     = IHBase.key_quickStack.Pressed();   // "Reverse" quickstack
                }
                else
                {
                    control_sort       = IHBase.key_sort.Pressed();
                    control_clean      = IHBase.key_cleanStacks.Pressed();
                    control_qStack     = IHBase.key_quickStack.Pressed();
                    control_depositAll = IHBase.key_depositAll.Pressed();
                    control_lootAll    = IHBase.key_lootAll.Pressed();
                }

                if (control_sort || control_rSort) // Sort inventory/chest
                {
                    if ( player.chestItems == null ) // no valid chest open, sort player inventory
                    {
                        IHOrganizer.SortPlayerInv(player, control_rSort ^ IHBase.oRevSortPlayer);
                        return;
                    }
                    // else call sort on the Item[] array returned by chestItems
                    IHOrganizer.SortChest(player.chestItems, control_rSort ^ IHBase.oRevSortChest);
                    return;
                }

                if (control_clean) //Consolidate Stacks
                {
                    if ( player.chestItems == null )
                    {
                        IHOrganizer.ConsolidateStacks(player.inventory, 0, 50);
                        return;
                    }
                    IHOrganizer.ConsolidateStacks(player.chestItems);
                    return;
                }

                if (control_qStack) //QuickStack
                {
                    if ( player.chestItems == null ) return;
                    IHUtils.DoQuickStack(player);
                    return;
                }
                if (control_lootAll) //LootAll
                {
                    if ( player.chestItems == null ) return;
                    IHUtils.DoLootAll(player);
                    return;
                }
                if (control_depositAll) //DepositAll
                {
                    if ( player.chestItems == null ) return;
                    IHUtils.DoDepositAll(player);
                    return;
                }
                if (control_sDeposit) //SmartDeposit
                {
                    if ( player.chestItems == null ) return;
                    IHSmartStash.SmartDeposit();
                    return;
                }
                if (control_rStack) //SmartLoot
                {
                    if ( player.chestItems == null ) return;
                    IHSmartStash.SmartLoot();
                    return;
                }

            }
        }

        public static bool SlotLocked(int slotIndex)
        {
            return slotIndex>9 && slotIndex<50 && lockedSlots[slotIndex-10];
        }

        public static void ToggleLock(int slotIndex)
        {
            if (slotIndex<10 || slotIndex>49) return;
            lockedSlots[slotIndex-10]=!lockedSlots[slotIndex-10];
        }
    }
}
