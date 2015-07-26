using System;
using System.Collections.Generic;
using Terraria;

namespace InvisibleHand
{
    public enum Sound
    {
        // just because it causes issues when there's no value for 0
        // in an enum; 0 is the dig sound, but this mod doesn't use it
        Dig       =  0,
        ItemMoved =  7,
        MouseOver = 12,
        Coins     = 18,
        Lock      = 22
    }

    public static class SoundManager
    {
        private static HashSet<Sound> soundQ;

        static SoundManager()
        {
            soundQ = new HashSet<Sound>();
        }

        /// Add a sound to list of sounds to play when the
        /// PlayQueuedSounds() method is called. If the same
        /// type of sound is already in the Queue, it will
        /// not be added a second time.
        public static void Queue(Sound toPlay)
        {
            soundQ.Add(toPlay);
        }

        public static void ClearQueue()
        {
            if (soundQ.Count > 0)
                soundQ.Clear();
        }

        /// play queued sounds (though not in any particular order)
        public static void PlayQueuedSounds()
        {
            if (soundQ.Count > 0)
            {
                foreach (Sound s in soundQ)
                {
                    // not sending extra parameters because none of
                    // the sound FX in this mod use them
                    Main.PlaySound((int)s);
                    // s.Play();
                }
                soundQ.Clear();
            }
        }
    }
}
