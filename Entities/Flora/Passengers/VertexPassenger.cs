using Celeste.Mod.PuzzleIslandHelper.Components;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{

    [Tracked]
    public abstract class VertexPassenger : Passenger
    {
        public class Vertex
        {
            public Color Color;
            public Color DefaultColor;
            public Group Group;
            public Vector2 WorldPosition;
            public Vector2 Position;
            public Vector2 WiggleOffset;
            public float RotationRate;
            public float AngleOffset;
            public Vector2 RotationOffset;
            public Vector2 OgOffset;
            public Tween Tween;
            public Vector2 WiggleDirection;
            public float WiggleMult = 1;
            public Vector2 Offset;
            public ColorShifter Shifter = null;
            private bool baked;

            public void Bake(Entity entity, float minWiggleTime, float maxWiggleTime)
            {
                baked = true;
                Tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.QuadInOut, Calc.Random.Range(minWiggleTime, maxWiggleTime), true);
                Tween.Randomize();
                entity.Add(Tween);
                if (Shifter != null && !Shifter.HasBeenAdded)
                {
                    entity.Add(Shifter);

                    Shifter.HasBeenAdded = true;
                }
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
            public bool ScaleApproachZero;
            public Vector2 ScaleApproachMult = Vector2.One;
            public Color Color;
            public float ColorLerp;
            public List<Vertex> Vertices = [];
            public Func<Vertex, Color> ModifyColor = null;
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
                    if (ScaleApproachZero)
                    {
                        Scale.X = Calc.Approach(Scale.X, 0, ScaleApproachMult.X * Engine.DeltaTime);
                        Scale.Y = Calc.Approach(Scale.Y, 0, ScaleApproachMult.Y * Engine.DeltaTime);
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
        public class VertexGroupData
        {
            public int Count => Vertices.Count;
            public List<Vertex> Vertices = [];
            public Vertex this[int i]
            {
                get => Vertices[i];
                set => Vertices[i] = value;
            }
            public override string ToString()
            {
                string output = "";
                foreach (Vertex v in Vertices)
                {
                    output += "\n\t" + v.ToString();
                }
                return "{" + output + "\n}";
            }
            public void Bake(Entity entity, float minWiggleTime, float maxWiggleTime, out Vector2[] positions)
            {
                positions = [.. Vertices.Select(item => item.Position)];
                foreach (Vertex v in Vertices)
                {
                    v.Bake(entity, minWiggleTime, maxWiggleTime);
                }
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
                        Vertices.Add(v);
                    }
                }
            }
        }
        public struct PointData
        {
            public static implicit operator Vector2(PointData data) => data.Point;
            public static implicit operator PointData(Vector2 v) => new PointData(v);
            public Vector2 Point;
            public float WiggleMult = 1;
            public Vector2 WiggleDir = Vector2.One;
            public ColorShifter Shifter = null;
            public Group Group = null;
            public Color? DefaultColor = null;
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

        public Color DefaultColor = Color.Lime;
        public VertexGroupData VertexList = new();
        public int VertexCount => VertexList.Count;
        public string VertexData => VertexList.ToString();
        public string VerticeData
        {
            get
            {
                string output = "{";
                foreach (var v in Vertices)
                {
                    output += "\n\t" + v.ToString();
                }
                return output + "\n}";
            }
        }
        private float debugAngleOffset = 0;
        public List<int> Indices = [];
        public bool Breathes = true;
        public int[] indices;
        public VertexPositionColor[] Vertices;
        public List<int> LineIndices = [];
        public int[] lineIndices;
        public float MainWiggleMult = 1;
        public float MinWiggleTime = 0.8f;
        public float MaxWiggleTime = 2;
        public Vector2 Scale;
        public Vector2 ScaleOffset;
        private Vector2 BreathOffset;
        public Vector2 BreathDirection;
        public float BreathDuration;
        public Vector2 ScaleApproach = Vector2.One;
        public bool Baked;
        public bool IsInView;
        public bool CanJump => HasGravity && onGround && CannotJumpTimer <= 0;
        public float Alpha = 1;
        public Color Color2;
        public float ColorMixLerp;
        public bool Outline = true;
        public int Lines;
        public Facings Facing = Facings.Left;
        public static Effect Effect;
        public bool DummyBreath;
        public bool DummyGravity;
        public VertexPassenger(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
        {

        }
        public VertexPassenger(EntityData data, Vector2 offset, EntityID id, float width, float height) : base(data, offset, id, width, height)
        {
            Position.Y -= Height - 16;
        }
        public VertexPassenger(EntityData data, Vector2 offset, EntityID id, float width, float height, Vector2 scale) :
            this(data, offset, id, width, height)
        {
            Scale = scale;
        }
        public VertexPassenger(EntityData data, Vector2 offset, EntityID id, float width, float height, Vector2 scale, Vector2 breathDirection, float breathDuration) :
            this(data, offset, id, width, height, scale)
        {
            BreathDirection = breathDirection;
            BreathDuration = breathDuration;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            foreach (string s in CutsceneArgs)
            {
                if (s.Contains(':'))
                {
                    string[] array = s.Split(':');
                    if (array != null && array.Length > 1)
                    {
                        string text = array[0];
                        switch (text.ToLower())
                        {
                            case "facing":
                                string direction = array[1].ToLower();
                                if (direction == "right")
                                {
                                    Facing = Facings.Right;
                                }
                                else if (direction == "left")
                                {
                                    Facing = Facings.Left;
                                }
                                break;
                            case "flee":
                                string direction2 = array[1].ToLower();
                                if (direction2 == ">")
                                {
                                    Add(new Coroutine(fleeRoutine(1)));
                                }
                                else if (direction2 == "<")
                                {
                                    Add(new Coroutine(fleeRoutine(-1)));
                                }
                                break;
                        }
                    }
                }
            }
        }
        public void Face(Entity entity)
        {
            if (entity.CenterX > CenterX)
            {
                Facing = Facings.Right;
            }
            else
            {
                Facing = Facings.Left;
            }
        }
        private IEnumerator fleeRoutine(int direction)
        {
            Player player = Scene.GetPlayer();
            while (!this.OnScreen())
            {
                yield return null;
            }

            GravityMult = 3;
            Jump();
            while (!onGround || Speed.Y < 0)
            {
                yield return null;
            }
            float speedMult = 1;
            Facing = (Facings)Math.Sign(direction);
            X += Width / 2;
            while (this.OnScreen())
            {
                float speed = Math.Max(40f, Math.Abs(player.Speed.X)) * direction * speedMult;
                NaiveMove(Vector2.UnitX * speed * Engine.DeltaTime);
                speedMult += Engine.DeltaTime * 3f;
                while (CollideCheck<Solid>())
                {
                    NaiveMove(Vector2.UnitY * -8);
                }
                yield return null;
            }

            SceneAs<Level>().Session.DoNotLoad.Add(EID);
            RemoveSelf();
        }
        public IEnumerator WalkX(float x, float speedMult = 1, bool walkBackwards = false)
        {
            yield return new SwapImmediately(WalkToX(X + x, speedMult, walkBackwards));
        }
        public IEnumerator WalkToX(float x, float speedMult = 1, bool walkBackwards = false)
        {
            x = (int)Math.Round(x);
            if (Position.X == x) yield break;
            int dir = Math.Sign(x - Position.X);
            if (walkBackwards) dir *= -1;
            Facing = (Facings)dir;
            while (Math.Abs(Position.X - x) > 2)
            {
                MoveTowardsX(x, 90f * speedMult * Engine.DeltaTime);
                yield return null;
            }
            Position.X = x;
        }
        private IEnumerator breathRoutine()
        {
            int skip = Calc.Random.Range(0, 30);
            Vector2 a = Vector2.Zero;
            Vector2 b = BreathDirection;
            void swap()
            {
                (b, a) = (a, b);
            }
            while (true)
            {
                while (!Breathes || BreathDuration <= 0) yield return null;
                for (float i = 0; i < 1 && Breathes; i += Engine.DeltaTime / BreathDuration / 2f)
                {
                    BreathOffset = Vector2.Lerp(a, b, Ease.QuadInOut(i));
                    if (skip > 0) skip--;
                    else yield return null;
                }
                BreathOffset = a;
                swap();
            }
        }
        [Obsolete("Use PointData functions")]
        protected void AddTriangle(Vector2 a, Vector2 b, Vector2 c, float multiplier, Vector2 wiggleMult, ColorShifter shifter = null, Group group = null, bool mergePoints = false)
        {
            PointData[] data = new PointData[3];
            Vector2[] points = [a, b, c];
            for (int i = 0; i < points.Length; i++)
            {
                data[i] = new PointData()
                {
                    Point = points[i],
                    WiggleMult = multiplier,
                    WiggleDir = wiggleMult,
                    Shifter = shifter,
                    Group = group
                };
            }
            AddTriangle(data[0], data[1], data[2], mergePoints);
        }
        [Obsolete("Use PointData functions")]
        public void AddQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float mult, Vector2 wiggleMult, ColorShifter shifter = null, Group groupA = null, Group groupB = null, bool mergePoints = true)
        {
            AddTriangle(a, b, c, mult, wiggleMult, shifter, groupA, mergePoints);
            AddTriangle(b, c, d, mult, wiggleMult, shifter, groupB, mergePoints);
        }
        [Obsolete("Use PointData functions")]
        protected void AddTriangle(float x1, float y1, float x2, float y2, float x3, float y3, float mult, Vector2 wiggleMult, ColorShifter shifter = null, Group group = null)
          => AddTriangle(new(x1, y1), new(x2, y2), new(x3, y3), mult, wiggleMult, shifter, group);
        public void AddQuad(PointData a, PointData b, PointData c, PointData d, bool mergePoints = true)
        {
            AddTriangle(a, b, c, mergePoints);
            AddTriangle(b, c, d, mergePoints);
        }
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
                VertexList.AddGroup([.. ungrouped]);
            }
            foreach (var pair in uniqueGroups)
            {
                VertexList.AddGroup(pair.Key, [.. pair.Value]);
            }
            //add line from last index to new index
            for (int i = 1; i < lines.Length; i++)
            {
                LineIndices.AddRange([lines[i - 1], lines[i]]);
                Lines++;
            }
            //include line from start index to end index
            if (lines.Length > 1)
            {
                LineIndices.AddRange([lines[0], lines[^1]]);
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
                    Shifter = center.Shifter,
                    DefaultColor = center.DefaultColor,
                    WiggleDir = center.WiggleDir,
                    WiggleMult = center.WiggleMult
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
                    WiggleMult = center.WiggleMult,
                    WiggleDir = center.WiggleDir,
                    Shifter = center.Shifter,
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
        private List<Vertex> vertexList = [];
        private bool TryCreateVertex(PointData data, out Vertex vertex, bool mergePoints = false)
        {
            vertex = null;
            if (mergePoints)
            {
                for (int i = 0; i < vertexList.Count; i++)
                {
                    Vertex check = vertexList[i];
                    if (check.Position == data.Point)
                    {
                        check.WiggleMult = Calc.Max(check.WiggleMult, data.WiggleMult);
                        Indices.Add(i);
                        return false;
                    }
                }
            }
            vertex = new();
            vertex.Position = data.Point;
            vertex.Shifter = data.Shifter;
            if (data.Shifter == null)
            {
                foreach (Vertex v in vertexList)
                {
                    if (v.Shifter != null)
                    {
                        vertex.Shifter = v.Shifter;
                    }
                }
            }
            vertex.WiggleMult = data.WiggleMult;
            vertex.WiggleDirection = data.WiggleDir;
            vertex.DefaultColor = data.DefaultColor ?? DefaultColor;
            Indices.Add(vertexList.Count);
            vertexList.Add(vertex);
            return true;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Add(new Coroutine(breathRoutine()));
        }
        public override void Jump()
        {
            base.Jump();
            ScaleApproach.X = 0.8f;
            ScaleApproach.Y = 0.9f;
        }
        public override void Update()
        {
            foreach (Vertex v in VertexList.Vertices)
            {
                v.Group?.Update();
            }
            IsInView = InView();
            if (ScaleApproach.X != 1)
            {
                ScaleApproach.X = Calc.Approach(ScaleApproach.X, 1, Engine.DeltaTime);
            }
            if (ScaleApproach.Y != 1)
            {
                ScaleApproach.Y = Calc.Approach(ScaleApproach.Y, 1, Engine.DeltaTime);
            }
            base.Update();

            if (ActiveFlag && IsInView)
            {
                UpdateVertices();
            }
            foreach (Vertex v in VertexList.Vertices)
            {
                v.Group?.PostUpdate();
            }
        }
        public virtual void EditVertice(int index)
        {

        }
        public void UpdateVertices()
        {
            if (Baked)
            {
                for (int i = 0; i < VertexList.Count; i++)
                {
                    Vertex vertex = VertexList[i];
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
                    point.X *= -(int)Facing;
                    vertex.WiggleOffset = vertex.OgOffset * vertex.Tween.Eased * (vertex.WiggleDirection * MainWiggleMult) * vertex.WiggleMult;
                    vertex.WorldPosition = Position + (point * Scale * ScaleApproach * group.Scale);
                    if (Facing > 0)
                    {
                        vertex.WorldPosition.X += Width;
                    }
                    Vertices[i].Position = new Vector3(vertex.WorldPosition + vertex.WiggleOffset + vertex.Offset + BreathOffset, 0);
                    if (vertex.Shifter != null)
                    {
                        vertex.Color = Color.Lerp(Color.Lerp(vertex.Shifter[i % vertex.Shifter.Colors.Length], Color2, ColorMixLerp), group.Color, group.ColorLerp) * group.Alpha * Alpha;
                    }
                    else
                    {
                        vertex.Color = vertex.DefaultColor;
                    }
                    Vertices[i].Color = group.ModifyColor != null ? group.ModifyColor.Invoke(vertex) : vertex.Color;
                    EditVertice(i);
                }
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (Vertices.Length > 1)
            {
                for (int i = 1; i < indices.Length; i++)
                {
                    int indexA = indices[i - 1];
                    int indexB = indices[i];
                    Draw.Line(Vertices[indexA].Position.XY(), Vertices[indexB].Position.XY(), Color.Magenta);
                }
                /*                for (int i = 1; i < Vertices.Length; i++)
                                {
                                    Draw.Line(Vertices[i - 1].Position.XY(), Vertices[i].Position.XY(), Color.Magenta);
                                }
                                Draw.Line(Vertices[0].Position.XY(), Vertices[^1].Position.XY(), Color.Magenta);*/
                foreach (Vertex v in VertexList.Vertices)
                {
                    Draw.Line(Position + v.Group.Center * Scale * ScaleApproach * -(int)Facing, v.WorldPosition, Color.White);
                }
            }
        }
        public void Bake(bool recreateHitbox = false)
        {
            VertexList.Bake(this, MinWiggleTime, MaxWiggleTime, out Vector2[] vertices);
            Vertices = vertices.CreateVertices(Scale, out indices, Color.Lime);
            indices = [.. Indices];
            lineIndices = [.. LineIndices];
            Baked = true;
            if (recreateHitbox)
            {
                float left = int.MaxValue, top = int.MaxValue, right = int.MinValue, bottom = int.MinValue;
                for (int i = 0; i < VertexList.Vertices.Count; i++)
                {
                    Vector2 p2 = VertexList.Vertices[i].Position;
                    left = Math.Min(left, p2.X);
                    right = Math.Max(right, p2.X);
                    top = Math.Min(top, p2.Y);
                    bottom = Math.Max(bottom, p2.Y);
                }
                Collider = new Hitbox(right - left, bottom - top, left, top);
            }
        }
        public bool InView()
        {
            Camera camera = (Scene as Level).Camera;
            float xPad = Width;
            float yPad = Height;
            if (X > camera.X - xPad && Y > camera.Y - yPad && X < camera.X + 320f + xPad)
            {
                return Y < camera.Y + 180f + yPad;
            }

            return false;
        }
        public override void Render()
        {
            if (!ActiveFlag) return;
            base.Render();
            if (!Baked || Scene is not Level level || !IsInView) return;
            Draw.SpriteBatch.End();
            Effect = ShaderHelperIntegration.TryGetEffect("PuzzleIslandHelper/Shaders/vertexPassengerShader");
            if (Outline)
            {
                DrawOutline(level, Effect);
            }
            DrawVertices(PrimitiveType.TriangleList, level, Effect);
            GameplayRenderer.Begin();
        }
        public virtual Effect ApplyParameters(Effect effect, Level level)
        {
            return effect;
        }
        public string indiceData
        {
            get
            {
                string array = "{";
                for (int i = 0; i < indices.Length; i++)
                {
                    array += indices[i].ToString() + ", ";
                }
                return array + "}";
            }
        }
        public virtual void DrawVertices(PrimitiveType type, Level level, Effect effect = null, BlendState blendState = null)
        {
            DrawIndexedVertices(type, level.Camera.Matrix, Vertices, Vertices.Length, indices, indices.Length / 3, null, effect, blendState);
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
        public virtual void DrawOutline(Level level, Effect effect = null, BlendState blendState = null)
        {
            DrawIndexedVertices(PrimitiveType.LineList, level.Camera.Matrix, Vertices, Vertices.Length, lineIndices, Lines, Color.Black, effect);
        }
        public void DrawLines<T>(Matrix matrix, T[] vertices, int vertexCount, int[] indices, int primitiveCount, Effect effect = null, BlendState blendState = null, Color? solidColor = null) where T : struct, IVertexType
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
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Vertices = null;
        }
        public IEnumerator PlayerStepBack(Player player, Vector2 screenSpaceFocusPoint, float zoom, float duration)
        {
            Coroutine zoomRoutine = new Coroutine(SceneAs<Level>().ZoomTo(screenSpaceFocusPoint, zoom, duration));
            Add(zoomRoutine);
            yield return new SwapImmediately(PlayerStepBack(player));
            while (!zoomRoutine.Finished)
            {
                yield return null;
            }
        }
        public void FacePlayer(Player player)
        {
            if (player.CenterX > CenterX)
            {
                Facing = Facings.Right;
            }
            else
            {
                Facing = Facings.Left;
            }
        }
        public IEnumerator PlayerStepBack(Player player, Facings facing)
        {
            float xTarget = CenterX + (int)facing * (16 + Width / 2);
            yield return new SwapImmediately(player.DummyWalkTo(xTarget));
            player.Facing = (Facings)(-(int)facing);
        }
        public IEnumerator PlayerStepBack(Player player)
        {
            yield return new SwapImmediately(PlayerStepBack(player, Facing));
        }
        public static string GetPositionString(VertexPassenger p)
        {
            string output = "";

            foreach (VertexPositionColor v in p.Vertices.OrderByDescending(item => item.Position.Y))
            {
                output += "{" + v.Position.X + "," + v.Position.Y + "} " + '\n';
            }
            return output;
        }
    }
}
