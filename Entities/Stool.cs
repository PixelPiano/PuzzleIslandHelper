using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Components;
using System.Collections.Generic;
using System.Linq;

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
    public class StackComponent : Component
    {
        public List<Entity> Stack = [];
        public StackComponent() : base(true, true)
        {

        }
    }

    [CustomEntity("PuzzleIslandHelper/Stool")]
    [Tracked]
    public class Stool : Actor
    {

        public const float LaunchYSpeed = -200f;
        private static Vector2 StoolJustify = new Vector2(0.5f, 1f);
        private int extraDashes;
        public int DashesHeld;
        private float boostGraceTimer, noGravityTimer, swatTimer, playerBoostGracePeriod, noLiftSpeedTimer, deadTimer, launchedTimer, hardVerticalHitSoundCooldown;
        private float PlatformYOffset;
        private float loweredOffset => sprite.Height - 9;
        private float raisedOffset = 1;
        private float gravityMult = 1;
        public bool IsHeld => Hold.IsHeld;
        public bool JustLaunched => launchedTimer > 0;
        public bool Dead;
        public bool DeadRefillImmunity;

        public bool DisablePickupCollider;
        public bool InStack => (inHeldStack || inPrivateStack);
        public bool Raised;
        private bool inHeldStack, inPrivateStack;
        private Vector2 prevLiftSpeed, pickupColliderOrigPosition, spriteOffset;
        public Vector2 Speed;
        public Vector2 PlatformPosition => Position + PlatformOffset;
        public Vector2 PlatformOffset => spriteOffset + new Vector2(1, PlatformYOffset);
        public Vector2 StackOffset;
        private EntityID id;
        public FlagData Flag;
        private Color refillColor
        {
            get
            {
                return DashesHeld switch
                {
                    <= 0 => Color.White,
                    1 => Color.LimeGreen,
                    > 1 => Color.Red
                };
            }
        }
        private Coroutine coroutine;

        public StoolPlatform Platform;
        public Holdable Hold;
        public HoldableCollider hitSeeker;
        private Hitbox HoldingHitbox;
        private Collision onCollideV, onCollideH;
        public Sprite sprite;
        public Alarm DeadRefillImmunityAlarm;
        private List<Actor> boosted = [];
        public List<Stool> StoolsInStack = [];


        public List<Stool> Riders = [];
        public Vector2 PrevPosition;
        public Stool(Vector2 position, string flag, bool inverted, EntityID id) : base(position)
        {
            IgnoreJumpThrus = false;
            Depth = 1;
            this.id = id;
            Flag = new FlagData(flag, inverted);
            Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/stool/"));
            sprite.AddLoop("down", "stoolext", 0.1f, 0);
            sprite.AddLoop("up", "stoolext", 0.1f, 8);
            sprite.Add("Rise", "stoolext", 0.02f, "up");
            sprite.Add("Lower", "stoolLower", 0.05f, "down");
            sprite.Add("die", "die", 1f);
            sprite.Play("down");
            sprite.Justify = StoolJustify;
            sprite.JustifyOrigin(StoolJustify);

            spriteOffset = new Vector2(-sprite.Width * StoolJustify.X, -sprite.Height * StoolJustify.Y);
            Collider = new Hitbox(sprite.Width - 8, sprite.Height, spriteOffset.X, spriteOffset.Y);
            PlatformYOffset = loweredOffset;
            HoldingHitbox = new Hitbox(sprite.Width - 8, sprite.Height, spriteOffset.X + 2, spriteOffset.Y);
            Add(new VertexLight(Collider.Center, Color.White, 0.7f, 32, 64));
            Add(DeadRefillImmunityAlarm = Alarm.Create(Alarm.AlarmMode.Persist, delegate { DeadRefillImmunity = false; }, 0.6f));
            Add(Hold = new Holdable(0.1f)
            {
                SlowFall = false,
                SlowRun = false,
                OnPickup = onPickup,
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
                    if (!Raised && Platform.HasPlayerRider() && dir.X != 0)
                    {
                        playerBoostGracePeriod = 0.3f;
                    }
                }
            });
            Add(new StaticMover()
            {
                OnMove = f =>
                {
                    Position += f;
                    Platform.Position += f;
                },
                SolidChecker = s =>
                {
                    return inHeldStack;
                }
                
            });
            ColliderUpdate();
            Tag |= Tags.TransitionUpdate;
        }
        public Stool(EntityData data, Vector2 offset, EntityID id)
        : this(data.Position + offset, data.Attr("flag"), data.Bool("inverted"), id)
        {

        }
        public override void Update()
        {
            base.Update();
            Level level = Scene as Level;
            Player player = level.GetPlayer();
            if (Dead)
            {
                deadTimer -= Engine.DeltaTime;
                if (deadTimer <= 0)
                {
                    RemoveSelf();
                }
                return;
            }
            Visible = Platform.Visible = Collidable = Platform.Collidable = Hold.Visible = Flag.State;
            if (!Flag.State) return;
            annoying(); //this stuff is fine

            if (IsHeld)
            {
                prevLiftSpeed = Vector2.Zero;
            }
            else if (!InStack) //don't update speed stuff if in a stack
            {
                if (OnGround())
                {
                    OnGroundUpdate();
                }
                else if (Hold.ShouldHaveGravity)
                {
                    GravityUpdate();
                }
                Vector2 speed = Speed * new Vector2(1, gravityMult) * Engine.DeltaTime;
                MoveH(speed.X, onCollideH);
                MoveV(speed.Y, onCollideV);
                VanillaMoveChecks();
            }
            inPrivateStack = !Hold.IsHeld && IsRiding<StoolPlatform>();
            if (hitSeeker != null && swatTimer <= 0f && !hitSeeker.Check(Hold)) hitSeeker = null;

            Platform.NoLiftSpeed = noLiftSpeedTimer > 0 || inHeldStack;
            bool prev = Platform.Collidable;
            if (IsHeld && prev)
            {
                Platform.Collidable = Platform.Bottom < player.Top;
            }
            if (boostGraceTimer <= Engine.DeltaTime)
            {
                Platform.LiftSpeedMult.Y = 1;
            }
            HorizontalDashExtendCheck(player);
            Platform.Collidable = prev;

            if (!inHeldStack)
            {
                Platform.MoveTo(PlatformPosition);
                foreach (Stool s in StoolsInStack)
                {
                    s.Platform.MoveTo(s.PlatformPosition);
                }
            }
            annoying2(player);
        }
        public void OnGroundUpdate()
        {
            float target = !OnGround(Position + Vector2.UnitX * 3f) ? 20f : !OnGround(Position - Vector2.UnitX * 3f) ? -20f : 0;
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
        public void GravityUpdate()
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
            if (noGravityTimer <= 0)
            {
                Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
            }
        }
        private void onPickup()
        {
            RestartDeadRefillImmunityTimer();
            foreach (Stool stool in Scene.Tracker.GetEntities<Stool>())
            {
                if (stool.inHeldStack)
                {
                    stool.RestartDeadRefillImmunityTimer();
                }
            }
            recreateStack();
            Hold.SlowRun = Raised;
            Speed = Vector2.Zero;
        }
        public void OnRelease(Vector2 force)
        {
            StopDeadRefillImmunityTimer();
            foreach (Stool stool in StoolsInStack)
            {
                stool.StopDeadRefillImmunityTimer();
            }
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
            }
            Speed = force * 200f;
            releaseStack();
        }
        public void AddRidersToStack(Stool parent)
        {
            MakePersistent(SceneAs<Level>());
            if (this != parent)
            {
                parent.StoolsInStack.Add(this);
                StackOffset = Position - parent.Position;
            }
            foreach (Stool stackItem in Platform.GetRiders<Stool>())
            {
                if (!stackItem.inHeldStack)
                {
                    stackItem.inHeldStack = true;
                    stackItem.AddRidersToStack(parent);
                }
            }
        }
        private void recreateStack()
        {
            releaseStack();
            AddRidersToStack(this);
            StoolsInStack = [.. StoolsInStack.OrderByDescending(item => item.Y)];
            foreach (Stool s in StoolsInStack)
            {
                s.Collidable = false;
                s.Speed = Vector2.Zero;
                s.prevLiftSpeed = Vector2.Zero;
            }
        }
        private void releaseStack(bool snap = false)
        {
            MakeLocal(SceneAs<Level>());
            foreach (Stool s in StoolsInStack)
            {
                s.Speed = Vector2.Zero;
                s.prevLiftSpeed = Vector2.Zero;
                s.StackOffset = Vector2.Zero;
                s.MakeLocal(Scene as Level);
                s.inHeldStack = false;
            }
            foreach (Stool s in StoolsInStack)
            {
                if (s.CollideCheck<Solid>())
                {
                    while (s.CollideCheck<Solid>())
                    {
                        s.Position += Vector2.UnitY;
                    }
                }
            }
            StoolsInStack.Clear();
        }
        private void annoying()
        {
            swatTimer = Math.Max(0, swatTimer - Engine.DeltaTime);
            hardVerticalHitSoundCooldown = Math.Max(0, hardVerticalHitSoundCooldown - Engine.DeltaTime);
            float lerp = 0;
            if (Hold.cannotHoldTimer > 0f) lerp = 0.3f;
            sprite.Color = Color.Lerp(refillColor, Color.Black, lerp);
            if (noGravityTimer > 0f)
            {
                noGravityTimer -= Engine.DeltaTime;
                gravityMult = Calc.Approach(gravityMult, 0, 20 * Engine.DeltaTime);
            }
            else
            {
                gravityMult = Calc.Approach(gravityMult, 1, 5 * Engine.DeltaTime);
            }
        }
        private void annoying2(Player player)
        {
            noLiftSpeedTimer = Calc.Max(0, noLiftSpeedTimer - Engine.DeltaTime);
            ColliderUpdate();
            DisablePickupCollider = InStack && player.CenterY > Bottom;
            if (DisablePickupCollider)
            {
                Hold.cannotHoldTimer += Engine.DeltaTime;
            }
            Hold.CheckAgainstColliders();
            if (boostGraceTimer > 0)
            {
                updateBoosted();
                boostGraceTimer -= Engine.DeltaTime;
                if (boostGraceTimer <= 0)
                {
                    boosted.Clear();
                }
            }
            if (launchedTimer > 0)
            {
                launchedTimer -= Engine.DeltaTime;
            }
        }

        public override void DebugRender(Camera camera)
        {
            Collider?.Render(camera, Collidable ? Color.Red : Color.Black);
            if (Hold.PickupCollider != null)
            {
                Color color = Color.Pink;
                if (DisablePickupCollider)
                {
                    color *= 0.5f;
                }
                Collider collider = Collider;
                Collider = Hold.PickupCollider;
                Collider.Render(camera, color);
                Collider = collider;
            }
            /*
                        for (int i = 0; i < StoolsInStack.Count && i < 9; i++)
                        {
                            string path = "objects/PuzzleIslandHelper/stool/debugNum0" + i;
                            GFX.Game[path].DrawOutline(StoolsInStack[i].CenterLeft - Vector2.UnitX * 8);
                        }*/

        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Platform.RemoveSelf();
            (scene as Level).Session.DoNotLoad.Remove(id);
        }
        public void RestartDeadRefillImmunityTimer()
        {
            DeadRefillImmunity = true;
            DeadRefillImmunityAlarm.Start();
        }
        public void StopDeadRefillImmunityTimer()
        {
            DeadRefillImmunity = false;
            DeadRefillImmunityAlarm.Stop();
        }
        private void updateBoosted()
        {
            foreach (Actor a in Scene.Tracker.GetEntities<Actor>())
            {
                if (!boosted.Contains(a) && a.IsRiding(Platform))
                {
                    if (a is Stool stool)
                    {
                        noGravityTimer = 0;
                        Launch(stool);
                    }
                    else
                    {
                        a.LiftSpeed = new Vector2(a.LiftSpeed.X, -70f);
                    }
                    boosted.Add(a);
                }
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(Platform = new StoolPlatform(this, PlatformPosition, (int)sprite.Width - 2, false));
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Platform.OnDashCollide = onDashed;
            Visible = Platform.Visible = Collidable = Platform.Collidable = Hold.Visible = Flag.State;
        }
        public override bool IsRiding(JumpThru jumpThru)
        {
            return Platform != jumpThru && base.IsRiding(jumpThru);
        }
/*        public StoolPlatform GetRiding()
        {
            if (IgnoreJumpThrus)
            {
                return null;
            }
            foreach (Entity item in Scene.Tracker.Entities[typeof(StoolPlatform)])
            {
                if (!Collide.Check(this, item) && Collide.Check(this, item, at))
                {
                    return item as T;
                }
            }
        }*/
        public void VanillaMoveChecks()
        {
            Level level = Scene as Level;
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
        public void HorizontalDashExtendCheck(Player player)
        {
            if (!IsHeld && !Raised && !coroutine.Active)
            {
                bool swap = false;
                if (player.DashAttacking && player.DashDir.X != 0 && player.DashDir.Y == 0 && player.CollideCheck(Platform) && MathHelper.Distance(player.Bottom, Platform.Top) > 4)
                {
                    if (player.Bottom > Platform.Top)
                    {
                        player.MoveToY(Platform.Top);
                    }
                    swap = true;
                }
                if (playerBoostGracePeriod > 0)
                {
                    if (!player.IsRiding(Platform))
                    {
                        playerBoostGracePeriod -= Engine.DeltaTime;
                    }
                    if (Input.Jump.Pressed)
                    {
                        player.LiftSpeed = new Vector2(player.LiftSpeed.X, Math.Max(player.LiftSpeed.Y - 50f, -220));
                        swap = true;
                    }
                }
                if (swap)
                {
                    SwapState(player);
                }
            }
        }
        public void MakePersistent(Level level)
        {
            AddTag(Tags.Persistent);
            Platform?.AddTag(Tags.Persistent);
            if (!level.Session.DoNotLoad.Contains(id))
            {
                level.Session.DoNotLoad.Add(id);
            }
        }
        public void MakeLocal(Level level)
        {
            RemoveTag(Tags.Persistent);
            Platform?.RemoveTag(Tags.Persistent);
            level.Session.DoNotLoad.Remove(id);
        }
        private IEnumerator stateRoutine(Player player)
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
                if (DashesHeld != 0)
                {
                    givePlayerDashes(player);
                }
            }
            float platTargetOffset = rising ? raisedOffset : loweredOffset; //y offset from the stool's "raised" y position
            float from = PlatformYOffset;
            float prev;
            Platform.LiftSpeedMult.Y = rising ? 3 : 1;

            if (rising)
            {
                boostGraceTimer = 30 * Engine.DeltaTime;
                if (!OnGround() && !IsRiding<JumpThru>())
                {
                    noGravityTimer = 15 * Engine.DeltaTime;
                }
            }
            for (float i = 0; i < 1; i += 0.3f)
            {
                prev = Platform.Y;
                PlatformYOffset = Calc.LerpClamp(from, platTargetOffset, i);
                Platform.MoveTo(PlatformPosition);
                if (rising)
                {
                    updateBoosted();
                }
                else if (player.IsRiding(Platform))
                {
                    player.MoveV(Platform.Y - prev);
                }
                yield return null;
            }
            PlatformYOffset = platTargetOffset;
            Platform.MoveTo(PlatformPosition);
            Raised = rising;
            yield return null;
        }
        public void SwapState(Player player)
        {
            Hold.gravityTimer = 0.3f;
            playerBoostGracePeriod = 0;
            coroutine.Replace(stateRoutine(player));
            Audio.Play("event:/PianoBoy/stoolImpact", Position);
        }
        public void Die()
        {
            sprite.Play("die");
            Dead = true;
            deadTimer = 0.7f;
            Collidable = Platform.Collidable = false;
        }
        public void ColliderUpdate()
        {
            Collider p = Hold.PickupCollider;
            Collider c = Collider;
            Vector2 o = pickupColliderOrigPosition;
            c.Position.Y = PlatformOffset.Y;
            p.Position.Y = c.Position.Y + 2;
            c.Height = sprite.Height - PlatformYOffset;
            p.Height = c.Height - 2;
            p.Position.X = o.X;
            c.Position.X = p.Position.X + 4;
            c.Position.X = p.Position.X + 4;
        }
        public void RemoveLeftoverDashes(Player player)
        {
            player.Dashes = Math.Max(0, player.Dashes - extraDashes);
        }
        private void givePlayerDashes(Player player)
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
                player.Dashes = DashesHeld + 1;
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
                DashesHeld = 0;
            }
        }
        private DashCollisionResults onDashed(Player player, Vector2 direction)
        {
            Hold.cannotHoldTimer = 0.1f;
            Hold.gravityTimer = 0.1f;
            SwapState(player);
            return DashCollisionResults.NormalCollision;
        }
        private void OnCollideV(CollisionData data)
        {
            if (data.Hit is DashSwitch && !inHeldStack)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            if (Speed.Y > 0f && !inHeldStack)
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

/*            if (data.Hit is StoolPlatform p)
            {
                Stool stool = p.Stool;
                Speed.Y = 0;
                while (true)
                {
                    IsRiding
                }
            }*/
            if (Speed.Y > 140f && data.Hit is not (SwapBlock or DashSwitch or StoolPlatform))// && !(data.Hit is StoolPlatform sp && sp.Stool.StackParent != null && sp.Stool.StackParent.IsHeld))
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
            if (inHeldStack) return;
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }
            Audio.Play("event:/PianoBoy/stool_hit_side", Position);

            Speed.X *= -0.4f;
        }
        public void Swat(HoldableCollider hc, int dir)
        {
            if (IsHeld && hitSeeker == null)
            {
                swatTimer = 0.1f;
                hitSeeker = hc;
                Hold.Holder.Swat(dir);
            }
        }
        public void Launch(Stool stool)
        {
            stool.Speed.X *= 0.5f;
            stool.Speed.Y = LaunchYSpeed;
            stool.Hold.gravityTimer = 0.3f;
            stool.launchedTimer = 0.2f;
        }
        public bool HitSpring(Spring spring)
        {
            if (!IsHeld)
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
            /*            if (!IsHeld && Speed.Length() < 0.01f && base.LiftSpeed.Length() < 0.01f && (previousPosition - base.ExactPosition).Length() < 0.01f && OnGround())
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
            if (!IsHeld)
            {
                Speed = (Center - seeker.Center).SafeNormalize(120f);
            }
            Audio.Play("event:/PianoBoy/stool_hit_side", Position);
        }
        public bool Dangerous(HoldableCollider holdableCollider)
        {
            if (!IsHeld && Speed != Vector2.Zero)
            {
                return hitSeeker != holdableCollider;
            }
            return false;
        }

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
                int cancelMult = (NoLiftSpeed ? 0 : 1);
                if (Collidable)
                {
                    if (move < 0)
                    {
                        foreach (Actor entity in base.Scene.Tracker.GetEntities<Actor>())
                        {
                            int stackMult = entity is Stool && (entity as Stool).inHeldStack ? 0 : 1;
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
                                entity.LiftSpeed = LiftSpeed * LiftSpeedMult * cancelMult * stackMult;
                                Collidable = true;
                            }
                            else if (!entity.TreatNaive && CollideCheck(entity, Position + Vector2.UnitY * move) && !CollideCheck(entity))
                            {
                                Collidable = false;
                                entity.MoveVExact((int)(base.Top + (float)move - entity.Bottom));
                                entity.LiftSpeed = LiftSpeed * LiftSpeedMult * cancelMult * stackMult;
                                Collidable = true;
                            }
                        }
                    }
                    else
                    {
                        foreach (Actor entity2 in base.Scene.Tracker.GetEntities<Actor>())
                        {
                            int stackMult = entity2 is Stool && (entity2 as Stool).inHeldStack ? 0 : 1;
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

                                entity2.LiftSpeed = LiftSpeed * cancelMult * stackMult;
                                Collidable = true;
                            }
                        }
                    }
                }
                Y += move;
                MoveStaticMovers(Vector2.UnitY * move);
            }
            public List<T> GetRiders<T>() where T : Actor
            {
                List<T> list = [];
                foreach (T entity in base.Scene.Tracker.GetEntities<T>())
                {
                    if (entity.IsRiding(this))
                    {
                        list.Add(entity);
                    }
                }

                return list;
            }
            public List<T> GetRiders<T>(List<T> fromList) where T : Actor
            {
                List<T> list = [];
                foreach (T entity in fromList)
                {
                    if (entity.IsRiding(this))
                    {
                        list.Add(entity);
                    }
                }

                return list;
            }
        }

        [OnLoad]
        public static void Load()
        {
            On.Celeste.Refill.ctor_EntityData_Vector2 += OnRefillCtor;
            On.Celeste.Actor.OnGround_int += Actor_OnGround_int;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.Refill.ctor_EntityData_Vector2 -= OnRefillCtor;
            On.Celeste.Actor.OnGround_int -= Actor_OnGround_int;
        }
        private static void OnRefillCtor(On.Celeste.Refill.orig_ctor_EntityData_Vector2 orig, Refill self, EntityData data, Vector2 offset)
        {
            orig(self, data, offset);
            self.Add(new HoldableCollider((hold) =>
            {
                if (hold.Entity is Stool stool && self.Collidable && stool.Scene.GetPlayer() is Player player)
                {
                    if ((self.twoDashes && stool.DashesHeld < 2) || (!self.twoDashes && stool.DashesHeld < 1))
                    {
                        Audio.Play(self.twoDashes ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", self.Position);
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        stool.DashesHeld = self.twoDashes ? 2 : 1;
                        self.Collidable = false;
                        self.Add(new Coroutine(self.RefillRoutine(player)));
                        self.respawnTimer = 2.5f;
                    }
                }
            }));
        }
        private static bool Actor_OnGround_int(On.Celeste.Actor.orig_OnGround_int orig, Actor self, int downCheck)
        {
            if (self is Stool s)
            {
                if (!s.CollideCheck<Solid>(s.Position + Vector2.UnitY * downCheck))
                {
                    bool prev = s.Platform.Collidable;
                    s.Platform.Collidable = false;
                    bool result = false;
                    if (!s.IgnoreJumpThrus)
                    {
                        result = s.CollideCheckOutside<JumpThru>(s.Position + Vector2.UnitY * downCheck);
                    }
                    s.Platform.Collidable = prev;

                    return result;
                }

                return true;
            }
            return orig(self, downCheck);
        }


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
        [Command("give_stool", "spawns a stool")]
        public static Stool GiveStool(bool atPlayer = true, float? x = null, float? y = null)
        {
            Vector2 offset = Vector2.Zero;
            if (x.HasValue)
            {
                offset.X = x.Value;
            }
            if (y.HasValue)
            {
                offset.Y = y.Value;
            }
            if (Engine.Scene.GetPlayer() is Player player && atPlayer)
            {
                offset += player.Position;
            }
            return SpawnStool(offset, Engine.Scene);

        }
        public static Stool SpawnStool(Vector2 position, Scene scene, string flag = "", bool inverted = false)
        {
            Stool stool = new Stool(position, flag, inverted, new EntityID(Guid.NewGuid().ToString(), 0));
            scene.Add(stool);
            return stool;
        }
        public bool IsRiding<T>() where T : JumpThru
        {
            foreach (T jumpthru in Scene.Tracker.GetEntities<T>())
            {
                if (IsRiding(jumpthru)) return true;
            }
            return false;
        }
        [Command("give_stool_stack", "spawns a stack of stools")]
        public static List<Stool> GiveStoolStack(int stoolsInStack = 3)
        {
            List<Stool> list = [];
            Stool first = GiveStool();
            for (int i = 1; i < stoolsInStack; i++)
            {
                list.Add(GiveStool(true, null, -first.Height * i));
            }
            return list;
        }
    }
}