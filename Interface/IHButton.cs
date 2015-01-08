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
        public readonly Texture2D texture;
        public readonly Action onClick;

        public string displayLabel;
        public Vector2 pos;
        public bool isHovered;

        public IHButton(string name, Texture2D tex, Action onClick, Vector2? pos=null)
        {
            this.displayLabel = this.name = name;
            this.texture = tex;
            this.onClick = onClick;
            this.pos = pos ?? default(Vector2);
        }

        public Vector2 Size
        {
            get {
                return (texture!=null) ? texture.Size() : Main.fontMouseText.MeasureString(displayLabel);
            }
        }

        public virtual void Draw(SpriteBatch sb)
        {
            if (texture==null)
            {
                sb.DrawString(Main.fontMouseText, displayLabel, pos, Color.White);
            }
            else {
                sb.Draw(texture, pos, Color.White);
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

    // a button with 2 states: active and inactive. OnClick() toggles between the states
    public class IHToggle : IHButton, IHUpdateable
    {
        public readonly string activeLabel, inactiveLabel;
        // public readonly Action onToggle;
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
}
