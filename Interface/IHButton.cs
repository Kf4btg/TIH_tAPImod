using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
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
                return (displayState.texture!=null) ? displayState.texture.Size() : Main.fontMouseText.MeasureString(displayState.label);
            }
        }

        public IHButton(Vector2? pos=null)
        {
            this.displayState = new ButtonState();
            this.pos = pos ?? default(Vector2);
        }

        //simple button w/ no right-click
        public IHButton(string name, Texture2D tex, Action onClick, Vector2? pos=null, Color? tintColor = null)
        {
            this.Name = name;
            this.displayState = new ButtonState(name, tex, onClick, null, tintColor ?? Color.White);
            this.pos = pos ?? default(Vector2);
        }

        public IHButton(string name, Texture2D tex, Action onClick, Action onRightClick=null, Vector2? pos=null, Color? tintColor = null)
        {
            this.Name = name;
            this.displayState = new ButtonState(name, tex, onClick, onRightClick, tintColor ?? Color.White);
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

        public virtual bool OnDraw(SpriteBatch sb, Vector2 position)
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
                ActiveState = new ButtonState(activeLabel, tex, () => {}, DoToggle );
                InactiveState = new ButtonState(inactiveLabel, tex, () => {}, DoToggle, Color.Gray );
            } else {
                ActiveState = new ButtonState(activeLabel, tex, DoToggle );
                InactiveState = new ButtonState(inactiveLabel, tex, DoToggle, null, Color.Gray );
            }
            displayState = ActiveState;
        }

        public IHToggle(ButtonState activeState, ButtonState inactiveState, Func<bool> isActive, Vector2? pos = null, bool toggleOnRightClick = false) : base(pos)
        {
            IsActive = isActive;

            if (toggleOnRightClick){
                makeActive   = inactiveState.onRightClick;
                makeInactive = activeState.onRightClick;

                activeState.onRightClick   = DoSwitch;
                inactiveState.onRightClick = DoSwitch;
            } else {
                makeActive   = inactiveState.onClick;
                makeInactive = activeState.onClick;

                activeState.onClick   = DoSwitch;
                inactiveState.onClick = DoSwitch;
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
        public void Init()
        {
            displayState = IsActive() ? ActiveState : InactiveState;
        }
    }

    // a button that dynamically changes its appearance and/or function based on an external condition (as opposed to changing on user-click like IHToggle)
    public class IHContextButton : IHButton
    {
        private readonly ButtonState defaultState;
        private readonly ButtonState altState;
        // public String CurrentState { get { return displayState.label; } }

        private readonly KeyWatcher[] keySwitch; //well this is cheesy

        public IHContextButton(ButtonState defaultState, ButtonState altState, KState.Special? watchedKey=null, Vector2? pos=null) :
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
    public class ButtonState
    {
        public string label;
        public Texture2D texture;
        public Action onClick;
        public Action onRightClick;
        public Color tint;      //How to tint the texture when this state is active

        public ButtonState()
        {
            label = "Button";
            texture = null;
            onClick = null;
            onRightClick = null;
            tint = Color.White;
        }

        public ButtonState(string label, Texture2D tex, Action onClick, Action onRightClick=null, Color? tintColor = null)
        {
            this.label = label;
            this.texture = tex;
            this.onClick = onClick;
            this.onRightClick = onRightClick;
            this.tint = tintColor ?? Color.White;
        }
    }

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
