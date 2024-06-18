using Celeste.Mod.PuzzleIslandHelper.Cutscenes.Prologue;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities.GearEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.PuzzleEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP.EscapeRoomEntities;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities;
using Celeste.Mod.PuzzleIslandHelper.Components;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public class PianoModuleSession : EverestModuleSession
    {
        public int DEBUGINT { get; set; }
        public bool DEBUGBOOL { get; set; }

        public bool MetWithCalidusFirstTime;
        public AltCalidus.AltCalidusScene.States AltCalidusSceneState;
        public ForkAmpState ForkAmpState = new();
        public bool GrassMazeCompleted;
        public bool ModeratorEscape;
        public bool MonitorActivated;
        public string CurrentAreaFlag;
        public SceneSwitch.Areas CurrentBackdropArea;
        public bool FixedElevator;
        public int FurthestElevatorLevel;
        public int OrderPoints { get; set; }
        public bool DrillUsed;
        public List<string> DrillBatteryIds = new();
        public bool HasFirstFloppy;
        public bool FountainCanOpen;
        public bool ForceFountainOpen;
        public Dictionary<EntityID, bool> MiniGenStates = new();
        public List<EntityID> SpoutWreckage = new();
        public List<int> FixedFloors = new();
        public List<string> GearCheckpointIDs = new();
        public List<string> ContinuousGearIDs = new();
        public GearData GearData = new();
        public int GameshowLivesLost;
        public HoldableData HoldableData = new();
        public List<string> HoldableGroupIDs = new();
        public List<string> HoldableCheckpointIDs = new();

        public Dictionary<EntityID, float> GearDoorStates = new();
        public int WasherSwitchAttempts;
        public List<PrologueGlitchBlock> ActiveGlitchBlocks = new();
        public List<FloppyDisk> CollectedDisks = new();

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

        public ForkAmpBattery LastHeld;
        public bool HasInvert;
        public List<string> ChainedMonitorsActivated = new();
        public bool GeneratorStarted;
        public bool GrassMazeFinished;
        public bool HasArtifact;
        public bool HasClearance;
        public bool Escaped;
        public List<string> UsedCutscenes = new();
        public Dictionary<EntityID, Vector2> PressedTSwitches = new();
        public List<string> BrokenPillars = new();
        public int PillarBlockState;
        public List<DashCodeCollectable> CollectedIDs = new();

        public bool PipeScrewLaunched;
        public Vector2? PipeScrewRestingPoint;
        public int PipeScrewRestingFrame;
        public void ResetPipeScrew()
        {
            PipeScrew screw = (PipeScrew)Engine.Scene.Tracker.GetEntities<PipeScrew>().Find(item => (item as PipeScrew).UsedInCutscene);
            if (screw is not null)
            {
                screw.Launched = false;
                screw.Position = screw.originalPosition;
                PipeScrewRestingPoint = null;
                PipeScrewRestingFrame = 0;
                PipeScrewLaunched = false;
                screw.Screw.Play("idle");
            }
        }

        public ChargedWater CutsceneWater;
        public List<PipeSpout> CutsceneSpouts = new List<PipeSpout>();
        public EntityID ActiveTransition { get; set; }
        public Interface Interface { get; set; }
        public BubbleParticleSystem BubbleSystem { get; set; }
        public float MaxDarkness = 0.5f;
        public float MinDarkness = 0.05f;
        public bool ThisTimeForSure { get; set; }
        public int ButtonsPressed { get; set; }
        public bool RestoredPower
        {
            get
            {
                if (Engine.Scene is Level level)
                {
                    level.Session.SetFlag("RestoredPower", PianoModule.Session.GeneratorStarted);
                }
                return PianoModule.Session.GeneratorStarted;
            }
            set
            {
                if (Engine.Scene is Level level)
                {
                    level.Session.SetFlag("RestoredPower", value);
                }
                PianoModule.Session.GeneratorStarted = value;
            }
        }
        public Dictionary<EntityID, LHLData> BrokenLamps { get; set; } = new Dictionary<EntityID, LHLData>();
        public Effect MonitorShader { get; set; }
        public int GlitchBlocks { get; set; }
        public string CurrentPrompt { get; set; }
        public List<FadeWarpKey.KeyData> Keys { get; set; } = new();
        public Dictionary<string, List<LightsIcon.LightsIconData>> IconDictionary { get; set; } = new();
        public List<EntityID> DoorIds { get; set; } = new();
        public Dictionary<string, string> LevelMusic { get; set; } = new();
        public float InvertWaitTime = 1.5f;

        public float JumpMult { get; set; } = 1;
        public float ScaleMult { get; set; } = 1;
        public float CurrentScale { get; set; } = 1;
        public Vector2 SpeedMult = Vector2.One;

        public float PotionJumpMult { get; set; } = 1;
        public Vector2 PotionSpeedMult = Vector2.One;

        public List<Vector2> PotionTiles { get; set; } = new();
        public Dictionary<FluidBottle.Side, List<Vector2>> Tiles = new();
    }
}
