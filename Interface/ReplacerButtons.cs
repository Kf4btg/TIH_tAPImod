using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TAPI;
using Terraria;
using System;

namespace InvisibleHand
{
    public class TextReplacerButtons : ButtonLayer
    {

        // new protected float opacity_inactive = 1.0f;

        private const float posX = 506;
        // these likely won't work without the original buttons...but we'll see
        // public static float scaleLA { get { return API.main.chestLootScale; } }
        // public static float scaleDA { get { return API.main.chestDepositScale; } }
        // public static float scaleQS { get { return API.main.chestStackScale; } }


        public TextReplacerButtons(IHBase mbase, bool swapNewActions = false) : base("TextReplacerButtons")
        {
            this.opacity_inactive = 1.0f;
            // pulled these numbers (and posX) from Terraria.Main
            float posYLA = API.main.invBottom + 40;
            float posYDA = posYLA + 26;
            float posYQS = posYLA + 52;


            // get labels with key-hint
            var labels = new
            {
                lootAll = IHBase.OriginalButtonLabels[TIH.LA] + IHUtils.GetKeyTip("Loot All"), //could use Constants.ButtonLabels[11]
                depAll  = IHBase.OriginalButtonLabels[TIH.DA] + IHUtils.GetKeyTip("Deposit All"), //could use Constants.ButtonLabels[9]
                qStack  = IHBase.OriginalButtonLabels[TIH.QS] + IHUtils.GetKeyTip("Quick Stack"), //could use Constants.ButtonLabels[6]
            };

            var lockOffset = new Vector2(-20, -18);

            var LAButton = ButtonFactory.LootAllButton(labels.lootAll, new Vector2(posX, posYLA), true);
            var DAButton = ButtonFactory.DepositAllButton(labels.depAll, new Vector2(posX, posYDA), this, lockOffset, true);
            var QSButton = ButtonFactory.QuickStackButton(labels.qStack, new Vector2(posX, posYQS), this, lockOffset, true);

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

            Buttons.Add(TIH.LA, baseLA);
            Buttons.Add(TIH.DA, baseDA);
            Buttons.Add(TIH.QS, baseQS);

            // TODO: add sort button somehow?
            if (swapNewActions)
            {
                var nlabels = new
                {
                    smartdep = Constants.ButtonLabels[8] + IHUtils.GetKeyTip(Constants.ButtonLabels[8]),
                    restock = Constants.ButtonLabels[5] + IHUtils.GetKeyTip(Constants.ButtonLabels[5])
                };

                var SDButton = ButtonFactory.SmartDepositButton(nlabels.smartdep, new Vector2(posX, posYDA), true);
                var SLButton = ButtonFactory.SmartLootButton(nlabels.restock, new Vector2(posX, posYQS), true);

                mbase.ButtonRepo.Add(nlabels.smartdep, SDButton);
                mbase.ButtonRepo.Add(nlabels.restock, SLButton);

                //now create keywatchers to toggle Restock/QS & SD/DA
                Buttons[TIH.DA].RegisterKeyToggle(KState.Special.Shift, labels.depAll, nlabels.smartdep);
                Buttons[TIH.QS].RegisterKeyToggle( KState.Special.Shift, labels.qStack, nlabels.restock);


            }

        }

        protected override void OnDraw(SpriteBatch sb)
        {
            if (!parentLayer.visible) return;

            // if (IsHovered)
            // {
                // on this case, the buttons themselves will handle this.
                // Main.localPlayer.mouseInterface = true;
                // we're also not worried about layer opacity here.
                // LayerOpacity=opacity_active;
            DrawButtons(sb);
            // }
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
                CurrentState.label,     //string
                new Vector2(pos.X, pos.Y), //position
                textColor,                    //color
                0f,                        //rotation
                origin,
                Scale,                  //scale
                SpriteEffects.None,        //effects
                0f                         //layerDepth
            );

            origin *= Scale;
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
