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
            var opacity = ParentLayer.LayerOpacity * Alpha;
            // opacity = 1.0f;
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
}
