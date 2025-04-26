using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Entities.WARP;
namespace Celeste.Mod.PuzzleIslandHelper
{
    public class PianoModuleSaveData : EverestModuleSaveData
    {
        public InterfaceData InterfaceData;
        public int CalJrState { get; set; }
        public bool WarpLockedToLab { get; set; }
        public WarpRune.RuneNodeInventory.ProgressionSets RuneProgression = WarpRune.RuneNodeInventory.ProgressionSets.Second;
        public WarpRune.RuneNodeInventory RuneNodeInventory = new();
        public List<WarpRune> VisitedRuneSites = new();
        public PlayerCalidus.CalidusInventory CalidusInventory { get; set; }
        public Dictionary<string, bool> Achievements = new();

        public void SetRuneProgression(WarpRune.RuneNodeInventory.ProgressionSets set)
        {
            RuneProgression = set;
            RuneNodeInventory.Set(set);
        }
        public void GiveAchievement(string name)
        {
            SetAchievement(name, true);
        }
        public void SetAchievement(string name, bool value)
        {
            if (Achievements.ContainsKey(name))
            {
                Achievements[name] = value;
            }
            else
            {
                Achievements.Add(name, value);
            }
        }
        public enum Endings
        {
            Null,
            Reset,
            Recover,
            Eject,
            Duplicate,
            Inject,
        }
        public Dictionary<Endings, bool> EndingsSeen = new()
        {
            {Endings.Null,false},
            {Endings.Reset,false},
            {Endings.Recover,false},
            {Endings.Eject, false},
            {Endings.Duplicate,false},
            {Endings.Inject,false}
        };
    }
}