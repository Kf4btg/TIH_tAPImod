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

        // public Dictionary<KState.Special,List<Action>> Subscribers;
        public Dictionary<KeyEventProvider.Event, List<Action>> shiftSubscribers;
        // public Dictionary<String, List<Action>> CtrlSubscribers;
        // public Dictionary<String, List<Action>> AltSubscribers;
        // public Dictionary<String, List<Action>> AnySubscribers;


        public KeyEventProvider()
        {
            shiftSubscribers = new Dictionary<KeyEventProvider.Event, List<Action>>();

            shiftSubscribers.Add(KeyEventProvider.Event.Pressed, new List<Action>());
            shiftSubscribers.Add(KeyEventProvider.Event.Released, new List<Action>());
            // Subscribers.Add(KState.Special.Ctrl, new List<Action>());
            // Subscribers.Add(KState.Special.Alt, new List<Action>());
            // Subscribers.Add(KState.Special.Any, new List<Action>());

        }

        public void Add(KState.Special key, KeyEventProvider.Event ev, Action handler)
        {
            switch(key)
            {
                case KState.Special.Shift:
                    shiftSubscribers[ev].Add(handler);
                    break;
            }
        }

        public void Remove(KState.Special key, KeyEventProvider.Event ev, Action handler)
        {
            switch(key)
            {
                case KState.Special.Shift:
                    shiftSubscribers[ev].Remove(handler);
                break;
            }
        }
        
        //call this from one of the "Updates-every-frame" hooks to send events to listeners (e.g. buttons) on a key change
        public void UpdateSubscribers()
        {
            // foreach (var kvp in shiftSubscribers)
            // {

            if (KState.Special.Shift.Pressed())
            {
                foreach (Action handler in shiftSubscribers[KeyEventProvider.Event.Pressed] )
                {
                    handler();
                }
            }
            if (KState.Special.Shift.Released())
            {
                foreach (Action handler in shiftSubscribers[KeyEventProvider.Event.Released] )
                {
                    handler();
                }
            }
            // }
        }
    }

}
