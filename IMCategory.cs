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
	public class IMCategory<T> : IComparable<IMCategory<T>>
	{
		public readonly ItemCat catID;
		public readonly Func<T, bool> match_params;

		public IMCategory(ItemCat catID, Func<T, bool> match_params)
		{
			this.catID=catID;
			this.match_params=match_params;
		}

		public int CompareTo(IMCategory<T> c2)
		{
			return this.catID.CompareTo(c2.catID);
		}

	}
}
