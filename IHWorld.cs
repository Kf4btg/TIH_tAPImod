using System;
using TAPI;
using Terraria;

namespace InvisibleHand
{
    public class IHWorld : ModWorld
    {
        public override void Initialize()
        {
            // set the value for the "Loot All", "Deposit All",
            // and "Quick Stack" labels to an empty string so
            // that the default buttons for these actions (that
            // appear to the right of an open chest) do not display,
            // allowing us to replace them with customized versions.
            if (IHBase.ModOptions["UseReplacers"])
            {
                var lii = Constants.LangInterIndices;

                  Lang.inter[lii[TIH.LootAll]]
                = Lang.inter[lii[TIH.DepAll]]
                = Lang.inter[lii[TIH.QuickStack]] = "";

                // and get rid of the edit-chest stuff if we're using
                // icons rather than text
                if (IHBase.ModOptions["IconReplacers"])
                      Lang.inter[lii[TIH.Rename]]
                    = Lang.inter[lii[TIH.SaveName]]
                    = Lang.inter[lii[TIH.CancelEdit]] = "";
            }

            // finding a place to do this where the buttons are actually *set correctly*
            // upon initial load was...difficult. I hope it doesn't bork on the server.
            while (IHBase.Instance.ButtonUpdates.Count>0)
            {
                // grab the next button that has been queued for update
                string btn = IHBase.Instance.ButtonUpdates.Pop();
                // and call it's update hook
                IHBase.Instance.ButtonRepo[btn].OnUpdate();
            }
        }
    }
}
