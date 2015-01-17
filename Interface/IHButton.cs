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
        public Action onClick       { get { return displayState.onClick;}   protected set { displayState.onClick=value; } }
        public string displayLabel  { get { return displayState.label;}     protected set { displayState.label=value; } }
        public Texture2D texture    { get { return displayState.texture;}   protected set { displayState.texture=value; } }
        public Color tint           { get { return displayState.tint;}      protected set { displayState.tint=value; } }


        public IHButton(string name, Texture2D tex, Action onClick, Vector2? pos=null, Color tintColor = Color.White)
        {
            this.Name = name;
            this.displayState = new ButtonState(name, tex, onClick, tintColor);
            this.pos = pos ?? default(Vector2);
        }

        public IHButton(ButtonState bState, Vector2? pos=null)
        {
            this.Name = bState.label;
            this.displayState = bState;
            this.pos = pos ?? default(Vector2);
        }

        public Vector2 Size
        {
            get {
                return (displayState.texture!=null) ? displayState.texture.Size() : Main.fontMouseText.MeasureString(displayState.label);
            }
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
            }
            else isHovered = false;
        }

        public static bool Hovered(IHButton b)
        {
            return (new Rectangle((int)b.pos.X, (int)b.pos.Y, (int)b.Size.X, (int)b.Size.Y).Contains(Main.mouseX, Main.mouseY));
        }
    }

    //, IHUpdateable
    // a button with 2 states: active and inactive. OnClick() toggles between the states
    public class IHToggle : IHButton
    {
        public readonly string activeLabel, inactiveLabel;
        public readonly Func<bool> IsActive;
        protected readonly Action setActive, setInactive;

        public Color stateColor = Color.White;

        public IHToggle(string activeLabel, string inactiveLabel, Texture2D tex, Func<bool> isActive, Action onToggle, Vector2? pos=null) :
            base(activeLabel, tex, null, pos)
        {
            this.activeLabel   = activeLabel;
            this.inactiveLabel = inactiveLabel;
            this.IsActive      = isActive;
            this.onClick       = () => Toggle(onToggle);

            //defaults if not specified
            this.setActive   = () => { stateColor = Color.White; displayLabel =   activeLabel; };
            this.setInactive = () => { stateColor =  Color.Gray; displayLabel = inactiveLabel; };
        }

        public IHToggle(string activeLabel, string inactiveLabel, Texture2D tex, Func<bool> isActive, Action onToggle, Action setActive, Action setInActive, Vector2? pos=null) :
        base(activeLabel, tex, null, pos)
        {
            this.activeLabel   = activeLabel;
            this.inactiveLabel = inactiveLabel;
            this.IsActive      = isActive;
            this.onClick       = () => Toggle(onToggle);
            this.setActive     = setActive;
            this.setInactive   = setInActive;
        }

        public void Update()
        {
            UpdateState(IsActive());
        }

        public void UpdateState(bool isActive)
        {
            if (isActive)
                setActive();
            else
                setInactive();
        }

        public void Toggle(Action onToggle)
        {
            onToggle();
            UpdateState(IsActive());
        }

        public override void Draw(SpriteBatch sb)
        {
            if (texture==null)
                sb.DrawString(Main.fontMouseText, displayLabel, pos, stateColor);
            else
                sb.Draw(texture, pos, stateColor);

            if (IHButton.Hovered(this))
            {
                if (!isHovered)
                {
                    Main.PlaySound(12, -1, -1, 1); // "mouse-over" sound
                    isHovered = true;
                }

                Main.localPlayer.mouseInterface = true;
                if (Main.mouseLeft && Main.mouseLeftRelease) onClick();
            }
            else isHovered = false;
        }

        public static bool GetState(IHToggle t)
        {
            return t.IsActive();
        }
    }

    /*this is what IHToggle should have *actually* been.  A toggle button doesn't have states. I suppose that what I really want is a button derived from the state-less toggle button or just IHButton that sets its appearance from isActive;
    */
    public class IHSwitch : IHButton
    {
        public readonly ButtonState ActiveState;
        public readonly ButtonState InactiveState;

        public readonly Func<bool> IsActive;
        public readonly Action Switch;

        //defaultish
        public IHSwitch(string activeLabel, string inactiveLabel, Texture2D tex, Func<bool> isActive, Action switchAction, Vector2? pos=null) : base(activeLabel, tex, DoSwitch, pos)
        {
            this.IsActive = isActive;
            this.Switch = switchAction;

            this.ActiveState = new ButtonState(activeLabel, tex, DoSwitch );
            this.InactiveState = new ButtonState(inactiveLabel, tex, DoSwitch, Color.Gray );
        }

        public IHSwitch(ButtonState activeState, ButtonState inactiveState, Func<bool> isActive, Action switchAction, Vector2? pos = null) : base(activeState, pos)
        {
            this.IsActive = isActive;
            this.Switch = switchAction;

            this.InactiveState = inactiveState;
            this.ActiveState = activeState;

        }

        public void DoSwitch()
        {
            Switch();
            displayState = IsActive() ? ActiveState : InactiveState;
        }

        // call this from ModWorld
        public void Init()
        {
            displayState = IsActive() ? ActiveState : InactiveState;
        }
    }

    // a button that dynamically changes its appearance and/or function based on an external condition
    public class IHContextButton : IHButton
    {
        public readonly Dictionary<string, ButtonState> States;
        public String CurrentState { get { return displayState.label; } }

        // private Dictionary<String, KeyWatcher> watchedKeys;
        private KeyWatcher[] keySwitch; //well this is cheesy

        // public IHContextButton(ButtonState defaultState, IEnumerable<Func<bool>> stateConditions, IEnumerable<ButtonState> altStates, Vector2? pos=null) :
        // base(defaultState, pos)

        // for just 1 alt state
        public IHContextButton(ButtonState defaultState, ButtonState altState, KState.Special? watchedKey=null, Vector2? pos=null) :
        base(defaultState, pos)
        {
            States = new Dictionary<string, ButtonState>(2);

            States.Add("Default", defaultState);
            States.Add(altState.label, altState);

            this.displayState = States["Default"]; //defaultState;

            if (watchedKey!=null)
            {
                KState.Special watchKey = watchedKey.Value;
                keySwitch = new KeyWatcher[2];
                keySwitch[0] = new KeyWatcher(watchKey, KeyEventProvider.Event.Released, () => {
                    ChangeState("Default");
                    keySwitch[0].Unsubscribe();
                    keySwitch[1].Subscribe();
                    });

                keySwitch[1] = new KeyWatcher(watchKey, KeyEventProvider.Event.Pressed,  () => {
                    ChangeState(altState);
                    keySwitch[1].Unsubscribe();
                    keySwitch[0].Subscribe();
                    });

                keySwitch[1].Subscribe();
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

        // TODO: also make use of ButtonState in IHToggle

        public void ChangeState(String stateName)
        {
            if (States.ContainsKey(stateName))
                displayState = States[stateName];
        }

        private void ChangeState(ButtonState newState)
        {
            displayState = newState;
        }

    }

    public class ButtonState
    {
        public string label;
        public Texture2D texture;
        public Action onClick;
        public Color tint;      //How to tint the texture when this state is active

        public ButtonState(string label, Texture2D tex, Action onClick, Color tintColor = Color.White)
        {
            this.label = label;
            this.texture = tex;
            this.onClick = onClick;
            this.tint = tintColor;
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

    public class KeyWatcher
    {
        // public readonly IHButton subscriber;
        private readonly KState.Special key;
        private readonly KeyEventProvider.Event evType;
        private readonly Action onKeyEvent;

        // public KeyWatcher(IHButton s, KState.Special k, KeyEventProvider.Event e, Action<IHButton> h)
        public KeyWatcher(KState.Special k, KeyEventProvider.Event e, Action h)
        {
            // subscriber = s;
            key = k;
            evType = e;
            onKeyEvent = h;
        }

        public void Subscribe()
        {
            KeyEventProvider[key].Add( evType, onKeyEvent);
        }

        // the callback
        // public void OnKeyEvent()
        // {
        //     onKeyEvent(subscriber);
        // }

        public void Unsubscribe()
        {
            KeyEventProvider[key].Remove( evType, onKeyEvent);
        }
    }

}
