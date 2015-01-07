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
        public string displayLabel;

        public readonly Texture2D texture;
        public readonly Action onClick;

        public IHButton(string name, Texture2D tex, Action onClick)
        {
            this.displayLabel = this.name = name;
            this.texture = tex;
            this.onClick = onClick;
        }

        public Vector2 Size
        {
            get {
                return (texture!=null) ? texture.Size() : Main.fontMouseText.MeasureString(displayLabel);
            }
        }

        public virtual void Draw(SpriteBatch sb, Vector2 pos)
        {
            if (texture==null)
            {
                sb.DrawString(Main.fontMouseText, displayLabel, pos, Color.White);
            }
            else {
                sb.Draw(texture, pos, Color.White);
            }
        }

        public static bool Hovered(IHButton b, Vector2 pos)
        {
            return (new Rectangle((int)pos.X, (int)pos.Y, (int)b.Size.X, (int)b.Size.Y).Contains(Main.mouseX, Main.mouseY));
        }
    }

    public class IHToggle : IHButton, IHUpdateable
    {
        // protected bool active;
        protected readonly string activeLabel, inactiveLabel;
        public readonly Action onToggle;
        public readonly Func<bool> isActive;
        // public readonly IHToggle me;

        public IHToggle(string name, string activeLabel, string inactiveLabel, Texture2D tex, Func<bool> isActive, Action onToggle) :
            base(name, tex, delegate{ onToggle(); })
        {
            this.activeLabel = activeLabel;
            this.inactiveLabel = inactiveLabel;
            this.isActive = isActive;
            this.onToggle = onToggle;

            // me=this;

            // FlagUpdate();
        }


        public void FlagUpdate()
        {
            IHToggle.MarkUpdate(this);
        }

        public void onUpdate(SpriteBatch sb)
        {
            displayLabel = isActive() ? activeLabel : inactiveLabel;
        }

        public override void Draw(SpriteBatch sb, Vector2 pos)
        {
            Color c = isActive() ? Color.White : Color.Gray;
            if (texture==null)
            {
                sb.DrawString(Main.fontMouseText, displayLabel, pos, c);
            }
            else {
                sb.Draw(texture, pos, c);
            }

            if (IHButton.Hovered(this, pos))
            {
                if (Main.mouseLeft && Main.mouseLeftRelease) onClick();
            }
        }

        public static bool IsActive(IHToggle t)
        {
            return t.isActive();
        }

        public static void MarkUpdate(IHToggle t)
        {
            IHBase.toUpdate.Push(t);
        }
    }

}
