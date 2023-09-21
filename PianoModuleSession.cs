using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public class PianoModuleSession : EverestModuleSession
    {
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
