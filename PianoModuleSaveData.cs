using Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public class PianoModuleSaveData : EverestModuleSaveData
    {
        public bool PlayerFixedWarpDisplay { get; set; }
        public bool PlayerHasWarpDisplayPart { get; set; }
        public PlayerCalidus.CalidusInventory CalidusInventory { get; set; }
        public Dictionary<string, bool> Achievements = new();
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