using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using MonoMod.Utils;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using System.Collections.Generic;
using FrostHelper;
using System.Windows.Media.Imaging;
using static Celeste.Mod.PuzzleIslandHelper.PuzzleData.AccessData;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities.GearEntities
{

    [CustomEntity("PuzzleIslandHelper/Gear")]
    [Tracked]
    public class Gear : Actor
    {
        public Vector2 PrevPosition;
        public Vector2 AttractTo;
        public bool InSlot;
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
        public GearHolder Holder;
        public bool Launching;
        private Vector2 launchDir;
        public EntityID EntityID;
        public bool WasNotLeader = true;
        public Collider HoldingHitbox;
        public Collider IdleHitbox;

        public float Rotation => Sprite is null ? 0 : Sprite.Rotation;
        public bool HolderIsNull => Holder is null;
        public bool StuckInGear;

        public string ContinuityID;
        public bool IsLeader;
        public string SubID;
        private float switchBackTimer;
        private bool switchColliders;
        public Gear(EntityData data, Vector2 offset, EntityID entityID) : base(data.Position + offset)
        {
            IsLeader = data.Bool("isLeader");
            SubID = data.Attr("subId");
            WasNotLeader = !IsLeader;
            ContinuityID = data.Attr("continuityID");
            Depth = 1;
            Add(Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/Gear/"));
            Sprite.AddLoop("idle", "Gear", 0.1f, 0);
            Sprite.Add("flash", "flash", 0.1f, "idle");
            Sprite.Justify = Justify;
            Sprite.JustifyOrigin(Justify);
            Add(new PostUpdateHook(Post));
            EntityID = entityID;
        }
        private void Post()
        {
            if (InSlot && Holder != null && Holder.InGearRoutine)
            {
                UpdateGear(Holder);
            }
        }
        public void Launch(Vector2 dir, float speed)
        {
            DropFromSlot();
            Sprite.JustifyOrigin(Justify);
            Sprite.Position.Y = 0;
            Vector2 newSpeed = dir * speed;
            if (newSpeed != Vector2.Zero)
            {
                Sprite.Play("flash");
                Speed = newSpeed;
                launchDir = dir;
                noGravityTimer = 0.2f;
                Launching = true;
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            bool isEmpty = string.IsNullOrEmpty(ContinuityID);
            if (!PianoModule.Session.GearData.ShouldLoad(this)) RemoveSelf();
            if (!isEmpty)
            {
                if (!PianoModule.Session.ContinuousGearIDs.Contains(ContinuityID) && !IsLeader)
                {
                    RemoveSelf();
                }
                Player player = scene.GetPlayer();
                if (player is not null && player.Holding is not null && player.Holding.Entity is Gear heldGear && heldGear != this)
                {
                    if (heldGear.IsLeader && ContinuityID == heldGear.ContinuityID || heldGear.EntityID.Equals(EntityID)) RemoveSelf();
                }
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
            if (Speed.Y > 0f && TimePassed > 1 && !InSlot)
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
            if (Launching && launchDir.Y != 0 && Math.Sign(launchDir.Y) == Math.Sign(data.Direction.Y))
            {
                Launching = false;
                launchDir = Vector2.Zero;
                Sprite.Play("flash");
            }

        }
        private void OnCollideH(CollisionData data)
        {
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            if (!InSlot)
            {
                Audio.Play("event:/PianoBoy/stool_hit_side", Position);
            }
            Speed.X *= -0.4f;
            if (Launching && launchDir.X != 0 && Math.Sign(launchDir.X) == Math.Sign(data.Direction.X))
            {
                Launching = false;
                launchDir = Vector2.Zero;
                Sprite.Play("flash");
            }
        }
        private void OnPickup()
        {
            Collider = HoldingHitbox;
            if (WasNotLeader)
            {
                IsLeader = true;
            }
            if (!string.IsNullOrEmpty(ContinuityID) && IsLeader)
            {
                if (!PianoModule.Session.ContinuousGearIDs.Contains(ContinuityID))
                {
                    PianoModule.Session.ContinuousGearIDs.Add(ContinuityID);
                }
            }
            if (Launching)
            {
                Player player = SceneAs<Level>().GetPlayer();
                if (player is not null)
                {
                    if (Math.Abs(Speed.X) > 16)
                    {
                        player.Speed.X += Speed.X * 0.5f;
                    }
                    if (Math.Abs(Speed.Y) > 16)
                    {
                        player.Speed.Y += Speed.Y * 0.5f;
                    }
                }
            }
            Launching = false;
            launchDir = Vector2.Zero;
            DropFromSlot();
            Sprite.JustifyOrigin(Justify);
            Sprite.Position.Y = 0;
            AddTag(Tags.Persistent);
        }
        public void UpdateGear(GearHolder holder)
        {
            if (holder is null) return;
            InSlot = true;
            Sprite.Rotation = holder.Rotation.ToRad();
            Center = holder.Center;
            Sprite.CenterOrigin();
            Sprite.Position.Y = -Sprite.Width / 2;
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
        #endregion
        public void DropFromSlot()
        {
            if (Launching) return;
            ResetCollider();
            Speed = Vector2.Zero;
            Sprite.Rotation = 0;
            prevLiftSpeed = Vector2.Zero;
            Holder = null;
            InSlot = false;
        }
        public void OnEnterSlot()
        {
            ResetCollider();
            Speed = Vector2.Zero;
            Sprite.Play("idle");
            Launching = false;
            launchDir = Vector2.Zero;
        }
        public void ResetCollider()
        {
            switchColliders = false;
            switchBackTimer = 0;
            Collider = IdleHitbox;
        }
        public override void Update()
        {
            PrevPosition = Position;
            base.Update();
            if (Scene is not Level level) return;

            TimePassed += Engine.DeltaTime;
            Hold.CheckAgainstColliders();

            if (!HolderIsNull && InSlot)
            {
                Collider = IdleHitbox;
            }
            if (CollideFirst<GearHolder>() is GearHolder holder)
            {
                if (holder.CanUseGear(this))
                {
                    holder.StartRoutine(this);
                    Holder = holder;
                }
            }

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
    public struct GearData
    {
        public List<Gear> Gears = new();
        public List<EntityID> HolderIDs = new();
        public bool HasHolder(EntityID id)
        {
            return HolderIDs.Contains(id);
        }
        public bool ShouldLoad(Gear gear)
        {
            bool hasId = HasGroup(gear.ContinuityID);
            bool hasEntityID = Gears.Find(item => item.EntityID.ID == gear.EntityID.ID) != null;
            bool hasSubID = PianoModule.Session.GearCheckpointIDs.Contains(gear.SubID);
            return !hasId && !hasEntityID && (hasSubID || gear.IsLeader);
        }
        public bool HasGroup(string continuity)
        {
            if (string.IsNullOrEmpty(continuity)) return false;
            return Gears.Find(item => item.ContinuityID == continuity) != null;
        }
        public void AddGear(Gear gear)
        {
            if (Gears.Contains(gear)) return;
            Gears.Add(gear);
            if (!gear.HolderIsNull && !HolderIDs.Contains(gear.Holder.EntityID))
            {
                HolderIDs.Add(gear.Holder.EntityID);
            }
        }
        public void Reset()
        {
            Gears.Clear();
            HolderIDs.Clear();
        }
        public void RemoveGear(Gear gear)
        {
            Gears.Remove(gear);

        }
        public GearData()
        {
        }
    }
}