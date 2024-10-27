using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static Celeste.Mod.PuzzleIslandHelper.Entities.BatterySystem;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [CustomEntity("PuzzleIslandHelper/Statid")]
    [Tracked]
    public class Statid : Entity
    {
        public bool Digital => PianoModule.Session.DEBUGBOOL1;

        public const float BaseAngle = -90;
        public const float MaxAngleDiff = 35;

        public class Petal : Component
        {
            public Vector2[] PetalPoints = new Vector2[] { new(-0.15f, 0), new(0.15f, 0), new(-0.25f, -0.5f), new(0.25f, -0.5f), new(0, -1) };
            public int[] PetalIndices = new int[] { 1, 0, 2, 2, 3, 1, 4, 3, 2 };
            public float[] RotMult = new float[] { 0.2f, 0.2f, 0.5f, 0.5f, 1 };
            public VertexPositionColor[] Vertices;
            public Vector2 Scale;
            private float startRotation;
            public float Rotation;
            public Vector2 RenderPosition
            {
                get
                {
                    return Parent.StemConnect + Position;
                }
                set
                {
                    Position = value - Parent.StemConnect;
                }
            }
            public Vector2 Position;
            public float SwayAmount;
            public Statid Parent;
            public bool Digital => Parent.Digital;
            public Petal(Statid parent, float rotation, Vector2 scale) : base(true, true)
            {
                Parent = parent;
                startRotation = rotation;
                Scale = scale;
                Vertices = new VertexPositionColor[PetalPoints.Length];

                for (int i = 0; i < PetalPoints.Length; i++)
                {
                    Vertices[i] = new VertexPositionColor(new Vector3(PetalPoints[i] * Scale, 0), Color.White);
                }
            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                //scaleOffset = Vector2.One * Calc.Random.Range(0, 3) * Calc.Random.Choose(-1, 1);
            }
            public override void Update()
            {
                base.Update();
                UpdateVertices();
            }

            public void UpdateVertices()
            {
                Vector2 p = RenderPosition;
                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vertices[i].Position = new Vector3(p + PianoUtils.RotatePoint(PetalPoints[i] * Scale, Vector2.Zero, startRotation + Rotation), 0);
                }
            }
            public void DrawPetal(Matrix matrix)
            {
                GFX.DrawIndexedVertices(matrix, Vertices, 5, PetalIndices, 3);
            }
        }
        public Petal[] Petals;

        public int PetalCount;
        public int Direction;
        private int scaleRange;
        private int bulbSize;
        public float Angle;
        public float AngleOffset;
        public float MaxAngleOffset;
        public float Mult;
        public float TargetMult;

        public bool onScreen;

        public Vector2 Orig;
        public Vector2 Ground;
        public Vector2 StemConnect;
        public Vector2 PetalScale;
        private Rectangle onScreenRect;
        public EntityID ID;
        public int Thickness = 1;
        private VirtualRenderTarget PetalTarget;
        private Ease.Easer ease;
        public IEnumerable<Entity> CollidingActors;


        public Statid(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, data.Int("petals"), data.Bool("digital"), Vector2.One * 4, 0)
        {
            ID = id;
        }
        public Statid(Vector2 position, int petals, bool digital, Vector2 petalScale, int scaleRange) : base(position)
        {
            ease = Ease.Follow(Ease.SineIn, Ease.BackOut);
            PetalScale = petalScale;
            this.scaleRange = scaleRange;
            Orig = Position;
            PetalCount = petals;
            Add(new BeforeRenderHook(BeforeRender));
        }
        public override void Update()
        {
            if (Scene is not Level level) return;

            onScreen = level.Camera.GetBounds().Colliding(onScreenRect);
            if (!onScreen) return;
            float ease = this.ease(Mult);
            if (Digital)
            {
                AngleOffset = Mult > 0 ? MaxAngleDiff : 0;
            }
            else
            {
                AngleOffset = Calc.Approach(AngleOffset, MaxAngleDiff * ease, (5 + 15 * ease));
            }

            Mult = Calc.Approach(Mult, TargetMult, Engine.DeltaTime);
            Angle = (BaseAngle + AngleOffset).ToRad();
            TargetMult = Calc.Approach(TargetMult, 0, Engine.DeltaTime * 1.2f);

            if (!Digital)
            {
                foreach (Petal p in Petals)
                {
                    p.Rotation = Angle;
                }
            }
            StemConnect = Ground + Calc.AngleToVector(Angle, Height);

            base.Update();
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            bulbSize = Calc.Random.Range(1, 4);
            Thickness = Calc.Random.Choose(1, 2);
            Ground = this.GroundedPosition() + Vector2.UnitY;
            Collider = new Hitbox(8, Ground.Y - Orig.Y, -4);
            onScreenRect = Collider.Bounds;
            int offset = 8;
            onScreenRect.X -= offset;
            onScreenRect.Width += offset * 2;
            onScreenRect.Y -= offset;
            onScreenRect.Height += offset;

            Petals = new Petal[PetalCount];

            for (int i = 0; i < PetalCount; i++)
            {
                Vector2 scale = new Vector2(4 + Calc.Random.Range(-scaleRange, scaleRange), 4 + Calc.Random.Range(-scaleRange, scaleRange));
                Petals[i] = new Petal(this, 360f / PetalCount * i, scale);
                Petals[i].Visible = false;
            }
            Add(Petals);
        }
        private bool drewOnce;

        public void BeforeRender()
        {
            if (drewOnce) return;
            float left = int.MaxValue;
            float right = int.MinValue;
            float top = int.MaxValue;
            float bottom = int.MinValue;
            foreach (Petal p in Petals)
            {
                foreach (VertexPositionColor vertice in p.Vertices)
                {
                    left = Calc.Min(vertice.Position.X, left);
                    right = Calc.Max(vertice.Position.X, right);
                    top = Calc.Min(vertice.Position.Y, top);
                    bottom = Calc.Max(vertice.Position.Y, bottom);
                }
            }
            int width = (int)(right - left);
            int height = (int)(bottom - top);
            PetalTarget = VirtualContent.CreateRenderTarget("StatidPetalTarget", width, height);
            PetalTarget.SetRenderTarget(null);
            foreach (Petal p in Petals)
            {
                p.DrawPetal(Matrix.Identity);
            }

            drewOnce = true;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            PetalTarget?.Dispose();
            PetalTarget = null;
        }
        public override void Render()
        {
            if (Scene is not Level level || !onScreen) return;
            if (Digital)
            {
                Draw.LineAngle(Ground, Angle, Height - 1, Color.White, Thickness);
                DrawPoint(Ground + Calc.AngleToVector(Angle, Height + 2), Color.White, bulbSize);
            }
            else
            {
                Draw.LineAngle(Ground, Angle, Height - 1, Color.White, Thickness);
                Draw.SpriteBatch.Draw(PetalTarget, Position, Color.White);
            }
        }
        public void DrawPoint(Vector2 position, Color color, int size)
        {
            Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, position, Draw.Pixel.ClipRect, color, 0f, Vector2.Zero, size, SpriteEffects.None, 0f);
        }
        public void DrawCurve(Vector2 from, Vector2 to, Vector2 control)
        {
            from = from.Round();
            to = to.Round();
            SimpleCurve curve = new SimpleCurve(from, to, (from + to) / 2f + control);
            Vector2 vector = curve.Begin;
            int steps = (int)Vector2.Distance(from, to);
            for (int j = 1; j <= steps; j++)
            {
                float percent = (float)j / steps;
                float colorAmount = Calc.Clamp(MathHelper.Distance(0.5f, percent), 0.15f, 0.85f) / 0.35f;
                Vector2 point = curve.GetPoint(percent).Round();
                Draw.Line(vector, point, Color.Lerp(Color.White, Color.Gray, 1 - colorAmount), 1);
                vector = point + (vector - point).SafeNormalize();
            }
        }
    }
}
