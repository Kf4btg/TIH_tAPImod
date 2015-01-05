using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using TAPI;
using TAPI.UIKit;
using Terraria;

namespace InvisibleHand
{
    public class IHInterface : ModInterface
    {
        public override void PostDrawItemSlotBackground(SpriteBatch sb, ItemSlot slot)
        {
            if (IHBase.oLockingEnabled && slot.type == "Inventory" && IHPlayer.SlotLocked(slot.index))
            {
                sb.Draw(IHBase.lockedIcon,      // the texture to draw
                            slot.pos,           // (Vector2) location in screen coords to draw sprite
                            null,               // Rectangle to specifies source texels from texture; null draws whole texture
                            Color.Firebrick,    // color to tint sprite; color.white=full color, no tint
                            0f,                 // angle in radians to rotate sprite around its center
                            default(Vector2),   // (Vector2) sprite origin, default=(0,0) i.e. upper left corner
                            slot.scale,         // (Vector2) scale factor
                            SpriteEffects.None, // effects to apply
                            0f                  // layer depth; 0=front layer, 1=backlayer; SpriteSortMode can sort sprites
                            );
            }
        }

        // Shift + Right Click on inventory slot toggles the lock state
        public override bool PreItemSlotRightClick(ItemSlot slot, ref bool release)
        {
            if (!KState.Special.Shift.Down()) return true;

            if (IHBase.oLockingEnabled && slot.modBase == null && Main.playerInventory && release )
            {
                if (slot.type == "Inventory" && slot.index >= 10) //not in the hotbar
                {
                    IHPlayer.ToggleLock(slot.index); //toggle lock state
                    IHUtils.RingBell();
                }
            }
            return false;
        }

        #region shiftmove

        /************************************************************************
        *   An implementation of the "Shift-click to move item between containers"
        *   concept. Possibly temporary, though it includes the feature of working
        *   with the craft-guide slot, which I really like.
        *
        *   Known Issues:
        */
        // Shift + Left Click on item slot to move it between inventory and chest
        public override bool PreItemSlotLeftClick(ItemSlot slot, ref bool release)
        {
            if (!(bool)IHBase.self.options["enableShiftMove"].Value) return true;

            if (slot.modBase == null && release && KState.Special.Shift.Down())
            {
                if (Main.localPlayer.chestItems != null && !slot.MyItem.IsBlank()) //chests and banks
                {
                    // Moving inventory item -> chest
                    if (slot.type == "Inventory" || slot.type == "Coin" || slot.type == "Ammo")
                    {
                        if (ShiftToChest(ref slot))
                        Recipe.FindRecipes(); // !ref:Main:#22640.36#
                    }
                    // Moving chest item -> inventory
                    else if (slot.type == "Chest")
                    {
                        if (ShiftToPlayer(ref slot, Main.localPlayer.chest>-1))
                        Recipe.FindRecipes();
                    }
                    return false;
                }
                if (Main.craftGuide) //the Guide's crafting info slot
                {
                    if (Main.guideItem.IsBlank() && !slot.MyItem.IsBlank() && (slot.type == "Inventory" || slot.type == "Coin" || slot.type == "Ammo") )
                    {
                        if (slot.MyItem.material && !slot.MyItem.notMaterial)
                        {
                            IHUtils.RingBell();
                            Main.guideItem = slot.MyItem.Clone();
                            slot.MyItem    = new Item();
                            Recipe.FindRecipes();
                        }
                    }
                    else if (!Main.guideItem.IsBlank() && slot.type == "CraftGuide")
                    {
                        if (ShiftToPlayer(ref slot, false))
                        Main.guideItem = new Item();
                        Recipe.FindRecipes();
                    }
                    return false;
                }
                if (Main.reforge) //Item reforging
                {
                    if (Main.reforgeItem.IsBlank() && slot.type == "Inventory" && !slot.MyItem.IsBlank())
                    {
                        if (slot.MyItem.maxStack == 1 && Prefix.CanHavePrefix(slot.MyItem))
                        {
                            IHUtils.RingBell();
                            Main.reforgeItem = slot.MyItem.Clone();
                            slot.MyItem = new Item();
                            Recipe.FindRecipes();
                        }
                    }
                    else if (!Main.reforgeItem.IsBlank() && slot.type == "Reforge")
                    {
                        if (ShiftToPlayer(ref slot, false))
                        Main.reforgeItem = new Item();
                        Recipe.FindRecipes();
                    }
                    return false;
                }
            }
            return true;
        }

        /**************************************************************
        *   returns true if item moved/itemstack emptied
        */

        // move item from player inventory slot to chest
        public static bool ShiftToChest(ref ItemSlot slot)
        {
            bool sendMessage = Main.localPlayer.chest > -1;

            Item pItem = slot.MyItem;

            int retIdx = -1;
            if (pItem.stack == pItem.maxStack) //move non-stackable items or full stacks to empty slot.
            {
                retIdx =  IHUtils.MoveToFirstEmpty( pItem, Main.localPlayer.chestItems, 0,
                new Func<int,bool>( i => i<Chest.maxItems ),
                new Func<int,int> ( i => i+1 ) );
            }

            // if we didn't find an empty slot...
            if (retIdx < 0)
            {
                if (pItem.maxStack == 1) return false; //we can't stack it, so we already know there's no place for it.

                retIdx = IHUtils.MoveItemP2C(ref pItem, Main.localPlayer.chestItems, sendMessage);

                if (retIdx < 0)
                {
                    if (retIdx == -1)  // partial success (stack amt changed), but we don't want to reset the item.
                    {
                        IHUtils.RingBell();
                        Recipe.FindRecipes();
                    }
                    return false;
                }
            }
            //else, success!
            IHUtils.RingBell();
            slot.MyItem = new Item();
            if (sendMessage) IHUtils.SendNetMessage(retIdx);
            return true;
        }

        // MoveChestSlotItem - moves item from chest/guide slot to player inventory
        public static bool ShiftToPlayer(ref ItemSlot slot, bool sendMessage)
        {
            //TODO: check for quest fish (item.uniqueStack && player.HasItem(item.type))
            Item cItem = slot.MyItem;

            if (cItem.IsBlank()) return false;

            if (cItem.Matches(ItemCat.COIN)) {
                // don't bother with "shifting", just move it as usual
                slot.MyItem = Main.localPlayer.GetItem(Main.myPlayer, slot.MyItem);
                return (slot.MyItem.IsBlank());
            }

            // ShiftToPlayer returns true if original item ends up empty
            if (cItem.Matches(ItemCat.AMMO)) {
                //ammo goes top-to-bottom
                if (cItem.maxStack > 1 && cItem.stack==cItem.maxStack && ShiftToPlayer(ref slot, 54, 57, sendMessage, false)) return true;
            }

            // if it's a stackable item and the stack is *full*, just shift it.
            else if (cItem.maxStack > 1 && cItem.stack==cItem.maxStack){
                if (ShiftToPlayer(ref slot,  0,  9, sendMessage, false) //try hotbar first, ascending order (vanilla parity)
                ||  ShiftToPlayer(ref slot, 10, 49, sendMessage,  true)) return true; //the other slots, descending
            }

            //if all of the above failed, then we have no empty slots.
            // Let's save some work and get traditional:
            slot.MyItem = Main.localPlayer.GetItem(Main.myPlayer, slot.MyItem);
            return (slot.MyItem.IsBlank());
        }

        // attempts to move an item to an empty slot
        public static bool ShiftToPlayer(ref ItemSlot slot, int ixStart, int ixStop, bool sendMessage, bool desc)
        {
            int iStart; Func<int,bool> iCheck; Func<int,int> iNext;

            if (desc) { iStart =  ixStop; iCheck = i => i >= ixStart; iNext = i => i-1; }
            else      { iStart = ixStart; iCheck = i => i <=  ixStop; iNext = i => i+1; }

            int retIdx = IHUtils.MoveToFirstEmpty( slot.MyItem, Main.localPlayer.inventory, iStart, iCheck, iNext );
            if (retIdx >= 0)
            {
                IHUtils.RingBell();
                slot.MyItem = new Item();
                if (sendMessage) IHUtils.SendNetMessage(retIdx);
                return true;
            }
            return false;
        }
        #endregion

        public override bool? ItemSlotAllowsItem(ItemSlot slot, Item item)
        {
            if (IHBase.oLockingEnabled && slot.type == "Inventory" && slot.index >=10 && IHPlayer.SlotLocked(slot.index))
            {
                return false;
                //FIXME: this just refuses to let anything _in_ to the slot.
                //Need to find a way to restrict taking things _out_ of the slot.
            }
            return null;
        }

    }
}
