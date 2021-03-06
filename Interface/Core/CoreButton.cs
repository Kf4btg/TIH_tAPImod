using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    /// This interface describes the overall universal structure and
    /// capabilities of a button in this mod.  Most methods that deal
    /// with buttons (and don't care precisely how they're drawn),
    /// take this type as a parameter.
    public interface ICoreButton
    {
        IButtonSlot ButtonBase { get; }

        string ID { get; }

        TIH Action { get; set; }
        string Label { get; set; }
        string Tooltip { get; set; }
        bool ShowTooltip { get; set; }

        Color Tint { get; set; }
        Vector2 Size { get; }

        ButtonHooks Hooks { get; }
        Dictionary<string, ButtonService> Services { get; }

        void AddService(ButtonService bs);

        void OnWorldLoad();
        void OnClick();
        void OnRightClick();
        bool OnMouseEnter();
        bool OnMouseLeave();
        bool PreDraw(SpriteBatch sb);
        void PostDraw(SpriteBatch sb);
    }

    // re-imagining the ButtonState and IHButton as one object
    /// This is a convenience class which provides default
    /// implementations and some helper methods for classes
    /// implementing ICoreButton. Ideally anything that takes
    /// ICoreButton as a parameter type could equivalently
    /// accept this class instead.
    public abstract class CoreButton : ICoreButton
    {
        // fields //

        private string _label = "";
        private string _tooltip = "";
        private string _id = String.Empty;
        private bool   _showTooltip;

        //Properties//

        /// Get or change the overall color of the button texture or text
        public Color Tint     { get; set; }
        /// Get or set this button's associated action
        public TIH Action     { get; set; }
        /// Get or set this button's label
        public string Label   { get { return _label; }   set { _label   = value; } }

        /// Get or set this button's Tooltip
        public virtual string Tooltip
        {
            get { return _tooltip; }
            // set ShowTooltip = true when setting tooltip,
            // or false when unsetting it.
            set { _showTooltip = (value != ""); _tooltip = value; }
        }

        /// Whether the tooltip should be drawn on hover
        public virtual bool ShowTooltip
        {
            get { return _showTooltip; }
            // set to true iff Tooltip is not empty
            set { _showTooltip = (Tooltip == "") ? false : value; }
        }


        /// Unique (well, effectively...), randomly-generated ID
        public string ID
        {
            get; protected set;
        }

        /// Get hook container
        public ButtonHooks Hooks { get; protected set; }

        /// Subscribed Services
        public Dictionary<string, ButtonService> Services { get; protected set; }

        /// Derived size
        public abstract Vector2 Size { get; }

        public virtual IButtonSlot ButtonBase { get; protected set; }


        // Constructors
        protected CoreButton(IButtonSlot parent, TIH action, string label = "", Color? tint = null)
        {
            ButtonBase = parent;
            Action   = action;
            Label    = label;
            Hooks    = new ButtonHooks();
            Services = new Dictionary<string, ButtonService>();
            Tint = tint ?? Color.White;

            ID = UICore.GenerateHoverID();
        }

    #region hooks
        // //////////////////////////////////////////////////////////
        // Handle hook events attached to this Button's Hooks interface.
        // //////////////////////////////////////////////////////////

        /// <summary>
        /// Called once when the player enters a world; use to set initial state
        /// </summary>
        public virtual void OnWorldLoad()
        {
            foreach (var callHook in Hooks.OnWorldLoad)
                callHook();
        }

        /// <summary>
        /// Action(s) this button performs when clicked with the left mouse button.
        /// </summary>
        public virtual void OnClick()
        {
            foreach (var callHook in Hooks.OnClick)
                callHook();
        }

        /// <summary>
        /// Action(s) this button performs when clicked with the right mouse button.
        /// </summary>
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

        /// <summary>
        /// Called after the button has been drawn completely.
        /// </summary>
        /// <param name="sb">Drawing SpriteBatch</param>
        public virtual void PostDraw(SpriteBatch sb)
        {
            foreach (var callHook in Hooks.PostDraw)
                callHook(sb);
        }
        #endregion

    #region serviceManagement

        public void AddService(ButtonService bs)
        {
            //NOTE: should this worry about catching/avoiding
            //a "key already exists" ArgumentException?
            //I'd say under normal operation that shouldn't happen,
            //and would mean either A) a poorly-coded service,
            //or B) trying to add a redundant service to a button
            Services.Add(bs.ServiceType, bs);
            bs.Subscribe();
        }

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

    /// This class is like a substrate: an area on the CoreButton
    /// where outside vectors can bond events, thus changing the
    /// behavior of the button. It is a wrapper for all a
    /// button's possible events, and all functionality of a button
    /// will be added by registering event-handlers with one of the
    /// hooks contained within.
    public class ButtonHooks
    {
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
