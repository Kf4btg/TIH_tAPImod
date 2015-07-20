using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TAPI;


namespace InvisibleHand
{
    public class LockingService<T> : ButtonService where T: CoreButton, ISocketedButton<T>
    {
        public override string ServiceType { get { return "Lock"; } }

        private readonly Color color;
        private readonly Vector2 offset;

        private readonly string lockedLabel;
        private readonly string initialLabel;
        // private readonly TIH clientAction;
        private bool isLocked;

        private readonly ButtonSocket<T> socket;

        public LockingService(T client, Vector2? lock_offset = null, Color? lock_color = null, string locked_string = "[Locked]" ) : base(client)
        {
            socket = client.ButtonBase;
            color = lock_color ?? Color.Firebrick;
            offset = lock_offset ?? default(Vector2);

            initialLabel = client.Label;
            lockedLabel = (locked_string == "") ? client.Label : client.Label + " " + locked_string;
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

    public abstract class ToggleService<T> : ButtonService where T: CoreButton, ISocketedButton<T>, new()
    {
        protected readonly ButtonSocket<T> socket;
        protected readonly KState.Special toggleKey;
        protected readonly T altButton;


        public ToggleService(T client, KState.Special toggle_key) :base(client)
        {
            socket = client.ButtonBase;
            toggleKey = toggle_key;
            altButton = InitAlternateButton(client);
        }

        /// Initialize the alternate button to which the first will toggle when specified key is pressed
        protected abstract T InitAlternateButton(T original_button);

        /// should either override this completely or at the least
        /// call this base version to properly set key-toggle.
        public override void Subscribe()
        {
            socket.RegisterKeyToggle(toggleKey, altButton);
        }
    }

    /// this class creates a second button and sets the given button's base to switch to it on shift
    // public class SorterService<T> : ButtonService where T: CoreButton, ISocketedButton<T>, new()
    public class SortingToggleService<T> : ToggleService<T> where T: CoreButton, ISocketedButton<T>, new()
    {
        public override string ServiceType { get { return "Sort"; } }

        private readonly Action sortAction;

        // public SorterService(T client, bool chest, KState.Special toggle_key) : base(client)
        public SortingToggleService(T client, bool chest, KState.Special toggle_key) : base(client, toggle_key)
        {
            if (chest)
            {
                sortAction = () => IHPlayer.SortChest();
                altButton.Hooks.OnClick += () => IHPlayer.SortChest(true);
            }
            else
            {
                sortAction = () => IHPlayer.SortInventory();
                altButton.Hooks.OnClick += () => IHPlayer.SortInventory(true);
            }
        }

        protected override T InitAlternateButton(T client)
        {
            var newButton = new T();
            newButton.CopyAttributes(client);
            return newButton;
        }

        public override void Subscribe()
        {
            base.Subscribe(); // registers toggle key
            Client.Hooks.OnClick += sortAction;
            // socket.RegisterKeyToggle(toggleKey, reverseButton);
        }
        public override void Unsubscribe()
        {
            Client.Hooks.OnClick -= sortAction;
            // TODO: figure out how to unregister key toggle
            // Also, make sure socket is displaying original
            // button and not toggled button when service removed.
        }
    }




}
