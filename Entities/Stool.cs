using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using MonoMod.Utils;
using static MonoMod.InlineRT.MonoModRule;
using Celeste.Mod.PuzzleIslandHelper.Components;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class StoolListener : Component
    {
        public Action<Stool> OnRaised;
        public Action<Stool> OnLowered;
        public Action<Stool> OnGiveDashes;
        public StoolListener(Action<Stool> onRaised, Action<Stool> onLowered = null, Action<Stool> onGiveDashes = null) : base(true, false)
        {
            OnRaised = onRaised;
            OnLowered = onLowered;
            OnGiveDashes = onGiveDashes;
        }
        public StoolListener() : base(true, false)
        {

        }
    }
    [Tracked]
    public class StoolSnapComponent : Component
    {
        public StoolSnapComponent() : base(true, true)
        {

        }
        public bool Collide(Stool stool)
        {
            return Entity != null && Entity.CollideCheck(stool);
        }
    }
    [CustomEntity("PuzzleIslandHelper/Stool")]
    [Tracked]
    public class Stool : Actor
    {
        [Command("refill_stool", "adds a refill to the nearest stool")]
        public static void RefillStool(int dashes = 2)
        {
            if (dashes > 1 && Engine.Scene.GetPlayer() is Player player)
            {
                if (Engine.Scene.Tracker.GetNearestEntity<Stool>(player.Center) is Stool stool)
                {
                    stool.DashesHeld = dashes;
                }
            }
        }

        private string flag;
        public Vector2 prevPosition = Vector2.Zero;
        public bool MoveStool = false;
        public bool IsHeld = false;
        private bool Inverted;
        private EntityID id;
        public bool FlagState => (string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag)) != Inverted;
        private bool Raised = false;
        private Coroutine coroutine;
        private float noGravityTimer;
        private float swatTimer;
        private float PlatformYOffset;
        private float hardVerticalHitSoundCooldown;
        public int DashesHeld = 0;

        private static Vector2 StoolJustify = new Vector2(0.5f, 1f);
        public Vector2 Speed;
        private Vector2 prevLiftSpeed;
        private Vector2 pickupColliderOrigPosition;

        private Color refillColor
        {
            get
            {
                return DashesHeld switch
                {
                    <= 1 => Color.White,
                    2 => Color.LimeGreen,
                    > 2 => Color.Red
                };
            }
        }

        public Sprite sprite;
        public Holdable Hold;
        public HoldableCollider hitSeeker;
        private Hitbox HoldingHitbox;
        public Hitbox RaisedHitbox;
        public Hitbox LoweredHitbox;
        private StoolPlatform platform;
        public VertexLight Light;
        private Color orig_Color = Color.White;
        private Collision onCollideV;
        private Collision onCollideH;
        private Vector2 spriteOffset;
        private int extraDashes;
        private float flashTimer;
        private float gracePeriod;
        private float loweredOffset => sprite.Height - 9;
        private float raisedOffset = 1;
        public Vector2 PlatformPosition => Position + spriteOffset + new Vector2(1, PlatformYOffset);
        [Tracked]
        public class StoolPlatform : JumpThru
        {
            public Vector2 LiftSpeedMult = new Vector2(1, 1);
            public bool NoLiftSpeed;
            public Stool Stool;
            public StoolPlatform(Stool parent, Vector2 position, int width, bool safe) : base(position, width, safe)
            {
                Stool = parent;
            }
            public override void MoveHExact(int move)
            {
                base.MoveHExact(move);
            }

            public override void MoveVExact(int move)
            {
                int cancelMult = NoLiftSpeed ? 0 : 1;
                if (Collidable)
                {
                    if (move < 0)
                    {
                        foreach (Actor entity in base.Scene.Tracker.GetEntities<Actor>())
                        {
                            if (entity.IsRiding(this))
                            {
                                Collidable = false;
                                if (entity.TreatNaive)
                                {
                                    entity.NaiveMove(Vector2.UnitY * move);
                                }
                                else
                                {
                                    entity.MoveVExact(move);
                                }
                                entity.LiftSpeed = LiftSpeed * LiftSpeedMult * cancelMult;
                                Collidable = true;
                            }
                            else if (!entity.TreatNaive && CollideCheck(entity, Position + Vector2.UnitY * move) && !CollideCheck(entity))
                            {
                                Collidable = false;
                                entity.MoveVExact((int)(base.Top + (float)move - entity.Bottom));
                                entity.LiftSpeed = LiftSpeed * LiftSpeedMult * cancelMult;
                                Collidable = true;
                            }
                        }
                    }
                    else
                    {
                        foreach (Actor entity2 in base.Scene.Tracker.GetEntities<Actor>())
                        {
                            if (entity2.IsRiding(this))
                            {
                                Collidable = false;
                                if (entity2.TreatNaive)
                                {
                                    entity2.NaiveMove(Vector2.UnitY * move);
                                }
                                else
                                {
                                    entity2.MoveVExact(move);
                                }

                                entity2.LiftSpeed = LiftSpeed * cancelMult;
                                Collidable = true;
                            }
                        }
                    }
                }
                Y += move;
                MoveStaticMovers(Vector2.UnitY * move);
            }
        }
        private bool stackSpeedCancel;
        //todo: add flash
        public Stool(Vector2 position, string flag, bool inverted, EntityID id) : base(position)
        {
            Depth = 1;
            this.id = id;
            this.flag = flag;
            Inverted = inverted;
            Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/stool/"));
            sprite.AddLoop("down", "stoolext", 0.1f, 0);
            sprite.AddLoop("up", "stoolext", 0.1f, 8);
            sprite.Add("Rise", "stoolext", 0.02f, "up");
            sprite.Add("Lower", "stoolLower", 0.05f, "down");
            sprite.Play("down");
            sprite.Justify = StoolJustify;
            sprite.JustifyOrigin(StoolJustify);

            spriteOffset = new Vector2(-sprite.Width * StoolJustify.X, -sprite.Height * StoolJustify.Y);
            LoweredHitbox = new Hitbox(sprite.Width - 8, 10, spriteOffset.X, spriteOffset.Y);
            RaisedHitbox = new Hitbox(sprite.Width - 8, sprite.Height, spriteOffset.X, spriteOffset.Y);
            Collider = RaisedHitbox;
            PlatformYOffset = loweredOffset;
            HoldingHitbox = new Hitbox(sprite.Width - 8, sprite.Height, spriteOffset.X + 2, spriteOffset.Y);
            Add(Light = new VertexLight(Collider.Center, Color.White, 0.7f, 32, 64));
            Add(Hold = new Holdable(0.1f)
            {
                SlowFall = false,
                SlowRun = false,
                OnPickup = OnPickup,
                OnRelease = OnRelease,
                DangerousCheck = Dangerous,
                OnHitSeeker = HitSeeker,
                OnSwat = Swat,
                OnHitSpring = HitSpring,
                OnHitSpinner = HitSpinner,
                SpeedSetter = (speed) => Speed = speed,
                SpeedGetter = () => Speed,
                PickupCollider = new Hitbox(sprite.Width, sprite.Height + 2, spriteOffset.X, spriteOffset.Y + 2)
            });

            Collider = HoldingHitbox;
            pickupColliderOrigPosition = Hold.PickupCollider.Position;
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            LiftSpeedGraceTime = 0.1f;
            Add(new MirrorReflection());
            Add(coroutine = new Coroutine(false));
            Add(new DashListener()
            {
                OnDash = (dir) =>
                {
                    if (!Raised && platform.HasPlayerRider() && dir.X != 0)
                    {
                        gracePeriod = 0.3f;
                    }
                }
            });
        }
        public Stool(EntityData data, Vector2 offset, EntityID id)
        : this(data.Position + offset, data.Attr("flag"), data.Bool("inverted"), id)
        {

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(platform = new StoolPlatform(this, PlatformPosition, (int)sprite.Width - 2, false));
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            platform.OnDashCollide = OnDashed;
            SetState();
        }
        public void UpdateColliders()
        {
            Collider p = Hold.PickupCollider;
            Vector2 o = pickupColliderOrigPosition;
            Collider = Raised ? RaisedHitbox : LoweredHitbox;
            p.Height = Collider.Height - 2;
            p.Position.X = o.X;
            p.Position.Y = Raised ? o.Y : o.Y + 10;
            Collider.Position.X = p.Position.X + 4;
            Collider.Position.Y = Raised ? o.Y - 2 : o.Y + 8;
        }
        public bool IsRiding<T>() where T : Platform
        {
            if (IgnoreJumpThrus)
            {
                return false;
            }
            bool c = platform.Collidable;
            platform.Collidable = false;
            bool result = CollideCheckOutside<T>(Position + Vector2.UnitY);
            platform.Collidable = c;
            return result;
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            SetState();

            if (!FlagState) return;
            orig_Color = refillColor;
            sprite.Color = Hold.cannotHoldTimer > 0f ? Color.Lerp(orig_Color, Color.Black, 0.3f) : orig_Color;
            #region Copied
            if (swatTimer > 0f)
            {
                swatTimer -= Engine.DeltaTime;
            }
            hardVerticalHitSoundCooldown -= Engine.DeltaTime;
            if (Hold.IsHeld)
            {
                prevLiftSpeed = Vector2.Zero;
            }
            else
            {
                if (OnGround())
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
                            Hold.gravityTimer = 0.15f;
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
            if (IsRiding<StoolPlatform>())
            {
                prevLiftSpeed = Vector2.Zero;
                //noLiftSpeedTimer += Engine.DeltaTime;
            }
            platform.NoLiftSpeed = noLiftSpeedTimer > 0;
            noLiftSpeedTimer = Calc.Max(0, noLiftSpeedTimer - Engine.DeltaTime);
            bool prev = platform.Collidable;
            if (Hold.IsHeld && prev)
            {
                platform.Collidable = platform.Bottom < player.Top;
            }
            if (!coroutine.Active)
            {
                platform.LiftSpeedMult.Y = 1;
            }
            platform.MoveTo(PlatformPosition);
            if (!Hold.IsHeld && !Raised && !coroutine.Active)
            {
                bool swap = false;
                if (player.DashAttacking && player.DashDir.X != 0 && player.DashDir.Y == 0 && player.CollideCheck(platform) && MathHelper.Distance(player.Bottom, platform.Top) > 4)
                {
                    if (player.Bottom > platform.Top)
                    {
                        player.MoveToY(platform.Top);
                    }
                    swap = true;
                }
                if (gracePeriod > 0)
                {
                    if (!player.IsRiding(platform))
                    {
                        gracePeriod -= Engine.DeltaTime;
                    }
                    if (Input.Jump.Pressed)
                    {
                        player.LiftSpeed = new Vector2(player.LiftSpeed.X, player.LiftSpeed.Y - 70f);
                        swap = true;
                    }
                }
                if (swap)
                {
                    SwapState(player);
                }
            }
            platform.Collidable = prev;
            UpdateColliders();
            Hold.CheckAgainstColliders();
        }
        private float noLiftSpeedTimer;
        private void OnPickup()
        {
            noLiftSpeedTimer = 0.3f;
            MoveStool = false;
            Hold.SlowRun = Raised;
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
            platform.AddTag(Tags.Persistent);
            SceneAs<Level>().Session.DoNotLoad.Add(id);
        }
        public void OnRelease(Vector2 force)
        {
            MoveStool = false;
            RemoveTag(Tags.Persistent);
            if (SceneAs<Level>().Session.DoNotLoad.Contains(id))
            {
                SceneAs<Level>().Session.DoNotLoad.Remove(id);
            }
            platform.RemoveTag(Tags.Persistent);
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
            }
            Speed = force * 200f;
            if (Speed != Vector2.Zero)
            {
                Hold.gravityTimer = 0.1f;
            }
        }
        public void SetState()
        {
            Visible = platform.Visible = Collidable = platform.Collidable = Hold.Visible = FlagState;
        }
        public Actor[] GetRiders()
        {
            List<Actor> riders = [];
            foreach (Actor entity in Scene.Tracker.GetEntities<Actor>())
            {
                if (entity is Glider glider)
                {
                    if (glider.BottomCenter.X >= Position.X && glider.BottomCenter.X <= Position.X + sprite.Width)
                    {
                        if (glider.BottomCenter.Y >= Position.Y && glider.BottomCenter.Y <= Position.Y + sprite.Height - 10)
                        {
                            riders.Add(glider);
                            continue;
                        }
                    }
                }
                if (entity.IsRiding(platform))
                {
                    riders.Add(entity);
                }
            }
            return [.. riders];
        }
        public void RemoveLeftoverDashes(Player player)
        {
            player.Dashes = Math.Max(0, player.Dashes - extraDashes);
        }
        private void GivePlayerDashes(Player player)
        {
            foreach (StoolListener listener in Scene.Tracker.GetComponents<StoolListener>())
            {
                if (listener.Active)
                {
                    listener.OnGiveDashes?.Invoke(this);
                }
            }
            if (player.Dashes < player.MaxDashes)
            {
                extraDashes = DashesHeld;
                player.Dashes = DashesHeld;
                player.RefillStamina();
                float dashes = DashesHeld;
                DashListener temporaryDashListener = new()
                {
                    OnDash = (dir) =>
                    {
                        dashes = (int)Calc.Max(dashes - 1, 0);
                    }
                };
                player.Add(temporaryDashListener);
                player.Add(new OnGroundAlarm(0.1f, 0.1f, null, delegate { RemoveLeftoverDashes(player); player.Remove(temporaryDashListener); }));
            }
            ConsumeDashes();
        }
        public void ConsumeDashes()
        {
            if (DashesHeld > 0)
            {
                flashTimer = 0.3f;
                DashesHeld = 0;
            }
        }
        private IEnumerator ChangeState(Player player)
        {
            bool rising = !Raised;
            foreach (StoolListener listener in Scene.Tracker.GetComponents<StoolListener>())
            {
                if (listener.Active)
                {
                    if (Raised)
                    {
                        listener.OnLowered?.Invoke(this);
                    }
                    else
                    {
                        listener.OnRaised?.Invoke(this);
                    }
                }
            }
            string name = rising ? "Rise" : "Lower";
            sprite.Play(name);
            int totalFrames = sprite.CurrentAnimationTotalFrames;
            Audio.Play("event:/PianoBoy/stool" + name, Position);
            if (rising)
            {
                foreach (StoolSnapComponent c in Scene.Tracker.GetComponents<StoolSnapComponent>())
                {
                    if (c.Entity != null && c.Collide(this))
                    {
                        c.Entity.Bottom = platform.Position.Y;
                    }
                }
                if (DashesHeld != 0)
                {
                    GivePlayerDashes(player);
                }
                //if entity is riding the platform, boost that entity into the air
                if (platform.HasRider())
                {
                    Actor[] rider = GetRiders();
                    foreach (Actor a in rider)
                    {
                        if (a is Glider g)
                        {
                            g.Speed.Y = -170;
                        }
                        else if (a is Stool stool)
                        {
                            Launched(stool);
                        }
                    }
                }
            }
            float platTargetOffset = rising ? raisedOffset : loweredOffset; //y offset from the stool's "raised" y position
            float from = PlatformYOffset;
            float prev;
            platform.LiftSpeedMult.Y = rising ? 3 : 1;
            for (float i = 0; i < 1; i += 0.3f)
            {
                prev = platform.Y;
                PlatformYOffset = Calc.LerpClamp(from, platTargetOffset, i);
                platform.MoveTo(PlatformPosition);
                if (player.IsRiding(platform))
                {
                    if (!rising)
                    {
                        player.MoveV(platform.Y - prev);
                    }
                }
                yield return null;
            }
            PlatformYOffset = platTargetOffset;
            platform.MoveTo(PlatformPosition);
            Raised = rising;
            platform.LiftSpeedMult.Y = rising ? 1 : 3;
            yield return null;
        }
        public void SwapState(Player player)
        {
            Hold.gravityTimer = 0.3f;
            gracePeriod = 0;
            coroutine.Replace(ChangeState(player));
            Audio.Play("event:/PianoBoy/stoolImpact", Position);
        }
        private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            Hold.cannotHoldTimer = 0.1f;
            SwapState(player);
            return DashCollisionResults.NormalCollision;
        }
        #region On Methods
        private void OnCollideV(CollisionData data)
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

            if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
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
        public void Swat(HoldableCollider hc, int dir)
        {
            if (Hold.IsHeld && hitSeeker == null)
            {
                swatTimer = 0.1f;
                hitSeeker = hc;
                Hold.Holder.Swat(dir);
            }
        }
        public void Launched(Stool stool)
        {
            stool.Speed.X *= 0.5f;
            stool.Speed.Y = -200f;
            stool.Hold.gravityTimer = 0.3f;

        }
        public bool HitSpring(Spring spring)
        {
            if (!Hold.IsHeld)
            {
                if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
                {
                    Speed.X *= 0.5f;
                    Speed.Y = -160f;
                    Hold.gravityTimer = 0.15f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = 220f;
                    Speed.Y = -80f;
                    Hold.gravityTimer = 0.1f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = -220f;
                    Speed.Y = -80f;
                    Hold.gravityTimer = 0.1f;
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
            if (!Hold.IsHeld)
            {
                Speed = (Center - seeker.Center).SafeNormalize(120f);
            }
            Audio.Play("event:/PianoBoy/stool_hit_side", Position);
        }
        public bool Dangerous(HoldableCollider holdableCollider)
        {
            if (!Hold.IsHeld && Speed != Vector2.Zero)
            {
                return hitSeeker != holdableCollider;
            }
            return false;
        }
        #endregion
        [Command("give_stool", "spawns a stool")]
        public static void GiveStool()
        {
            if (Engine.Scene is not Level level || level.GetPlayer() is not Player player) return;
            level.Add(new Stool(player.Position, "", false, new EntityID(Guid.NewGuid().ToString(), 0)));
        }
        [OnLoad]
        public static void Load()
        {
            On.Celeste.Refill.ctor_EntityData_Vector2 += OnRefillCtor;
        }
        private static void OnRefillCtor(On.Celeste.Refill.orig_ctor_EntityData_Vector2 orig, Refill self, EntityData data, Vector2 offset)
        {
            orig(self, data, offset);
            self.Add(new HoldableCollider((hold) =>
            {
                if (hold.Entity is Stool stool && self.Collidable && stool.Scene.GetPlayer() is Player player)
                {
                    stool.DashesHeld = self.twoDashes ? 3 : 2;
                    Audio.Play(self.twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", self.Position);
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    self.Collidable = false;
                    self.Add(new Coroutine(self.RefillRoutine(player)));
                    self.respawnTimer = 2.5f;
                }
            }));
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.Refill.ctor_EntityData_Vector2 -= OnRefillCtor;
        }

    }
}