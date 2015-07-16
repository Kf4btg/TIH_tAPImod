using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace InvisibleHand
{
    public abstract class ButtonService
    {
        /// TODO: do I even need this?
        public abstract string ServiceID { get; }

        /// The button for which our actions will occur
        protected readonly CoreButton Client;

        public CoreButton.ButtonHooks Hooks;

        public ButtonService(CoreButton client)
        {
            this.Client = client;
            Hooks = new CoreButton.ButtonHooks();
        }

        /// Register required hooks with Client here
        public abstract void Subscribe();

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

    public class LockingService : ButtonService
    {
        public override string ServiceID { get { return "Locker"; } }

        private readonly Color color;
        private readonly Vector2 offset;
        private readonly string lockString;
        private readonly TIH clientAction;
        private bool isLocked;

        public LockingService(CoreButton client, Vector2? lock_offset = null, Color? lock_color = null, string locked_string = "[Locked]" ) : base(client)
        {
            color = lock_color ?? Color.Firebrick;
            offset = lock_offset ?? default(Vector2);
            lockString = locked_string;

            Hooks.preDraw = PreDraw;
            Hooks.onRightClick = () => IHPlayer.ToggleActionLock(clientAction);
            Hooks.postDraw = PostDraw;
            Hooks.onWorldLoad = OnWorldLoad;
        }

        public override void Subscribe()
        {
            RegisterHooks("onWorldLoad", "onRightClick", "preDraw");
        }

        private void OnWorldLoad()
        {
            isLocked = IHPlayer.ActionLocked(clientAction);

            if (isLocked)
                RegisterHook("postDraw");
            else
                RemoveHook("postDraw");
                // List<>.Remove() doesn't fail on missing keys
        }

        private bool PreDraw(SpriteBatch sb)
        {
            // Func<bool> isActive = () => IHPlayer.ActionLocked(Main.localPlayer, toLock);
            // don't run unless there's a change to avoid calling Reg/Rem Hook every frame
            if (IHPlayer.ActionLocked(clientAction) != isLocked)
            {
                isLocked = !isLocked;
                if (isLocked)
                    RegisterHook("postDraw");
                else
                    RemoveHook("postDraw");
            }
            return true;
        }

        private void PostDraw(SpriteBatch sb)
        {
            // DrawLockIndicator(sb, Client.Base); //FIXME: reenable this!
        }

        private void DrawLockIndicator(SpriteBatch sb, ButtonBase bb)
        {
            sb.Draw(IHBase.LockedIcon, bb.Position + offset, Client.Tint * bb.Container.LayerOpacity * bb.Alpha);
        }

    }
}
