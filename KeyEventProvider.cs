using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        // private ConcurrentDictionary< Keys, Notifier > activeProviders;
        // private Dictionary< Keys, bool > acPrevState;

        private ConcurrentDictionary< KState.Special, Notifier > specialProviders;
        private Dictionary< KState.Special, bool > spPrevState;

        // initializer
        public KeyEventProvider()
        {
            // activeProviders  = new ConcurrentDictionary<Keys, Provider>();
            specialProviders = new ConcurrentDictionary<KState.Special, Notifier>();
            spPrevState = new Dictionary<KState.Special, bool>();
        }

        //indexers
        // public Provider this[Keys key]
        // {
        //     get { return activeProviders.GetOrAdd(key, new KeysProvider(key)); }
        // }

        public Notifier this[KState.Special key]
        {
            get { return specialProviders.GetOrAdd(key, new Notifier()); }
        }

        // add new Providers
        // public void AddProvider(Keys key)
        // {
        //     activeProviders.TryAdd(key, new Notifier());
        // }

        public void AddProvider(KState.Special key)
        {
            specialProviders.TryAdd(key, new Notifier());
        }

        //call this from one of the "Updates-every-frame" hooks to send events to listeners (e.g. buttons) on a key change
        public void Update()
        {
            // if (activeProviders != null) {
            //     foreach (var kvp in activeProviders)
            //         { kvp.Value.UpdateSubscribers(); }}

            bool wasPressed;
            bool isPressed;
            foreach (var kvp in specialProviders)
                {
                    isPressed = kvp.Key.Down();
                    spPrevState.TryGetValue(kvp.Key, out wasPressed);
                    if (wasPressed!=isPressed) kvp.Value.NotifySubscribers(isPressed); //only update on change (pressed/released)
                    spPrevState[kvp.Key]=isPressed;
                }
        }

        public class Notifier
        {
            protected ConcurrentDictionary<KeyEventProvider.Event, ConcurrentBag<Action>> subscribers;

            public Notifier()
            {
                subscribers = new ConcurrentDictionary<KeyEventProvider.Event, ConcurrentBag<Action>>();

                subscribers.TryAdd(KeyEventProvider.Event.Pressed,  new ConcurrentBag<Action>());
                subscribers.TryAdd(KeyEventProvider.Event.Released, new ConcurrentBag<Action>());
            }

            public virtual void Add(KeyEventProvider.Event ev, Action handler)
            {
                    subscribers[ev].Add(handler);
            }

            //@param keyPressed will be true if the key has changed from up to down, false if down to up
            public virtual void NotifySubscribers(bool keyPressed)
            {
                Action handler;
                if (keyPressed)
                {
                    while (!subscribers[KeyEventProvider.Event.Pressed].IsEmpty ){
                        if (subscribers[KeyEventProvider.Event.Pressed].TryTake(out handler)) handler();
                    }
                }

                else
                {
                    while (!subscribers[KeyEventProvider.Event.Released].IsEmpty ){
                        if (subscribers[KeyEventProvider.Event.Released].TryTake(out handler)) handler();
                    }
                }
            }
        }
    }

    //only accepts KState.Special at the moment
    public class KeyWatcher
    {
        private readonly KState.Special key;
        private readonly KeyEventProvider.Event evType;
        private Action onKeyEvent;

        // allows for setting (just once!) after the watcher is created
        public Action OnKeyEvent {
            set { if (onKeyEvent==null) onKeyEvent = value; } }

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

        // public void Unsubscribe()  // done automatically due to ConcurrentBag
        // {
        //     IHBase.KEP[key].Remove( evType, onKeyEvent);
        // }
    }

}
