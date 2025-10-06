using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.PuzzleIslandHelper.Effects.TriangleField;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public struct FlagList : IEnumerable<FlagData>, IEnumerable
    {
        public int Count => List.Count;
        public List<FlagData> List = [];
        public string Flag;
        public bool Inverted;
        public bool Ignore;
        public bool? ForcedValue;
        public readonly bool Empty
        {
            get
            {
                if (!(List == null || List.Count == 0))
                {
                    foreach (var item in List)
                    {
                        if (!item.Empty) return false;
                    }
                }
                return true;
            }
        }
        public static bool operator true(FlagList list)
        {
            return list.State;
        }
        public static bool operator false(FlagList list)
        {
            return !list.State;
        }
        public static implicit operator bool(FlagList list)
        {
            return list.State;
        }
        public bool this[int i]
        {
            get => List.Count > i ? List[i] : default;
            set
            {
                if (List.Count > i)
                {
                    List[i].Set(value);
                }
            }
        }
        public bool State
        {
            get
            {
                if (ForcedValue.HasValue) return ForcedValue.Value;
                if (Ignore || Empty) return !Inverted;
                else
                {
                    foreach (FlagData data in List)
                    {
                        if (data.State == Inverted) return false;
                    }
                }
                return true;
            }
            set
            {
                foreach (FlagData data in List)
                {
                    data.Set(data.Inverted != value);
                }
            }
        }
        public override string ToString()
        {
            string output = "";
            foreach (var item in List)
            {
                output += item.ToString() + '\n';
            }
            output += "Count: " + Count;
            return output.TrimEnd('\n');
        }

        public IEnumerator<FlagData> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public FlagList(string[] flags, bool inverted = false)
        {
            Inverted = inverted;
            foreach (var item in flags)
            {
                if (item[0] == '!' && item.Length > 1)
                {
                    List.Add(new FlagData(item.Substring(1), true));
                }
                else
                {
                    List.Add(new FlagData(item, false));
                }
            }
        }
        public static string[] format(string input)
        {
            return input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
        public FlagList(string flags, bool inverted = false) : this(format(flags), inverted)
        {
        }
    }
}
