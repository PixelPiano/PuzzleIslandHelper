using Celeste.Mod.Core;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public static class VertexHelper
    {
        public class Vertex
        {
            public PointData Data;
            public Color Color;
            public Group Group;
            public Vector2 WorldPosition;
            public Vector2 Position;
            public float RotationRate;
            public float AngleOffset;
            public Vector2 RotationOffset;
            public Vector2 OgOffset;
            public Vector2 Offset;
            public Action OnBake;
            public Action<int> Edit;
            public void Bake()
            {
                OnBake?.Invoke();
            }
            public override string ToString()
            {
                return string.Format("{0}, Group:{1}", Position, Group.ToString());
            }
        }
        public class Group
        {
            public bool Visible = true;
            private bool updated;
            public Vector2 Center;
            public bool AutoCalculateCenter = true;
            public float RotationRate;
            public float Angle;
            public float Alpha
            {
                get => Visible ? alpha : 0;
                set => alpha = value;
            }
            private float alpha = 1;
            public Vector2 Scale = Vector2.One;
            public Color Color;
            public List<Vertex> Vertices = [];
            public void RecalculateCenter()
            {
                Center = Vector2.Zero;
                if (Vertices.Count > 0)
                {
                    foreach (Vertex v in Vertices)
                    {
                        Center += v.Position;
                    }
                    Center /= Vertices.Count;
                }
            }
            public override string ToString()
            {
                return string.Format("Center: {0}, Alpha: {3}, RotationRate: {1}, Angle: {2}", Center, RotationRate, Angle, Alpha);
            }
            public void Update()
            {
                if (!updated)
                {
                    if (AutoCalculateCenter)
                    {
                        RecalculateCenter();
                    }
                    Angle += RotationRate;
                    Angle %= MathHelper.TwoPi;
                    updated = true;
                }
            }
            public void PostUpdate()
            {
                updated = false;
            }
        }
        [TrackedAs(typeof(PostUpdateHook))]
        public class VertexGroupData : PostUpdateHook
        {
            public Vector2 Scale;
            public Color Color;
            public Vector2 Position;
            public bool Baked;
            public int Count => VertexList.Count;
            public List<Vertex> VertexList = [];
            public Vertex this[int i]
            {
                get => VertexList[i];
                set => VertexList[i] = value;
            }
            public VertexPositionColor[] Vertices;
            public int[] Indices;
            public int[] LineIndices;
            public int[] DefaultIndices;
            private List<int> lineIndiceList = [];
            private List<int> indiceList = [];
            public Rectangle Bounds;
            public int Lines { get; private set; }
            public VertexGroupData() : base(null)
            {
                OnPostUpdate = PostUpdate;
                Active = true;
            }
            public void PostUpdate()
            {
                foreach (var v in VertexList)
                {
                    v.Group?.PostUpdate();
                }
            }
            public override string ToString()
            {
                string output = "";
                foreach (Vertex v in VertexList)
                {
                    output += "\n\t" + v.ToString();
                }
                return "{" + output + "\n}";
            }
            public void Bake()
            {
                foreach (Vertex v in VertexList)
                {
                    v.Bake();
                }
                Vector2[] array = [.. VertexList.Select(item => item.Position)];
                Vertices = array.CreateVertices(Scale, out DefaultIndices, Color);
                Indices = [.. indiceList];
                LineIndices = [.. lineIndiceList];
                Baked = true;
                float left = int.MaxValue, top = int.MaxValue, right = int.MinValue, bottom = int.MinValue;
                for (int i = 0; i < Count; i++)
                {
                    Vector2 p2 = this[i].Position;
                    left = Math.Min(left, p2.X);
                    right = Math.Max(right, p2.X);
                    top = Math.Min(top, p2.Y);
                    bottom = Math.Max(bottom, p2.Y);
                }
                Bounds = new Rectangle((int)left, (int)top, (int)(right - left), (int)(bottom - top));
            }
            public void AddQuad(PointData a, PointData b, PointData c, PointData d, bool mergePoints = true)
            {
                AddTriangle(a, b, c, mergePoints);
                AddTriangle(b, c, d, mergePoints);
            }
            public void AddLine(PointData a, PointData b, bool mergePoints = false) => AddPoints([a, b], mergePoints);
            public void AddPoints(PointData[] data, bool mergePoints = false)
            {
                if (Baked || data == null || data.Length == 0) return;
                int[] lines = new int[data.Length];
                List<Vertex> vertices = [];
                Dictionary<Group, List<Vertex>> uniqueGroups = [];
                List<Vertex> ungrouped = [];
                for (int i = 0; i < data.Length; i++)
                {
                    TryCreateVertex(data[i], out Vertex vertex);
                    lines[i] = Indices[^1];
                    if (vertex == null) continue;
                    vertices.Add(vertex);

                    if (data[i].Group != null)
                    {
                        if (!uniqueGroups.ContainsKey(data[i].Group))
                        {
                            uniqueGroups[data[i].Group] = [];
                        }
                        uniqueGroups[data[i].Group].Add(vertex);
                    }
                    else
                    {
                        ungrouped.Add(vertex);
                    }

                }
                if (ungrouped.Count > 0)
                {
                    AddGroup([.. ungrouped]);
                }
                foreach (var pair in uniqueGroups)
                {
                    AddGroup(pair.Key, [.. pair.Value]);
                }
                //add line from last index to new index
                for (int i = 1; i < lines.Length; i++)
                {
                    lineIndiceList.AddRange([lines[i - 1], lines[i]]);
                    Lines++;
                }
                //include line from start index to end index
                if (lines.Length > 1)
                {
                    lineIndiceList.AddRange([lines[0], lines[^1]]);
                    Lines++;
                }
            }
            public void AddTriangle(PointData a, PointData b, PointData c, bool mergePoints = false)
                => AddPoints([a, b, c], mergePoints);
            public void AddCircle(PointData center, PointData[] corners, bool mergePoints = true)
            {
                for (int i = 1; i < corners.Length; i++)
                {
                    AddTriangle(center, corners[i - 1], corners[i], mergePoints);
                }
                AddTriangle(center, corners[^1], corners[0], mergePoints);
            }
            public PointData[] AddEquilateral(PointData center, Vector2 radius, float angle)
            {
                PointData[] t = new PointData[3];
                for (int i = 0; i < 3; i++)
                {
                    t[i] = new PointData()
                    {
                        X = center.X + radius.X * (float)Math.Cos(angle + i * (MathHelper.Pi / 3) * 2 - MathHelper.PiOver2),
                        Y = center.Y + radius.Y * (float)Math.Sin(angle + i * (MathHelper.Pi / 3) * 2 - MathHelper.PiOver2),
                        Group = center.Group,
                        DefaultColor = center.DefaultColor,
                    };
                }
                AddTriangle(t[0], t[1], t[2]);
                return t;
            }
            public PointData[] AddEquilateral(PointData center, float radius, float angle) => AddEquilateral(center, Vector2.One * radius, angle);
            public PointData[] AddCircle(PointData center, float radius, float rotation, int resolution)
            {
                PointData[] cornerData = new PointData[Math.Abs(resolution)];
                for (int i = 0; i < Math.Abs(resolution); i++)
                {
                    cornerData[i] = new()
                    {
                        Point = center.Point + Calc.AngleToVector(i * (MathHelper.TwoPi / resolution) + rotation, radius),
                        Group = center.Group
                    };
                }
                AddCircle(center, cornerData);
                return [center, .. cornerData];
            }
            public Vector2[] GetEquilateral(Vector2 center, Vector2 radius, float angle)
            {
                Vector2[] t = new Vector2[3];
                for (int i = 0; i < 3; i++)
                {
                    t[i].X = center.X + radius.X * (float)Math.Cos(angle + i * (MathHelper.Pi / 3) * 2 - MathHelper.PiOver2);
                    t[i].Y = center.Y + radius.Y * (float)Math.Sin(angle + i * (MathHelper.Pi / 3) * 2 - MathHelper.PiOver2);
                }
                return t;
            }
            private bool TryCreateVertex(PointData data, out Vertex vertex, bool mergePoints = false)
            {
                vertex = null;
                if (mergePoints)
                {
                    for (int i = 0; i < Count; i++)
                    {
                        Vertex check = this[i];
                        if (check.Position == data.Point)
                        {
                            check.Data = data;
                            indiceList.Add(i);
                            return false;
                        }
                    }
                }
                vertex = new();
                vertex.Data = data;
                vertex.Position = data.Point;
                vertex.Color = (data.DefaultColor ?? Color) * data.Alpha;
                indiceList.Add(Count);
                VertexList.Add(vertex);
                return true;
            }
            public void AddGroup(params Vertex[] vertices) => AddGroup(null, vertices);
            public void AddGroup(Group data, params Vertex[] vertices)
            {
                if (vertices != null && vertices.Length > 0)
                {
                    data ??= new Group()
                    {
                        Alpha = 1,
                        Angle = 0,
                        RotationRate = 0
                    };
                    foreach (Vertex v in vertices)
                    {
                        v.Group = data;
                        data.Vertices.Add(v);
                        VertexList.Add(v);
                    }
                }
            }
            public override void Update()
            {
                foreach (var v in VertexList)
                {
                    v.Group?.Update();
                }
                UpdateVertices();
            }

            public virtual void EditVertice(int index)
            {

            }
            public void UpdateVertices()
            {
                if (Baked)
                {
                    for (int i = 0; i < Count; i++)
                    {
                        Vertex vertex = this[i];
                        Group group = vertex.Group;
                        Vector2 point;
                        if (group.Angle == 0)
                        {
                            point = vertex.Position;
                        }
                        else
                        {
                            point = PianoUtils.RotateAroundRad(vertex.Position, group.Center, group.Angle);
                        }
                        vertex.WorldPosition = Position + (point * group.Scale);
                        vertex.Edit?.Invoke(i);
                        if (Vertices.Length > i)
                        {
                            Vertices[i].Position = new Vector3(vertex.WorldPosition, 0);
                            Vertices[i].Color = vertex.Color * vertex.Data.Alpha;
                        }

                    }
                }
            }
        }
        public struct PointData
        {
            public static implicit operator Vector2(PointData data) => data.Point;
            public static implicit operator PointData(Vector2 v) => new PointData(v);
            public Vector2 Point;
            public Group Group = null;
            public Color? DefaultColor = null;
            public float Alpha = 1;
            public float X
            {
                get => Point.X;
                set => Point.X = value;
            }
            public float Y
            {
                get => Point.Y;
                set => Point.Y = value;
            }
            public PointData()
            {

            }
            public PointData(Vector2 position)
            {
                Point = position;
            }
            public PointData(Vector2 position, Color defaultColor)
            {
                Point = position;
                DefaultColor = defaultColor;
            }
        }
        internal static Effect Prepare(Matrix matrix, Color? color = null, Effect ineffect = null, BlendState blendstate = null)
        {
            Effect outeffect = (ineffect != null) ? ineffect : GFX.FxPrimitive;
            BlendState blendState2 = ((blendstate != null) ? blendstate : BlendState.AlphaBlend);
            Vector2 vector = new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height);
            matrix *= Matrix.CreateScale(1f / vector.X * 2f, (0f - 1f / vector.Y) * 2f, 1f);
            matrix *= Matrix.CreateTranslation(-1f, 1f, 0f);
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Engine.Instance.GraphicsDevice.BlendState = blendState2;
            bool hasColor = color.HasValue;
            outeffect.Parameters["World"]?.SetValue(matrix);
            outeffect.Parameters["Shift"]?.SetValue(hasColor ? 1 : 0);
            outeffect.Parameters["Color"]?.SetValue(hasColor ? color.Value.ToVector4() : Vector4.Zero);
            return outeffect;
        }
        public static void DrawIndexedVertices<T>(PrimitiveType type, Matrix matrix, T[] vertices, int vertexCount, int[] indices, int primitiveCount, Color? color = null, Effect effect = null, BlendState blendState = null) where T : struct, IVertexType
        {
            Effect obj = Prepare(matrix, color, effect, blendState);
            foreach (EffectPass pass in obj.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(type, vertices, 0, vertexCount, indices, 0, primitiveCount);
            }
        }
        public static void DrawOutline(Matrix matrix, VertexPositionColor[] vertices, int vertexCount, int[] indices, int lines, Color color, Effect effect = null)
        {
            DrawIndexedVertices(PrimitiveType.LineList, matrix, vertices, vertices.Length, indices, lines, color, effect);
        }
        public static void DrawLines<T>(Matrix matrix, T[] vertices, int vertexCount, int[] indices, int primitiveCount, Effect effect = null, BlendState blendState = null, Color? solidColor = null) where T : struct, IVertexType
        {
            Effect obj = ((effect != null) ? effect : GFX.FxPrimitive);
            BlendState blendState2 = ((blendState != null) ? blendState : BlendState.AlphaBlend);
            Vector2 vector = new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height);
            matrix *= Matrix.CreateScale(1f / vector.X * 2f, (0f - 1f / vector.Y) * 2f, 1f);
            matrix *= Matrix.CreateTranslation(-1f, 1f, 0f);
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Engine.Instance.GraphicsDevice.BlendState = blendState2;
            obj.Parameters["World"]?.SetValue(matrix);
            if (solidColor.HasValue)
            {
                obj.Parameters["Color"]?.SetValue(solidColor.Value.ToVector4());
                obj.Parameters["Shift"]?.SetValue(1);
            }
            else
            {
                obj.Parameters["Shift"]?.SetValue(0);
            }
            foreach (EffectPass pass in obj.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, vertices, 0, vertexCount, indices, 0, primitiveCount);
            }
        }

    }
}
