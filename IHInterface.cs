using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

using TAPI;
using TAPI.UIKit;
using Terraria;

namespace InvisibleHand
{
    public class IHInterface : ModInterface
    {
        public override void ModifyInterfaceLayerList(List<InterfaceLayer> list)
        {
            if (Main.playerInventory)
            {
                if (Main.localPlayer.chest == -1)
                {
                    InterfaceLayer.Add(list, IHBase.Instance.InventoryButtons, InterfaceLayer.LayerInventory, true);
                }
                else
                {
                    // --haven't made a replacement for the separate chestbuttons yet
                    InterfaceLayer.Add(list, IHBase.Instance.ReplacerButtons, InterfaceLayer.LayerInventory, true);
                }
            }
        }

        public override bool PreDrawInventory(SpriteBatch sb)
        {
            IHBase.KEP.Update();
            return true;
        }

        /// Used to draw Button Tooltips so they don't appear under other elements
        public override void PostDrawInventory(SpriteBatch sb)
        {
            // there's probably only ever going to be one, but this is an easy
            // way to ensure it's reset every frame
            while (IHBase.Instance.ButtonTooltips.Count > 0)
            {
                API.main.MouseText(IHBase.Instance.ButtonTooltips.Pop());
                Main.mouseText = true;
            }
        }

        public override void PostDrawItemSlotBackground(SpriteBatch sb, ItemSlot slot)
        {
            if (IHBase.ModOptions["LockingEnabled"] && slot.type == "Inventory" && IHPlayer.SlotLocked(slot.index))
            {
                sb.Draw(IHBase.LockedIcon,      // the texture to draw
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

            if (IHBase.ModOptions["LockingEnabled"] && slot.modBase == null && Main.playerInventory && release )
            {
                if (slot.type == "Inventory" && slot.index >= 10) //not in the hotbar
                {
                    IHPlayer.ToggleLock(slot.index); //toggle lock state
                    Sound.Lock.Play();
                }
            }
            return false;
        }

        // TODO: Here are my thoughts on making sure shift-click syncs correctly with
        // the server: obviously, blindly placing the NetMessage update calls
        // wherever it seemed like Vanilla indicated they should go didn't work.
        // Now that I have a better understanding of what all that rigamarole is
        // supposed to do, I think that the most efficient way to handle this
        // would be to have any function that modifies a slot or slots in a
        // container record or return a list of the indices of those slots.
        // Numerous methods in IHUtils already return an int to indicate the
        // effect of their run, but only under certain conditions does that int
        // indicate the affected index. Other return statuses (statii?) could
        // indicate a modified slot or even multiple slots, but since I didn't
        // think I had any immediate need to know which slots those were, that
        // information was not recorded.

        // Now, it wouldn't be too hard to whip up a small data-transfer-object
        // that can be passed between method calls and hold both the return
        // status as it is now and a record of any slots which were modified
        // along the way; this would prevent us from having to take a
        // sledgehammer approach like we did with the calls that modify
        // chest-item-slot en-masse. While the sledge may be the best or
        // most-effective tool when many slots are likely to be modified at
        // once, this shift-click isn't likely to affect more than a few during
        // any one run, and looping over the entire container atleast twice for
        // each click is probably something we should avoid if we can.

        // Of course this is all obvious and I'm just being verbose for
        // absolute-clarity's sake. My point--and the catch--is that even
        // though the DTO would be simple to create and use, the code in IHUtils
        // is much more of a tangled mass of spaghetti-code than I realized, and
        // tracing down every spot where we'd need to make changes (though
        // likely small) to the code to integrate the object will be a delicate
        // job very prone to breaking things in unexpected ways. At least with
        // my luch it would be.  Anyway, that's why, for now, I'm going to take
        // the sledgehammer to this and hope it doesn't affect performance too
        // badly.

        /// <summary>
        /// Shift + Left Click on item slot to move it between inventory and chest
        /// </summary>
        /// <param name="slot"> </param>
        /// <param name="release"> </param>
        /// <returns>True if shift not held while left-clicking, or if no applicable
        /// recipient is present to move item into. </returns>
        public override bool PreItemSlotLeftClick(ItemSlot slot, ref bool release)
        {
            if (!(bool)IHBase.Instance.options["enableShiftMove"].Value) return true;

            if (slot.modBase == null && release && KState.Special.Shift.Down())
            {
                if (Main.localPlayer.chestItems != null && !slot.MyItem.IsBlank()) //chests and banks
                {
                    // Moving inventory item -> chest
                    if (slot.type == "Inventory" || slot.type == "Coin" || slot.type == "Ammo")
                    {
                        IHPlayer.DoChestUpdateAction(
                        () => {
                            if (IHUtils.ShiftToChest(ref slot))
                                Recipe.FindRecipes(); // !ref:Main:#22640.36#
                        });
                    }
                    // Moving chest item -> inventory
                    else if (slot.type == "Chest")
                    {
                        if (IHUtils.ShiftToPlayer(ref slot, Main.localPlayer.chest>-1))
                        {
                            Recipe.FindRecipes();

                            // We can take the easy route here since there's only
                            // one chest slot that could have been affected.
                            if (Main.netMode == 1 && Main.localPlayer.chest > -1) //non-bank
                                IHUtils.SendNetMessage(slot.index);
                        }
                    }
                    return false;
                }
                if (Main.craftGuide) //the Guide's crafting info slot
                {
                    if (Main.guideItem.IsBlank() && !slot.MyItem.IsBlank() && (slot.type == "Inventory" || slot.type == "Coin" || slot.type == "Ammo") )
                    {
                        if (slot.MyItem.material && !slot.MyItem.notMaterial)
                        {
                            Sound.ItemMoved.Play();
                            Main.guideItem = slot.MyItem.Clone();
                            slot.MyItem    = new Item();
                            Recipe.FindRecipes();
                        }
                    }
                    else if (!Main.guideItem.IsBlank() && slot.type == "CraftGuide")
                    {
                        if (IHUtils.ShiftToPlayer(ref slot, false))
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
                            Sound.ItemMoved.Play();
                            Main.reforgeItem = slot.MyItem.Clone();
                            slot.MyItem = new Item();
                            Recipe.FindRecipes();
                        }
                    }
                    else if (!Main.reforgeItem.IsBlank() && slot.type == "Reforge")
                    {
                        if (IHUtils.ShiftToPlayer(ref slot, false))
                            Main.reforgeItem = new Item();
                        Recipe.FindRecipes();
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
