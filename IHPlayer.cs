using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using TAPI;
using Terraria;

namespace InvisibleHand
{

    public class IHPlayer : ModPlayer
    {
        private bool[] lockedSlots;

        private Dictionary<IHAction,bool> LockedActions;

        public override void Initialize()
        {
            // MUST use "new", as tAPI derps with clearing (quoth: Miraimai)
            lockedSlots = new bool[40]; //not the hotbar

            LockedActions = new Dictionary<IHAction,bool>();
            foreach (IHAction aID in Enum.GetValues(typeof(IHAction)))
            {
                LockedActions.Add(aID, false);
            }
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
            bb.Write(LockedActions.Count);
            foreach (KeyValuePair<IHAction, bool> kvp in LockedActions)
            {
                bb.Write((int)kvp.Key);
                bb.Write(kvp.Value);
            }
        }

        //load back locked-slot state
        public override void Load(BinBuffer bb)
        {
            if (bb.IsEmpty) return;

            for (int i=0; i<lockedSlots.Length; i++)
            {
                lockedSlots[i]=bb.ReadBool();
            }
            if (bb.IsEmpty) return;

            int count = bb.ReadInt();
            for (int i=0; i<count; i++)
            {
                int aID = bb.ReadInt();
                bool state = bb.ReadBool();
                if (Enum.IsDefined(typeof(IHAction), aID))
                    LockedActions[(IHAction)aID] = state;
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
                if (IHBase.ActionKeys["sort"].Pressed()) // Sort inventory/chest
                {
                    // NOTE: this used to check player.chestItems==null, but I once got a
                    // "object reference not set to instance of object" or whatever kind of error
                    // with that check elsewhere in the code. This should be safer and have the exact same result.
                    if ( player.chest == -1 ) // no valid chest open, sort player inventory
                    {
                        // shift-pressed XOR Reverse-sort-mod-option:
                        //   this will reverse the sort IFF exactly one of these two bools is true
                        IHOrganizer.SortPlayerInv(player, KState.Special.Shift.Down() ^ IHBase.ModOptions["ReverseSortPlayer"]);
                        return;
                    }
                    // else call sort on the Item[] array returned by chestItems
                    IHOrganizer.SortChest(player.chestItems, KState.Special.Shift.Down() ^ IHBase.ModOptions["ReverseSortChest"]);
                }

                else if (IHBase.ActionKeys["cleanStacks"].Pressed()) //Consolidate Stacks
                {
                    if ( player.chest == -1 )
                    {
                        IHOrganizer.ConsolidateStacks(player.inventory, 0, 50);
                        return;
                    }
                    IHOrganizer.ConsolidateStacks(player.chestItems);
                }
                else {
                    if ( player.chest == -1 ) return; //no action w/o open container

                    if (IHBase.ActionKeys["quickStack"].Pressed()) {
                        if (KState.Special.Shift.Down()) IHSmartStash.SmartLoot();
                        else IHUtils.DoQuickStack(player);
                    }
                    else if (IHBase.ActionKeys["depositAll"].Pressed()) {
                        if (KState.Special.Shift.Down()) IHSmartStash.SmartDeposit();
                        else IHUtils.DoDepositAll(player);
                    }
                    else if (IHBase.ActionKeys["lootAll"].Pressed())
                        IHUtils.DoLootAll(player);
                }
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

            mp.lockedSlots[slotIndex-10]=!mp.lockedSlots[slotIndex-10];
        }

        public static bool ActionLocked(Player player, IHAction actionID)
        {
            IHPlayer mp = player.GetSubClass<IHPlayer>();
                return mp.LockedActions[actionID];
        }

        public static void ToggleActionLock(Player p, IHAction actionID)
        {
            IHPlayer mp = p.GetSubClass<IHPlayer>();
            mp.LockedActions[actionID] = !mp.LockedActions[actionID];
        }
    }
}
