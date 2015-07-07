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
                    btns = new TextReplacerButtons(IHBase.Instance);
                    break;
                default:
                    throw new ArgumentException("Invalid ButtonLayer type \"" + type + "\"; valid types are \"Inventory\", \"Chest\", and \"TextReplacer\".");
            }
            btns.UpdateFrame();
            return btns;
        }

        // ***************************************************************
        // Collecting common code

        public static IHButton LootAllButton(string label, Vector2 position, bool textual = false)
        {
            var bState = new ButtonState(label)
            {
                onClick = IHUtils.DoLootAll
            };

            // set up textures if this is not a text-button
            if (! textual)
            {
                bState.texture       = IHBase.ButtonGrid;
                bState.defaultTexels = IHUtils.GetSourceRect("Loot All");
                bState.altTexels     = IHUtils.GetSourceRect("Loot All", true);
            }

            return new IHButton(bState, position);
        }

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
            if (! textual)
            {
                bState.texture       = IHBase.ButtonGrid;
                bState.defaultTexels = IHUtils.GetSourceRect("Deposit All");
                bState.altTexels     = IHUtils.GetSourceRect("Deposit All", true);
            }
            else
            {
                bState.tint = Main.mouseTextColor.toColor();
            }

            if (lockable)
            {
                bState.onRightClick = () =>
                {
                    Main.PlaySound(22); //lock sound
                    IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.DA);
                };

                // if the "locked" state ever has different textures than the
                // unlocked, we'll need to do the !textual block again.
                // But, for now, we can just duplicate most of the initial state
                var bState2 = bState.Duplicate();
                bState2.label = label + " (Locked)";

                var offset = lockOffset ?? default(Vector2);
                Color color = lockColor ?? Color.Firebrick;
                // use buttonstate's PostDraw hook to draw the lock indicator
                bState2.PostDraw = (sb, bBase) => DrawLockIndicator(sb, bBase, parent, offset, color);

                Func<bool> isActive = () => IHPlayer.ActionLocked(Main.localPlayer, IHAction.DA);

                // return toggling button
                return new IHToggle(bState, bState2, isActive, position, true);
            }

            //return plain single-state button
            return new IHButton(bState, position);
        }

        // -------------------------------------------------------------------
        /// Generate Quick Stack button
        /// This overload is used for lockable, non-textual buttons
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

            // set up textures if this is not a text-button
            if (! textual)
            {
                bState.texture       = IHBase.ButtonGrid;
                bState.defaultTexels = IHUtils.GetSourceRect("Quick Stack");
                bState.altTexels     = IHUtils.GetSourceRect("Quick Stack", true);
            }

            if (lockable)
            {
                bState.onRightClick = () =>
                {
                    Main.PlaySound(22); //lock sound
                    IHPlayer.ToggleActionLock(Main.localPlayer, IHAction.QS);
                };

                // if the "locked" state ever has different textures than the
                // unlocked, we'll need to do the !textual block again.
                // But, for now, we can just duplicate most of the initial state
                var bState2 = bState.Duplicate();
                bState2.label = label + " (Locked)";

                var offset = lockOffset ?? default(Vector2);
                Color color = lockColor ?? Color.Firebrick;
                // use buttonstate's PostDraw hook to draw the lock indicator
                bState2.PostDraw = (sb, bBase) => DrawLockIndicator(sb, bBase, parent, offset, color);

                Func<bool> isActive = () => IHPlayer.ActionLocked(Main.localPlayer, IHAction.QS);

                // return toggling button
                return new IHToggle(bState, bState2, isActive, position, true);
            }

            //return plain single-state button
            return new IHButton(bState, position);
        }

        // -------------------------------------------------------------------


        // -------------------------------------------------------------------
        // for the lockable-actions
        private static void DrawLockIndicator(SpriteBatch sb, ButtonBase bb, ButtonLayer parent, Vector2 offset, Color tint)
        {
            sb.Draw(IHBase.LockedIcon, bb.Position + offset, tint * parent.LayerOpacity * bb.Alpha);
        }
    }
}
