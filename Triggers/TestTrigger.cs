
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    [CustomEntity("PuzzleIslandHelper/TestTrigger")]
    [Tracked]
    public class TestTrigger : Trigger
    {
        public ShaderOverlay Overlay;
        public TestTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Overlay = new ShaderOverlay("PuzzleIslandHelper/Shaders/fuzzyNoise", "", true);
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Scene.Add(Overlay);
            Overlay.ForceLevelRender = true;
            Overlay.Amplitude = 1;
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            Scene.Remove(Overlay);
        }
    }
}
