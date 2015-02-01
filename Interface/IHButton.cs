using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    /**
    * These are the "Contexts" in the ButtonBase Class. They define how a button interacts with
    * ButtonStates (which contain the actual user-facing details of the button like
    * texture, name, and click-action).
    * IDEA: Consider removing the derived states (Toggle/Dynamic) and add methods like
    * e.g. IHButton.MakeToggle() to add that feature to a base IHButton.  This will,
    * I think, remove some of the confusing complexity of the state->context->base hierarchy.
    */
    public class IHButton
    {
        public readonly string Name;

        public Vector2 pos;
        public bool isHovered;

        public ButtonState displayState { get; protected set; }
        //these for backwards-compat (Temporary?)
        public Action onClick       { get { return displayState.onClick;}       protected set { displayState.onClick=value; } }
        public Action onRightClick  { get { return displayState.onRightClick;}  protected set { displayState.onRightClick=value; } }
        public string displayLabel  { get { return displayState.label;}         protected set { displayState.label=value; } }
        public Texture2D texture    { get { return displayState.texture;}       protected set { displayState.texture=value; } }
        public Color tint           { get { return displayState.tint;}          protected set { displayState.tint=value; } }

        public Vector2 Size
        {
            get {
                return (displayState.texture!=null) ?
                (displayState.sourceRect.HasValue ? displayState.sourceRect.Value.Size() : displayState.texture.Size()) :
                Main.fontMouseText.MeasureString(displayState.label);
            }
        }

        public IHButton(Vector2? pos=null)
        {
            this.displayState = new ButtonState();
            this.pos = pos ?? default(Vector2);
        }

        //simple button w/ no right-click
        public IHButton(string name, Texture2D tex, Action onClick, Rectangle? source, Vector2? pos=null, Color? tintColor = null)
        {
            this.Name = name;
            this.displayState = new ButtonState(name, tex, source, onClick, null, tintColor ?? Color.White);
            this.pos = pos ?? default(Vector2);
        }
        //with right click
        public IHButton(string name, Texture2D tex, Action onClick, Rectangle? source, Action onRightClick=null, Vector2? pos=null, Color? tintColor = null)
        {
            this.Name = name;
            this.displayState = new ButtonState(name, tex, source, onClick, onRightClick, tintColor ?? Color.White);
            this.pos = pos ?? default(Vector2);
        }

        public IHButton(ButtonState bState, Vector2? pos=null)
        {
            this.Name = bState.label;
            this.displayState = bState;
            this.pos = pos ?? default(Vector2);
        }

        protected virtual void SetState(ButtonState newState)
        {
            displayState = newState;
        }

        public virtual bool OnMouseEnter(ButtonBase bBase)
        {
            if (displayState.onMouseEnter!=null) return displayState.onMouseEnter(bBase);
            return true;
        }

        public virtual bool OnMouseLeave(ButtonBase bBase)
        {
            if (displayState.onMouseLeave!=null) return displayState.onMouseLeave(bBase);
            return true;
        }

        //this is just here to enable the IHToggle Update(init) function from ModWorld; better solution later.
        public virtual void OnUpdate()
        {       }

        public virtual void Draw(SpriteBatch sb)
        {
            if (displayState.texture==null)
                sb.DrawString(Main.fontMouseText, displayState.label, pos, displayState.tint);
            else
                sb.Draw(displayState.texture, pos, displayState.tint);

            if (IHButton.Hovered(this))
            {
                if (!isHovered)
                {
                    Main.PlaySound(12, -1, -1, 1); // "mouse-over" sound
                    isHovered = true;
                }

                Main.localPlayer.mouseInterface = true;
                if (Main.mouseLeft && Main.mouseLeftRelease) onClick();
                if (Main.mouseRight && Main.mouseRightRelease && onRightClick!=null) onRightClick();
            }
            else isHovered = false;
        }

        public virtual bool OnDraw(SpriteBatch sb, ButtonBase bBase)
        {
            return true;
        }

        public static bool Hovered(IHButton b)
        {
            return (new Rectangle((int)b.pos.X, (int)b.pos.Y, (int)b.Size.X, (int)b.Size.Y).Contains(Main.mouseX, Main.mouseY));
        }
    }

    // This is basically an "On/Off" switch - has an active and an inactive state, and onClick toggles between them.
    // The First constructor is a literal toggle, performing one action that causes IsActive to swap from true to false or vice-versa and simply graying out the button
    // The second constructor allows for more flexibility in defining what actions are taken when switching from one state to another and how the button display changes.
    public class IHToggle : IHButton
    {
        public readonly ButtonState ActiveState;
        public readonly ButtonState InactiveState;

        public readonly Func<bool> IsActive;
        public readonly Action OnToggle;

        private readonly Action makeActive;
        private readonly Action makeInactive;

        //defaultish - gray out the inactive state, change the label
        public IHToggle(string activeLabel, string inactiveLabel, Texture2D tex, Func<bool> isActive, Action onToggle, Vector2? pos=null, bool toggleOnRightClick = false) : base(pos)
        {
            IsActive = isActive;
            OnToggle = onToggle;

            if (toggleOnRightClick){
                ActiveState   = new ButtonState(activeLabel,   tex, null, () => {}, DoToggle );
                InactiveState = new ButtonState(inactiveLabel, tex, null, () => {}, DoToggle, Color.Gray );
            } else {
                ActiveState   = new ButtonState(activeLabel,   tex, null, DoToggle );
                InactiveState = new ButtonState(inactiveLabel, tex, null, DoToggle, null, Color.Gray );
            }
            displayState = ActiveState;
        }

        public IHToggle(ButtonState activeState, ButtonState inactiveState, Func<bool> isActive, Vector2? pos = null, bool toggleOnRightClick = false) : base(pos)
        {
            IsActive = isActive;

            if (toggleOnRightClick){
                makeActive                 = inactiveState.onRightClick;
                makeInactive               = activeState.onRightClick;

                activeState.onRightClick   = DoSwitch;
                inactiveState.onRightClick = DoSwitch;
            } else {
                makeActive                 = inactiveState.onClick;
                makeInactive               = activeState.onClick;

                activeState.onClick        = DoSwitch;
                inactiveState.onClick      = DoSwitch;
            }

            ActiveState   = activeState;
            InactiveState = inactiveState;

            displayState = ActiveState; //just to make sure nothing is null
        }

        public void DoToggle()
        {
            OnToggle();
            SetState(IsActive() ? ActiveState : InactiveState);
        }

        //rather than just doing something like: activeState.onClick = setState(inActiveState)
        //I prefer to have it actually check the state of the game parameter it affects,
        //in order to avoid issues from race-conditions or some possible other action
        // that changed said paramater.
        public void DoSwitch()
        {
            if (IsActive())
            {
                makeInactive();
                SetState(InactiveState);
            } else
            {
                makeActive();
                SetState(ActiveState);
            }
        }

        // call from ModWorld
        public override void OnUpdate()
        {
            SetState(IsActive() ? ActiveState : InactiveState);
        }
    }

    // a button that dynamically changes its appearance and/or function based on an external condition (as opposed to changing on user-click like IHToggle)
    public class IHDynamicButton : IHButton
    {
        private readonly ButtonState defaultState;
        private readonly ButtonState altState;
        // public String CurrentState { get { return displayState.label; } }

        private readonly KeyWatcher[] keySwitch; //well this is cheesy

        public IHDynamicButton(ButtonState defaultState, ButtonState altState, KState.Special? watchedKey=null, Vector2? pos=null) :
        base(defaultState, pos)
        {
            this.defaultState = defaultState;
            this.altState     = altState;

            this.displayState = defaultState;

            if (watchedKey!=null)
            {
                KState.Special watchKey = watchedKey.Value;
                keySwitch = new KeyWatcher[2];

                // in default, going to alt
                keySwitch[1] = new KeyWatcher(watchKey, KeyEventProvider.Event.Pressed,  () => {
                    SetState(altState);
                    // keySwitch[1].Unsubscribe(); //this happens automatically with a concurrent bag
                    keySwitch[0].Subscribe();
                    });

                // in alt, going to default
                keySwitch[0] = new KeyWatcher(watchKey, KeyEventProvider.Event.Released, () => {
                    SetState(defaultState);
                    // keySwitch[0].Unsubscribe();
                    keySwitch[1].Subscribe();
                    });

                keySwitch[1].Subscribe(); //initialize
            }
        }

        // protected KeyWatcher RegisterKeyWatcher(String bindState, KState.Special watchKey)
        // {
        //     // return new KeyWatcher(watchKey, KeyEventProvider.Event.Pressed,  () => { ChangeState(bindState); watchedKeys["Default"].Unsubscribe();} ));
        //     return new KeyWatcher(watchKey, KeyEventProvider.Event.Pressed,  () => { ChangeState(bindState); keySwitch[0].Unsubscribe();} ));
        //
        //     // KeyWatcher kw2 = new KeyWatcher(watchKey, KeyEventProvider.Event.Released, () => ChangeState("Default"));
        //
        // }
    }

    // this class will allow multiple dynamically-activated states...if I ever implement it.
/*
    public class IHMultiContext : IHButton
    {
        private readonly Dictionary<string, ButtonState> States;
        private Dictionary<String, KeyWatcher> watchedKeys;

        public IHContextButton(ButtonState defaultState, Vector2? pos=null) : base(defaultState, pos) {
            States = new Dictionary<string, ButtonState>();
            States.Add("Default", defaultState);       }

        public void AddState(Keys watchedKey, ButtonState state) {       }

        public void AddState(KState.Special watchedKey, ButtonState state) {        }

        protected void SetState(String stateName)
        {
            if (States.ContainsKey(stateName))
                SetState(States[stateName]);
        }

    }

*/
    

    // public class ButtonContext
    // {
    //     public String stateName;
    //     public Func<bool> isCurrent;
    //     public Tuple<KState.Special, KeyEventProvider.Event> watchKey;
    //
    //
    //     public ButtonContext(String sn, Func<bool> ic, Tuple<KState.Special, KeyEventProvider.Event> wk=null)
    //     {
    //         stateName = sn;
    //         isCurrent = ic;
    //         watchKey = wk;
    //     }
    // }



}
