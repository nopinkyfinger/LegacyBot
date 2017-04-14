using System;
using System.Collections.Generic;
using System.Timers;

namespace lulzbot
{
    public class Timers
    {
        private static Dictionary<String, Timer> timers = new Dictionary<String, Timer>();

        public static int Count
        {
            get
            {
                return timers.Count;
            }
        }

        public static String Add (int delay, ElapsedEventHandler action, bool repeat = false)
        {
            String id = Tools.md5(String.Format("{0}", Bot.EpochTimestampMS + (ulong)timers.Count));
            Timer t = new Timer(delay);
            t.Elapsed += action;
            if (!repeat)
                t.Elapsed += delegate
                {
                    t.Stop();
                    Remove(id);
                };

            lock (timers)
            {
                timers.Add(id, t);
                t.Start();
                return id;
            }
        }

        public static bool Remove (String id)
        {
            if (timers.ContainsKey(id))
            {
                lock (timers)
                {
                    timers[id].Dispose();
                    return timers.Remove(id);
                }
            }
            return false;
        }

        public static void Clear ()
        {
            lock (timers)
            {
                foreach (var T in timers)
                {
                    T.Value.Dispose();
                }
                timers.Clear();
            }
        }
    }
}
