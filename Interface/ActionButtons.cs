using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    // FIXME: Port this file over to the new ButtonFactory functions for button creation.
    public class InventoryButtons : ButtonLayer
    {
        // private readonly float[] posX = { 496, 532 }; //X-pos of the two buttons

        //put these above the coins/ammo slot to be less intrusive.
        // TODO: maybe put this in constructor so it's not constantly held in memory;
        //       this is only supposed to run once per session, after all
        private const float posY = 28; //maintain height
        private static readonly Dictionary<TIH,float> PosX = new Dictionary<TIH, float> {
            {TIH.SortInv,  496},
            {TIH.RSortInv, 496},
            {TIH.CleanInv, 532}
        };

        /// make the bg slightly translucent even at max button opacity
        private readonly Color bgColor = Constants.InvSlotColor*0.8f;

        public InventoryButtons(IHBase mbase) : base("InventoryButtons")
        {
            var actions = new TIH[] { TIH.SortInv, TIH.RSortInv, TIH.CleanInv };



                // create a button for each action & add it to the main ButtonRepo.
                // also create a button base and add it to the Buttons
                // collection for each action other than RSort,
                // which is instead registered as a toggle for Sort
                // (this actually eliminates the need for IHDynamicButton)
                foreach (var a in actions)
                {
                    // uses default label for the action
                    var button = ButtonFactory.GetSimpleButton(a, new Vector2(PosX[a], posY));
                    mbase.ButtonRepo.Add(button.Label, button);

                    if (a != TIH.RSortInv)
                        Buttons.Add(a, new ButtonBase(this, button));
                    else
                        Buttons[TIH.SortInv].RegisterKeyToggle(KState.Special.Shift, button);
                }
        }

        /*************************************************************************/

        /// Draw each button in this layer (bg first, then button)
        protected override void DrawButtons(SpriteBatch sb)
        {
            foreach (KeyValuePair<TIH, ButtonBase> kvp in Buttons)
            {
                sb.DrawButtonBG(kvp.Value, IHBase.ButtonBG, bgColor);
                kvp.Value.Draw(sb);
            }
        }
    }

    public class ChestButtons : ButtonLayer
    {
        private static readonly float[] posX = {
            453 - (3*Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE),   //leftmost
            453 - (2*Main.inventoryBackTexture.Width * Constants.CHEST_INVENTORY_SCALE),   //middle
            453 - (Main.inventoryBackTexture.Width   * Constants.CHEST_INVENTORY_SCALE)    //right beside trash
        };

        private static readonly Dictionary<TIH,float> PosX = new Dictionary<TIH,float>{
            {TIH.SortChest,  posX[0]},   //leftmost
            {TIH.RSortChest, posX[0]},   //leftmost
            {TIH.SmartLoot,  posX[1]},   //middle
            {TIH.QuickStack, posX[1]},   //middle
            {TIH.SmartDep,   posX[2]},    //right beside trash
            {TIH.DepAll,     posX[2]}    //right beside trash
        };

        private static readonly Dictionary<TIH, TIH> togglesWith = new Dictionary<TIH, TIH>{
            {TIH.RSortChest, TIH.SortChest},
            {TIH.QuickStack, TIH.SmartLoot},
            {TIH.DepAll, TIH.SmartDep}
        };

        private readonly float posY = API.main.invBottom + (224*Constants.CHEST_INVENTORY_SCALE) + 4;

        //position offset for the "locked" icon on QS/DA
        private static readonly Vector2 lockOffset = new Vector2((float)(int)((float)Constants.ButtonW/2),
                                                         -(float)(int)((float)Constants.ButtonH/2));

        private static readonly Color bgColor = Constants.ChestSlotColor*0.8f;

        public ChestButtons(IHBase mbase, bool replace) : base("ChestButtons")
        {
            var simpleActions = new TIH[] { TIH.SortChest, TIH.RSortChest, TIH.SmartDep, TIH.SmartLoot };
            var lockingActions = new TIH[] { TIH.QuickStack, TIH.DepAll };

            // FIXME: This code needs to be in ReplacerButtons, and it needs to behind a
            // "text or not-text ReplacerButtons" check; this way the type of Replacers
            // (if any) can be decided at mod-load time and we can just keep the one call
            // to ReplacerButtons in ModifyInterfaceLayerList() rather than having to
            // check every frame "text or not-text" and drawing the appropriate set.
            // This will also let a user have replacers and non-replacers at the same
            // time, if so desired.
            if (replace)
            {
                float posX = 506;

                //40  22
                //66  54
                //92  86
                //118
                // resize these to the button size
                // float posYLA = API.main.invBottom + 40;
                float posYLA = API.main.invBottom + 22;
                // float posYDA = posYLA + 26;
                float posYDA = posYLA + Constants.ButtonH; //32
                // float posYQS = posYLA + 52;
                float posYQS = posYDA + Constants.ButtonH;
                //edit chest
                float posYEC = posYQS + Constants.ButtonH;
                // cancel edit
                float posYCE = posYEC + Constants.ButtonH;

                // FIXME: the bottom button overlaps "Edit Chest"
                // TODO: since we're replacing "edit chest" too,
                // we can add sort and stuff in addition to loot all.
                var PosY = new Dictionary<TIH, float>{
                    {TIH.SortChest,  posYLA},
                    {TIH.RSortChest, posYLA},
                    {TIH.SmartLoot,  posYQS},
                    {TIH.QuickStack, posYQS},
                    {TIH.SmartDep,   posYDA},
                    {TIH.DepAll,     posYDA},
                    {TIH.Rename,  posYEC}
                };

                foreach (var a in simpleActions)
                {
                    // uses default label for the action
                    var button = ButtonFactory.GetSimpleButton(a, new Vector2(posX, PosY[a]));
                    mbase.ButtonRepo.Add(button.Label, button);

                    if (a != TIH.RSortChest)
                        Buttons.Add(a, new ButtonBase(this, button));
                    else
                        Buttons[TIH.SortChest].RegisterKeyToggle(KState.Special.Shift, button);
                }

                foreach (var a in lockingActions)
                {
                    var button = ButtonFactory.GetLockableButton(a, new Vector2(posX, PosY[a]), this, lockOffset);
                    mbase.ButtonRepo.Add(button.Label, button);
                    // set QS & DA to have their state initialized on world load
                    mbase.ButtonUpdates.Push(button.Label);

                    Buttons[togglesWith[a]].RegisterKeyToggle(KState.Special.Shift, button);
                }

            }
            else
            {
                foreach (var a in simpleActions)
                {
                    // uses default label for the action
                    var button = ButtonFactory.GetSimpleButton(a, new Vector2(PosX[a], posY));
                    mbase.ButtonRepo.Add(button.Label, button);

                    if (a != TIH.RSortChest)
                        Buttons.Add(a, new ButtonBase(this, button));
                    else
                        Buttons[TIH.SortChest].RegisterKeyToggle(KState.Special.Shift, button);
                }

                foreach (var a in lockingActions)
                {
                    var button = ButtonFactory.GetLockableButton(a, new Vector2(PosX[a], posY), this, lockOffset);
                    mbase.ButtonRepo.Add(button.Label, button);
                    // set QS & DA to have their state initialized on world load
                    mbase.ButtonUpdates.Push(button.Label);

                    Buttons[togglesWith[a]].RegisterKeyToggle(KState.Special.Shift, button);
                }
            }
        }

        /*************************************************************************
         * Draw each button in this layer (bg first, then button)
         */
        protected override void DrawButtons(SpriteBatch sb)
        {
            foreach (KeyValuePair<TIH, ButtonBase> kvp in Buttons)
            {
                sb.DrawButtonBG(kvp.Value, IHBase.ButtonBG, bgColor);
                kvp.Value.Draw(sb);
            }
        }
    }
}
