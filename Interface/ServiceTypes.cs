using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using TAPI;

namespace InvisibleHand
{

    /// adds the default OnClick to this button based on on its Action property.
    /// Set right_click to true to make the action happen on right click instead
    /// of left.
    public class DefaultClickService: ButtonService
    {
        private readonly bool rightClick;

        private readonly string _serviceType;
        public override string ServiceType { get { return _serviceType; } }

        public DefaultClickService(ICoreButton client, bool right_click = false) : base(client)
        {
            rightClick = right_click;

            _serviceType = "Default" + Enum.GetName(typeof(TIH), client.Action)
                           + (right_click ? "RightClick" : "Click");
        }

        public override void Subscribe()
        {
            if (rightClick)
                Client.Hooks.OnRightClick += Constants.DefaultClickActions[Client.Action];
            else
                Client.Hooks.OnClick += Constants.DefaultClickActions[Client.Action];
        }

        public override void Unsubscribe()
        {
            if (rightClick)
                Client.Hooks.OnRightClick -= Constants.DefaultClickActions[Client.Action];
            else
                Client.Hooks.OnClick -= Constants.DefaultClickActions[Client.Action];
        }
    }

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
            IHBase.Instance.ButtonUpdates.Push(Client.ID);

            Client.Hooks.OnRightClick += () => IHPlayer.ToggleActionLock(Client.Action);
            Client.Hooks.PreDraw      += PreDraw;
        }
        public override void Unsubscribe()
        {
            Client.Hooks.OnWorldLoad  -= OnWorldLoad;
            Client.Hooks.OnRightClick -= () => IHPlayer.ToggleActionLock(Client.Action);
            Client.Hooks.PreDraw      -= PreDraw;
            Client.Hooks.PostDraw     -= PostDraw;
        }

        private void OnWorldLoad()
        {
            isLocked = IHPlayer.ActionLocked(Client.Action);

            if (isLocked)
            {
                Client.Hooks.PostDraw += PostDraw; // draw lock indicator
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
            sb.Draw(IHBase.LockedIcon, socket.Position + offset,
                    Client.Tint * socket.ParentLayer.LayerOpacity * socket.Alpha);
        }
    }

    /// Generic Toggling Service for two arbitary buttons.
    public class ToggleService : ButtonService
    {
        protected readonly KState.Special toggleKey;

        private string _serviceType;
        private ICoreButton _altButton;

        public override string ServiceType { get { return _serviceType; } }
        protected virtual ICoreButton AltButton { get { return _altButton; } }

        public ToggleService(ICoreButton client, ICoreButton altButton, KState.Special toggle_key) : base(client)
        {
            _serviceType = Enum.GetName(typeof(TIH), client.Action) + Enum.GetName(typeof(TIH), altButton.Action) + "Toggle";
            _altButton   = altButton;
        }

        /// should either override this completely or at the least
        /// call this base version to properly set key-toggle.
        public override void Subscribe()
        {
            Client.ButtonBase.RegisterKeyToggle(toggleKey, AltButton.ID);
        }

        public override void Unsubscribe()
        {
            // TODO: figure out how to unregister key toggle.
            // Also, make sure socket is displaying original
            // button and not toggled button when service removed.
        }
    }

    /// Register a key-toggle between sort/rev-sort and set appropriate click actions
    public class SortingToggleService : ToggleService
    {
        private Action sortAction;
        private Action revSortAction;

        public SortingToggleService(ICoreButton forward, ICoreButton reverse, KState.Special toggle_key) : base(forward, reverse, toggle_key)
        {
            sortAction = () => IHPlayer.Sort();
            revSortAction = () => IHPlayer.Sort(true);
        }

        public override void Subscribe()
        {
            base.Subscribe(); // registers toggle key

            Client.Hooks.OnClick    += sortAction;
            AltButton.Hooks.OnClick += revSortAction;
        }
        public override void Unsubscribe()
        {
            Client.Hooks.OnClick    -= sortAction;
            AltButton.Hooks.OnClick -= revSortAction;
        }
    }

    /// Listens to a given game propery and changes state automatically
    public class DynamicToggleService: ButtonService
    {
        private readonly Func<bool> gameState;
        private bool inMain;

        private string _serviceType;

        public override string ServiceType { get { return _serviceType; } }
        private ICoreButton AltButton { get; set; }

        public DynamicToggleService(ICoreButton button_if_true, ICoreButton button_if_false, Func<bool> check_game_state) : base(button_if_true)
        {
            _serviceType = Enum.GetName(typeof(TIH), button_if_true.Action) + Enum.GetName(typeof(TIH), button_if_false.Action) + "DynamicToggle";

            gameState = check_game_state;
            AltButton = button_if_false;
        }

        public override void Subscribe()
        {
            Client.Hooks.PostDraw += postDraw;
            inMain = gameState();
            Client.ButtonBase.ChangeContent(inMain ? Client.ID : AltButton.ID);
        }

        public override void Unsubscribe()
        {
            Client.Hooks.PostDraw -= postDraw;
            Client.ButtonBase.ChangeContent(Client.ID);
        }

        // check in post draw so as not to switch content mid-frame
        private void postDraw(SpriteBatch sb)
        {
            if (gameState() != inMain)
            {
                inMain = !inMain;
                Client.ButtonBase.ChangeContent(inMain ? Client.ID : AltButton.ID);
            }
        }
    }

}
