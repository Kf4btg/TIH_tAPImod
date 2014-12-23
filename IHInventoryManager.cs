using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using TAPI;
using Terraria;

namespace InvisibleHand {

	public static class InventoryManager
	{
		public static List<IMCategory<Item>> Categories = new List<IMCategory<Item>>();

		public static IMCategory<Item>
			catHead, catBody, catLegs, catVanity, catMelee, catRanged, catAmmo, catMagic, catSummon, catAccessory,
			catPick, catAxe, catHammer, catConsume, catDye, catPaint, catOre, catTile, catWall, catPet, catOther, catBait;

		// category id defines where its items will be sorted with regards
		// to items of other categories.
		public const int    ID_PICK 		=  1,
							ID_AXE 			=  2,
							ID_HAMMER 		=  3,
							ID_MELEE 		=  4,
							ID_RANGED 		=  5,
							ID_MAGIC 		=  6,
							ID_SUMMON 		=  7,
							ID_AMMO 		=  8,
							ID_HEAD 		=  9,
							ID_BODY 		= 10,
							ID_LEGS 		= 11,
							ID_ACCESSORY 	= 12,
							ID_VANITY 		= 13,
							ID_PET 			= 14,
							ID_CONSUME 		= 15,
							ID_BAIT			= 16,
							ID_DYE 			= 17,
							ID_PAINT 		= 18,
							ID_ORE			= 19,
							ID_TILE 		= 20,
							ID_WALL 		= 21,
							ID_OTHER		= 22;


		/*************************************************************************
		*  Define the categories and put them in an array.
		*/
		public static void Initialize()
		{
			// Item matching functions unceremoniously stolen from Shockah's Fancy Cheat Menu mod.
			// Thanks for doing all the hard work figuring these out so I didn't have to!
			// Edit: ok, so I ended up editing a few to make them mutually exclusive
			// (e.g. adding the !vanity check to the armor items)
			catPick		= new IMCategory<Item>( ID_PICK, 		item   	=> item.pick > 0);
			catAxe		= new IMCategory<Item>( ID_AXE, 		item   	=> item.axe > 0);
			catHammer	= new IMCategory<Item>( ID_HAMMER, 		item   	=> item.hammer > 0);
			catHead		= new IMCategory<Item>( ID_HEAD,		item 	=> item.headSlot != -1 && !item.vanity);
			catBody		= new IMCategory<Item>( ID_BODY,		item 	=> item.bodySlot != -1 && !item.vanity);
			catLegs		= new IMCategory<Item>( ID_LEGS,		item 	=> item.legSlot  != -1 && !item.vanity);
			catAccessory= new IMCategory<Item>( ID_ACCESSORY,	item   	=> item.accessory && !item.vanity);
			catVanity	= new IMCategory<Item>( ID_VANITY,		item 	=> item.vanity);
			catMelee	= new IMCategory<Item>( ID_MELEE,		item 	=> item.damage > 0 && item.melee);
			catRanged	= new IMCategory<Item>( ID_RANGED,		item 	=> item.damage > 0 && item.ranged && (item.ammo == 0));
			catAmmo		= new IMCategory<Item>( ID_AMMO, 		item   	=> item.damage > 0 && item.ranged && item.ammo != 0 && !item.notAmmo);
			catMagic	= new IMCategory<Item>( ID_MAGIC, 		item   	=> item.damage > 0 && item.magic);
			catSummon	= new IMCategory<Item>( ID_SUMMON, 		item   	=> item.damage > 0 && item.summon);
			catConsume	= new IMCategory<Item>( ID_CONSUME, 	item   	=> item.consumable && item.bait == 0 && item.damage <= 0 && item.createTile == -1 && item.tileWand == -1 && item.createWall == -1 && item.ammo == 0 && item.name != "Xmas decorations");
			catBait 	= new IMCategory<Item>( ID_BAIT, 		item   	=> item.bait > 0 && item.consumable);
			catDye		= new IMCategory<Item>( ID_DYE, 		item   	=> item.dye != 0);
			catPaint	= new IMCategory<Item>( ID_PAINT, 		item   	=> item.paint != 0);
			catOre 		= new IMCategory<Item>( ID_ORE, 		item   	=> item.createTile != -1 && item.tileWand == -1 && item.consumable && item.maxStack==999 && item.name != "Xmas decorations"  );
			catTile		= new IMCategory<Item>( ID_TILE, 		item   	=> item.createTile != -1 || item.tileWand != -1 || item.name == "Xmas decorations");
			catWall		= new IMCategory<Item>( ID_WALL, 		item   	=> item.createWall != -1);
			catPet		= new IMCategory<Item>( ID_PET, 		item   	=> item.damage <= 0 && ((item.shoot > 0 && Main.projPet[item.shoot]) || (item.buffType > 0 && (Main.vanityPet[item.buffType] || Main.lightPet[item.buffType]))));
			catOther	= new IMCategory<Item>( ID_OTHER, 		null);

			Categories.AddRange(new IMCategory<Item>[]
				{
					catPick, 		//  1
					catAxe, 		//  2
					catHammer, 		//  3
					catMelee, 		//  4
					catRanged, 		//  5
					catMagic, 		//  6
					catSummon, 		//  7
					catAmmo, 		//  8
					catHead, 		//  9
					catBody, 		// 10
					catLegs, 		// 11
					catAccessory, 	// 12
					catVanity, 		// 13
					catPet, 		// 14
					catConsume,
					catBait,		//
					catDye, 		//
					catPaint,
					catOre, 		//
					catTile, 		//
					catWall, 		//
					catOther		//  --must be last
				});
		} //end initialize()


		/*************************************************************************
		*  SortPlayerInv - perform the sort operation on the items in the player's
		*	inventory, excluding the hotbar and optionally any slots marked as locked
		*
		*  @param player: The player whose inventory to sort.
		*/
		public static void SortPlayerInv(Player player, bool reverse=false)
		{
			ConsolidateStacks(player.inventory, 0, 49); //include hotbar in this step

			Sort(player.inventory, false, reverse, 10, 49);
		}

		public static void SortChest(Item[] chestitems, bool reverse=false)
		{
			ConsolidateStacks(chestitems);

			Sort(chestitems, true, reverse);
		}

		public static void SortChest(Chest chest, bool reverse=false)
		{
			ConsolidateStacks(chest.item);

			Sort(chest.item, true, reverse);
		}


		/*************************************************************************
		*  Sort Container
		*
		*  @param container: The container whose contents to sort.
		*  @param checkLocks: whether to check for & exclude locked slots
		*  @param chest : whether the container is a chest (otherwise the player inventory)
		*  @param rangeStart: starting index of the sort operation
		*  @param rangeEnd: end index of the sort operation
		*
		*  Omitting both range arguments will sort the entire container.
		*/
		public static void Sort(Item[] container, bool chest, bool reverse, int rangeStart, int rangeEnd)
		{
			Sort(container, chest, reverse, new Tuple<int,int>(rangeStart, rangeEnd));
		}

		public static void Sort(Item[] container, bool chest, bool reverse, Tuple<int,int> range = null)
		{
			// if range param not specified, set it to whole container
			if (range == null) range = new Tuple<int,int>(0, container.Length -1);

			// for clarity
			bool checkLocks = IHBase.lockingEnabled;

			// initialize the list that will hold the items to sort
			List<CategorizedItem> itemSorter = new List<CategorizedItem>();

			// delegate a category to each item, then add to sorted list
			for (int i=range.Item1; i<=range.Item2; i++)
			{
				// if this is the player inv && locking is enabled && slot is locked skip it
				if (!chest && checkLocks && IHPlayer.SlotLocked(i)) continue;

				if ( !container[i].IsBlank() )
				{
					Item itemcopy = container[i].Clone();

					// go through category-list, checking the item against the match parameters
					// for each category until the item either matches a category or fails all
					// the checks (except for the last category, Other, which isn't checked and
					// into which all unmatched items will fall).
					int cid = 0;
					while ( cid<(Categories.Count - 1) && !Categories[cid].match_params(itemcopy) ) { cid++; }

					// currently, this makes category order dependent on the order
					// in which they are added to the Categories[] array...which is
					// not the intention, but it works for now.
					itemSorter.Add( new CategorizedItem(itemcopy, Categories[cid]) );
				}
			}

			/* now this seems too easy... */
			itemSorter.Sort();  // sort using the CategorizedItem.CompareTo() method
			if (reverse) itemSorter.Reverse();

			// depending on user settings, decide if we re-copy items to end or beginning of container
			bool fillFromEnd = false;
			switch(IHBase.opt_reverseSort)
			{
				case IHBase.RS_DISABLE:		//normal sort to beginning
					break;
				case IHBase.RS_PLAYER:		//copy to end for player inventory only
					fillFromEnd = !chest;
					break;
				case IHBase.RS_CHEST:		//copy to end for chests only
					fillFromEnd = chest;
					break;
				case IHBase.RS_BOTH:		//copy to end for all containers
					fillFromEnd=true;
					break;
			}

			// set up the functions that will be used in the iterators ahead
			Func<int,int> getIndex, getIter;
			Func<int,bool> getCond, getWhileCond;

			if (fillFromEnd)	// use decrementing iterators
			{
				getIndex = x => range.Item2 - x;
				getIter = x => x-1;
				getCond = x => x >= range.Item1;
				getWhileCond = x => x>range.Item1 && IHPlayer.SlotLocked(x);

			}
			else 	// use incrementing iterators
			{
				getIndex = y => range.Item1 + y;
				getIter = y => y+1;
				getCond = y => y <= range.Item2;
				getWhileCond = y => y<range.Item2 && IHPlayer.SlotLocked(y);
			}

			int filled = 0;
			if (!chest && checkLocks) // move these checks out of the loop
			{
				// copy the sorted items back to the original container
				foreach (CategorizedItem citem in itemSorter)
				{
					// find the first unlocked slot.
					// this would throw an exception if range.Item1+filled somehow went over 49,
					// but if the categorizer and slot-locker are functioning correctly,
					// that _shouldn't_ be possible. Shouldn't. Probably.
					while (IHPlayer.SlotLocked(getIndex(filled))) { filled++; }
					container[getIndex(filled++)] = citem.item.Clone();
				}
				// and the rest of the slots should be empty
				for (int i=getIndex(filled); getCond(i); i=getIter(i))
				{
					// find the first unlocked slot.
					if (IHPlayer.SlotLocked(i)) continue;

					container[i] = new Item();
				}
			}
			else // just run through 'em all
			{
				foreach (CategorizedItem citem in itemSorter)
				{
					container[getIndex(filled++)] = citem.item.Clone();
					// filled++;
				}
				// and the rest of the slots should be empty
				for (int i=getIndex(filled); getCond(i); i=getIter(i))
				{
					container[i] = new Item();
				}
			}
		} // sort()


		/*************************************************************************
		*  Adapted from "PutItem()" in ShockahBase.SBase
		*  @params container, rangeStart, rangeEnd
		*/
		public static void ConsolidateStacks(Item[] container, int rangeStart, int rangeEnd)
		{
			ConsolidateStacks(container, new Tuple<int,int>(rangeStart, rangeEnd));
		}

		public static void ConsolidateStacks(Item[] container, Tuple<int, int> range = null)
		{
			if (range == null) range = new Tuple<int,int>(0, container.Length -1);

			// for (int i = range.Item1; i<=range.Item2; i++)
			for (int i = range.Item2; i>=range.Item1; i--) //iterate in reverse
			{
				Item item = container[i];

				//found non-blank item in a <full stack
				if (!item.IsBlank() && item.stack < item.maxStack)
				{
					// search the remaining slots for other stacks of this item
					// StackItems(ref item, container, i+1, range.Item2);
					StackItems(ref item, container, range.Item1, i-1);
				}
			}
		}

		// called by ConsolidateStacks, this takes a single item and searches a subset of the original
		// range for other non-max stacks of that item
		private static void StackItems(ref Item item, Item[] container, int rangeStart, int rangeEnd)
		{
			// for (int j=rangeStart; j<=rangeEnd; j++)
			// Func<int,int> jiter;
			// if (IHBase.lockingEnabled) {
			// 	jiter = jay => jay--;
			// }

			for (int j=rangeEnd; j>=rangeStart; j--) //iterate in reverse
			{
				Item item2 = container[j];
				// found another <full stack of a matching item
				if (!item2.IsBlank() && item2.IsTheSameAs(item) && item2.stack < item2.maxStack)
				{
					int diff = Math.Min(item2.maxStack - item2.stack, item.stack);
					item2.stack += diff;
					item.stack -= diff;

					if (item.IsBlank())
					{
						item = new Item();
						return;
					}
				}
			}
		}

	} //class
} //namespace
