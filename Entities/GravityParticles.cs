using Microsoft.Xna.Framework;
using Monocle;
using System;

// PuzzleIslandHelper.WaterDroplet
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]

    public class GravityParticle : Actor
    {
        public const float LifetimeMin = 4f;

        public const float LifetimeMax = 8f;

        public const float BlastAngleRange = (float)Math.PI * 3f / 4f;

        public const float MaxFallSpeed = 280f;

        public const float Gravity = 220f;

        public const float AirFriction = 5f;

        public const float GroundFriction = 330f;

        private Vector2 speed;

        public bool CanFadeAway = true;
        private Vector2 previousLiftSpeed;

        public float Alpha = 1;
        private bool startFade;

        private bool outline;

        private Color Color;
        private float colorLerp;

        private float OnGroundTimer;

        public GravityParticle(Vector2 position, Vector2 initialSpeed, Color color, float colorLerp = -1)
        : base(position)
        {
            int rand = Calc.Random.Choose(1, 1, 1, 1, 1, 1, 1, 2, 2, 3);
            Collider = new Hitbox(rand, rand);
            speed = initialSpeed;
            Color = color;
            this.colorLerp = colorLerp < 0 ? Calc.Random.Range(0, 0.4f) : colorLerp;
        }
        private bool OutsideBounds()
        {
            if (Scene is not Level level) return true;
            return Top >= (level.Bounds.Bottom + 5) || Bottom <= (level.Bounds.Top - 5) || Left >= (level.Bounds.Right + 5) || Right <= (level.Bounds.Left - 5);
        }
        public override void Update()
        {
            base.Update();
            Collidable = true;
            bool onGround = OnGround();
            float y = speed.Y;
            float num = onGround ? 75f : 5f;
            speed.X = Calc.Approach(speed.X, 0f, num * Engine.DeltaTime);
            if (!onGround)
            {
                speed.Y = Calc.Approach(speed.Y, 280f, 220f * Engine.DeltaTime);
                OnGroundTimer = 0;
            }
            else
            {
                OnGroundTimer += Engine.DeltaTime;
            }
            if (speed != Vector2.Zero)
            {
                Vector2 position = Position;
                MoveH(speed.X * Engine.DeltaTime, OnCollideH);
                MoveV(speed.Y * Engine.DeltaTime, OnCollideV);
                bool interval = Scene.OnInterval(0.035f);
                if (speed.Y != 0f)
                {
                    speed.Y += Engine.DeltaTime * Gravity;
                }
                if (speed.X != 0f)
                {
                    speed.X = Calc.Approach(speed.X, 0, Engine.DeltaTime * (onGround ? GroundFriction : AirFriction));
                }
                if (OnGroundTimer > 5)
                {
                    startFade = true;
                }
                if (startFade && CanFadeAway)
                {
                    Alpha -= Engine.DeltaTime;
                }
                if (OutsideBounds() || Alpha <= 0)
                {
                    RemoveSelf();
                }
            }
            if (previousLiftSpeed != Vector2.Zero && LiftSpeed == Vector2.Zero)
            {
                speed += previousLiftSpeed;
            }
            previousLiftSpeed = LiftSpeed;
            Collidable = false;
        }
        private void OnCollideH(CollisionData data)
        {
            if (speed.X > 0)
            {
                speed.X = -speed.X / 3f;
            }
            else
            {
                speed.X = 0;
            }
        }
        private void OnCollideV(CollisionData data)
        {
            if (OnGroundTimer > 0.1f)
            {
                speed.X = Calc.Approach(speed.X, 0, GroundFriction * Engine.DeltaTime);
            }
            else if (speed.Y > 0)
            {
                speed.Y = -speed.Y / 2.4f;
            }
            else
            {
                speed.Y = 0;
            }
        }
        public override void Render()
        {
            base.Render();
            Draw.Rect(Collider, Color.Lerp(Color.White, Color, colorLerp) * Alpha);
            if (outline)
            {
                Draw.HollowRect(Collider.AbsolutePosition - Vector2.One, Width + 2, Height + 2, Color.Black);
            }
        }
    }
}
