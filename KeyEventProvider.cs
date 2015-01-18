using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

using TAPI;


namespace InvisibleHand
{
    public class KeyEventProvider
    {

        public enum Event
        {
            Pressed,
            Released
        }

        // tracker
        // private int subscriberCount;
        public int Count { get
            {
                int count = 0;

                if (activeProviders != null)
                    foreach (var kvp in activeProviders)
                        count+=kvp.Value.Count;

                if (specialProviders != null)
                    foreach (var kvp in specialProviders)
                        count+=kvp.Value.Count;

                return count;
            }}

        //lists of key-specific providers
        private Dictionary< Keys,           Provider > activeProviders;
        private Dictionary< KState.Special, Provider > specialProviders;

        // initializer
        public KeyEventProvider()
        {
            activeProviders = null;
            specialProviders = null;
        }

        //indexers
        public Provider this[Keys key]
        {
            get { return GetProvider(key); }
            // get { return activeProviders[key]; }
            //private set { AddProvider(key); }
        }

        public Provider this[KState.Special key]
        {
            get { return GetProvider(key); }
            // get { return specialProviders[key]; }
            //set { AddProvider(value); }
        }

        // add new Providers
        public void AddProvider(Keys key)
        {
            if (activeProviders == null) activeProviders = new Dictionary<Keys, Provider>();
            if (!activeProviders.ContainsKey(key))
                activeProviders.Add(key, new KeysProvider(key));
        }

        public void AddProvider(KState.Special key)
        {
            if (specialProviders == null) specialProviders = new Dictionary<KState.Special, Provider>();
            if (!specialProviders.ContainsKey(key))
                specialProviders.Add(key, new SpecialProvider(key));
        }

        //getters
        private Provider GetProvider(Keys key)
        {
            if (activeProviders == null || !activeProviders.ContainsKey(key))
                AddProvider(key);

            return activeProviders[key];
        }

        private Provider GetProvider(KState.Special key)
        {
            if (specialProviders == null || !specialProviders.ContainsKey(key))
                AddProvider(key);

            return specialProviders[key];
        }


        public void UpdateAll()
        {
            // if (subscriberCount<1) return;

            if (activeProviders != null) {
                foreach (var kvp in activeProviders)
                    { kvp.Value.UpdateSubscribers(); }}

            if (specialProviders != null) {
                foreach (var kvp in specialProviders)
                    { kvp.Value.UpdateSubscribers(); }}
        }



        public abstract class Provider
        {
            protected int subscriberCount;
            public int Count { get { return subscriberCount; }}

            protected Dictionary<KeyEventProvider.Event, List<Action>> subscribers;

            protected Provider()
            {
                subscribers = new Dictionary<KeyEventProvider.Event, List<Action>>();

                subscribers.Add(KeyEventProvider.Event.Pressed, new List<Action>());
                subscribers.Add(KeyEventProvider.Event.Released, new List<Action>());
            }

            public virtual void Add(KeyEventProvider.Event ev, Action handler)
            {
                subscribers[ev].Add(handler);
                subscriberCount++;
            }

            public virtual void Remove(KeyEventProvider.Event ev, Action handler)
            {
                subscribers[ev].Remove(handler);
                subscriberCount--;
            }

            //call this from one of the "Updates-every-frame" hooks to send events to listeners (e.g. buttons) on a key change
            public abstract void UpdateSubscribers();

        }

        public class KeysProvider : Provider
        {
            private readonly Keys _key;

            public KeysProvider(Keys key) : base()
            {
                _key = key;
            }

            public override void UpdateSubscribers()

            {
                if (subscriberCount < 1) return;

                if (_key.Pressed())
                    foreach (Action handler in subscribers[KeyEventProvider.Event.Pressed] )
                        handler();

                if (_key.Released())
                    foreach (Action handler in subscribers[KeyEventProvider.Event.Released] )
                        handler();
            }
        }

        public class SpecialProvider : Provider
        {
            private readonly KState.Special _key;

            public SpecialProvider(KState.Special key) : base()
            {
                _key = key;
            }

            public override void UpdateSubscribers()
            {
                if (subscriberCount < 1) return;

                if (_key.Pressed())
                    foreach (Action handler in subscribers[KeyEventProvider.Event.Pressed] )
                        handler();

                if (_key.Released())
                    foreach (Action handler in subscribers[KeyEventProvider.Event.Released] )
                        handler();
            }
        }
    }

    //only accepts KState.Special at the moment
    public class KeyWatcher
    {
        private readonly KState.Special key;
        private readonly KeyEventProvider.Event evType;
        private readonly Action onKeyEvent;

        public KeyWatcher(KState.Special k, KeyEventProvider.Event e, Action h)
        {
            key = k;
            evType = e;
            onKeyEvent = h;
        }

        public void Subscribe()
        {
            IHBase.KEP[key].Add(evType, onKeyEvent);
        }

        public void Unsubscribe()
        {
            IHBase.KEP[key].Remove( evType, onKeyEvent);
        }
    }

}
