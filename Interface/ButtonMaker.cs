using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace InvisibleHand {

    /// The Button Factory (tm). Since 2015
    public static class ButtonFactory
    {
        /// Use this to get instances of the button-layer types rather than
        /// directly calling their constructor.  This ensures that the layer
        /// frame will be properly updated.
        public static ButtonLayer BuildButtons(String type)
        {
            ButtonLayer btns;
            switch(type)
            {
                case "Inventory":
                    btns = new InventoryButtons(IHBase.Instance);
                    break;
                case "Chest":
                    btns = new ChestButtons(IHBase.Instance);
                    break;
                case "TextReplacer":
                    btns = new TextReplacerButtons(IHBase.Instance, true);
                    break;
                default:
                    throw new ArgumentException("Invalid ButtonLayer type \"" + type + "\"; valid types are \"Inventory\", \"Chest\", and \"TextReplacer\".");
            }
            btns.UpdateFrame();
            return btns;
        }

        // ***************************************************************
        // Collecting common code

        // public static IHButton LootAllButton(string label, Vector2 position, bool textual = false)
        // {
        //     var bState = new ButtonState(label)
        //     {
        //         onClick = IHUtils.DoLootAll
        //     };
        //
        //     // get font color if this is a text button
        //     // set up textures if this is not a text-button
        //     // if (textual)
        //     //     bState.tint = Main.mouseTextColor.toColor();
        //     // else
        //     if (! textual)
        //         GetButtonTexture("Loot All", ref bState);
        //
        //     return new IHButton(bState, position);
        // }

        // -------------------------------------------------------------------
        /// Generate Deposit All button
        /// This overload is best for lockable, non-textual buttons
        public static IHButton DepositAllButton(string label, Vector2 position, ButtonLayer parent, Vector2? lockOffset = null, bool textual = false, Color? lockColor = null, bool lockable = true)
        {
            return DepositAllButton(label, position, lockable, parent, lockOffset, lockColor, textual);
        }
        /// Generate Deposit All button
        /// This overload most easily creates non-lockable or lockable textual buttons.
        public static IHButton DepositAllButton(string label, Vector2 position, bool lockable = false,  ButtonLayer parent = null, Vector2? lockOffset = null, Color? lockColor = null, bool textual = true)
        {
            // init button state w/ label and left-click Action
            var bState = new ButtonState(label)
            {
                onClick = IHUtils.DoDepositAll
            };

            // set up textures if this is not a text-button
            // if (textual)
            //     bState.tint = Main.mouseTextColor.toColor();
            // else
            if (! textual)
                GetButtonTexture(TIH.DepAll, ref bState);

            if (lockable)
                return CreateLockableButton(bState, TIH.DepAll, parent, position, lockOffset, lockColor);

            //return plain single-state button
            return new IHButton(bState, position);
        }

        // -------------------------------------------------------------------
        /// Generate Quick Stack button
        /// This overload is best for lockable, non-textual buttons
        public static IHButton QuickStackButton(string label, Vector2 position, ButtonLayer parent, Vector2? lockOffset = null, bool textual = false, Color? lockColor = null, bool lockable = true)
        {
            return QuickStackButton(label, position, lockable, parent, lockOffset, lockColor, textual);
        }
        /// Generate Quick Stack button
        /// This overload most easily creates non-lockable or lockable textual buttons.
        public static IHButton QuickStackButton(string label, Vector2 position, bool lockable = false,  ButtonLayer parent = null, Vector2? lockOffset = null, Color? lockColor = null, bool textual = true)
        {
            // init button state w/ label and left-click Action
            var bState = new ButtonState(label)
            {
                onClick = IHUtils.DoQuickStack
            };

            // get font color if this is a text button
            // set up textures if this is not a text-button
            // if (textual)
            //     bState.tint = Main.mouseTextColor.toColor();
            // else
            if (! textual)
                GetButtonTexture(TIH.QuickStack, ref bState);

            if (lockable)
                return CreateLockableButton(bState, TIH.QuickStack, parent, position, lockOffset, lockColor);

            //else return plain single-state button
            return new IHButton(bState, position);
        }

        // public static IHButton SmartLootButton(string label, Vector2 position, bool textual = false)
        // {
        //     var bState = new ButtonState(label)
        //     {
        //         onClick = IHSmartStash.SmartLoot
        //     };
        //
        //     //TODO: In light of the "Smart Loot" action added to
        //     // Terraria 1.3, maybe this should be renamed (back) to
        //     // Smart Loot?
        //     if (! textual)
        //         GetButtonTexture("Restock", ref bState);
        //
        //     return new IHButton(bState, position);
        // }
        //
        // public static IHButton SmartDepositButton(string label, Vector2 position, bool textual = false)
        // {
        //     var bState = new ButtonState(label)
        //     {
        //         onClick = IHSmartStash.SmartDeposit
        //     };
        //
        //     if (! textual)
        //         GetButtonTexture("Smart Deposit", ref bState);
        //
        //     return new IHButton(bState, position);
        // }
        //
        // public static IHButton CleanStacksButton(TIH action, string label, Vector2 position, bool textual = false)
        // {
        //     var bState = new ButtonState(label)
        //     {
        //         onClick = IHSmartStash.SmartDeposit
        //     };
        //
        //     if (! textual)
        //         GetButtonTexture(action, ref bState);
        //
        //     return new IHButton(bState, position);
        // }

        public static IHButton GetSimpleButton(TIH action, string label, Vector2 position, bool textual = false)
        {
            var bState = new ButtonState(label);
            switch(action)
            {
                case TIH.LootAll:
                    bState.onClick = IHUtils.DoLootAll;
                    break;
                case TIH.SmartDep:
                    bState.onClick = IHSmartStash.SmartDeposit;
                    break;
                case TIH.SmartLoot:
                    bState.onClick = IHSmartStash.SmartLoot;
                    break;
                case TIH.CleanChest:
                    bState.onClick = IHPlayer.CleanChestStacks;
                    break;
            }

            if (! textual)
                GetButtonTexture(action, ref bState);

            return new IHButton(bState, position);


        }

        // -------------------------------------------------------------------
        /// set base texture and default/alt texels for given ButtonState
        private static void GetButtonTexture(TIH action, ref ButtonState bState)
        {
            bState.texture       = IHBase.ButtonGrid;
            bState.defaultTexels = IHUtils.GetSourceRect(action);
            bState.altTexels     = IHUtils.GetSourceRect(action, true);
        }

        // -------------------------------------------------------------------
        // for the lockable-actions

        private static IHToggle CreateLockableButton(ButtonState bState1, TIH toLock, ButtonLayer parent, Vector2 position, Vector2? lockOffset, Color? lockColor)
        {
            bState1.onRightClick = () =>
            {
                Main.PlaySound(22); //lock sound
                IHPlayer.ToggleActionLock(Main.localPlayer, toLock);
            };

            // if the "locked" state ever has different textures than the
            // unlocked, we'll need to do the "if (textual)" block again.
            // But, for now, we can just duplicate most of the initial state
            var bState2 = bState1.Duplicate();
            // bState2.label = bState1.label + " (Locked)";

            var offset = lockOffset ?? default(Vector2);
            Color color = lockColor ?? Color.Firebrick;
            // bState2.tint = Color.LightCoral; // a bit brighter text?
            // use buttonstate's PostDraw hook to draw the lock indicator
            bState2.PostDraw = (sb, bBase) => DrawLockIndicator(sb, bBase, parent, offset, color);

            Func<bool> isActive = () => IHPlayer.ActionLocked(Main.localPlayer, toLock);

            // return toggling button
            return new IHToggle(bState1, bState2, isActive, position, true);
        }


        private static void DrawLockIndicator(SpriteBatch sb, ButtonBase bb, ButtonLayer parent, Vector2 offset, Color tint)
        {
            sb.Draw(IHBase.LockedIcon, bb.Position + offset, tint * parent.LayerOpacity * bb.Alpha);
        }
    }
}
