using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities;
using Celeste.Mod.PuzzleIslandHelper.PuzzleData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public class PianoModuleSession : EverestModuleSession
    {
        public ChargedWater CutsceneWater;
        public List<PipeSpout> CutsceneSpouts = new List<PipeSpout>();
        public EntityID ActiveTransition { get; set; }
        public Interface Interface { get; set; }
        public BubbleParticleSystem BubbleSystem { get; set; }
        public float MaxDarkness = 0.5f;
        public float MinDarkness = 0.05f;
        public bool ThisTimeForSure { get; set; }
        public int ButtonsPressed { get; set; }
        public bool RestoredPower { get; set; }
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
