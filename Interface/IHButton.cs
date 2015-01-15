using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class IHButton
    {
        public readonly string name;

        public Action onClick { get { return displayState.onClick}; protected set { (a) => displayState.onClick=a; }; }


        public ButtonState displayState;

        //these for backwards-compat (Temporary)
        public string displayLabel { get { return displayState.label}; protected set { (l) => displayState.label=l; }; };
        public Texture2D texture { get { return displayState.texture}; protected set { (t) => displayState.texture=t; }; };
        public Color tint { get { return displayState.tint}; protected set { (t) => displayState.tint=t; }; };

        public Vector2 pos;
        public bool isHovered;

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
            // IHButton(bState.label, bState.texture, bState.onClick, pos);
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


    // a button that changes its appearance and/or function based on an external condition
    public class IHContextButton : IHButton
    {
        // private readonly string[] labels;
        // private readonly Func<bool>[] contextChecks;
        // protected readonly Action[] clickActions;

        private readonly string mainStateLabel;
        // private readonly Dictionary<string, Tuple<Func<bool>, ButtonState>> Contexts;
        private readonly List<Tuple<Func<bool>, ButtonState>> Contexts;

        private Func<Bool> stateIsCurrent; //set to the most recently true state Condition; checked each "onDraw" call to see if it still applies.


        public IHContextButton(IEnumerable<Func<bool>> stateConditions, IEnumerable<ButtonState> bStates, Vector2? pos=null) :
        base(bStates[0], pos)
        {
            mainStateLabel = bStates[0].label;
            Contexts = new List<Tuple<Func<bool>, ButtonState>>();

            for (int i=0, i<bStates.Count; i++)
            {
                // string key = bStates[i].label;

                try  // does a simple matching of list indices to associate elements
                {
                    var cState = new Tuple<Func<bool>, ButtonState>( stateConditions[i], bStates[i] );

                    Contexts.Add(cState);
                }
                catch // signify missing conditions without throwing an error
                {
                    break; //for now, just exit the loop
                    // Contexts.Add(key, new Tuple( () => false, new ButtonState("ERROR", null, () => { return; }) ));
                }
            }

        }

        // TODO: replace these 3 fields w/ ButtonState in the base IHButton class.
        // TODO: also make use of ButtonState in IHToggle
        public void ChangeState(ButtonState newState)
        {
            displayLabel = newState.label;
            texture = newState.texture;
            onClick = newState.onClick;
        }

        private void RefreshState()
        {
            foreach (var c in Contexts)
            {
                if ( c.Item1.Invoke() )
                {
                    stateIsCurrent=c.Item1;
                    // ChangeState(c.Item2);
                    this.displayState = c.Item2; //works?
                }
            }
        }

        public override void Draw(SpriteBatch sb)
        {

            if (!stateIsCurrent()) RefreshState();

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

        public ButtonState(string label, Texture2D tex, Action onClick)
        {
            label = label;
            texture = tex;
            onClick = onClick;

        }
    }
}
