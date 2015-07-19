using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{

    public class IHEvent<T> : IEnumerable<T> where T : class
	{
		internal List<T> handlers = new List<T>();
		public int Count { get { return handlers.Count; } }

		public IEnumerator<T> GetEnumerator()
		{
			return handlers.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public void Add(T a)
		{
			handlers.Add(a);
		}
		public void Remove(T a)
		{
			handlers.Remove(a);
		}
		public void Clear()
		{
			handlers.Clear();
		}

		public static IHEvent<T> operator +(IHEvent<T> ev, T a)
		{
			ev.handlers.Add(a);
			return ev;
		}
		public static IHEvent<T> operator -(IHEvent<T> ev, T a)
		{
			ev.handlers.Remove(a);
			return ev;
		}
	}

    public class ButtonHooks
    {
        // public enum On
        // {
        //     Click,
        //     RightClick,
        //     MouseEnter,
        //     MouseLeave,
        //     PreDraw,
        //     PostDraw,
        //     WorldLoad
        // }

        public IHEvent<Action> OnClick, OnRightClick, OnWorldLoad;
        public IHEvent<Action<SpriteBatch>> PostDraw;
        public IHEvent<Func<bool>> OnMouseEnter, OnMouseLeave;
        public IHEvent<Func<SpriteBatch, bool>> PreDraw;

        public ButtonHooks()
        {
            OnClick      = new IHEvent<Action>();
            OnRightClick = new IHEvent<Action>();
            OnWorldLoad  = new IHEvent<Action>();
            PostDraw     = new IHEvent<Action<SpriteBatch>>();
            OnMouseEnter = new IHEvent<Func<bool>>();
            OnMouseLeave = new IHEvent<Func<bool>>();
            PreDraw      = new IHEvent<Func<SpriteBatch, bool>>();
        }
    }
}
