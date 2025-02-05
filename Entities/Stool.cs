using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using MonoMod.Utils;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class StoolMoverComponent : Component
    {
        public StoolMoverComponent() : base(true, true)
        {

        }
        public bool Collide(Stool stool)
        {
            if (Entity != null)
            {
                return Entity.CollideCheck(stool);
            }
            return false;
        }
    }
    [CustomEntity("PuzzleIslandHelper/Stool")]
    [Tracked]
    public class Stool : Actor
    {
        private float holdTimer = 0;
        private string flag;
        public Vector2 prevPosition = Vector2.Zero;
        public bool MoveStool = false;
        public bool IsHeld = false;
        private bool Inverted;
        private EntityID id;
        public bool FlagState => (string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag)) != Inverted;
        private static bool InRefill = false;
        private bool Raised = false;
        private bool inRoutine = false;

        private float noGravityTimer;
        private float swatTimer;
        private float StateAdjustment;
        private float hardVerticalHitSoundCooldown;
        private int DashesHeld = 0;

        private static Vector2 StoolJustify = new Vector2(0.5f, 1f);
        public Vector2 Speed;
        private Vector2 prevLiftSpeed;
        private Vector2 _platPos;

        private Color refillColor = Color.White;

        public Sprite sprite;
        public Holdable Hold;
        public HoldableCollider hitSeeker;
        private Hitbox HoldingHitbox;
        private JumpThru platform;
        public VertexLight Light;
        private Color orig_Color = Color.White;
        private Collision onCollideV;
        private Collision onCollideH;

        private Player player;

        private Coroutine routine;
        public Entity GetRider()
        {
            foreach (Stool entity in Scene.Tracker.GetEntities<Stool>())
            {
                if (entity.IsRiding(platform))
                {
                    return entity;
                }
            }
            foreach (Actor entity in Scene.Tracker.GetEntities<Actor>())
            {
                if (entity.IsRiding(platform))
                {
                    return entity;
                }
            }
            foreach (Glider glider in Scene.Tracker.GetEntities<Glider>())
            {
                if (glider.BottomCenter.X >= Position.X && glider.BottomCenter.X <= Position.X + sprite.Width)
                {
                    if (glider.BottomCenter.Y >= Position.Y && glider.BottomCenter.Y <= Position.Y + sprite.Height - 10)
                    {
                        return glider;
                    }
                }
                if (glider.IsRiding(platform))
                {
                    return glider;
                }
            }
            return null;
        }

        private void GivePlayerDashes(Player player)
        {
            if (player.Dashes < player.MaxDashes)
            {
                player.Dashes = DashesHeld;
                player.RefillStamina();
            }
            refillColor = Color.White;
            DashesHeld = 0;
        }

        public Stool(EntityData data, Vector2 offset, EntityID id)
        : base(data.Position + offset)
        {
            this.id = id;
            Inverted = data.Bool("inverted");
            flag = data.Attr("flag");
            Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/stool/"));
            sprite.AddLoop("down", "stoolext", 0.1f, 0);
            sprite.AddLoop("up", "stoolext", 0.1f, 8);
            sprite.Add("extUp", "stoolext", 0.03f, "up");
            sprite.Add("extDown", "stoolextDown", 0.05f, "down");
            sprite.Play("down");
            sprite.Justify = StoolJustify;
            sprite.JustifyOrigin(StoolJustify);
            Add(Hold = new Holdable(0.5f));
            Collider = new Hitbox(sprite.Width, sprite.Height, -sprite.Width * StoolJustify.X, -sprite.Height * StoolJustify.Y);
            Hold.PickupCollider = new Hitbox(sprite.Width, sprite.Height, -sprite.Width * StoolJustify.X, -sprite.Height * StoolJustify.Y);
            _platPos = Hold.PickupCollider.Position;
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
            HoldingHitbox = new Hitbox(sprite.Width - 8, sprite.Height, -sprite.Width * StoolJustify.X + 2, -sprite.Height * StoolJustify.Y);
            Collider = HoldingHitbox;
        }
        private void OnPickup()
        {
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
            holdTimer = 0.5f;
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
                noGravityTimer = 0.1f;
            }
        }
        private IEnumerator ChangeState(Player player)
        {
            inRoutine = true;
            float platTarget = Raised ? -2f : 2f;
            if (!Raised)
            {
                sprite.Play("extUp");
                Audio.Play("event:/PianoBoy/stoolRise", Position);
            }
            else
            {
                sprite.Play("extDown");
                Audio.Play("event:/PianoBoy/stoolLower", Position);

            }
            for (float i = 0; i < 1; i += 0.1f)
            {
                platform.MoveTowardsY(platTarget, i * platTarget);
                if (!Raised)
                {
                    foreach (StoolMoverComponent c in Scene.Tracker.GetComponents<StoolMoverComponent>())
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
                        Entity rider = GetRider();
                        if (rider as Glider is not null)
                        {
                            (rider as Glider).Speed.Y = -170;
                        }
                        else
                        {
                            if (rider as Stool is not null)
                            {
                                Launched(rider as Stool);
                            }
                        }
                    }
                    platform.LiftSpeed = Vector2.UnitY * 10;
                }
                yield return null;
            }
            Raised = !Raised;
            yield return null;
            inRoutine = false;
        }
        private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            if (!inRoutine)
            {
                Add(new Coroutine(ChangeState(player), true));
            }

            if (player.DashDir == new Vector2(0, 1))
            {
                Audio.Play("event:/PianoBoy/stoolImpact", Position);
            }
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
            stool.noGravityTimer = 0.3f;

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
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(platform = new JumpThru(Position - new Vector2(sprite.Width * StoolJustify.X, sprite.Height * StoolJustify.Y) + Vector2.UnitY * (sprite.Height - 10), (int)sprite.Width, false));
            //platform.OnDashCollide = OnDashed;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            platform.OnDashCollide = OnDashed;
            SetState();
        }
        public void SetState()
        {
            Visible = platform.Visible = Collidable = platform.Collidable = Hold.Visible = FlagState;
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
                if (hold.Entity is Stool stool)
                {
                    DynamicData dynData = DynamicData.For(self);
                    stool.routine = new Coroutine(dynData.Invoke<IEnumerator>("RefillRoutine", stool.player), true);
                    stool.routine.RemoveOnComplete = true;
                    stool.refillColor = dynData.Get<bool>("twoDashes") ? Color.Red : Color.LimeGreen;

                    if (self.Collidable)
                    {
                        InRefill = true;
                        dynData = DynamicData.For(self);
                        stool.DashesHeld = dynData.Get<bool>("twoDashes") ? 3 : 2;
                        Audio.Play(dynData.Get<bool>("twoDashes") ? "event:/new_content/game/10_farewell/pinkdiamond_touch" : "event:/game/general/diamond_touch", self.Position);
                        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                        self.Collidable = false;
                        self.Add(stool.routine);
                        dynData.Set("respawnTimer", 2.5f);
                    }
                }
            }));
        }

        [OnUnload]
        public static void Unload()
        {
            On.Celeste.Refill.ctor_EntityData_Vector2 -= OnRefillCtor;
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level) return;
            SetState();
            if (!FlagState) return;
            Hold.CheckAgainstColliders();
            sprite.SetColor(refillColor);
            orig_Color = refillColor;
            if (holdTimer > 0f)
            {
                sprite.SetColor(Color.Lerp(orig_Color, Color.Black, 0.3f));
                holdTimer -= Engine.DeltaTime;
            }
            else
            {
                sprite.SetColor(orig_Color);
            }
            Collider.Position = Hold.PickupCollider.Position = Raised ? _platPos : _platPos + Vector2.UnitY * 10;
            Collider.Position.X = Hold.PickupCollider.Position.X + 4;
            Collider.Height = Hold.PickupCollider.Height = Raised ? sprite.Height : 10;
            player = Scene.Tracker.GetEntity<Player>();
            StateAdjustment = Raised ? 0 : sprite.Height - 10;

            if (!inRoutine)
            {
                platform.Position = Position - new Vector2(sprite.Width * StoolJustify.X, sprite.Height * StoolJustify.Y) + Vector2.UnitY * StateAdjustment;
            }
            if (player is null || Scene as Level is null)
            {
                return;
            }

            Depth = player.Depth + 1;
            level = Scene as Level;
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