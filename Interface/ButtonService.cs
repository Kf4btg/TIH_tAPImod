using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace InvisibleHand
{
    public abstract class ButtonService
    {
        public abstract string ServiceID { get; }

        protected readonly CoreButton Client;

        public CoreButton.ButtonHooks Hooks;

        public ButtonService(CoreButton client)
        {
            this.Client = client;
            Hooks = new CoreButton.ButtonHooks();
        }

        public abstract void Subscribe();

        protected virtual void RegisterHook(string hookname)
        {
            Client.RegisterHook(this, hookname);
        }
        protected virtual void RegisterHooks(params string[] hookNames)
        {
            foreach (var h in hookNames)
                RegisterHook(h);
        }

        protected virtual void RemoveHook(string hookname)
        {
            Client.RemoveHook(this, hookname);
        }
        protected virtual void RemoveHooks(params string[] hookNames)
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
            Hooks.onWorldLoad = onWorldLoad;
        }

        public override void Subscribe()
        {
            RegisterHooks("onWorldLoad", "onRightClick", "preDraw");
        }

        private void onWorldLoad()
        {
            isLocked = IHPlayer.ActionLocked(clientAction);

            if (isLocked)
                RegisterHook("postDraw");
            else
                RemoveHook("postDraw");
                // List<>.Remove() doesn't fail on missing keys

        }

        private bool PreDraw(SpriteBatch sb, ButtonBase bb)
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

        private void PostDraw(SpriteBatch sb, ButtonBase bBase)
        {

        }

        private static void DrawLockIndicator(SpriteBatch sb, ButtonBase bb, ButtonLayer parent, Vector2 offset, Color tint)
        {
            sb.Draw(IHBase.LockedIcon, bb.Position + offset, tint * parent.LayerOpacity * bb.Alpha);
        }

    }
}
