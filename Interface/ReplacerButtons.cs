using Microsoft.Xna.Framework;
using TAPI;
// using Terraria;

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


        public TextReplacerButtons(IHBase mbase) : base("TextReplacerButtons")
        {
            this.opacity_inactive = 1.0f;
            // pulled these numbers (and posX) from Terraria.Main
            float posYLA = API.main.invBottom + 40;
            float posYDA = posYLA + 26;
            float posYQS = posYLA + 52;


            // get labels with key-hint
            var labels = new
            {
                lootAll = IHBase.OriginalButtonLabels[IHAction.LA] + IHUtils.GetKeyTip("Loot All"), //could use Constants.ButtonLabels[11]
                depAll  = IHBase.OriginalButtonLabels[IHAction.DA] + IHUtils.GetKeyTip("Deposit All"), //could use Constants.ButtonLabels[9]
                qStack  = IHBase.OriginalButtonLabels[IHAction.QS] + IHUtils.GetKeyTip("Quick Stack"), //could use Constants.ButtonLabels[6]
            };

            var lockOffset = new Vector2(-20, -10);

            var LAButton = ButtonFactory.LootAllButton(labels.lootAll, new Vector2(posX, posYLA), true);
            var DAButton = ButtonFactory.DepositAllButton(labels.depAll, new Vector2(posX, posYDA), this, lockOffset, true);
            var QSButton = ButtonFactory.QuickStackButton(labels.qStack, new Vector2(posX, posYQS), this, lockOffset, true);

            mbase.ButtonRepo.Add(labels.lootAll, LAButton);
            mbase.ButtonRepo.Add(labels.depAll, DAButton);
            mbase.ButtonRepo.Add(labels.qStack, QSButton);

            mbase.ButtonUpdates.Push(labels.depAll);
            mbase.ButtonUpdates.Push(labels.qStack);

            // ButtonBase(ButtonLayer, Button, normal-scale, hovered-scale, scale-step, [alpha] )
            // scale goes from 0.75 (base) -> 1 (when hovered) (from Main code)
            var baseLA = new ButtonBase(this, mbase.ButtonRepo[labels.lootAll], 0.75f, 1.0f, 0.05f);
            var baseDA = new ButtonBase(this, mbase.ButtonRepo[labels.depAll],  0.75f, 1.0f, 0.05f);
            var baseQS = new ButtonBase(this, mbase.ButtonRepo[labels.qStack],  0.75f, 1.0f, 0.05f);

            Buttons.Add(IHAction.LA, baseLA);
            Buttons.Add(IHAction.DA, baseDA);
            Buttons.Add(IHAction.QS, baseQS);

        }

    }
}
