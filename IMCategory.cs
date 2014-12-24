using System;
using System.Collections.Generic;
using TAPI;
using Terraria;

namespace InvisibleHand
{
	/**
	*  A generic category.
	*  @param catID: integer that allows sorting of categories
	*  @param match_params: Function that determines whether an object fits
	*         in this category.
	*/
	public class IMCategory<T> : IComparable<IMCategory<T>>, IEquatable<IMCategory<T>>
	{
		public readonly Enum catID;
		public readonly Func<T, bool> matches;

		public IMCategory(Enum cID, Func<T, bool> match_params)
		{
			catID=cID;
			matches=match_params;
		}

		public int CompareTo(IMCategory<T> c2)
		{
			return catID.CompareTo(c2.catID);
		}

		public bool Equals(IMCategory<T> c2)
		{
			return catID == c2.catID;
		}

	}
}
