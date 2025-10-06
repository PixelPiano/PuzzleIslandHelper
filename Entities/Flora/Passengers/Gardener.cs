using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/Passengers/Gardener")]
    [Tracked]
    public class Gardener : VertexPassenger
    {
        public Vector2 FootB = new Vector2(10, 28);
        public Vector2 FootL = new Vector2(2, 20);
        public Vector2 FootR = new Vector2(18, 20);
        public Vector2 NeckT = new Vector2(10, 6);
        public Vector2 NeckL = new Vector2(2, 14);
        public Vector2 NeckR = new Vector2(18, 14);
        public Vector2 HeadD = new Vector2(10, 14);
        public Vector2 HeadL = new Vector2(3, 7);
        public Vector2 HeadR = new Vector2(17, 7);
        public Vector2 HeadT = new Vector2(10, 3);
        public Vector2 HatL = new Vector2(1, 6);
        public Vector2 HatR = new Vector2(19, 6);
        public Vector2 BodyConnectFoot => new Vector2(FootB.X, FootL.Y - 4);
        public Vector2 BodyConnectBody => new Vector2(HeadD.X, HeadD.Y + 6);
        public Gardener(EntityData data, Vector2 offset,EntityID id) : base(data, offset, id, 21, 29, new(1), new(-1, 1), 1.8f)
        {
            MinWiggleTime = 1;
            MaxWiggleTime = 2.5f;

            AddTriangle(FootL, FootR, FootB, 0, Vector2.Zero, new(Color.DarkGreen.Darken(0.05f), Color.ForestGreen.Darken(0.05f), Color.Cyan.Darken(0.05f)));
            AddTriangle(FootL, BodyConnectFoot, FootR, 0, Vector2.Zero, null);

            AddTriangle(NeckL, NeckT, NeckR, 0.6f, Vector2.One, new(Color.DarkGreen, Color.ForestGreen, Color.Cyan));
            AddTriangle(NeckL, BodyConnectBody, NeckR, 0.6f, Vector2.One, null);

            AddTriangle(HeadL, HeadT, HeadR, 1, Vector2.One, new(Color.Green, Color.Turquoise, Color.Cyan));
            AddTriangle(HeadL, HeadD, HeadR, 1, Vector2.One, null);

            AddTriangle(HatL, HeadT, HatR, 1, Vector2.Zero, new(Color.Green, Color.Turquoise, Color.Cyan));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Bake();
        }
    }
}
