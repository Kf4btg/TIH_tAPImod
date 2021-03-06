using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using TAPI;

namespace InvisibleHand
{

    /// adds the default OnClick to this button based on on its Action property.
    /// Set right_click to true to make the action activate with a right click
    /// rather than a left.
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

        // private readonly string  lockedLabel;
        // private readonly string  initialLabel;

        // // should we cache these for performance reasons?
        // // Benefit is probably small, if any.
        // // Compiler may do that, anyway.
        // private readonly ButtonSlot buttonBase;
        // private readonly TIH clientAction;
        private bool isLocked;

        private readonly string _serviceType;
        public override string ServiceType { get { return _serviceType; } }

        public LockingService(ICoreButton client, Vector2? lock_offset = null, Color? lock_color = null, string locked_suffix = "[Locked]" ) : base(client)
        {
            _serviceType = Enum.GetName(typeof(TIH), client.Action) + "Lock";

            color  = lock_color ?? Color.Firebrick;
            offset = lock_offset ?? default(Vector2);

            // we're not doing the suffix thing right now.
            // initialLabel = client.Label;
            // lockedLabel  = (locked_suffix == "") ? client.Label : client.Label + " " + locked_suffix;
        }

        public override void Subscribe()
        {
            Client.Hooks.OnWorldLoad  += OnWorldLoad;
            IHBase.Instance.ButtonUpdates.Push(Client.ID);

            Client.Hooks.OnRightClick += ToggleLock;
            Client.Hooks.PreDraw      += PreDraw;
        }
        public override void Unsubscribe()
        {
            Client.Hooks.OnWorldLoad  -= OnWorldLoad;
            Client.Hooks.OnRightClick -= ToggleLock;
            Client.Hooks.PreDraw      -= PreDraw;
            Client.Hooks.PostDraw     -= PostDraw;
        }

        private void ToggleLock()
        {
            Sound.Lock.Play();
            IHPlayer.ToggleActionLock(Client.Action);
        }

        private void OnWorldLoad()
        {
            isLocked = IHPlayer.ActionLocked(Client.Action);

            if (isLocked)
            {
                Client.Hooks.PostDraw += PostDraw; // draw lock indicator
                // Client.Label = lockedLabel;
            }
            else
            {
                // List<>.Remove() doesn't fail on missing keys,
                // so this is safe
                Client.Hooks.PostDraw -= PostDraw;
                // Client.Label = initialLabel;
            }
        }

        private bool PreDraw(SpriteBatch sb)
        {
            // don't run unless there's a change to avoid adding/removing
            // the event every frame
            if (IHPlayer.ActionLocked(Client.Action) != isLocked)
            {
                isLocked = !isLocked;
                if (isLocked)
                {
                    Client.Hooks.PostDraw += PostDraw;
                    // Client.Label = lockedLabel;
                }
                else
                {
                    Client.Hooks.PostDraw -= PostDraw;
                    // Client.Label = initialLabel;
                }
            }
            return true;
        }

        private void PostDraw(SpriteBatch sb)
        {
            sb.Draw(IHBase.LockedIcon, Client.ButtonBase.Position + offset,
                    color * Client.ButtonBase.ParentLayer.LayerOpacity * Client.ButtonBase.Alpha);
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

        public ToggleService(ICoreButton client, ICoreButton altButton, KState.Special toggle_key = KState.Special.Shift) : base(client)
        {
            _serviceType = Enum.GetName(typeof(TIH), client.Action) + Enum.GetName(typeof(TIH), altButton.Action) + "Toggle";
            _altButton   = altButton;

            toggleKey = toggle_key;
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

    /// Listens to a given game property and changes state automatically
    public class DynamicToggleService: ButtonService
    {
        // get current state of watched game property (or whatever)
        private readonly Func<bool> gameState;
        // state of game property in the previous frame
        private bool prevGameState;

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
            AltButton.Hooks.PostDraw += postDraw;
            prevGameState = gameState();
            Client.ButtonBase.ChangeContent(prevGameState ? Client.ID : AltButton.ID);
        }

        public override void Unsubscribe()
        {
            Client.Hooks.PostDraw -= postDraw;
            AltButton.Hooks.PostDraw -= postDraw;
            Client.ButtonBase.ChangeContent(Client.ID);
        }

        // check in post draw so as not to switch content mid-frame
        private void postDraw(SpriteBatch sb)
        {
            bool gs = gameState();
            if (gs != prevGameState)
            {
                Client.ButtonBase.ChangeContent(gs ? Client.ID : AltButton.ID);
            }
            prevGameState = gs;
        }
    }

}
