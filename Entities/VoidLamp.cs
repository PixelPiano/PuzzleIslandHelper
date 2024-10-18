using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VivHelper.Entities;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class VoidLampJudge : Entity
    {
        public VoidLampJudge() : base()
        {
            Tag |= Tags.TransitionUpdate;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (scene is Level level && scene.GetPlayer() is Player player)
            {
                List<Entity> lamps = scene.Tracker.GetEntities<VoidLamp>();
                VoidLamp closest = null;
                Vector2 spawn = level.GetSpawnPoint(player.Position);
                float dist = int.MaxValue;
                foreach (VoidLamp lamp in lamps)
                {
                    if (lamp.CompeteForPlayerOnSpawn)
                    {
                        float lampdist = Vector2.DistanceSquared(spawn, lamp.Position);
                        if (lampdist < dist)
                        {
                            dist = lampdist;
                            closest = lamp;
                        }
                    }
                }
                foreach (VoidLamp lamp in lamps)
                {
                    if (lamp.CompeteForPlayerOnSpawn && lamp != closest)
                    {
                        lamp.RemoveSelf();
                    }
                }
            }
            RemoveSelf();
        }
    }
    [CustomEntity("PuzzleIslandHelper/VoidLamp")]
    [Tracked]
    public class VoidLamp : Actor
    {
        private float holdTimer = 0;

        public Vector2 prevPosition = Vector2.Zero;

        public bool IsHeld = false;
        private EntityID id;

        private float noGravityTimer;
        private float swatTimer;
        private float hardVerticalHitSoundCooldown;

        private static Vector2 SpriteJustify = new Vector2(0.5f, 1f);
        public Vector2 Speed;
        private Vector2 prevLiftSpeed;

        public Sprite sprite;
        public Holdable Hold;
        public HoldableCollider hitSeeker;
        private Hitbox HoldingHitbox;
        public VertexLight Light;
        private Collision onCollideV;
        private Collision onCollideH;
        private VoidSafeZone SafeZone;
        private PulsingCircle BigCircle;
        private PulsingCircle SmallCircle;
        public CritterLight Mask;
        public bool TransitionCheck;
        public string Flag;
        public bool Inverted;
        private string groupID;
        private bool GroupIsVisible;
        private bool isGroupLeader;
        public bool FlagState;
        public bool Enabled => FlagState && GroupIsVisible;
        public Vector2 Scale = Vector2.One;
        public PlayerCritterLight PlayerLight;
        private Wiggler wiggler;
        private bool destroyed;
        public bool CompeteForPlayerOnSpawn;
        public VoidLamp(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            this.id = id;
            Depth = 1;
            Tag |= Tags.TransitionUpdate;
            Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/voidLamp/"));
            sprite.AddLoop("idle", "wip", 0.1f);
            sprite.Play("idle");
            sprite.Justify = SpriteJustify;
            sprite.JustifyOrigin(SpriteJustify);
            Add(Hold = new Holdable(0.5f));
            Inverted = data.Bool("inverted");
            Flag = data.Attr("flag");
            TransitionCheck = data.Bool("transitionCheck");
            CompeteForPlayerOnSpawn = data.Bool("removeIfNotClosestToSpawn");
            groupID = data.Attr("groupID");
            isGroupLeader = data.Bool("isGroupLeader", true);
            Hold.PickupCollider = new Hitbox(sprite.Width, sprite.Height);
            Hold.PickupCollider.Position = new Vector2(-sprite.Width, -sprite.Height) * SpriteJustify;
            Collider = new Hitbox(8, sprite.Height);
            Collider h = Hold.PickupCollider;
            Collider.Position = h.Position + new Vector2(h.Width / 2 - 4, 0);

            Hold.SpeedSetter = delegate (Vector2 speed)
            {
                Speed = speed;
            };
            Hold.SlowFall = false;
            Hold.SlowRun = false;
            Hold.OnPickup = OnPickup;
            Hold.OnRelease = OnRelease;
            Hold.DangerousCheck = Dangerous;
            Hold.OnHitSeeker = HitSeeker;
            Hold.OnSwat = Swat;
            Hold.OnHitSpring = HitSpring;
            Hold.OnHitSpinner = HitSpinner;
            Hold.SpeedGetter = () => Speed;
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            LiftSpeedGraceTime = 0.1f;
            Add(Light = new VertexLight(Collider.Center, Color.White, 0.7f, 32, 64));
            Add(new MirrorReflection());
            Add(SafeZone = new(Hold.PickupCollider.Position + Hold.PickupCollider.HalfSize - Vector2.One * 20, 40, 40, false));
            SmallCircle = new PulsingCircle(SafeZone.Center, Calc.Max(0, SafeZone.Width / 2 - 4), 0.3f, 1, 2);
            BigCircle = new PulsingCircle(SafeZone.Center, Calc.Max(0, SafeZone.Width / 2 - 8), 0.5f, 3, 3);
            Add(BigCircle, SmallCircle);
            BigCircle.StartTween(Ease.SineInOut, 2, 0.2f, 4);
            SmallCircle.StartTween(Ease.SineInOut, 2, 0.1f, 8);
            Mask = new CritterLight(SafeZone.Width / 1.5f, Light);
            Add(Mask);
            Mask.Enabled = true;
            Add(new Coroutine(SpinRoutine()));
        }
        private IEnumerator SpinRoutine()
        {
            float scale = 2;
            float maxSpeed = 1.6f;
            float fastest = 0;
            float speed = maxSpeed / 2;
            while (true)
            {
                if (Mask.InCritterWall)
                {
                    speed = Calc.Approach(speed, maxSpeed, Engine.DeltaTime);
                    fastest = Calc.Min(speed, maxSpeed);
                    scale += speed * Engine.DeltaTime;
                }
                else
                {
                    scale = Calc.Approach(scale, 2, fastest * Engine.DeltaTime);
                    speed = maxSpeed / 2;
                }
                if (scale > 2)
                {
                    scale -= 2;
                }
                BigCircle.Scale.X = Ease.SineInOut(scale - 1);
                yield return null;
            }
        }
        public override void Render()
        {
            if (Enabled)
            {
                base.Render();
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (CompeteForPlayerOnSpawn && PianoUtils.SeekController<VoidLampJudge>(scene) == null)
            {
                scene.Add(new VoidLampJudge());
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            FlagState = (string.IsNullOrEmpty(Flag) || SceneAs<Level>().Session.GetFlag(Flag)) != Inverted;
            GroupIsVisible = isGroupLeader || string.IsNullOrEmpty(groupID) || PianoModule.Session.VoidLampGroups.Contains(groupID);
            PlayerLight = Scene.Tracker.GetEntity<PlayerCritterLight>();
            if (Scene.GetPlayer() is Player player)
            {
                if (TransitionCheck && player.Holding is Holdable holdable && holdable.Entity is VoidLamp lamp && lamp != this && lamp.groupID.Equals(groupID))
                {
                    RemoveSelf();
                    return;
                }
            }
        }
        [Tracked]
        public class PlayerCritterLight : Entity
        {
            public CritterLight Light;
            public Player Player;
            public PlayerCritterLight() : base()
            {
                Tag |= Tags.TransitionUpdate | Tags.Global;
            }
            public void Start(float radius)
            {
                Light.Radius = radius;
                Light.Enabled = true;
            }
            public void Stop()
            {
                if (!SaveData.Instance.Assists.Invincible)
                {
                    Light.Radius = 0;
                    Light.Enabled = false;
                }
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                if (scene.GetPlayer() is Player player)
                {
                    Light = new CritterLight(0, player.Light);
                    Add(Light);
                }
                else RemoveSelf();
            }
            public override void Update()
            {
                base.Update();
                if (Scene is not Level level || level.GetPlayer() is not Player player) return;
                if (SaveData.Instance.Assists.Invincible)
                {
                    Light.Radius = 24;
                    Light.Enabled = true;
                }
                Light.Light = player.Light;
                Position = player.Position;
                Light.GradientBoost = 0.1f + player.Dashes switch
                {
                    1 => 0.1f,
                    2 => 0.5f,
                    _ => 0
                };
            }
            [OnLoad]
            public static void Load()
            {
                Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
            }

            [OnUnload]
            public static void Unload()
            {
                Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
            }

            private static void LevelLoader_OnLoadingThread(Level level)
            {
                level.Add(new PlayerCritterLight());
            }
        }
        private void OnPickup()
        {
            if (!string.IsNullOrEmpty(groupID) && !PianoModule.Session.VoidLampGroups.Contains(groupID))
            {
                PianoModule.Session.VoidLampGroups.Add(groupID);
            }
            PlayerLight.Start(SafeZone.Width / 1.5f);
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
            SceneAs<Level>().Session.DoNotLoad.Add(id);
        }
        public void OnRelease(Vector2 force)
        {
            PlayerLight.Stop();
            holdTimer = 0.5f;
            RemoveTag(Tags.Persistent);
            if (SceneAs<Level>().Session.DoNotLoad.Contains(id))
            {
                SceneAs<Level>().Session.DoNotLoad.Remove(id);
            }
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
            }
            Speed = force * 200f;
            if (Speed != Vector2.Zero)
            {
                noGravityTimer = 0.1f;
            }
        }
        #region On Methods
        private void OnCollideV(CollisionData data)
        {
            if (Enabled)
            {
                if (data.Hit is DashSwitch)
                {
                    (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
                }
                if (Speed.Y > 0f)
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
            }


            if (Speed.Y > 140f && (!Enabled || (!(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))))
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
            if (Enabled)
            {
                if (data.Hit is DashSwitch)
                {
                    (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
                }
                Audio.Play("event:/PianoBoy/stool_hit_side", Position);
            }
            Speed.X *= -0.4f;
        }
        public void Swat(HoldableCollider hc, int dir)
        {
            if (Hold.IsHeld && hitSeeker == null && Enabled)
            {
                swatTimer = 0.1f;
                hitSeeker = hc;
                Hold.Holder.Swat(dir);
            }
        }
        public bool HitSpring(Spring spring)
        {
            if (!Hold.IsHeld && Enabled)
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

        public void HitSpinner(Entity spinner)
        {
            /*            if (!Hold.IsHeld && Speed.Length() < 0.01f && base.LiftSpeed.Length() < 0.01f && (previousPosition - base.ExactPosition).Length() < 0.01f && OnGround())
                        {
                            int num = Math.Sign(base.X - spinner.X);
                            if (num == 0)
                            {
                                num = 1;
                            }
                            Speed.X = (float)num * 120f;
                            Speed.Y = -30f;
                        }*/
        }
        public void HitSeeker(Seeker seeker)
        {
            if (Enabled)
            {
                if (!Hold.IsHeld)
                {
                    Speed = (Center - seeker.Center).SafeNormalize(120f);
                }
                Audio.Play("event:/PianoBoy/stool_hit_side", Position);
            }
        }
        public bool Dangerous(HoldableCollider holdableCollider)
        {
            if (!Enabled) return false;
            if (!Hold.IsHeld && Speed != Vector2.Zero)
            {
                return hitSeeker != holdableCollider;
            }
            return false;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
        private IEnumerator DestroyAnimationRoutine()
        {
            //todo: Add destroy animation
            yield return 1;
            RemoveSelf();
        }
        #endregion
        public override void Update()
        {
            base.Update();
            Hold.CheckAgainstColliders();
            SafeZone.IsSafe = Enabled;
            if (Scene is not Level level || level.GetPlayer() is not Player player)
            {
                return;
            }
            bool onGround = OnGround();
            if (!destroyed)
            {
                foreach (SeekerBarrier entity in base.Scene.Tracker.GetEntities<SeekerBarrier>())
                {
                    entity.Collidable = true;
                    bool collided = CollideCheck(entity);
                    entity.Collidable = false;
                    if (collided)
                    {
                        destroyed = true;
                        Collidable = false;
                        if (Hold.IsHeld)
                        {
                            Vector2 speed2 = Hold.Holder.Speed;
                            Hold.Holder.Drop();
                            Speed = speed2 * 0.333f;
                            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        }

                        Add(new Coroutine(DestroyAnimationRoutine()));
                        return;
                    }
                }
                FlagState = (string.IsNullOrEmpty(Flag) || level.Session.GetFlag(Flag)) != Inverted;

                if (!Enabled)
                {
                    if (Hold.IsHeld)
                    {
                        player.Drop();
                    }
                    Hold.cannotHoldTimer = Engine.DeltaTime * 2;
                }
                else
                {

                    Mask.GradientBoost = 0.1f + player.Dashes switch
                    {
                        1 => 0.1f,
                        2 => 0.5f,
                        _ => 0
                    };
                }
                SafeZone.Position = Hold.PickupCollider.Position + Hold.PickupCollider.HalfSize - Vector2.One * 20;
                Mask.Enabled = Enabled;
                #region Copied
                if (swatTimer > 0f)
                {
                    swatTimer -= Engine.DeltaTime;
                }
                if (hardVerticalHitSoundCooldown > 0f)
                {
                    hardVerticalHitSoundCooldown -= Engine.DeltaTime;
                }
                if (Hold.IsHeld)
                {
                    prevLiftSpeed = Vector2.Zero;
                }
                else if (!level.Transitioning)
                {
                    if (onGround)
                    {
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
                    if (Center.X > level.Bounds.Right)
                    {
                        MoveH(32f * Engine.DeltaTime);
                        if (Left - 8f > level.Bounds.Right)
                        {
                            RemoveSelf();
                        }
                    }
                    else if (Left < level.Bounds.Left)
                    {
                        Left = level.Bounds.Left;
                        Speed.X *= -0.4f;
                    }
                    else if (Top < level.Bounds.Top - 4)
                    {
                        Top = level.Bounds.Top + 4;
                        Speed.Y = 0f;
                    }
                    else if (Bottom > level.Bounds.Bottom && SaveData.Instance.Assists.Invincible)
                    {
                        Bottom = level.Bounds.Bottom;
                        Speed.Y = -300f;
                        Audio.Play("event:/game/general/assist_screenbottom", Position);
                    }
                    if (X < level.Bounds.Left + 10)
                    {
                        MoveH(32f * Engine.DeltaTime);
                    }
                    Player entity = Scene.Tracker.GetEntity<Player>();
                    TempleGate templeGate = CollideFirst<TempleGate>();
                    if (templeGate != null && entity != null)
                    {
                        templeGate.Collidable = false;
                        MoveH(Math.Sign(entity.X - X) * 32 * Engine.DeltaTime);
                        templeGate.Collidable = true;
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
}