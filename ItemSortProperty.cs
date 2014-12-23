using System;

namespace InvisibleHand
{
    [Flags] //allows for bitwise OR operations
    public enum ItemSortProperty
    {
        None=           0x0,
        Value=          0x1,
        Rare=           0x2,
        Damage=         0x4,
        BuffTime=       0x8,
        Paint=          0x10,
        Dye=            0x20,
        Stack=          0x40,
        MaxStack=       0x80,
        Ammo=           0x100,
        Material=       0x200,
        Bait=           0x400,
        CreateWall=     0x800

    }
}
