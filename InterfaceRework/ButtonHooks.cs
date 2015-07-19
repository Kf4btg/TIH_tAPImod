using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;
using System.Dynamic;
using System.Reflection;


namespace InvisibleHand
{

    public class ButtonHookEventProvider
    {
        public enum Event
        {
            Click,
            RightClick,
            MouseEnter,
            MouseLeave,
            PreDraw,
            PostDraw,
            WorldLoad
        }
    }















    // //////////////////////////////////////////
    /// contains the functionality of a button
    // //////////////////////////////////////////
    public class ButtonHooks : DynamicObject
    {
        public Action onClick;
        public Action onRightClick;

        public Func<bool> onMouseEnter;
        public Func<bool> onMouseLeave;

        public Func<SpriteBatch, bool> preDraw;
        public Action<SpriteBatch> postDraw;

        public Action onWorldLoad;

        /// Map of hook names to dynamic option holding the actual
        /// hook Action/Function; will throw runtime errors if
        /// the hook is not invoked with correct parameter types
        private Dictionary<string, dynamic> hooks;
        public Dictionary<string, dynamic> AllHooks { get { return hooks;}}


        // Dictionary<string, object> dhooks
        // = new Dictionary<string, object>();

        public int Count
        {
            get { return hooks.Count; }
        }

        // public override bool TryGetMember(
        // GetMemberBinder binder, out object result)
        // {
        //     return hooks.TryGetValue(binder.Name, out result);
        // }
        //
        // public override bool TrySetMember(
        // SetMemberBinder binder, object value)
        // {
        //     hooks[binder.Name] = value;
        //     return true;
        // }
        //
        // public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        // {
        //     Type dictType = typeof(Dictionary<string, object>);
        //     try
        //     {
        //         result = dictType.InvokeMember(
        //                      binder.Name,
        //                      BindingFlags.InvokeMethod,
        //                      null, hooks, args);
        //         return true;
        //     }
        //     catch
        //     {
        //         result = null;
        //         return false;
        //     }
        // }

        // allows accessing the hooks like: Hooks["onClick"]
        // FIXME: this is not goodstuff for setting the value;
        public dynamic this[string hookName]
        {
            get { return hooks[hookName]; }
            set { hooks[hookName] = value; }
        }

        public ButtonHooks()
        {
            hooks = new Dictionary<string, dynamic>
            {
                {"onClick", onClick},
                {"onRightClick", onRightClick},
                {"onWorldLoad", onWorldLoad},
                {"onMouseEnter", onMouseEnter},
                {"onMouseLeave", onMouseLeave},
                {"preDraw", preDraw},
                {"postDraw", postDraw}
            };

        }

        public IEnumerator<KeyValuePair<string, dynamic>> GetEnumerator()
        {
            return hooks.GetEnumerator();
        }

        public bool isAssigned(string hookName)
        {
            return hooks[hookName] != null;
        }




    }
}
