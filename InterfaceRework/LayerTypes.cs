using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    // Order of operations:
    //  - First, create layer (that's what this is doing)
    //  - Second, create bases and assign to layer
    //	 - Finally, create buttons and assign to bases
    // This allows assigning references to parent objects down the stack

    public class ChestButtonReplacerLayer : ButtonContainerLayer
    {
        protected readonly bool textButtons;

        // Constructor

        public ChestButtonReplacerLayer(bool text) : base("ChestButtonReplacerLayer", false)
        {
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
                {   // order of creation; determines positioning per the transform above
                    TIH.SortChest,
                    TIH.LootAll,
                    TIH.DepAll,    // +smartdep
                    TIH.QuickStack, // + smartloot
                    TIH.Rename
                }) ButtonBases.Add(action, new IconButtonBase(this, getPosFromIndex(slotOrder++), IHBase.ButtonBG));

                // Now add the base for the Cancel Edit Button, a text button which
                // only appears under certain conditions.
                getPosFromIndex = (i) => new Vector2(
                                pos0.X,
                                // Add another half-button-height to prevent overlap
                                pos0.Y + (i * Constants.ButtonH) + (Constants.ButtonH / 2) );

                ButtonBases.Add(TIH.CancelEdit, new TextButtonBase(this, getPosFromIndex(slotOrder)));
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

                // // // // // // //
                // Makin buttons
                // // // // // // //

                Func<TIH, string> getLabel = a => Constants.DefaultButtonLabels[a];
                Func<TIH, Color>  getBGcol = a => a == TIH.SaveName
                                                    ? Constants.ChestSlotColor * 0.85f
                                                    : Constants.EquipSlotColor * 0.85f;
                Func<TIH, string> getTtip  = a => getLabel(a) + IHUtils.GetKeyTip(a);

                Func<TIH, TIH, TexturedButton> getButton
                    = (base_by_action, a)
                    => TexturedButton.New( (ButtonSocket<TexturedButton>)ButtonBases[base_by_action],
                                           action: a,
                                           label: getLabel(a),
                                           tooltip: getTtip(a),
                                           bg_color: getBGcol(a) );

                // Btn obj            Socket Action   Button Action
                // -------            -------------   -------------
                var sort  = getButton(TIH.SortChest,  TIH.SortChest);
                var rsort = getButton(TIH.SortChest,  TIH.RSortChest);
                var loot  = getButton(TIH.LootAll,    TIH.LootAll);
                var depo  = getButton(TIH.DepAll,     TIH.DepAll);
                var sdep  = getButton(TIH.DepAll,     TIH.SmartDep);
                var qstk  = getButton(TIH.QuickStack, TIH.QuickStack);
                var sloo  = getButton(TIH.QuickStack, TIH.SmartLoot);
                var rena  = getButton(TIH.Rename,     TIH.Rename);
                var save  = getButton(TIH.Rename,     TIH.SaveName);

                var cancel = TextButton.New(
                             (ButtonSocket<TextButton>)ButtonBases[TIH.CancelEdit],
                             TIH.CancelEdit, getLabel(TIH.CancelEdit) );


                // Add Services //

                sort.AddSortToggle(rsort, sort_chest: true);

                depo.MakeLocking().AddToggle(sdep);
                qstk.MakeLocking().AddToggle(sloo);

            }
        }
    }
}
