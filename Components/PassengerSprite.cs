using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [Tracked]
    public class PassengerSprite : Component
    {
        public readonly int TotalVertices;
        public readonly int TotalIndices;
        public Vector2[] HeadPoints;
        public Vector2[] BodyPoints;
        public int[] HeadIndices;
        public int[] BodyIndices;
        public VertexPositionColor[] Vertices;
        public int[] Indices;
        public Vector3 Scale = Vector3.One;
        public Vector2 Position;
        public Vector2 RenderPosition
        {
            get
            {
                return ((base.Entity == null) ? Vector2.Zero : base.Entity.Position) + Position;
            }
            set
            {
                Position = value - ((base.Entity == null) ? Vector2.Zero : base.Entity.Position);
            }
        }

        public PassengerSprite(Vector2 position, Vector2[] headPoints, int[] headIndices, Vector2[] bodyPoints, int[] bodyIndices) : base(true, true)
        {
            Position = position;
            HeadPoints = headPoints;
            BodyPoints = bodyPoints;
            HeadIndices = headIndices;
            BodyIndices = bodyIndices;
            TotalVertices = headPoints.Length + bodyPoints.Length;
            TotalIndices = headIndices.Length + bodyIndices.Length;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            Vertices = new VertexPositionColor[TotalVertices];
            Indices = new int[TotalIndices];

            for (int i = 0; i < HeadPoints.Length; i++)
            {
                Vertices[i] = new VertexPositionColor(new Vector3(HeadPoints[i], 0), Color.White);
            }
            for (int i = HeadPoints.Length; i < TotalVertices; i++)
            {
                Vertices[i] = new VertexPositionColor(new Vector3(BodyPoints[i], 0), Color.White);
            }
        }
    }
}
