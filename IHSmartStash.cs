using System.Collections.Generic;
using System;
using System.Linq;
// using System.Linq.Dynamic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public static class IHSmartStash
    {
        /****************************************************
        *   This will compare the categories of items in the player's
            inventory to those of items in the open container and
            deposit any items of matching categories.
        */
        public static void SmartDeposit()
        {
            if (Main.localPlayer.chest == -1) return;

            Item[] pInventory = Main.localPlayer.inventory;
            Item[] chestItems;
            bool sendNetMsg;
            Action coinFunc;

            if (Main.localPlayer.chest >-1)
            {
                chestItems = Main.chest[Main.localPlayer.chest].item;
                sendNetMsg = true;
                coinFunc=Main.ChestCoins;
            }
            else
            {
                chestItems = Main.localPlayer.chest==-3 ? Main.localPlayer.bank2.item : Main.localPlayer.bank.item;
                sendNetMsg = false;
                coinFunc=Main.BankCoins;
            }

            // create a query that creates category groups for the items in the chests,
            // then pull out the category keys into a distinct list
            List<ItemCat> catList =
                    (from item in chestItems
                        where !item.IsBlank()
                        group item by item.GetCategory() into catGroup
                        from cat in catGroup
                        select catGroup.Key).Distinct()
                        .ToList();

            // basically just the deposit all code with an extra check for category
            bool moveSuccess = false;
            if (IHBase.oLockingEnabled) //slot locking on
            {
                for (int i=49; i>=10; i--)  // reverse through player inv
                {
                    if ( !pInventory[i].IsBlank() && !IHPlayer.SlotLocked(i) &&
                    catList.Contains(pInventory[i].GetCategory()) )
                    {
                        // returned index
                        int retIdx = IHUtils.MoveItem(ref pInventory[i], chestItems);
                        if (retIdx >= 0 ) // item/entirety of stack successfully moved
                        {
                            pInventory[i] = new Item();
                            if (!moveSuccess) moveSuccess=true; //toggle success flag

                            if (sendNetMsg) IHUtils.SendNetMessage(retIdx); //only for non-bank chest
                        }
                    }
                }//end loop
            }
            else //no locking
            {
                for (int i=49; i>=10; i--)
                {
                    // if chest contains a matching category
                    if ( !pInventory[i].IsBlank() && catList.Contains(pInventory[i].GetCategory()) )
                    {
                        // returned index
                        int retIdx = IHUtils.MoveItem(ref pInventory[i], chestItems);
                        if (retIdx >= 0 ) // item/entirety of stack successfully moved
                        {
                            pInventory[i] = new Item();
                            if (!moveSuccess) moveSuccess=true; //toggle success flag

                            if (sendNetMsg) IHUtils.SendNetMessage(retIdx); //only for non-bank chest
                        }
                    }
                }//end loop
            }

            coinFunc();
            if (moveSuccess) IHUtils.RingBell();

        }
    }
}
