using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.Mod.PuzzleIslandHelper.Entities.WARP.WARPData;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WARP
{
    public class WarpRune
    {
        public string Pattern;
        public class RuneNodeInventory
        {
            public static RuneNodeInventory Second = new RuneNodeInventory(ProgressionSets.Second);
            public static RuneNodeInventory First = new RuneNodeInventory(ProgressionSets.First);
            public static RuneNodeInventory Default = new RuneNodeInventory(ProgressionSets.Default);
            public enum ProgressionSets
            {
                //Default:
                //  0   0   0
                //0   1   1   0
                //  0   0   0
                Default,
                //After Tutorial:
                //  0   1   0
                //0   1   1   0
                //  0   1   0
                First,
                //After Freeing Calidus:
                //  1   1   1
                //1   1   1   1
                //  1   1   1
                Second
            }
            public bool[] IsObtained = new bool[10];
            public bool HasNode(NodeTypes node)
            {
                return IsObtained[(int)node];
            }
            public bool HasNodes(params NodeTypes[] types)
            {
                bool result = true;
                foreach (NodeTypes t in types)
                {
                    result &= IsObtained[(int)t];
                }
                return result;
            }
            [Command("node_set", "change the node set the player has access to")]
            public static void SetProgress(int i = 0)
            {
                PianoModule.SaveData.SetRuneProgression((ProgressionSets)i);
            }

            public RuneNodeInventory()
            {
            }
            public RuneNodeInventory(ProgressionSets set)
            {
                Set(set);
            }
            public void Set(ProgressionSets set)
            {
                for (int i = 0; i < IsObtained.Length; i++)
                {
                    IsObtained[i] = false;
                }
                List<NodeTypes> types = [];
                switch (set)
                {
                    case ProgressionSets.Default:
                        types = [NodeTypes.ML, NodeTypes.MR];
                        break;

                    case ProgressionSets.First:
                    case ProgressionSets.Second:
                        types = [.. Enum.GetValues<NodeTypes>()];
                        break;
                }
                if (set == ProgressionSets.First)
                {
                    types.Remove(NodeTypes.ML);
                    types.Remove(NodeTypes.MR);
                }
                foreach (NodeTypes t in types)
                {
                    IsObtained[(int)t] = true;
                }
            }
        }
        public static Dictionary<string, string> ReplaceAssumed = new()
            {
                {"02","0112"},
                {"08","0448"},
                {"17","1447"},
                {"19","1559"},
                {"28","2558"},
                {"35","3445"},
                {"46","4556"},
                {"36","344556"},
                {"79","7889"},
                {"20","0112"},
                {"80","0448"},
                {"71","1447"},
                {"91","1559"},
                {"82","2558"},
                {"53","3445"},
                {"64","4556"},
                {"63","344556"},
                {"97","7889"},
            };
        public static WarpRune Default;
        public string ID;
        public List<(int, int)> Segments = new();
        [Command("dr", "s")]
        public static void WriteDefaultRunes()
        {
            foreach (WarpRune r in DefaultRunes)
            {
                Engine.Commands.Log(r.ToString());
            }
        }
        [Command("wr", "s")]
        public static void WriteAllRunes()
        {
            foreach (WarpRune r in DefaultRunes)
            {
                Engine.Commands.Log(r.ToString());
            }
        }
        public static List<(int, int)> GetSortedPattern(string pattern)
        {
            //split the pattern into groups of 2
            var split = pattern.Replace(" ", "").Segment(2, false);

            //replace connections that overlap additional nodes
            string newString = "";
            foreach (string s in split)
            {
                ReplaceAssumed.TryGetValue(s, out string value);
                newString += !string.IsNullOrEmpty(value) ? value : s;
            }
            //transform the new pattern into groups of (int, int) tuples
            var normalized = newString.Segment(2, false).Select(item => (item[0] - '0', item[1] - '0')).ToList();

            //sort each tuple value group from lowest to highest
            //eg.   10 -> 01   75 -> 57
            for (int i = 0; i < normalized.Count; i++)
            {
                (int, int) t = normalized[i];
                if (t.Item1 > t.Item2)
                {
                    normalized[i] = (t.Item2, t.Item1);
                }
            }
            //order tuple list by Item1, then Item2
            //eg.   01, 24, 04, 28, 89, 79, 02     ->      01, 02, 04, 24, 28, 79, 89
            var ordered = normalized.OrderBy(item => item.Item1).ThenBy(item => item.Item2);

            //return the list with any duplicates removed
            return ordered.Distinct().ToList();
        }
        public WarpRune(string id, string pattern) : this(id, GetSortedPattern(pattern))
        {
        }
        public WarpRune(string id, List<(int, int)> sequence)
        {
            Segments = sequence;
            ID = id;
        }
        public static List<Fragment> ToFragments(WarpRune rune)
        {
            List<Fragment> fragments = [];
            foreach (var a in rune.Segments)
            {
                fragments.Add(new(rune.ID, (NodeTypes)a.Item1, (NodeTypes)a.Item2));
            }
            return fragments;
        }
        public static string GetSortedPattern(UI.ConnectionList list)
        {
            return list.ToString();
        }
        public override string ToString()
        {
            return "{" + string.Join(' ', Segments.Select(item => item.Item1.ToString() + item.Item2.ToString())) + "}";
        }
        public bool Match(WarpRune rune)
        {
            return rune.ToString() == ToString();
        }
        [OnUnload]
        public static void Unload()
        {
            ClearRunes();
        }
        public static void ClearRunes()
        {
            Default = null;
            DefaultRunes.Clear();
            AllRunes.Clear();
        }
    }
}
