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
        // public bool isHovered;

        public ButtonState DisplayState { get; protected set; }
        //these for backwards-compat (Temporary?)
        public Action OnClick       { get { return DisplayState.onClick;}       }// protected set { DisplayState.onClick=value; } }
        public Action OnRightClick  { get { return DisplayState.onRightClick;}  }//protected set { DisplayState.onRightClick=value; } }
        public string Label         { get { return DisplayState.label;}         }//protected set { DisplayState.label=value; } }
        public Texture2D Texture    { get { return DisplayState.texture;}       }//protected set { DisplayState.texture=value; } }
        public Color Tint           { get { return DisplayState.tint;}          }//protected set { DisplayState.tint=value; } }

        public Rectangle? InactiveRect { get { return DisplayState.defaultTexels;}}
        public Rectangle? ActiveRect   { get { return DisplayState.altTexels;}}

        public Vector2 Size
        {
            get {
                return (Texture!=null) ?
                    (InactiveRect.HasValue ? InactiveRect.Value.Size() : Texture.Size()) :
                Main.fontMouseText.MeasureString(Label);
            }
        }

        public IHButton(Vector2? pos=null)
        {
            this.DisplayState = new ButtonState();
            this.pos = pos ?? default(Vector2);
        }

        //simple button w/ no right-click
        // public IHButton(string name, Texture2D tex, Action onClick, Rectangle? source, Vector2? pos=null, Color? tintColor = null)
        // {
        //     this.Name = name;
        //     this.DisplayState = new ButtonState(name, tex, source, onClick, null, tintColor ?? Color.White);
        //     this.pos = pos ?? default(Vector2);
        // }
        // //with right click
        // public IHButton(string name, Texture2D tex, Action onClick, Rectangle? source, Action onRightClick=null, Vector2? pos=null, Color? tintColor = null)
        // {
        //     this.Name = name;
        //     this.DisplayState = new ButtonState(name, tex, source, onClick, onRightClick, tintColor ?? Color.White);
        //     this.pos = pos ?? default(Vector2);
        // }

        //require a pre-constructed state
        public IHButton(ButtonState bState, Vector2? pos=null)
        {
            this.Name = bState.label;
            this.DisplayState = bState;
            this.pos = pos ?? default(Vector2);
        }

        protected virtual void SetState(ButtonState newState)
        {
            DisplayState = newState;
        }

        public virtual bool OnMouseEnter(ButtonBase bBase)
        {
            if (DisplayState.onMouseEnter!=null) return DisplayState.onMouseEnter(bBase);
            return true;
        }

        public virtual bool OnMouseLeave(ButtonBase bBase)
        {
            if (DisplayState.onMouseLeave!=null) return DisplayState.onMouseLeave(bBase);
            return true;
        }

        //this is just here to enable the IHToggle Update(init) function from ModWorld; better solution later.
        public virtual void OnUpdate()
        {       }

        // public virtual void Draw(SpriteBatch sb)
        // {
        //     if (DisplayState.texture==null)
        //         sb.DrawString(Main.fontMouseText, Label, pos, Tint);
        //     else
        //         sb.Draw(Texture, pos, Tint);
        //
        //     if (IHButton.Hovered(this))
        //     {
        //         if (!isHovered)
        //         {
        //             Main.PlaySound(12, -1, -1, 1); // "mouse-over" sound
        //             isHovered = true;
        //         }
        //
        //         Main.localPlayer.mouseInterface = true;
        //         if (Main.mouseLeft && Main.mouseLeftRelease) OnClick();
        //         if (Main.mouseRight && Main.mouseRightRelease && OnRightClick!=null) OnRightClick();
        //     }
        //     else isHovered = false;
        // }

        public virtual bool OnDraw(SpriteBatch sb, ButtonBase bBase)
        {
            return true;
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
            DisplayState = ActiveState;
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

            DisplayState  = ActiveState; //just to make sure nothing is null
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
        // public String CurrentState { get { return DisplayState.label; } }

        private readonly KeyWatcher[] keySwitch; //well this is cheesy

        public IHDynamicButton(ButtonState defaultState, ButtonState altState, KState.Special? watchedKey=null, Vector2? pos=null) :
        base(defaultState, pos)
        {
            this.defaultState = defaultState;
            this.altState     = altState;

            this.DisplayState = defaultState;

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

}
