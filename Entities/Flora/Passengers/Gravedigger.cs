using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/Passengers/Gravedigger")]
    [Tracked]
    public class Gravedigger : VertexPassenger
    {
        public Gravedigger(EntityData data, Vector2 offset) : base(data.Position + offset, 12, 20, data.Attr("cutsceneID"), data.Attr("dialog"), new(12, 20), new(-1, 1), 0.95f)
        {
            MinWiggleTime = 1;
            MaxWiggleTime = 2.5f;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Bake();
        }
    }
}
