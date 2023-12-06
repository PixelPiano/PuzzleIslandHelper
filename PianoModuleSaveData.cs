using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.PianoEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.EscapeRoomEntities;
using IL.Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public class PianoModuleSaveData : EverestModuleSaveData
    {
        #region Pipes
        public int PipeSwitchAttempts;
        public enum PipeStates
        {
            Untouched,
            Broken,
            Fixable,
            Fixed
        }
        public bool PipeSwitched;
        public bool HasFixedPipes;
        public bool CanFixPipes;
        public bool HasBrokenPipes;
        public void SetPipeState(int state)
        {
            if (state > 3)
            {
                HasBrokenPipes = true;
                CanFixPipes = false;
                HasFixedPipes = true;

            }
            else if (state > 2)
            {
                HasBrokenPipes = true;
                CanFixPipes = true;
                HasFixedPipes = false;
            }
            else if (state > 1)
            {
                HasBrokenPipes = true;
                CanFixPipes = false;
                HasFixedPipes = false;
            }
            else
            {
                HasBrokenPipes = false;
                CanFixPipes = false;
                HasFixedPipes = false;
            }
        }
         public int GetPipeState()
        {
            if (HasFixedPipes)
            {
                return 4;
            }
            else if (CanFixPipes)
            {
                return 3;
            }
            else if (HasBrokenPipes)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }
        #endregion

        #region Escape Room
        public EscapeInv EscapeInv;

        #endregion
        public bool HasInvert;
        public List<string> ChainedMonitorsActivated = new();
        public bool GeneratorStarted;
        public bool HasArtifact;
        public bool HasClearance;
        public bool Escaped;
        public List<int> UsedCutscenes = new();
        public Dictionary<EntityID, Vector2> PressedTSwitches = new();
        public List<string> BrokenPillars = new();
        public int PillarBlockState;
        public List<DashCodeCollectable> CollectedIDs = new();

        #region PipeScrew
        public bool PipeScrewLaunched;
        public Vector2? PipeScrewRestingPoint;
        public int PipeScrewRestingFrame;
        public void ResetPipeScrew()
        {
            PipeScrewLaunched = false;
            PipeScrewRestingFrame = 0;
            PipeScrewRestingPoint = null;
        }
        #endregion
    }
}