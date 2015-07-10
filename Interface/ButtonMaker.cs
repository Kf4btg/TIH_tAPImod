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
                    btns = new InventoryButtons(IHBase.Instance, false);
                    break;
                case "Chest":
                    btns = new ChestButtons(IHBase.Instance, false);
                    break;
                case "TextReplacer":
                    btns = new TextReplacerButtons(IHBase.Instance, true);
                    break;
                //TODO: add Tile-Button-replacers
                default:
                    throw new ArgumentException("Invalid ButtonLayer type \"" + type + "\"; valid types are \"Inventory\", \"Chest\", and \"TextReplacer\".");
            }
            btns.UpdateFrame();
            return btns;
        }

        // ***************************************************************
        // Collecting common code

        /// <returns>A text-based button that, in addition to performing its regular action
        /// on left click, on a right-click will toggle between respecting or ignoring
        /// locked inventory slots.</returns>
        public static IHButton GetLockableTextButton(TIH action, string label, Vector2 position, ButtonLayer parent, Vector2? lockOffset = null, Color? lockColor = null)
        {
            return GetLockableButton(action, label, position, parent, lockOffset, lockColor, true);
        }
        ///<returns>A button that, in addition to performing its regular action on left click,
        /// on a right-click will toggle between respecting or ignoring locked inventory slots.</returns>
        public static IHButton GetLockableButton(TIH action, string label, Vector2 position, ButtonLayer parent, Vector2? lockOffset = null, Color? lockColor = null, bool textual = false)
        {
            return GenerateIHButton(action, label, position, true, textual, parent, lockOffset, lockColor);
        }
        public static IHButton GetLockableButton(TIH action, Vector2 position, ButtonLayer parent, Vector2? lockOffset = null, Color? lockColor = null, bool textual = false)
        {
            return GetLockableButton(action, Constants.DefaultButtonLabels[action], position, parent, lockOffset, lockColor, true);
        }


        /// This method makes it easy to create the simpler button types, mainly:
        ///      - LootAll
        ///      - SmartLoot/Restock
        ///      - Smart Deposit
        ///      - Clean Stacks
        /// Just give it the corresponding action type, the label you want
        /// to show up on screen, the screen position, and true for the last
        /// param if you want this to be a text button.
        public static IHButton GetSimpleButton(TIH action, string label, Vector2 position, bool textual = false)
        {
            return GenerateIHButton(action, label, position, false, textual, null, null, null);
        }

        /// Use default label for this button's action
        public static IHButton GetSimpleButton(TIH action, Vector2 position, bool textual = false)
        {
            return GetSimpleButton(action, Constants.DefaultButtonLabels[action], position, textual);
        }

        /// Generic button-generator which is fed info from the other "Get...Button" methods.
        public static IHButton GenerateIHButton(TIH action, string label, Vector2 position, bool lockable,  bool textual,  ButtonLayer parent, Vector2? lockOffset, Color? lockColor)
        {
            var bState = new ButtonState(action, label)
            {
                onClick = Constants.DefaultClickActions[action]
            };

            if (!textual)
                GetButtonTexture(action, ref bState);

            if (lockable)
                return CreateLockableButton(bState, action, parent, position, lockOffset, lockColor);
            else
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

        /// Given one pre-made state, this will automatically construct the
        /// "locked" version of that state and return a button that toggles
        /// between the two on right-click.
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
            // bState2.label = bState1.label + " (Locked)";  //just using the icon now.

            var offset  = lockOffset ?? default(Vector2);
            Color color = lockColor  ?? Color.Firebrick;
            // bState2.tint = Color.LightCoral; // a bit brighter text?
            // use buttonstate's PostDraw hook to draw the lock indicator
            bState2.PostDraw = (sb, bBase) => DrawLockIndicator(sb, bBase, parent, offset, color);

            Func<bool> isActive = () => IHPlayer.ActionLocked(Main.localPlayer, toLock);

            // return toggling button
            return new IHToggle(bState1, bState2, isActive, position, true);
        }

        /// This method can be used as the PostDraw function-property of
        /// a button state to make the lock indicator icon appear with
        /// the button at the specified offset from its top-left corner.
        private static void DrawLockIndicator(SpriteBatch sb, ButtonBase bb, ButtonLayer parent, Vector2 offset, Color tint)
        {
            sb.Draw(IHBase.LockedIcon, bb.Position + offset, tint * parent.LayerOpacity * bb.Alpha);
        }
    }
}
