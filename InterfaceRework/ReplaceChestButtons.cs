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
    public class ReplaceChestButtons : ButtonLayer
    {




        public ReplaceChestButtons(): base("ReplaceChestButtons")
        {
            // Order of operations:
            //  - First, create layer (that's what this is doing)
            //  - Second, create bases and assign to layer
            //	 - Finally, create buttons and assign to bases
            // This allow assigning references to parent objects down the stack	 

            var buttonStack = new Stack<CoreButton>();

            var lockOffset = new Vector2((float)(int)((float)Constants.ButtonW/2),
                                        -(float)(int)((float)Constants.ButtonH/2));
            var bgColor = Constants.ChestSlotColor * 0.85f;
            var saveColor = Constants.EquipSlotColor * 0.85f;
            var tex = IHBase.ButtonGrid;
            var bgtex = IHBase.ButtonBG;

            foreach (var tih in new[] {
                TIH.SortChest, TIH.RSortChest,
                TIH.LootAll, TIH.DepAll, TIH.QuickStack
                }) {
                buttonStack.Push(
                    new TexturedButton(
                        action : tih,
                        bgColor : tih == TIH.SaveName ? Constants.ChestSlotColor * 0.85f :
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
