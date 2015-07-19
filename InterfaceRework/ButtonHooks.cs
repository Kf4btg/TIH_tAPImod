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

    /// because there's no void/none/null type
    public sealed class None
    {
        private None() { }
        private readonly static None _none = new None();
        public static None none { get { return _none; } }
    }


    public class ButtonHook
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

        public abstract class ButtonEvent<Tresult>
        {
            private Event eventType;

            public ButtonEvent(Event event_type)
            {
                this.eventType = event_type;
            }

            public abstract Tresult Call();

        }

        public class EventAction : ButtonEvent<None>
        {
            private Action eventHandler;

            public EventAction(Event event_type, Action handler) : base(event_type)
            {
                this.eventHandler = handler;
            }

            public override None Call()
            {
                eventHandler.Invoke();
                return None.none;
            }
        }

        public class EventAction<Targ> : ButtonEvent<None>
        {
            private Action<Targ> eventHandler;
            private Targ handlerParam;

            public EventAction(Event event_type, Targ param, Action<Targ> handler) : base(event_type)
            {
                this.eventHandler = handler;
                handlerParam = param;
            }

            public override None Call()
            {
                eventHandler.Invoke(handlerParam);
                return None.none;
            }
        }



        public class EventFunc<Tresult> : ButtonEvent<Tresult>
        {
            private Func<Tresult> eventHandler;

            public EventFunc(Event event_type, Func<Tresult> handler) : base(event_type)
            {
                this.eventHandler = handler;
            }

            public override Tresult Call()
            {
                return eventHandler.Invoke();
            }
        }

        public class EventFunc<Targ, Tresult> : ButtonEvent<Tresult>
        {
            private Func<Targ, Tresult> eventHandler;
            private Targ handlerParam;

            public EventFunc(Event event_type, Targ param, Func<Targ, Tresult> handler) : base(event_type)
            {
                this.eventHandler = handler;
                handlerParam = param;
            }

            public override Tresult Call()
            {
                return eventHandler.Invoke(handlerParam);
            }
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
