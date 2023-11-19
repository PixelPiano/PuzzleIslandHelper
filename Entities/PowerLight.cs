using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using MonoMod.Utils;
using System.Security.Cryptography.X509Certificates;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/PowerLight")]
    [Tracked]
    public class PowerLight : Actor
    {
        private float holdTimer = 0;
        public Vector2 prevPosition = Vector2.Zero;

        private EntityID id;

        private float noGravityTimer;
        private float swatTimer;
        private float hardVerticalHitSoundCooldown;

        public Vector2 Speed;
        private Vector2 prevLiftSpeed;
        public Image Texture;
        public Holdable Hold;
        public HoldableCollider hitSeeker;
        public VertexLight Light;
        private Collision onCollideV;
        private Collision onCollideH;
        public bool DialogueOnHitGround;
        public bool SaidFunnyLightDialogue;
        public bool InAir;
        private Level l;

        public PowerLight(EntityData data, Vector2 offset, EntityID id)
        : base(data.Position + offset)
        {
            this.id = id;
            Vector2 justify = new Vector2(0.5f, 1);
            Add(Texture = new Image(GFX.Game["objects/PuzzleIslandHelper/decisionMachine/missingLight"]));
            Add(Hold = new Holdable(0.5f));
            Texture.JustifyOrigin(justify);
            Collider = new Hitbox(Texture.Width - 1, Texture.Height, -justify.X * Texture.Width, -justify.Y * Texture.Height);
            Hold.PickupCollider = new Hitbox(Collider.Width, Collider.Height, Collider.Left, Collider.Top);
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
            Depth = 1;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);

        }
        public override void Render()
        {
            Texture.DrawSimpleOutline();
            base.Render();
        }
        private void OnPickup()
        {
            InAir = false;
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
            SceneAs<Level>().Session.DoNotLoad.Add(id);
        }
        public void OnRelease(Vector2 force)
        {
            holdTimer = 0.5f;
            InAir = true;
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
        private IEnumerator LightThrowCutscene()
        {
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            player.StateMachine.State = Player.StDummy;
            yield return Textbox.Say("LightFail");
            player.StateMachine.State = Player.StNormal;
            yield return null;
        }

        #region On Methods
        private void OnCollideV(CollisionData data)
        {
            InAir = false;
            if (DialogueOnHitGround && !SaidFunnyLightDialogue)
            {
                Add(new Coroutine(LightThrowCutscene()));
                SaidFunnyLightDialogue = true;
            }
            if (data.Hit is DashSwitch)
            {
                (data.Hit as DashSwitch).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }
            if (Speed.Y > 0f)
            {
                if (hardVerticalHitSoundCooldown <= 0f)
                {
                    Audio.Play("event:/PianoBoy/stool_hit_ground", Position, "crystal_velocity", Calc.ClampedMap(Speed.Y, 0f, 200f)); //todo change sounds
                    hardVerticalHitSoundCooldown = 0.5f;
                }
                else
                {
                    Audio.Play("event:/PianoBoy/stool_hit_ground", Position, "crystal_velocity", 0f); //todo change sound
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
            Audio.Play("event:/PianoBoy/stool_hit_side", Position); //todo change sound
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

        public void Launched(PowerLight powerLight)
        {
            powerLight.Speed.X *= 0.5f;
            powerLight.Speed.Y = -200f;
            powerLight.noGravityTimer = 0.3f;

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
                Speed = (base.Center - seeker.Center).SafeNormalize(120f);
            }
            Audio.Play("event:/PianoBoy/stool_hit_side", Position);
        }
        public bool Dangerous(HoldableCollider holdableCollider)
        {
            return !Hold.IsHeld && Speed != Vector2.Zero && hitSeeker != holdableCollider;
        }
        #endregion

        public override void Update()
        {
            base.Update();
            Hold.CheckAgainstColliders();
            if (holdTimer > 0f)
            {
                holdTimer -= Engine.DeltaTime;
            }

            l = Scene as Level;
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
                    float target = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
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
                if (base.Center.X > (float)l.Bounds.Right)
                {
                    MoveH(32f * Engine.DeltaTime);
                    if (base.Left - 8f > (float)l.Bounds.Right)
                    {
                        RemoveSelf();
                    }
                }
                else if (base.Left < (float)l.Bounds.Left)
                {
                    base.Left = l.Bounds.Left;
                    Speed.X *= -0.4f;
                }
                else if (base.Top < (float)(l.Bounds.Top - 4))
                {
                    base.Top = l.Bounds.Top + 4;
                    Speed.Y = 0f;
                }
                else if (base.Bottom > (float)l.Bounds.Bottom && SaveData.Instance.Assists.Invincible)
                {
                    base.Bottom = l.Bounds.Bottom;
                    Speed.Y = -300f;
                    Audio.Play("event:/game/general/assist_screenbottom", Position);
                }
                if (X < (l.Bounds.Left + 10))
                {
                    MoveH(32f * Engine.DeltaTime);
                }
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                TempleGate templeGate = CollideFirst<TempleGate>();
                if (templeGate != null && entity != null)
                {
                    templeGate.Collidable = false;
                    MoveH((float)(Math.Sign(entity.X - base.X) * 32) * Engine.DeltaTime);
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