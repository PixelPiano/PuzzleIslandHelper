using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using ExtendedVariants.Entities.ForMappers;
using FrostHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.PuzzleIslandHelper.Components.Segment;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    public class Flicker : Component
    {
        public float Amount;
        public float Interval;
        public float Max;
        public float Rate;
        private float target;
        private float lerp;
        private float from;
        public Flicker(float interval, float max, float rate) : base(true, true)
        {
            Interval = interval;
            Max = max;
            Rate = rate;
        }
        public override void Update()
        {
            base.Update();
            if (Scene.OnInterval(Interval))
            {
                target = Calc.Random.Range(0, Max);
                from = Amount;
                lerp = 0;
            }
            Amount = Calc.LerpClamp(from, target, lerp);
            lerp = Calc.Min(lerp + Engine.DeltaTime * Rate, 1);
        }
    }
    [CustomEntity("PuzzleIslandHelper/Ascwiit")]
    [Tracked]
    public class Ascwiit : Actor
    {
        public Vector2[] WingPoints = new Vector2[] { new(0, 0), new(0, 2), new(2, 0) };
        public Vector2[] BodyPoints = new Vector2[] { new(0, 0), new(1, 0), new(2, 0), new(3, 0), new(1, 1), new(2, 1), new(3, 1), new(4, 1) };
        public int[] WingIndices = new int[] { 0, 1, 2 };
        public int[] BodyIndices = new int[] { 0, 1, 4, 4, 1, 5, 1, 2, 5, 2, 3, 6, 5, 2, 6, 6, 3, 7 };

        public VertexPositionColor[] WingVertices;
        public VertexPositionColor[] BodyVertices;
        public List<float> BodyAlphas = new();
        public List<Color> BodyColors = new();

        public Flicker[] Flickers = new Flicker[6];
        public readonly Vector2 BodyOffset = Vector2.Zero;
        public const int BirdHeight = 4;
        public const int BirdWidth = 8;
        public bool WingUp;
        public Facings Facing = Facings.Right;
        public Vector2 WingOffset;
        public Vector2 Speed;

        public const float HopSpeedX = 120f;
        public const float HopSpeedY = -60f;

        private float peckTimer;
        private float flapTimer;
        private float chirpTimer;
        private bool idleFlapping;
        public float MinX;
        public float MaxX;
        public float MinY;
        public float FlySpeedX;
        public float FlySpeedY;
        public static bool Optimize = true;


        public bool OnScreen;

        private float breatheAmount;
        private enum WingStates
        {
            Up = -1,
            AtRest = 0,
            Down = 1
        }
        private enum States
        {
            Idle,
            Flying,
        }
        private States State;
        private WingStates WingState;
        private Collider DetectBox;
        public bool Flying => State is States.Flying;
        public bool Idle => State is States.Idle;

        private const float DetectXRange = 20f;
        private const float DetectYRange = 16f;

        public SoundSource tweetingSfx;
        private float hopTimer;
        private float frictionMult = 1;
        public const float FlyingFrictionMult = 0.1f;
        private float flyLerp = 0;
        private float flyXAmount;

        private bool ignoreSolids;
        private bool onGround;
        private float ColorLerp;
        private float firstPeckTimer;
        public Ascwiit(EntityData data, Vector2 offset) : this(data.Position + offset)
        {

        }
        public Ascwiit(Vector2 position) : base(position)
        {
            Depth = 1;
            WingVertices = PianoUtils.Initialize((VertexPositionColor)default, WingPoints.Length);
            BodyVertices = PianoUtils.Initialize((VertexPositionColor)default, BodyPoints.Length);
            Collider = new Hitbox(BirdWidth, BirdHeight);
            DetectBox = new Hitbox(DetectXRange * 2, DetectYRange * 2);
            WingOffset = new Vector2(BirdWidth / 3f, Height / 2f);
            for (int i = 0; i < 6; i++)
            {
                Flickers[i] = new Flicker(0.5f, 0.6f, 3);
            }
            IgnoreJumpThrus = true;
            Add(Flickers);
            AddTag(Tags.TransitionUpdate);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            for (int i = 0; i < BodyPoints.Length + 3; i += 3)
            {
                BodyAlphas.Add(Calc.Random.Range(0.4f, 0.8f));
                BodyColors.Add(Calc.Random.Choose(Color.LightGreen, Color.ForestGreen));
            }
            firstPeckTimer = Calc.Random.Range(0, 10f);
            flyXAmount = Calc.Random.Range(0.3f, 1f);
            FlySpeedX = Calc.Random.Range(8f,30f);
            FlySpeedY = Calc.Random.Range(-6f, -2f);
            while (!OnGround())
            {
                Position.Y++;
                if (Position.Y > (scene as Level).Bounds.Bottom)
                {
                    RemoveSelf();
                }
            }
            if (Optimize)
            {
                Vector2 orig = Position;
                bool inAir = false;
                while (!CollideCheck<Solid>() && Left > (scene as Level).Bounds.Left)
                {
                    if (!OnGround())
                    {
                        inAir = true;
                        break;
                    }
                    Position.X--;
                }
                MinX = Left + (inAir ? Width : 0);
                Position = orig;
                inAir = false;
                while (!CollideCheck<Solid>() && Right < (scene as Level).Bounds.Right)
                {
                    if (!OnGround())
                    {
                        inAir = true;
                        break;
                    }
                    Position.X++;
                }
                MaxX = Right - (inAir ? Width : 0);
                Position = orig;
                MinY = Top;
            }

        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (Optimize)
            {
                Draw.Line(MinX, MinY, MaxX, MinY, Color.Magenta);
            }
            Draw.HollowRect(DetectBox, Color.Blue);
        }
        public override void Update()
        {
            base.Update();

            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            DetectBox.BottomCenter = BottomCenter;
            OnScreen = level.Camera.GetBounds().Intersects(Collider.Bounds);
            float prevY = Position.Y;
            if (OnScreen)
            {
                float num = (Math.Abs(Speed.Y) < 40f ? 0.5f : 1f) * frictionMult;
                onGround = Optimize ? Position.Y == MinY : OnGround();
                if (!onGround)
                {
                    Speed.Y = Calc.Approach(Speed.Y, 160f, 900f * num * Engine.DeltaTime);
                }
                if (Math.Abs(Speed.X) > 90f)
                {
                    Speed.X = Calc.Approach(Speed.X, 90f * (float)Math.Sign(Speed.X), 2500f * Engine.DeltaTime);
                }
                Speed.X = Calc.Approach(Speed.X, 0f, (1000f * Engine.DeltaTime) * frictionMult);
                switch (State)
                {
                    case States.Idle:
                        IdleUpdate(player);
                        break;
                    case States.Flying:
                        FlyingUpdate();
                        break;
                }
                if (Flying && !onGround)
                {
                    ignoreSolids = true;
                }
                if (ignoreSolids)
                {
                    Position.X += Speed.X * Engine.DeltaTime;
                    Position.Y += Speed.Y * Engine.DeltaTime;
                }
                else
                {
                    MoveH(Speed.X * Engine.DeltaTime);
                    MoveV(Speed.Y * Engine.DeltaTime);
                }
                if (Optimize)
                {
                    Position.Y = Calc.Min(Position.Y, MinY);
                }

            }
            UpdateVertices();
        }
        public void Chirp()
        {
            if (chirpTimer > 0) return;
            chirpTimer = 0.4f;
            //play chirp sound
        }
        public void Peck()
        {
            if (peckTimer > 0) return;
            peckTimer = 0.1f;
        }
        public void FleeFromPlayer(Player player)
        {
            Flee((Facings)(-Math.Sign(player.X - CenterX)));
        }
        public void Flee(Facings facing)
        {
            Facing = facing;
            Fly();
            foreach (Ascwiit bird in Scene.Tracker.GetEntities<Ascwiit>())
            {
                if (bird == this || bird.Flying || Vector2.Distance(Center, bird.Center) > DetectXRange) continue;
                bird.WaitThenFlee(facing);
            }

        }
        public void WaitThenFlee(Facings facing)
        {
            Alarm a = Alarm.Create(Alarm.AlarmMode.Oneshot,
                () => { Flee(facing); }, Calc.Random.Range(0f, 0.2f), true);
            Add(a);
        }

        public void IdleUpdate(Player player)
        {
            frictionMult = 1;
            firstPeckTimer = Calc.Max(firstPeckTimer - Engine.DeltaTime, 0);
            if (player != null && Math.Abs(player.X - X) < DetectXRange && player.Y > Y - DetectYRange && player.Y < Y + 8f)
            {
                FleeFromPlayer(player);
                FlyingUpdate();
                return;
            }
            if (onGround)
            {
                if (Calc.Random.Chance(0.0005f))
                {
                    IdleFlap();
                }
                chirpTimer = Calc.Max(chirpTimer - Engine.DeltaTime, 0);
                if (Calc.Random.Chance(0.001f))
                {
                    Chirp();
                }
                peckTimer = Calc.Max(peckTimer - Engine.DeltaTime, 0);
                if (Calc.Random.Chance(0.01f))
                {
                    Peck();
                }
                if (peckTimer > 0 || firstPeckTimer > 0) return;
                hopTimer -= Engine.DeltaTime;
                if (hopTimer < 0)
                {
                    hopTimer = 0;
                    int dir = Calc.Random.Choose(-1, 1);
                    if (Optimize)
                    {
                        float side = dir > 0 ? Right : Left;
                        float other = dir > 0 ? Left : Right;
                        float target = side + (dir * HopSpeedX * Engine.DeltaTime);
                        if (target < MinX || target > MaxX)
                        {
                            return;
                        }
                    }
                    else if (CollideCheck<Solid>(Position + Vector2.UnitX * (dir * (HopSpeedX * Engine.DeltaTime + 1))))
                    {
                        return;
                    }
                    Speed.Y = HopSpeedY;
                    Facing = (Facings)dir;
                    hopTimer = Calc.Random.Range(0.7f, 4);
                }
            }
            else
            {
                Speed.X = Calc.Approach(Speed.X, (int)Facing * HopSpeedX, 1000f * Engine.DeltaTime);
                peckTimer = 0;
            }

        }
        public void FlyingUpdate()
        {
            if (flyLerp > 0.5f)
            {
                ColorLerp += Engine.DeltaTime;
            }
            frictionMult = FlyingFrictionMult;
            flapTimer -= Engine.DeltaTime;
            if (flapTimer < 0)
            {
                flapTimer = 0.21f - (flyLerp * 0.15f);
                flyLerp = Calc.Min(flyLerp + Engine.DeltaTime * 2, 1);
                WingState = (WingStates)(-(int)WingState);
            }
            flyLerp = Calc.Min(flyLerp + Engine.DeltaTime, 1);
            Speed.Y += FlySpeedY * flyLerp;
            Speed.X += FlySpeedX * (int)Facing * flyLerp * flyXAmount;

            if (!SceneAs<Level>().Bounds.Contains(Collider.Bounds))
            {
                RemoveSelf();
            }
        }
        public void IdleFlap()
        {
            Add(new Coroutine(IdleFlapRoutine()));
        }
        private IEnumerator IdleFlapRoutine()
        {
            if (idleFlapping) yield break;
            idleFlapping = true;
            WingState = WingStates.Up;
            for (int i = 0; i < Calc.Random.Choose(4, 6); i++)
            {
                WingState = (WingStates)(-(int)WingState);
                yield return 0.05f;
            }
            if (!Flying)
            {
                WingState = WingStates.AtRest;
            }
            idleFlapping = false;
        }

        public void Fly()
        {
            SceneAs<Level>().ParticlesFG.Emit(Calc.Random.Choose(ParticleTypes.Dust), Center, -(float)Math.PI / 2f);
            idleFlapping = false;
            State = States.Flying;
            flyLerp = 0;
            Speed.Y -= 50f;
            Speed.X += (int)Facing * 30f;
            frictionMult = FlyingFrictionMult;
            WingState = WingStates.Up;
            if (Calc.Random.Chance(0.5f))
            {
                Depth = -10001;
            }
        }
        public void Wander()
        {
            State = States.Idle;
            frictionMult = 1;
        }
        public void UpdateVertices()
        {
            Vector2 wingOffset = WingOffset;
            Vector2 scale = new Vector2(Width / 4f, Height).Floor();
            scale *= Calc.LerpClamp(1, Depth > 0 ? 0.3f : 2, ColorLerp);
            


            Vector2 beakPoint = BodyPoints[0];
            if (peckTimer > 0)
            {
                BodyPoints[0].Y += 1;
            }
            Color to = Depth > 0 ? Color.DarkGreen : Color.LightGreen;
            for (int i = 0; i < BodyPoints.Length; i += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (BodyPoints.Length <= i + j) break;
                    Color color = Color.Lerp(BodyColors[i / 3], Color.Green, BodyAlphas[i / 3]);
                    BodyVertices[i + j].Color = Color.Lerp(color, to, ColorLerp);
                    if (Flickers.Length > i / 3)
                    {
                        BodyVertices[i + j].Color = Color.Lerp(BodyVertices[i + j].Color, Color.Black, Flickers[i / 3].Amount / 1.2f);
                    }
                    Vector2 point = BodyPoints[i + j] * scale;
                    if (Facing is Facings.Right)
                    {
                        point.X = (point.X * -1) + Width;
                    }
                    BodyVertices[i + j].Position = new Vector3(Position + point, 0);
                }
            }
            BodyPoints[0] = beakPoint;
            scale.Y *= WingState == 0 ? 0.3f : (int)WingState;

            for (int i = 0; i < WingPoints.Length; i++)
            {
                WingVertices[i].Color = Color.Lerp(Color.LightGreen, to, ColorLerp);
                Vector2 point = wingOffset + (WingPoints[i] * scale);
                if (Facing is Facings.Right)
                {
                    point.X = (point.X * -1) + Width;
                }
                WingVertices[i].Position = new Vector3(Position + point, 0);
            }
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level || !OnScreen) return;
            Draw.SpriteBatch.End();
            GFX.DrawIndexedVertices(level.Camera.Matrix, BodyVertices, BodyVertices.Length, BodyIndices, 6);
            GFX.DrawIndexedVertices(level.Camera.Matrix, WingVertices, WingVertices.Length, WingIndices, 1);
            GameplayRenderer.Begin();
        }
    }
}
