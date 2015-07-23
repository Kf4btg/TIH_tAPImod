using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class InventoryButtonLayer : FadingButtonLayer
    {
        InventoryButtonLayer(float min_opacity, float max_opacity, float fade_step) : base("PlayerInventoryButtons", min_opacity, max_opacity, fade_step)
        {}

        /// Use a generator function to seamlessly create and initialize a new
        /// instance of this Button Layer.
        public static InventoryButtonLayer New()
        {
            var newThis = new InventoryButtonLayer(0.45f, 1.0f, 0.5f);
            newThis.Initialize();
            return newThis;
        }

        protected override void AddBasesToLayer()
        {
            // only two buttons right now; they're just above
            // the coin and ammo slots.
            var positions = new Vector2[] {
                new Vector2(496, 28),
                new Vector2(532, 28)
            };

            // Func<int,Vector2> getPosFromIndex = (i) => positions[i];;

            int slotOrder = 0;

            foreach (var tih in new[] {
                TIH.Sort,
                TIH.CleanInv
                })
                ButtonBases.Add(tih, new IconButtonBase(this, positions[slotOrder++], IHBase.ButtonBG));
                // ButtonBases.Add(tih, new IconButtonBase(this, getPosFromIndex(slotOrder++)));
        }

        protected override void AddButtonsToBases()
        {
            var bgColor = Constants.InvSlotColor * 0.8f;

            Func<TIH, string> getLabel = a => a.DefaultLabelForAction(true);

            Func<TIH, string> getTtip  = a => getLabel(a) + a.GetKeyTip();

            Func<TIH, TIH, TexturedButton> getButton
                = (base_by_action, a)
                => TexturedButton.New((ButtonSlot<TexturedButton>)ButtonBases[base_by_action],
                                       action: a,
                                       label: getLabel(a),
                                       tooltip: getTtip(a),
                                       bg_color: bgColor);

            // put buttons together
            var sort  = getButton(TIH.Sort,       TIH.Sort);
            var rsort = getButton(TIH.Sort,       TIH.ReverseSort);
            var clean = getButton(TIH.CleanInv,   TIH.CleanInv);

            // add services/actions

            sort.AddSortToggle(rsort, sort_chest: false);

            clean.EnableDefault();
        }
    }
}
