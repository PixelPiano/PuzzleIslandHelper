using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/Passengers/Child")]
    [Tracked]
    public class ChildPassenger : VertexPassenger
    {
        public Balloon Balloon;
        public bool HasBalloon;
        public ChildPassenger(EntityData data, Vector2 offset) : base(data.Position + offset, 12, 20, data.Attr("cutsceneID"), new(1), new(-1, 1), 0.95f)
        {
            MinWiggleTime = 1;
            MaxWiggleTime = 2.5f;

            AddTriangle(new(1.2f, 0), new(9.6f, 4f), new(1.2f, 12f), 1, Vector2.One, new(1.4f, Ease.Linear, Color.DarkGreen, Color.DarkOliveGreen, Color.DarkOliveGreen));
            AddTriangle(new(9.6f, 5f), new(12f, 15f), new(0f, 15f), 0.5f, Vector2.One, new(1.2f, Ease.Linear, Color.Green, Color.DarkSeaGreen, Color.DarkSeaGreen));
            AddTriangle(new(2.4f, 16f), new(3.6f, 20), new(1.2f, 20), 0, Vector2.One, new(0.9f, Ease.Linear, Color.DarkGreen, Color.DarkOliveGreen, Color.DarkSeaGreen));
            AddTriangle(new(9.6f, 16f), new(10.8f, 20), new(8.4f, 20), 0, Vector2.One, new(0.9f, Ease.Linear, Color.DarkGreen, Color.DarkOliveGreen, Color.DarkSeaGreen));
            HasBalloon = data.Bool("hasBalloon");
            Balloon = new Balloon(this, 30, Color.Red, Color.White)
            {
                Offset = new Vector2(0.1f, 0.5f) * new Vector2(12, 20)
            };
        }
        public override void Update()
        {
            base.Update();
            Balloon.Visible = HasBalloon;
            if (Facing == Facings.Left)
            {
                Balloon.Offset.X = 0.1f * 12;
            }
            else
            {
                Balloon.Offset.X = 0.9f * 12;
            }
        }
        private IEnumerator routine()
        {
            while (true)
            {
                yield return WalkX(120, 0.8f);
                yield return 1;
                yield return WalkX(-120, 0.8f);
                yield return 1;
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Bake();
            scene.Add(Balloon);
            if (!HasBalloon) Balloon.Visible = false;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(Balloon);
        }
    }
}
