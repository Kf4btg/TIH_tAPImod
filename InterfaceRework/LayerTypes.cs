using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TAPI;
using Terraria;
using System;
using System.Collections.Generic;

namespace InvisibleHand
{
    /// TextReplacer and IconReplacer will inherit from this class;
    /// This is intended to allow other code to simply request the type
    /// "ReplacerButtons" without worrying about which variety is returned.
    public class ChestButtonReplacerLayer : ButtonContainerLayer
    {


        // Constructor

        public ChestButtonReplacerLayer(bool text): base("ChestButtonReplacerLayer", false)
        {
            // Order of operations:
            //  - First, create layer (that's what this is doing)
            //  - Second, create bases and assign to layer
            //	 - Finally, create buttons and assign to bases
            // This allow assigning references to parent objects down the stack

            if (text)
            {

            }
            else
            {
                // // // // // //
                // some datas
                // // // // // //

                // offset of lock indicator
                var lockOffset = new Vector2((float)(int)((float)Constants.ButtonW/2),
                                            -(float)(int)((float)Constants.ButtonH/2));

                // background tints
                Color bgColor = Constants.ChestSlotColor * 0.85f;
                Color saveNameBgColor = Constants.EquipSlotColor * 0.85f;

                // position of first button (right of chests, below coin slots)
                var pos0 = new Vector2(506, API.main.invBottom + 22);


                // // // // // // //
                // Create the Sockets
                // // // // // // //

                // a transform to calculate the position of the socket from the
                // order in which it is created (each offset by button height)
                Func<int,Vector2> getPosFromIndex =
                    (i) => new Vector2( pos0.X, pos0.Y + (i * Constants.ButtonH) );
                int slotOrder = 0;

                // Dictionary uses IButtonSlot type as one of the buttons is still text
                var bases = new Dictionary<TIH, IButtonSlot>();
                foreach (var action in new[]
                {
                    // order of creation; determines positioning per the transform above
                    TIH.SortChest,
                    TIH.LootAll,
                    TIH.DepAll,
                    TIH.QuickStack
                })
                {
                    bases.Add(action, new IconButtonBase(this, getPosFromIndex(slotOrder++), IHBase.ButtonBG));
                }

                // Now add the base for the Cancel Edit Button, a text button which
                // only appears under certain conditions.
                getPosFromIndex = (i) => new Vector2( pos0.X,
                    // Add another half-button-height to prevent overlap
                    pos0.Y + (i * Constants.ButtonH) + (Constants.ButtonH/2) );
                bases.Add(TIH.CancelEdit, new TextButtonBase(this, getPosFromIndex(slotOrder)));


                // // // // // // //
                // Makin buttons
                // // // // // // //

                var buttonStack = new Stack<CoreButton>();

                foreach (var tih in new[] { TIH.SortChest, TIH.RSortChest, TIH.LootAll, TIH.DepAll, TIH.QuickStack })
                {
                    buttonStack.Push( new TexturedButton
                        (
                            action : tih,
                            bg_color : tih == TIH.SaveName ?
                                                Constants.ChestSlotColor * 0.85f :
                                                Constants.EquipSlotColor * 0.85f
                        ).With((button) =>
                        {
                            button.Tooltip = button.Label + IHUtils.GetKeyTip(button.Action);

                            if (button.Action == TIH.QuickStack || button.Action == TIH.DepAll)
                                button.AddService(new LockingService<TexturedButton>( button, lockOffset ));
                        })
                    );

                }
            }

        }

    }

}
