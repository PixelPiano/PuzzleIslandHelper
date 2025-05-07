using Celeste.Mod.PuzzleIslandHelper.Entities;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public struct FlagData(string flag = "", bool inverted = false, bool ignore = false)
    {
        public string Flag = flag;
        public bool Inverted = inverted;
        public bool Ignore = ignore;
        public readonly bool Empty => string.IsNullOrEmpty(Flag);
        public readonly bool State
        {
            get => Ignore || Empty ? !Inverted : Engine.Scene is Level level && level.Session.GetFlag(Flag) != Inverted;
            set => Flag.SetFlag(value);
        }
        public readonly bool GetState(Scene scene)
        {
            return Ignore || Empty ? !Inverted : (scene as Level).Session.GetFlag(Flag) != Inverted;
        }
    }
}
