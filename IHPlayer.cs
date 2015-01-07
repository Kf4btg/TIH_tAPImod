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

        private bool[] lockedSlots; // = new bool[40]; //not the hotbar

        public bool daLocked, laLocked, qsLocked;

        public override void Initialize()
        {
            // MUST use "new", as tAPI derps with clearing (quote: Miraimai)
            lockedSlots = new bool[40];
            daLocked = laLocked = qsLocked = false;
        }

        // save locked-slot state with player
        public override void Save(BinBuffer bb)
        {
            // if (!IHBase.oLockingEnabled) return;
            // let's just do it anyway
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
            // lockedSlots = new bool[40];

            for (int i=0; i<lockedSlots.Length; i++)
            {
                lockedSlots[i]=bb.ReadBool();
            }
            if (bb.IsEmpty) return;
            daLocked=bb.ReadBool();
            laLocked=bb.ReadBool();
            qsLocked=bb.ReadBool();

            // update buttons to set initial state FIXME: NOT WORKING!!!!!
            while (IHBase.toUpdate.Peek()!=null)
            {
                IHBase.toUpdate.Pop().Update();
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
                    // NOTE: this used to check player.chestItems==null, but I once got a
                    // "object reference not set to instance of object" or whatever kind of error
                    // with that check elsewhere in the code. This should be safer and have the exact same result.
                    if ( player.chest == -1 ) // no valid chest open, sort player inventory
                    {
                        // control_rSort XOR oRevSortPlayer:
                        //   this will reverse the sort IFF exactly one of these two bools is true
                        IHOrganizer.SortPlayerInv(player, control_rSort ^ IHBase.oRevSortPlayer);
                        return;
                    }
                    // else call sort on the Item[] array returned by chestItems
                    IHOrganizer.SortChest(player.chestItems, control_rSort ^ IHBase.oRevSortChest);
                    // return;
                }

                else if (control_clean) //Consolidate Stacks
                {
                    if ( player.chest == -1 )
                    {
                        IHOrganizer.ConsolidateStacks(player.inventory, 0, 50);
                        return;
                    }
                    IHOrganizer.ConsolidateStacks(player.chestItems);
                    // return;
                }
                else if (control_qStack) //QuickStack
                {
                    if ( player.chest == -1 ) return;
                    IHUtils.DoQuickStack(player);
                    // return;
                }
                else if (control_lootAll) //LootAll
                {
                    if ( player.chest == -1 ) return;
                    IHUtils.DoLootAll(player);
                    // return;
                }
                else if (control_depositAll) //DepositAll
                {
                    if ( player.chest == -1 ) return;
                    IHUtils.DoDepositAll(player);
                    // return;
                }
                else if (control_sDeposit) //SmartDeposit
                {
                    if ( player.chest == -1 ) return;
                    IHSmartStash.SmartDeposit();
                    // return;
                }
                else if (control_rStack) //SmartLoot
                {
                    if ( player.chest == -1 ) return;
                    IHSmartStash.SmartLoot();
                    // return;
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
