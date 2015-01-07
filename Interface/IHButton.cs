using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class IHButton
    {
        public string label;
        public readonly Texture2D? texture;
        public readonly Action onClick;

        public IHButton(string label, Texture2D? tex, Action onClick)
        {
            this.label = label;
            this.texture = tex;
            this.onClick = onClick;
        }

        public Vector2 Size
        {
            get
            {
                return (texture.HasValue) ? texture.Value.Size() : Main.fontMouseText.MeasureString(label);
            }
        }

        public virtual void Draw(SpriteBatch sb, Vector2 pos)
        {
            texture.HasValue ? sb.Draw(texture.Value, pos, Color.White) : sb.DrawString(Main.fontMouseText, label, pos, Color.White);
        }

        public static bool Hovered(IHButton b, Vector2 pos)
        {
            return (new Rectangle((int)pos.X, (int)pos.Y, (int)b.Size.X, (int)b.Size.Y).Contains(Main.mouseX, Main.mouseY));
        }

    }

    public class IHToggle : IHButton
    {
        // protected bool active;
        protected readonly string activeLabel, inactiveLabel;
        public readonly Action onToggle;
        public readonly Func<bool> isActive;

        public IHToggle(string activeLabel, string inactiveLabel, Texture2D? tex, Func<bool> isActive, Action onToggle) :
            base(label, tex, () => { this.onToggle(); this.changeState(); })
        {
            this.activeLabel = activeLabel;
            this.inactiveLabel = inactiveLabel;
            this.isActive = isActive;
            this.onToggle = onToggle;
        }

        protected void changeState()
        {
            // active = !active;
            label = isActive() ? activeLabel : inactiveLabel;
        }

        public override void Draw(SpriteBatch sb, Vector2 pos)
        {
            Color c = isActive() ? Color.White : Color.Gray;
            texture.HasValue ? sb.Draw(texture.Value, pos, c) : sb.DrawString(Main.fontMouseText, label, pos, c);
        }

        public static bool IsActive(IHToggle t)
        {
            return t.isActive();
        }

    }

}
