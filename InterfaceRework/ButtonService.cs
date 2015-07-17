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
        protected void RegisterHook(string hookname)
        {
            Client.RegisterServiceHook(this, hookname);
        }
        /// register a list of hooks with the client
        protected void RegisterHooks(params string[] hookNames)
        {
            foreach (var h in hookNames)
                RegisterHook(h);
        }

        /// Tell client we're no longer using this hook
        protected void RemoveHook(string hookname)
        {
            Client.RemoveServiceHook(this, hookname);
        }
        /// remove several hooks at once
        protected void RemoveHooks(params string[] hookNames)
        {
            foreach (var h in hookNames)
                RemoveHook(h);
        }
    }

    public class LockingService<T> : ButtonService where T: CoreButton, ISocketedButton<T>
    {
        public override string ServiceType { get { return "Lock"; } }

        private readonly Color color;
        private readonly Vector2 offset;

        private readonly string locked_label;
        private readonly string initial_label;
        // private readonly TIH clientAction;
        private bool isLocked;

        private readonly ButtonSocket<T> bBase;

        public LockingService(T client, Vector2? lock_offset = null, Color? lock_color = null, string locked_string = "[Locked]" ) : base(client)
        {
            bBase = client.ButtonBase;
            color = lock_color ?? Color.Firebrick;
            offset = lock_offset ?? default(Vector2);

            initial_label = client.Label;
            locked_label = (locked_string == "") ? client.Label : client.Label + " " + locked_string;

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
                Client.Label = locked_label;
            }
            else
            {
                // List<>.Remove() doesn't fail on missing keys
                RemoveHook("postDraw");
                Client.Label = initial_label;
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
                    Client.Label = locked_label;
                }
                else
                {
                    RemoveHook("postDraw");
                    Client.Label = initial_label;
                }
            }
            return true;
        }

        private void PostDraw(SpriteBatch sb)
        {
            sb.Draw(IHBase.LockedIcon, bBase.Position + offset, Client.Tint * bBase.parentLayer.LayerOpacity * bBase.Alpha);
        }
    }

    /// this class creates a second button and set's the given button's base to switch to it on shift
    public class SorterService<T> : ButtonService where T: CoreButton, ISocketedButton<T>, new()
    {
        public override string ServiceType { get { return "Sort"; } }

        private readonly ButtonSocket<T> socket;
        private readonly T reverseButton;

        public SorterService(T client, KState.Special toggleKey) : base(client)
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
