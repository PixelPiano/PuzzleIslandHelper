using Celeste.Mod.PuzzleIslandHelper.Entities;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public struct CounterData
    {
        public CounterData(string flag = "", bool ignore = false)
        {
            Flag = flag;
            Ignore = ignore;
        }
        public string Flag;
        public bool Ignore;
        public int Increment(int? mod = null) => Flag.IncrementCounter(mod);
        public int Decrement(int? mod = null) => Flag.DecrementCounter(mod);
        public int Value
        {
            get
            {
                return Ignore || string.IsNullOrEmpty(Flag) || Engine.Scene is not Level level ? 0 : level.Session.GetCounter(Flag);
            }
            set
            {
                if (!string.IsNullOrEmpty(Flag) && Engine.Scene is Level level)
                {
                    level.Session.SetCounter(Flag, value);
                }
            }
        }
    }
}
