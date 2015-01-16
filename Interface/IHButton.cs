using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class IHButton
    {
        public readonly string name;
        public ButtonState displayState;

        public Vector2 pos;
        public bool isHovered;

        //these for backwards-compat (Temporary?)
        public Action onClick       { get { return displayState.onClick;}   protected set { (a) => displayState.onClick=a; } }
        public string displayLabel  { get { return displayState.label;}     protected set { (l) => displayState.label=l; } }
        public Texture2D texture    { get { return displayState.texture;}   protected set { (t) => displayState.texture=t; } }
        public Color tint           { get { return displayState.tint;}      protected set { (t) => displayState.tint=t; } }


        public IHButton(string name, Texture2D tex, Action onClick, Vector2? pos=null, Color tintColor = Color.White)
        {
            IHButton(new ButtonState(name, tex, onClick, tintColor), pos);

            // this.texture = tex;
            // this.onClick = onClick;
            // this.pos = pos ?? default(Vector2);
        }

        public IHButton(ButtonState bState, Vector2? pos=null)
        {
            this.name = bState.label;
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
            {
                sb.DrawString(Main.fontMouseText, displayState.displayLabel, pos, displayState.tint);
            }
            else {
                sb.Draw(displayState.texture, pos, displayState.tint);
            }

            if (IHButton.Hovered(this))
            {
                Main.localPlayer.mouseInterface = true;
                if (Main.mouseLeft && Main.mouseLeftRelease) onClick();
            }
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


    // a button that dynamically changes its appearance and/or function based on an external condition
    public class IHContextButton : IHButton
    {
        // private readonly ButtonState defaultState;
        private static readonly ButtonContext defaultContext = new ButtonContext( "Default", () => false);

        private bool inDefaultState;
        private readonly List<ButtonContext> Contexts = null;
        private readonly Func<ButtonContext> stateChooser;
        private Func<Bool> maintainState; //set to the most recently true state Condition; checked each "onDraw" call to see if it still applies.

        private readonly Dictionary<string, ButtonState> States;

        // private List<Tuple<KState.Special, KeyEventProvider.Event>>
        private Tuple<KState.Special, KeyEventProvider.Event> watchingKey; //just 1 for now

        public IHContextButton(ButtonState defaultState, IEnumerable<Func<bool>> stateConditions, IEnumerable<ButtonState> altStates, Vector2? pos=null) :
        base(defaultState, pos)
        {
            // this.defaultState = defaultState;
            States = new Dictionary<string, ButtonState>();
            Contexts = new List<ButtonContext>();

            States.Add(IHContextButton.defaultContext);
            // defaultContext = new Tuple ( () => false, "Default")

            for (int i=0; i<altStates.Count; i++)
            {
                try  // does a simple matching of list indices to associate elements
                {
                    var cState = new ButtonContext( altStates[i].label, stateConditions[i]  );
                    Contexts.Add(cState);
                    States.Add(altStates[i].label, altStates[i]);
                }
                catch // signify missing conditions without throwing an error
                {
                    break; //for now, just exit the loop
                    // Contexts.Add(key, new Tuple( () => false, new ButtonState("ERROR", null, () => { return; }) ));
                }
            }
            stateChooser = this.GetState;
            InitDefaultState();
        }

        //for custom state-chooser
        public IHContextButton(ButtonState defaultState, Func<String> stateChooser,
            IEnumerable<ButtonState> altStates, Vector2? pos=null) : base(defaultState, pos)
        {
            States = new Dictionary<string, ButtonState>();
            States.Add(IHContextButton.defaultContext);

            foreach (var s in altStates)
            {
                States.Add(s.label, s);
            }

            stateChooser = stateChooser;
            InitDefaultState();
        }

        // for just 1 alt state
        public IHContextButton(ButtonState defaultState, Func<bool> altCondition, ButtonState altState, Vector2? pos=null) :
        base(defaultState, pos)
        {
            // this.defaultState = defaultState;
            Contexts = new List<ButtonContext>(1);
            States = new Dictionary<string, ButtonState>(2);

            States.Add(IHContextButton.defaultContext);
            States.Add(altState.label, altState);

            Contexts.Add( new ButtonContext(altState.label, altCondition));
            stateChooser = this.GetState;
            InitDefaultState();
        }

        // TODO: replace these 3 fields w/ ButtonState in the base IHButton class.
        // TODO: also make use of ButtonState in IHToggle
        // public void ChangeState(ButtonState newState)
        // {
        //     displayLabel = newState.label;
        //     texture = newState.texture;
        //     onClick = newState.onClick;
        // }

        public void InitDefaultState()
        {
            maintainState = () => false; //always check for update in default state
            this.displayState = States["Default"]; //defaultState;
            inDefaultState = true;
        }

        // private void RefreshState()
        // {
        //     foreach (var c in Contexts)
        //     {
        //         if ( c.Item1.Invoke() )
        //         {
        //             maintainState=c.Item1;
        //             inDefaultState = false;
        //             // ChangeState(c.Item2);
        //             this.displayState = c.Item2; //works?
        //             return; //new state was set
        //         }
        //     }
        //     if (!inDefaultState)  ToDefaultState(); //reset to default
        // }

        private ButtonContext GetState()
        {
            foreach (var c in Contexts)
            {
                if ( c.isCurrent() )
                {
                    inDefaultState = false;
                    return c;
                    // ChangeState(c.Item2);
                    // this.displayState = c.Item2; //works?
                    // return; //new state was set
                }
            }
            if (!inDefaultState)
            {
                inDefaultState = true;
                return IHContextButton.defaultContext; //reset to default
            }
        }

        private void RefreshState()
        {
            var c = stateChooser();

            maintainState = c.isCurrent;
            displayState = States[c.stateName];
        }

        public override void Draw(SpriteBatch sb)
        {
            if (!maintainState()) RefreshState();

            if (texture==null)
            sb.DrawString(Main.fontMouseText, displayLabel, pos, tint);
            else
            sb.Draw(texture, pos, tint);

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

    }

    public struct ButtonState
    {
        public string label;
        public Texture2D texture;
        public Action onClick;
        public Color tint;      //How to tint the texture when this state is active

        public ButtonState(string label, Texture2D tex, Action onClick, Color tintColor)
        {
            label = label;
            texture = tex;
            onClick = onClick;
            tint = tintColor;
        }
    }

    public class ButtonContext
    {
        public String stateName;
        public Func<bool> isCurrent;
        public Tuple<KState.Special, KeyEventProvider.Event> watchKey;


        public ButtonContext(String sn, Func<bool> ic, Tuple<KState.Special, KeyEventProvider.Event> wk=null)
        {
            stateName = sn;
            isCurrent = ic;
            watchKey = wk;
        }
    }

    public class KeyWatcher
    {
        public readonly IHButton subscriber;
        public readonly KState.Special key;
        public readonly KeyEventProvider.Event evType;
        public readonly MethodInfo onKeyEvent;

        public KeyWatcher(IHButton s, KState.Special k, KeyEventProvider.Event e, Action h)
        {
            subscriber = s;
            key = k;
            evType = e;
            onKeyEvent = TypeOf(h);
            KeyEventProvider.Add(k,e,h);

        }

        //the callback
        public void OnKeyEvent()
        {
            onKeyEvent.Invoke();
        }

        public void UnSubscribe()
        {
            KeyEventProvider.Remove(key, evType, onKeyEvent);
        }
    }

}
