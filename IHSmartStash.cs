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
            Item[] chestItems = Main.localPlayer.chestItems;
            bool sendNetMsg   = Main.localPlayer.chest >-1;

            // create a query that creates category groups for the items in the chests,
            // then pull out the category keys into a distinct list
            List<ItemCat> catList =
                    (from item in chestItems
                        where !item.IsBlank()
                        group item by item.GetCategory() into catGroup
                        from cat in catGroup
                        select catGroup.Key).Distinct()
                        .ToList();

            if (IHBase.oLockingEnabled) //slot locking on
            {
                for (int i=49; i>=10; i--)  // reverse through player inv
                {
                    if ( !pInventory[i].IsBlank() && !IHPlayer.SlotLocked(i) &&
                        catList.Contains(pInventory[i].GetCategory()) )
                            IHUtils.MoveItemToChest(i, sendNetMsg);
                }//end loop
            }
            else //no locking
            {
                for (int i=49; i>=10; i--)
                {
                    // if chest contains a matching category
                    if ( !pInventory[i].IsBlank() && catList.Contains(pInventory[i].GetCategory()) )
                        IHUtils.MoveItemToChest(i, sendNetMsg);
                }//end loop
            }
        }

        /****************************************************
        *   This is a bit of a "reverse-quick-stack" in that only items that add to
        *   stacks currently in the player's inventory will be pulled from the chest.
        *
        *   The code actually works out to be a bit of a combination of the
        *   QuickStack and LootAll methods.
        *   Also based a fair bit on Player.GetItem()
        *   !ref:Player:#4497.00#
        *
        *   @param takeAll : original implementation of this would take ALL stacks
        *       of a matching item from the chest if even one <max stack was present
        *       in the player's inventory. Leaving this functionality in place as a
        *       possible future option.

        */
        // public static void SmartLoot(bool takeAll = false)
        public static void SmartLoot()
        {
            if (Main.localPlayer.chest == -1) return;

            Item[] pInventory = Main.localPlayer.inventory;
            Item[] chestItems = Main.localPlayer.chestItems;
            bool sendNetMsg   = Main.localPlayer.chest >-1;

        #region takeAll
            // if (takeAll){
            //     //for each item in inventory (including coins, ammo, hotbar)...
            //     for (int i=0; i<58; i++)
            //     {
            //         //...if item is not blank && not a full stack...
            //         if (!pInventory[i].IsBlank() && pInventory[i].stack < pInventory[i].maxStack)
            //         {   //...check every item in chest...
            //             for (int j=0; j<Chest.maxItems; j++)
            //             {   //...for a matching item...
            //                 if (chestItems[j].IsTheSameAs(pInventory[i]))
            //                 {   //...and move it to the Player's inventory
            //                     chestItems[j] = Main.localPlayer.GetItem(Main.localPlayer.whoAmI, chestItems[j]);
            //                     if (sendNetMsg) IHUtils.SendNetMessage(j); //only for non-bank chest
            //                 }
            //             }
            //         }
            //     }
            // return;}
        #endregion

            //do a first run through to fill the ammo slots
            // for (int ak=0; ak<Chest.maxItems; ak++)
            // {
            //     if (!chestItems[ak].IsBlank() && chestItems[ak].ammo > 0 && !chestItems[ak].notAmmo) //if ammo
            //     {
            //         for (int j=54; j<58; j++)
            //         {
            //             if (pInventory[j].IsTheSameAs(chestItems[ak]))
            //             chestItems[ak]=Main.localPlayer.FillAmmo(Main.localPlayer.whoAmI, chestItems[ak]);
            //
            //         }
            //
            //     }
            // } actually forget all that

            int index=0;
            //for each item in inventory (including coins & hotbar)...
            for (int i=-8; i<50; i++)   //this trick from the vanilla code
            {
                index = i<0 ? 58 + i : i; //do ammo and coins first

                //...if item is not blank && not a full stack...
                if (!pInventory[index].IsBlank() && pInventory[index].stack < pInventory[index].maxStack)
                {   //...check every item in chest...
                    int j=-1;
                    // quit if we max out this stack or reach the end of the chest;
                    // also note that the DoCoins() call may reduce this stack to 0, so check that too
                    while (pInventory[index].stack < pInventory[index].maxStack &&
                            ++j < Chest.maxItems && pInventory[index].stack > 0 )
                    {   //...for a matching item stack...
                        if (!chestItems[j].IsBlank() && chestItems[j].IsTheSameAs(pInventory[index]))
                        {
                            IHUtils.RingBell();
                            //...and merge it to the Player's inventory

                            // I *think* this ItemText.NewText command just makes the text pulse...
                            // I don't entirely grok how it works, but included for parity w/ vanilla
                            ItemText.NewText(chestItems[j], IHUtils.StackMergeD(ref chestItems[j], ref pInventory[index]));
                            Main.localPlayer.DoCoins(index);
                            if (chestItems[j].stack<=0)
                            {
                                chestItems[j] = new Item(); //reset this item if all stack transferred
                                if (sendNetMsg) IHUtils.SendNetMessage(j); //only for non-bank chest
                                break;
                            }
                            if (sendNetMsg) IHUtils.SendNetMessage(j);
                        }
                    }// </while>
                }
            }// </for outer>
            //when all is said and done, check for newly available recipes.
            Recipe.FindRecipes();
        } // </smartloot>
    } // </class>
} // </namespace>
