using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{

    public class IHPlayer : ModPlayer
    {
        private static IHPlayer Instance;

        /// map of user-locked item slots in player inventory
        private bool[] lockedSlots;

        /// Holds status of flag indicating whether a particular
        /// action will respect (i.e. ignore) locked slots.
        /// Some actions are unaffected by this flag.
        private Dictionary<TIH,bool> LockedActions;

        public override void Initialize()
        {

            Instance = this;

            // MUST use "new", as tAPI derps with clearing (quoth: Miraimai)
            lockedSlots = new bool[40]; //not the hotbar

            // init "locked" status of all available actions;
            // not all actions are affected by this flag
            LockedActions = new Dictionary<TIH,bool>();
            foreach (TIH aID in Enum.GetValues(typeof(TIH)))
            {
                LockedActions.Add(aID, false);
            }
        }

        public override void Save(BinBuffer bb)
        {

            if (IHBase.ModOptions["UseReplacers"])
            {
                // reset original chest-button strings if we're quitting to main
                // menu, which should be indicated by checking:
                //     if (Main.gameMenu == true)
                // as this is set during the SaveAndQuit() method of the worldgen
                // immediately before player save. So:
                if (Main.gameMenu)
                {
                    Lang.inter[IHBase.iLA] = IHBase.OriginalButtonLabels[TIH.LootAll];
                    Lang.inter[IHBase.iDA] = IHBase.OriginalButtonLabels[TIH.DepAll];
                    Lang.inter[IHBase.iQS] = IHBase.OriginalButtonLabels[TIH.QuickStack];


                    if (IHBase.ModOptions["IconReplacers"])
                    {
                        Lang.inter[IHBase.iRC] = IHBase.OriginalButtonLabels[TIH.Rename];
                        Lang.inter[IHBase.iSC] = IHBase.OriginalButtonLabels[TIH.SaveName];
                        Lang.inter[IHBase.iCE] = IHBase.OriginalButtonLabels[TIH.CancelEdit];
                    }
                }
                // should take care of it and make sure the strings are set
                // correctly if the mod is unloaded/the replacer-button option
                // is disabled.



            }
            // if (!IHBase.oLockingEnabled) return; //maybe?

            // save locked-slot state with player
            foreach (var l in lockedSlots)
            {
                bb.Write(l);
            }
            bb.Write(LockedActions.Count);
            //KeyValuePair<TIH, bool>
            foreach (var kvp in LockedActions)
            {
                bb.Write((int)kvp.Key);
                bb.Write(kvp.Value);
            }
        }

        ///load back locked-slot state
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
                if (Enum.IsDefined(typeof(TIH), aID))
                    LockedActions[(TIH)aID] = state;
            }
        }

        /// During this phase we check if the player has pressed any hotkeys;
        /// if so, the corresponding action is called, with the chest-related
        /// actions wrapped in special net-update code to prevent syncing
        /// issues during multiplayer.
        public override void PreUpdate()
        {
            //activate only if:
                // not typing
                // inventory is open
                // not shopping
                // not talking to an npc
            if (!API.KeyboardInputFocused() && Main.playerInventory && Main.npcShop==0 && Main.localPlayer.talkNPC==-1)
            {
                // Sort inventory/chest
                if (IHBase.ActionKeys["sort"].Pressed())
                {
                    // NOTE: this used to check player.chestItems==null, but I once got a
                    // "object reference not set to instance of object" or whatever kind of error
                    // with that check elsewhere in the code. This should be safer and have the exact same result.
                    if ( player.chest == -1 ) // no valid chest open, sort player inventory
                    {
                        // shift-pressed XOR Reverse-sort-mod-option:
                        //   this will reverse the sort IFF exactly one of these two bools is true
                        IHOrganizer.SortPlayerInv(player,
                                KState.Special.Shift.Down() ^ IHBase.ModOptions["ReverseSortPlayer"]);
                        return;
                    }
                    // else call sort on the Item[] array returned by chestItems
                    DoChestUpdateAction( () => {
                        IHOrganizer.SortChest(player.chestItems,
                                KState.Special.Shift.Down() ^ IHBase.ModOptions["ReverseSortChest"]);
                    });
                }

                //Consolidate Stacks
                else if (IHBase.ActionKeys["cleanStacks"].Pressed())
                {
                    if ( player.chest == -1 )
                    {
                        IHOrganizer.ConsolidateStacks(player.inventory, 0, 50);
                        return;
                    }
                    DoChestUpdateAction( () => { IHOrganizer.ConsolidateStacks(player.chestItems); } );
                }
                else {
                    if ( player.chest == -1 ) return; //no action w/o open container

                    // smartloot or quickstack
                    if (IHBase.ActionKeys["quickStack"].Pressed()) {
                        if (KState.Special.Shift.Down())
                            DoChestUpdateAction( IHSmartStash.SmartLoot );
                        else
                            DoChestUpdateAction( () => { IHUtils.DoQuickStack(player); } );
                    }
                    // smart-deposit or deposit-all
                    else if (IHBase.ActionKeys["depositAll"].Pressed()) {
                        if (KState.Special.Shift.Down())
                            DoChestUpdateAction( IHSmartStash.SmartDeposit );
                        else
                            DoChestUpdateAction( () => { IHUtils.DoDepositAll(player); } );
                    }
                    // loot all
                    else if (IHBase.ActionKeys["lootAll"].Pressed())
                        DoChestUpdateAction( () => { IHUtils.DoLootAll(player); } );
                }

            }
        }

        /// <summary>
        /// Takes an Action and will perform it wrapped in some net update code if we are a client. Otherwise it just does whatever it is.
        /// </summary>
        /// <param name="action">An Action (a lambda with no output)</param>
        protected void DoChestUpdateAction(Action action)
        {

            // check net status and make sure a non-bank chest is open
            // (bank-chests, i.e. piggy-bank & safe, are handled solely client-side)
            if (Main.netMode == 1 && player.chest > -1)
            {
                Item[] oldItems = new Item[player.chestItems.Length];

                // make an exact copy of the chest's original contents
                for (int i = 0; i < oldItems.Length; i++)
                {
                    oldItems[i] = player.chestItems[i].Clone();
                }

                // perform the requested action
                action();

                // compare each item in the old copy of the original contents
                // to the chest's new contents and send net-update message
                // if they do not match.
                for (int i = 0; i < oldItems.Length; i++)
                {
                    var oldItem = oldItems[i];
                    var newItem = player.chestItems[i];

                    if (oldItem.IsNotTheSameAs(newItem) || oldItem.stack != newItem.stack)
                    {
                        IHUtils.SendNetMessage(i);
                    }
                }
            }
            else // And this is important...
            {
                action();
            }
        }

        public static void SortInventory(bool reverse = false)
        {
            if ( Main.localPlayer.chest == -1 )
                IHOrganizer.SortPlayerInv(Main.localPlayer,
                reverse ^ IHBase.ModOptions["ReverseSortPlayer"]);
        }

        public static void SortChest(bool reverse = false)
        {
            if ( Main.localPlayer.chest != -1 )
                Instance.DoChestUpdateAction( () =>
                    IHOrganizer.SortChest(Main.localPlayer.chestItems,
                    reverse ^ IHBase.ModOptions["ReverseSortChest"])
                );
        }

        public static void CleanInventoryStacks()
        {
            if ( Main.localPlayer.chest == -1 )
                IHOrganizer.ConsolidateStacks(Main.localPlayer.inventory);
        }

        public static void CleanChestStacks()
        {
            if ( Main.localPlayer.chest != -1 )
                Instance.DoChestUpdateAction( () =>
                    IHOrganizer.ConsolidateStacks(Main.localPlayer.chestItems)
                );
        }

        /// Only valid for the 40 Player inventory slots below the hotbar.
        /// <returns>True if indicated slot is locked</returns>
        public static bool SlotLocked(Player player, int slotIndex)
        {
            // pull IHPlayer subclass from the current Player-object's
            // list of subclasses
            IHPlayer mp = player.GetSubClass<IHPlayer>();
            // subtract 10 since our array only contains 40 items and
            // we're ignoring the first 10 actual slots (the hotbar).
            return slotIndex>9 && slotIndex<50 && mp.lockedSlots[slotIndex-10];
        }

        /// Locks/unlocks indicated slot depending on current status.
        public static void ToggleLock(Player player, int slotIndex)
        {
            if (slotIndex<10 || slotIndex>49) return;
            IHPlayer mp = player.GetSubClass<IHPlayer>();

            mp.lockedSlots[slotIndex-10]=!mp.lockedSlots[slotIndex-10];
        }

        /// <returns>True if indicated action is set to respect locked slots.</returns>
        public static bool ActionLocked(Player player, TIH actionID)
        {
            IHPlayer mp = player.GetSubClass<IHPlayer>();
                return mp.LockedActions[actionID];
        }

        /// Set indicated action to respect/not-respect locked slots,
        /// depending on current status.
        public static void ToggleActionLock(Player p, TIH actionID)
        {
            IHPlayer mp = p.GetSubClass<IHPlayer>();
            mp.LockedActions[actionID] = !mp.LockedActions[actionID];
        }
    }
}
