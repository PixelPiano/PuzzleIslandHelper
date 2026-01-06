using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/Passengers/Author")]
    [Tracked]
    public class Author : VertexPassenger
    {
        public static FlagList IntroOneWatched = new FlagList("AuthorIntroWatched");
        public Author(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id, 16, 20, new(1), new(-1, 1), 1.1f)
        {
            HasGravity = false;
            AddTriangle(new(1.6f, 8), new(8f, 0), new(14.5f, 5.6f), 1, Vector2.One, new(1, Ease.Linear, Color.SpringGreen, Color.Lime, Color.LimeGreen));
            AddTriangle(new(8f, 10f), new(16, 6), new(16, 11), 0.6f, Vector2.One * 0.6f, new(0.8f, Ease.Linear, Color.Green, Color.ForestGreen, Color.LawnGreen));
            AddTriangle(new(8f, 12), new(14.5f, 12), new(12.5f, 20), 0.2f, Vector2.One * 0.2f, new(0.5f, Ease.Linear, Color.DarkGreen, Color.DarkOliveGreen, Color.DarkSeaGreen));
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (IntroOneWatched)
            {
                Bake();
                if (Marker.TryFind("authorDefault", out var position))
                {
                    X = position.X;
                    Facing = Facings.Right;
                }
            }
            else
            {
                Bake();
                if (Marker.TryFind("authorWait", out var position))
                {
                    X = position.X;
                    Facing = Facings.Left;
                }
            }

        }
    }
}
