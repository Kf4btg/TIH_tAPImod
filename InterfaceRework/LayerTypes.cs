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

        private bool showCancel;
        private TextButtonBase CancelEditBase;

        // Constructor

        public ChestButtonReplacerLayer(bool text) : base("ChestButtonReplacerLayer", false)
        {
            textButtons = text;
        }

        protected override void AddBasesToLayer()
        {
            if (textButtons) addTextBases();
            else addIconBases();
        }

        protected override void AddButtonsToBases()
        {
            if (textButtons) addTextButtons();
            else addIconButtons();
        }

        // // // // // // //
        // Create the Sockets
        // // // // // // //

        private void addTextBases()
        {
            // position of first button (right of chests, below coin slots)
            var pos0 = new Vector2(506, API.main.invBottom + 40);
            // let's try moving it up a bit to account for extra button
            // var pos0 = new Vector2(506, API.main.invBottom + 30);

            // a transform to calculate the position of the socket from the
            // order in which it is created (each offset by button height)
            Func<int,Vector2> getPosFromIndex
                = (i) => new Vector2( pos0.X, pos0.Y + (i * 26) );
            int slotOrder = 0;

            foreach (var action in new[]
            {   // order of creation; determines positioning per the transform above
                // TIH.Sort,
                TIH.LootAll,
                TIH.DepAll,    // +smartdep
                TIH.QuickStack, // + smartloot
                // TIH.Rename
            }) ButtonBases.Add(action, new TextButtonBase(this, getPosFromIndex(slotOrder++)));

            // create but don't yet add base for Cancel Button
            // CancelEditBase = new TextButtonBase(this, getPosFromIndex(slotOrder));
        }

        private void addIconBases()
        {
            // position of first button (right of chests, below coin slots)
            var pos0 = new Vector2(506, API.main.invBottom + 22);

            // a transform to calculate the position of the socket from the
            // order in which it is created (each offset by button height)
            Func<int,Vector2> getPosFromIndex
                = (i) => new Vector2( pos0.X, pos0.Y + (i * Constants.ButtonH) );
            int slotOrder = 0;

            foreach (var action in new[]
            {   // order of creation; determines positioning per the transform above
                TIH.Sort,
                TIH.LootAll,
                TIH.DepAll,    // +smartdep
                TIH.QuickStack, // + smartloot
                TIH.Rename
            }) ButtonBases.Add(action, new IconButtonBase(this, getPosFromIndex(slotOrder++), IHBase.ButtonBG));

            // Now create the base for the Cancel Edit Button (a text button),
            // but don't add it to the list yet because it only appears
            // under certain conditions (handle in AddButtonsToBases())

            CancelEditBase = new TextButtonBase(this, new Vector2(pos0.X,
                            // Add another half-button-height to prevent overlap
                            pos0.Y + (slotOrder * Constants.ButtonH) + (Constants.ButtonH / 2) ));
        }

        // // // // // // //
        // Makin buttons
        // // // // // // //

        private void addTextButtons()
        {
            // just feels right, man.
            var lockOffset = new Vector2(-20, -18);

            // get original or default label
            Func<TIH, string> getLabel = a => a.DefaultLabelForAction(true) + a.GetKeyTip();

            // put it all together, add to base
            Func<TIH, TIH, TextButton> getButton
                = (base_by_action, a)
                => TextButton.New( (ButtonSocket<TextButton>)ButtonBases[base_by_action],
                                       action: a,
                                       label: getLabel(a)
                                       );

            // Btn obj            Socket Action   Button Action
            // -------            -------------   -------------
            // var sort  = getButton(TIH.Sort,       TIH.Sort);
            // var rsort = getButton(TIH.Sort,       TIH.ReverseSort);


            var loot  = getButton(TIH.LootAll,    TIH.LootAll);
            var depo  = getButton(TIH.DepAll,     TIH.DepAll);
            var sdep  = getButton(TIH.DepAll,     TIH.SmartDep);
            var qstk  = getButton(TIH.QuickStack, TIH.QuickStack);
            var sloo  = getButton(TIH.QuickStack, TIH.SmartLoot);

            // * we're going to leave the vanilla buttons for these
            // var rena  = getButton(TIH.Rename,     TIH.Rename);
            // var save  = getButton(TIH.Rename,     TIH.SaveName);

            depo.MakeLocking().AddToggle(sdep);
            qstk.MakeLocking().AddToggle(sloo);

        }

        private void addIconButtons()
        {


                // offset of lock indicator
                var lockOffset = new Vector2((float)(int)((float)Constants.ButtonW / 2),
                                            -(float)(int)((float)Constants.ButtonH / 2));

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
                var sort  = getButton(TIH.Sort,       TIH.Sort);
                var rsort = getButton(TIH.Sort,       TIH.ReverseSort);
                var loot  = getButton(TIH.LootAll,    TIH.LootAll);
                var depo  = getButton(TIH.DepAll,     TIH.DepAll);
                var sdep  = getButton(TIH.DepAll,     TIH.SmartDep);
                var qstk  = getButton(TIH.QuickStack, TIH.QuickStack);
                var sloo  = getButton(TIH.QuickStack, TIH.SmartLoot);
                var rena  = getButton(TIH.Rename,     TIH.Rename);
                var save  = getButton(TIH.Rename,     TIH.SaveName);

                var cancel = TextButton.New( CancelEditBase, TIH.CancelEdit, getLabel(TIH.CancelEdit) );


                // Add Services //

                sort.AddSortToggle(rsort, sort_chest: true);

                depo.MakeLocking().AddToggle(sdep);
                qstk.MakeLocking().AddToggle(sloo);

                // make Rename Chest button change to Save Name button
                // when clicked (and vice-versa). Also show/hide Cancel button
                rena.AddDynamicToggle(save, () =>
                {
                    // Need to know if the player has clicked the Rename Chest button
                    if (Main.editChest)
                    {
                        if (!showCancel) // cancel button not shown, need to change that
                        {
                            // add cancel base to the layer's list of bases so it gets drawn
                            // (don't throw an error if it's already there)
                            ButtonBases[TIH.CancelEdit] = CancelEditBase;
                            showCancel = true;
                        }
                        // since the save button is the "show when false" button,
                        // we have to return false when Main.Edit is true, and true
                        // when it is false.
                        // Which is exactly what we'll do,
                        // rather than checking the negation.
                        return false;
                    }
                    if (showCancel) // need to hide cancel button
                    {
                        // remove from the base list so no calls to Draw() reach it
                        ButtonBases.Remove(TIH.CancelEdit);
                        showCancel = false;
                    }
                    return true;
                });
        }
    }
}
