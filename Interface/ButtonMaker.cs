using System;

namespace InvisibleHand {
    
    // The Button Factory (tm)
    // Use this to get instances of the button-layer types rather than
    // directly calling their constructor.  This ensures that the layer
    // frame will be properly updated.
    public static class ButtonMaker
    {
        public static ButtonLayer GetButtons(String type)
        {
            ButtonLayer btns;
            switch(type)
            {
                case "Inventory":
                    btns = new InventoryButtons(IHBase.Instance);
                    break;
                case "Chest":
                    btns = new ChestButtons(IHBase.Instance);
                    break;
                default:
                    throw new ArgumentException("Invalid ButtonLayer type \"" + type + "\"; valid types are \"Inventory\" and \"Chest\".");
            }
            btns.UpdateFrame();
            return btns;
        }
    }
}
