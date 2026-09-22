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
    [Obsolete("Use AscendBarrierManager.cs")]
    public class AscendBarrierManagerBeta : Entity
    {
        public class Barrier : Entity
        {
            //Class that holds a point that all vertex positions are based on
            private class MapPoint(Vector3 position)
            {
                public Vector3 Position = position;
                public bool Pushed;
            }
            //Struct to allow variation without disrupting the position held in MapPoint
            private struct MapPointRef(MapPoint mapPoint)
            {
                public MapPoint MapPoint = mapPoint;
                public readonly Vector3 Render => MapPoint != null ? MapPoint.Position + Offset : Offset;
                //Any change to the visual position must be stored in Offset
                public Vector3 Offset;
                public Color Color;
            }
            //Triangle list gets data from these
            private class VertComponent : Component, IComparable<VertComponent>
            {
                public float Rank;
                public int[] Indices;
                public VertComponent() : base(true, false) { }
                public int CompareTo(VertComponent other)
                {
                    return Math.Sign(Rank - other.Rank);
                }
            }
            private class VertTriangle : VertComponent, IComparable<VertTriangle>
            {
                public List<SubTriangle> SubTriangles = [];
                public List<ShatterLine> ShatterLines = [];
                public float PrevRank;
                public MapPointRef A, B, C;
                public float Alpha
                {
                    get => alpha * AlphaMult;
                    set => alpha = value;
                }
                private float alpha = 0.3f;
                public float AlphaMult = 1;
                public bool FallenOut;

                public VertTriangle(MapPoint a, MapPoint b, MapPoint c, int index)
                {
                    Indices = [index, index + 1, index + 2];
                    A = new MapPointRef(a); B = new MapPointRef(b); C = new MapPointRef(c);
                }
                public void CreatePane(float invYaw, float invPitch, float invRoll, float push, Vector2 speed, float yawRate, float pitchRate, float rollRate, float ySpeedTarget, float ySpeedMove)
                {
                    FallenOut = true;
                    List<VertexPositionColor> tris = [];
                    List<VertexPositionColor> lines = [];
                    var a = A.Render;
                    var b = B.Render;
                    var c = C.Render;
                    tris.AddRange([
                        new(a, Color.White * 0.3f),
                        new(b, Color.White * 0.3f),
                        new(c, Color.White * 0.3f)
                        ]);

                    List<VertexPositionColor> subs = [];
                    foreach (var s in SubTriangles)
                    {
                        var a2 = s.PositionA;
                        var b2 = s.PositionB;
                        var c2 = s.PositionC;
                        if (s.UsePositionD)
                        {
                            var d = s.PositionD;
                            tris.AddRange([
                                new(a2, s.ColorA * s.AlphaA),
                                new(b2, s.ColorB * s.AlphaB),
                                new(d, s.ColorD * s.AlphaD),
                                new(d, s.ColorD * s.AlphaD),
                                new(c2, s.ColorC * s.AlphaC),
                                new(a2, s.ColorA * s.AlphaA)
                                ]);
                        }
                        else
                        {
                            tris.AddRange([
                                new(a, s.ColorA * s.AlphaA),
                                new(b, s.ColorB * s.AlphaB),
                                new(c, s.ColorC * s.AlphaC)
                                ]);
                        }
                        s.AlphaMult = 0;
                    }

                    foreach (var s in ShatterLines)
                    {
                        lines.Add(new(s.PositionA, Color.White));
                        if (s.Jagged)
                        {
                            var mid = s.Mid;
                            lines.Add(new(mid, Color.White));
                            lines.Add(new(mid, Color.White));
                        }
                        lines.Add(new(s.PositionB, Color.White));
                        s.AlphaMult = 0;
                    }
                    //VertTriangle lines
                    lines.AddRange([
                        new(a, Color.White),
                        new(b, Color.White),
                        new(b, Color.White),
                        new(c, Color.White),
                        new(c, Color.White),
                        new(a, Color.White)
                        ]);
                    Scene.Add(new Pane(invYaw, invPitch, invRoll, lines, tris, speed, yawRate, pitchRate, rollRate, ySpeedTarget, ySpeedMove));
                    AlphaMult = 0;
                }

                public int CompareTo(VertTriangle other)
                {
                    return Math.Sign(other.Rank - Rank);
                }
            }
            //Lines[] gets data from these
            private class VertLine : VertComponent, IComparable<VertLine>
            {
                public MapPointRef A, B;
                public float AlphaA = 1, AlphaB = 1;
                public float Alpha = 1;
                public Color ColorA, ColorB;
                public Action<VertLine> OnUpdate;
                public VertLine(MapPoint a, MapPoint b, int index)
                {
                    Indices = [index, index + 1];
                    A = new MapPointRef(a);
                    B = new MapPointRef(b);
                    ColorA = Color.LightGray;
                    ColorB = Color.LightGray;
                }
                public void CreatePane(float invYaw, float invPitch, float invRoll, float push, Vector2 speed, float yawRate, float pitchRate, float rollRate, float ySpeedTarget, float ySpeedMove)
                {

                    List<VertexPositionColor> lines = [];
                    lines.AddRange([
                        new(CalculateZ(push, A.Render), ColorA),
                        new(CalculateZ(push, B.Render), ColorB)
                        ]);
                    Scene.Add(new Pane(invYaw, invPitch, invRoll, lines, null, speed, yawRate, pitchRate, rollRate, ySpeedTarget, ySpeedMove));
                }
                public override void Update()
                {
                    base.Update();
                    OnUpdate?.Invoke(this);
                }
                public int CompareTo(VertLine other)
                {
                    return Math.Sign(other.Rank - Rank);
                }
            }
            private class SubTriangleBeta
            {
                public VertTriangle Parent;
                public float LerpA, LerpB, LerpC;
                public MapPointRef TargetA => LerpA < 0 ? Parent.C : Parent.B;
                public MapPointRef TargetB => LerpB < 0 ? Parent.A : Parent.C;
                public MapPointRef TargetC => LerpC < 0 ? Parent.B : Parent.A;
                public Vector3 PositionA => Vector3.Lerp(Parent.A.Render, TargetA.Render, Math.Abs(LerpA));
                public Vector3 PositionB => Vector3.Lerp(Parent.B.Render, TargetB.Render, Math.Abs(LerpB));
                public Vector3 PositionC => Vector3.Lerp(Parent.C.Render, TargetC.Render, Math.Abs(LerpC));
                public float MinLerp, MaxLerp;
                public int? AnchorIndex;
                public Color ColorA, ColorB, ColorC;
                public float AlphaA = 1, AlphaB = 1, AlphaC = 1;
                public SubTriangleBeta(VertTriangle parent, float approachA, float approachB, float approachC, float minLerp, float maxLerp, int? anchorIndex)
                {
                    Parent = parent;
                    //Parent.SubTriangles.Add(this);
                    LerpA = approachA;
                    LerpB = approachB;
                    LerpC = approachC;
                    MinLerp = minLerp;
                    MaxLerp = maxLerp;
                    AnchorIndex = anchorIndex;
                    List<int> toGetColor = [0, 1, 2];
                    if (AnchorIndex.HasValue)
                    {
                        toGetColor.Remove(AnchorIndex.Value);
                        toGetColor.RemoveAt(Calc.Random.Range(0, 2));
                        toGetColor.Add(AnchorIndex.Value);
                    }
                    else
                    {
                        toGetColor.Remove(Calc.Random.Range(0, 3));
                    }
                    foreach (int i in toGetColor)
                    {
                        float mult = (AnchorIndex.HasValue && AnchorIndex.Value == i) ? 1.3f : 1;
                        switch (i)
                        {
                            case 0:
                                ColorA = Color.White * Calc.Random.Range(0.1f, 0.5f) * mult;
                                break;
                            case 1:
                                ColorB = Color.White * Calc.Random.Range(0.1f, 0.5f) * mult;
                                break;
                            case 2:
                                ColorC = Color.White * Calc.Random.Range(0.1f, 0.5f) * mult;
                                break;
                        }
                    }
                }
            }
            //Smaller triangle contained within a VertTriangle. Can also be a 4-sided polygon if necessary.
            private class SubTriangle
            {
                private MapPointRef this[int i]
                {
                    get
                    {
                        return i switch
                        {
                            0 => Parent.A,
                            1 => Parent.B,
                            2 => Parent.C,
                        };
                    }
                }

                public VertTriangle Parent;
                public float Lerp;
                public Vector3 PositionA => this[Anchor].Render;
                public Vector3 PositionB => Vector3.Lerp(PositionA, this[targetIndices[0]].Render, Lerp);
                public Vector3 PositionC => Vector3.Lerp(PositionA, this[targetIndices[1]].Render, Lerp);
                public Vector3 PositionD;
                public int Anchor;
                public bool UsePositionD;
                public Color ColorA, ColorB, ColorC, ColorD;
                public float AlphaA = 1, AlphaB = 1, AlphaC = 1, AlphaD = 1;
                public float Alpha
                {
                    get => alpha * AlphaMult;
                    set => alpha = value;
                }
                private float alpha = 1;
                public float AlphaMult = 1;
                private int[] targetIndices = new int[2];

                public SubTriangle(VertTriangle parent, float lerp, int anchor)
                {
                    List<int> indices = [0, 1, 2];
                    indices.Remove(Math.Clamp(anchor, 0, 2));
                    targetIndices = [.. indices];
                    Parent = parent;
                    Parent.SubTriangles.Add(this);
                    Lerp = lerp;
                    Anchor = anchor;
                    ColorA = Color.White * 0.6f;
                    if (Calc.Random.Chance(0.5f))
                    {
                        ColorB = Color.White * Calc.Random.Range(0.4f, 0.6f);
                        ColorC = Color.Transparent;
                    }
                    else
                    {
                        ColorC = Color.White * Calc.Random.Range(0.4f, 0.6f);
                        ColorB = Color.Transparent;
                    }
                    ColorD = Color.White * Calc.Random.Range(0.4f, 0.8f);
                }
            }
            //SubTriangle inner outlines
            private class ShatterLine : Component
            {
                public VertTriangle Parent;
                private SubTriangle linkedSubTriangle;
                public float Lerp;
                public float BCAngleOffset;
                public bool Jagged;
                public float JaggedTimer;
                public Vector3 PositionA => linkedSubTriangle.PositionB;
                public Vector3 PositionB => linkedSubTriangle.PositionC;
                public Vector3 Mid => linkedSubTriangle.PositionD;
                public float Alpha
                {
                    get => alpha * AlphaMult;
                    set => alpha = value;
                }
                private float alpha = 1;
                public float AlphaMult = 1;
                public float JaggedMult;
                public ShatterLine(VertTriangle parent, SubTriangle link, float lerp, bool jagged) : base(true, false)
                {
                    linkedSubTriangle = link;
                    Parent = parent;
                    parent.ShatterLines.Add(this);
                    Lerp = lerp;
                    Jagged = jagged;
                    if (jagged)
                    {
                        linkedSubTriangle.UsePositionD = true;
                        linkedSubTriangle.PositionD = GetMid(linkedSubTriangle.PositionB.XY(), linkedSubTriangle.PositionC.XY()).ToVec3();
                        JaggedTimer = Calc.Random.Range(0.05f, 0.15f);
                        linkedSubTriangle.AlphaC = 0;
                        linkedSubTriangle.AlphaB *= 0.5f;
                    }
                    BCAngleOffset = Calc.Random.NextAngle() * 0.15f * Calc.Random.Sign();
                }
                public Vector2 GetMid(Vector2 b, Vector2 c)
                {
                    float angle = Calc.Angle(b, c) + BCAngleOffset;
                    float length = (b - c).Length();
                    Vector2 mid = Calc.AngleToVector(angle, length / 2);
                    return b + mid;
                }
                public override void Update()
                {
                    base.Update();
                    if (Jagged)
                    {
                        linkedSubTriangle.PositionD = GetMid(linkedSubTriangle.PositionB.XY(), linkedSubTriangle.PositionC.XY()).ToVec3();
                        if (JaggedTimer > 0)
                        {
                            JaggedTimer -= Engine.DeltaTime;
                            if (JaggedTimer < 0)
                            {
                                Parent.AlphaMult = 0;
                                JaggedMult = 1;
                                linkedSubTriangle.AlphaA = linkedSubTriangle.AlphaB = linkedSubTriangle.AlphaC = 1;
                            }
                        }
                    }
                }
            }
            //Todo: make small shard textures


            private class Window : IComparable<Window>
            {
                public bool Attached;
                public bool Pushed => points[0].MapPoint.Pushed || points[1].MapPoint.Pushed || points[2].MapPoint.Pushed;
                public float Rank;
                public MapPointRef[] points;
                private Vector3 speedVector;
                private class primitive : IComparable<primitive>
                {
                    public bool Visible = true;
                    public float Rank;
                    public VertexPositionColor[] Vertices;
                    public PrimitiveType Type;
                    public int Verts;
                    public Color Color = Color.White;
                    public float Alpha = 1;
                    public int Primitives = 1;
                    public primitive(int verts)
                    {
                        Verts = verts;
                        Vertices = new VertexPositionColor[verts];
                    }
                    public int CompareTo(primitive other)
                    {
                        return Math.Sign(Rank - other.Rank);
                    }
                    public virtual void Update(Vector3 a, Vector3 b, Vector3 c)
                    {

                    }
                    public virtual void Render()
                    {
                        Engine.Instance.GraphicsDevice.DrawUserPrimitives(Type, Vertices, 0, Primitives);
                    }

                }
                private class tri : primitive
                {
                    public float AlphaA = 1, AlphaB = 1, AlphaC = 1;
                    public tri() : base(3)
                    {
                        Type = PrimitiveType.TriangleList;
                        Alpha = 0.3f;
                    }
                    public override void Update(Vector3 a, Vector3 b, Vector3 c)
                    {
                        Vertices[0].Position = a;
                        Vertices[1].Position = b;
                        Vertices[2].Position = c;
                        Vertices[0].Color = Color * Alpha * AlphaA;
                        Vertices[1].Color = Color * Alpha * AlphaB;
                        Vertices[2].Color = Color * Alpha * AlphaC;
                        Rank = (a.Z + b.Z + c.Z) / 3f;
                    }

                }
                private class crack : primitive
                {
                    private static readonly int[] triIndices = [0, 1, 2, 0, 2, 3];
                    private static readonly int[] lineIndices = [1, 2, 2, 3];
                    public float Lerp;
                    public int Anchor;
                    public float ApproachAnchor;
                    public float AlphaA, AlphaB, AlphaC, AlphaBC;
                    public float Delay;
                    private float flashTimer;
                    public crack(float lerp, int anchor, float approachAnchor) : base(4)
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
                    public override void Update(Vector3 a, Vector3 b, Vector3 c)
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
                        Vector3 at = a, bt = b, ct = c;
                        switch (Anchor)
                        {
                            case 1:
                                a = bt;
                                b = ct;
                                c = at;
                                break;
                            case 2:
                                a = ct;
                                b = at;
                                c = bt;
                                break;
                        }
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
                private class line : primitive
                {
                    public float AlphaA, AlphaB;
                    public line() : base(2)
                    {
                        Type = PrimitiveType.LineList;
                    }
                    public override void Update(Vector3 a, Vector3 b, Vector3 c)
                    {
                        Vertices[0].Position = a;
                        Vertices[1].Position = b;
                        Vertices[0].Color = Color.White * Alpha * AlphaA;
                        Vertices[1].Color = Color.White * Alpha * AlphaB;
                        Rank = (a.Z + b.Z) / 2f + 0.000001f;
                    }
                }
                private List<primitive> primitives = [];
                private tri triangle;

                private bool broken;
                private bool detached;
                private float yaw, pitch, roll, speedX, speedY, ySpeedTarget, ySpeedMove;
                private float yawRate, pitchRate, rollRate;
                private float flashTimer;
                private Matrix origRotation;
                public bool Active = true;
                public bool Visible = true;
                public Window(MapPoint a, MapPoint b, MapPoint c)
                {
                    points = [new(a), new(b), new(c)];
                    primitives.Add(triangle = new tri());
                    primitives.Add(new line());
                    primitives.Add(new line());
                    primitives.Add(new line());
                }
                public void Break(int cracks = 1)
                {
                    broken = true;

                    for (int i = 0; i < cracks; i++)
                    {
                        crack c = new crack(Calc.Random.Range(0.2f, 0.8f), Calc.Random.Range(0, 3), Calc.Random.Choose(0, 0, Calc.Random.Range(-0.1f, 0.6f)));
                        c.Delay = 0.3f + i * 0.07f;
                        primitives.Add(c);
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
                    detached = true;
                }
                public void Update()
                {
                    if (flashTimer > 0)
                    {
                        flashTimer -= Engine.DeltaTime;
                        if (flashTimer <= 0)
                        {
                            flashTimer = 0;
                            if (broken) triangle.Alpha = 0;
                        }
                    }
                    Vector3 a = points[0].Render;
                    Vector3 b = points[1].Render;
                    Vector3 c = points[2].Render;
                    if (detached)
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
                    foreach (var p in primitives)
                    {
                        p.Update(a, b, c);
                    }
                    primitives.Sort();
                    Rank = (a.Z + b.Z + c.Z) / 3f;
                }
                public void Render()
                {
                    foreach (primitive p in primitives)
                    {
                        if (p.Visible) p.Render();
                    }
                }
                public int CompareTo(Window other)
                {
                    return Math.Sign(Rank - other.Rank);
                }
            }



            [Tracked]
            public class Pane : Entity
            {
                private VertexPositionColor[] origLines, origTris;
                private VertexPositionColor[] lines, tris;
                public float Yaw, Pitch, Roll;
                private float yawRate, pitchRate, rollRate;
                public Vector2 Speed;
                public Vector2 RenderOffset;
                public Vector2 OrigCenter;
                private int triangleCount;
                private int lineCount;
                private float ySpeedTarget, ySpeedMove;
                private float rotationTime = 0.9f;
                private float timer = 0.9f;
                public Pane(float inverseYaw, float inversePitch, float inverseRoll, List<VertexPositionColor> lineVerts, List<VertexPositionColor> triVerts, Vector2 speed, float yawRate, float pitchRate, float rollRate, float ySpeedTarget, float ySpeedMove)
                {
                    /*                    invYaw = inverseYaw;
                                        invPitch = inversePitch;
                                        invRoll = inverseRoll;*/
                    Speed = speed;
                    this.ySpeedTarget = ySpeedTarget;
                    this.ySpeedMove = ySpeedMove;
                    this.yawRate = yawRate;
                    this.pitchRate = pitchRate;
                    this.rollRate = rollRate;
                    int left = int.MaxValue;
                    int right = int.MinValue;
                    int top = int.MaxValue;
                    int bottom = int.MinValue;
                    foreach (VertexPositionColor vert in lineVerts)
                    {
                        left = (int)Math.Min(left, vert.Position.X);
                        right = (int)Math.Max(right, vert.Position.X);
                        top = (int)Math.Min(top, vert.Position.Y);
                        bottom = (int)Math.Max(bottom, vert.Position.Y);
                    }
                    foreach (VertexPositionColor vert in triVerts)
                    {
                        left = (int)Math.Min(left, vert.Position.X);
                        right = (int)Math.Max(right, vert.Position.X);
                        top = (int)Math.Min(top, vert.Position.Y);
                        bottom = (int)Math.Max(bottom, vert.Position.Y);
                    }
                    Collider = new Hitbox(right - left, bottom - top);
                    Position = new Vector2(left, top);
                    OrigCenter = Center;
                    Vector3 offset = new Vector3(Center, 0);
                    if (triVerts != null)
                    {
                        tris = new VertexPositionColor[triVerts.Count];
                        origTris = [.. triVerts];
                        for (int i = 0; i < triVerts.Count; i++)
                        {
                            this.tris[i] = triVerts[i];
                            VertexPositionColor modified = triVerts[i];
                            modified.Position -= offset;
                            origTris[i] = modified;
                        }
                        triangleCount = triVerts.Count / 3;
                    }
                    if (lineVerts != null)
                    {
                        lines = new VertexPositionColor[lineVerts.Count];
                        origLines = [.. lineVerts];
                        for (int i = 0; i < lineVerts.Count; i++)
                        {
                            lines[i] = lineVerts[i];
                            VertexPositionColor modified = lineVerts[i];
                            modified.Position -= offset;
                            origLines[i] = modified;
                        }
                        lineCount = lineVerts.Count / 2;
                    }
                }
                public override void Awake(Scene scene)
                {
                    base.Awake(scene);
                    if (lines == null && tris == null) RemoveSelf();
                }
                public void DrawVertices()
                {
                    if (tris != null) Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, tris, 0, triangleCount);
                    if (lines != null) Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, lines, 0, lineCount);
                }

                public override void Update()
                {
                    base.Update();
                    Yaw = (Yaw + yawRate) * Engine.DeltaTime % MathHelper.TwoPi;
                    Pitch = (Pitch + pitchRate * Engine.DeltaTime) % MathHelper.TwoPi;
                    Roll = (Roll + rollRate * Engine.DeltaTime) % MathHelper.TwoPi;
                    timer = Calc.Approach(timer, 0, Engine.DeltaTime);
                    Matrix transform = Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll);

                    RenderOffset += Speed * Engine.DeltaTime;
                    if (Speed.Y != ySpeedTarget) Speed.Y = Calc.Approach(Speed.Y, ySpeedTarget, ySpeedMove * Engine.DeltaTime);
                    if (Speed.X != 0) Speed.X = Calc.Approach(Speed.X, 0, 120f * Engine.DeltaTime);
                    Vector3 offset = new Vector3(Center + RenderOffset, 0);
                    if (lines != null)
                    {
                        for (int i = 0; i < origLines.Length; i++)
                        {
                            lines[i].Position = Vector3.Transform(origLines[i].Position, transform) + offset;
                        }
                    }
                    if (tris != null)
                    {
                        for (int i = 0; i < origTris.Length; i++)
                        {
                            tris[i].Position = Vector3.Transform(origTris[i].Position, transform) + offset;
                        }
                    }
                    Camera c = SceneAs<Level>().Camera;
                    if (Position.Y + RenderOffset.Y + 16 > c.Bottom)
                    {
                        RemoveSelf();
                    }
                }
            }
            public float Scale = 1;
            private int vertices;
            private int triangles;
            private int lines;
            private int subTriangles;
            private int shatterLines;
            public float Variance;
            private VirtualMap<MapPoint> mapPoints;
            private VirtualMap<MapPoint> modMapPoints;
            private Vector3 HalfSize;
            public VertexPositionColor[] TriangleVerts;
            public VertexPositionColor[] LineVerts;
            public VertexPositionColor[] MainVerts;



            public VertexPositionColor[] SubTriangleVerts;
            public VertexPositionColor[] ShatterLineVerts;

            private List<VertTriangle> Triangles = [];
            private List<SubTriangle> SubTriangles = [];
            private List<VertLine> Lines = [];
            private List<ShatterLine> ShatterLines = [];

            private List<Window> Windows = [];

            private List<VertexPositionColor> subTriangleVertList = [];
            private List<VertexPositionColor> shatterLineVertList = [];


            public int[] TriangleIndices;
            public int[] LineIndices;
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
            public static Stopwatch PlayerStopwatch = new Stopwatch();
            [OnLoad]
            public static void Load()
            {
                On.Celeste.Player.Render += Player_Render;
            }
            [OnUnload]
            public static void Unload()
            {
                On.Celeste.Player.Render -= Player_Render;
            }
            private static void Player_Render(On.Celeste.Player.orig_Render orig, Player self)
            {
                PlayerStopwatch.Restart();
                orig(self);
                PlayerStopwatch.Stop();
            }


            public enum Modes
            {
                OneByOne,
                Grouped
            }
            public Modes RenderMode;
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
                    int m = (int)RenderMode;
                    if (m - 1 < 0) m = 1;
                    else m--;
                    RenderMode = (Modes)m;
                    rotIndex--;
                    //pitch -= Engine.DeltaTime * 10;
                    if (rotIndex < 0) rotIndex = 2;
                }, right: () =>
                {
                    int m = ((int)RenderMode + 1) % 2;
                    RenderMode = (Modes)m;
                    //pitch += Engine.DeltaTime * 10;
                    rotIndex = (rotIndex + 1) % 3;
                }, up: () =>
                {
                    Push++;
                    /*                    switch (rotIndex)
                                        {
                                            case 0:
                                                yaw += Engine.DeltaTime * 5;
                                                break;
                                            case 1:
                                                pitch += Engine.DeltaTime * 5;
                                                break;
                                            case 2:
                                                roll += Engine.DeltaTime * 5;
                                                break;
                                        }*/
                }, down: () =>
                {
                    Push--;

                    /*                    switch (rotIndex)
                                        {
                                            case 0:
                                                yaw -= Engine.DeltaTime * 5;
                                                break;
                                            case 1:
                                                pitch -= Engine.DeltaTime * 5;
                                                break;
                                            case 2:
                                                roll -= Engine.DeltaTime * 5;
                                                break;
                                        }*/
                });
                //FieldDebug.Debug(this, "yaw", () => yaw.ToString());
                //FieldDebug.Debug(this, "pitch", () => pitch.ToString());
                //FieldDebug.Debug(this, "roll", () => roll.ToString());
                //FieldDebug.Debug(this, "push", () => Push.ToString());
                //FieldDebug.Debug(this, "Render Time", () => Stopwatch.ToString());
                //FieldDebug.Debug(this, "Update Time", () => Stopwatch2.ToString());
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
                UpdateVertices(false);
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
                if (Triangles.Count > 0 && Lines.Count > 0)
                {
                    obj.Parameters["MaxZ"]?.SetValue(Math.Max(Triangles.Last().Rank, Lines.Last().Rank));
                    obj.Parameters["MinZ"]?.SetValue(Math.Min(Triangles[0].Rank, Lines[0].Rank));
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
                switch (RenderMode)
                {
                    case Modes.OneByOne:
                        foreach (Window w in Windows)
                        {
                            w.Render();
                        }
                        break;
                    case Modes.Grouped:
                        Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, TriangleVerts, 0, triangles);
                        if (subTriangles > 0)
                        {
                            Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, SubTriangleVerts, 0, subTriangles);
                        }
                        if (shatterLines > 0)
                        {
                            Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, ShatterLineVerts, 0, shatterLines);
                        }
                        Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, LineVerts, 0, lines);
                        break;
                }
                /*
                                foreach (Pane shard in Scene.Tracker.GetEntities<Pane>())
                                {
                                    shard.DrawVertices();
                                }*/

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
                vertices = cols * rows;
                triangles = 0;
                lines = 0;
                modMapPoints = new VirtualMap<MapPoint>(cols, rows);
                mapPoints = new VirtualMap<MapPoint>(cols, rows);
                if (Triangles.Count > 0) Triangles.RemoveSelves();
                if (Lines.Count > 0) Lines.RemoveSelves();
                Triangles.Clear();
                Lines.Clear();
                Matrix rotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
                Matrix scale = Matrix.CreateScale(Scale);
                Matrix translation = Matrix.CreateTranslation(HalfSize);
                int totalVertCount = 0;
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        float oddRowOffsetMult = 1 - (r % 2);
                        Vector3 position = new Vector3(
                            x: c * size + ((size / 2) * oddRowOffsetMult) - (size * padx),
                            y: r * size - (size * pady), 0);
                        Vector3 i = -HalfSize + position;
                        mapPoints[c, r] = new MapPoint(i);
                        modMapPoints[c, r] = new MapPoint(i);//Vector3.Transform(i, rotation * scale * translation));
                    }
                }
                for (int i = 1; i < vertices - cols; i++)
                {
                    int r = i / cols;
                    int c = i % cols;
                    if (c == 0) continue;
                    int index = r * cols + c;

                    //top
                    Lines.Add(new VertLine(modMapPoints[c - 1, r], modMapPoints[c, r], totalVertCount));
                    totalVertCount += 2;
                    lines++;

                    //left
                    Lines.Add(new VertLine(modMapPoints[c - 1, r], modMapPoints[c - 1, r + 1], totalVertCount));
                    totalVertCount += 2;
                    lines++;

                    //cross
                    if (r % 2 == 1)
                    {
                        Lines.Add(new VertLine(modMapPoints[c - 1, r + 1], modMapPoints[c, r], totalVertCount));
                        totalVertCount += 2;
                        lines++;

                        Triangles.Add(new VertTriangle(modMapPoints[c - 1, r], modMapPoints[c, r], modMapPoints[c - 1, r + 1], totalVertCount));

                        Windows.Add(new Window(modMapPoints[c - 1, r], modMapPoints[c, r], modMapPoints[c - 1, r + 1]));


                        totalVertCount += 3;
                        triangles++;

                        Triangles.Add(new VertTriangle(modMapPoints[c - 1, r + 1], modMapPoints[c, r], modMapPoints[c, r + 1], totalVertCount));
                        totalVertCount += 3;
                        Windows.Add(new Window(modMapPoints[c - 1, r + 1], modMapPoints[c, r], modMapPoints[c, r + 1]));
                        triangles++;
                    }
                    else
                    {
                        Lines.Add(new VertLine(modMapPoints[c - 1, r], modMapPoints[c, r + 1], totalVertCount));
                        totalVertCount += 2;
                        lines++;

                        Triangles.Add(new VertTriangle(modMapPoints[c - 1, r], modMapPoints[c - 1, r + 1], modMapPoints[c, r + 1], totalVertCount));
                        totalVertCount += 3;
                        Windows.Add(new Window(modMapPoints[c - 1, r], modMapPoints[c - 1, r + 1], modMapPoints[c, r + 1]));
                        triangles++;

                        Triangles.Add(new VertTriangle(modMapPoints[c - 1, r], modMapPoints[c, r], modMapPoints[c, r + 1], totalVertCount));
                        Windows.Add(new Window(modMapPoints[c - 1, r], modMapPoints[c, r], modMapPoints[c, r + 1]));
                        totalVertCount += 3;
                        triangles++;

                    }
                    if (c == cols - 1)
                    {
                        //right
                        Lines.Add(new VertLine(modMapPoints[c, r], modMapPoints[c, r + 1], totalVertCount));
                        totalVertCount += 2;
                        lines++;
                    }
                    if (r == rows - 2)
                    {
                        //bottom
                        Lines.Add(new VertLine(modMapPoints[c - 1, r + 1], modMapPoints[c, r + 1], totalVertCount));
                        totalVertCount += 2;
                        lines++;
                    }
                    #region CoolPattern1
                    /*                    triIndices.Add(index - 1);
                                        triIndices.Add(index);
                                        triIndices.Add(index + cols);
                                        Triangles.Add(new VertTriangle(mapPoints[c - 1, r], mapPoints[c, r], mapPoints[c, r + 1]));
                                        triangles++;

                                        triIndices.Add(index - 1 + cols);
                                        triIndices.Add(index);
                                        triIndices.Add(index + cols);
                                        Triangles.Add(new VertTriangle(mapPoints[c - 1, r + 1], mapPoints[c, r], mapPoints[c, r + 1]));
                                        triangles++;*/
                    #endregion
                }
                MainVerts = new VertexPositionColor[totalVertCount];
                Add([.. Triangles]);
                Add([.. Lines]);
                TriangleVerts = new VertexPositionColor[Triangles.Count * 3];
                LineVerts = new VertexPositionColor[Lines.Count * 2];
                UpdateVertices(true);
            }
            public void UpdateVertices(bool forceOrderUpdate)
            {
                UpdateMapPoints();
                switch (RenderMode)
                {
                    case Modes.OneByOne:
                        foreach (Window w in Windows)
                        {
                            w.Update();
                        }
                        Windows.Sort();
                        break;
                    case Modes.Grouped:
                        UpdateTriangleVertices(forceOrderUpdate);
                        UpdateLineVertices(forceOrderUpdate);
                        break;


                }


                //UpdateAll();
                //UpdateTriangleVertices(forceOrderUpdate);
                //UpdateLineVertices(forceOrderUpdate);
                //UpdateSubTriangleVertices();
                //UpdateShatterLineVertices();
            }
            public void UpdateMapPoints()
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        Vector3 pushed = CalculateZ(Push, mapPoints[j, i].Position);
                        Vector3 position = Vector3.Transform(pushed, scale * rotation);
                        modMapPoints[j, i].Position = position;
                        modMapPoints[j, i].Pushed = pushed != mapPoints[j, i].Position;
                    }
                }
            }
            /*            public void UpdateAll()
                        {
                            int count = 0;
                            bool updateOrder = false;
                            foreach (VertComponent v in All)
                            {
                                float prev = v.Rank;
                                if (v is VertTriangle)
                                {
                                    updateGlobalTri(v as VertTriangle, count);
                                    count += 3;
                                }
                                else
                                {
                                    updateGlobalLine(v as VertLine, count);
                                    count += 2;
                                }

                                if (!updateOrder && prev != v.Rank) updateOrder = true;
                            }
                            if (updateOrder) All.Sort();
                        }*/
            public void UpdateTriangleVertices(bool forceOrderUpdate)
            {
                int count = 0;
                bool updateOrder = forceOrderUpdate;
                foreach (VertTriangle tri in Triangles)
                {
                    updateTri(tri, count);
                    count += 3;
                }
                Triangles.Sort();
            }
            private void updateGlobalTri(VertTriangle tri, int vertIndex)
            {
                var a = tri.A.Render;
                var b = tri.B.Render;
                var c = tri.C.Render;
                int i = tri.Indices[0];
                int j = tri.Indices[1];
                int k = tri.Indices[2];
                tri.Rank = (a.Y + b.Y + c.Y) / 3;
                MainVerts[i].Position = a;
                MainVerts[j].Position = b;
                MainVerts[k].Position = c;
                MainVerts[i].Color = Color.White * tri.Alpha;
                MainVerts[j].Color = Color.White * tri.Alpha;
                MainVerts[k].Color = Color.White * tri.Alpha;
                if (tri.A.MapPoint.Pushed)
                {
                    MainVerts[i].Color = Color.Red;
                }
                if (tri.B.MapPoint.Pushed)
                {
                    MainVerts[j].Color = Color.Red;
                }
                if (tri.C.MapPoint.Pushed)
                {
                    MainVerts[k].Color = Color.Red;
                }
            }
            private void updateGlobalLine(VertLine line, int vertIndex)
            {
                var a = line.A.Render;
                var b = line.B.Render;
                line.Rank = (a.Y + b.Y) / 2f;
                MainVerts[line.Indices[0]].Position = line.A.Render;
                MainVerts[line.Indices[1]].Position = line.B.Render;
                MainVerts[line.Indices[0]].Color = line.ColorA * line.AlphaA * line.Alpha;
                MainVerts[line.Indices[1]].Color = line.ColorB * line.AlphaB * line.Alpha;
                if (line.A.MapPoint.Pushed)
                {
                    MainVerts[line.Indices[0]].Color = Color.Red;
                }
                if (line.B.MapPoint.Pushed)
                {
                    MainVerts[line.Indices[1]].Color = Color.Red;
                }
            }
            private void updateTri(VertTriangle tri, int vertIndex)
            {
                var a = tri.A.Render;
                var b = tri.B.Render;
                var c = tri.C.Render;
                tri.Rank = (a.Y + b.Y + c.Y) / 3;
                TriangleVerts[vertIndex].Position = a;
                TriangleVerts[vertIndex + 1].Position = b;
                TriangleVerts[vertIndex + 2].Position = c;
                TriangleVerts[vertIndex].Color = Color.White * tri.Alpha;
                TriangleVerts[vertIndex + 1].Color = Color.White * tri.Alpha;
                TriangleVerts[vertIndex + 2].Color = Color.White * tri.Alpha;
                if (tri.A.MapPoint.Pushed)
                {
                    TriangleVerts[vertIndex].Color = Color.Red;
                }
                if (tri.B.MapPoint.Pushed)
                {
                    TriangleVerts[vertIndex + 1].Color = Color.Red;
                }
                if (tri.C.MapPoint.Pushed)
                {
                    TriangleVerts[vertIndex + 2].Color = Color.Red;
                }
            }
            private void updateLine(VertLine line, int vertIndex)
            {
                var a = line.A.Render;
                var b = line.B.Render;
                line.Rank = (a.Y + b.Y) / 2f;
                LineVerts[vertIndex].Position = line.A.Render;
                LineVerts[vertIndex + 1].Position = line.B.Render;
                LineVerts[vertIndex].Color = line.ColorA * line.AlphaA * line.Alpha;
                LineVerts[vertIndex + 1].Color = line.ColorB * line.AlphaB * line.Alpha;
                if (line.A.MapPoint.Pushed)
                {
                    LineVerts[vertIndex].Color = Color.Red;
                }
                if (line.B.MapPoint.Pushed)
                {
                    LineVerts[vertIndex + 1].Color = Color.Red;
                }
            }
            public void UpdateLineVertices(bool forceOrderUpdate)
            {
                int count = 0;
                foreach (VertLine line in Lines)
                {
                    int index = count * 2;
                    updateLine(line, index);
                    count++;
                }
                Lines.Sort();


            }
            public void UpdateSubTriangleVertices()
            {
                if (subTriangleVertList.Count > 2)
                {
                    if (SubTriangleVerts == null || subTriangleVertList.Count != SubTriangleVerts.Length)
                    {
                        SubTriangleVerts = [.. subTriangleVertList];
                    }
                    int index = 0;
                    foreach (SubTriangle tri in SubTriangles)
                    {
                        Vector3 a = tri.PositionA;
                        Vector3 b = tri.PositionB;
                        Vector3 c = tri.PositionC;
                        if (tri.UsePositionD)
                        {
                            Vector3 d = tri.PositionD;
                            SubTriangleVerts[index].Position = a;
                            SubTriangleVerts[index + 1].Position = b;
                            SubTriangleVerts[index + 2].Position = d;
                            SubTriangleVerts[index + 3].Position = a;
                            SubTriangleVerts[index + 4].Position = c;
                            SubTriangleVerts[index + 5].Position = d;

                            SubTriangleVerts[index].Color = tri.ColorA * tri.AlphaA * tri.Alpha;
                            SubTriangleVerts[index + 1].Color = tri.ColorB * tri.AlphaB * tri.Alpha;
                            SubTriangleVerts[index + 2].Color = tri.ColorD * tri.AlphaD * tri.Alpha;
                            SubTriangleVerts[index + 3].Color = tri.ColorA * tri.AlphaA * tri.Alpha;
                            SubTriangleVerts[index + 4].Color = tri.ColorC * tri.AlphaC * tri.Alpha;
                            SubTriangleVerts[index + 5].Color = tri.ColorD * tri.AlphaD * tri.Alpha;
                            index += 6;
                        }
                        else
                        {
                            SubTriangleVerts[index].Position = a;
                            SubTriangleVerts[index + 1].Position = b;
                            SubTriangleVerts[index + 2].Position = c;
                            SubTriangleVerts[index].Color = tri.ColorA * tri.AlphaA * tri.Alpha;
                            SubTriangleVerts[index + 1].Color = tri.ColorB * tri.AlphaB * tri.Alpha;
                            SubTriangleVerts[index + 2].Color = tri.ColorC * tri.AlphaC * tri.Alpha;
                            index += 3;
                        }
                    }
                }
            }
            public void UpdateShatterLineVertices()
            {
                if (shatterLineVertList.Count > 1)
                {
                    if (ShatterLineVerts == null || shatterLineVertList.Count != ShatterLineVerts.Length)
                    {
                        ShatterLineVerts = [.. shatterLineVertList];
                    }
                    int index = 0;
                    foreach (ShatterLine line in ShatterLines)
                    {
                        Vector3 a = line.PositionA;
                        Vector3 b = line.PositionB;
                        Color c = Color.White * 0.4f;
                        if (line.Jagged)
                        {
                            Vector3 mid = line.Mid;
                            ShatterLineVerts[index].Position = a;
                            ShatterLineVerts[index + 1].Position = mid;
                            ShatterLineVerts[index + 2].Position = mid;
                            ShatterLineVerts[index + 3].Position = b;
                            ShatterLineVerts[index].Color = Color.White * 0.4f * line.Alpha;
                            ShatterLineVerts[index + 1].Color = c * line.Alpha;
                            ShatterLineVerts[index + 2].Color = c * line.Alpha * line.JaggedMult;
                            ShatterLineVerts[index + 3].Color = c * line.Alpha * line.JaggedMult;
                            index += 4;
                        }
                        else
                        {
                            ShatterLineVerts[index].Position = a;
                            ShatterLineVerts[index + 1].Position = b;
                            ShatterLineVerts[index].Color = c * line.Alpha;
                            ShatterLineVerts[index + 1].Color = c * line.Alpha;
                            index += 2;
                        }
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
            //private int rotIndex;
            private SubTriangleBeta CreateSubTriangleBeta(VertTriangle tri, float minLerp, float maxLerp, bool randomAnchor, int? anchorIndex, bool randomSign, bool randomLerp, List<VertexPositionColor> list)
            {
                int[] mults = [1, 1, 1];
                if (randomAnchor)
                {
                    anchorIndex = Calc.Random.Range(0, 3);
                }
                if (anchorIndex.HasValue && anchorIndex.Value >= 0 && anchorIndex.Value < 3)
                {
                    mults[anchorIndex.Value] = 0;
                }
                int signA = 1, signB = 1, signC = 1;
                float lerpA = Calc.Random.Range(minLerp, maxLerp);
                float lerpB = lerpA, lerpC = lerpA;
                if (randomSign)
                {
                    signA = Calc.Random.Sign();
                    signB = Calc.Random.Sign();
                    signC = Calc.Random.Sign();
                }
                if (randomLerp)
                {
                    lerpB = Calc.Random.Range(minLerp, maxLerp);
                    lerpC = Calc.Random.Range(minLerp, maxLerp);
                }
                SubTriangleBeta st = new(tri, lerpA * mults[0], lerpB * mults[1], lerpC * mults[2], minLerp, maxLerp, anchorIndex);
                //SubTriangles.Add(st);
                list.Add(default);
                list.Add(default);
                list.Add(default);
                subTriangles++;
                return st;


            }
            private SubTriangle CreateSubTriangle(VertTriangle tri, float lerp, int anchorIndex)
            {
                SubTriangle st = new(tri, lerp, anchorIndex);
                SubTriangles.Add(st);
                subTriangleVertList.Add(default);
                subTriangleVertList.Add(default);
                subTriangleVertList.Add(default);
                subTriangles++;
                return st;
            }

            public void CreateSubTriangles(int count, float minLerp, float maxLerp, bool allowOverlap)
            {
                List<VertTriangle> available = allowOverlap ? Triangles : [.. Triangles.Where(item => item.SubTriangles.Count == 0)];
                for (int i = count; i > 0; i--)
                {
                    if (available.Count == 0) break;
                    VertTriangle tri = available.Random();
                    CreateSubTriangle(tri, Calc.Random.Range(minLerp, maxLerp), Calc.Random.Range(0, 3));
                    available.Remove(tri);
                }
                UpdateSubTriangleVertices();
            }
            public void CreateSubTrianglesBeta(int count, float minLerp, float maxLerp, bool allowOverlap, bool randomAnchor = true, bool randomSign = true, bool equalLerp = false)
            {
                List<VertTriangle> available = allowOverlap ? Triangles : [.. Triangles.Where(item => item.SubTriangles.Count == 0)];
                for (int i = count; i > 0; i--)
                {
                    if (available.Count == 0) break;
                    VertTriangle tri = available.Random();
                    CreateSubTriangleBeta(tri, minLerp, maxLerp, randomAnchor, null, randomSign, equalLerp, subTriangleVertList);
                    available.Remove(tri);
                }
                UpdateSubTriangleVertices();
            }
            private void CreateTriPane(VertTriangle tri, float rot, float ySpeedTarget, float ySpeedMove)
            {
                int[] mults = [1, 1, 1];
                mults[Calc.Random.Range(0, 3)] = 0;
                tri.CreatePane(yaw, pitch, roll, Push, new Vector2(Calc.Random.Range(-40f, 40f), Calc.Random.Range(-30, -120f)), Calc.Random.Range(-rot, rot) * mults[0], Calc.Random.Range(-rot, rot) * mults[1], Calc.Random.Range(-rot, rot) * mults[2], ySpeedTarget, ySpeedMove);
            }
            private void CreateLinePane(VertLine line, float rot, float ySpeedTarget, float ySpeedMove)
            {
                int[] mults = [1, 1, 1];
                mults[Calc.Random.Range(0, 3)] = 0;
                line.CreatePane(yaw, pitch, roll, Push, new Vector2(Calc.Random.Range(-40f, 40f), Calc.Random.Range(-30, -120f)), Calc.Random.Range(-rot, rot) * mults[0], Calc.Random.Range(-rot, rot) * mults[1], Calc.Random.Range(-rot, rot) * mults[2], ySpeedTarget, ySpeedMove);
            }
            private void ShatterField(int perTri, float ySpeedTarget, float ySpeedMove, Func<VertTriangle, bool> triShatterCondition, Func<VertLine, bool> lineFallOutCondition = null, Func<VertTriangle, bool> triFallOutCondition = null)
            {
                float rot = 12f;
                foreach (var tri in Triangles)
                {
                    int origCount = tri.ShatterLines.Count;
                    if (triFallOutCondition != null && triFallOutCondition(tri))
                    {
                        CreateTriPane(tri, rot, ySpeedTarget, ySpeedMove);
                    }
                    if (triShatterCondition != null && triShatterCondition(tri))
                    {
                        tri.AlphaMult = 1;
                        for (int j = 0; j < perTri; j++)
                        {
                            ShatterTriangle(tri, Calc.Random.Range(0.3f, 0.8f), Calc.Random.Range(0, 3));
                        }
                    }
                }
                foreach (var line in Lines)
                {
                    if (lineFallOutCondition != null && lineFallOutCondition(line))
                    {
                        CreateLinePane(line, rot, ySpeedTarget, ySpeedMove);
                    }
                }
                UpdateSubTriangleVertices();
                UpdateShatterLineVertices();
                shaker.ShakeFor(0.3f);
            }
            public void ShatterField(int trianglesToAffect, int shattersPerTriangle, bool allowOverlap, bool turnExistingIntoPanes)
            {
                float rot = 8f;
                int paneCount = Scene.Tracker.GetEntities<Pane>().Count;
                foreach (var tri in Triangles)
                {
                    if (tri.FallenOut) continue;
                    int origCount = tri.ShatterLines.Count;
                    if (turnExistingIntoPanes && origCount != 0)
                    {
                        if (paneCount < 350)
                        {
                            CreateTriPane(tri, rot, Player.Gravity, 900f);
                            paneCount++;
                        }
                    }
                    else if (trianglesToAffect > 0 && (allowOverlap || origCount == 0))
                    {
                        tri.AlphaMult = 1;
                        for (int j = 0; j < shattersPerTriangle; j++)
                        {
                            ShatterTriangle(tri, Calc.Random.Range(0.3f, 0.8f), Calc.Random.Range(0, 3));
                        }
                        trianglesToAffect--;
                    }
                }
                UpdateSubTriangleVertices();
                UpdateShatterLineVertices();
                shaker.ShakeFor(0.3f);
            }

            private void ShatterTriangle(VertTriangle tri, float lerp, int anchor)
            {
                bool jagged = Calc.Random.Chance(1f);
                ShatterLine line = new ShatterLine(tri, CreateSubTriangle(tri, lerp, anchor), lerp, jagged);
                tri.ShatterLines.Add(line);
                tri.Alpha = jagged ? 0.6f : 0;
                shatterLineVertList.Add(default);
                shatterLineVertList.Add(default);
                shatterLines++;
                if (jagged)
                {
                    subTriangleVertList.Add(default);
                    subTriangleVertList.Add(default);
                    subTriangleVertList.Add(default);
                    subTriangles++;
                    shatterLineVertList.Add(default);
                    shatterLineVertList.Add(default);
                    shatterLines++;
                }
                Add(line);
                ShatterLines.Add(line);
            }
            public void Clear()
            {
                foreach (SubTriangle s in SubTriangles)
                {
                    s.Parent.SubTriangles.Remove(s);
                }
                SubTriangles.Clear();
                subTriangleVertList.Clear();
                SubTriangleVerts = null;
                subTriangles = 0;
                foreach (ShatterLine line in ShatterLines)
                {
                    line.Parent.ShatterLines.Remove(line);
                }
                ShatterLines.Clear();
                Components.RemoveAll<ShatterLine>();
                shatterLines = 0;
                shatterLineVertList.Clear();
                ShatterLineVerts = null;
                foreach (Pane s in Scene.Tracker.GetEntities<Pane>())
                {
                    s.RemoveSelf();
                }
                foreach (VertTriangle tri in Triangles)
                {
                    tri.AlphaMult = 1;
                    tri.Alpha = 0.3f;
                    tri.FallenOut = false;
                    tri.ShatterLines.Clear();
                    tri.SubTriangles.Clear();
                }
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