using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities
{
    [Tracked]
    public class HoldableEntity : Actor
    {
        public string OrigRoom;
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
        public bool WasNotLeader = true;
        public Collider HoldingHitbox;
        public Collider IdleHitbox;
        public bool UsesCheckpointSystem;
        public float Rotation
        {
            get
            {
                if (Sprite is null) return 0;
                return Sprite.Rotation;
            }
            set
            {
                if (Sprite is null) return;
                Sprite.Rotation = value;
            }
        }

        public string GroupID;
        public bool IsLeader;
        public string SubID;
        public bool KillPlayerIfPit;
        public float TimePassed;
        public HoldableEntity(Vector2 position, EntityID id, bool usesCheckpointSystem, bool isLeader, string subID, string groupID, string spritePath) : base(position)
        {
            UsesCheckpointSystem = usesCheckpointSystem;
            IsLeader = isLeader;
            SubID = subID;
            GroupID = groupID;
            WasNotLeader = !IsLeader;
            Depth = 1;
            EntityID = id;
            Add(Sprite = new Sprite(GFX.Game, spritePath));
            Sprite.AddLoop("idle", "idle", 0.1f);
            Sprite.Play("idle");
            Sprite.Justify = Justify;
            Sprite.JustifyOrigin(Justify);

            Collider = new Hitbox(Sprite.Width, Sprite.Height, -Sprite.Width * Justify.X, -Sprite.Height * Justify.Y);
            addHoldable();
            int moe = 7;
            HoldingHitbox = new Hitbox(Sprite.Width - moe, Sprite.Height, moe / 2 - Sprite.Width * Justify.X, -Sprite.Height * Justify.Y);
            IdleHitbox = new Hitbox(Sprite.Width, Sprite.Height, -Sprite.Width * Justify.X, -Sprite.Height * Justify.Y);
            Collider = IdleHitbox;

            Add(Light = new VertexLight(Collider.Center, Color.White, 0.7f, 32, 64));
            Add(new MirrorReflection());
            Sprite.Play("idle");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (UsesCheckpointSystem && !PianoModule.Session.HoldableData.ShouldLoad(scene, this)) RemoveSelf();

        }
        private void addHoldable()
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

        }
        public virtual void OnCollideV(CollisionData data)
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

            if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
            {
                Speed.Y *= -0.6f;
            }
            else
            {
                Speed.Y = 0f;
            }
        }
        public virtual void OnCollideH(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            Audio.Play("event:/PianoBoy/stool_hit_side", Position);
            Speed.X *= -0.4f;
        }
        public virtual void OnPickup()
        {
            Collider = HoldingHitbox;
            if (WasNotLeader)
            {
                IsLeader = true;
            }
            if (!string.IsNullOrEmpty(GroupID) && IsLeader)
            {
                if (!PianoModule.Session.HoldableGroupIDs.Contains(GroupID))
                {
                    PianoModule.Session.HoldableGroupIDs.Add(GroupID);
                }
            }
            Sprite.JustifyOrigin(Justify);
            Sprite.Position.Y = 0;
            AddTag(Tags.Persistent);
        }
        public virtual bool HitSpring(Spring spring)
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
        public virtual void OnRelease(Vector2 force)
        {
            if (WasNotLeader)
            {
                IsLeader = false;
            }
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
        public void ResetCollider()
        {
            Collider = IdleHitbox;
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level) return;

            TimePassed += Engine.DeltaTime;
            Hold.CheckAgainstColliders();

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
                    if (KillPlayerIfPit && Collider.AbsoluteTop > level.Bounds.Bottom && player.Holding == null && !player.Dead)
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
        }
    }
    public struct HoldableData
    {
        public List<HoldableEntity> Entities = new();
        public bool ShouldLoad(Scene scene, HoldableEntity entity)
        {
            if ((scene as Level).Session.Level == entity.OrigRoom) return true;
            bool hasId = HasGroup(entity.GroupID);
            bool hasEntityID = Entities.Find(item => item.EntityID.ID == entity.EntityID.ID) != null;
            bool hasSubID = PianoModule.Session.HoldableCheckpointIDs.Contains(entity.SubID);

            if (!string.IsNullOrEmpty(entity.GroupID))
            {
                if (!PianoModule.Session.HoldableGroupIDs.Contains(entity.GroupID) && !entity.IsLeader)
                {
                    return false;
                }
                Player player = scene.GetPlayer();
                if (player.Holding is not null && player.Holding.Entity is HoldableEntity held && held != entity)
                {
                    if (held.IsLeader && entity.GroupID == held.GroupID || held.EntityID.Equals(entity.EntityID)) return false;
                }
            }
            return !hasId && !hasEntityID && (hasSubID || entity.IsLeader);
        }
        public bool HasGroup(string group)
        {
            if (string.IsNullOrEmpty(group)) return false;
            return Entities.Find(item => item.GroupID == group) != null;
        }
        public void AddEntity(HoldableEntity entity)
        {
            if (!Entities.Contains(entity))
            {
                Entities.Add(entity);
            }
        }
        public void Reset()
        {
            Entities.Clear();
        }
        public void RemoveEntity(HoldableEntity entity)
        {
            Entities.Remove(entity);

        }
        public HoldableData()
        {
        }
    }
}