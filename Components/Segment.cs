using System;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class Segment : Component
    {
        public enum Limbs
        {
            Base,
            Head,
            Body
        }
        public Limbs Limb;
        public float Anger;
        public HelixWorm Parent => ParentIsValid ? Entity as HelixWorm : null;
        public bool ParentIsValid => Entity is not null && Entity is HelixWorm;
        public HelixWorm.Behaviors Behavior;
        public Facings Facing;
        public float waveTimer;

        public Vector2 Position = default;
        public Vector2 Speed = default;
        public Vector2 Curve;
        public Vector2 orig;
        public Vector2 origRenderPosition;
        public float Left, Top, Right, Bottom;
        public float LeftExt, TopExt, RightExt, BottomExt;
        public float RenderLeft => origRenderPosition.X + Left;
        public float RenderRight => origRenderPosition.X + Right;
        public float RenderTop => origRenderPosition.Y + Top;
        public float RenderBottom => origRenderPosition.Y + Bottom;
        public Segment Previous;

        public float Width => RenderRight - RenderLeft;
        public float Height => RenderBottom - RenderTop;
        public Vector2 RenderPosition
        {
            get
            {
                return (Previous == null ? (Entity == null) ? Vector2.Zero : Entity.Position : Previous.RenderPosition) + Position;
            }
            set
            {
                Position = value - (Previous == null ? ((Entity == null) ? Vector2.Zero : Entity.Position) : Previous.RenderPosition);
            }
        }
        public float SpeedMult = 1;
        public Entity Track;
        public Vector2 LimitScale;
        public float FacingTarget;
        public float FacingAmount;
        public float Stress;
        public bool Colliding;
        public Vector2 ShockOffset;
        public Vector2 ShockAmount;
        public bool Shocked;
        private float shockTimer;
        private float shockTime = 4;
        private float wiggleAmount = 1;
        public bool IgnoreBounds;
        public Vector2 TargetOffset;
        public bool Wiggling;
        public Vector2 Target;
        public float RetractAmount;
        public Rectangle Bounds;
        public Vector2 Momentum;
        public Vector2 CurveExtend;
        public float CurveExtendX;
        public float CurveExtendY;
        public float TurningSpeed => 60f * Engine.DeltaTime;
        public static MTexture Head = GFX.Game["objects/PuzzleIslandHelper/flora/helixWorm/head"];
        public Segment(Vector2 position, Limbs limb) : base(true, true)
        {

            orig = Position = position;
            Limb = limb;

        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            Left += orig.X;
            Right += orig.X;
            Top += orig.Y;
            Bottom += orig.Y;
            origRenderPosition = RenderPosition;
            Bounds = new Rectangle((int)Left, (int)Top, (int)Width, (int)Height);
        }
        public Segment(Segment previous, Vector2 position, Limbs limb) : this(position, limb)
        {
            Previous = previous;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (Limb == Limbs.Base) return;
            Draw.Line(RenderPosition.Round(), Previous.RenderPosition.Round(), Color.Lerp(Color.Brown, Color.Orange, (Speed.X + Speed.Y) / 2));
            Draw.HollowRect(RenderLeft, RenderTop, Width, Height, Color.Blue);
            if (Limb == Limbs.Head)
            {
                Draw.HollowRect(Target, 4, 4, Color.LightGreen);
                Draw.HollowRect(RenderPosition, 4, 4, Color.White);
            }
        }
        private const float Epsilon = 0.1f;
        public void StartWander()
        {
            SpeedMult = 0.2f;
        }
        public override void Update()
        {
            base.Update();
            if (ParentIsValid)
            {
                Facing = Parent.Facing;
                waveTimer = Parent.waveTimer;
                Behavior = Parent.Behavior;
                FacingTarget = Parent.FacingTarget;
                RetractAmount = Parent.RetractAmount;
                TargetOffset = Parent.TargetOffset;
                Target = Parent.Target + TargetOffset;
            }
            if (Limb == Limbs.Base) return;
            switch (Behavior)
            {
                case HelixWorm.Behaviors.LookingAtPlayer:
                    LookingAtPlayerUpdate();
                    break;
                case HelixWorm.Behaviors.Wandering:
                    WanderingUpdate();
                    break;
                case HelixWorm.Behaviors.Sleeping:
                    SleepingUpdate();
                    break;
            }

            FacingAmount = Calc.Approach(FacingAmount, -(int)Facing * FacingTarget, TurningSpeed);
            if (Vector2.Distance(Target, RenderPosition) > Epsilon)
            {
                Vector2 direction = (Target - RenderPosition).SafeNormalize();
                Speed.X = Math.Abs(direction.X) * Engine.DeltaTime;
                Speed.Y = Math.Abs(direction.Y) * Engine.DeltaTime;

                float x = Calc.LerpClamp(RenderPosition.X, Target.X, Speed.X * SpeedMult);
                float y = Calc.LerpClamp(RenderPosition.Y, Target.Y, Speed.Y * SpeedMult);
                RenderPosition = new Vector2(x,y);

                Position = Calc.Clamp(Position, Left, Top, Right, Bottom);

            }
            if (Behavior is HelixWorm.Behaviors.WakingUp or HelixWorm.Behaviors.Sleeping)
            {
                RenderPosition = Vector2.Lerp(RenderPosition, Previous.RenderPosition - Vector2.UnitY * 4, RetractAmount);
            }
            Momentum = Vector2.Lerp(Momentum, Vector2.Zero, Engine.DeltaTime);
        }
        public void SleepingUpdate()
        {
            if (Limb == Limbs.Base) return;
            Shocked = false;
            shockTimer = 0;
            ShockAmount = Vector2.Zero;
            ShockOffset = Vector2.Zero;

        }
        public void WanderingUpdate()
        {
            SpeedMult = Calc.LerpClamp(SpeedMult, 1, Engine.DeltaTime);
        }
        public void LookingAtPlayerUpdate()
        {
            if (Shocked)
            {
                shockTimer += Engine.DeltaTime;
                float lerp = Calc.Clamp(shockTimer / shockTime, 0, 1);
                float shockSpeed = Calc.LerpClamp(100f, 250f, 1 - Ease.SineOut(lerp));
                float offsetSpeed = Calc.Clamp(Calc.LerpClamp(-10f, 50f, Ease.SineOut(lerp)), 0, 50f);
                ShockAmount = Calc.Approach(ShockAmount, ShockOffset, shockSpeed * Engine.DeltaTime);
                ShockOffset = Approach(ShockOffset, Vector2.Zero, offsetSpeed * Engine.DeltaTime, out bool arrived);
                if (shockTimer > shockTime || arrived)
                {
                    ShockOffset = Vector2.Zero;
                    ShockAmount = Vector2.Zero;
                    Shocked = false;
                }
            }
            SpeedMult = 1 + Calc.Clamp(0.5f - FacingAmount,0,1);
        }
        public void Wiggle(float time, int wiggles, float amount)
        {
            if (Wiggling || Limb == Limbs.Base) return;
            AddRoutine(wiggleRoutine(time, wiggles, amount));
        }
        public void AddRoutine(IEnumerator routine)
        {
            if (Entity != null)
            {
                Entity.Add(new Coroutine(routine));
            }
        }
        public Vector2 Approach(Vector2 start, Vector2 end, float amount, out bool arrived)
        {
            arrived = start == end;
            if (arrived) return start;
            return Approach(start, end, amount);
        }
        public Vector2 Approach(Vector2 start, Vector2 end, float amount)
        {
            if (start.X != end.X)
            {
                start.X = Calc.Approach(start.X, end.X, amount);
            }
            if (start.Y != end.Y)
            {
                start.Y = Calc.Approach(start.Y, end.Y, amount);
            }
            return start;
        }
        private IEnumerator wiggleRoutine(float time, int wiggles, float amount)
        {
            Wiggling = true;
            bool back = false;
            wiggleAmount = 0;
            for (float j = 0; j < 1; j += (Engine.DeltaTime / time) * 2)
            {
                wiggleAmount = Calc.LerpClamp(1, -amount, j);
                yield return null;
            }
            for (int i = 0; i < wiggles; i++)
            {
                for (float j = 0; j < 1; j += Engine.DeltaTime / time)
                {
                    wiggleAmount = Calc.LerpClamp(back ? amount : -amount, back ? -amount : amount, j);
                    yield return null;
                }
                back = !back;
            }
            float from = wiggleAmount;
            for (float j = 0; j < 1; j += (Engine.DeltaTime / time) * 2)
            {
                wiggleAmount = Calc.LerpClamp(from, 0, j);
                yield return null;
            }
            Wiggling = false;
            for (float j = 0; j < 1; j += Engine.DeltaTime / time)
            {
                wiggleAmount = Calc.LerpClamp(0, 1, Ease.CubeIn(j));
                yield return null;
            }
            wiggleAmount = 1;
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
        public override void Render()
        {
            base.Render();
            if (Previous is null) return;
            //Vector2 extend = new Vector2(CurveExtend.X * FacingAmount, CurveExtend.Y * FacingAmount);
            Vector2 scaled = new Vector2(Curve.X * FacingAmount, Curve.Y) * wiggleAmount;
            float sine = (1 + (float)Math.Sin(waveTimer) / 2f);
            if (Limb == Limbs.Body)
            {
                sine *= 1.5f;
            }
            Vector2 a = Previous.RenderPosition + Previous.ShockAmount;
            Vector2 b = RenderPosition + ShockAmount;

            Vector2 control = scaled + (scaled.SafeNormalize() * sine);
            if (!Wiggling)
            {
                DrawCurve(a, b, control);
            }
            else
            {
                Vector2 m = (a + b) / 2;
                DrawCurve(a, m, control);
                DrawCurve(m, b, -control);
            }
            if (Limb == Limbs.Head)
            {
                Draw.SpriteBatch.Draw(Head.Texture.Texture_Safe, b - Vector2.One * 4, Color.White);
            }
        }
        public void Shock(Vector2 dir)
        {
            ShockOffset = dir * 32;
            shockTimer = 0;
            Shocked = true;
        }
    }
}
