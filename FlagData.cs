using Celeste.Mod.PuzzleIslandHelper.Entities;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public struct FlagData
    {
        public static implicit operator bool(FlagData value) => value.State;
        public static implicit operator FlagData(string s) => new(s);
        public string Flag = "";
        public bool Inverted;
        public bool Ignore;
        public readonly bool Empty => string.IsNullOrEmpty(Flag);
        public readonly bool State
        {
            get => Ignore || Empty ? !Inverted : Engine.Scene is Level level && level.Session.GetFlag(Flag) != Inverted;
            set => Flag.SetFlag(value);
        }
        public FlagData(string flag)
        {
            if (flag != null)
            {
                if (flag.Length > 1 && flag[0] == '!')
                {
                    Inverted = true;
                    Flag = flag[1..];
                }
                else
                {
                    Flag = flag;
                }
            }
        }
        public FlagData(string flag, bool inverted)
        {
            Flag = flag;
            Inverted = inverted;
        }
        public FlagData(string flag, bool inverted, bool ignore)
        {
            Flag = flag;
            Inverted = inverted;
            Ignore = ignore;
        }
        public readonly bool GetState(Scene scene)
        {
            return Ignore || Empty ? !Inverted : (scene as Level).Session.GetFlag(Flag) != Inverted;
        }
    }
}
