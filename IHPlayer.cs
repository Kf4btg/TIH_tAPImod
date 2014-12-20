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

        public override void PreUpdate()
        {

            control_sort = control_clean = control_qStack = control_depositAll = control_lootAll = false;

            //activate only if:
                // not typing
                // inventory is open
                // not shopping
                // not talking to an npc
            if (!API.KeyboardInputFocused() && Main.playerInventory && Main.npcShop==0 && Main.localPlayer.talkNPC==-1)
            {

                control_sort       = IHBase.key_sort.Pressed();
                control_clean      = IHBase.key_cleanStacks.Pressed();
                control_qStack     = IHBase.key_quickStack.Pressed();
                control_depositAll = IHBase.key_depositAll.Pressed();
                control_lootAll    = IHBase.key_lootAll.Pressed();

                if (control_sort)
                {
                    if ( player.chestItems == null ) // no valid chest open, sort player inventory
                    {
                        InventoryManager.SortPlayerInv(player);
                        return;
                    }
                    // else call sort on the Item[] array returned by chestItems
                    InventoryManager.Sort(player.chestItems);
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
    }
}
