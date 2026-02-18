using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public struct FlagData
    {
        public static implicit operator bool(FlagData value) => value.State;
        public static implicit operator FlagData(string s) => new(s);
        public string Flag = "";
        public bool Inverted;
        public bool Ignore;
        public bool? ForcedValue;
        public readonly bool Empty => string.IsNullOrEmpty(Flag);
        public bool State
        {
            get => GetState(Engine.Scene);
            set
            {
                if (!Empty)
                {
                    Flag.SetFlag(value);
                }
            }
        }
        public void Set(bool value)
        {
            State = value;
        }
        public void Invert()
        {
            State = !State;
        }
        public FlagData(string flag)
        {
            if (!string.IsNullOrEmpty(flag))
            {
                if (flag.StartsWith('!'))
                {
                    if (flag.Length == 1)
                    {
                        ForcedValue = false;
                    }
                    else
                    {
                        Inverted = !Inverted;
                        Flag = flag[1..];
                    }
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
        public bool GetState(Scene scene)
        {
            if (scene == null || scene is not Level level) return false;
            if (ForcedValue.HasValue) return ForcedValue.Value;
            if (Ignore || Empty)
            {
                return !Inverted;
            }
            return level.Session.GetFlag(Flag) != Inverted;
        }
    }
}
