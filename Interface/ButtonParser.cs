using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TAPI;
using Terraria;
using LitJson;

namespace InvisibleHand
{
    public static class ButtonParser
    {

        private static Dictionary<String,Action> availableActions;

        static ButtonParser()
        {
            availableActions = new Dictionary<String,Action>();
        }

        public static void RegisterAction(String n, Action a){
            availableActions.Add(n,a);
        }

        public static void RegisterActions(params IEnumerable<KeyValuePair<String, Action>> aPairs)
        {
            foreach (var kvp in aPairs){
                RegisterAction(kvp.Key, kvp.Value);
            }
        }

        public static Action GetAction(string name)
        {
            return availableActions[name];
        }
    }

    
}
