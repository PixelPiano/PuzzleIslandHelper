using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Entities.WARP;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using System;
namespace Celeste.Mod.PuzzleIslandHelper
{
    public class PianoModuleSaveData : EverestModuleSaveData
    {
        [Obsolete("Not used in Puzzle Island")]
        public PlayerCalidus.CalidusInventory CalidusInventory { get; set; }
        [Obsolete("Scrapped")]
        public int CalJrState { get; set; }
        public InterfaceData InterfaceData;
        public WarpRune.RuneNodeInventory RuneNodeInventory { get; set; } = WarpRune.RuneNodeInventory.Second;
        public List<WarpRune> VisitedRuneSites = new();
        public Dictionary<string, bool> Achievements = new();

        public WarpRune.RuneNodeInventory.ProgressionSets RuneProgression { get; set; } = WarpRune.RuneNodeInventory.ProgressionSets.Second;
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
            if (!Achievements.TryAdd(name, value))
            {
                Achievements[name] = value;
            }
        }
    }
}