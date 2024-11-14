using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/Passengers/Civilian")]
    [Tracked]
    public class NormalPassenger : VertexPassenger
    {
        public NormalPassenger(EntityData data, Vector2 offset) : base(data.Position + offset, 16, 20, data.Attr("cutsceneID"), new(16, 20), new(-1, 1), 1.1f)
        {
            AddTriangle(new(0.1f, 0.4f), new(0.5f, 0), new(0.9f, 0.28f), 1, Vector2.One, new(1, Ease.Linear, Color.SpringGreen, Color.Lime, Color.LimeGreen));
            AddTriangle(new(0.5f), new(1, 0.3f), new(1, 0.55f), 0.6f, Vector2.One * 0.6f, new(0.8f, Ease.Linear, Color.Green, Color.ForestGreen, Color.LawnGreen));
            AddTriangle(new(0.55f, 0.6f), new(0.9f, 0.6f), new(0.8f, 1), 0.2f, Vector2.One * 0.2f, new(0.5f, Ease.Linear, Color.DarkGreen, Color.DarkOliveGreen, Color.DarkSeaGreen));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Bake();
        }
    }
}
