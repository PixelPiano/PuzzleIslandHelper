using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [CustomEntity("PuzzleIslandHelper/Statid")]
    [Tracked]
    public class Statid : Entity
    {

        public const int MaxSway = 75;
        public class Petal : Component
        {
            public Vector2[] PetalPoints = new Vector2[] { new(0, 0), new(-2, -4), new(2, -4), new(0, -8) };
            public int[] PetalIndices = new int[] { 0, 1, 2, 1, 3, 2 };
            public float[] RotMult = new float[] { 0, 0.5f, 0.5f, 1 };
            public VertexPositionColor[] Vertices;
            public Vector2 Scale;
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
            public Petal(Statid parent, float rotation) : base(true, true)
            {
                Parent = parent;
                Rotation = rotation;
                Vertices = new VertexPositionColor[PetalPoints.Length];
                for (int i = 0; i < PetalPoints.Length; i++)
                {
                    Vertices[i] = new VertexPositionColor(new Vector3(PetalPoints[i], 0), Color.White);
                }
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
                    Vertices[i].Position = new Vector3(p + PianoUtils.RotatePoint(PetalPoints[i], Vector2.Zero, Rotation), 0);
                }
            }
            public override void Render()
            {
                base.Render();
                if (Scene is not Level level) return;
                GFX.DrawIndexedVertices(level.Camera.Matrix, Vertices, 4, PetalIndices, 2);
            }
        }
        public Petal[] Petals;
        public int PetalCount;
        public float SwayTimer;
        private bool wasColliding;
        private bool colliding;
        public Vector2 Orig;
        public Vector2 Ground;
        public Vector2 StemConnect;
        public float Angle;
        public float BaseAngle = -90;
        public float MaxAngleDiff = 35;
        public int Direction;
        public float MaxMove;
        public float Speed;

        public float MaxAngleOffset;
        public float AngleOffset;
        public float Mult = 1;
        public float VelocityMult = 1;
        private float velocity;
        private float smoothMult;
        public Statid(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Orig = Position;
            PetalCount = data.Int("petals");
        }
        public void Sway(float speed)
        {
            float abs = Math.Abs(speed);
            lastSpeed = abs;

            if (abs < 90) Mult = 0.4f;
            else if (abs <= 240) Mult = 0.7f;
            else Mult = 1.3f;

            Direction = Math.Sign(speed);
            MaxAngleOffset = Direction * MaxAngleDiff;
            smoothMult = 1;
            AngleOffset += Mult * Direction;
        }
        private float lastSpeed;
        public override void Update()
        {
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            colliding = CollideCheck(player);
            if (colliding && (!wasColliding || lastSpeed < Math.Abs(player.Speed.X)))
            {
                Sway(player.Speed.X);
            }
            float prev = AngleOffset;
            AngleOffset = Calc.Approach(AngleOffset, MaxAngleOffset * Ease.SineIn(Mult * smoothMult), (20 + 15 * Mult) * Engine.DeltaTime);
            Mult = Calc.Approach(Mult, 0, Engine.DeltaTime);
            velocity = AngleOffset - prev;
            if (Math.Sign(velocity) != Direction)
            {
                smoothMult = Calc.Approach(smoothMult, 0, Engine.DeltaTime / 2);
            }
            StemConnect = Ground + Calc.AngleToVector(((BaseAngle + AngleOffset) % 360).ToRad(), Height);
            wasColliding = colliding;
            base.Update();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Circle(StemConnect, 4, Color.Lime, 1);
            Draw.Circle(Ground, 4, Color.Magenta, 1);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Collider = new Hitbox(1, 1);
            Level level = scene as Level;
            while (Position.Y < level.Bounds.Bottom && !CollideCheck<Solid>())
            {
                Position.Y++;
            }
            Ground = Position + Vector2.UnitY;
            Position = Orig;
            Collider = new Hitbox(16, Ground.Y - Orig.Y, -8);
            if (PetalCount <= 0) RemoveSelf();

            Petals = new Petal[PetalCount];
            for (int i = 0; i < PetalCount; i++)
            {
                Petals[i] = new Petal(this, 360f / PetalCount * i);
                Petals[i].Visible = false;
            }
            Add(Petals);
        }
        public override void Render()
        {
            base.Render();
            DrawCurve(StemConnect, Ground, Vector2.UnitX * (Angle - BaseAngle) / MaxAngleDiff);

            Draw.SpriteBatch.End();
            foreach (Petal p in Petals)
            {
                p.Render();
            }
            GameplayRenderer.Begin();
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
                Draw.Line(vector, point, Color.Lerp(Color.White, Color.Gray, 1 - colorAmount), 2);
                vector = point + (vector - point).SafeNormalize();
            }
        }
    }
}
