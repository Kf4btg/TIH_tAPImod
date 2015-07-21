using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using TAPI;

namespace InvisibleHand
{
    public class LockingService: ButtonService
    {
        private readonly Color   color;
        private readonly Vector2 offset;

        private readonly string  lockedLabel;
        private readonly string  initialLabel;
        // private readonly TIH clientAction;
        private bool isLocked;

        private readonly IButtonSlot socket;

        private readonly string _serviceType;
        public override string ServiceType { get { return _serviceType; } }

        public LockingService(ICoreButton client, Vector2? lock_offset = null, Color? lock_color = null, string locked_string = "[Locked]" ) : base(client)
        {
            _serviceType = Enum.GetName(typeof(TIH), client.Action) + "Lock";

            socket = client.ButtonBase;
            color  = lock_color ?? Color.Firebrick;
            offset = lock_offset ?? default(Vector2);

            initialLabel = client.Label;
            lockedLabel  = (locked_string == "") ? client.Label : client.Label + " " + locked_string;
        }

        public override void Subscribe()
        {
            Client.Hooks.OnWorldLoad  += OnWorldLoad;
            Client.Hooks.OnRightClick += () => IHPlayer.ToggleActionLock(Client.Action);
            Client.Hooks.PreDraw      += PreDraw;

            // RegisterHooks("onWorldLoad", "onRightClick", "preDraw");
        }
        public override void Unsubscribe()
        {
            Client.Hooks.OnWorldLoad  -= OnWorldLoad;
            Client.Hooks.OnRightClick -= () => IHPlayer.ToggleActionLock(Client.Action);
            Client.Hooks.PreDraw      -= PreDraw;
            Client.Hooks.PostDraw     -= PostDraw;
            // RemoveHooks("onWorldLoad", "onRightClick", "preDraw");
        }

        private void OnWorldLoad()
        {
            isLocked = IHPlayer.ActionLocked(Client.Action);

            if (isLocked)
            {
                // RegisterHook("postDraw");
                Client.Hooks.PostDraw += PostDraw;
                Client.Label = lockedLabel;
            }
            else
            {
                // List<>.Remove() doesn't fail on missing keys
                Client.Hooks.PostDraw -= PostDraw;
                Client.Label = initialLabel;
            }
        }

        private bool PreDraw(SpriteBatch sb)
        {
            // Func<bool> isActive = () => IHPlayer.ActionLocked(Main.localPlayer, toLock);
            // don't run unless there's a change to avoid calling Reg/Rem Hook every frame
            if (IHPlayer.ActionLocked(Client.Action) != isLocked)
            {
                isLocked = !isLocked;
                if (isLocked)
                {
                    Client.Hooks.PostDraw += PostDraw;
                    Client.Label = lockedLabel;
                }
                else
                {
                    Client.Hooks.PostDraw -= PostDraw;
                    Client.Label = initialLabel;
                }
            }
            return true;
        }

        private void PostDraw(SpriteBatch sb)
        {
            sb.Draw(IHBase.LockedIcon, socket.Position + offset, Client.Tint * socket.ParentLayer.LayerOpacity * socket.Alpha);
        }
    }

    /// Generic Toggling Service for two arbitary buttons.
    public class ToggleService<T> : ButtonService where T: ISocketedButton<T>
    {
        protected readonly IButtonContentHandler<T> socket;
        protected readonly KState.Special  toggleKey;

        private string _serviceType;
        private T _altButton;

        public override string ServiceType { get { return _serviceType; } }
        protected virtual T AltButton { get { return _altButton; } }

        public ToggleService(T client, T altButton, KState.Special toggle_key) : base(client)
        {
            socket       = client.Socket;
            _serviceType = Enum.GetName(typeof(TIH), client.Action) + Enum.GetName(typeof(TIH), altButton.Action) + "Toggle";
            _altButton   = altButton;
        }

        /// should either override this completely or at the least
        /// call this base version to properly set key-toggle.
        public override void Subscribe()
        {
            socket.RegisterKeyToggle(toggleKey, AltButton);
        }

        public override void Unsubscribe()
        {
            // TODO: figure out how to unregister key toggle.
            // Also, make sure socket is displaying original
            // button and not toggled button when service removed.
        }
    }

    /// this class creates a second button and sets the given button's base to switch to it on shift
    // public class SorterService<T> : ButtonService where T: CoreButton, ISocketedButton<T>, new()
    public class SortingToggleService<T>: ToggleService<T> where T: ISocketedButton<T>
    {
        public override string ServiceType { get { return "SortingToggle"; } }

        private Action sortAction;
        private Action revSortAction;
        private readonly bool sortChest;

        // public SorterService(T client, bool chest, KState.Special toggle_key) : base(client)
        public SortingToggleService(T forward, T reverse, bool chest, KState.Special toggle_key) : base(forward, reverse, toggle_key)
        {
            sortChest = chest;
        }

        public override void Subscribe()
        {
            base.Subscribe(); // registers toggle key
            if (sortChest)
            {
                sortAction    = () => IHPlayer.SortChest();
                revSortAction = () => IHPlayer.SortChest(true);
            }
            else
            {
                sortAction    = () => IHPlayer.SortInventory();
                revSortAction = () => IHPlayer.SortInventory(true);
            }
            Client.Hooks.OnClick    += sortAction;
            AltButton.Hooks.OnClick += revSortAction;
        }
        public override void Unsubscribe()
        {
            Client.Hooks.OnClick    -= sortAction;
            AltButton.Hooks.OnClick -= revSortAction;
        }
    }
}
