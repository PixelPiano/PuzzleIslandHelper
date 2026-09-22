using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;
using YamlDotNet.Core.Tokens;
using static Celeste.Mod.PuzzleIslandHelper.Components.VertexOrb;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Singularity
{
    [Tracked]
    public class AscendBarrierManager : Entity
    {
        [ConstantEntity("afhafha")]
        public class Barrier : Entity
        {
            //Class that holds a point that all vertex positions are based on
            public class MapPoint(Vector3 position)
            {
                public Vector3 Position = position;
                public bool Pushed;
            }
            //Struct to allow variation without disrupting the position held in MapPoint
            public struct MapPointRef(MapPoint mapPoint)
            {
                public MapPoint MapPoint = mapPoint;
                public readonly Vector3 Render => MapPoint != null ? MapPoint.Position + Offset : Offset;
                //Any change to the visual position must be stored in Offset
                public Vector3 Offset;
                public Color Color;
            }
            //Todo: make small shard textures


            public class Window : IComparable<Window>
            {
                public bool Pushed => points[0].MapPoint.Pushed || points[1].MapPoint.Pushed || points[2].MapPoint.Pushed;
                public float Rank;
                public MapPointRef[] OrigPoints;
                public MapPointRef[] points;
                private Vector3 speedVector;
                public List<Primitive> Primitives = [];
                public Tri triangle;
                public Color? Custom;
                public bool Broken;
                public bool Detached;
                private float yaw, pitch, roll, speedX, speedY, ySpeedTarget, ySpeedMove;
                private float yawRate, pitchRate, rollRate;
                private float flashTimer;
                public bool Active = true;
                public bool Visible = true;
                private VertexPositionColor[] customVerts;
                public Window(MapPoint a, MapPoint b, MapPoint c)
                {
                    points = new MapPointRef[3];
                    OrigPoints = [new(a), new(b), new(c)];
                    Reset();
                    customVerts = new VertexPositionColor[2];
                }
                public void Break(int cracks = 1)
                {
                    Broken = true;
                    for (int i = 0; i < cracks; i++)
                    {
                        Crack c = new Crack(Calc.Random.Range(0.2f, 0.8f), Calc.Random.Range(0, 3), Calc.Random.Choose(0, 0, Calc.Random.Range(-0.1f, 0.6f)));
                        c.Delay = 0.3f + i * 0.07f;
                        Primitives.Add(c);
                    }
                    triangle.Alpha = 0.6f;
                    flashTimer = 0.3f;
                    //todo: this
                }
                public void Detach(float rot, float speedX, float speedY, float ySpeedTarget, float ySpeedMove)
                {
                    this.speedX = speedX;
                    this.speedY = speedY;
                    this.ySpeedTarget = ySpeedTarget;
                    this.ySpeedMove = ySpeedMove;
                    int[] mult = [1, 1, 1];
                    mult[Calc.Random.Range(0, 3)] = 0;
                    yawRate = Calc.Random.Range(-rot, rot) * mult[0];
                    pitchRate = Calc.Random.Range(-rot, rot) * mult[1];
                    rollRate = Calc.Random.Range(-rot, rot) * mult[2];
                    yaw = roll = pitch = 0;

                    //create new references that won't be affected by the main entity's rotation matrix
                    points[0] = new MapPointRef(new MapPoint(points[0].Render));
                    points[1] = new MapPointRef(new MapPoint(points[1].Render));
                    points[2] = new MapPointRef(new MapPoint(points[2].Render));
                    //todo: this
                    Detached = true;
                }
                public void Reset()
                {
                    Active = true;
                    Visible = true;
                    Broken = false;
                    Detached = false;
                    OrigPoints.CopyTo(points, 0);
                    speedVector = default;
                    yaw = pitch = roll = speedX = speedY = ySpeedTarget = ySpeedMove = yawRate = pitchRate = rollRate = flashTimer = 0;
                    Primitives.Clear();
                    Primitives.Add(triangle = new Tri());
                    Primitives.Add(new Line());
                    Primitives.Add(new Line(1));
                    Primitives.Add(new Line(2));
                }
                public void Update()
                {
                    if (flashTimer > 0)
                    {
                        flashTimer -= Engine.DeltaTime;
                        if (flashTimer <= 0)
                        {
                            flashTimer = 0;
                            if (Broken) triangle.Alpha = 0;
                        }
                    }
                    Vector3 a = points[0].Render;
                    Vector3 b = points[1].Render;
                    Vector3 c = points[2].Render;
                    if (Detached)
                    {
                        speedVector += new Vector3(speedX, speedY, 0) * Engine.DeltaTime;
                        speedX = Calc.Approach(speedX, 0, 120f * Engine.DeltaTime);
                        speedY = Calc.Approach(speedY, ySpeedTarget, ySpeedMove * Engine.DeltaTime);

                        Vector3 min = Vector3.Min(Vector3.Min(a, b), c);
                        Vector3 max = Vector3.Max(Vector3.Max(a, b), c);
                        Vector3 size = max - min;
                        Vector3 centroid = min + size / 2;
                        Matrix m = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
                        a += Vector3.Transform(a - centroid, m) + speedVector;
                        b += Vector3.Transform(b - centroid, m) + speedVector;
                        c += Vector3.Transform(c - centroid, m) + speedVector;
                    }
                    Vector3[] array = [a, b, c];
                    foreach (var p in Primitives)
                    {
                        p.Update(array);
                    }
                    Primitives.Sort();
                    if (Custom.HasValue)
                    {
                        customVerts[0].Position = points[0].Render;
                        customVerts[1].Position = points[0].Render - Vector3.UnitY * 12;
                        customVerts[0].Color = customVerts[1].Color = Custom.Value;
                    }

                    Rank = (a.Z + b.Z + c.Z) / 3f;
                }
                public void Render()
                {
                    foreach (Primitive p in Primitives)
                    {
                        if (p.Visible) p.Render();
                    }
                    if (Custom.HasValue)
                    {
                        Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, customVerts, 0, 1);

                    }
                }
                public int CompareTo(Window other)
                {
                    return Math.Sign(Rank - other.Rank);
                }
                public class Primitive : IComparable<Primitive>
                {
                    public bool Visible = true;
                    public float Rank;
                    public VertexPositionColor[] Vertices;
                    public PrimitiveType Type;
                    public int Verts;
                    public Color Color = Color.White;
                    public float Alpha = 1;
                    public int Primitives = 1;
                    public Primitive(int verts)
                    {
                        Verts = verts;
                        Vertices = new VertexPositionColor[verts];
                    }
                    public int CompareTo(Primitive other)
                    {
                        return Math.Sign(Rank - other.Rank);
                    }
                    public virtual void Update(Vector3[] points)
                    {

                    }
                    public virtual void Render()
                    {
                        Engine.Instance.GraphicsDevice.DrawUserPrimitives(Type, Vertices, 0, Primitives);
                    }

                }
                public class Tri : Primitive
                {
                    public float AlphaA = 1, AlphaB = 1, AlphaC = 1;
                    public Tri() : base(3)
                    {
                        Type = PrimitiveType.TriangleList;
                        Alpha = 0.3f;
                    }
                    public override void Update(Vector3[] points)
                    {
                        Vector3 a = points[0];
                        Vector3 b = points[1];
                        Vector3 c = points[2];
                        Vertices[0].Position = a;
                        Vertices[1].Position = b;
                        Vertices[2].Position = c;
                        Vertices[0].Color = Color * Alpha * AlphaA;
                        Vertices[1].Color = Color * Alpha * AlphaB;
                        Vertices[2].Color = Color * Alpha * AlphaC;
                        Rank = (a.Z + b.Z + c.Z) / 3f;
                    }

                }
                public class Line : Primitive
                {
                    public int PointOffset;
                    public float AlphaA = 1, AlphaB = 1;
                    public Line(int pointOffset = 0) : base(2)
                    {
                        Type = PrimitiveType.LineList;
                        PointOffset = pointOffset;
                    }
                    public override void Update(Vector3[] points)
                    {
                        Vector3 a = points[PointOffset % 3];
                        Vector3 b = points[(1 + PointOffset) % 3];
                        
                        Vertices[0].Position = a;
                        Vertices[1].Position = b;
                        Vertices[0].Color = Color.White * Alpha * AlphaA;
                        Vertices[1].Color = Color.White * Alpha * AlphaB;
                        Rank = (a.Z + b.Z) / 2f + 0.000001f;
                    }
                }
                public class Crack : Primitive
                {
                    private static readonly int[] triIndices = [0, 1, 2, 0, 2, 3];
                    private static readonly int[] lineIndices = [1, 2, 2, 3];
                    public float Lerp;
                    public int Anchor;
                    public float ApproachAnchor;
                    public float AlphaA, AlphaB, AlphaC, AlphaBC;
                    public float Delay;
                    private float flashTimer;
                    public Crack(float lerp, int anchor, float approachAnchor) : base(4)
                    {
                        Type = PrimitiveType.TriangleList;
                        Primitives = 2;
                        Lerp = lerp;
                        Anchor = anchor;
                        ApproachAnchor = approachAnchor;
                        AlphaA = 0.6f;
                        if (Calc.Random.Chance(0.5f))
                        {
                            AlphaB = Calc.Random.Range(0.4f, 0.6f);
                            AlphaC = 0;
                        }
                        else
                        {
                            AlphaC = Calc.Random.Range(0.4f, 0.6f);
                            AlphaB = 0;
                        }
                        AlphaBC = Calc.Random.Range(0.4f, 0.6f);
                        if (approachAnchor == 0) AlphaBC *= Calc.Random.Choose(0, 1);
                    }
                    public override void Render()
                    {
                        if (Delay <= 0)
                        {
                            Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Verts, triIndices, 0, 2);
                            Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, Vertices, 0, Verts, lineIndices, 0, 2);
                        }
                    }
                    public override void Update(Vector3[] points)
                    {
                        if (Delay > 0)
                        {
                            Delay -= Engine.DeltaTime;
                            if (Delay <= 0)
                            {
                                flashTimer = 0.2f;
                                Delay = 0;
                            }
                        }
                        Vector3 a = points[Anchor % 3];
                        Vector3 b = points[(1 + Anchor) % 3];
                        Vector3 c = points[(2 + Anchor) % 3];
                        
                        var ab = Vector3.Lerp(a, b, Lerp);
                        var ac = Vector3.Lerp(a, c, Lerp);
                        var bc = Vector3.Lerp(Vector3.Lerp(ab, ac, 0.5f), a, ApproachAnchor);
                        Vertices[0].Position = a;
                        Vertices[1].Position = ab;
                        Vertices[2].Position = bc;
                        Vertices[3].Position = ac;
                        if (flashTimer > 0)
                        {
                            Vertices[0].Color = Color * Alpha;
                            Vertices[1].Color = Color * Alpha;
                            Vertices[2].Color = Color * Alpha;
                            Vertices[3].Color = Color * Alpha;
                        }
                        else
                        {
                            Vertices[0].Color = Color * Alpha * AlphaA;
                            Vertices[1].Color = Color * Alpha * AlphaB;
                            Vertices[2].Color = Color * Alpha * AlphaBC;
                            Vertices[3].Color = Color * Alpha * AlphaC;
                        }
                        Rank = (a.Z + b.Z + c.Z) / 3f + 0.0001f;
                    }
                }
            }
            public float Scale = 1;
            private VirtualMap<MapPoint> mapPoints2D;
            private VirtualMap<MapPoint> modMapPoints;
            private Vector3 HalfSize;

            private List<Window> Windows = [];
            public VirtualRenderTarget Target;
            private int cols, rows;
            private Vector2 shake;
            private BetterShaker shaker;
            public float Push;
            private float yaw, pitch, roll;
            private Matrix rotation, scale, translate;
            private Matrix transformationMatrix;
            private Matrix prevTransformationMatrix;
            public float MaxDist;
            public float MaxZ;
            public Stopwatch Stopwatch;
            public Stopwatch Stopwatch2;
            public Barrier() : base()
            {
                Stopwatch = new Stopwatch();
                Stopwatch2 = new Stopwatch();
                Target = VirtualContent.CreateRenderTarget("TowerBarrier", 320, 180);
                Add(new BeforeRenderHook(() =>
                {
                    RenderVertices(ForEachPass);
                }));
                Add(shaker = new BetterShaker((v) =>
                {
                    shake += v;
                }));
                KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.B, () =>
                {
                    //ShatterField(13, 2, false, true);
                });
                KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.U, () =>
                {
                    //Clear();
                });
                int rotIndex = 0;
                pitch = 1.3f;
                KeyComponent.Wasd(this, left: () =>
                {
                    pitch -= Engine.DeltaTime * 10;
                    if (rotIndex < 0) rotIndex = 2;
                }, right: () =>
                {
                    pitch += Engine.DeltaTime * 10;
                    rotIndex = (rotIndex + 1) % 3;
                }, up: () =>
                {
                    Push++;
                }, down: () =>
                {
                    Push--;
                });
                //FieldDebug.Debug(this, "yaw", () => yaw.ToString());
                //FieldDebug.Debug(this, "pitch", () => pitch.ToString());
                //FieldDebug.Debug(this, "roll", () => roll.ToString());
                //FieldDebug.Debug(this, "push", () => Push.ToString());
                FieldDebug.Debug(this, "Render Time", () => Stopwatch.ToString());
                FieldDebug.Debug(this, "Update Time", () => Stopwatch2.ToString());
                //FieldDebug.Debug(this, "Render Mode", () => Enum.GetName(typeof(Modes), RenderMode));
                //FieldDebug.Debug(this, "Player Render Time", () => PlayerStopwatch.ToString());

            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                CreateVertices(320, 180, 0, 0);
            }
            public override void Update()
            {
                base.Update();
                Stopwatch2.Restart();
                Stopwatch2.Start();
                scale = Matrix.CreateScale(Scale);
                rotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
                translate = Matrix.CreateTranslation(HalfSize);
                transformationMatrix = rotation * scale * translate;
                UpdateVertices();
                prevTransformationMatrix = transformationMatrix;
                Stopwatch2.Stop();
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw(Target, SceneAs<Level>().Camera.Position, Color.White);
            }
            public void RenderVertices(Action<EffectPass> forEachPass)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Effect obj = ShaderHelper.TryGetEffect("towerBarrier");
                BlendState blendState2 = BlendState.AlphaBlend;
                Vector2 vector = new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height);
                Matrix matrix = Matrix.Identity;
                matrix *= Matrix.CreateScale(1f / vector.X * 2f, (0f - 1f / vector.Y) * 2f, 1f);
                matrix *= Matrix.CreateTranslation(-1f, 1f, 0f);
                Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                Engine.Instance.GraphicsDevice.BlendState = blendState2;
                Matrix rotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
                Matrix scale = Matrix.CreateScale(Scale);
                Matrix translation = Matrix.CreateTranslation(HalfSize);
                obj.Parameters["World"].SetValue(translation * matrix);
                obj.Parameters["Push"]?.SetValue(Push);
                obj.Parameters["Time"]?.SetValue(Scene.TimeActive);
                if (Windows.Count > 0)
                {
                    Window last = Windows.Last();
                    Window first = Windows.First();
                    if (last.Primitives.Count > 0 && first.Primitives.Count > 0)
                    {
                        Window.Primitive pLast = last.Primitives.Last();
                        Window.Primitive pFirst = first.Primitives.First();
                        obj.Parameters["MaxZ"]?.SetValue(pLast.Rank);
                        obj.Parameters["MinZ"]?.SetValue(pFirst.Rank);
                    }
                }
                foreach (EffectPass pass in obj.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    forEachPass?.Invoke(pass);
                }
            }
            public bool Mode = true;
            public void ForEachPass(EffectPass pass)
            {
                Stopwatch.Restart();
                foreach (Window w in Windows)
                {
                    w.Render();
                }
                Stopwatch.Stop();
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Target?.Dispose();
            }
            public void CreateVertices(int width, int height, int padx, int pady)
            {
                int size = 16;
                cols = width / size + padx * 2;
                rows = height / size + pady * 2;
                HalfSize = new Vector3((cols * size) / 2, (rows * size) / 2, 0);
                int vertices = cols * rows;
                modMapPoints = new VirtualMap<MapPoint>(cols, rows);
                mapPoints2D = new VirtualMap<MapPoint>(cols, rows);
                Windows.Clear();
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        float oddRowOffsetMult = 1 - (r % 2);
                        Vector3 position = -HalfSize + new Vector3(
                            x: c * size + ((size / 2) * oddRowOffsetMult) - (size * padx),
                            y: r * size - (size * pady), 0);
                        mapPoints2D[c, r] = new MapPoint(position); //2d reference of points
                        modMapPoints[c, r] = new MapPoint(position); //points that will be rotated based off of 2d references
                    }
                }
                for (int i = 1; i < vertices - cols; i++)
                {
                    int r = i / cols;
                    int c = i % cols;
                    if (c == 0) continue;
                    //cross

                    if (r % 2 == 1)
                    {
                        Windows.Add(new Window(modMapPoints[c - 1, r], modMapPoints[c, r], modMapPoints[c - 1, r + 1]));
                        Windows.Add(new Window(modMapPoints[c - 1, r + 1], modMapPoints[c, r], modMapPoints[c, r + 1]));
                    }
                    else
                    {
                        Windows.Add(new Window(modMapPoints[c - 1, r], modMapPoints[c - 1, r + 1], modMapPoints[c, r + 1]));
                        Windows.Add(new Window(modMapPoints[c - 1, r], modMapPoints[c, r], modMapPoints[c, r + 1]));
                    }
                }
                UpdateVertices();
            }
            public void UpdateVertices()
            {
                UpdateMapPoints();
                foreach (Window w in Windows)
                {
                    w.Update();
                }
                Windows.Sort();
            }
            public void UpdateMapPoints()
            {
                Matrix transform = scale * rotation;
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        Vector3 pushed = CalculateZ(Push, mapPoints2D[j, i].Position);
                        Vector3 position = Vector3.Transform(pushed, transform);
                        modMapPoints[j, i].Position = position;
                        modMapPoints[j, i].Pushed = pushed != mapPoints2D[j, i].Position;
                    }
                }
            }
            public static Vector3 CalculateZ(float push, Vector3 input)
            {
                if (push == 0) return input;
                float len = input.XY().Length();
                float abs = Math.Abs(push);
                if (len < abs)
                {
                    input.Z = (1 - len / abs) * push;
                }
                return input;
            }
            public void BreakWindow(Window w, int cracks = 1)
            {
                w.Break(cracks);
            }
            public void FallWindow(Window w, float rotationRange, float speedX, float speedY, float ySpeedTarget, float ySpeedMove)
            {
                w.Detach(rotationRange, speedX, speedY, ySpeedTarget, ySpeedMove);
            }
            public void FallWindow(Window w)
            {
                w.Detach(12f, Calc.Random.Range(-40f, 40f), Calc.Random.Range(-30, -120f), Player.Gravity, 900f);
            }
            public void Clear()
            {
                Windows.Clear();
            }
        }

        public class GlowEntity : Entity
        {

        }
        public class Eye : GlowEntity
        {

        }
        public class Platform : GlowEntity
        {

        }
        public class Blocks : GlowEntity
        {

        }
    }
}