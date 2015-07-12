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
    public abstract class ReplacerButtons : ButtonLayer
    {
        protected const float posX = 506;

        protected ReplacerButtons(string name) : base(name)
        {
            this.opacity_inactive = 1.0f; // don't fade buttons
        }

        protected override void OnDraw(SpriteBatch sb)
        {
            if (!parentLayer.visible) return;

            // handling mouseInterface individually by button
            DrawButtons(sb);
        }

    }

    public class TextReplacerButtons : ReplacerButtons
    {
        // private static float posYLA = API.main.invBottom + 40;
        // private static float posYDA = posYLA + 26;
        // private static float posYQS =  posYLA + 52;
        /// Without swapNewActions == true, these buttons will look and act
        /// just like fancy versions of the normal Quick Stack, Loot All, and
        /// Deposit All text-buttons. With swapNewActions, holding down the
        /// Shift button will swap Loot/Deposit buttons with their smart counterparts.
        public TextReplacerButtons(IHBase mbase, bool swapNewActions = false) : base("TextReplacerButtons")
        {
            // this.opacity_inactive = 1.0f; // don't fade buttons

            // pulled these numbers (and posX) from Terraria.Main
            float posYLA = API.main.invBottom + 40;
            float posYDA = posYLA + 26;
            float posYQS = posYLA + 52;

            // get labels with key-hint (e.g. "Loot All (Z)")
            var labels = new
            {
                lootAll = IHBase.OriginalButtonLabels[TIH.LootAll]    + IHUtils.GetKeyTip(TIH.LootAll),
                depAll  = IHBase.OriginalButtonLabels[TIH.DepAll]     + IHUtils.GetKeyTip(TIH.DepAll),
                qStack  = IHBase.OriginalButtonLabels[TIH.QuickStack] + IHUtils.GetKeyTip(TIH.QuickStack),
            };

            var lockOffset = new Vector2(-20, -18);

            var LAButton = ButtonFactory.GetSimpleButton(TIH.LootAll, labels.lootAll, new Vector2(posX, posYLA), true);
            var DAButton = ButtonFactory.GetLockableTextButton(TIH.DepAll, labels.depAll, new Vector2(posX, posYDA), this, lockOffset);
            var QSButton = ButtonFactory.GetLockableTextButton(TIH.QuickStack, labels.qStack, new Vector2(posX, posYQS), this, lockOffset);

            // mbase.ButtonRepo.Add(labels.lootAll, LAButton);
            IHUtils.AddToButtonRepo(LAButton);
            // mbase.ButtonRepo.Add(labels.depAll, DAButton);
            IHUtils.AddToButtonRepo(DAButton);
            // mbase.ButtonRepo.Add(labels.qStack, QSButton);
            IHUtils.AddToButtonRepo(QSButton);


            mbase.ButtonUpdates.Push(LAButton.ID);
            mbase.ButtonUpdates.Push(QSButton.ID);

            // TextReplacerBase(ButtonLayer, Button, normal-scale, hovered-scale, scale-step, [alpha] )
            // scale goes from 0.75 (base) -> 1 (when hovered) (from Main code)
            var baseLA = new TextReplacerBase(this, LAButton);
            var baseDA = new TextReplacerBase(this, DAButton);
            var baseQS = new TextReplacerBase(this, QSButton);

            Buttons.Add(TIH.LootAll, baseLA);
            Buttons.Add(TIH.DepAll, baseDA);
            Buttons.Add(TIH.QuickStack, baseQS);

            // TODO: add sort button somehow somewhere?
            if (swapNewActions)
            {
                var nlabels = new
                {
                    smartdep = Constants.ButtonLabels[8] + IHUtils.GetKeyTip(TIH.SmartDep),
                    restock  = Constants.ButtonLabels[5] + IHUtils.GetKeyTip(TIH.SmartLoot)
                };

                var SDButton = ButtonFactory.GetSimpleButton(TIH.SmartDep, nlabels.smartdep, new Vector2(posX, posYDA), true);
                var SLButton = ButtonFactory.GetSimpleButton(TIH.SmartLoot, nlabels.restock, new Vector2(posX, posYQS), true);

                // mbase.ButtonRepo.Add(nlabels.smartdep, SDButton);
                IHUtils.AddToButtonRepo(SDButton);
                // mbase.ButtonRepo.Add(nlabels.restock, SLButton);
                IHUtils.AddToButtonRepo(SLButton);

                //now create keywatchers to toggle Restock/QS & SD/DA
                Buttons[TIH.DepAll].RegisterKeyToggle(KState.Special.Shift, DAButton, SDButton);
                Buttons[TIH.QuickStack].RegisterKeyToggle( KState.Special.Shift, QSButton, SLButton);
            }
        }

        // moved this to ReplacerButtons base class
        // protected override void OnDraw(SpriteBatch sb)
        // {
        //     if (!parentLayer.visible) return;
        //
        //     // handling mouseInterface individually by button
        //     DrawButtons(sb);
        // }
    }

    public class IconReplacerButtons : ReplacerButtons
    {
        private static readonly Dictionary<TIH, TIH> togglesWith = new Dictionary<TIH, TIH>{
            {TIH.SortChest, TIH.RSortChest},
            {TIH.SmartLoot, TIH.QuickStack},
            {TIH.SmartDep, TIH.DepAll}
        };

        public readonly Dictionary<TIH, ButtonBase> ChestEditButtons;

        public IconReplacerButtons(IHBase mbase, bool swapNewActions = false) : base("IconReplacerButtons")
        {
            ChestEditButtons = new Dictionary<TIH, ButtonBase>();

            // move this up due to larger buttons
            float posYLA = API.main.invBottom + 22;
            // float posYDA = posYLA + 26;
            // float posYDA = posYLA + Constants.ButtonH; //32
            // // float posYQS = posYLA + 52;
            // float posYQS = posYDA + Constants.ButtonH;
            // //edit chest
            // float posYEC = posYQS + Constants.ButtonH;
            // // cancel edit
            // float posYCE = posYEC + Constants.ButtonH;

            var PosY = new Dictionary<TIH, float>{
                // {TIH.SortChest,  posYLA},
                // {TIH.RSortChest, posYLA},
                {TIH.LootAll,    posYLA},

                {TIH.DepAll,     posYLA + Constants.ButtonH},
                {TIH.SmartDep,   posYLA + Constants.ButtonH},

                {TIH.QuickStack, posYLA + 2 * Constants.ButtonH},
                {TIH.SmartLoot,  posYLA + 2 * Constants.ButtonH},

                {TIH.Rename,     posYLA + 3 * Constants.ButtonH},
                {TIH.SaveName,   posYLA + 3 * Constants.ButtonH},
                // add another half-height space to prevent overlap
                {TIH.CancelEdit, posYLA + 4 * Constants.ButtonH + (Constants.ButtonH/2)}
            };

            var lockOffset = new Vector2((float)(int)((float)Constants.ButtonW/2),
                                        -(float)(int)((float)Constants.ButtonH/2));


            // var simpleActions = new TIH[] { TIH.SortChest, TIH.RSortChest, TIH.SmartDep, TIH.SmartLoot };
            var simpleActions = new TIH[] { TIH.LootAll, TIH.SmartDep, TIH.SmartLoot };

            var lockingActions = new TIH[] { TIH.QuickStack, TIH.DepAll };

            foreach (var a in lockingActions)
            {
                var button = ButtonFactory.GetLockableButton(a, new Vector2(posX, PosY[a]), this, lockOffset);
                button.DisplayState.PreDraw = PreDraw;

                IHUtils.AddToButtonRepo(button);
                // set QS & DA to have their state initialized on world load
                mbase.ButtonUpdates.Push(button.ID);

                Buttons.Add(a, new ButtonBase(this, button));
            }

            foreach (var a in simpleActions)
            {
                // uses default label for the action
                var button = ButtonFactory.GetSimpleButton(a, new Vector2(posX, PosY[a]));
                button.DisplayState.PreDraw = PreDraw;

                IHUtils.AddToButtonRepo(button);

                if (a == TIH.LootAll)
                    Buttons.Add(a, new ButtonBase(this, button));
                else
                    Buttons[togglesWith[a]].RegisterKeyToggle(KState.Special.Shift, button);
            }

            // =============================================================================
            // set up the Rename/SaveName Button(s)

            var rename = ButtonFactory.GetSimpleButton(
                            TIH.Rename,
                            IHBase.OriginalButtonLabels[TIH.Rename],
                            new Vector2(posX, PosY[TIH.Rename])
                            );
            var savename = ButtonFactory.GetSimpleButton(
                            TIH.SaveName,
                            IHBase.OriginalButtonLabels[TIH.SaveName],
                            new Vector2(posX, PosY[TIH.SaveName])
                            );

            // whoops, this was easier than I thought
            // rename.DisplayState.onClick = () => EditChest.DoChestEdit();
            // savename.DisplayState.onClick = () => EditChest.DoChestEdit();

            // mbase.ButtonRepo.Add(rename.Label, rename);
            IHUtils.AddToButtonRepo(rename);
            // mbase.ButtonRepo.Add(savename.Label, savename);
            IHUtils.AddToButtonRepo(savename);

            ChestEditButtons.Add(TIH.Rename, new ButtonBase(this, rename));
            ChestEditButtons.Add(TIH.SaveName, new ButtonBase(this, savename));

            // --------------------------------------------
            // And now the Cancel button.
            // It's just gonna be text because ehhhhh

            var cancel = ButtonFactory.GetSimpleButton(
                            TIH.CancelEdit,
                            IHBase.OriginalButtonLabels[TIH.CancelEdit],
                            new Vector2(posX, PosY[TIH.CancelEdit]),
                            true
                            );
            // cancel.DisplayState.onClick = () => EditChest.CancelRename();
            // mbase.ButtonRepo.Add(cancel.Label, cancel);
            IHUtils.AddToButtonRepo(cancel);


            ChestEditButtons.Add(TIH.CancelEdit, new TextReplacerBase(this, cancel));
        }

        private static readonly Color bgColor = Constants.ChestSlotColor*0.8f;
        private static readonly Color saveNameBGColor = Constants.EquipSlotColor*0.8f;

        /// Draw each button in this layer (bg first, then button)
        protected override void DrawButtons(SpriteBatch sb)
        {
            foreach (KeyValuePair<TIH, ButtonBase> kvp in Buttons)
            {
                sb.DrawButtonBG(kvp.Value, IHBase.ButtonBG, bgColor);
                kvp.Value.Draw(sb);
            }
            // foreach (var kvp in ChestEditButtons)
            // {
            if (Main.editChest)
            {
                sb.DrawButtonBG(ChestEditButtons[TIH.SaveName], IHBase.ButtonBG, saveNameBGColor);
                ChestEditButtons[TIH.SaveName].Draw(sb);
                ChestEditButtons[TIH.CancelEdit].Draw(sb);
            }
            else
            {
                sb.DrawButtonBG(ChestEditButtons[TIH.Rename], IHBase.ButtonBG, bgColor);
                ChestEditButtons[TIH.Rename].Draw(sb);
            }
            // }
        }

        public bool PreDraw(SpriteBatch sb, ButtonBase bb)
        {
            sb.DrawIHButton(bb, bb.CurrentContext.DisplayState);

            if (bb.IsHovered)
            {
                if (!bb.HasMouseFocus) { bb.HasMouseFocus=true; bb.OnMouseEnter(); }

                //handling mouseInterface individually again
                Main.localPlayer.mouseInterface = true;

                if (bb.AlphaMult!=1.0f)
                    bb.AlphaMult += bb.alphaStep;

                bb.OnHover();

                return false;
            }
            if (bb.HasMouseFocus) { bb.OnMouseLeave(); }
            if (bb.AlphaMult!=bb.alphaBase)
                    bb.AlphaMult -= bb.alphaStep;
            bb.HasMouseFocus=false;

            return false;
        }

    }

    public class TextReplacerBase : ButtonBase
    {
        private readonly float baseScale;
        private readonly float hoverScale;
        private readonly float scaleStep;

        public override float Scale {
            get { return scale; }
            set { scale = value < baseScale ? baseScale :
                          value > hoverScale ? hoverScale : value;
                }
            }

        /**
        * Enable scale effect on the button by providing a base scaling value (values less than
        * 0.5 will be set to 0.5) and optionally one for on-mouse-hover (defaults to 1.0f).
        * Values larger than 1.0 are allowed. Provide a scale_step larger than 0 to control
        * how quickly the scale effect eases in and out (default is .05 / frame).
        */
        public TextReplacerBase(ButtonLayer container, IHButton defaultContext,
            float base_scale = 0.75f,
            float focused_scale = 1.0f,
            float scale_step = 0.05f) : base(container, defaultContext, 1.0f)
        {
            this.baseScale = base_scale < 0.5f ? 0.5f : base_scale;
            Scale = this.baseScale;
            this.hoverScale = focused_scale;
            this.scaleStep = scale_step;
        }

        /// overriding Draw to focus on the oddities of drawing/scaling these buttons
        public override void Draw(SpriteBatch sb)
        {
            //doing this enables the "pulse" effect all the vanilla text has
            // var textColor = Main.mouseTextColor.toScaledColor(Scale, CurrentState.tint);
            var textColor = Main.mouseTextColor.toScaledColor(Scale);

            // and setting the origin and position like this makes the word expand
            // from its center (rather than the top-left edge) while staying anchored
            // on left side (i.e. it expands to the right only)
            var origin = Size / 2;

            var pos = Position;
            pos.X += (int)(origin.X * Scale);

            sb.DrawString(
                Main.fontMouseText,        //font
                CurrentState.label,        //string
                new Vector2(pos.X, pos.Y), //position
                textColor,                 //color
                0f,                        //rotation
                origin,
                Scale,
                SpriteEffects.None,        //effects
                0f                         //layerDepth
            );

            // shift origin up-right or down-left
            origin *= Scale;
            // use specialized isHovered() below
            if (isHovered(pos, origin))
            {
                if (!HasMouseFocus)
                {
                    HasMouseFocus=true;
                    OnMouseEnter();
                }

                // handling mouseInterface individually rather than by
                // the ButtonFrame so that the buttons will act like the
                // vanilla versions.
                Main.localPlayer.mouseInterface = true;

                if (Scale!=hoverScale)
                    Scale += scaleStep;

                OnHover();
                currentContext.PostDraw(sb, this);
                return;
            }
            if (HasMouseFocus)
                OnMouseLeave();
            if (Scale!=baseScale)
                Scale -= scaleStep;
            HasMouseFocus=false;

            currentContext.PostDraw(sb, this);
        }

        /// weird.
        private bool isHovered(Vector2 pos, Vector2 origin)
        {
            return (float)Main.mouseX > (float)pos.X - origin.X &&
            (float)Main.mouseX < (float)pos.X + origin.X &&
            (float)Main.mouseY > (float)pos.Y - origin.Y &&
            (float)Main.mouseY < (float)pos.Y + origin.Y;
        }

        /// Also not bothering with opacity.
        public override void OnHover()
        {
            if (Main.mouseLeft && Main.mouseLeftRelease)
                currentContext.OnClick();

            if (Main.mouseRight && Main.mouseRightRelease && currentContext.OnRightClick!=null)
                currentContext.OnRightClick();
        }
    }
}
