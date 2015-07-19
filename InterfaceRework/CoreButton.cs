using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;
using System.Dynamic;
using System.Reflection;

namespace InvisibleHand
{
    /// Having subclasses of CoreButton ALSO implement this interface seems to be
    /// the ONLY way to get a reliably type-safe reference to the parent buttonbase
    public interface ISocketedButton<T> where T: CoreButton
    {
        ButtonSocket<T> ButtonBase { get; }
    }

    /// re-imagining the ButtonState and IHButton as one object
    public abstract class CoreButton
    {
        // fields //

        private string _label = "";
        private string _tooltip = "";

        private string _id = String.Empty;

        // protected TIH action;
        // protected Color tint;


        //Properties//

        // public TIH Action     { get { return action; }  set { action  = value; } }
        // public Color Tint     { get { return tint; }    set { tint    = value; } }

        /// can change the overall color of the button texture or text
        public Color Tint     { get; set; }
        public TIH Action     { get; set; }
        public string Label   { get { return _label; }   set { _label   = value; } }
        public string Tooltip { get { return _tooltip; } set { _tooltip = value; } }


        /// Unique (well, effectively...), randomly-generated ID
        public string ID
        {
            //generate on first access
            get { return _id == String.Empty ? UICore.GenerateHoverID() : _id; }
        }

        /// Get hook container
        public ButtonHooks Hooks { get; protected set; }

        /// Subscribed Services
        public Dictionary<string, ButtonService> Services { get; protected set; }

        /// hooks requested by services
        protected Dictionary<String, List<ButtonService>> enabledHooks { get; set; }

        /// Derived size
        public abstract Vector2 Size { get; }

        /// blank button not intended for use other than by services
        public CoreButton()
        {
            Hooks = new ButtonHooks();
            Services = new Dictionary<string, ButtonService>();
        }

        // Constructors
        public CoreButton(TIH action, string label = "") : this()
        {
            Action = action;
            if (label == "") Label = Constants.DefaultButtonLabels[action];
            else Label = label;
            Tooltip = Label;

        }

        /// create this button by copying all the aspects of the given button
        /// other than hooks and services
        // public CoreButton(CoreButton other, bool copyHooks = false, bool copyServices = false) : this(other.Action, other.Label)
        // {
        //     if (copyHooks)
        //         foreach (var h in other.Hooks)
        //         {
        //             if (h.Value != null) Hooks[h.Key] = h.Value;
        //         }
        // }

        /// copying all the aspects of the given button
        /// other than hooks, services, and ID
        public virtual void CopyAttributes(CoreButton other)
        {
            this.Action  = other.Action;
            this.Label   = other.Label;
            this.Tooltip = other.Tooltip;
            this.Tint    = other.Tint;
        }


    #region hooks
        // //////////////////////////////////////////////////////////
        // Handle hooks attached directly to this button and also
        // those of subscribed services.
        // //////////////////////////////////////////////////////////

        public virtual void OnWorldLoad()
        {
            foreach (var callHook in Hooks.OnWorldLoad)
                callHook();
        }

        public virtual void OnClick()
        {
            foreach (var callHook in Hooks.OnClick)
                callHook();
        }

        public virtual void OnRightClick()
        {
            foreach (var callHook in Hooks.OnRightClick)
                callHook();
        }

        /// <returns> whether to continue with the base's OnMouseEnter</returns>
        public virtual bool OnMouseEnter()
        {
            bool result = true;
            foreach (var callHook in Hooks.OnMouseEnter)
                result = callHook() & result;

            return result;
        }

        /// <returns> whether to continue with the base's OnMouseLeave</returns>
        public virtual bool OnMouseLeave()
        {
            bool result = true;
            foreach (var callHook in Hooks.OnMouseLeave)
                result = callHook() & result;

            return result;
        }

        ///<returns>False if any hooks returned false, indicating
        /// not to continue with the rest of the ButtonSocket's Draw() Command
        /// and immediately skip to the PostDraw hook.
        public virtual bool PreDraw(SpriteBatch sb)
        {
            bool result = true;
            foreach (var callHook in Hooks.PreDraw)
                // any false return will lock result to false
                result = callHook(sb) & result;

            return result;
        }

        public virtual void PostDraw(SpriteBatch sb)
        {
            foreach (var callHook in Hooks.PreDraw)
                callHook(sb);
        }
        #endregion

    #region serviceManagement

        internal void addService(ButtonService bs)
        {
            Services.Add(bs.ServiceType, bs);
            bs.Subscribe();
        }

        internal void RemoveService(string serviceType)
        {
            ButtonService bs;
            if (Services.TryGetValue(serviceType, out bs))
            {
                Services[serviceType].Unsubscribe();
                Services.Remove(serviceType);
            }
        }

        #endregion
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
                              Color bg_color,
                              Texture2D texture = null,
                              Rectangle? default_texels = null,
                              Rectangle? focused_texels = null,
                              string label = "") : base(action, label)
        {
            BackgroundColor = bg_color;
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
