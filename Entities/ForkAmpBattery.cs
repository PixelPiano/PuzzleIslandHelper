using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities.GearEntities;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/ForkAmpBattery")]
    [Tracked]
    public class ForkAmpBattery : Actor
    {
        public Vector2 PrevPosition;
        public float TimePassed;
        private float noGravityTimer;
        private float swatTimer;
        private float hardVerticalHitSoundCooldown;
        private static Vector2 Justify = new Vector2(0.5f, 1f);
        public Vector2 Speed;
        private Vector2 prevLiftSpeed;
        public Sprite Sprite;
        public Holdable Hold;
        public HoldableCollider hitSeeker;
        public VertexLight Light;
        private Collision onCollideV;
        private Collision onCollideH;
        public EntityID EntityID;
        public Collider HoldingHitbox;
        public Collider IdleHitbox;

        public string FlagOnFinish;
        public ForkAmpBattery(EntityData data, Vector2 offset, EntityID entityID) : base(data.Position + offset)
        {
            FlagOnFinish = data.Attr("flagOnFinish");
            Depth = 1;
            Add(Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/forkAmp/"));
            Sprite.AddLoop("idle", "battery", 0.1f, 0);
            Sprite.Justify = Justify;
            Sprite.JustifyOrigin(Justify);
            EntityID = entityID;
        }
        public IEnumerator Approach(float x)
        {
            while (Center.X != x)
            {
                CenterX = Calc.Approach(Center.X, x, 20f * Engine.DeltaTime);
                yield return null;
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            if (!string.IsNullOrEmpty(FlagOnFinish) && (scene as Level).Session.GetFlag(FlagOnFinish))
            {
                RemoveSelf();
            }
        }
        public void AddHoldable()
        {
            Add(Hold = new Holdable(0.1f));
            Hold.PickupCollider = new Hitbox(Width, Height, Collider.Position.X, Collider.Position.Y);
            Hold.SpeedSetter = delegate (Vector2 speed)
            {
                Speed = speed;
            };
            Hold.SlowFall = false;
            Hold.SlowRun = false;
            Hold.OnPickup = OnPickup;
            Hold.OnRelease = OnRelease;
            Hold.OnHitSpring = HitSpring;
            Hold.SpeedGetter = () => Speed;
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            LiftSpeedGraceTime = 0.1f;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            Collider = new Hitbox(Sprite.Width, Sprite.Height, -Sprite.Width * Justify.X, -Sprite.Height * Justify.Y);
            AddHoldable();
            int moe = 7;
            HoldingHitbox = new Hitbox(Sprite.Width - moe, Sprite.Height, moe / 2 - Sprite.Width * Justify.X, -Sprite.Height * Justify.Y);
            IdleHitbox = new Hitbox(Sprite.Width, Sprite.Height, -Sprite.Width * Justify.X, -Sprite.Height * Justify.Y);
            Collider = IdleHitbox;

            Add(Light = new VertexLight(Collider.Center, Color.White, 0.7f, 32, 64));
            Add(new MirrorReflection());
            Sprite.Play("idle");
        }
        #region Finished Methods
        private void OnCollideV(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            if (Speed.Y > 0f && TimePassed > 1)
            {
                if (hardVerticalHitSoundCooldown <= 0f)
                {

                    Audio.Play("event:/PianoBoy/stool_hit_ground", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f));
                    hardVerticalHitSoundCooldown = 0.5f;
                }
                else
                {
                    Audio.Play("event:/PianoBoy/stool_hit_ground", Position, "crystal_velocity", 0f);
                }
            }

            if (Speed.Y > 140f && data.Hit is not SwapBlock && data.Hit is not DashSwitch)
            {
                Speed.Y *= -0.6f;
            }
            else
            {
                Speed.Y = 0f;
            }

        }
        private void OnCollideH(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            Audio.Play("event:/PianoBoy/stool_hit_side", Position);
            Speed.X *= -0.4f;
        }
        private void OnPickup()
        {
            Collider = HoldingHitbox;
            Sprite.JustifyOrigin(Justify);
            Sprite.Position.Y = 0;
            AddTag(Tags.Persistent);
        }
        public bool HitSpring(Spring spring)
        {
            if (!Hold.IsHeld)
            {
                if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
                {
                    Speed.X *= 0.5f;
                    Speed.Y = -160f;
                    noGravityTimer = 0.15f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = 220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = -220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }
            }
            return false;
        }
        public void OnRelease(Vector2 force)
        {
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
            }
            Speed = force * 180f;
            if (Speed != Vector2.Zero)
            {
                noGravityTimer = 0.1f;
            }
            RemoveTag(Tags.Persistent);
        }
        #endregion
        public override void Update()
        {
            PrevPosition = Position;
            base.Update();
            if (Scene is not Level level) return;

            TimePassed += Engine.DeltaTime;
            Hold.CheckAgainstColliders();

            #region Copied
            if (swatTimer > 0f)
            {
                swatTimer -= Engine.DeltaTime;
            }
            hardVerticalHitSoundCooldown -= Engine.DeltaTime;

            if (Hold.IsHeld)
            {
                prevLiftSpeed = Vector2.Zero;
                Collider = HoldingHitbox;
                if (level.GetPlayer() is Player player)
                {
                    if (player.StateMachine.State == Player.StRedDash || player.StateMachine.State == Player.StBoost)
                    {
                        player.Drop();
                    }
                }

            }
            else
            {
                if (OnGround())
                {
                    Collider = IdleHitbox;
                    float target = !OnGround(Position + Vector2.UnitX * 3f) ? 20f : OnGround(Position - Vector2.UnitX * 3f) ? 0f : -20f;
                    Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
                    Vector2 liftSpeed = LiftSpeed;
                    if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                    {
                        Speed = prevLiftSpeed;
                        prevLiftSpeed = Vector2.Zero;
                        Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                        if (Speed.X != 0f && Speed.Y == 0f)
                        {
                            Speed.Y = -60f;
                        }
                        if (Speed.Y < 0f)
                        {
                            noGravityTimer = 0.15f;
                        }
                    }
                    else
                    {
                        prevLiftSpeed = liftSpeed;
                        if (liftSpeed.Y < 0f && Speed.Y < 0f)
                        {
                            Speed.Y = 0f;
                        }
                    }
                }
                else if (Hold.ShouldHaveGravity)
                {
                    float num = 800f;
                    if (Math.Abs(Speed.Y) <= 30f)
                    {
                        num *= 0.5f;
                    }
                    float num2 = 350f;
                    if (Speed.Y < 0f)
                    {
                        num2 *= 0.5f;
                    }
                    Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                    if (noGravityTimer > 0f)
                    {
                        noGravityTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
                    }
                }
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);

                TempleGate templeGate = CollideFirst<TempleGate>();
                Player player = level.GetPlayer();
                if (player is not null)
                {
                    if (Collider.AbsoluteTop > level.Bounds.Bottom && player.Holding == null && !player.Dead)
                    {
                        player.Die(Vector2.Zero);
                    }
                    if (templeGate != null)
                    {
                        templeGate.Collidable = false;
                        MoveH(Math.Sign(player.X - X) * 32 * Engine.DeltaTime);
                        templeGate.Collidable = true;
                    }
                }

            }
            if (hitSeeker != null && swatTimer <= 0f && !hitSeeker.Check(Hold))
            {
                hitSeeker = null;
            }
            #endregion

        }
    }
}