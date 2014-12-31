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

  1.  __Coins__ :<br>
   `Money. New car, caviar, four star daydream.`
  2.  __Picks/Drills__ :<br>
   `Because you must dig it.`
  2.  __Axes/Chainsaws__ :<br> `For murdering trees (you monster).`
  2.  __Hammers__ :<br>
		`"Mr. Redigit, tear down this wall!"`
  2.  __Tools (other)__ :<br>
 `Non-pick/axe/hammer useful things, e.g. bug net, fishing poles, ropes, glowsticks, ...`
  2.  __Mechanisms__ :<br>
 `Wire, switches, wrenches, most anything related to the wiring system. And minecart tracks.`
  2.  __Melee Weapons__ :<br>
 `Swords, Flails, Spears. The classics. (Also boomerangs).`
  2.  __Ranged Weapons__ :<br>
 `Bows, guns, shurikens--anything that keeps you out of poking distance.`
  2.  __Bombs__ :<br>
 `Grenades, bombs, dynamite, et al. Boom.`
  2.  __Ammo__ :<br>
 `Because throwing your gun at enemies is surprisingly ineffective.`
  2.  __Magic Weapons__ :<br>
 `Wizard.`
  2.  __Summoning Weapons__ :<br>
 `Minions of all magnitudes.`
  2.  __Pets__ :<br>
 `Faithful and useless companions (though some produce light).`
  2.  __Head Armor__ :<br>
 `For keeping your wits about you.`
  2.  __Body Armor__ :<br>
 `For seriously tough torsos.`
  2.  __Leg Armor__ :<br>
 `For preventing scuffed knees.`
  2.  __Accessories__ :<br>
 `For feeling fabulous.`
  2.  __Vanity Items__ :<br>
 `For looking glamorous.`
  2.  __Potions__ :<br>
 `All things quaffable.`
  2.  __Consumables__ :<br>
 `One time use stuff that you don't drink (includes food).`
  2.  __Bait__ :<br>
 `Worms, fireflies, Master Bait, enough said.`
  2.  __Dyes__ :<br>
 `Color is the spice of life. I think.`
  2.  __Paint__ :<br>
 `For when you grow tired of brown and grey.`
  2.  __Ores__ :<br>
 `Those rocks with "Ore" in their name. Also obsidian, hellstone, meteorite.`
  2.  __Metal Bars__ :<br>
 `The above rocks made useful.`
  2.  __Gems__ :<br>
 `All the shiny stones you can dig up. Also amber.`
  2.  __Seeds__ :<br>
 `Herb, grass, and pumpkin seeds; acorns.`
  2.  __Light Sources__ :<br>
 `Torches, candles, lamps, etc. (not glowsticks)`
  2.  __Crafting Stations__ :<br>
 `From workbenches to kegs to the AutoHammer.`
  2.  __Furniture__ :<br>
 `All household objects and containers (chests) not used for crafting.`
  2.  __Statues__ :<br>
 `Statues (obviously); also gravestones & the bubble machine.`
  2.  __Wall Decorations__ :<br>
 `Paintings, trophies, a few other things.`
  2.  __Banners__ :<br>
 `Both found and dropped by NPCs.`
  2.  __Decorative Clutter__ :<br>
 `bowls, books, bottles, beer, etc`
  2.  __Wood__ :<br>
 `Raw dead trees, all varieties.`
  2.  __Blocks__ :<br>
 `e.g. dirt, stone, sand, etc.`
  2.  __Bricks__ :<br>
 `Fancy blocks.`
  2.  __Tiles__ :<br>
 `All other placeable items that do not fall into one of the above categories`
  2.  __Walls__ :<br>
 `Universally reviled by hammers.`
  2.  __Miscellaneous Materials__ :<br>
 `If you can't eat it, wear it, throw it, shoot it, or build your house out of it--but you *can* make it into something else--it'll end up here. Think herbs, goblin cloth, rotten chunks, etc.`
  2.  __Special Items__ :<br>
 `Boss summoning items, heart and mana crystals.`
  2.  __Other__ :<br>
 `If, somehow, something doesn't fit ANY of the above categories, you'll find it here. Umbrellas are a Vanilla example; unique mod items may land here for now. Further revisions to categories could move these items up the list at any item.`

#### License
  This mod was created by wuli1986(a)forums.terraria.org/Kf4btg(a)GitHub. This mod is released as open source, and you are free to modify it or use pieces of it in your own personal project. You may NOT re-release this mod in its original form or with only minor modifications and claim it as your own.  Any use you make of this mod or any mod designed for tAPI is subject to the terms of use defined by Re-Logic and the tAPI developers.

  The Dynamic LINQ API is (c) 2006 Microsoft Corporation, included here under the Microsoft Public License ([see accompanying LICENSE file][2]).

[1]:http://forums.terraria.org/index.php?threads/the-invisible-hand-inventory-management-for-the-organizationally-challenged.7547/
[2]:/DynamicQuery/LICENSE
