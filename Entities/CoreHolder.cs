using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static Celeste.Overworld;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public class InterfaceCore
    {
        public class CoreStartCutscene : CutsceneEntity
        {
            public CoreHolder Holder;
            public CoreOrb Orb;
            private bool orbStationary;
            private float lockTimer = 1;
            private bool dummyOrb;
            private Coroutine camMove, camZoom;
            private float rotationRateApproachMult;
            private float rotationRateApproachMultTarget = 1;
            private float rotationDir = 1;
            private Vector2 prevSpeed;
            public CoreStartCutscene(CoreHolder holder, CoreOrb orb) : base()
            {
                Holder = holder;
                Orb = orb;
                prevSpeed = Orb.Speed;
                lockTimer = 2f;
                Add(camMove = new(false));
                Add(camZoom = new(false));
            }
            public override void Update()
            {
                base.Update();
                if (dummyOrb)
                {
                    Orb.Holdable.gravityTimer = Engine.DeltaTime * 2;
                    if (!orbStationary)
                    {
                        float dist = Vector2.DistanceSquared(Holder.Position, Orb.Position);
                        if (dist > 1 || lockTimer > 0)
                        {
                            float angle = (Orb.Position - Holder.Position).Angle();
                            Vector2 offset = -Calc.AngleToVector(angle, Math.Max(dist, 20));
                            prevSpeed = Orb.Speed;
                            Orb.Speed = Calc.Approach(Orb.Speed, offset, 700f * Engine.DeltaTime);
                        }
                        else
                        {
                            Orb.Position = Holder.Position;
                            Orb.Speed = default;
                            orbStationary = true;
                        }
                    }
                    if (lockTimer > 0)
                    {
                        lockTimer -= Engine.DeltaTime;
                    }
                }

                Vector2 prevSign = prevSpeed.Sign();
                Vector2 currentSign = Orb.Speed.Sign();
                if (prevSign.X != currentSign.X && prevSign.Y != currentSign.Y)
                {
                    rotationDir *= -1;
                }
                float mult = Math.Max(1, (int)(Orb.Speed.LengthSquared() / 6));
                Orb.RotationRate = Calc.Approach(Orb.RotationRate, 30f * rotationDir * mult, Engine.DeltaTime * rotationRateApproachMult * mult);
                rotationRateApproachMult = Calc.Approach(rotationRateApproachMult, rotationRateApproachMultTarget, Engine.DeltaTime);
            }
            public IEnumerator Cutscene()
            {
                camMove.Replace(CameraTo(Holder.Position - new Vector2(160, 90), 1f, Ease.SineInOut));
                camZoom.Replace(Level.ZoomTo(new Vector2(160, 90), 2f, 1f));
                yield return 0.5f;
                dummyOrb = true;
                while (!orbStationary) yield return null;
                yield return 0.3f;
                rotationRateApproachMultTarget = 0;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    Orb.Sprite.Scale = Vector2.One * i;
                    yield return null;
                }
                Orb.RemoveSelf();
                //holderShake
                camZoom.Replace(Level.ZoomBack(1));
                //prongs display hologram
                //computer screen flickers on
                yield return null;
                yield return 1f;
                if (Scene.GetPlayer() is Player player)
                {
                    camMove.Replace(CameraTo(player.CameraTarget, 1f, Ease.SineInOut));
                }
                EndCutscene(Level);
            }
            public override void OnBegin(Level level)
            {
                if (level.GetPlayer() is Player player)
                {
                    player.StateMachine.State = Player.StDummy;
                }
                Add(new Coroutine(Cutscene()));
            }

            public override void OnEnd(Level level)
            {
                if (WasSkipped)
                {
                    camMove.Cancel();
                    camZoom.Cancel();
                    level.ResetZoom();
                    if (level.GetPlayer() is Player player)
                    {
                        level.Camera.Position = player.CameraTarget;
                        player.StateMachine.State = Player.StNormal;
                    }
                    Orb.RemoveSelf();
                    //hologram activate
                    //computer enabled
                }
            }
        }
        [CustomEntity("PuzzleIslandHelper/CoreHolder")]
        public class CoreHolder : Entity
        {
            public HoldableCollider HoldableCollider;
            public CoreHolder(EntityData data, Vector2 offset) : base(data.Position + offset)
            {
                Image image = new Image(GFX.Game["objects/PuzzleIslandHelper/interface/coreHolder"]);
                image.CenterOrigin();
                Add(image);
                Collider = new Hitbox(image.Width, image.Height, -image.Width / 2, -image.Height / 2);
                Add(HoldableCollider = new HoldableCollider(OnHoldableCollide));
            }
            public void OnHoldableCollide(Holdable holdable)
            {
                if (!holdable.IsHeld && holdable.Entity is CoreOrb orb)
                {
                    orb.SceneAs<Level>().Session.SetFlag("CoreReturned");
                    orb.InHolder = true;
                    orb.Collidable = false;
                    Scene.Add(new CoreStartCutscene(this, orb));
                    //start interface core cutscene
                }
            }
        }
        [CustomEntity("PuzzleIslandHelper/CoreOrb")]
        [Tracked]
        public class CoreOrb : Actor
        {
            public float RotationRate;
            public Vector2 Speed;
            private float hardVerticalHitSoundCooldown;
            private HoldableCollider hitSeeker;
            private float swatTimer;
            private float noGravityTimer;
            private Vector2 prevLiftSpeed;
            public bool InHolder;

            public Sprite Sprite;
            public Holdable Holdable;
            public EntityID ID;
            public float SpriteRate
            {
                get => Sprite.Rate;
                set => Sprite.Rate = value;
            }
            public CoreOrb(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
            {
                ID = id;
                Depth = 3;
                Tag |= Tags.TransitionUpdate | Tags.Persistent;
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                if ((scene as Level).Session.GetFlag("CoreReturned"))
                {
                    RemoveSelf();
                    return;
                }
                Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/");
                Sprite.AddLoop("idle", "coreOrb", 0.1f);
                Sprite.CenterOrigin();
                Sprite.Play("idle");
                SpriteRate = 0;
                Collider = new Hitbox(Sprite.Width, Sprite.Height, -Sprite.Width / 2, -Sprite.Height / 2);
                Tag |= Tags.TransitionUpdate | Tags.Persistent;
                Holdable = new Holdable();
                Add(Holdable);
                Holdable.PickupCollider = Collider;
                Holdable.SpeedSetter = (s) => Speed = s;
                Holdable.SpeedGetter = () => Speed;
                Holdable.SlowFall = false;
                Holdable.SlowRun = false;
                Holdable.OnPickup = OnPickup;
                Holdable.OnRelease = OnRelease;
                Holdable.DangerousCheck = Dangerous;
                Holdable.OnHitSeeker = HitSeeker;
                Holdable.OnSwat = Swat;
                Holdable.OnHitSpring = HitSpring;
                Holdable.OnHitSpinner = HitSpinner;
                LiftSpeedGraceTime = 0.1f;
            }
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
                if (Holdable.IsHeld && hitSeeker == null)
                {
                    swatTimer = 0.1f;
                    hitSeeker = hc;
                    Holdable.Holder.Swat(dir);
                }
            }
            public bool HitSpring(Spring spring)
            {
                if (!Holdable.IsHeld)
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
                if (!Holdable.IsHeld)
                {
                    Speed = (base.Center - seeker.Center).SafeNormalize(120f);
                }
                Audio.Play("event:/PianoBoy/stool_hit_side", Position);
            }
            public bool Dangerous(HoldableCollider holdableCollider)
            {
                return !Holdable.IsHeld && Speed != Vector2.Zero && hitSeeker != holdableCollider;
            }
            private void OnPickup()
            {
                Speed = Vector2.Zero;
                AddTag(Tags.Persistent);
                SceneAs<Level>().Session.DoNotLoad.Add(ID);
            }
            public void OnRelease(Vector2 force)
            {
                RemoveTag(Tags.Persistent);
                SceneAs<Level>().Session.DoNotLoad.Remove(ID);

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
            public override void Update()
            {
                base.Update();
                Sprite.Rotation = (Sprite.Rotation + RotationRate * Engine.DeltaTime) % MathHelper.TwoPi;
                Holdable.CheckAgainstColliders();
                if (!InHolder)
                {
                    if (Holdable.IsHeld)
                    {
                        SpriteRate = Calc.Approach(SpriteRate, 1, Engine.DeltaTime);
                    }
                    else
                    {
                        SpriteRate = Calc.Approach(SpriteRate, 0, Engine.DeltaTime);
                    }
                }

                #region Copied
                if (swatTimer > 0f)
                {
                    swatTimer -= Engine.DeltaTime;
                }
                hardVerticalHitSoundCooldown -= Engine.DeltaTime;
                if (Holdable.IsHeld)
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
                    else if (Holdable.ShouldHaveGravity)
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
                    MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
                    MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
                    Level l = SceneAs<Level>();
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
                if (hitSeeker != null && swatTimer <= 0f && !hitSeeker.Check(Holdable))
                {
                    hitSeeker = null;
                }
                #endregion
            }
        }
    }
}