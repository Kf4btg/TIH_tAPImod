using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
	// I used Shockah's mods for inspiration a lot when I first started,
	// though I soon found my own way of doing things. However, I seem
	// to have a serious mental block on understanding how an Event-
	// based system works/is supposed to work.  After a half-dozen failed
	// iterations of my own event-model, I found that Shockah's SEvent
	// class did precisely what I wanted in an embarrasingly simple and 
	// understandable fashion.  So thanks go to Shockah for this beauty,
	// mostly unchanged from how I found it.
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

    
}
