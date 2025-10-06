using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/Passengers/Civilian")]
    [Tracked]
    public class NormalPassenger : VertexPassenger
    {
        public NormalPassenger(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id, 16, 20, new(1), new(-1, 1), 1.1f)
        {
            AddTriangle(new(1.6f, 8), new(8f, 0), new(14.5f, 5.6f), 1, Vector2.One, new(1, Ease.Linear, Color.SpringGreen, Color.Lime, Color.LimeGreen));
            AddTriangle(new(8f, 10f), new(16, 6), new(16, 11), 0.6f, Vector2.One * 0.6f, new(0.8f, Ease.Linear, Color.Green, Color.ForestGreen, Color.LawnGreen));
            AddTriangle(new(8f, 12), new(14.5f, 12), new(12.5f, 20), 0.2f, Vector2.One * 0.2f, new(0.5f, Ease.Linear, Color.DarkGreen, Color.DarkOliveGreen, Color.DarkSeaGreen));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Bake();
        }
    }
}
