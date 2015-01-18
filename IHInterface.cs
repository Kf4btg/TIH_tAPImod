using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

using TAPI;
using TAPI.UIKit;
using Terraria;

namespace InvisibleHand
{
    public class IHInterface : ModInterface
    {
        // public LockOptions lockButtons = new LockOptions();

        public static IHInterface self;
        public IHInterface() : base() {self=this;}

        public override void ModifyInterfaceLayerList(List<InterfaceLayer> list)
        {
            if (Main.playerInventory)
            {
                if (Main.localPlayer.chest!=-1)
                    InterfaceLayer.Add(list, IHBase.self.lockOptions, InterfaceLayer.LayerInventory, true);
                else
                    InterfaceLayer.Add(list, IHBase.self.invButtons, InterfaceLayer.LayerInventory, true);
            }
        }

        public override bool PreDrawInventory(SpriteBatch sb)
        {
            IHBase.KEP[KState.Special.Shift].UpdateSubscribers();
            return true;
        }

        public override void PostDrawItemSlotBackground(SpriteBatch sb, ItemSlot slot)
        {
            if (IHBase.ModOptions["LockingEnabled"] && slot.type == "Inventory" && IHPlayer.SlotLocked(Main.localPlayer, slot.index))
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

            if (IHBase.ModOptions["LockingEnabled"] && slot.modBase == null && Main.playerInventory && release )
            {
                if (slot.type == "Inventory" && slot.index >= 10) //not in the hotbar
                {
                    IHPlayer.ToggleLock(Main.localPlayer, slot.index); //toggle lock state
                    Main.PlaySound(22, -1, -1, 1); // I think this is the actual "lock" sound
                }
            }
            return false;
        }

        /************************************************************************
        *   An implementation of the "Shift-click to move item between containers"
        *   concept. Possibly temporary, though it includes the feature of working
        *   with the craft-guide and reforge slots, which I really like.
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
                        if (IHUtils.ShiftToChest(ref slot))
                            Recipe.FindRecipes(); // !ref:Main:#22640.36#
                    }
                    // Moving chest item -> inventory
                    else if (slot.type == "Chest")
                    {
                        if (IHUtils.ShiftToPlayer(ref slot, Main.localPlayer.chest>-1))
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
                            IHUtils.RingBell();
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

        // public override bool? ItemSlotAllowsItem(ItemSlot slot, Item item)
        // {
        //     if (IHBase.oLockingEnabled && slot.type == "Inventory" && slot.index >=10 && IHPlayer.SlotLocked(slot.index))
        //     {
        //
        //     }
        //     return null;
        // }

    }
}
