using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.PuzzleIslandHelper.Components.Segment;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [CustomEntity("PuzzleIslandHelper/HelixWorm")]
    [Tracked]
    public class HelixWorm : Entity
    {
        private Vector2 aHeadPosition => Head is not null ? Head.Position : Vector2.Zero;
        private Vector2 aHeadSpeed => Head is not null ? Head.Speed : Vector2.Zero;
        private Vector2 aHeadSpeedMultiplied => Head is not null ? aHeadSpeed * Head.SpeedMult * Engine.DeltaTime : Vector2.Zero;
        public const float BaseSpeed = 500f;
        public float MaxHeight = 32;
        public float waveTimer;
        public Facings Facing = Facings.Left;
        public float FacingTarget;
        public Vector2 Scale;
        public Player Player;
        public float Stress = 1;
        public Segment Base;
        public Segment Neck;
        public Segment Head;
        public List<Segment> Segments = new();
        public Collider HeadCollider;
        public enum Behaviors
        {
            LookingAtPlayer,
            Sleeping,
            Wandering,
            WakingUp
        }
        public Behaviors Behavior;
        public Vector2 TargetOffset;
        public Vector2 TestPoint;
        public Vector2 LineStart;
        public Vector2 LineEnd;
        public Vector2 DodgeEnd;
        private float wanderTimer;


        public bool Shocked;
        public float WanderAmount;
        public float Energy;
        public float EnergyRate;

        public const float LookingEnergyRate = -0.05f;
        public const float SleepingEnergyRate = 0.08f;
        public const float WanderingEnergyRate = -0.02f;
        public const float ShockedEnergyRate = -0.7f;
        public const float AngrySleepEnergyRate = 0.005f;
        public const float LookingAwareness = 1;
        public const float SleepingAwareness = 0.5f;
        public const float WanderingAwareness = 0.7f;
        public float Awareness;
        public float LeftAwarenessMult;
        public float RightAwarenessMult;
        public float PlayerDistance;
        public float PlayerSpeedX;

        private float maxRunTimer;

        public Coroutine WanderRoutine;

        public float RetractAmount;
        public bool Sleeping;
        public float outOfRangeTimer;

        public Vector2 Target;
        public Vector2 WanderTarget;
        public Collider SenseBox;
        public float ShockRate;
        public bool UpdateSegmentPosition;
        public Behaviors PreviousBehavior;

        private IEnumerator WanderLerp(float time)
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                WanderAmount = Ease.SineInOut(i);
                yield return null;
            }
            WanderAmount = 1;
        }
        public void Sleep()
        {
            if (Behavior == Behaviors.Sleeping) return;
            Behavior = Behaviors.Sleeping;
            Awareness = 0;
        }
        public void Wake(float energyMult)
        {
            Energy = 1 * energyMult;
            Behavior = Behaviors.WakingUp;
        }
        public void LookAtPlayer()
        {
            Behavior = Behaviors.LookingAtPlayer;
        }
        public void Wander()
        {
            if (Behavior != Behaviors.Wandering)
            {
                wanderTimer = 0;
                //Add(WanderRoutine = new Coroutine(Wandering()));
            }
            Behavior = Behaviors.Wandering;
        }
        public bool SensesPlayer()
        {
            return Math.Abs(PlayerDistance) < 1.5f && Awareness > 0.5f;
        }
        public void LookingAtPlayerUpdate()
        {
            if (Math.Abs(PlayerDistance) > 2)
            {
                outOfRangeTimer += Engine.DeltaTime;
            }
            else
            {
                outOfRangeTimer = 0;
            }
            if (outOfRangeTimer > 3)
            {
                Wander();
                outOfRangeTimer = 0;
                return;
            }
            Energy += LookingEnergyRate * Engine.DeltaTime;
            Target = Player.Center;
            LeftAwarenessMult = RightAwarenessMult = LookingAwareness;
        }
        public void WanderingUpdate()
        {
            wanderTimer -= Engine.DeltaTime;
            if (wanderTimer <= 0)
            {
                wanderTimer = Calc.Random.Range(4, 8f);
                float third = Head.Width / 3;
                int mult = Calc.Random.Choose(0, 2);
                float x = (mult * third) + Calc.Random.Range(0, third);
                Target = new Vector2(Head.RenderLeft + x, Head.RenderBottom - Calc.Random.Range(0f, Head.Height));
                Add(new Coroutine(WanderLerp(Calc.Random.Range(4, 8f))));
                foreach (Segment s in Segments)
                {
                    s.StartWander();
                }
            }

            outOfRangeTimer = 0;
            if (SensesPlayer())
            {
                LookAtPlayer();
                return;
            }
            Energy += WanderingEnergyRate * Engine.DeltaTime;
            LeftAwarenessMult = WanderingAwareness * (Facing is Facings.Right ? 0.5f : 1);
            RightAwarenessMult = WanderingAwareness * (Facing is Facings.Right ? 1 : 0.5f);
        }
        public void SleepingUpdate()
        {
            RetractAmount = Calc.Clamp(RetractAmount + Engine.DeltaTime, 0, 1);
            Energy += SleepingEnergyRate * Engine.DeltaTime;
            LeftAwarenessMult = RightAwarenessMult = SleepingAwareness;
            if (Energy >= 1)
            {
                Wake(4);
            }
            if (Awareness >= 1)
            {
                Wake(2);
            }
            Shocked = false;
            ShockRate = 0;
        }
        public void WakingUpUpdate()
        {
            RetractAmount = Calc.Clamp(RetractAmount - Engine.DeltaTime, 0, 1);
            if (RetractAmount == 0)
            {
                if (SensesPlayer()) LookAtPlayer();
                else Wander();
            }
        }
        public void UpdateBehavior()
        {
            switch (Behavior)
            {
                case Behaviors.LookingAtPlayer:
                    LookingAtPlayerUpdate();
                    break;
                case Behaviors.Sleeping:
                    SleepingUpdate();
                    break;
                case Behaviors.Wandering:
                    WanderingUpdate();
                    break;
                case Behaviors.WakingUp:
                    WakingUpUpdate();
                    break;
            }
            if (Math.Abs(PlayerDistance) <= 2)
            {
                if (Math.Abs(Player.Speed.X) >= Player.MaxRun)
                {
                    float mult = Math.Sign(PlayerDistance) == -1 ? LeftAwarenessMult : RightAwarenessMult;
                    mult *= 1 + Math.Abs(Player.Speed.X) / Player.MaxRun;
                    Awareness += Engine.DeltaTime * mult;
                }
                else
                {
                    Awareness -= Engine.DeltaTime;
                }
            }
            Awareness = Calc.Clamp(Awareness, 0, 1);
            Energy = Calc.Max(Energy, 0);
            if (!Shocked && Energy <= 0)
            {
                Sleep();
            }
            PreviousBehavior = Behavior;
        }
        public override void Update()
        {
            waveTimer += Engine.DeltaTime * Stress;
            PlayerDistance = (Player.CenterX - CenterX) / Width;
            PlayerSpeedX = Player.Speed.X;
            HeadCollider.Position = Head.RenderPosition.Round() - new Vector2(6, 8) + Head.ShockAmount;
            float targetDistance = (HeadCollider.CenterX - CenterX) / Width / 2;
            FacingTarget = Calc.Clamp(Math.Abs(targetDistance), 0, 1f);
            Facing = (Facings)(HeadCollider.CenterX < CenterX ? -1 : 1);
            UpdateBehavior();
            TargetOffset = TargetOffset.Round();
            base.Update();

            if (Shocked)
            {
                foreach (Segment s in Segments)
                {
                    if (!s.Shocked)
                    {
                        Shocked = false;
                        break;
                    }
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////

        public HelixWorm(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = -1000000;
            Collider = new Hitbox(40, MaxHeight);
            waveTimer = Calc.Random.NextFloat();
            DashListener listener = new DashListener();
            listener.OnDash = OnDash;
            Add(listener);
            Add(WanderRoutine = new Coroutine(false));
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(HeadCollider, Color.Orange);
            Draw.Rect(Position.X, Position.Y - 8, 8, 8, Color.Lerp(Color.Gray, Color.Red, Awareness));
            Draw.Rect(Position.X + 8, Position.Y - 8, 8, 8, Color.Lerp(Color.Gray, Color.Green, Energy));

            Draw.Point(Target, Color.White);
            Draw.Point(WanderTarget, Color.Yellow);


        }

        private IEnumerator WiggleChecker()
        {
            bool hitZero = false;
            int passes = 0;
            float passTimer = 0;
            int maxPasses = 5;
            while (true)
            {
                while (passes < maxPasses)
                {
                    if (passTimer <= 0)
                    {
                        passes = 0;
                    }
                    if (hitZero)
                    {
                        if (FacingTarget >= 0.2f)
                        {
                            passes++;
                            passTimer = 0.5f;
                            hitZero = false;
                        }
                    }
                    else if (FacingTarget <= 0.05f)
                    {
                        hitZero = true;
                    }
                    passTimer -= Engine.DeltaTime;
                    yield return null;
                }
                foreach (Segment s in Segments)
                {
                    s.Wiggle(0.2f, 12, 0.4f);
                }
                passes = 0;
                passTimer = 0;
                hitZero = false;
                yield return null;
            }
        }
        public void OnDash(Vector2 dir)
        {
            Vector2 p = Player.Center;
            Vector2 c = HeadCollider.Center;
            LineStart = p;
            LineEnd = LineStart + 48 * dir;
            if (Collide.RectToLine(HeadCollider.Bounds, LineStart, LineEnd))
            {
                Vector2 a = dir.Perpendicular();
                DodgeEnd = c + a * 32;
                if (Collide.LineCheck(LineStart, LineEnd, c, DodgeEnd))
                {
                    a = -a;
                    DodgeEnd = c + a * 32;
                }
                Shock(a);
            }
        }
        public void Shock(Vector2 dir)
        {
            Shocked = true;
            foreach (Segment s in Segments)
            {
                s.Shock(dir);
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player = scene.GetPlayer();
            Base = new Segment(new Vector2(Width / 2, Height), Limbs.Base)
            {
            };
            Neck = new Segment(Base, Vector2.UnitY * -Height, Limbs.Body)
            {
                Curve = new Vector2(35,-8),
                Left = -8,
                Right = 8,
                Top = -16,
                Bottom = -8,
                Track = Player
            };
            Head = new Segment(Neck, Vector2.UnitY * 8, Limbs.Head)
            {
                Curve = Vector2.UnitY * -8,
                Left = -12,
                Right = 12,
                Top = -16,
                Bottom = 8,
                Track = Player
            };
            HeadCollider = new Hitbox(12, 12, Head.RenderPosition.X - 6, Head.RenderPosition.Y - 8);
            Segments.Add(Base);
            Segments.Add(Neck);
            Segments.Add(Head);
            Add(Base, Neck, Head);
            Add(new Coroutine(WiggleChecker()));
        }
        public float AmountBetween(float input, float a, float b)
        {
            float min = Calc.Min(a, b);
            float max = Calc.Max(a, b);
            float foo = Calc.Max(input - min, 0);
            return Calc.Clamp(foo / (max - min), 0, 1);
        }
        public void DrawCurve(Vector2 a, Vector2 b, Vector2 curveOffset)
        {
            float sine = 1 + (float)Math.Sin(waveTimer) / 2f;
            SimpleCurve curve = new SimpleCurve(a, b, ((a + b) / 2f + curveOffset + curveOffset.SafeNormalize() * sine));

            Vector2 vector = curve.Begin;
            int steps = (int)Height;
            for (int j = 1; j <= steps; j++)
            {
                float percent = (float)j / steps;
                Vector2 point = curve.GetPoint(percent).Round();
                Draw.Line(vector, point, Color.White);
                vector = point + (vector - point).SafeNormalize();
            }
        }
    }
}
