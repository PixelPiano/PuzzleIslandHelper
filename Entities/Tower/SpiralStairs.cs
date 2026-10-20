using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.DEBUG;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Singularity.AscendBarrier.Window;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Tower.SpiralStairs;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    public class SpiralStairs : Entity
    {
        public class Renderer : Entity
        {
            public VirtualRenderTarget Target;
            public Action OnBeforeRender;
            public Action OnRender;
            public Renderer(int depth)
            {
                Target = VirtualContent.CreateRenderTarget("Render Target", 320, 180);
                Depth = depth;
                Add(new BeforeRenderHook(() =>
                {
                    OnBeforeRender?.Invoke();
                }));
            }
            public override void Render()
            {
                base.Render();
                OnRender?.Invoke();
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Target?.Dispose();
            }
        }
        public Renderer BG;
        public Renderer FG;
        public class VertexPoint(Vector3 point, int index)
        {
            public Vector3 OrigPoint = point;
            public Vector3 Offset;
            public Vector3 OrigMult = Vector3.One;
            public Color Color = Color.White;
            public Color FinalColor => Color * Alpha;
            public float Alpha = 1;
            public Vector3 Point
            {
                get => OrigPoint * OrigMult + Offset;
            }
            public int Index = index;
        }
        private List<VertexPoint> rimPoints = [];
        private List<VertexPoint> insidePoints = [];
        public List<Line> Lines = [];
        public VertexPositionColor[] Vertices;
        public int[] BGIndices;
        public int[] FGIndices;
        public int BGPrimitives;
        public int FGPrimitives;
        public int VertCount;
        public float Radius = 64;
        public class Line : IComparable<Line>
        {
            public bool Foreground => A.Point.X > 0 && A.Point.Z >= 0 && B.Point.X > 0 && B.Point.Z >= 0;
            public bool IsRim;
            public float Depth => (A.Point.Z + B.Point.Z) / 2f;
            public VertexPoint A, B;
            public int CompareTo(Line other)
            {
                int sign = Math.Sign(Depth - other.Depth);
                if (sign == 0)
                {
                    if (Depth < 0)
                    {
                        return IsRim ? -1 : 1;
                    }
                    else
                    {
                        return IsRim ? 1 : -1;
                    }
                }
                return sign;
            }
            public Line(VertexPoint a, VertexPoint b)
            {
                A = a;
                B = b;
            }
            public void UpdateIndices(ref int[] array, int index)
            {
                array[index] = A.Index;
                array[index + 1] = B.Index;
            }
        }
        public SpiralStairs(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            BG = new Renderer(2);
            FG = new Renderer(-2);
            BG.OnRender = () =>
            {
                if (BGIndices != null && BGPrimitives > 0)
                {
                    Draw.SpriteBatch.End();
                    PianoUtils.DrawUserIndexedPrimitives(PrimitiveType.LineList, SceneAs<Level>().Camera.Matrix, Vertices, VertCount, BGIndices, BGPrimitives);
                    GameplayRenderer.Begin();
                }
            };
            FG.OnRender = () =>
            {
                if (FGIndices != null && FGPrimitives > 0)
                {
                    Draw.SpriteBatch.End();
                    PianoUtils.DrawUserIndexedPrimitives(PrimitiveType.LineList, SceneAs<Level>().Camera.Matrix, Vertices, VertCount, FGIndices, FGPrimitives);
                    GameplayRenderer.Begin();
                }
            };
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            CreateVertices(64, 120, 3, 24);
            scene.Add(BG, FG);
        }
        public void CreateVertices(float radius, float floorHeight, int floors, int steps)
        {
            rimPoints.Clear();
            insidePoints.Clear();
            VertCount = 0;
            Lines.Clear();
            float halfFloorHeight = floorHeight / 2;
            for (int i = 0; i < floors; i++)
            {
                float y = floorHeight * i;
                if (floorHeight > 0)
                {
                    while (y < floorHeight * (i + 1))
                    {
                        Vector3 rimPoint = new Vector3(GetX(radius, -y, halfFloorHeight), y, GetZ(radius, -y, halfFloorHeight));
                        Vector3 insidePoint = rimPoint * (30f / 64f);
                        VertexPoint rim = new(rimPoint, VertCount);
                        VertexPoint inside = new(insidePoint, VertCount + 1);
                        rimPoints.Add(rim);
                        insidePoints.Add(inside);
                        rim.Offset = new Vector3(Position + Vector2.UnitY * floorHeight * floors, 0);
                        inside.Offset = new Vector3(Position + Vector2.UnitY * floorHeight * floors, 0);
                        VertCount += 2;
                        y += floorHeight / steps;
                    }
                }
            }
            for (int i = 0; i < rimPoints.Count; i++)
            {
                Line line = new Line(rimPoints[i], insidePoints[i]) { IsRim = false };
                Lines.Add(line);
                if (i > 0)
                {
                    Lines.Add(new Line(rimPoints[i - 1], rimPoints[i]) { IsRim = true });
                }
            }

            Vertices = new VertexPositionColor[VertCount];
            UpdateVertices();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
        public void UpdateVertices()
        {
            BGPrimitives = 0;
            FGPrimitives = 0;
            int index = 0;
            for (int i = 0; i < VertCount; i += 2, index++)
            {
                Vertices[i].Position = rimPoints[index].Point;
                Vertices[i].Color = rimPoints[index].FinalColor;
                Vertices[i + 1].Position = insidePoints[index].Point;
                Vertices[i + 1].Color = insidePoints[index].FinalColor;
            }
            Lines.Sort();
            index = 0;
            int bgIndexCount = 0;
            int fgIndexCount = 0;
            foreach (Line line in Lines)
            {
                if (line.Foreground) fgIndexCount += 2;
                else bgIndexCount += 2;
            }
            if (bgIndexCount > 0 && (BGIndices == null || BGIndices.Length != bgIndexCount))
            {
                BGIndices = new int[bgIndexCount];
            }
            if (BGIndices != null)
            {
                int bgIndex = 0;
                foreach (Line line in Lines.Where(item => !item.Foreground))
                {
                    line.UpdateIndices(ref BGIndices, bgIndex);
                    bgIndex += 2;
                    BGPrimitives++;
                }
            }
            if (fgIndexCount > 0 && (FGIndices == null || FGIndices.Length != fgIndexCount))
            {
                FGIndices = new int[fgIndexCount];
            }
            if (FGIndices != null)
            {
                int fgIndex = 0;
                foreach (Line line in Lines.Where(item => item.Foreground))
                {
                    line.UpdateIndices(ref FGIndices, fgIndex);
                    fgIndex += 2;
                    FGPrimitives++;
                }
            }
        }
        public float GetX(float radius, float y, float halfWavelength, float angleOffset = 0)
        {
            return (float)(double)(radius * Math.Sin(angleOffset + y * Math.PI / halfWavelength));
        }
        public float GetZ(float radius, float y, float halfWavelength, float angleOffset = 0)
        {
            return (float)(double)(radius * Math.Cos(angleOffset + y * Math.PI / halfWavelength));
        }
        public override void Update()
        {
            base.Update();
            UpdateVertices();
        }
        public override void Render()
        {
            base.Render();
        }
    }
}