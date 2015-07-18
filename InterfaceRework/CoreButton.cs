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
        // fields
        protected TIH action;
        protected string label = "";
        protected string tooltip = "";
        protected Color tint;

        // field-access properties
        public TIH Action     { get { return action; }  set { action  = value; } }
        public string Label   { get { return label; }   set { label   = value; } }
        public string Tooltip { get { return tooltip; } set { tooltip = value; } }

        /// can change the overall color of the button texture or text
        public Color Tint     { get { return tint; }    set { tint    = value; } }

        /// Unique (well, effectively...), randomly-generated ID
        public string ID { get; protected set; }

        // hook container
        public ButtonHooks Hooks;
        // plugins
        protected List<ButtonService> services;
        /// hooks requested by services
        protected Dictionary<String, List<ButtonService>> enabledHooks;

        // Derived size
        public abstract Vector2 Size { get; }

        /// blank button not intended for use other than by services
        public CoreButton()
        {
            Hooks = new ButtonHooks();
            services = new List<ButtonService>();

            // set randomly-generated unique ID
            ID = UICore.GenerateHoverID();
        }

        // Constructors
        public CoreButton(TIH action, string label = "")
        {
            this.Action = action;
            if (label == "") Label = Constants.DefaultButtonLabels[action];
            else this.Label = label;
            Tooltip = Label;

            Hooks = new ButtonHooks();
            services = new List<ButtonService>();

            // set randomly-generated unique ID
            ID = UICore.GenerateHoverID();
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
            this.Action = other.Action;
            this.Label = other.Label;
            this.Tooltip = other.Tooltip;
            this.Tint = other.Tint;
        }


        #region hooks
        // //////////////////////////////////////////////////////////
        // Handle hooks attached directly to this button and also
        // those of subscribed services.
        // //////////////////////////////////////////////////////////

        public virtual void OnWorldLoad()
        {
            if (Hooks.onWorldLoad != null) Hooks.onWorldLoad();
            CallServiceHooks("onWorldLoad");
        }

        public virtual void OnClick()
        {
            if (Hooks.onClick != null) Hooks.onClick();
            CallServiceHooks("onClick");
        }

        public virtual void OnRightClick()
        {
            if (Hooks.onRightClick != null) Hooks.onRightClick();
            CallServiceHooks("onRightClick");
        }

        /// <returns> whether to continue with the base's OnMouseEnter</returns>
        public virtual bool OnMouseEnter()
        {
            bool result = true;
            if (Hooks.onMouseEnter!=null) result = Hooks.onMouseEnter() & result;
            return (CallServiceHooks("onMouseEnter") & result);
        }

        /// <returns> whether to continue with the base's OnMouseLeave</returns>
        public virtual bool OnMouseLeave()
        {
            bool result = true;
            if (Hooks.onMouseLeave!=null) result = Hooks.onMouseLeave() & result;
            return (CallServiceHooks("onMouseLeave") & result);
        }

        // bypass the dynamic calls for pre/post-draw for performance gains

        ///<returns>False if any hooks returned false, indicating
        /// not to continue with the rest of the Button Draw() Command
        /// and immediately skip to the PostDraw hook.
        public virtual bool PreDraw(SpriteBatch sb)
        {
            bool result = true;
            if (Hooks.preDraw!=null) result = Hooks.preDraw(sb) & result;
            // return CallServiceHooks("preDraw");
            List<ButtonService> list;
            if (enabledHooks.TryGetValue("preDraw", out list))
            {
                foreach ( var service in list )
                    // any false return will lock result to false
                    result = service.Hooks.preDraw(sb) & result;
            }
            return result;
        }

        public virtual void PostDraw(SpriteBatch sb)
        {
            if (Hooks.postDraw!=null) Hooks.postDraw(sb);
            // CallServiceHooks("postdraw", sb);
            List<ButtonService> list;
            if (enabledHooks.TryGetValue("postDraw", out list))
            {
                foreach ( var service in list )
                    service.Hooks.postDraw(sb);
            }
        }

        ///<param name="hook">String name of the hook being called, e.g. "onClick"</param>
        ///<param name="sb">SpriteBatch object for Hooks which use it</param>
        ///<param name="isFunction">whether the hook being called is a Func (otherwise it's an Action)</param>
        ///<returns>Result of Function call if hook is Func, otherwise true</returns>
        protected bool CallServiceHooks(string hook, SpriteBatch sb = null, bool isFunc = false )
        {
            List<ButtonService> list;
            if (!enabledHooks.TryGetValue(hook, out list)) return true;

            bool result = true;
            if (isFunc)
                if (sb == null) // Func<bool>
                    foreach (var service in list)
                        result = service.Hooks[hook].Invoke() & result;
                else // Func<sb, bool>
                    foreach (var service in list)
                        result = service.Hooks[hook].Invoke(sb) & result;
            else
                if (sb == null) //Action
                    foreach (var service in list)
                        service.Hooks[hook].Invoke();
                else  //Action<sb>
                    foreach (var service in list)
                        service.Hooks[hook].Invoke(sb);

            return result;
        }
        #endregion

        #region serviceManagement

        internal void addService(ButtonService bs)
        {
            services.Add(bs);
            bs.Subscribe();
        }

        // TODO: implement this (maybe)
        public void RemoveService(string serviceType) {}

        /// <summary>
        /// RegisterServiceHook
        /// </summary>
        /// <param name="service"></param>
        /// <param name="hookName"></param>
        public void RegisterServiceHook(ButtonService service, string hookName)
        {
            // if the entry already exists, Add will throw an ArgumentException
            // which we can catch and add the service to the existing list instead
            try
            {
                enabledHooks.Add(hookName, new List<ButtonService>() { service });
            }
            catch (ArgumentException)
            {
                enabledHooks[hookName].Add(service);
            }
        }

        /// <summary>
        /// RemoveServiceHook
        /// </summary>
        /// <param name="service"></param>
        /// <param name="hookName"></param>
        public void RemoveServiceHook(ButtonService service, string hookName)
        {
            enabledHooks[hookName].Remove(service);
            if (enabledHooks[hookName].Count == 0)
                enabledHooks.Remove(hookName);
        }

        #endregion


    }


    // //////////////////////////////////////////
    /// contains the functionality of a button
    // //////////////////////////////////////////
    public class ButtonHooks : DynamicObject
    {
        public Action onClick;
        public Action onRightClick;

        public Func<bool> onMouseEnter;
        public Func<bool> onMouseLeave;

        public Func<SpriteBatch, bool> preDraw;
        public Action<SpriteBatch> postDraw;

        public Action onWorldLoad;

        /// Map of hook names to dynamic option holding the actual
        /// hook Action/Function; will throw runtime errors if
        /// the hook is not invoked with correct parameter types
        private Dictionary<string, dynamic> hooks;
        public Dictionary<string, dynamic> AllHooks { get { return hooks;}}


        // Dictionary<string, object> dhooks
        // = new Dictionary<string, object>();

        public int Count
        {
            get { return hooks.Count; }
        }

        // public override bool TryGetMember(
        // GetMemberBinder binder, out object result)
        // {
        //     return hooks.TryGetValue(binder.Name, out result);
        // }
        //
        // public override bool TrySetMember(
        // SetMemberBinder binder, object value)
        // {
        //     hooks[binder.Name] = value;
        //     return true;
        // }
        //
        // public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        // {
        //     Type dictType = typeof(Dictionary<string, object>);
        //     try
        //     {
        //         result = dictType.InvokeMember(
        //                      binder.Name,
        //                      BindingFlags.InvokeMethod,
        //                      null, hooks, args);
        //         return true;
        //     }
        //     catch
        //     {
        //         result = null;
        //         return false;
        //     }
        // }

        // allows accessing the hooks like: Hooks["onClick"]
        // FIXME: this is not goodstuff for setting the value;
        public dynamic this[string hookName]
        {
            get { return hooks[hookName]; }
            set { hooks[hookName] = value; }
        }

        public ButtonHooks()
        {
            hooks = new Dictionary<string, dynamic>
            {
                {"onClick", onClick},
                {"onRightClick", onRightClick},
                {"onWorldLoad", onWorldLoad},
                {"onMouseEnter", onMouseEnter},
                {"onMouseLeave", onMouseLeave},
                {"preDraw", preDraw},
                {"postDraw", postDraw}
            };
        }

        public IEnumerator<KeyValuePair<string, dynamic>> GetEnumerator()
        {
            return hooks.GetEnumerator();
        }

        public bool isAssigned(string hookName)
        {
            return hooks[hookName] != null;
        }
    }

    // ///////////////////////////////////////////////
    /// Icon Button with a texture and ability to vary
    /// its appearance when hovered with the mouse.
    // ///////////////////////////////////////////////
    public class TexturedButton : CoreButton, ISocketedButton<TexturedButton>
    {
        protected Texture2D texture;
        protected Rectangle? defaultTexels;
        protected Rectangle? altTexels;

        protected Color bgColor;

        public Texture2D Texture       { get { return texture; }       set { texture       = value; } }
        public Rectangle? InactiveRect { get { return defaultTexels; } set { defaultTexels = value; } }
        public Rectangle? ActiveRect   { get { return altTexels; }     set { altTexels     = value; } }

        public Color BgColor           { get { return bgColor; }       set { bgColor       = value; } }

        public override Vector2 Size
        {
            get {
                return InactiveRect.HasValue ? InactiveRect.Value.Size() : Texture.Size();
            }
        }

        private ButtonSocket<TexturedButton> parent;
        public ButtonSocket<TexturedButton> ButtonBase
        {
            get { return parent; }
            protected set { parent = value; }
        }


        public TexturedButton() : base()
        {
            Texture = IHBase.ButtonBG;
            BgColor = Color.White;

        }

        public TexturedButton(TIH action,
                              Color bgColor,
                              Texture2D texture=null,
                              Rectangle? defaultTexels = null,
                              Rectangle? focusedTexels = null,
                              string label = "") : base(action, label)
        {
            BgColor = bgColor;
            Texture = (texture==null) ? IHBase.ButtonGrid : texture;

            InactiveRect = defaultTexels.HasValue ? defaultTexels : IHUtils.GetSourceRect(action);
            ActiveRect = altTexels.HasValue ? altTexels : IHUtils.GetSourceRect(action, true);

        }

        public override void CopyAttributes(CoreButton other)
        {
            base.CopyAttributes(other);
            if (other is TexturedButton)
            {
                TexturedButton ob = (TexturedButton)other;
                this.Texture = ob.Texture;
                this.InactiveRect = ob.InactiveRect;
                this.ActiveRect = ob.ActiveRect;
                this.BgColor = ob.BgColor;

            }
        }

    }



    // ////////////////////////////////////////////////////////////////////////////
    /// Text-only button in the vein of those directly beside the chest in Vanilla
    // ////////////////////////////////////////////////////////////////////////////
    public class TextButton : CoreButton, ISocketedButton<TextButton>
    {

        // Derived size
        public override Vector2 Size
        {
            get { return Main.fontMouseText.MeasureString(Label); }
        }

        private ButtonSocket<TextButton> parent;
        public ButtonSocket<TextButton> ButtonBase
        {
            get { return parent; }
            protected set { parent = value; }
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
        public static T AddService<T>(this T button, ButtonService bs) where T: CoreButton
        {
            button.addService(bs);
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
