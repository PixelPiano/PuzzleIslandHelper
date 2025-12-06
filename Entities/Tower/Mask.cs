using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.DEBUG;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    public class Mask : Component
    {
        public int LightIndex = -1;
        public Vector2 Position;
        public Vector2 Anchor;
        public VertexPositionColor[] Vertices;
        public VertexPositionColor[] DebugVertices;
        public int VertexCount;
        public int PrimitiveCount;
        public int[] Indices;
        public VirtualRenderTarget target;
        public Vector2 End;
        public bool SortRight;
        public Vector2[] Points;
        public Mask(Vector2[] points, bool sortRight, int width, int height) : base(true, false)
        {
            SortRight = sortRight;
            List<VertexPositionColor> vertices = [];
            List<VertexPositionColor> debugVertices = [];
            void addVertex(Vector2 position, int index)
            {
                vertices.Add(new(new(position, 0), Color.White));
                debugVertices.Add(new(new(position, 0), index == 0 ? Color.Black : index < 5 ? Color.Magenta : Color.White));
            }
            List<int> indices = [];
            Vector2 left, right;
            if (sortRight)
            {
                points = [.. points.OrderByDescending(item => item.X)];
                right = points[0];
                left = points[^1];
            }
            else
            {
                points = [.. points.OrderBy(item => item.X)];
                left = points[0];
                right = points[^1];
            }
            float y = points.OrderBy(item => item.Y).First().Y;
            Position = new Vector2(left.X, y);
            Vector2 anchor = new Vector2(points[0].X, points[^1].Y);
            List<Vector2> newPoints = [anchor];
            newPoints.AddRange(points);
            Anchor = anchor;
            Vector2 prev = newPoints[1];
            addVertex(anchor, 0);
            addVertex(prev, 1);
            for (int i = 2; i < newPoints.Count; i++)
            {
                addVertex(newPoints[i], i);
                indices.Add(0); //Anchor       P        
                indices.Add(i - 1); //Prev     |\ C ->    _ P
                indices.Add(i); //Current     A|/      A //C 
                PrimitiveCount++;
            }
            Vertices = [.. vertices];
            DebugVertices = [.. debugVertices];
            Indices = [.. indices];
            VertexCount = Vertices.Length;
            Points = [.. points];
            target = VirtualContent.CreateRenderTarget("verticeMaskTarget", width, height);
        }
        public override void DebugRender(Camera camera)
        {
            Color colorA = SortRight ? Color.Red : Color.Yellow;
            Color colorB = SortRight ? Color.Purple : Color.Cyan;
            int count = 0;
            int max = DebugVertices.Length;
            foreach (var v in DebugVertices)
            {
                Draw.Line(Anchor, v.Position.XY(), Color.Lerp(colorA, colorB, (float)count / max));
                count++;
            }
            foreach (var v in DebugVertices)
            {
                Draw.Rect(v.Position.XY() - Vector2.One, 2, 2, Color.White);
            }
        }
        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            target?.Dispose();
        }
        public void DrawToMask(Matrix matrix)
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Position -= new Vector3(Position, 0);
            }
            GFX.DrawIndexedVertices(matrix, Vertices, VertexCount, Indices, PrimitiveCount);
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Position += new Vector3(Position, 0);
            }
        }

    }
}