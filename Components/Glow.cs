using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class Glow : GraphicsComponent
    {
        public float Radius;
        public float Alpha;
        public readonly int Resolution;
        private VertexPositionColor[] Vertices;
        private int[] Indices;
        public Color ColorB;
        private int primitives;
        private Vector2[] Points;
        public Glow(Vector2 position, float radius, Color color, float alpha, int resolution, Color colorB = default) : base(true)
        {
            Position = position;
            Radius = radius;
            Color = color;
            Alpha = alpha;
            Resolution = resolution;
            ColorB = colorB;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            RecreateVertices();
        }
        public void RecreateVertices()
        {
            primitives = 0;
            List<VertexPositionColor> vertices = [];
            List<int> indices = [];
            vertices.Add(new VertexPositionColor(Vector3.Zero, Color));
            List<Vector2> points = [];
            for (int i = 0; i < Resolution; i++)
            {
                Vector3 point = new Vector3(Calc.AngleToVector((360f / Resolution * i).ToRad(), 1), 0);
                vertices.Add(new VertexPositionColor(point, ColorB));
                points.Add(point.XY());
            }
            Points = [.. points];
            for (int i = 2; i < Resolution; i++)
            {
                indices.Add(0);
                indices.Add(i - 1);
                indices.Add(i);
                primitives++;
            }
            Vertices = [.. vertices];
            Indices = [.. indices];
            UpdateVertices();
        }
        public void UpdateVertices(bool rotationUpdate = false)
        {
            Vertices[0].Position = new Vector3(RenderPosition, 0);
            Vertices[0].Color = Color * Alpha;
            for (int i = 1; i < Vertices.Length; i++)
            {
                Vertices[i].Position = new Vector3(Points[i - 1] * Scale * Radius, 0);
                Vertices[i].Color = ColorB * Alpha;
            }
        }
        public override void Render()
        {
            base.Render();
            GFX.DrawIndexedVertices(SceneAs<Level>().Camera.Matrix, Vertices, Vertices.Length, Indices, primitives);
        }
        public Glow(Vector2 position, float radius, Color color, float alpha) : this(position, radius, color, alpha, 30)
        {
        }
    }
}
