using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    // ///////////////////////////////////////////////
    /// Icon Button with a texture and ability to vary
    /// its appearance when hovered with the mouse.
    // ///////////////////////////////////////////////
    public class TexturedButton : CoreButton
    {
        public Texture2D Texture       { get; set; }
        public Rectangle? InactiveRect { get; set; }
        public Rectangle? ActiveRect   { get; set; }
        public Color BackgroundColor   { get; set; }

        public override Vector2 Size
        {
            get { return InactiveRect.HasValue ? InactiveRect.Value.Size() : Texture.Size(); }
        }

        protected TexturedButton(ButtonSlot<TexturedButton> parent,
                              TIH action,
                              string label,
                              string tooltip           = "",
                              Color? bg_color          = null,
                              Texture2D texture        = null,
                              Rectangle? inactive_rect = null,
                              Rectangle? active_rect   = null
                              ) : base(parent, action, label)
        {
            Tooltip = tooltip; // this should set ShowTooltip = true automatically if not ""
            BackgroundColor = bg_color ?? Color.White;

            Texture      = (texture == null) ? IHBase.ButtonGrid : texture;
            InactiveRect = inactive_rect.HasValue ? inactive_rect : IHUtils.GetSourceRect(action);
            ActiveRect   = active_rect.HasValue ? active_rect : IHUtils.GetSourceRect(action, true);
        }

        /// <summary>
        /// Create a new TexturedButton instance with the given properties
        /// and automatically associate it with its base (ButtonSlot).
        /// </summary>
        /// <param name="parent"> </param>
        /// <param name="action"> </param>
        /// <param name="label"> </param>
        /// <param name="tooltip"> </param>
        /// <param name="bg_color"> </param>
        /// <param name="texture"> </param>
        /// <param name="inactive_rect"> </param>
        /// <param name="active_rect"> </param>
        /// <returns>The newly created TexturedButton</returns>
        public static TexturedButton New(ButtonSlot<TexturedButton> parent,
                                  TIH action,
                                  string label,
                                  string tooltip           = "",
                                  Color? bg_color          = null,
                                  Texture2D texture        = null,
                                  Rectangle? inactive_rect = null,
                                  Rectangle? active_rect   = null)
        {
            var newThis = new TexturedButton(parent, action, label, tooltip, bg_color, texture, inactive_rect, active_rect);

            parent.AddButton(newThis);
            return newThis;
        }
    }

    // ////////////////////////////////////////////////////////////////////////////
    /// Text-only button in the vein of those directly beside the chest in Vanilla
    // ////////////////////////////////////////////////////////////////////////////
    public class TextButton : CoreButton
    {
        // Derived size
        public override Vector2 Size
        {
            get { return Main.fontMouseText.MeasureString(Label); }
        }

        protected TextButton(ButtonSlot<TextButton> parent, TIH action, string label = "")
            : base(parent, action, label) { }

        /// <summary>
        /// Create a new TextButton instance with the given properties
        /// and automatically associate it with its base.</summary>
        /// <param name="parent"> </param>
        /// <param name="action"> </param>
        /// <param name="label"> </param>
        /// <returns>The newly created TextButton</returns>
        public static TextButton New(ButtonSlot<TextButton> parent, TIH action, string label = "")
        {
            var newThis = new TextButton(parent, action, label);

            parent.AddButton(newThis);
            return newThis;
        }
    }
}
