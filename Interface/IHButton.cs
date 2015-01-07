using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class IHButton
    {
        public readonly string label;
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
        protected bool active;
        public readonly Action onToggle;

        public IHToggle(string label, Texture2D? tex, Action onToggle) : base(label, tex, () => { this.onToggle(); this.active = !this.active; })
        {
            this.onToggle = onToggle;
        }

        public override void Draw(SpriteBatch sb, Vector2 pos)
        {
            Color c = this.active ? Color.White : Color.Gray;
            texture.HasValue ? sb.Draw(texture.Value, pos, c) : sb.DrawString(Main.fontMouseText, label, pos, c);
        }

    }

}
