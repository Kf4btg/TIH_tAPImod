using System;
using Microsoft.Xna.Framework.Input;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class IHPlayer : ModPlayer
    {
        //because C# enums annoy me...
        public const int VACTION_QS=0, VACTION_DA=1, VACTION_LA=2; // vanilla action IDs

        private bool[] lockedSlots;

        private bool[] lockedActions;

        public override void Initialize()
        {
            // MUST use "new", as tAPI derps with clearing (quote: Miraimai)
            lockedSlots = new bool[40]; //not the hotbar

            lockedActions = new bool[VACTION_LA+1]; //initialize all elements to false
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
        }

        //load back locked-slot state
        // FIXME: sometimes this seems to load a different player's info
        public override void Load(BinBuffer bb)
        {
            if (bb.IsEmpty) return;

            for (int i=0; i<lockedSlots.Length; i++)
            {
                lockedSlots[i]=bb.ReadBool();
            }
            if (bb.IsEmpty) return;
            for (int i=0; i<lockedActions.Length; i++)
            {
                lockedActions[i]=bb.ReadBool();
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
                }

                else if (IHBase.ActionKeys["CleanStacks"].Pressed()) //Consolidate Stacks
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

                    if (IHBase.ActionKeys["QuickStack"].Pressed()) {
                        if (KState.Special.Shift.Down()) IHSmartStash.SmartLoot();
                        else IHUtils.DoQuickStack(player);
                    }
                    else if (IHBase.ActionKeys["DepositAll"].Pressed()) {
                        if (KState.Special.Shift.Down()) IHSmartStash.SmartDeposit();
                        else IHUtils.DoDepositAll(player);
                    }
                    else if (IHBase.ActionKeys["LootAll"].Pressed())
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

        public static bool ActionLocked(Player player, int actionID)
        {
            IHPlayer mp = player.GetSubClass<IHPlayer>();
            return mp.lockedActions[actionID];
        }

        public static void ToggleActionLock(Player p, int actionID)
        {
            IHPlayer mp = p.GetSubClass<IHPlayer>();
            mp.lockedActions[actionID] = !mp.lockedActions[actionID];
        }
    }
}
