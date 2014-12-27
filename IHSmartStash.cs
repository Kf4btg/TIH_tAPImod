using System.Collections.Generic;
using System;
using System.Linq;
// using System.Linq.Dynamic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    //<class>
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

        /****************************************************
        *   This is a bit of a "reverse-quick-stack" in that only items that add to
        *   stacks currently in the player's inventory will be pulled from the chest.
        *
        *   The code actually works out to be a bit of a combination of the
        *   QuickStack and LootAll methods.
        *
        *   @param takeAll : original implementation of this would take ALL stacks
        *       of a matching item from the chest if even one <max stack was present
        *       in the player's inventory. Leaving this functionality in place as a
        *       possible future option.
        */
        public static void SmartLoot(bool takeAll = false)
        {
            if (Main.localPlayer.chest == -1) return;

            Item[] pInventory = Main.localPlayer.inventory;
            Item[] chestItems;
            bool sendNetMsg;

            if (Main.localPlayer.chest >-1)
            {
                chestItems = Main.chest[Main.localPlayer.chest].item;
                sendNetMsg = true;
            }
            else
            {
                chestItems = Main.localPlayer.chest==-3 ? Main.localPlayer.bank2.item : Main.localPlayer.bank.item;
                sendNetMsg = false;
            }

            #region takeAll
            if (takeAll){
                //for each item in inventory (including coins, ammo, hotbar)...
                for (int i=0; i<58; i++)
                {
                    //...if item is not blank && not a full stack...
                    if (!pInventory[i].IsBlank() && pInventory[i].stack < pInventory[i].maxStack)
                    {   //...check every item in chest...
                        for (int j=0; j<Chest.maxItems; j++)
                        {   //...for a matching item...
                            if (chestItems[j].IsTheSameAs(pInventory[i]))
                            {   //...and move it to the Player's inventory
                                chestItems[j] = Main.localPlayer.GetItem(Main.localPlayer.whoAmI, chestItems[j]);
                                if (sendNetMsg) IHUtils.SendNetMessage(j); //only for non-bank chest
                            }
                        }
                    }
                }
            return;}
            #endregion


            //do a first run through to fill the ammo slots
            for (int ak=0; ak<Chest.maxItems; ak++)
            {
                if (!chestItems[ak].IsBlank() && chestItems[ak].ammo > 0 && !chestItems[ak].notAmmo) //if ammo
                {
                    chestItems[ak]=Main.localPlayer.FillAmmo(Main.localPlayer.whoAmI, chestItems[ak]);
                }
            }

            int index=0;
            //for each item in inventory (including coins & hotbar)...
            for (int i=-4; i<50; i++)   //this little i=-4 trick from the vanilla code
            {
                index = i<0 ? 54 + i : i; //do coins first

                //...if item is not blank && not a full stack...
                if (!pInventory[index].IsBlank() && pInventory[index].stack < pInventory[index].maxStack)
                {   //...check every item in chest...
                    for (int j=0; j<Chest.maxItems; j++)
                    {   //...for a matching item stack...
                        if (chestItems[j].IsTheSameAs(pInventory[index]))
                        {
                            IHUtils.RingBell();
                            //...and merge it to the Player's inventory
                            if (IHUtils.StackMerge(ref chestItems[j], ref pInventory[index]))
                            {
                                chestItems[j] = new Item(); //reset this item if all stack transferred
                                if (sendNetMsg) IHUtils.SendNetMessage(j); //only for non-bank chest
                            }
                            Main.localPlayer.DoCoins(index); // call coin-handling code

                            // now check to see if original stack is full, stop looking for more stacks if true.
                            // the do coins call could also have reduced the stack to 0
                            if (pInventory[index].stack==pInventory[index].maxStack || pInventory[index].stack==0) break;

                        }
                    }// </for inner>
                }
            }// </for outer>

            //when all is said and done, check for newly available recipes.
            Recipe.FindRecipes();

        } // </smartloot>

    } // </class>
} // </namespace>
