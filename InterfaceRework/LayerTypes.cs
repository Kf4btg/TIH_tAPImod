using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    /// TextReplacer and IconReplacer will inherit from this class;
    /// This is intended to allow other code to simply request the type
    /// "ReplacerButtons" without worrying about which variety is returned.
    public class ChestButtonReplacerLayer : ButtonContainerLayer
    {
        protected readonly bool textButtons;

        // Constructor

        public ChestButtonReplacerLayer(bool text) : base("ChestButtonReplacerLayer", false)
        {
            // Order of operations:
            //  - First, create layer (that's what this is doing)
            //  - Second, create bases and assign to layer
            //	 - Finally, create buttons and assign to bases
            // This allows assigning references to parent objects down the stack

            textButtons = text;
        }

        protected override void AddBasesToLayer()
        {
            if (textButtons)
            {
            }
            else
            {
                // position of first button (right of chests, below coin slots)
                var pos0 = new Vector2(506, API.main.invBottom + 22);

                // // // // // // //
                // Create the Sockets
                // // // // // // //

                // a transform to calculate the position of the socket from the
                // order in which it is created (each offset by button height)
                Func<int,Vector2> getPosFromIndex
                    = (i) => new Vector2( pos0.X, pos0.Y + (i * Constants.ButtonH) );
                int slotOrder = 0;

                // Dictionary uses IButtonSlot type as one of the buttons is still text
                var bases = new Dictionary<TIH, IButtonSlot>();
                foreach (var action in new[]
                {
                    // order of creation; determines positioning per the transform above
                    TIH.SortChest,
                    TIH.LootAll,
                    TIH.DepAll,    // +smartdep
                    TIH.QuickStack // + smartloot
                })
                {
                    bases.Add(action, new IconButtonBase(this, getPosFromIndex(slotOrder++), IHBase.ButtonBG));
                }

                // Now add the base for the Cancel Edit Button, a text button which
                // only appears under certain conditions.
                getPosFromIndex = (i) => new Vector2( pos0.X,
                                                      // Add another half-button-height to prevent overlap
                                                      pos0.Y + (i * Constants.ButtonH) + (Constants.ButtonH / 2) );
                bases.Add(TIH.CancelEdit, new TextButtonBase(this, getPosFromIndex(slotOrder)));
            }
        }

        protected override void AddButtonsToBases()
        {
            if (textButtons)
            {
            }
            else
            {
                // // // // // //
                // some datas
                // // // // // //

                // offset of lock indicator
                var lockOffset = new Vector2((float)(int)((float)Constants.ButtonW / 2),
                                            -(float)(int)((float)Constants.ButtonH / 2));

                // background tints
                Color bgColor = Constants.ChestSlotColor * 0.85f;
                Color saveNameBgColor = Constants.EquipSlotColor * 0.85f;

                // // // // // // //
                // Makin buttons
                // // // // // // //
                var buttonStack = new Stack<CoreButton>();


                // buttonStack.Push
                // (
                //     new TexturedButton
                // )

                Func<TIH, string> getLabel = a => Constants.DefaultButtonLabels[a];
                Func<TIH, Color>  getBGcol = a => a == TIH.SaveName ? Constants.ChestSlotColor * 0.85f : Constants.EquipSlotColor * 0.85f;
                Func<TIH, string> getTtip  = a => getLabel(a) + IHUtils.GetKeyTip(a);

                foreach (var tih in new[] { TIH.SortChest, TIH.RSortChest, TIH.LootAll, TIH.DepAll, TIH.QuickStack, TIH.Rename, TIH.SaveName })
                {
                    buttonStack.Push(new TexturedButton( action: tih,
                                                         label:    getLabel(tih),
                                                         tooltip:  getTtip(tih),
                                                         bg_color: getBGcol(tih)
                                                         ));
                }
                buttonStack.Push(new TextButton(TIH.CancelEdit, getLabel(TIH.CancelEdit)));

                foreach (var tih in new[] { TIH.SortChest, TIH.RSortChest, TIH.LootAll, TIH.DepAll, TIH.QuickStack, TIH.Rename, TIH.SaveName })
                {
                    buttonStack.Push( new TexturedButton (
                                                action:   tih,
                                                label:    Constants.DefaultButtonLabels[tih],
                                                bg_color: tih == TIH.SaveName ? Constants.ChestSlotColor * 0.85f : Constants.EquipSlotColor * 0.85f
                                                ).With((button) => { button.Tooltip = button.Label + IHUtils.GetKeyTip(button.Action); })
                    );
                }
                // if (button.Action == TIH.QuickStack || button.Action == TIH.DepAll)
                //     button.AddService(new LockingService<TexturedButton>( button, lockOffset ));
            }
        }
    }
}
