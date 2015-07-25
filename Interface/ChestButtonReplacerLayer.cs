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

    public class ChestButtonReplacerLayer : ButtonLayer
    {
        protected readonly bool textButtons;

        private bool showCancel;
        private TextButtonBase CancelEditBase;

        // Constructor

        ChestButtonReplacerLayer(bool text) : base("ChestButtonReplacerLayer", false)
        {
            textButtons = text;
        }

        /// Use a generator function to seamlessly create and initialize a new
        /// instance of this Button Layer.
        public static ChestButtonReplacerLayer New(bool text)
        {
            var newThis = new ChestButtonReplacerLayer(text);
            newThis.Initialize();
            return newThis;
        }

        protected override void AddBasesToLayer()
        {
            if (textButtons) addTextBases();
            else addIconBases();
        }

        protected override void AddButtonsToBases()
        {
            //DON'T FORGET TO ENABLE CLICK ACTIONS!!
            if (textButtons) addTextButtons();
            else addIconButtons();
        }

        // // // // // // //
        // Create the Sockets
        // // // // // // //

        //TODO: standardize this maybe? And put somewhere else.
        /// Plot and return the position of a button based on the origin position
        /// given in the ButtonPlot, shifted by the plot's Offset Vector
        /// a number of times equal to the index (order number).
        /// <example><code>
        /// ButtonPlot bp = new ButtonPlot(10, 20, 0, 15);
        /// Vector2 pos_buttonTheFirst = PlotPosition(bp, 0); // returns (10, 20): the initial position
        /// Vector2 pos_buttonTheThird = PlotPosition(bp, 2); // returns (10, 50): X=(10 + 2*0), Y=(20 + 2*15)
        ///</code></example>
        public static Vector2 PlotPosition(ButtonPlot plot, int index)
        {
            return plot.Origin + index * plot.Offset;
        }

        private void addTextBases()
        {
            // position of first button (right of chests, below coin slots)
            // var pos0 = new Vector2(506, API.main.invBottom + 40);
            // let's try moving it up a bit to account for extra button
            // var pos0 = new Vector2(506, API.main.invBottom + 30);

            // a transform to calculate the position of the socket from the
            // order in which it is created (each offset by button height)
            // Func<int,Vector2> getPosFromIndex
            //     = (i) => new Vector2( pos0.X, pos0.Y + (i * 26) );

            int slotOrder = 0;
            foreach (var action in new[]
            {   // order of creation; determines positioning per the ButtonPlot transform
                // TIH.Sort,
                TIH.LootAll,
                TIH.DepositAll,    // +smartdep
                TIH.QuickStack, // + smartloot
                // TIH.Rename
            }) ButtonBases.Add(action, new TextButtonBase(this, PlotPosition(Constants.TextReplacersPlot, slotOrder++)));

            // create but don't yet add base for Cancel Button
            // CancelEditBase = new TextButtonBase(this, getPosFromIndex(slotOrder));
        }

        private void addIconBases()
        {
            // position of first button (right of chests, below coin slots)
            // var pos0 = new Vector2(506, API.main.invBottom + 22);
            var plot = Constants.IconReplacersPlot;

            // a transform to calculate the position of the socket from the
            // order in which it is created (each offset by button height)
            // Func<int,Vector2> PlotPosition
            //     // = (i) => new Vector2( pos0.X, pos0.Y + (i * Constants.ButtonH) );
            //     = (i) => new Vector2( plot.X, plot.Y + (i * Constants.ButtonH) );

            int slotOrder = 0;

            foreach (var action in new[]
            {   // order of creation; determines positioning per the ButtonPlot transform
                TIH.Sort,
                TIH.LootAll,
                TIH.DepositAll,    // +smartdep
                TIH.QuickStack, // + smartloot
                TIH.Rename
            }) ButtonBases.Add(action, new IconButtonBase(this, PlotPosition(plot, slotOrder++), IHBase.ButtonBG));

            // Now create the base for the Cancel Edit Button (a text button),
            // but don't add it to the list yet because it only appears
            // under certain conditions (handle in AddButtonsToBases())


            // Add another half-button-height to prevent overlap
            var nudge = new Vector2(0, Constants.ButtonH / 2);
            CancelEditBase = new TextButtonBase(this, PlotPosition(plot, slotOrder) + nudge);

            // CancelEditBase = new TextButtonBase(this, new Vector2(pos0.X,
            //                 // Add another half-button-height to prevent overlap
            //                 pos0.Y + (slotOrder * Constants.ButtonH) + (Constants.ButtonH / 2) ));
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
                => TextButton.New( (ButtonSlot<TextButton>)ButtonBases[base_by_action],
                                       action: a,
                                       label: getLabel(a)
                                       );

            // Btn obj            Socket Action   Button Action
            // -------            -------------   -------------
            // var sort  = getButton(TIH.Sort,       TIH.Sort);
            // var rsort = getButton(TIH.Sort,       TIH.ReverseSort);


            var loot  = getButton(TIH.LootAll,    TIH.LootAll);
            var depo  = getButton(TIH.DepositAll,     TIH.DepositAll);
            var sdep  = getButton(TIH.DepositAll,     TIH.SmartDeposit);
            var qstk  = getButton(TIH.QuickStack, TIH.QuickStack);
            var sloo  = getButton(TIH.QuickStack, TIH.SmartLoot);

            // * we're going to leave the vanilla buttons for these
            // var rena  = getButton(TIH.Rename,     TIH.Rename);
            // var save  = getButton(TIH.Rename,     TIH.SaveName);

            loot.EnableDefault();

            depo.EnableDefault().MakeLocking().AddToggle(sdep.EnableDefault());
            qstk.EnableDefault().MakeLocking().AddToggle(sloo.EnableDefault());

        }

        private void addIconButtons()
        {
                // offset of lock indicator
                var lockOffset = new Vector2((float)(int)((float)Constants.ButtonW / 2),
                                            -(float)(int)((float)Constants.ButtonH / 2));

                Func<TIH, string> getLabel = a => Constants.DefaultButtonLabels[a];
                Func<TIH, Color>  getBGcol = (a) => (a == TIH.SaveName)
                                                    ? Constants.ChestSlotColor * 0.85f
                                                    : Constants.EquipSlotColor * 0.85f;
                Func<TIH, string> getTtip  = a => getLabel(a) + IHUtils.GetKeyTip(a);

                Func<TIH, TIH, TexturedButton> getButton
                    = (base_by_action, a)
                    => TexturedButton.New( (ButtonSlot<TexturedButton>)ButtonBases[base_by_action],
                                           action: a,
                                           label: getLabel(a),
                                           tooltip: getTtip(a),
                                           bg_color: getBGcol(a),
                                           texture: IHBase.ButtonGrid,
                                           inactive_rect: IHUtils.GetSourceRect(a),
                                           active_rect: IHUtils.GetSourceRect(a, true)
                                           );

                // Btn obj            Socket Action   Button Action
                // -------            -------------   -------------
                var sort  = getButton(TIH.Sort,       TIH.Sort);
                var rsort = getButton(TIH.Sort,       TIH.ReverseSort);
                var loot  = getButton(TIH.LootAll,    TIH.LootAll);
                var depo  = getButton(TIH.DepositAll, TIH.DepositAll);
                var sdep  = getButton(TIH.DepositAll, TIH.SmartDeposit);
                var qstk  = getButton(TIH.QuickStack, TIH.QuickStack);
                var sloo  = getButton(TIH.QuickStack, TIH.SmartLoot);
                var rena  = getButton(TIH.Rename,     TIH.Rename);
                var save  = getButton(TIH.Rename,     TIH.SaveName);

                var cancel = TextButton.New( CancelEditBase, TIH.CancelEdit, getLabel(TIH.CancelEdit) );


                // Add Services //

                // sort enables default action for sort/rsort by ... default.
                sort.AddSortToggle(rsort);
                // ButtonBases[TIH.Sort].RegisterKeyToggle(KState.Special.Shift, sort.ID, rsort.ID);

                // add default click, let rClick lock it, and make shift switch buttons
                depo.EnableDefault().MakeLocking(lockOffset, Color.Firebrick).AddToggle(sdep.EnableDefault());
                qstk.EnableDefault().MakeLocking(lockOffset, Color.Firebrick).AddToggle(sloo.EnableDefault());

                // these just need their default actions enabled.
                loot.EnableDefault();
                cancel.EnableDefault();

                // make Rename Chest button change to Save Name button
                // when clicked, and vice-versa. Well, technically, the buttons
                // will switch automatically when Main.editChest changes state,
                // but since that's what clicking these buttons does...
                save.EnableDefault().AddDynamicToggle(rena.EnableDefault(), () => Main.editChest);
        }

        protected override void DrawButtons(SpriteBatch sb)
        {
            base.DrawButtons(sb);

            // draw the cancel button if needed
            if (Main.editChest)
                CancelEditBase.Draw(sb);

        }
    }
}
