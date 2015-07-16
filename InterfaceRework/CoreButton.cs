using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;
using System.Dynamic;

namespace InvisibleHand
{
    // the typing on this is confusing as hell; see
    // http://blogs.msdn.com/b/ericlippert/archive/2011/02/03/curiouser-and-curiouser.aspx
    // for a discussion on it.
    /// re-imagining the ButtonState and IHButton as one object
    public abstract class CoreButton
    {
        // holds the button
        // public readonly ButtonRebase<T> Base;

        // fields
        protected TIH action;
        protected string label;
        protected string tooltip;
        protected Color tint;

        // field-access properties
        public TIH Action     { get { return action; }  set { action  = value; } }
        public string Label   { get { return label; }   set { label   = value; } }
        public string Tooltip { get { return tooltip; } set { tooltip = value; } }

        /// can change the overall color of the button texture or text
        public Color Tint     { get { return tint; }    set { tint    = value; } }

        // ID
        public string ID { get; protected set; }

        // hook container
        public ButtonHooks Hooks;
        // plugins
        protected List<ButtonService> services;
        /// hooks requested by services
        protected Dictionary<String, List<ButtonService>> enabledHooks;

        // Derived size
        public abstract Vector2 Size { get; }

        // Constructors
        public CoreButton(TIH action, string label = "")
        {
            this.Action = action;
            if (label == "") Label = Constants.DefaultButtonLabels[action];
            else this.Label = label;

            Hooks = new ButtonHooks();
            services = new List<ButtonService>();

            // set randomly-generated unique ID
            ID = UICore.GenerateHoverID();
        }



        #region hooks
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

        public virtual bool OnMouseEnter()
        {
            bool result = true;
            if (Hooks.onMouseEnter!=null) result = Hooks.onMouseEnter() & result;
            return (CallServiceHooks("onMouseEnter") & result);
        }

        public virtual bool OnMouseLeave()
        {
            bool result = true;
            if (Hooks.onMouseLeave!=null) result = Hooks.onMouseLeave() & result;
            return (CallServiceHooks("onMouseLeave") & result);
        }

        // bypass the dynamic calls for pre/post-draw for performance gains
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
        public void RemoveServiceHook(ButtonService service, string hookName)
        {
            enabledHooks[hookName].Remove(service);
            if (enabledHooks[hookName].Count == 0)
                enabledHooks.Remove(hookName);
        }

        #endregion


    }

    /// contains the functionality of a button
    public class ButtonHooks
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

        /// allows accessing the hooks like: Hooks["onClick"]
        public dynamic this[string hookName]
        {
            get { return hooks[hookName]; }
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

        public bool isAssigned(string hookName)
        {
            return hooks[hookName] != null;
        }
    }


    public class TexturedButton : CoreButton
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

        public TexturedButton(TIH action, Color bgColor, Texture2D texture=null, string label = "") : base(action, label)
        {
            BgColor = bgColor;
        }
    }

    public class TextButton : CoreButton
    {
        // Derived size
        public override Vector2 Size
        {
            get { return Main.fontMouseText.MeasureString(Label); }
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
        public static T AddService<T>(this T button, ButtonService bs) where T: CoreButton
        {
            button.addService(bs);
            return button;
        }

        /// use this to help with creating buttons; e.g.:
        /// CoreButton cb = new TexturedButton(TIH.Sort).With<TexturedButton>(delegate(TexturedButton b) {
        ///          b.Hooks.onClick = () => IHOrganizer.SortPlayerInv(Main.localPlayer);
        ///          b.ToolTip = "Sort Me";
        ///          // ... etc.
        ///    })
        public static T With<T>(this T button, Action<T> action) where T: CoreButton
        {
            if (button != null)
                action(button);
            return button;
        }

    }

}
