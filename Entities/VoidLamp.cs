using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/VoidLamp")]
    [Tracked]
    public class VoidLamp : Actor
    {
        public CritterLight PlayerLight;
        private float holdTimer = 0;

        public Vector2 prevPosition = Vector2.Zero;

        public bool IsHeld = false;
        private EntityID id;

        private float noGravityTimer;
        private float swatTimer;
        private float hardVerticalHitSoundCooldown;

        private static Vector2 StoolJustify = new Vector2(0.5f, 1f);
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
        public VoidLamp(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            this.id = id;
            Tag |= Tags.TransitionUpdate;
            Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/voidLamp/"));
            sprite.AddLoop("idle", "wip", 0.1f);
            sprite.Play("idle");
            sprite.Justify = StoolJustify;
            sprite.JustifyOrigin(StoolJustify);
            Add(Hold = new Holdable(0.5f));


            Hold.PickupCollider = new Hitbox(sprite.Width, sprite.Height);
            Hold.PickupCollider.Position = new Vector2(-sprite.Width, -sprite.Height) * StoolJustify;
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
            TransitionListener l = new();
            Add(l);
            l.OnOut = f =>
            {
                if (Scene.GetPlayer() is Player player && player.Holding is Holdable holdable && holdable.Entity is VoidLamp lamp && lamp != this)
                {
                    Rectangle b = SceneAs<Level>().Camera.GetBounds();
                    b.X -= 8; b.Y -= 8; b.Width += 16; b.Height += 16;
                    Collider c = Collider;
                    if (!Collide.CheckRect(this, b))
                    {
                        RemoveSelf();
                    }
                }
            };
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (scene.GetPlayer() is Player player)
            {
                if (player.Holding is Holdable holdable && holdable.Entity is VoidLamp lamp && lamp != this)
                {
                    RemoveSelf();
                    return;
                }

                player.Add(PlayerLight = new CritterLight(SafeZone.Width / 1.5f, player.Light));
            }
        }
        private void OnPickup()
        {
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
            SceneAs<Level>().Session.DoNotLoad.Add(id);
        }
        public void OnRelease(Vector2 force)
        {
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
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if(scene.GetPlayer() is Player player)
            {
                player.Remove(PlayerLight);
            }
        }
        #endregion
        public override void Update()
        {
            base.Update();
            PlayerLight.Enabled = Hold.IsHeld;
            
            Hold.CheckAgainstColliders();
            SafeZone.Position = Hold.PickupCollider.Position + Hold.PickupCollider.HalfSize - Vector2.One * 20;
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;

            Mask.GradientBoost = PlayerLight.GradientBoost = 0.1f;
            Mask.GradientBoost = PlayerLight.GradientBoost += player.Dashes switch
            {
                1 => 0.1f,
                2 => 0.5f,
                _ => 0
            };
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