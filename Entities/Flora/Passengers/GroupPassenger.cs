using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/Passengers/Group")]
    [Tracked]
    public class GroupPassenger : VertexPassenger
    {
        private int boxes = 3;
        public GroupPassenger(EntityData data, Vector2 offset) : base(data.Position + offset, (int)data.Float("groupWidth") * 3, 22, data.Attr("cutsceneID"), Vector2.One, new(0, 1), 1f)
        {
            boxes = data.Int("groups");
            int boxWidth = (int)data.Float("groupWidth");
            MinWiggleTime = 1;
            MaxWiggleTime = 1.4f;
            int leg = (int)(Height / 3 * 2);
            int w = boxWidth;
            int half = boxWidth / 2;
            for (int i = 0; i < boxes; i++)
            {
                int x = i * boxWidth;
                AddTriangle(new(x, 0), new(x + w - 1, leg), new(x, leg), 1, Vector2.One * 0.4f, new(1, Ease.Linear, Color.Green, Color.GreenYellow, Color.LawnGreen));
                AddTriangle(new(x + 1, 0), new(x + w - 1, 0), new(x + w - 1, leg), 1, Vector2.One * 0.4f, new(1, Ease.Linear, Color.Green, Color.GreenYellow, Color.LawnGreen));
                AddTriangle(new(x + half / 2, leg), new(x + half * 1.5f, leg), new(x + half, Height), 1, Vector2.One * 0.4f, new(0, Ease.Linear, Color.DarkGreen, Color.DarkOliveGreen, Color.LawnGreen));
            }

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Bake();
        }
    }
}
