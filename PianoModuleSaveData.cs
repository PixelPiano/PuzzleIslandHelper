using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public class PianoModuleSaveData : EverestModuleSaveData 
    {
        public bool HasInvert;
        public bool HasArtifact;
        public bool HasClearance;
        public bool Escaped;
        public List<int> UsedCutscenes = new();
        public Dictionary<EntityID,Vector2> PressedTSwitches = new();
        public List<string> BrokenPillars = new();
        public int PillarBlockState;
        public List<DashCodeCollectable> CollectedIDs = new();
    }
}