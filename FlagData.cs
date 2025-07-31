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
        public bool FalseIfEmpty;
        public bool? ForcedValue;
        public readonly bool Empty => string.IsNullOrEmpty(Flag);
        public readonly bool State
        {
            get => GetState(Engine.Scene);
            set => Flag.SetFlag(value);
        }
        public void SetState(bool value)
        {
            State = value;
        }
        public FlagData(string flag)
        {
            if (!string.IsNullOrEmpty(flag))
            {
                if (flag.StartsWith('!'))
                {
                    Inverted = !Inverted;
                    Flag = flag[1..];
                }
                else
                {
                    Flag = flag;
                }
            }
        }
        public FlagData(string flag, bool inverted) : this(flag)
        {
            Inverted = inverted;
        }
        public FlagData(string flag, bool inverted, bool ignore) : this(flag, inverted)
        {
            Ignore = ignore;
        }
        public override string ToString()
        {
            return "{Flag:" + Flag + "=" + State + "}";
        }
        public readonly bool GetState(Scene scene) => scene != null && scene is Level level && GetState(level);
        public readonly bool GetState(Level scene)
        {
            return ForcedValue ?? (Ignore || Empty ? !FalseIfEmpty && !Inverted : scene.Session.GetFlag(Flag) != Inverted);
        }
    }
}
