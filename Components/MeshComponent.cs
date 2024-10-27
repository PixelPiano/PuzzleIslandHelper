using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using static Celeste.Mod.PuzzleIslandHelper.Helpers.BitrailHelper;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class MeshComponent : GraphicsComponent
    {
        public readonly int TotalVertices;
        public readonly int TotalIndices;
        public readonly Vector2[] Points;
        public readonly int[] Indices;
        public VertexPositionColor[] Vertices;
        public Color[] Colors;
        public MeshComponent(Vector2 position, Vector2 scale, Vector2[] points, int[] indices, params Color[] verticeColors) : base(true)
        {
            Scale = scale;
            Position = position;
            Points = points;
            Indices = indices;
            if (Indices == null)
            {
                Indices = new int[Points.Length];
                for (int i = 0; i < Points.Length; i++)
                {
                    Indices[i] = i;
                }
            }
            Vertices = new VertexPositionColor[Points.Length];

            Colors = new Color[Points.Length];
            Color color = Color.White;
            for (int i = 0; i < Points.Length; i++)
            {
                Color c = verticeColors != null && verticeColors.Length > i ? verticeColors[i] : color;
                Colors[i] = c;
                Vertices[i] = new VertexPositionColor(new Vector3(Points[i], 0), c);
                color = c;
            }
        }
        public override void Update()
        {
            base.Update();
            Vector2 renderPosition = RenderPosition;
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Position = new Vector3(renderPosition + Points[i] * Scale, 0);
                Vertices[i].Color = Colors[i];
            }
        }
        public void Draw(Matrix matrix)
        {
            GFX.DrawIndexedVertices(matrix, Vertices, Vertices.Length, Indices, Indices.Length / 3);
        }
    }
}
