using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    /// Having subclasses of CoreButton ALSO implement this interface seems to be
    /// the ONLY way to get a reliably type-safe reference to the parent buttonbase
    public interface ISocketedButton<T> where T: CoreButton
    {
        ButtonSocket<T> ButtonBase { get; }
    }

    // ///////////////////////////////////////////////
    /// Icon Button with a texture and ability to vary
    /// its appearance when hovered with the mouse.
    // ///////////////////////////////////////////////
    public class TexturedButton : CoreButton, ISocketedButton<TexturedButton>
    {

        //store for implementing ISocketedButton
        private ButtonSocket<TexturedButton> _parent;


        // protected Texture2D texture;
        // protected Rectangle? defaultTexels;
        // protected Rectangle? altTexels;
        //
        // protected Color bgColor;

        // public Texture2D Texture       { get { return texture; }       set { texture       = value; } }
        // public Rectangle? InactiveRect { get { return defaultTexels; } set { defaultTexels = value; } }
        // public Rectangle? ActiveRect   { get { return altTexels; }     set { altTexels     = value; } }
        //
        // public Color BgColor           { get { return bgColor; }       set { bgColor       = value; } }

        //Can I get rid of the backing stores? They were honestly just
        //for default values, but maybe those are generated for properties too
        public Texture2D Texture       { get; set; }
        public Rectangle? InactiveRect { get; set; }
        public Rectangle? ActiveRect   { get; set; }

        public Color BackgroundColor   { get; set; }



        public override Vector2 Size
        {
            get {
                return InactiveRect.HasValue ? InactiveRect.Value.Size() : Texture.Size();
            }
        }

        /// Get ButtonSocket in which this button is placed
        public ButtonSocket<TexturedButton> ButtonBase
        {
            get { return _parent; }
            protected set { _parent = value; }
        }


        public TexturedButton() : base()
        {
            Texture = IHBase.ButtonBG;
            BackgroundColor = Color.White;

        }

        public TexturedButton(TIH action,
                              string label = "",
                              Color? bg_color = null,
                              Texture2D texture = null,
                              Rectangle? default_texels = null,
                              Rectangle? focused_texels = null
                              ) : base(action, label)
        {
            BackgroundColor = bg_color ?? Color.White;
            Texture = (texture==null) ? IHBase.ButtonGrid : texture;

            InactiveRect = default_texels.HasValue ? default_texels : IHUtils.GetSourceRect(action);
            ActiveRect = focused_texels.HasValue ? focused_texels : IHUtils.GetSourceRect(action, true);

        }

        public override void CopyAttributes(CoreButton other_button)
        {
            base.CopyAttributes(other_button);

            //being an inherited method, there didn't seem to be
            // any practical way to make this method require a
            // TexturedButton; so here we'll check type and
            // handle the other properties once we're sure
            if (other_button is TexturedButton)
            {
                // have to cast to access properties
                TexturedButton other = (TexturedButton)other_button;
                this.Texture = other.Texture;
                this.InactiveRect = other.InactiveRect;
                this.ActiveRect = other.ActiveRect;
                this.BackgroundColor = other.BackgroundColor;

            }
        }

    }



    // ////////////////////////////////////////////////////////////////////////////
    /// Text-only button in the vein of those directly beside the chest in Vanilla
    // ////////////////////////////////////////////////////////////////////////////
    public class TextButton : CoreButton, ISocketedButton<TextButton>
    {

        private ButtonSocket<TextButton> _parent;

        // Derived size
        public override Vector2 Size
        {
            get { return Main.fontMouseText.MeasureString(Label); }
        }

        ///ISocketedButton
        public ButtonSocket<TextButton> ButtonBase
        {
            get { return _parent; }
            protected set { _parent = value; }
        }

        // public override void CopyAttributes(CoreButton other)
        // {
        //     base.CopyAttributes(other);
        //     if (other is TextButton)
        //     {
        //         TextButton ob = (TextButton)other;
        //     }
        // }

        public TextButton() : base()
        {
            Label = "Uninitialized";
        }

        public TextButton(TIH action, string label = "") : base(action, label)
        {

        }
    }

    /// intended to use in a fluent-interface type of way;
    /// these are generic so that a separate class so that the proper
    /// subtype will be returned rather than a generic CoreButton
    public static class CBExtensions
    {
        ///<summary>
        /// Add a ButtonService to this button and subscribe to its hooks
        ///</summary>
        public static T AddService<T>(this T button, ButtonService service) where T: CoreButton
        {
            button.addService(service);
            return button;
        }

        /// <summary>
        /// use this to help with creating buttons; e.g.:
        /// </summary>
        /// <example>
        /// <code>
        /// 	TexturedButton cb = new TexturedButton(TIH.Sort).With( (b) => {
        ///          b.Hooks.onClick = () => IHOrganizer.SortPlayerInv(Main.localPlayer);
        ///          b.ToolTip = "Sort Me";
        ///          // ... etc.
        ///    })
        /// </code>
        ///</example>
        public static T With<T>(this T button, Action<T> action) where T: CoreButton
        {
            if (button != null)
                action(button);
            return button;
        }

    }

}
