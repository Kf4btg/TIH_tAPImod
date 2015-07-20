using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{

    public class IconButtonBase : ButtonSocket<TexturedButton>
    {

        /// Texture resource that will be drawn in the background of this buttonbase.
        /// BgColor property of current button content object is used for tint.
        public Texture2D ButtonBackground { get; set; }

        /// this will actively set the Source Texels based on whether or not the mouse is currently over this button.
        /// If both rects are null, then the entire texture will be drawn as per default
        public Rectangle? SourceRect
        {
            get { return HasMouseFocus ?
                            CurrentContent.ActiveRect :
                            CurrentContent.InactiveRect;
                }
        }

        public IconButtonBase(ButtonContainerLayer parent, Vector2 position, Texture2D button_bg ) : base(parent, position)
        {
            ButtonBackground = button_bg;
            DefaultContent = CurrentContent = new TexturedButton();
        }

        public IconButtonBase(ButtonContainerLayer parent, TexturedButton content, Vector2 position, Texture2D button_bg ) : base(parent, position)
        {
            ButtonBackground = button_bg;
            SetDefault(content);
        }

        protected override void DrawButtonContent(SpriteBatch sb)
        {
            var opacity = ParentLayer.LayerOpacity*Alpha;
            //draw button background first
            // (otherwise button content will be below bg!)
            sb.Draw(ButtonBackground,
                    Position,
                    null,
                    CurrentContent.BackgroundColor*opacity,
                    0f,
                    default(Vector2),
                    Scale,
                    SpriteEffects.None,
                    0f);

            // and now the real button stuff
            sb.Draw(CurrentContent.Texture,
                    Position,
                    SourceRect,
                    CurrentContent.Tint*opacity,
                    0f,
                    default(Vector2),
                    Scale,
                    SpriteEffects.None,
                    0f);
        }

    }

    public class TextButtonBase : ButtonSocket<TextButton>
    {

        //class fields//

        /// modified position used in scaling/hover calculations
        private Vector2 posMod;

        /// makes the button smoothly grow and shrink as the mouse moves on and off
        private float scaleStep = float.MaxValue;

        //Properties//

        /// Get the (relative) center of the full-size button
        private Vector2 origin
        {
            get { return CurrentContent.Size / 2; }
        }
        /// shift origin up-right or down-left as button is scaled
        private Vector2 scaledOrigin
        {
            get { return origin * Scale; }
        }

        /// get current color of the text
        public Color TextColor
        {
            // basing textColor on Main.mouseTextColor enables
            // the "pulse" effect all the vanilla text has;
            // Combining it with the current scale makes the
            // intensity of the text color fade up and down as the
            // button zooms in or out.
            get { return Main.mouseTextColor.toScaledColor(Scale); }
        }

        ///<summary>
        /// Create an empty socket at the given position</summary>
        public TextButtonBase
        (   ButtonContainerLayer parent, Vector2 position,
            float base_scale = 0.75f,
            float focus_scale = 1.0f,
            float scale_step = 0.05f
        ) : base(parent, position)
        {
            posMod = Position;
            // 30 is honestly kind of a ridiculously high limit;
            // I don't think I saw any text-scaling values go higher
            // than 4 in the vanilla code.
            _minScale = base_scale.Clamp(0.5f, 30.0f);
            _maxScale = focus_scale.Clamp(_minScale, 30.0f);


            scaleStep = (_minScale == _maxScale) ? 0 :
             scale_step;
        }

        public TextButtonBase
        (   ButtonContainerLayer parent, TextButton content, Vector2 position,
            float base_scale = 0.75f, float focus_scale = 1.0f, float scale_step = 0.05f
        ) : this(parent, position, base_scale, focus_scale, scale_step)
        {
            base.SetDefault(content);
        }

        protected override bool GetIsHovered(Vector2 mouse)
        {
            var o = scaledOrigin; //cache it
            return (float)mouse.X > (float)posMod.X - o.X &&
                    (float)mouse.X < (float)posMod.X + o.X &&
                    (float)mouse.Y > (float)posMod.Y - o.Y &&
                    (float)mouse.Y < (float)posMod.Y + o.Y;
        }

        protected override void DrawButtonContent(SpriteBatch sb)
        {
            // var textColor = Main.mouseTextColor.toScaledColor(Scale, CurrentState.tint);

            posMod = Position; //reset
            posMod.X += (int)(origin.X * Scale);

            sb.DrawString(
                Main.fontMouseText,        //font
                CurrentContent.Label,        //string
                new Vector2(posMod.X, posMod.Y), //position
                TextColor,                 //color
                0f,                        //rotation
                origin,
                Scale,
                SpriteEffects.None,        //effects
                0f                         //layerDepth
            );
        }

        /// Handle mouseInterface, Scale up
        protected override void WhenFocused()
        {
            // handling mouseInterface individually rather than by
            // the ButtonFrame so that the buttons will act like the
            // vanilla versions.
            Main.localPlayer.mouseInterface = true;
            if (Scale!=_maxScale)
                Scale += scaleStep;
        }

        /// Scale down
        protected override void WhenNotFocused()
        {
            if (Scale!=_minScale)
                Scale -= scaleStep;
        }

    }
}
