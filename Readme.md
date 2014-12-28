The Invisible Hand
==================
### A mod for Terraria made using tAPI.

[Click here to go to this mod's thread on the Terraria.org forums][1]

This mod implements a number of convenience features to make managing one's inventory in Terraria as close to painless as possible, saving time and allowing the player to get back to enjoying the game as quickly as possible.


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
        -   Organized storage made easy!
    9.  Reverse Stack
        -   Hold Shift and press the Quickstack key to perform what is effectively a backwards Quickstack. Any stack of items in your inventory that is not already at its maximum will pull matching items from an open chest to refill itself (only as many as needed to reach the max-stack for that item type, or as many as it can before the chest's supply is exhausted).
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

#### License
  This mod was created by wuli1986(a)forums.terraria.org/Kf4btg(a)GitHub. This mod is released as open source, and you are free to modify it or use pieces of it in your own personal project. You may NOT re-release this mod in its original form or with only minor modifications and claim it as your own.  Any use you make of this mod or any mod designed for tAPI is subject to the terms of use defined by Re-Logic and the tAPI developers.

  The Dynamic LINQ API is (c) 2006 Microsoft Corporation, included here under the Microsoft Public License ([see accompanying LICENSE file][2]).

[1]:http://forums.terraria.org/index.php?threads/the-invisible-hand-inventory-management-for-the-organizationally-challenged.7547/
[2]:/DynamicQuery/LICENSE
