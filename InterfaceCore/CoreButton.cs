using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
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

    public class ButtonHooks
    {
        // public enum On
        // {
        //     Click,
        //     RightClick,
        //     MouseEnter,
        //     MouseLeave,
        //     PreDraw,
        //     PostDraw,
        //     WorldLoad
        // }

        public IHEvent<Action> OnClick, OnRightClick, OnWorldLoad;
        public IHEvent<Action<SpriteBatch>> PostDraw;
        public IHEvent<Func<bool>> OnMouseEnter, OnMouseLeave;
        public IHEvent<Func<SpriteBatch, bool>> PreDraw;

        public ButtonHooks()
        {
            OnClick      = new IHEvent<Action>();
            OnRightClick = new IHEvent<Action>();
            OnWorldLoad  = new IHEvent<Action>();
            PostDraw     = new IHEvent<Action<SpriteBatch>>();
            OnMouseEnter = new IHEvent<Func<bool>>();
            OnMouseLeave = new IHEvent<Func<bool>>();
            PreDraw      = new IHEvent<Func<SpriteBatch, bool>>();
        }
    }
}
