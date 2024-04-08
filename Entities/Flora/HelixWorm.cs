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
        public const float BaseSpeed = 500f;
        public float MaxHeight = 32;
        public float waveTimer;
        public Facings Facing = Facings.Left;
        public float FacingTarget;
        public Vector2 Scale;
        public Player Player;
        public float Stress = 1;
        public Segment BaseSegment;
        public Segment NeckSegment;
        public Segment HeadSegment;
        public List<Segment> Segments = new();
        public Collider HeadCollider;
        public enum Behaviors
        {
            LookingAtPlayer,
            Sleeping,
            Wandering
        }
        public Behaviors Behavior;
        public Vector2 TargetOffset;
        public float Anger;
        public Vector2 TestPoint;
        public Vector2 LineStart;
        public Vector2 LineEnd;
        public Vector2 DodgeEnd;

        public bool Shocked;

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

        public float RetractAmount;
        public bool Sleeping;
        public float outOfRangeTimer;

        public Vector2 Target;
        public Collider SenseBox;
        public float ShockRate;
        public Behaviors PreviousBehavior;
        public void Sleep()
        {
            if (Behavior == Behaviors.Sleeping) return;
            Behavior = Behaviors.Sleeping;
            Awareness = 0;
        }
        public void Wake(float energyMult)
        {
            //Energy = 1 * energyMult;
            Energy = 0;
            Behavior = Behaviors.LookingAtPlayer;
        }
        public void LookingAtPlayerUpdate()
        {
            if (Anger > 0.5f)
            {
                Anger -= Engine.DeltaTime / 2;
            }
            else
            {
                Anger -= Engine.DeltaTime;
            }
            RetractAmount = Calc.Clamp(RetractAmount - Engine.DeltaTime, 0, 1);
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
                Behavior = Behaviors.Sleeping;
                outOfRangeTimer = 0;
                return;
            }
            //Energy += LookingEnergyRate * Engine.DeltaTime;
            Target = Player.Center + TargetOffset.Round();
            LeftAwarenessMult = RightAwarenessMult = LookingAwareness;
            Facing = (Facings)(Player.X < CenterX ? -1 : 1);
        }
        public void WanderingUpdate()
        {
            Anger -= Engine.DeltaTime;
            RetractAmount = Calc.Clamp(RetractAmount - Engine.DeltaTime, 0, 1);
            outOfRangeTimer = 0;
            if (Awareness >= 0.4f)
            {
                Behavior = Behaviors.LookingAtPlayer;
                return;
            }
            //Energy += WanderingEnergyRate * Engine.DeltaTime;
            LeftAwarenessMult = WanderingAwareness * (Facing is Facings.Right ? 0.5f : 1);
            RightAwarenessMult = WanderingAwareness * (Facing is Facings.Right ? 1 : 0.5f);
        }
        public void SleepingUpdate()
        {
            Anger -= 0.05f * Engine.DeltaTime;
            RetractAmount = Calc.Clamp(RetractAmount + Engine.DeltaTime, 0, 1);
            Energy += SleepingEnergyRate * Engine.DeltaTime;
            //LeftAwarenessMult = RightAwarenessMult = SleepingAwareness;
            //Target = new Vector2(Position.X, Bottom);
            if (Anger < 0.5f)
            {
                if (Shocked)
                {
                    Wake(3);
                    return;
                }
                if (Energy >= 1)
                {
                    Wake(2);
                }
            }
            Shocked = false;
            ShockRate = 0;
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
            }
            /*             if (Math.Abs(PlayerDistance) <= 2)
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
                        if (Awareness >= 1)
                        {
                            Behavior = Behaviors.LookingAtPlayer;
                        }
                       if (Shocked)
                        {
                            Energy += ShockedEnergyRate * Engine.DeltaTime;
                        }
                        Energy = Calc.Max(Energy, 0);
                        if (Energy <= 0)
                        {
                            Sleep();
                        }*/
            PreviousBehavior = Behavior;
        }
        public override void Update()
        {
            waveTimer += Engine.DeltaTime * Stress;
            PlayerDistance = (Player.CenterX - CenterX) / Width;
            PlayerSpeedX = Player.Speed.X;
            if (Scene.OnInterval(4))
            {
                //TargetOffset = Calc.AngleToVector(Calc.Random.NextAngle(), 4);
            }

            float targetDistance = (Target.X - CenterX) / Width;
            FacingTarget = Calc.Clamp(Math.Abs(targetDistance), 0, 1f);
            HeadCollider.Position = HeadSegment.RenderPosition.Round() - new Vector2(6, 8) + HeadSegment.ShockAmount;
            UpdateBehavior();
            Anger = Calc.Max(Anger, 0);
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
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(HeadCollider, Color.Orange);
            Draw.Rect(Position.X, Position.Y - 8, 8, 8, Color.Lerp(Color.Gray, Color.Red, Awareness));
            Draw.Rect(Position.X + 8, Position.Y - 8, 8, 8, Color.Lerp(Color.Gray, Color.Green, Energy));
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
            Anger += 0.1f;
            foreach (Segment s in Segments)
            {
                s.Shock(dir);
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player = scene.GetPlayer();
            BaseSegment = new Segment(new Vector2(Width / 2, Height), Limbs.Base)
            {
                SpeedMult = 5
            };
            NeckSegment = new Segment(BaseSegment, Vector2.UnitY * -Height, Limbs.Body)
            {
                Curve = Vector2.UnitX * 20,
                Left = -8,
                Right = 8,
                Top = -16,
                Bottom = -8,
                SpeedMult = 30,
                Track = Player
            };
            HeadSegment = new Segment(NeckSegment, Vector2.UnitY * 8, Limbs.Head)
            {
                Curve = Vector2.UnitY * -8,
                Left = -12,
                Right = 12,
                Top = -16,
                Bottom = 8,
                SpeedMult = 30,
                Track = Player
            };
            HeadCollider = new Hitbox(12, 12, HeadSegment.RenderPosition.X - 6, HeadSegment.RenderPosition.Y - 8);
            Segments.Add(BaseSegment);
            Segments.Add(NeckSegment);
            Segments.Add(HeadSegment);
            Add(BaseSegment, NeckSegment, HeadSegment);
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
