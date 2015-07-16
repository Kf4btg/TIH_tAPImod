using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;
using System.Dynamic;

namespace InvisibleHand
{
    /// re-imagining the ButtonState and IHButton as one object
    public abstract class CoreButton
    {
        // holds the button
        public readonly ButtonRebase Base;

        // fields
        protected TIH action;
        protected string label;
        protected string tooltip;
        // protected Vector2 position; // position determined by base

        protected Color tint;

        // field-access properties
        public TIH Action              { get { return action; }        set { action        = value; } }
        public string Label            { get { return label; }         set { label         = value; } }
        public string Tooltip          { get { return tooltip; }       set { tooltip       = value; } }
        public Color Tint              { get { return tint; }          set { tint          = value; } }

        // public Vector2 Position { get { return position; }
        //     set { position = value == null ? default(Vector2) : value; } }

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
        public CoreButton(TIH action, Vector2 position, string label = "")
        {
            this.Action = action;
            if (label == "")
                label = Constants.DefaultButtonLabels[action];
            this.Label = label;
            // this.Position = position;
            this.Hooks = new ButtonHooks();
            this.services = new List<ButtonService>();
        }

        /// use this to help with creating buttons; e.g.:
        /// CoreButton cb = new CoreButton(TIH.Sort).With(delegate(CoreButton b) {
        ///          b.Hooks.onClick = () => IHOrganizer.SortPlayerInv(Main.localPlayer);
        ///          b.ToolTip = "Sort Me";
        ///          // ... etc.
        ///    })
        public CoreButton With(Action<CoreButton> action)
        {
            if (this != null)
            {
                action(this);
            }
            return this;
        }

        #region hooks
        public virtual void OnClick()
        {
            if (this.Hooks.onClick != null) this.Hooks.onClick();
            List<ButtonService> list;
            if (enabledHooks.TryGetValue("onClick", out list))
            {
                foreach ( var service in list )
                    service.Hooks.onClick();
            }
        }

        public virtual void OnRightClick()
        {
            if (this.Hooks.onRightClick != null) this.Hooks.onRightClick();
            List<ButtonService> list;
            if (enabledHooks.TryGetValue("onRightClick", out list))
            {
                foreach ( var service in list )
                    service.Hooks.onRightClick();
            }
        }

        public virtual bool OnMouseEnter()
        {
            List<ButtonService> list;
            bool result = true;
            if (enabledHooks.TryGetValue("onMouseEnter", out list))
            {
                foreach ( var service in list )
                    // any false return will lock result to false
                    result = service.Hooks.onMouseEnter() & result;
            }
            return result;
        }

        public virtual bool OnMouseLeave()
        {
            List<ButtonService> list;
            bool result = true;
            if (enabledHooks.TryGetValue("onMouseLeave", out list))
            {
                foreach ( var service in list )
                    // any false return will lock result to false
                    result = service.Hooks.onMouseLeave() & result;
            }
            return result;
        }

        public virtual bool PreDraw(SpriteBatch sb)
        {
            List<ButtonService> list;
            bool result = true;
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
            List<ButtonService> list;
            if (enabledHooks.TryGetValue("postDraw", out list))
            {
                foreach ( var service in list )
                    service.Hooks.postDraw(sb);
            }
        }


        public bool callhook(string hook, SpriteBatch sb = null, bool isFunction = false )
        {
            List<ButtonService> list;
            if (!enabledHooks.TryGetValue(hook, out list)) return true;

            if (isFunction)
            {
                bool result = true;
                if (sb == null)
                {
                    // Func<bool>
                    foreach (var service in list)
                        result = service.Hooks.GetHook(hook).Invoke() & result;
                }
                else
                {
                    // Func<sb, bool>
                    foreach (var service in list)
                        result = service.Hooks.GetHook(hook).Invoke(sb) & result;
                }
                return result;
            }
            if (sb == null) //Action
                foreach (var service in list)
                    service.Hooks.GetHook(hook).Invoke();
            else  //Action<sb>
                foreach (var service in list)
                    service.Hooks.GetHook(hook).Invoke(sb);

            return true;
        }


        #endregion

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

        /// contains the functionality of the button
        public class ButtonHooks
        {
            public Action onClick;
            public Action onRightClick;

            public Func<bool> onMouseEnter;
            public Func<bool> onMouseLeave;

            public Func<SpriteBatch, bool> preDraw;
            public Action<SpriteBatch> postDraw;

            public Action onWorldLoad;

            public dynamic GetHook(string hookname)
            {
                return hooks[hookname];
            }

            private Dictionary<string, dynamic> hooks;



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

            public bool callhook(bool isFunction )
            {
                return true;
            }

        }
    }

    // public class LockingButton<T> where T: CoreButton
    // {
    //     protected Color lockColor;
    //     // protected Vector2 lockOffset;
    //
    //     public Color LockColor { get { return lockColor; } set { lockColor = value; } }
    //     public Vector2 LockOffset { get; protected set; }
    //
    // }

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

        public TexturedButton(TIH action, Vector2 position, string label = "") : base(action, position, label)
        {

        }

    }

    public class TextButton : CoreButton
    {
        // Derived size
        public override Vector2 Size
        {
            get { return Main.fontMouseText.MeasureString(Label); }
        }

        public TextButton(TIH action, Vector2 position, string label = "") : base(action, position, label)
        {

        }
    }


}
