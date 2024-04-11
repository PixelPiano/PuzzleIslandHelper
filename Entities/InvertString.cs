using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using MonoMod.Utils;
// PuzzleIslandHelper.FloatingCobweb
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/InvertString")]
    [TrackedAs(typeof(Cobweb))]
    public class InvertString : FloatingCobweb
    {
        public InvertString(EntityData data, Vector2 offset) : base(data.Position, offset, data.Nodes, Color.White)
        {
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Visible = InvertOverlay.State;
        }
        public override void Update()
        {
            base.Update();
            Visible = InvertOverlay.State;
        }
    }
}
