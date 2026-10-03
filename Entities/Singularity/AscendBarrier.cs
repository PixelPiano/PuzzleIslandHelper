using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Singularity
{
    [CustomEntity("PuzzleIslandHelper/AscendBarrier")]
    public class AscendBarrier : Entity
    {
        public bool FollowCamera = true;
        //Class that holds a point that all vertex positions are based on
        public class MapPoint(Vector3 position)
        {
            public Vector3 Position = position;
            public float PushAmount;
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
        public static Vector3 ShakeVector;
        //Todo: make small shard textures
        public class Window : IComparable<Window>
        {
            public bool Idle = true;
            public float PushAmount => points.Sum(item => item.MapPoint.PushAmount) / points.Length;
            public float Rank;
            public Vector3 OrigCenter => (OrigPoints[0].Render + OrigPoints[1].Render + OrigPoints[2].Render) / 3f;
            public Vector3 Center => (points[0].Render + points[1].Render + points[2].Render) / 3f;
            public MapPointRef[] OrigPoints;
            public MapPointRef[] points;
            private Vector3 speedVector;
            public List<Primitive> Primitives = [];
            public Tri triangle;
            public Color? Custom;
            public bool Broken;
            public bool Detached;
            public float Alpha = 1;
            private float yaw, pitch, roll, ySpeedTarget, ySpeedMove;
            private float yawRate, pitchRate, rollRate;
            private float flashTimer;
            public bool Active = true;
            public bool Visible = true;
            private Vector3 detachSpeed;
            private bool rotationalDetach;
            private bool fade;
            private float fallRotationSpeed;
            private bool falling;
            private float frictionMult = 1;
            private float gravityMult = 1;
            private float detachTimer;
            private float rotationSpeedMult = 0;
            private float rotationSpeedMultMult = 0;
            public Window(MapPoint a, MapPoint b, MapPoint c)
            {
                points = new MapPointRef[3];
                OrigPoints = [new(a), new(b), new(c)];
                Reset();
            }
            public void BreakAndFall(float rotateSpeed, int cracks = 1)
            {
                Break(cracks);
                Fall(rotateSpeed);
            }
            public void Break(int cracks = 1)
            {
                Broken = true;
                for (int i = 0; i < cracks; i++)
                {
                    Crack c = new Crack(Calc.Random.Range(0.4f, 0.8f), Calc.Random.Range(0, 3), Calc.Random.Choose(0, Calc.Random.Range(-0.1f, 0.6f)));
                    c.Delay = i * 0.02f;
                    Primitives.Add(c);
                }
                triangle.Alpha = 0.6f;
                flashTimer = 0.05f;
                //todo: this
            }
            public void Fall(float rotateSpeed)
            {
                rotationSpeedMultMult = Calc.Random.Range(1, 4);
                Idle = false;
                falling = true;
                fallRotationSpeed = rotateSpeed * Calc.Random.Sign();
                Vector3 a, b, c;
                a = points[0].Render;
                b = points[1].Render;
                c = points[2].Render;

                Vector3 center = (a + b + c) / 3f;
                a = Vector3.Lerp(a, center, 0.25f);
                b = Vector3.Lerp(b, center, 0.25f);
                c = Vector3.Lerp(c, center, 0.25f);
                points[0] = new MapPointRef(new MapPoint(a));
                points[1] = new MapPointRef(new MapPoint(b));
                points[2] = new MapPointRef(new MapPoint(c));
            }
            public void DetachRotational(Matrix matrix, float rot, float speed)
            {
                rotationalDetach = true;
                fade = true;
                Vector2 center = ((OrigPoints[0].Render + OrigPoints[1].Render + OrigPoints[2].Render) / 3f).XY();
                Vector3 beforeRot = new Vector3(Vector2.Normalize(center), 0);
                detachSpeed = Vector3.Transform(beforeRot, matrix) * speed;
                detachSpeed.Z *= 0.1f;
                frictionMult = 0;
                gravityMult = 0;
                detachTimer = 1f;
                Detach(rot, false);
            }
            public void Detach2D(float yawRate, float pitchRate, float rollRate, float speedX, float speedY, float ySpeedTarget, float ySpeedMove)
            {
                detachSpeed = new Vector3(speedX, speedY, 0);

                this.ySpeedTarget = ySpeedTarget;
                this.ySpeedMove = ySpeedMove;
                rotationalDetach = false;
                fade = false;
                Alpha = 1;
                Detach(yawRate, pitchRate, rollRate, true);
            }
            public void Detach(float rot, bool squashZ)
            {
                int[] mult2 = [1, 1, 1];
                mult2[Calc.Random.Range(0, 3)] = 0;
                float rotA = rot * 0.5f;
                float yawRate = Calc.Random.Range(rotA, rot) * mult2[0] * Calc.Random.Sign();
                float pitchRate = Calc.Random.Range(rotA, rot) * mult2[1] * Calc.Random.Sign();
                float rollRate = Calc.Random.Range(rotA, rot) * mult2[2] * Calc.Random.Sign();

                Detach(yawRate, pitchRate, rollRate, squashZ);
            }
            public void Detach(float yawRate, float pitchRate, float rollRate, bool squashZ)
            {
                Idle = false;
                this.yawRate = yawRate;
                this.pitchRate = pitchRate;
                this.rollRate = rollRate;
                yaw = roll = pitch = 0;
                Vector3 mult = new Vector3(1, 1, squashZ ? 0 : 1) * 0.45f;
                //create new references that won't be affected by the main entity's rotation matrix
                points[0] = new MapPointRef(new MapPoint(points[0].Render * mult));
                points[1] = new MapPointRef(new MapPoint(points[1].Render * mult));
                points[2] = new MapPointRef(new MapPoint(points[2].Render * mult));
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
                yaw = pitch = roll = ySpeedTarget = ySpeedMove = yawRate = pitchRate = rollRate = flashTimer = 0;
                detachSpeed = Vector3.Zero;
                fade = false;
                rotationalDetach = false;
                Primitives.Clear();
                Primitives.Add(triangle = new Tri());
                Primitives.Add(new Line());
                Primitives.Add(new Line(1));
                Primitives.Add(new Line(2));
            }
            public void Update()
            {
                if (Math.Abs(PushAmount) > 3 && !Broken)
                {
                    Break(Calc.Random.Range(1, 3));
                }
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
                    speedVector += detachSpeed * Engine.DeltaTime;
                    yaw += yawRate * Engine.DeltaTime;
                    pitch += pitchRate * Engine.DeltaTime;
                    roll += rollRate * Engine.DeltaTime;
                    float y, p, r;
                    if (rotationalDetach)
                    {
                        y = yaw %= MathHelper.TwoPi;
                        p = pitch %= MathHelper.TwoPi;
                        r = roll %= MathHelper.TwoPi;
                    }
                    else
                    {
                        double sinY = (Math.Sin(yaw) + 1) / 2f;
                        double sinP = (Math.Sin(pitch) + 1) / 2f;
                        double sinR = (Math.Sin(roll) + 1) / 2f;
                        y = (float)sinY * MathHelper.PiOver2;
                        p = (float)sinP * MathHelper.PiOver2;
                        r = (float)sinR * MathHelper.PiOver2;
                    }
                    Vector3 center = (a + b + c) / 3f;

                    Matrix m = Matrix.CreateFromYawPitchRoll(y, p, r);
                    a += Vector3.Transform(a - center, m) + speedVector;
                    b += Vector3.Transform(b - center, m) + speedVector;
                    c += Vector3.Transform(c - center, m) + speedVector;
                }
                else if (falling) //i'm falling
                {
                    rotationSpeedMult = Calc.Approach(rotationSpeedMult, 1, Engine.DeltaTime * rotationSpeedMultMult);
                    speedVector.Y += 15f * rotationSpeedMult * Engine.DeltaTime;
                    Vector3 center = (a + b + c) / 3;
                    yaw = (yaw + fallRotationSpeed * rotationSpeedMult * Engine.DeltaTime) % MathHelper.TwoPi;
                    Matrix rotation = Matrix.CreateFromYawPitchRoll(yaw, 0, 0);

                    a += Vector3.Transform(a - center, rotation) + speedVector;
                    b += Vector3.Transform(b - center, rotation) + speedVector;
                    c += Vector3.Transform(c - center, rotation) + speedVector;

                }
                Vector3[] array = [a + ShakeVector, b + ShakeVector, c + ShakeVector];
                foreach (var p in Primitives)
                {
                    float alpha = p.Alpha;
                    p.Alpha *= Alpha;
                    p.Update(array);
                    p.Alpha = alpha;
                }
                Primitives.Sort();
                Rank = (a.Z + b.Z + c.Z) / 3f;
                if (fade)
                {
                    if (detachTimer > 0)
                    {
                        detachTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        Alpha = Calc.Approach(Alpha, 0, Engine.DeltaTime / 2f);
                    }
                }
                if (Alpha <= 0)
                {
                    Active = false;
                    Visible = false;
                    Alpha = 0;

                }
            }
            public void Render(bool bg)
            {
                foreach (Primitive p in Primitives)
                {
                    if (p.Visible && p.BG == bg)
                    {
                        p.Render();
                    }
                }
            }
            public void Render()
            {
                foreach (Primitive p in Primitives)
                {
                    if (p.Visible) p.Render();
                }
            }
            public int CompareTo(Window other)
            {
                return Math.Sign(Rank - other.Rank);
            }
            public class Primitive : IComparable<Primitive>
            {
                public bool Visible = true;
                public bool BG => Rank > 0;
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
                    int sign = Math.Sign(Rank - other.Rank);
                    if (sign != 0)
                    {
                        return sign;
                    }
                    if (this is not Tri)
                    {
                        return 1;
                    }
                    return -1;
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
                    Rank = (a.Z + b.Z) / 2f;
                }
            }
            public class Crack : Primitive
            {
                private static readonly int[] triIndices = [0, 1, 2, 0, 2, 3];
                private static readonly int[] lineIndices = [0, 1, 1, 2];
                public float Lerp;
                public int Anchor;
                public float ApproachAnchor;
                public float AlphaA, AlphaB, AlphaC, AlphaBC;
                public float Delay;
                private float flashTimer;
                private float lineAlpha;
                private VertexPositionColor[] lineVerts;
                public Crack(float lerp, int anchor, float approachAnchor) : base(4)
                {
                    lineVerts = new VertexPositionColor[3];
                    Alpha = 1;
                    Type = PrimitiveType.TriangleList;
                    Primitives = 2;
                    Lerp = lerp;
                    Anchor = anchor;
                    ApproachAnchor = approachAnchor;
                    lineAlpha = Calc.Random.Range(0.75f, 0.9f);
                    AlphaA = 0.6f;
                    if (Calc.Random.Chance(0.5f))
                    {
                        AlphaB = Calc.Random.Range(0.4f, 0.6f);
                        AlphaC = Calc.Random.Range(0.3f, 0.4f);
                    }
                    else
                    {
                        AlphaC = Calc.Random.Range(0.4f, 0.6f);
                        AlphaB = Calc.Random.Range(0.3f, 0.4f);
                    }
                    AlphaBC = Calc.Random.Range(0.4f, 0.6f);
                }
                public override void Render()
                {
                    if (Delay <= 0)
                    {
                        Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Vertices, 0, Verts, triIndices, 0, 2);
                        Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, lineVerts, 0, 3, lineIndices, 0, 2);
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

                    lineVerts[0].Position = ab;
                    lineVerts[1].Position = bc;
                    lineVerts[2].Position = c;

                    if (flashTimer > 0)
                    {
                        Vertices[0].Color = Color * Alpha;
                        Vertices[1].Color = Color * Alpha;
                        Vertices[2].Color = Color * Alpha;
                        Vertices[3].Color = Color * Alpha;
                        for (int i = 0; i < lineVerts.Length; i++)
                        {
                            lineVerts[i].Color = Color * Alpha;
                        }
                    }
                    else
                    {
                        Vertices[0].Color = Color * Alpha * AlphaA;
                        Vertices[1].Color = Color * Alpha * AlphaB;
                        Vertices[2].Color = Color * Alpha * AlphaBC;
                        Vertices[3].Color = Color * Alpha * AlphaC;


                        for (int i = 0; i < lineVerts.Length; i++)
                        {
                            lineVerts[i].Color = Color * Alpha * lineAlpha;
                        }
                    }
                    if (flashTimer > 0) flashTimer -= Engine.DeltaTime;
                    Rank = (a.Z + b.Z + c.Z) / 3f;
                }
            }
        }
        private class fg : Entity
        {
            private AscendBarrier parent;
            public fg(AscendBarrier parent) : base()
            {
                Depth = -1;
                this.parent = parent;
            }
            public override void Render()
            {
                base.Render();
                Draw.SpriteBatch.Draw(parent.FGTarget, parent.RenderPosition, Color.White);
            }
        }
        public float Scale = 1;
        private VirtualMap<MapPoint> mapPoints2D;
        private VirtualMap<MapPoint> modMapPoints;
        private Vector3 HalfSize;

        private List<Window> Windows = [];
        public VirtualRenderTarget BGTarget, FGTarget;
        public Entity FGEntity;
        private int cols, rows;
        private BetterShaker shaker;
        public float Push
        {
            get => push + bulgeShake;
            set => push = value;
        }
        private float push;
        private float yaw, pitch, roll;
        public float Pitch
        {
            get => pitch;
            set => pitch = value;
        }
        private Matrix rotation, scale, translate;
        public float MaxDist;
        public float MaxZ;
        public float TotalHeight;
        public Vector2 Dimensions;
        public Stopwatch Stopwatch;
        public Stopwatch Stopwatch2;
        public bool CameraRender = true;
        private float bulgeShake;
        public Vector2 Pad;
        private Tween bulgeShakeTween;
        public bool Shattered;
        public int WindowSize;
        private Matrix beforeRenderTranslate;
        public void StartShakingBulge()
        {
            bulgeShakeTween.Start();
        }
        public void StopShakingBulge()
        {
            bulgeShakeTween.Stop();
        }
        public AscendBarrier(EntityData data, Vector2 offset) : this(data.Position + offset)
        {
        }
        public AscendBarrier(Vector2 position) : this()
        {
            Position = position;
        }
        public AscendBarrier() : base()
        {
            Collider = new Hitbox(160, 8);
            Depth = 1;
            bulgeShakeTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.ElasticInOut, 0.05f, false);
            bulgeShakeTween.OnUpdate = (t) => bulgeShake = t.Eased;
            Add(bulgeShakeTween);
            pitch = 45f.ToRad();
            //to = 65
            BGTarget = VirtualContent.CreateRenderTarget("TowerBarrierBG", 320, 180);
            FGTarget = VirtualContent.CreateRenderTarget("TowerBarrierFG", 320, 180);
            Add(new BeforeRenderHook(RenderVertices));
            Add(shaker = new BetterShaker((v) => ShakeVector += v.ToVec3()));
            int c = 0;
            KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.B, () =>
            {
                //CreateVertices(320, 180, 0, 0, 20);
                //Add(new Coroutine(shatterRoutine()));
            });
/*            KeyComponent.Wasd(entity: this, left: () => CreateVertices(WindowSize - 8),
                up: () => Add(new Coroutine(routine())),
                right: () => CreateVertices(WindowSize + 8),
                down: () => Components.RemoveAll<Coroutine>());*/
        }
        public void ShakeFor(float time = -1)
        {
            shaker.ShakeFor(time);
        }

        public void StartRotation(float from, float to, float time, Ease.Easer ease)
        {
            Tween.Set(this, Tween.TweenMode.Oneshot, time, ease, t => pitch = Calc.LerpClamp(from, to, t.Eased), t => pitch = to);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            beforeRenderTranslate = Matrix.CreateTranslation(160, 90, 0);
            ShakeVector = default;
            scene.Add(FGEntity = new fg(this));
            CreateVertices(60);
        }
        public override void Update()
        {
            base.Update();
            Level level = SceneAs<Level>();
            Position.X = level.Bounds.X;
            scale = Matrix.CreateScale(Scale);
            rotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
            UpdateVertices();
        }
        public Vector2 RenderPosition
        {
            get
            {
                if (FollowCamera)
                {
                    return SceneAs<Level>().Camera.Position;
                }
                else
                {
                    return Position;
                }
            }
        }
        public Vector3 VertexOffset;
        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Draw(BGTarget, RenderPosition, Color.White);
        }
        public void RenderVertices()
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(BGTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            //ShaderHelperIntegration.TryGetEffect("PuzzleIslandHelper/Shaders/towerBarrier");
            Effect obj = ShaderHelper.TryGetEffect("towerBarrier");
            BlendState blendState2 = BlendState.AlphaBlend;
            Vector2 vector = new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height);
            Matrix matrix = Matrix.Identity;
            matrix *= Matrix.CreateScale(1f / vector.X * 2f, (0f - 1f / vector.Y) * 2f, 1f);
            matrix *= Matrix.CreateTranslation(-1f, 1f, 0f);
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Engine.Instance.GraphicsDevice.BlendState = blendState2;
            obj.Parameters["World"]?.SetValue(beforeRenderTranslate * matrix);
            obj.Parameters["Push"]?.SetValue(Push);
            obj.Parameters["Time"]?.SetValue(Scene.TimeActive);
            obj.Parameters["Width"]?.SetValue(HalfSize.X * 2);
            obj.Parameters["Height"]?.SetValue(HalfSize.Y * 2);
            obj.Parameters["MaxScale"]?.SetValue(2);
            obj.Parameters["Offset"]?.SetValue(VertexOffset);
            foreach (EffectPass pass in obj.CurrentTechnique.Passes)
            {
                pass.Apply();
                RenderWindows(true);
            }
            Engine.Graphics.GraphicsDevice.SetRenderTarget(FGTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            foreach (EffectPass pass in obj.CurrentTechnique.Passes)
            {
                pass.Apply();
                RenderWindows(false);
            }
        }
        public void RenderWindows(bool bg)
        {
            foreach (Window w in Windows)
            {
                if (w.Visible)
                {
                    w.Render(bg);
                }
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            BGTarget?.Dispose();
            FGTarget?.Dispose();
            FGEntity?.RemoveSelf();
            ShakeVector = default;
        }
        public void CreateVertices(int size) => CreateVertices(320, 180, size, 2);
        public void CreateVertices(int width, int height, int size, int pad)
        {
            WindowSize = size;
            cols = width / size + pad * 2;
            rows = height / size + pad * 2;
            int vertices = cols * rows;
            //RenderOffset = new Vector2((320 - width) / 2, (180 - height) / 2);
            modMapPoints = new VirtualMap<MapPoint>(cols, rows);
            mapPoints2D = new VirtualMap<MapPoint>(cols, rows);
            Windows.Clear();
            float left = int.MaxValue, right = int.MinValue, top = int.MaxValue, bottom = int.MinValue;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    float oddRowOffsetMult = 1 - (r % 2);
                    Vector3 position = new Vector3(
                        x: c * size + ((size / 2) * oddRowOffsetMult),
                        y: r * size, 0);
                    left = Math.Min(left, position.X);
                    right = Math.Max(right, position.X);
                    top = Math.Min(top, position.Y);
                    bottom = Math.Max(bottom, position.Y);
                    mapPoints2D[c, r] = new MapPoint(position); //2d reference of points
                    modMapPoints[c, r] = new MapPoint(position); //points that will be rotated based off of 2d references
                }
            }
            HalfSize = new Vector3((right - left) / 2, (bottom - top) / 2, 0);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    mapPoints2D[c, r].Position -= HalfSize;
                    modMapPoints[c, r].Position -= HalfSize;
                }
            }
            for (int i = 1; i < vertices - cols + 1; i++)
            {
                int r = i / cols;
                int c = i % cols;
                if (c == 0) continue;

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
        private IEnumerator routine()
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.7f)
            {
                Pitch = Calc.LerpClamp(45f, 60f, i).ToRad();
                yield return null;
            }
            Player player = Scene.GetPlayer();
            float maxSpeed = 180f;
            float speed = 10f;

            while (Y < player.Y)
            {
                Position.Y += speed * Engine.DeltaTime;
                speed = Calc.Approach(speed, maxSpeed, 200f * Engine.DeltaTime);
                yield return null;
            }
            Position.Y = player.Y;
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
            float top = int.MaxValue, bottom = int.MinValue;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Vector3 pushed = CalculateZ(mapPoints2D[j, i].Position);
                    Vector3 position = Vector3.Transform(pushed, transform);
                    modMapPoints[j, i].Position = position;
                    modMapPoints[j, i].PushAmount = pushed.Z;
                    top = Math.Min(top, position.Y);
                    bottom = Math.Max(bottom, position.Y);
                }
            }
            TotalHeight = bottom - top;
        }
        public void DetachPushedWindows()
        {
            List<Window> list = [.. Windows.OrderBy(item => Math.Abs(item.PushAmount)).Where(item => Math.Abs(item.PushAmount) > 0)];
            if (list.Count >= 2)
            {
                Window a = list[0], b = list[1];
                Vector3 centerA = a.Center, centerB = b.Center;
                int[] mults = new int[2];
                mults[0] = centerA.X < centerB.X ? -1 : 1;
                mults[1] = mults[0] * -1;

                float rate = 2;
                list[0].Detach2D(rate, 0.05f, rate / 1.5f, 130 * mults[0], -50f, 0, 60f);
                list[1].Detach2D(-rate, 0.05f, -rate / 1.5f, 130 * mults[1], -50f, 0, 60f);
                list.RemoveRange(0, 2);
            }
            Matrix m = (scale) * rotation;
            foreach (Window w in list)
            {
                w.DetachRotational(m, 24, Calc.Random.Range(400f, 500f));
            }
        }
        public void Shatter()
        {
            Add(new Coroutine(shatterRoutine()));
        }
        public void BreakFromCenter(int groupSize, float groupDelay)
        {
            Add(new Coroutine(radialBreak(groupSize, groupDelay)));
        }
        public void FallFromCenter(int groupSize, float groupDelay)
        {
            Add(new Coroutine(radialFall(groupSize, groupDelay)));
        }
        private IEnumerator shatterRoutine()
        {
            BreakFromCenter(2, 0.001f);
            yield return 0.4f;
            DetachPushedWindows();
            Shattered = true;
            yield return null;
            FallFromCenter(1, 0.002f);
        }
        private IEnumerator radialBreak(int groupSize, float groupDelay)
        {
            Dictionary<int, List<Window>> dictionary = [];
            foreach (Window window in Windows)
            {
                if (window.Detached) continue;
                int length = (int)window.OrigCenter.Length();
                if (!dictionary.TryGetValue(length, out List<Window> value))
                {
                    value = [window];
                    dictionary.Add(length, value);
                }
                else
                {
                    dictionary[length].Add(window);
                }
            }
            int buffer = groupSize;
            foreach (var pair in dictionary.OrderBy(item => item.Key))
            {
                foreach (Window w2 in pair.Value)
                {
                    w2.Break(Calc.Random.Range(1, 3));
                }
                buffer--;
                if (buffer <= 0)
                {
                    buffer = groupSize;
                    yield return groupDelay;
                }
            }

        }
        private IEnumerator radialFall(int groupSize, float groupDelay)
        {
            Dictionary<int, List<Window>> dictionary = [];
            foreach (Window window in Windows)
            {
                if (window.Detached) continue;
                int length = (int)window.OrigCenter.Length();
                if (!dictionary.TryGetValue(length, out List<Window> value))
                {
                    value = [window];
                    dictionary.Add(length, value);
                }
                else
                {
                    dictionary[length].Add(window);
                }
            }
            int buffer = groupSize;
            foreach (var pair in dictionary.OrderBy(item => item.Key))
            {
                foreach (Window w2 in pair.Value)
                {
                    w2.Fall(2f);
                }
                buffer--;
                if (buffer <= 0)
                {
                    buffer = groupSize;
                    yield return groupDelay;
                }
            }

        }
        private Vector3 CalculateZ(Vector3 input)
        {
            if (Push == 0) return input;
            float len = input.XY().Length();
            float abs = Math.Abs(Push);
            if (len < abs)
            {
                float lerp = Ease.CubeIn(1 - len / abs);
                input.Z = lerp * Push;
            }
            return input;
        }
    }
}