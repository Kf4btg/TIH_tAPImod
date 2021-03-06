using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class ChestButtonReplacerLayer : ButtonLayer
    {
        protected readonly bool iconButtons;

        private bool showCancel;
        private TextButtonBase CancelEditBase;

        // Constructor

        ///<param name="icons">Use the icon replacers</param>
        ChestButtonReplacerLayer(bool icons) : base("ChestButtonReplacerLayer", false)
        {
            iconButtons = icons;
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
            if (!iconButtons) addTextBases();
            else addIconBases();
        }

        protected override void AddButtonsToBases()
        {
            //DON'T FORGET TO ENABLE CLICK ACTIONS!!
            if (!iconButtons) addTextButtons();
            else addIconButtons();
        }

        // // // // // // //
        // Create the Sockets
        // // // // // // //

        private void addTextBases()
        {
            // position buttons right of chests, below coin slots

            int slotOrder = 0;
            foreach (var action in new[]
            {   // order of creation; determines positioning per the ButtonPlot transform
                TIH.LootAll,       // + sort/rsort
                TIH.DepositAll,    // + smartdep
                TIH.QuickStack,    // + smartloot
            }) ButtonBases.Add(action, new TextButtonBase(this, Constants.TextReplacersPlot.GetPosition(slotOrder++) ));
        }

        private void addIconBases()
        {
            // position of first button = right of chests, below coin slots
            var plot = Constants.IconReplacersPlot;

            int slotOrder = 0;
            foreach (var action in new[]
            {   // order of creation; determines positioning per the ButtonPlot transform
                TIH.Sort,
                TIH.LootAll,
                TIH.DepositAll,    // + smartdep
                TIH.QuickStack,    // + smartloot
                TIH.Rename
            }) ButtonBases.Add(action, new ChestIconBase(this, plot.GetPosition(slotOrder++), IHBase.ButtonBG));

            // Now create the base for the Cancel Edit Button (a text button),
            // but don't add it to the list yet because it only appears under
            // certain conditions (handle in AddButtonsToBases())

            // Add another half-button-height to prevent overlap
            var nudge = new Vector2(0, Constants.ButtonH / 2);
            CancelEditBase = new TextButtonBase(this, plot.GetPosition(slotOrder) + nudge);
        }

        // // // // // // //
        // Makin buttons
        // // // // // // //

        private void addTextButtons()
        {
            // just feels right, man.
            var lockOffset = new Vector2(-20, -18);

            // get original or default label
            Func<TIH, string> getLabel = a => a.DefaultLabelForAction(true)
                + (IHBase.ModOptions["ShowKeyBind"] ? a.GetKeyTip() : "");

            // put it all together, add to base
            Func<TIH, TIH, TextButton> getButton
                = (base_by_action, a)
                => TextButton.New( (ButtonSlot<TextButton>)ButtonBases[base_by_action],
                                       action: a,
                                       label: getLabel(a)
                                       );

            // putting these 2 on same base; see below
            var loot  = getButton(TIH.LootAll,    TIH.LootAll);
            var sort  = getButton(TIH.LootAll,       TIH.Sort);

            var depo  = getButton(TIH.DepositAll, TIH.DepositAll);
            var sdep  = getButton(TIH.DepositAll, TIH.SmartDeposit);
            var qstk  = getButton(TIH.QuickStack, TIH.QuickStack);
            var sloo  = getButton(TIH.QuickStack, TIH.SmartLoot);

            // * we're going to leave the vanilla buttons for these
            // var rena  = getButton(TIH.Rename,     TIH.Rename);
            // var save  = getButton(TIH.Rename,     TIH.SaveName);


            // here's an IDEA: Have LootAll toggle to Sort on shift.
            // Sort will reverse-sort on right-click.
            sort.EnableDefault().Hooks.OnRightClick += () => IHPlayer.Sort(true);
            loot.EnableDefault().AddToggle(sort);

            depo.EnableDefault().MakeLocking(lockOffset).AddToggle(sdep.EnableDefault());
            qstk.EnableDefault().MakeLocking(lockOffset).AddToggle(sloo.EnableDefault());
        }

        private void addIconButtons()
        {
            // offset of lock indicator
            var lockOffset = new Vector2(-(float)(int)((float)Constants.ButtonW / 2) - 4,
                                         -(float)(int)((float)Constants.ButtonH / 2) + 8);

            Func<TIH, string> getLabel = a => Constants.DefaultButtonLabels[a];
            Func<TIH, Color>  getBGcol = (a) => (a == TIH.SaveName)
                                                ? Constants.EquipSlotColor * 0.85f
                                                : Constants.ChestSlotColor * 0.85f;
            Func<TIH, string> getTtip;

            if (IHBase.ModOptions["ShowTooltips"])
                if (IHBase.ModOptions["ShowKeyBind"])
                    getTtip = a => getLabel(a) + IHUtils.GetKeyTip(a);
                else
                    getTtip = a => getLabel(a);
            else
                getTtip = a => "";

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
            //for funsies let's just make that whole toggle thing pointless
            sort.Hooks.OnRightClick += () => IHPlayer.Sort(true);
            rsort.Hooks.OnRightClick += () => IHPlayer.Sort();


            // add default click, let rClick lock it, and make shift switch buttons
            depo.EnableDefault().MakeLocking(lockOffset, Color.Firebrick).AddToggle(sdep.EnableDefault());
            qstk.EnableDefault().MakeLocking(lockOffset, Color.Firebrick).AddToggle(sloo.EnableDefault());

            // these just need their default actions enabled.
            loot.EnableDefault().Hooks.OnRightClick += () => IHPlayer.CleanStacks();  //why not
            cancel.EnableDefault();
            // this prevents the "Cancel" text from being too big when the player
            // goes back into the rename interface (though it seems the vanilla
            // "Cancel" text behaves the same way...improvement!)
            cancel.Hooks.OnClick += CancelEditBase.ResetScale;

            // make Rename Chest button change to Save Name button when
            // clicked, and vice-versa. Well, technically, the buttons will
            // switch automatically when Main.editChest changes state, but
            // since that's what clicking these buttons does...
            save.EnableDefault().AddDynamicToggle(rena.EnableDefault(), () => Main.editChest);
        }

        protected override void DrawButtons(SpriteBatch sb)
        {
            base.DrawButtons(sb);

            // draw the cancel button if needed
            if (iconButtons && Main.editChest)
                CancelEditBase.Draw(sb);
        }
    }
}
