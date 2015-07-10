using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
        public Vector2 pos;

        public ButtonState DisplayState { get; protected set; }

        public TIH Action           { get { return DisplayState.action; } }
        public Action OnClick       { get { return DisplayState.onClick;} }
        public Action OnRightClick  { get { return DisplayState.onRightClick;} }
        public string Label         { get { return DisplayState.label;} }
        public Texture2D Texture    { get { return DisplayState.texture;} }
        public Color Tint           { get { return DisplayState.tint;} }

        public Rectangle? InactiveRect { get { return DisplayState.defaultTexels;} }
        public Rectangle? ActiveRect   { get { return DisplayState.altTexels;} }

        public Vector2 Size
        {
            get {
                return (Texture!=null) ?
                    (InactiveRect.HasValue ? InactiveRect.Value.Size() : Texture.Size()) :
                Main.fontMouseText.MeasureString(Label); }
        }

        public IHButton(TIH action, Vector2? pos=null)
        {
            this.DisplayState = new ButtonState(action);
            this.pos = pos ?? default(Vector2);
        }

        public IHButton(TIH action, string label, Vector2? pos=null)
        {
            this.DisplayState = new ButtonState(action, label);
            this.pos = pos ?? default(Vector2);
        }

        //require a pre-constructed state (or no state, as per the above constructor)
        public IHButton(ButtonState bState, Vector2? pos=null)
        {
            this.DisplayState = bState;
            this.pos = pos ?? default(Vector2);
        }

        protected virtual void SetState(ButtonState newState)
        {
            DisplayState = newState;
        }

        //this is just here to enable the IHToggle Update(init) function from ModWorld; better solution later.
        public virtual void OnUpdate() {}

    // the Boolean value returned from some of these hooks decide if the calling
    // function will continue after the hook has run.
    #region hooks
        public virtual bool OnMouseEnter(ButtonBase bBase)
        {
            return DisplayState.onMouseEnter==null || DisplayState.onMouseEnter(bBase);
        }

        public virtual bool OnMouseLeave(ButtonBase bBase)
        {
            return DisplayState.onMouseLeave==null || DisplayState.onMouseLeave(bBase);
        }
        // NOTE: PreDraw currently disabled in ButtonBase
        public virtual bool PreDraw(SpriteBatch sb, ButtonBase bBase)
        {
            return DisplayState.PreDraw==null || DisplayState.PreDraw(sb, bBase);
        }

        public virtual void PostDraw(SpriteBatch sb, ButtonBase bBase)
        {
            if (DisplayState.PostDraw!=null) DisplayState.PostDraw(sb, bBase);
        }

    #endregion
    }

    // This is basically an "On/Off" switch - has an active and an
    // inactive state, and onClick toggles between them.
    // The First constructor is a literal toggle, performing one action
    // that causes IsActive to swap from true to false or vice-versa and simply graying out the button
    // The second constructor allows for more flexibility in defining what actions
    // are taken when switching from one state to another and how the button display changes.
    public class IHToggle : IHButton
    {
        public readonly ButtonState ActiveState;
        public readonly ButtonState InactiveState;

        public readonly Func<bool> IsActive;
        public readonly Action OnToggle;

        private readonly Action makeActive;
        private readonly Action makeInactive;

        //defaultish - gray out the inactive state, change the label
        public IHToggle(TIH action, string inactiveLabel, string activeLabel, Texture2D tex, Func<bool> isActive,
                        Action onToggle, Vector2? pos=null, bool toggleOnRightClick = false) : base(action, inactiveLabel, pos)
        {
            IsActive = isActive;
            OnToggle = onToggle;

            if (toggleOnRightClick){
                ActiveState   = new ButtonState(action, activeLabel)   { texture = tex, onRightClick = DoToggle };
                InactiveState = new ButtonState(action, inactiveLabel) { texture = tex, onRightClick = DoToggle, tint = Color.Gray };
            } else {
                ActiveState   = new ButtonState(action, activeLabel)   { texture=tex, onClick = DoToggle };
                InactiveState = new ButtonState(action, inactiveLabel) { texture=tex, onClick = DoToggle, tint=Color.Gray };
            }
            DisplayState = InactiveState;
        }

        // provide two complete states
        public IHToggle(ButtonState inactiveState, ButtonState activeState, Func<bool> isActive,
                        Vector2? pos = null, bool toggleOnRightClick = false) : base(inactiveState, pos)
        {
            IsActive = isActive;

            ActiveState   = activeState;
            InactiveState = inactiveState;

            if (toggleOnRightClick){
                //store provided click actions in the makeXXX Actions
                makeActive   = inactiveState.onRightClick;
                makeInactive = activeState.onRightClick;

                // change button click actions to the DoSwitch wrapper function
                ActiveState.onRightClick = InactiveState.onRightClick = DoSwitch;

            } else {
                makeActive   = inactiveState.onClick;
                makeInactive = activeState.onClick;

                ActiveState.onClick = InactiveState.onClick = DoSwitch;
            }
            DisplayState  = InactiveState; //initialize
        }

        public void DoToggle()
        {
            OnToggle(); //swap the keyed game property
            SetState(IsActive() ? ActiveState : InactiveState); //update state from current value of property
        }

        // rather than just doing something like: activeState.onClick = setState(inActiveState)
        // I prefer to have it actually check the state of the game parameter it affects
        // in order to avoid issues from race-conditions or some other possible action
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

        // used to initialize button to correct state on world load
        public override void OnUpdate()
        {
            SetState(IsActive() ? ActiveState : InactiveState);
        }
    }

    // a button that dynamically changes its appearance and/or function based on
    // an external condition (as opposed to changing on user-click like IHToggle)
    public class IHDynamicButton : IHButton
    {
        private readonly ButtonState defaultState;
        private readonly ButtonState altState;

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
                    keySwitch[0].Subscribe(); //unsubscribe happens automatically due to ConcurrentBag
                    });

                // in alt, going to default
                keySwitch[0] = new KeyWatcher(watchKey, KeyEventProvider.Event.Released, () => {
                    SetState(defaultState);
                    keySwitch[1].Subscribe();
                    });

                keySwitch[1].Subscribe(); //initialize
            }
        }
    }
}
