using Microsoft.Xna.Framework;
using Monocle;
using System;

// PuzzleIslandHelper.WaterDroplet
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]

    public class GravityParticle : Actor
    {
        private Player player;
        private bool debugColl;
        private Rectangle PlayerRectangle;
        public const float LifetimeMin = 4f;

        public const float LifetimeMax = 8f;

        public const float BlastAngleRange = (float)Math.PI * 3f / 4f;

        public const float MaxFallSpeed = 280f;

        public const float Gravity = 220f;

        public const float AirFriction = 5f;

        public const float GroundFriction = 330f;

        private Vector2 speed;

        private Vector2 previousLiftSpeed;

        private Level level;

        private float alpha;
        private bool startFade;

        private bool outline;

        private Color Color;
        private float colorLerp;

        private float OnGroundTimer;

        public GravityParticle(Vector2 position, Vector2 initialSpeed, Color color)
        : base(position)
        {
            int rand = Calc.Random.Choose(1, 1, 1, 1, 1, 1, 1, 2, 2, 3);
            Collider = new Hitbox(rand, rand);
            speed = initialSpeed;
            Color = color;
            colorLerp = Calc.Random.Range(0, 0.4f);
        }
        private bool OutsideBounds()
        {
            return Top >= (level.Bounds.Bottom + 5) || Bottom <= (level.Bounds.Top - 5) || Left >= (level.Bounds.Right + 5) || Right <= (level.Bounds.Left - 5);
        }
        public override void Update()
        {
            base.Update();
            if (player is not null)
            {
                PlayerRectangle.X = (int)(player.X - player.Width / 2);
                PlayerRectangle.Y = (int)(player.Y - player.Height - 5);
                PlayerRectangle.Height = (int)(player.Height + 5);
                //debugColl = CollideRect(PlayerRectangle);
            }
            Collidable = true;
            bool onGround = OnGround();

            float y = speed.Y;
            float num = (onGround ? 75f : 5f);
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
                    if (onGround)
                    {
                        speed.X = Calc.Approach(speed.X, 0, Engine.DeltaTime * GroundFriction);
                        //speed.X -= Engine.DeltaTime * GroundFriction;
                    }
                    else
                    {
                        //speed.X -= Engine.DeltaTime * AirFriction;
                        speed.X = Calc.Approach(speed.X, 0, Engine.DeltaTime * AirFriction);
                    }

                }
                if (OnGroundTimer > 5)
                {
                    startFade = true;
                }
                if (startFade)
                {
                    alpha -= Engine.DeltaTime;
                }
                if (OutsideBounds() || alpha <= 0)
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
            Draw.Rect(Collider, Color.Lerp(Color.White, Color, colorLerp) * alpha);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            alpha = 1;
            level = scene as Level;
            player = level.Tracker.GetEntity<Player>();
            if (player is not null)
            {
                PlayerRectangle = new Rectangle((int)(player.X - player.Width / 2), (int)(player.Y - player.Height - 5), (int)(player.Width), (int)(player.Height + 5));
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            //Draw.Rect(Collider, Color.Red);
            if (player is not null)
            {
                Draw.HollowRect(PlayerRectangle, Color.Black);
            }
        }
    }
}
