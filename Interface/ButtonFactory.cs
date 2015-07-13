using System;
using System.Collections.Generic;
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
                    btns = new ChestButtons(IHBase.Instance, false);
                    break;
                case "TextReplacer":
                    btns = new TextReplacerButtons(IHBase.Instance, true);
                    break;
                case "IconReplacer":
                    btns = new IconReplacerButtons(IHBase.Instance, true);
                    break;
                default:
                    throw new ArgumentException("Invalid ButtonLayer type \"" + type + "\"; valid types are \"Inventory\", \"Chest\", \"TextReplacer\", and \"IconReplacer\".");
            }
            btns.UpdateFrame();
            return btns;
        }

        // ***************************************************************
        // Collecting common code

        /// <returns>A text-based button that, in addition to performing its regular action
        /// on left click, on a right-click will toggle between respecting or ignoring
        /// locked inventory slots.</returns>
        public static IHButton GetLockableTextButton(TIH action, string label, Vector2 position, ButtonLayer parent,
            Vector2? lockOffset = null, Color? lockColor = null)
        {
            return GetLockableButton(action, label, position, parent, lockOffset, lockColor, true);
        }
        public static IHButton GetLockableTextButton(TIH action, Vector2 position, ButtonLayer parent,
            Vector2? lockOffset = null, Color? lockColor = null)
        {
            return GetLockableButton(action,
                Constants.DefaultButtonLabels[action], position, parent, lockOffset, lockColor, true);
        }

        ///<returns>A button that, in addition to performing its regular action on left click,
        /// on a right-click will toggle between respecting or ignoring locked inventory slots.</returns>
        public static IHButton GetLockableButton(TIH action, string label, Vector2 position, ButtonLayer parent,
            Vector2? lockOffset = null, Color? lockColor = null, bool textual = false)
        {
            return GenerateIHButton(action, label, position, true, textual, parent, lockOffset, lockColor);
        }
        public static IHButton GetLockableButton(TIH action, Vector2 position, ButtonLayer parent,
            Vector2? lockOffset = null, Color? lockColor = null, bool textual = false)
        {
            return GetLockableButton(action,
                Constants.DefaultButtonLabels[action], position, parent, lockOffset, lockColor, textual);
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
        public static IHButton GenerateIHButton(TIH action, string label, Vector2 position, bool lockable,  bool textual,
            ButtonLayer parent, Vector2? lockOffset, Color? lockColor)
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
        private static IHToggle CreateLockableButton(ButtonState bState1, TIH toLock, ButtonLayer parent,
            Vector2 position, Vector2? lockOffset, Color? lockColor)
        {
            bState1.onRightClick = () =>
            {
                // Main.PlaySound(22); //lock sound
                Sound.Lock.Play();
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

        // ------------------------------------------------------------
        // One of those fancy flowy-interfaces

        // /// entry point to CreateButton, though the class itself can be used
        // public static CreateButton MakeButton()
        // {
        //     return new CreateButton();
        // }
        //
        public static IHButton testBtn()
        {
            return MakeButton().forAction(TIH.SortInv).atPosition(12, 15)
                .withLabel("testing")
                .withBehaviors(
                    onRightClick: IHUtils.DoLootAll
                ).Return();
        }

        public static CreateButton MakeButton()
        {
            return new CreateButton();
        }

        public class CreateButton
        {
            public CreateButton() {}

            private IHButton button;
            private List<ButtonState> states;
            private ButtonState state;
            private int stateCount = 0;
            private Vector2 position;

            //lock stuff
            private bool lockable;
            private ButtonLayer container;
            private Vector2? lockOffset;
            private Color? lockColor;

            /// Finalize and return the created button
            public IHButton Return()
            {
                if (button == null)
                {
                    if (lockable)
                        button = CreateLockableButton(state, state.action, container, position, lockOffset, lockColor);
                    else
                        button = new IHButton(state, position);
                }
                return button;
            }

            public CreateButton forAction(TIH action)
            {
                // states.Add(new ButtonState(action));
                // stateCount++;
                state = new ButtonState(action);
                return this;
            }

            public CreateButton atPosition(float x, float y)
            {
                position = new Vector2(x, y);
                return this;
            }

            public CreateButton withLabel(string label)
            {
                state.label = label;
                state.tooltip = label + IHUtils.GetKeyTip(state.action);

                return this;
            }

            public CreateButton isLockable(ButtonLayer parent, Vector2? lockOffset = null, Color? lockColor = null)
            {
                lockable = true;
                this.container = parent;
                if (lockOffset!=null) this.lockOffset = lockOffset;
                if (lockColor!=null) this.lockColor = lockColor;

                return this;
            }

            public CreateButton withLockColor(Color lockColor)
            {
                this.lockColor = lockColor;
                return this;
            }

            public CreateButton withLockColor(int R, int G, int B, int A=255)
            {
                return this.withLockColor(new Color(R % 256, G % 256, B % 256, A % 256));
                // return this.WithLockColor(new Color((byte)R, (byte)G, (byte)B, (byte)A));
            }

            public CreateButton withLockOffset(Vector2 lockOffset)
            {
                this.lockOffset = lockOffset;
                return this;
            }

            public CreateButton withLockOffset(float offX, float offY)
            {
                return this.withLockOffset(new Vector2(offX, offY));
            }

            public CreateButton withBackgroundColor(Color bgColor)
            {
                state.bgColor = bgColor;
                return this;
            }

            public CreateButton withBackgroundColor(int R, int G, int B, int A=255)
            {
                return this.withLockColor(new Color(R % 256, G % 256, B % 256, A % 256));
                // return this.WithLockColor(new Color((byte)R, (byte)G, (byte)B, (byte)A));
            }

            // WhichCan ? WhichDoes? WithFunctions ?
            public CreateButton withBehaviors(
             Action onClick = null,
             Action onRightClick = null,
             Func<ButtonBase,bool> onMouseEnter = null,
             Func<ButtonBase,bool> onMouseLeave = null,
             Func<SpriteBatch, ButtonBase, bool> preDraw = null,
             Action<SpriteBatch, ButtonBase> postDraw = null)
            {
                if (onClick!=null)      state.onClick = onClick;
                if (onRightClick!=null) state.onClick = onClick;
                if (onMouseEnter!=null) state.onMouseEnter = onMouseEnter;
                if (onMouseLeave!=null) state.onMouseLeave = onMouseLeave;
                if (preDraw!=null)      state.PreDraw = preDraw;
                if (postDraw!=null)     state.PostDraw = postDraw;

                return this;
            }




        }
    }


}
