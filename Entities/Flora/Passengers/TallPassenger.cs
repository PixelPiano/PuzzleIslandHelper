using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/Passengers/Tall")]
    [Tracked]
    public class TallPassenger : VertexPassenger
    {
        public TallPassenger(EntityData data, Vector2 offset) : base(data.Position + offset, 16, 32, data.Attr("cutsceneID"), new(1), new(-1, 1), 1.6f)
        {
            MinWiggleTime = 1;
            MaxWiggleTime = 2;
            AddTriangle(new(7, 0), new(16, 5), new(0, 8), 1, Vector2.One, new(1, Ease.Linear, Color.Green, Color.LawnGreen, Color.ForestGreen));
            AddTriangle(new(8, 9), new(16, 32), new(1, 32), 0.2f, Vector2.One * 0.2f, new(1, Ease.Linear, Color.Green, Color.LawnGreen, Color.ForestGreen));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Bake();
        }
    }
}
