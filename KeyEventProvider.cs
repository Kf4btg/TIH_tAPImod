using System;
using System.Collections.Concurrent;
// using System.Collections.Generic;
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

        //lists of key-specific providers
        private ConcurrentDictionary< Keys,           Provider > activeProviders;
        private ConcurrentDictionary< KState.Special, Provider > specialProviders;

        // initializer
        public KeyEventProvider()
        {
            activeProviders  = new ConcurrentDictionary<Keys, Provider>();
            specialProviders = new ConcurrentDictionary<KState.Special, Provider>();
        }

        //indexers
        public Provider this[Keys key]
        {
            get { return activeProviders.GetOrAdd(key, new KeysProvider(key)); }
        }

        public Provider this[KState.Special key]
        {
            get { return specialProviders.GetOrAdd(key, new SpecialProvider(key)); }
        }

        // add new Providers
        public void AddProvider(Keys key)
        {
            // if (activeProviders == null) activeProviders = new ConcurrentDictionary<Keys, Provider>();
            activeProviders.TryAdd(key, new KeysProvider(key));
        }

        public void AddProvider(KState.Special key)
        {
            // if (specialProviders == null) specialProviders = new ConcurrentDictionary<KState.Special, Provider>();
            specialProviders.TryAdd(key, new SpecialProvider(key));
        }

        public void UpdateAll()
        {
            if (activeProviders != null) {
                foreach (var kvp in activeProviders)
                    { kvp.Value.UpdateSubscribers(); }}

            if (specialProviders != null) {
                foreach (var kvp in specialProviders)
                    { kvp.Value.UpdateSubscribers(); }}
        }

        public abstract class Provider
        {
            protected ConcurrentDictionary<KeyEventProvider.Event, ConcurrentBag<Action>> subscribers;

            protected Provider()
            {
                subscribers = new ConcurrentDictionary<KeyEventProvider.Event, ConcurrentBag<Action>>();

                subscribers.TryAdd(KeyEventProvider.Event.Pressed,  new ConcurrentBag<Action>());
                subscribers.TryAdd(KeyEventProvider.Event.Released, new ConcurrentBag<Action>());
            }

            public virtual void Add(KeyEventProvider.Event ev, Action handler)
            {
                    subscribers[ev].Add(handler);
            }

            // public virtual void Remove(KeyEventProvider.Event ev, Action handler)
            // {
            //         subscribers[ev].TryTake(out handler);
            //         subscriberCount--;
            //
            // }

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
                Action handler;
                if (_key.Pressed())
                    while (!subscribers[KeyEventProvider.Event.Pressed].IsEmpty ){
                        subscribers[KeyEventProvider.Event.Pressed].TryTake(out handler);
                        handler();
                    }

                if (_key.Released())
                    while (!subscribers[KeyEventProvider.Event.Released].IsEmpty ){
                        subscribers[KeyEventProvider.Event.Released].TryTake(out handler);
                        handler();
                    }
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
                Action handler;
                if (_key.Pressed() && _key.Down()){
                while (!subscribers[KeyEventProvider.Event.Pressed].IsEmpty ){
                    if (subscribers[KeyEventProvider.Event.Pressed].TryTake(out handler))
                        handler();
                }}

                if (_key.Released() && _key.Up()){
                while (!subscribers[KeyEventProvider.Event.Released].IsEmpty ){
                    if (subscribers[KeyEventProvider.Event.Released].TryTake(out handler))
                        handler();
                }}
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

        // public void Unsubscribe()
        // {
        //     IHBase.KEP[key].Remove( evType, onKeyEvent);
        // }
    }

}
