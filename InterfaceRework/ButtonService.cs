using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TAPI;


namespace InvisibleHand
{
    public abstract class ButtonService
    {
        /// do I even need this?
        public abstract string ServiceType { get; }

        /// The button to which this service's
        /// actions will attach.
        protected readonly CoreButton Client;

        public ButtonHooks Hooks;

        public ButtonService(CoreButton client)
        {
            this.Client = client;
            Hooks = new ButtonHooks();
        }

        /// Register required hooks with Client here
        public abstract void Subscribe();
        public abstract void Unsubscribe();

        /// Tell client we're using this hook
        protected void RegisterHook(string hook_name)
        {
            Client.RegisterServiceHook(this, hook_name);
        }
        /// register a list of hooks with the client
        protected void RegisterHooks(params string[] hook_names)
        {
            foreach (var h in hook_names)
                RegisterHook(h);
        }

        /// Tell client we're no longer using this hook
        protected void RemoveHook(string hook_name)
        {
            Client.RemoveServiceHook(this, hook_name);
        }
        /// remove several hooks at once
        protected void RemoveHooks(params string[] hook_names)
        {
            foreach (var h in hook_names)
                RemoveHook(h);
        }
    }

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

            Hooks.preDraw = PreDraw;
            Hooks.onRightClick = () => IHPlayer.ToggleActionLock(Client.Action);
            Hooks.postDraw = PostDraw;
            Hooks.onWorldLoad = OnWorldLoad;
        }

        public override void Subscribe()
        {
            RegisterHooks("onWorldLoad", "onRightClick", "preDraw");
        }
        public override void Unsubscribe()
        {
            RemoveHooks("onWorldLoad", "onRightClick", "preDraw");
        }

        private void OnWorldLoad()
        {
            isLocked = IHPlayer.ActionLocked(Client.Action);

            if (isLocked)
            {
                RegisterHook("postDraw");
                Client.Label = lockedLabel;
            }
            else
            {
                // List<>.Remove() doesn't fail on missing keys
                RemoveHook("postDraw");
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
                    RegisterHook("postDraw");
                    Client.Label = lockedLabel;
                }
                else
                {
                    RemoveHook("postDraw");
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

    /// this class creates a second button and set's the given button's base to switch to it on shift
    public class SorterService<T> : ButtonService where T: CoreButton, ISocketedButton<T>, new()
    {
        public override string ServiceType { get { return "Sort"; } }

        private readonly ButtonSocket<T> socket;
        private readonly T reverseButton;

        public SorterService(T client, KState.Special toggle_key) : base(client)
        {
            reverseButton = new T();
            reverseButton.CopyAttributes(client);


        }

        public override void Subscribe()
        {
            RegisterHooks("onClick", "onRightClick");
        }
        public override void Unsubscribe()
        {
            RemoveHooks("onClick", "onRightClick");
        }

        private void sort()
        {

        }

        private void reverseSort()
        {

        }

    }

}
