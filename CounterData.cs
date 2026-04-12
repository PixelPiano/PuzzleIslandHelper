using Celeste.Mod.PuzzleIslandHelper.Entities;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public static class CounterExt
    {
        public static void Increment(this Session.Counter counter)
        {
            counter.Value++;
        }
        public static void Decrement(this Session.Counter counter)
        {
            counter.Value--;
        }
        public static void Set(this Session.Counter counter, int value)
        {
            counter.Value = value;
        }
        public static Session.Counter GetCounterObject(this Session session, string key)
        {
            foreach (Session.Counter counter in session.Counters)
            {
                if (counter.Key == key)
                {
                    return counter;
                }
            }
            Session.Counter newCounter = new Session.Counter() { Key = key };
            session.Counters.Add(newCounter);
            return newCounter;
        }
    }
    public struct CounterData
    {
        public CounterData(string counter = "", bool ignore = false)
        {
            Key = counter;
            Ignore = ignore;
        }
        public string Key;
        public bool Ignore;
        public Session.Counter Counter;
        public int Increment(int? mod = null) => Key.IncrementCounter(mod);
        public int Decrement(int? mod = null) => Key.DecrementCounter(mod);
        public int Value
        {
            get
            {
                return Ignore || string.IsNullOrEmpty(Key) || Engine.Scene is not Level level ? 0 : level.Session.GetCounter(Key);
            }
            set
            {
                if (!string.IsNullOrEmpty(Key) && Engine.Scene is Level level)
                {
                    level.Session.SetCounter(Key, value);
                }
            }
        }
    }
}
