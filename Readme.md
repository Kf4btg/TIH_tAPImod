The Invisible Hand
==================
### A mod for Terraria made using tAPI.

[Click here to go to this mod's thread on the Terraria.org forums][1]

This mod implements a number of convenience features to make managing one's storage and inventory in Terraria as close to painless as possible, saving time and allowing the player to get back to enjoying the game as quickly as possible.


#### Current features
  *  __Keybinds added for several existing mechanics__
    1.  Quickstack : default key "Q"
    2.  Loot All : default key "Z"
    3.  Deposit All : default key "Y"
  *  __New actions added, along with corresponding keybinds.__
    1.  Consolidate Stacks
        *  Default key "X"
        *  Acts either on the player's inventory or the currently open chest.
        *  Collects stackable item-types and puts them into as few maximized-stacks as possible.
    2.  Sort Inventory
        -   Default key "R"
        *  Acts either on the player's inventory or the currently open chest.
        -   First, performs a stack-consolidation, then groups all items together with other items in their category (e.g. pickaxes, blocks, accessories, etc.) and re-sorts the player's inventory or currently open chest.
        -   Hold Shift while pressing this key to sort the items in reverse order.
    2.  Smart Deposit
        -   Hold Shift and press the Deposit All hotkey to only deposit items from your inventory that match a category of an item already in that chest.
        -   Items in locked inventory slots will not be deposited by this action.
        -   Organized storage made easy!
    9.  Reverse Stack
        -   Hold Shift and press the Quickstack key to perform what is effectively a backwards Quickstack. Any stack of items in your inventory that is not already at its maximum will pull matching items from an open chest to refill to refill its stack (either to maximum or as much as it can).
        -   Good for a quick refill of materials when building and crafting or for replenishing your waning reserves of potions and ammo.

  * __Several other features accessible through the mod-options__
    1.  Enable/Disable the ability to lock slots
        -   If enabled (the default), you can Shift+Right-click on any non-hotbar slot in the player inventory to "lock" that slot. An item in a locked slot will not be moved around when the player sorts their inventory. Use this feature to keep commonly-used items in certain slots so you always know exactly where they are when you need them!
        -   Items in locked slots will also stay put when doing a Smart Deposit.
    3.  Set player or chest inventories (or both) to place sorted items at the end (bottom right) of the inventory rather than the default beginning (top-left).
        -   This is similar to the way vanilla Terraria fills the inventory from the end when picking up items or using "Loot-All" on a chest.
    117.    Reverse the sort order by default.
        -   If you find you prefer the order of items that are given by the Reverse-Sort command, you can set that ordering as default for your player inventory and/or chests.
        -   With this setting, Shift+sort will now do a normal forward-sort.
        -   Experiment with various combinations of this and the previous option until you find a sorting set-up you really like.

##### Category Specifications
A few details on the defined categories presented in the order in which they will be sorted. These are obviously also the categories that will be matched against when using the Smart Deposit action.

  1.  __Coins__
   - Money. New car, caviar, four star daydream.
  2.  __Picks/Drills__
   - Because you must dig it.
  2.  __Axes/Chainsaws__
   - For murdering trees (you monster).
  2.  __Hammers__
	- "Mr. Redigit, tear down this wall!"
  2.  __Tools (other)__
   - Non-pick/axe/hammer useful things, e.g. bug net, fishing poles, ropes, glowsticks, ...
  2.  __Mechanisms__
   - Wire, switches, wrenches, most anything related to the wiring system. And minecart tracks.
  2.  __Melee Weapons__
   - Swords, Flails, Spears. The classics. (Also boomerangs).
  2.  __Ranged Weapons__
   - Bows, guns, shurikens--anything that keeps you out of poking distance.
  2.  __Bombs__
   - Grenades, bombs, dynamite, et al. Boom.
  2.  __Ammo__
   - Because throwing your gun at enemies is surprisingly ineffective.
  2.  __Magic Weapons__
   - Wizard.
  2.  __Summoning Weapons__
   - Minions of all magnitudes.
  2.  __Pets__
   - Faithful and useless companions (though some produce light).
  2.  __Head Armor__
   - For keeping your wits about you.
  2.  __Body Armor__
   - For seriously tough torsos.
  2.  __Leg Armor__
   - For preventing scuffed knees.
  2.  __Accessories__
   - For feeling fabulous.
  2.  __Vanity Items__
   - For looking glamorous.
  2.  __Potions__
   - All things quaffable.
  2.  __Consumables__
   - One time use stuff that you don't drink (includes food).
  2.  __Bait__
   - Worms, fireflies, Master Bait, enough said.
  2.  __Dyes__
   - Color is the spice of life. I think.
  2.  __Paint__
   - For when you grow tired of brown and grey.
  2.  __Ores__
   - Those rocks with "Ore" in their name. Also obsidian, hellstone, meteorite.
  2.  __Metal Bars__
   - The above rocks made useful.
  2.  __Gems__
   - All the shiny stones you can dig up. Also amber.
  2.  __Seeds__
   - Herb, grass, and pumpkin seeds; acorns.
  2.  __Light Sources__
   - Torches, candles, lamps, etc. (not glowsticks)
  2.  __Crafting Stations__
   - From workbenches to kegs to the AutoHammer.
  2.  __Furniture__
   - All household objects and containers (chests) not used for crafting.
  2.  __Statues__
   - Statues (obviously); also gravestones & the bubble machine.
  2.  __Wall Decorations__
   - Paintings, trophies, a few other things.
  2.  __Banners__
   - Both found and dropped by NPCs.
  2.  __Decorative Clutter__
   - bowls, books, bottles, beer, etc
  2.  __Wood__
   - Raw dead trees, all varieties.
  2.  __Blocks__
   - e.g. dirt, stone, sand, etc.
  2.  __Bricks__
   - Fancy blocks.
  2.  __Tiles__
   - All other placeable items that do not fall into one of the above categories
  2.  __Walls__
   - Universally reviled by hammers.
  2.  __Miscellaneous Materials__
   - If you can't eat it, wear it, throw it, shoot it, or build your house out of it--but you *can* make it into something else--it'll end up here. Think herbs, goblin cloth, rotten chunks, etc.
  2.  __Special Items__
   - Boss summoning items, heart and mana crystals.
  2.  __Other__
   - If, somehow, something doesn't fit ANY of the above categories, you'll find it here. Umbrellas are a Vanilla example; unique mod items may land here for now. Further revisions to categories could move these items up the list at any item.

#### License
  This mod was created by wuli1986(a)forums.terraria.org/Kf4btg(a)GitHub. This mod is released as open source, and you are free to modify it or use pieces of it in your own personal project. You may NOT re-release this mod in its original form or with only minor modifications and claim it as your own.  Any use you make of this mod or any mod designed for tAPI is subject to the terms of use defined by Re-Logic and the tAPI developers.

  The Dynamic LINQ API is (c) 2006 Microsoft Corporation, included here under the Microsoft Public License ([see accompanying LICENSE file][2]).

[1]:http://forums.terraria.org/index.php?threads/the-invisible-hand-inventory-management-for-the-organizationally-challenged.7547/
[2]:/DynamicQuery/LICENSE
