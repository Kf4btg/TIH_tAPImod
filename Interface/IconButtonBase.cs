using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class IconButtonBase : ButtonSlot<TexturedButton>
    {
        /// Texture resource that will be drawn in the background of this buttonbase.
        /// BgColor property of current button content object is used for tint.
        public Texture2D ButtonBackground { get; set; }

        /// this will actively set the Source Texels based on whether or not the mouse is currently over this button.
        /// If both rects are null, then the entire texture will be drawn as per default
        private Rectangle? SourceRect
        {
            get { return HasMouseFocus ?
                         CurrentContent.ActiveRect :
                         CurrentContent.InactiveRect; }
        }

        public IconButtonBase(ButtonLayer parent, Vector2 position, Texture2D button_bg ) : base(parent, position)
        {
            ButtonBackground = button_bg;
        }

        protected override void DrawButtonContent(SpriteBatch sb)
        {
            // take button's alpha and paren't opacity into account
            // (yes maybe those should be named the same but whatever)
            var opacity = ParentLayer.LayerOpacity * Alpha;

            // draw button background first
            // (otherwise button content will be below bg!)
            sb.Draw(ButtonBackground,                           // texture
                    Position,                                   // position
                    null,                                       // sourceRectangle (null=all)
                    CurrentContent.BackgroundColor * opacity,   // color
                    0f,                                         // rotation
                    default(Vector2),                           // origin
                    Scale,                                      // scale
                    SpriteEffects.None,                         // effects
                    0f);                                        // layerdepth

            // and now the real button stuff
            sb.Draw(CurrentContent.Texture,
                    Position,
                    SourceRect,
                    CurrentContent.Tint * opacity,
                    0f,
                    default(Vector2),
                    Scale,
                    SpriteEffects.None,
                    0f);
        }
    }

    /// fancy fading! Also handles mouseInterface.
    /// actually...forget the fancy fading. for now at least.
    // I'd prefer the button fade from the inactive state to
    // active (i.e. the color eases in & out), but I think
    // that might involve drawing an additional texture layer
    // and doing some sprite blending or just some very carefully
    // calculated synchronizization of alpha values. Either way,
    // it likely won't be straight-forward and I wonder about
    // the performance hit. For future investigation.
    public class ChestIconBase : IconButtonBase
    {
        private float maxAlpha;
        private float alphaStep;

        public ChestIconBase(ButtonLayer parent, Vector2 position, Texture2D button_bg,
        float base_alpha = 0.75f,
        float focus_alpha = 1.0f,
        float alpha_step = 0.01f ) : base(parent, position, button_bg)
        {
            // BaseAlpha = base_alpha;
            // maxAlpha = focus_alpha.Clamp(BaseAlpha, 1.0f);
            //
            // alpha_step = (BaseAlpha == maxAlpha) ? 0 : alpha_step.Clamp(0.001f, (maxAlpha - BaseAlpha));

            // Alpha = BaseAlpha;
        }

        protected override void WhenFocused()
        {
            base.WhenFocused(); // draws tooltips

            Main.localPlayer.mouseInterface = true;
            // if (Alpha != maxAlpha)
            //     Alpha += alphaStep;
        }

        // protected override void WhenNotFocused()
        // {
        //     if (Alpha != BaseAlpha)
        //         Alpha -= alphaStep;
        // }

    }
}
