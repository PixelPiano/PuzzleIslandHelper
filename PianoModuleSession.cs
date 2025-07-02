using Celeste.Mod.PuzzleIslandHelper.Cutscenes.Prologue;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP.EscapeRoomEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using System;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using YamlDotNet.Core.Tokens;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public enum LabPowerState
    {
        Backup,
        Barely,
        Restored
    }
    public class PianoModuleSession : EverestModuleSession
    {
        public bool DEBUG { get; set; }
        public bool HasPiano { get; set; }
        public Dictionary<string, List<int>> DestroyedVanillaSpinnerIDs = [];
        public Dictionary<string, List<int>> DestroyedCustomSpinnerIDs = [];
        public Dictionary<EntityID, List<int>> TiletypePuzzleCache = new();
        public Dictionary<EntityID, int> CrystalElevatorFurthestLevelReached = new();
        public HeartInventory HeartInventory = new();
        public Dictionary<EntityID, (string, string)> UsedHeartMachines = new();

        public List<int> RuneNodes = [0, 2, 5];
        public List<EntityID> BathroomStallsOpen = new();
        public bool BathroomStallOpened { get; set; }
        public int TimesUsedCapsuleWarpWithRunes { get; set; }
        public int TimesUsedCapsuleWarp { get; set; }
        public List<EntityID> CollectedFirfilIDs = new();
        public List<string> VoidLampGroups = new();
        public Dictionary<string, Vector2> PortalNodePositions = new();
        public Dictionary<EntityID, string> PersistentWarpLinks = new();
        public Dictionary<string, bool> LoggedCapsules = new();
        public bool DEBUGBOOL1 { get; set; }
        public int DEBUGINT { get; set; }
        public bool DEBUGBOOL2 { get; set; }
        public bool DEBUGBOOL3 { get; set; }
        public bool DEBUGBOOL4 { get; set; }
        public float DEBUGFLOAT1 { get; set; }
        public Vector2 DEBUGVECTOR { get; set; }
        public string DEBUGSTRING { get; set; }
        public List<RecordedMemory> Memories = new();
        public int TimesMetWithCalidus;
        public AltCalidus.AltCalidusScene.States AltCalidusSceneState;
        public ForkAmpState ForkAmpState = new();
        public bool ModeratorEscape;
        public bool MonitorActivated;
        public string CurrentAreaFlag;
        public string CurrentBackdropArea;
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
        public bool PipeSwitched;
        public bool PipesFixed;
        public bool PipesFixable;
        public bool PipesBroken;
        public bool PipesSafe => !PipesBroken || PipesFixed;
        public void SetPipeState(int state)
        {
            if (state > 3)
            {
                PipesBroken = true;
                PipesFixable = false;
                PipesFixed = true;

            }
            else if (state > 2)
            {
                PipesBroken = true;
                PipesFixable = true;
                PipesFixed = false;
            }
            else if (state > 1)
            {
                PipesBroken = true;
                PipesFixable = false;
                PipesFixed = false;
            }
            else
            {
                PipesBroken = false;
                PipesFixable = false;
                PipesFixed = false;
                ResetPipeScrew();
            }
        }
        public int GetPipeState()
        {
            if (PipesFixed)
            {
                return 4;
            }
            else if (PipesFixable)
            {
                return 3;
            }
            else if (PipesBroken)
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
        public EntityID ActiveTransition { get; set; }
        public Interface Interface { get; set; }
        public BubbleParticleSystem BubbleSystem { get; set; }
        public float MaxDarkness = 0.5f;
        public float MinDarkness = 0.05f;
        public bool ThisTimeForSure { get; set; }
        public int ButtonsPressed { get; set; }
        public LabPowerState PowerState
        {
            get => actualPowerState;
            set
            {
                actualPowerState = value;
                if (value is LabPowerState.Restored)
                {
                    TimesMetWithCalidus = Math.Min(1, TimesMetWithCalidus);
                }
                UpdatePowerStateFlags(Engine.Scene);
            }
        }
        private LabPowerState actualPowerState;
        public void UpdatePowerStateFlags(Scene scene)
        {
            foreach (LabPowerState state in Enum.GetValuesAsUnderlyingType<LabPowerState>())
            {
                (scene as Level).Session.SetFlag("Power:" + state.ToString(), actualPowerState == state); //store the state in a set of flags so it can be accessed anywhere
            }

        }
        public bool RestoredPower
        {
            get => PowerState == LabPowerState.Restored;
            set => PowerState = LabPowerState.Restored;
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
