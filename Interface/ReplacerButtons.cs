using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TAPI;
using Terraria;
using System;

namespace InvisibleHand
{
    public class TextReplacerButtons : ButtonLayer
    {
        private const float posX = 506;

        /// Without swapNewActions == true, these buttons will look and act
        /// just like fancy versions of the normal Quick Stack, Loot All, and
        /// Deposit All text-buttons. With swapNewActions, holding down the
        /// Shift button will swap Loot/Deposit buttons with their smart counterparts.
        public TextReplacerButtons(IHBase mbase, bool swapNewActions = false) : base("TextReplacerButtons")
        {
            this.opacity_inactive = 1.0f; // don't fade buttons

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

            mbase.ButtonRepo.Add(labels.lootAll, LAButton);
            mbase.ButtonRepo.Add(labels.depAll, DAButton);
            mbase.ButtonRepo.Add(labels.qStack, QSButton);

            mbase.ButtonUpdates.Push(labels.depAll);
            mbase.ButtonUpdates.Push(labels.qStack);

            // TextReplacerBase(ButtonLayer, Button, normal-scale, hovered-scale, scale-step, [alpha] )
            // scale goes from 0.75 (base) -> 1 (when hovered) (from Main code)
            var baseLA = new TextReplacerBase(this, mbase.ButtonRepo[labels.lootAll]);
            var baseDA = new TextReplacerBase(this, mbase.ButtonRepo[labels.depAll]);
            var baseQS = new TextReplacerBase(this, mbase.ButtonRepo[labels.qStack]);

            Buttons.Add(TIH.LootAll, baseLA);
            Buttons.Add(TIH.DepAll, baseDA);
            Buttons.Add(TIH.QuickStack, baseQS);

            // TODO: add sort button somehow somewhere?
            if (swapNewActions)
            {
                var nlabels = new
                {
                    smartdep = Constants.ButtonLabels[8] + IHUtils.GetKeyTip(Constants.ButtonLabels[8]),
                    restock  = Constants.ButtonLabels[5] + IHUtils.GetKeyTip(Constants.ButtonLabels[5])
                };

                var SDButton = ButtonFactory.GetSimpleButton(TIH.SmartDep, nlabels.smartdep, new Vector2(posX, posYDA), true);
                var SLButton = ButtonFactory.GetSimpleButton(TIH.SmartLoot, nlabels.restock, new Vector2(posX, posYQS), true);

                mbase.ButtonRepo.Add(nlabels.smartdep, SDButton);
                mbase.ButtonRepo.Add(nlabels.restock, SLButton);

                //now create keywatchers to toggle Restock/QS & SD/DA
                Buttons[TIH.DepAll].RegisterKeyToggle(KState.Special.Shift, labels.depAll, nlabels.smartdep);
                Buttons[TIH.QuickStack].RegisterKeyToggle( KState.Special.Shift, labels.qStack, nlabels.restock);
            }
        }

        protected override void OnDraw(SpriteBatch sb)
        {
            if (!parentLayer.visible) return;

            // handling mouseInterface individually by button
            DrawButtons(sb);
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
                if (!hasMouseFocus)
                {
                    hasMouseFocus=true;
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
            if (hasMouseFocus)
                OnMouseLeave();
            if (Scale!=baseScale)
                Scale -= scaleStep;
            hasMouseFocus=false;

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
