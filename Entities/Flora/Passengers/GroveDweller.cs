using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/Passengers/GroveDweller")]
    [Tracked]
    public class GroveDweller : VertexPassenger
    {
        public GroveDweller(EntityData data, Vector2 offset) : base(data.Position + offset, 12, 20, data.Attr("cutsceneID"), new(12, 20), new(-1, 1), 0.95f)
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
