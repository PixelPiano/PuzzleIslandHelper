using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.PianoEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.EscapeRoomEntities;
using IL.Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue;
using Celeste.Mod.PuzzleIslandHelper.Triggers;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public class PianoModuleSaveData : EverestModuleSaveData
    {
        public bool DrillUsed;
        public List<string> DrillBatteryIds = new();
        public bool HasFirstFloppy;
        public bool FountainCanOpen;
        public bool ForceFountainOpen;
        public int LastElevatorLevel;
        public bool OpenedFountain;
        public Dictionary<EntityID, bool> MiniGenStates = new();
        public List<EntityID> SpoutWreckage = new();
        public List<int> FixedFloors = new();
        public List<string> ContinuousCogIDs = new();
        public CogData CogData = new();
        public Dictionary<EntityID, float> CogDoorStates = new();
        public int WasherSwitchAttempts;
        public List<PrologueGlitchBlock> ActiveGlitchBlocks = new();
        public List<FloppyDisk> CollectedDisks = new();
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
        public bool TryAddDisk(FloppyDisk disk)
        {
            if (CollectedDisks.Find(item => item.Preset == disk.Preset) == null)
            {
                CollectedDisks.Add(disk);
                return true;
            }
            return false;
        }
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
                ResetPipeScrew();
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

        #region Game of Life
        public List<bool[,]> LifeGrids = new();
        private int gridIndex;
        public bool[,] GetLifeGrid()
        {
            if (LifeGrids.Count == 0)
            {
                return null;
            }
            int index = gridIndex;
            gridIndex++;
            if (gridIndex > LifeGrids.Count - 1)
            {
                gridIndex = 0;
            }
            return LifeGrids[index];
        }
        public void AddLifeGrid(bool[,] lifeGrid)
        {
            LifeGrids.Add((bool[,])lifeGrid.Clone());
            if (LifeGrids.Count > 5)
            {
                LifeGrids.RemoveAt(0);
            }
        }
        #endregion

        public bool HasInvert;
        public List<string> ChainedMonitorsActivated = new();
        public bool GeneratorStarted;
        public bool HasArtifact;
        public bool HasClearance;
        public bool Escaped;
        public List<string> UsedCutscenes = new();
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
            PipeScrew screw = (PipeScrew)Monocle.Engine.Scene.Tracker.GetEntities<PipeScrew>().Find(item => (item as PipeScrew).UsedInCutscene);
            if (screw is not null)
            {
                screw.Launched = false;
                screw.Position = screw.originalPosition;
                PianoModule.SaveData.PipeScrewRestingPoint = null;
                PianoModule.SaveData.PipeScrewRestingFrame = 0;
                PianoModule.SaveData.PipeScrewLaunched = false;
                screw.Screw.Play("idle");
            }
        }
        #endregion
    }
}