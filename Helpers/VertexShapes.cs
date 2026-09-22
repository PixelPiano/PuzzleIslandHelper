using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.CommunalHelper.Utils;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Helpers;

public static class VertexShapes
{
    public class Circle : GraphicsComponent
    {
        public VertexPositionColor[] Vertices;
        public VertexPositionColor[] EdgeVertices;
        public float EdgeRadiusOffset;
        public int[] Indices;
        public int Corners
        {
            get => corners;
            set
            {
                int prev = corners;
                corners = value;
                if (prev != value && corners > 1) RecalculateVertices();
            }
        }
        private int corners;
        public float Radius;
        public float RotationRate;
        public int[] edgeIndices;
        public Color EdgeColor = Color.Black;
        public bool DrawEdge;
        public bool DrawFill = true;
        public bool Baked;
        public float FlashTimer;
        public void Flash(float time)
        {
            FlashTimer = time;
        }
        public Circle(Vector2 position, float radius, int corners, Color color, Color edgeColor = default, float edgeRadiusOffset = 0) : base(true)
        {
            Position = position;
            Radius = radius;
            Corners = corners;
            Color = color;
            EdgeColor = edgeColor;
            EdgeRadiusOffset = edgeRadiusOffset;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            RecalculateVertices();
        }
        public override void Update()
        {
            base.Update();
            Rotation = (Rotation + RotationRate) % MathHelper.TwoPi;
            UpdateVertices();
            if (FlashTimer > 0)
            {
                FlashTimer -= Engine.DeltaTime;
            }
        }
        public virtual Color GetEdgeColor(int index)
        {
            return FlashTimer > 0 ? Color.White : EdgeColor;
        }
        public virtual Color GetCircleColor(int index)
        {
            return FlashTimer > 0 ? Color.White : Color;
        }
        public void UpdateVertices()
        {
            if (Baked)
            {
                Vector2 p = RenderPosition;
                float angle = Rotation;
                Vertices[0].Position = p.ToVec3();
                Vertices[0].Color = GetCircleColor(0);
                for (int i = 1; i < Corners + 1; i++)
                {
                    Vertices[i].Position = (p + Calc.AngleToVector(angle, Radius)).Floor().ToVec3();
                    if (EdgeRadiusOffset == 0)
                    {
                        EdgeVertices[i - 1].Position = Vertices[i].Position;
                    }
                    else
                    {
                        EdgeVertices[i - 1].Position = (p + Calc.AngleToVector(angle, Radius + EdgeRadiusOffset)).ToVec3();
                    }
                    Vertices[i].Color = GetCircleColor(i);
                    angle += MathHelper.TwoPi / Corners;
                }
                if (Vertices.Length > 1)
                {
                    EdgeVertices[Corners].Position = Vertices[1].Position;
                }
                for (int i = 0; i < EdgeVertices.Length; i++)
                {
                    EdgeVertices[i].Color = GetEdgeColor(i);
                }
            }
        }
        public void RecalculateVertices()
        {
            Baked = false;
            List<int> indices = [];
            Indices = null;
            edgeIndices = new int[Corners * 2];
            Vertices = new VertexPositionColor[Corners + 1];
            EdgeVertices = new VertexPositionColor[Corners + 1];
            for (int i = 0; i < Corners; i++)
            {
                if (i < Corners - 1)
                {
                    indices.AddRange([0, i + 1, i + 2]);
                }
                else
                {
                    indices.AddRange([0, Corners, 1]);
                }
            }
            Indices = [.. indices];
            Baked = true;
            UpdateVertices();

        }

        public void RenderVertices()
        {
            if (Baked && Radius != 0 && Corners > 1 && (DrawFill || DrawEdge))
            {
                Matrix matrix = SceneAs<Level>().Camera.Matrix;
                if (DrawFill)
                {
                    GFX.DrawIndexedVertices(matrix, Vertices, Vertices.Length, Indices, Corners);
                }
                if (DrawEdge)
                {
                    PianoUtils.DrawUserPrimitives(PrimitiveType.LineStrip, matrix, EdgeVertices, Corners);
                }
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
        }
    }
}
