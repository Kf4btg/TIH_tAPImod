using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public interface ICoreButton
    {
        IButtonSlot ButtonBase { get; }

        string ID { get; }

        TIH Action { get; set; }
        string Label { get; set; }
        string Tooltip { get; set; }
        Color Tint { get; set; }
        Vector2 Size { get; }

        ButtonHooks Hooks { get; }
        Dictionary<string, ButtonService> Services { get; }

        void OnWorldLoad();
        void OnClick();
        void OnRightClick();
        bool OnMouseEnter();
        bool OnMouseLeave();
        bool PreDraw(SpriteBatch sb);
        void PostDraw(SpriteBatch sb);

    }

    /// Having subclasses of CoreButton ALSO implement this interface seems to be
    /// the only way to get a reliably type-safe reference to the parent buttonbase
    public interface ISocketedButton<T> :ICoreButton where T: ISocketedButton<T>
    {
        IButtonContentHandler<T> Socket { get; }

        // void Duplicate(out ISocketedButton newCopy);
    }

    /// re-imagining the ButtonState and IHButton as one object
    public abstract class CoreButton : ICoreButton
    {
        // fields //

        private string _label = "";
        private string _tooltip = "";
        private string _id = String.Empty;

        //Properties//

        /// Get or change the overall color of the button texture or text
        public Color Tint     { get; set; }
        /// Get or set this button's associated action
        public TIH Action     { get; set; }
        /// Get or set this button's label
        public string Label   { get { return _label; }   set { _label   = value; } }
        /// Get or set this button's Tooltip
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

        public abstract IButtonSlot ButtonBase { get; }


        // Constructors
        protected CoreButton(TIH action, string label = "")
        {
            Action   = action;
            Label    = label;
            Hooks    = new ButtonHooks();
            Services = new Dictionary<string, ButtonService>();
        }

        /// copying all the aspects of the given button
        /// other than hooks, services, and ID
        ///<remarks>Probaby going to get rid of this</remarks>
        public virtual void CopyAttributes<T>(T other) where T:ICoreButton
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

        ///!
        /// <summary>
        /// Executed before any part of this button is drawn.
        /// </summary>
        /// <param name="sb">Drawing SpriteBatch</param>
        /// <returns>False if any hooks returned false, indicating
        /// not to continue with the rest of the ButtonSocket's Draw() Command
        /// and immediately skip to the PostDraw hook.</returns>
        public virtual bool PreDraw(SpriteBatch sb)
        {
            bool result = true;
            foreach (var callHook in Hooks.PreDraw)
                // any false return will lock result to false
                result = callHook(sb) & result;

            return result;
        }

        ///!
        /// <summary>
        /// Called after the button has been drawn completely.
        /// </summary>
        /// <param name="sb">Drawing SpriteBatch</param>
        public virtual void PostDraw(SpriteBatch sb)
        {
            foreach (var callHook in Hooks.PreDraw)
                callHook(sb);
        }
        #endregion

    #region serviceManagement

        internal void addService(ButtonService bs)
        {
            //NOTE: should this worry about catching/avoiding
            //a "key already exists" ArgumentException?
            //I'd say under normal operation that shouldn't happen,
            //and would mean either A) a poorly-coded service,
            //or B) trying to add a redundant service to a button
            Services.Add(bs.ServiceType, bs);
            bs.Subscribe();
        }

        //
        internal bool RemoveService(string serviceType)
        {
            ButtonService bs;
            if (Services.TryGetValue(serviceType, out bs))
            {
                Services[serviceType].Unsubscribe();
                Services.Remove(serviceType);
                return true;
            }
            return false;

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
