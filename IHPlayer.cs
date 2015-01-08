using System;
using Microsoft.Xna.Framework.Input;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class IHPlayer : ModPlayer
    {

        // private bool control_sort,  control_clean,
        //              control_qStack, control_depositAll, control_lootAll,
        //              control_rStack, control_sDeposit,   control_rSort ;     //alt functions

        private bool[] lockedSlots; // = new bool[40]; //not the hotbar

        // private bool daLocked, laLocked, qsLocked;
        // private Dictionary<VAction, bool> lockedActions;
        private bool[] lockedActions;

        public override void Initialize()
        {
            // MUST use "new", as tAPI derps with clearing (quote: Miraimai)
            lockedSlots = new bool[40];
            // daLocked = laLocked = qsLocked = false;
            lockedActions = new bool[(int)VAction.LA+1]; //this should initialize all elements to false, right?
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
            for (int i=0; i<lockedActions.Length; i++)
            {
                bb.Write(lockedActions[i]);
            }
            // bb.Write(daLocked);
            // bb.Write(laLocked);
            // bb.Write(qsLocked);
        }

        //load back locked-slot state
        // FIXME: sometimes this seems to load a different player's info
        public override void Load(BinBuffer bb)
        {
            if (bb.IsEmpty) return;
            // lockedSlots = new bool[40];

            for (int i=0; i<lockedSlots.Length; i++)
            {
                lockedSlots[i]=bb.ReadBool();
            }
            if (bb.IsEmpty) return;
            for (int i=0; i<lockedActions.Length; i++)
            {
                lockedActions[i]=bb.ReadBool();
            }
            // daLocked=bb.ReadBool();
            // laLocked=bb.ReadBool();
            // qsLocked=bb.ReadBool();

            // update buttons to set initial state FIXME: NOT WORKING!!!!!
            foreach (IHUpdateable u in IHBase.GetUpdateables())
            {
                u.Update();
            }
            // while (IHBase.toUpdate.Peek()!=null)
            // {
            //     IHBase.toUpdate.Pop().Update();
            // }
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
                // control_sort = control_clean = control_qStack = control_depositAll = control_lootAll = false;
                // control_rSort = false;
                // bool control_sort, control_clean , control_qStack , control_depositAll , control_lootAll ,
                // control_rSort, control_sDeposit, control_rStack = false;
                //
                // if (KState.Special.Shift.Down()) //alt functions
                // {
                //     // control_rSort      = IHBase.key_sort.Pressed();         // reverse sort
                //     // control_sDeposit   = IHBase.key_depositAll.Pressed();   // smart-deposit
                //     // control_rStack     = IHBase.key_quickStack.Pressed();   // "Reverse" quickstack
                //     control_rSort      = IHBase.ActionKeys["Sort"].Pressed();         // reverse sort
                //     control_sDeposit   = IHBase.ActionKeys["DepositAll"].Pressed();   // smart-deposit
                //     control_rStack     = IHBase.ActionKeys["QuickStack"].Pressed();   // "Reverse" quickstack
                //
                // }
                // else
                // {
                //     control_sort       = IHBase.ActionKeys["Sort"].Pressed();
                //     control_clean      = IHBase.ActionKeys["Clean"].Pressed();
                //     control_qStack     = IHBase.ActionKeys["QuickStack"].Pressed();
                //     control_depositAll = IHBase.ActionKeys["DepositAll"].Pressed();
                //     control_lootAll    = IHBase.ActionKeys["LootAll"].Pressed();
                // }

                if (IHBase.ActionKeys["Sort"].Pressed())// || control_rSort) // Sort inventory/chest
                {
                    // NOTE: this used to check player.chestItems==null, but I once got a
                    // "object reference not set to instance of object" or whatever kind of error
                    // with that check elsewhere in the code. This should be safer and have the exact same result.
                    if ( player.chest == -1 ) // no valid chest open, sort player inventory
                    {
                        // control_rSort XOR oRevSortPlayer:
                        //   this will reverse the sort IFF exactly one of these two bools is true
                        IHOrganizer.SortPlayerInv(player, KState.Special.Shift.Down() ^ IHBase.ModOptions["ReverseSortPlayer"]);
                        return;
                    }
                    // else call sort on the Item[] array returned by chestItems
                    IHOrganizer.SortChest(player.chestItems, KState.Special.Shift.Down() ^ IHBase.ModOptions["ReverseSortChest"]);
                    // return;
                }

                else if (IHBase.ActionKeys["Clean"].Pressed()) //Consolidate Stacks
                {
                    if ( player.chest == -1 )
                    {
                        IHOrganizer.ConsolidateStacks(player.inventory, 0, 50);
                        return;
                    }
                    IHOrganizer.ConsolidateStacks(player.chestItems);
                    // return;
                }
                else {
                    if ( player.chest == -1 ) return;

                    if (IHBase.ActionKeys["QuickStack"].Pressed()) //QuickStack
                        KState.Special.Shift.Down() ? IHSmartStash.SmartLoot() : IHUtils.DoQuickStack(player);

                    else if (IHBase.ActionKeys["DepositAll"].Pressed()) //DepositAll
                        KState.Special.Shift.Down() ? IHSmartStash.SmartDeposit() : IHUtils.DoDepositAll(player);

                    else if (IHBase.ActionKeys["LootAll"].Pressed()) //LootAll
                        IHUtils.DoLootAll(player);

                    // else if (control_sDeposit) //SmartDeposit
                    // {
                    //     IHSmartStash.SmartDeposit();
                    // }
                    // else if (control_rStack) //SmartLoot
                    // {
                    //     IHSmartStash.SmartLoot();
                    // }

                }




                // if (control_sort || control_rSort) // Sort inventory/chest
                // {
                //     // NOTE: this used to check player.chestItems==null, but I once got a
                //     // "object reference not set to instance of object" or whatever kind of error
                //     // with that check elsewhere in the code. This should be safer and have the exact same result.
                //     if ( player.chest == -1 ) // no valid chest open, sort player inventory
                //     {
                //         // control_rSort XOR oRevSortPlayer:
                //         //   this will reverse the sort IFF exactly one of these two bools is true
                //         IHOrganizer.SortPlayerInv(player, control_rSort ^ IHBase.oRevSortPlayer);
                //         return;
                //     }
                //     // else call sort on the Item[] array returned by chestItems
                //     IHOrganizer.SortChest(player.chestItems, control_rSort ^ IHBase.oRevSortChest);
                //     // return;
                // }
                //
                // else if (control_clean) //Consolidate Stacks
                // {
                //     if ( player.chest == -1 )
                //     {
                //         IHOrganizer.ConsolidateStacks(player.inventory, 0, 50);
                //         return;
                //     }
                //     IHOrganizer.ConsolidateStacks(player.chestItems);
                //     // return;
                // }
                // else if (control_qStack) //QuickStack
                // {
                //     if ( player.chest == -1 ) return;
                //     IHUtils.DoQuickStack(player);
                //     // return;
                // }
                // else if (control_lootAll) //LootAll
                // {
                //     if ( player.chest == -1 ) return;
                //     IHUtils.DoLootAll(player);
                //     // return;
                // }
                // else if (control_depositAll) //DepositAll
                // {
                //     if ( player.chest == -1 ) return;
                //     IHUtils.DoDepositAll(player);
                //     // return;
                // }
                // else if (control_sDeposit) //SmartDeposit
                // {
                //     if ( player.chest == -1 ) return;
                //     IHSmartStash.SmartDeposit();
                //     // return;
                // }
                // else if (control_rStack) //SmartLoot
                // {
                //     if ( player.chest == -1 ) return;
                //     IHSmartStash.SmartLoot();
                //     // return;
                // }

            }
        }

        public static bool SlotLocked(Player player, int slotIndex)
        {
            IHPlayer mp = player.GetSubClass<IHPlayer>();
            return slotIndex>9 && slotIndex<50 && mp.lockedSlots[slotIndex-10];
        }

        public static void ToggleLock(Player player, int slotIndex)
        {
            if (slotIndex<10 || slotIndex>49) return;
            IHPlayer mp = player.GetSubClass<IHPlayer>();

            mp.lockedSlots[slotIndex-10]=!lockedSlots[slotIndex-10];
        }

        public static bool ActionLocked(Player player, VAction a)
        {
            IHPlayer mp = player.GetSubClass<IHPlayer>();
            return mp.lockedActions[(int)a];
        }

        public static void ToggleActionLock(Player p, VAction a)
        {
            IHPlayer mp = p.GetSubClass<IHPlayer>();
            mp.lockedActions[(int)a] = !mp.lockedActions[(int)a];
        }

    }

    // identifiers for the vanilla actions
    public enum VAction
    {
        QS,     //Quick Stack
        DA,     //Deposit All
        LA      //LootAll
    }
}
