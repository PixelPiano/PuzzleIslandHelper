using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.PuzzleIslandHelper.Entities.FlowTexture;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class VertexCircle : GraphicsComponent
    {
        public VertexPositionColor[] Vertices;
        public int StartFade;
        public int EndFade;
        public int[] Indices;
        public float RotationRate;
        public Color ColorB;
        public Color? CenterColor;
        public Color? CenterColorB;
        public float FillColorLerp, CenterColorLerp;
        public float FillAlpha = 1;
        public float CenterAlpha = 1;
        public bool Baked;
        public bool ResetOnRemoved;
        public int Corners
        {
            get => _corners;
            set
            {
                if (_corners != value)
                {
                    RecalculateVertices(value);
                }
                _corners = value;
            }
        }
        private int _corners;
        public Func<Color, int, Color> CustomGetFillColor;
        public Func<Color, Color> CustomGetCenterColor;
        public VertexCircle(Vector2 position, Color color, int startFade, int endFade) : base(true)
        {
            Position = position;
            StartFade = startFade;
            EndFade = endFade;
            Color = color;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            Corners = 16;
        }
        public override void Update()
        {
            base.Update();
            Rotation = (Rotation + RotationRate) % MathHelper.TwoPi;
            UpdateVertices();
        }
        public virtual Color GetFillColor(Color baseColor, int index)
        {
            if (CustomGetFillColor != null) return CustomGetFillColor(baseColor, index);
            return baseColor * FillAlpha;
        }
        public virtual Color GetCenterColor(Color baseColor)
        {
            if (CustomGetCenterColor != null) return CustomGetCenterColor(baseColor);
            return baseColor * FillAlpha;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (Entity != null)
            {
                Draw.Circle(RenderPosition, StartFade, Color.Red, 16);
                Draw.Circle(RenderPosition, EndFade, Color.Red, 16);
            }
        }

        public void UpdateVertices()
        {
            if (Baked && Corners > 0)
            {
                Color fillColor = Color.Lerp(Color, ColorB, FillColorLerp);
                Vector2 p = RenderPosition;
                Vertices[0].Position = p.ToVec3();
                if (CenterColor.HasValue)
                {
                    Color centerColor = Color.Lerp(CenterColor.Value, CenterColorB ?? Color.Transparent, CenterColorLerp);
                    Vertices[0].Color = GetCenterColor(centerColor);
                }
                else
                {
                    Vertices[0].Color = GetFillColor(fillColor, 0);
                }
                float angle = Rotation;
                float angleStep = MathHelper.TwoPi / 16;
                for (int i = 1; i < Corners + 1; i++, angle += angleStep)
                {
                    Vertices[i].Position = (p + Calc.AngleToVector(angle, StartFade)).ToVec3();
                    Vertices[i + Corners].Position = (p + Calc.AngleToVector(angle, EndFade)).ToVec3();
                    Vertices[i].Color = GetFillColor(fillColor, i);
                }
            }
        }
        public void RecalculateVertices(int corners)
        {
            Vertices = new VertexPositionColor[corners * 2 + 1];
            for (int i = 0; i < corners; i++)
            {
                Vertices[corners + i].Color = Color.Transparent;
            }
            UpdateVertices();
        }
        public void RecreateCircleIndices(int corners)
        {
            Indices = null;
            List<int> indices = [];

            for (int i = 0; i < corners; i++)
            {
                indices.AddRange([0, i + 1, i + 2]);
            }
            indices.AddRange([0, corners, 1]);
            for (int i = 0; i < corners; i++)
            {
                indices.AddRange([i + 1, i + corners + 1, i + 2, i + corners + 1, i + corners + 2, i + 2]);
            }
            indices.AddRange([corners, corners * 2, 1, corners * 2, corners + 1, 1]);

            Indices = [.. indices];
            RecalculateVertices(corners);
        }
        public void DirectRenderVertices()
        {
            Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Vertices.Length, Indices, 0, Corners * 2);
        }
        public void RenderFillVertices(Matrix matrix)
        {
            GFX.DrawIndexedVertices(matrix, Vertices, Vertices.Length, Indices, Corners * 2);
        }
        public void RenderVertices()
        {
            if (Baked && Corners > 0 && StartFade != 0 && StartFade + EndFade != 0)
            {
                Matrix matrix = Entity.SceneAs<Level>().Camera.Matrix;
                RenderFillVertices(matrix);
            }
        }
    }
}