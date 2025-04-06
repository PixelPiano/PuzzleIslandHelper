using Celeste.Mod.PuzzleIslandHelper.Entities;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public struct FlagData
    {
        public FlagData(string flag = "", bool inverted = false, bool ignore = false)
        {
            Flag = flag;
            Inverted = inverted;
            Ignore = ignore;
        }
        public string Flag;
        public bool Inverted;
        public bool Ignore;
        public bool State
        {
            get
            {
                return Ignore || (string.IsNullOrEmpty(Flag) ? !Inverted :
                Engine.Scene is Level level && level.Session.GetFlag(Flag) != Inverted);
            }
            set
            {
                if (!string.IsNullOrEmpty(Flag) && Engine.Scene is Level level)
                {
                    level.Session.SetFlag(Flag, value);
                }
            }
        }
    }
}
