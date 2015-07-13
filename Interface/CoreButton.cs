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
        // holds the button
        public readonly ButtonRebase container;

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

        #region hooks
        public virtual bool OnMouseEnter(ButtonBase bBase)
        {
            return Hooks.onMouseEnter==null || Hooks.onMouseEnter(bBase);
        }

        public virtual bool OnMouseLeave(ButtonBase bBase)
        {
            return Hooks.onMouseLeave==null || Hooks.onMouseLeave(bBase);
        }

        public virtual bool PreDraw(SpriteBatch sb, ButtonBase bBase)
        {
            return Hooks.preDraw==null || Hooks.preDraw(sb, bBase);
        }

        public virtual void PostDraw(SpriteBatch sb, ButtonBase bBase)
        {
            List<ButtonService> list;
            if (enabledHooks.TryGetValue("postDraw", out list))
            {
                foreach ( var service in list )
                    service.Hooks.postDraw(sb, bBase);
            }

            if (Hooks.postDraw!=null) Hooks.postDraw(sb, bBase);
        }
        #endregion

        public void RegisterHook(ButtonService service, string hookName)
        {
            enabledHooks[hookName].Add(service);
        }
        public void RemoveHook(ButtonService service, string hookName)
        {
            enabledHooks[hookName].Remove(service);
        }

        /// contains the functionality of the button
        public class ButtonHooks
        {
            public Action onClick;
            public Action onRightClick;

            public Func<ButtonBase,bool> onMouseEnter;
            public Func<ButtonBase,bool> onMouseLeave;

            public Func<SpriteBatch, ButtonBase, bool> preDraw;
            public Action<SpriteBatch, ButtonBase> postDraw;

            public Action onWorldLoad;
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
