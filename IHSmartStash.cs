using System;
using System.Linq;
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

            var pInventory = Main.localPlayer.inventory; //Item[]
            var chestItems = Main.localPlayer.chestItems; //Item[]
            var sendNetMsg = Main.localPlayer.chest >-1; //bool

            // define a query that creates category groups for the items in the chests,
            // then pulls out the category keys into a distinct list (List<ItemCat>)
            var catList =
                    (from item in chestItems
                        where !item.IsBlank()
                        group item by item.GetCategory() into catGroup
                        from cat in catGroup
                        select catGroup.Key).Distinct() //no duplicates
                        .ToList(); //store the query results

            if (IHBase.ModOptions["LockingEnabled"]) //slot locking on
            {
                for (int i=49; i>=10; i--)  // reverse through player inv
                {
                    if ( !pInventory[i].IsBlank() && !IHPlayer.SlotLocked(Main.localPlayer, i) &&
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
            Recipe.FindRecipes();
        }

        /****************************************************
        *   This is a bit of a "reverse-quick-stack" in that only items that add to
        *   stacks currently in the player's inventory will be pulled from the chest.
        *
        *   The code actually works out to be a bit of a combination of the
        *   QuickStack and LootAll methods.
        *   Also based a fair bit on Player.GetItem()
        *   !ref:Player:#4497.00#
        */
        public static void SmartLoot()
        {
            if (Main.localPlayer.chest == -1) return;

            var pInventory = Main.localPlayer.inventory; //Item[]
            var chestItems = Main.localPlayer.chestItems; //Item[]
            var sendNetMsg = Main.localPlayer.chest >-1; //bool

            int index;
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
