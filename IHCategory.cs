using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
	/**
	*  A generic category.
	*  @param catID: Enum that allows sorting of categories
	*  @param match_params: Function that determines whether an object fits
	*         in this category.
	*/
	public class IHCategory<T> : IComparable<IHCategory<T>>, IEquatable<IHCategory<T>>
	{
		public readonly Enum catID;
		public readonly Func<T, bool> matches;

		public IHCategory(Enum cID, Func<T, bool> match_params)
		{
			catID=cID;
			matches=match_params;
		}

		public int CompareTo(IHCategory<T> c2)
		{
			return catID.CompareTo(c2.catID);
		}

		public bool Equals(IHCategory<T> c2)
		{
			return catID == c2.catID;
		}

	}
}
