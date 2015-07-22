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
    public class TexturedButton : CoreButton, ISocketedButton<TexturedButton>
    {
        // store for implementing ISocketedButton
        private IButtonContentHandler<TexturedButton> _parent;

        public Texture2D Texture       { get; set; }
        public Rectangle? InactiveRect { get; set; }
        public Rectangle? ActiveRect   { get; set; }
        public Color BackgroundColor   { get; set; }

        public override Vector2 Size
        {
            get { return InactiveRect.HasValue ? InactiveRect.Value.Size() : Texture.Size(); }
        }

        /// Get ButtonSocket in which this button is placed
        public override IButtonSlot ButtonBase
        {
            get { return _parent; }
        }

        public IButtonContentHandler<TexturedButton> Socket
        {
            get { return _parent; }
            protected set { _parent = value; }
        }


        public TexturedButton(TIH action,
                              string label,
                              string tooltip           = "",
                              Color? bg_color          = null,
                              Texture2D texture        = null,
                              Rectangle? inactive_rect = null,
                              Rectangle? active_rect   = null
                              ) : base(action, label)
        {
            BackgroundColor = bg_color ?? Color.White;

            Texture      = (texture == null) ? IHBase.ButtonGrid : texture;
            InactiveRect = inactive_rect.HasValue ? inactive_rect : IHUtils.GetSourceRect(action);
            ActiveRect   = active_rect.HasValue ? active_rect : IHUtils.GetSourceRect(action, true);
        }


        public TexturedButton Duplicate()
        {
            return new TexturedButton(this.Action, this.Label, this.Tooltip, this.BackgroundColor, this.Texture, this.InactiveRect, this.ActiveRect);
        }

        public void Duplicate(out TexturedButton newTB)
        {
            newTB = this.Duplicate();
        }
    }



    // ////////////////////////////////////////////////////////////////////////////
    /// Text-only button in the vein of those directly beside the chest in Vanilla
    // ////////////////////////////////////////////////////////////////////////////
    public class TextButton : CoreButton
    {
        private IButtonContentHandler<TexturedButton> _parent;

        // Derived size
        public override Vector2 Size
        {
            get { return Main.fontMouseText.MeasureString(Label); }
        }

        ///ISocketedButton
        public override IButtonSlot ButtonBase
        {
            get { return _parent; }
        }

        public IButtonContentHandler<TexturedButton> Socket
        {
            get { return _parent; }
            protected set { _parent = value; }
        }

        public TextButton(TIH action, string label = "") : base(action, label)
        {
        }

        public void Duplicate(out TextButton newButton)
        {
            newButton = new TextButton(this.Action, this.Label);
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
        public static T AddNewService<T>(this T button, ButtonService service) where T : ICoreButton
        {
            button.AddService(service);
            return button;
        }

        public static T MakeLocking<T>(this T button, Vector2? lock_offset = null, Color? lock_color = null, string locked_string = "[Locked]") where T: CoreButton
        {
            return button.AddNewService(new LockingService(button, lock_offset, lock_color, locked_string));
        }

        public static T AddToggle<T>(this T button, T toggle_to_button, KState.Special toggle_key = KState.Special.Shift) where T: ISocketedButton<T>
        {
            return button.AddNewService(new ToggleService<T>(button, toggle_to_button, toggle_key));
        }

        /// <summary>
        /// use this to help with creating buttons; e.g.:
        /// </summary>
        /// <example>
        /// <code>
        ///     TexturedButton cb = new TexturedButton(TIH.Sort).With( (b) => {
        ///          b.Hooks.onClick = () => IHOrganizer.SortPlayerInv(Main.localPlayer);
        ///          b.ToolTip = "Sort Me";
        ///          // ... etc.
        ///    })
        /// </code>
        ///</example>
        public static T With<T>(this T button, Action<T> action) where T : ICoreButton
        {
            if (button != null)
                action(button);
            return button;
        }

        // public static T Duplicate<T>(this ISocketedButton<T> button) where T: CoreButton
        // {
        //     T newButton;
        //     button.Duplicate(out newButton);
        //     return newButton;
        // }
    }
}
