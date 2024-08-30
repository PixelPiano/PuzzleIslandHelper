using Celeste.Mod.Core;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Looking = Celeste.Mod.PuzzleIslandHelper.Entities.Calidus.Looking;
using Mood = Celeste.Mod.PuzzleIslandHelper.Entities.Calidus.Mood;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [TrackedAs(typeof(Player))]
    public class PlayerCalidus : Player
    {
        public enum Upgrades
        {
            Nothing,
            Grounded,
            Slowed,
            Weakened,
            Eye,
            Head,
            Blip,
            Arms,
        }
        public const float AlphaDefault = 1;
        public const float EndFadeDefault = 64;
        public const float LightingDiff = 0.5f;
        public const float MaxFlyTime = 1.2f;
        public const float MaxRollRate = 15f;
        public const int NormalState = 0;
        public const int BlipState = 2;
        public const int DummyState = 11;
        public const int RailState = 26;
        public const int FlyState = 27;
        public const float StartFadeDefault = 32;
        public static readonly Dictionary<Upgrades, CalidusInventory> Inventories = new()
        {
            {Upgrades.Nothing,CalidusInventory.Nothing},
            {Upgrades.Grounded,CalidusInventory.Grounded},
            {Upgrades.Slowed,CalidusInventory.Slowed},
            {Upgrades.Weakened,CalidusInventory.Weakened},
            {Upgrades.Eye,CalidusInventory.EyeUpgrade},

            {Upgrades.Head,CalidusInventory.HeadUpgrade},
            {Upgrades.Arms, CalidusInventory.ArmUpgrade},
            {Upgrades.Blip,CalidusInventory.FullUpgrade},
        };
        public int Jumps;
        public int JumpsUsed;
        public float GravityMult = 1;
        private int chainedJumps;

        public int State => StateMachine.State;
        public float SpeedMult => Math.Max((1 * (1 - SlugMult)) - RoboInventory.Slowness, 0);
        public bool BlipEnabled => RoboInventory.Blip;
        public bool Blipping => State == BlipState;
        public bool CanSee => RoboInventory.CanSee;

        public bool CanBlip => BlipEnabled && Dashes > 0 && Input.CrouchDashPressed && dashCooldownTimer <= 0f && (TalkComponent.PlayerOver == null || !Input.Talk.Pressed);

        public bool CanJump => !Blipping && !FlyToggled && (onGround || jumpGraceTimer > 0) && RoboInventory.Jumps > 0 && Jumps > 0 && Input.Jump.Pressed;

        public bool FlyEnabled => RoboInventory.Fly;
        public bool Shaking => ForceShake || shakeTimer > 0;
        public bool Sluggish => State != DummyState && State != RailState && RoboInventory.Weak;
        public Collider SpriteBox => sprite.SpriteBox;
        public CalidusInventory RoboInventory => PianoModule.SaveData.CalidusInventory;


        private float chainedJumpTimer;
        private float shakeTimer;
        public float FlyTimer;
        public float LightingShiftAmount;
        public float NoRailTimer;
        public float SlugMult;
        public bool FlyToggled;
        public bool ForceShake;
        public bool InRail;
        public bool Moving;
        public bool StartedBlipping;
        public bool ValidatingBlip;
        public bool HasHead => RoboInventory.Sticky;
        private bool jumping;

        public Vector2 LastBlip;
        public Vector2 LastBlipDir;
        public Vector2 LastDashOrBlipDir;
        public Vector2 OrigPosition;
        public Vector2 SpriteOffset;
        private CalidusSpawnerData Data;
        public CalidusSprite sprite;
        public CalidusVision Vision;
        public CalidusGlow Glow;

        public PlayerCalidus(Vector2 position, CalidusSpawnerData data) : base(position, PlayerSpriteMode.Madeline)
        {
            Glow = new CalidusGlow(24);
            Add(Glow);
            sprite = new CalidusSprite(Vector2.Zero)
            {
                FloatHeight = 0,
                LookTargetEnabled = true,
                LookSpeed = 0.5f,
                LookDir = Looking.Center,
                CurrentMood = Mood.Normal,
            };
            Add(new Coroutine(SlogRoutine()));
            Depth = 0;
            Data = data;
            Tag |= Tags.Persistent;
            StateMachine.State = 0;
            TransitionListener t = new();
            t.OnInBegin = () =>
            {
                if (StateMachine.State == RailState)
                {
                    StateMachine.Locked = true;
                }
            };
            t.OnInEnd = () =>
            {
                StateMachine.Locked = false;
            };
            Add(t);
            Add(new BeforeRenderHook(BeforeRender));
            Light.Position = new Vector2(3);
        }
        public static bool LevelContainsSpawner(Level level, out CalidusSpawnerData data)
        {
            data = default;
            if (PianoMapDataProcessor.CalidusSpawners.ContainsKey(level.Session.Level))
            {
                data = PianoMapDataProcessor.CalidusSpawners[level.Session.Level];
                return true;
            }
            return false;
        }

        public static void SetInventory(Upgrades upgrade)
        {
            PianoModule.SaveData.CalidusInventory = Inventories[upgrade];
        }

        public static void SetInventory(CalidusInventory inv)
        {
            PianoModule.SaveData.CalidusInventory = inv;
        }

        public static void SetLighting(Level level, float amount)
        {
            level.Lighting.Alpha = level.BaseLightingAlpha + LightingDiff * amount;
            level.Session.LightingAlphaAdd = LightingDiff * amount;
        }

        public void absorbRender()
        {
            Vector2 prev = Position;
            Position = (SpriteBox.HalfSize - Vector2.One * 4 + Vector2.UnitY * SpriteBox.Height / 2).Floor();
            RenderParts();
            Position = prev;
        }

        public void AddBitrailAbsorb(BitrailNode node)
        {
            Visible = false;
            Vector2 pos = node.RenderPosition + Vector2.One * 4 - Vector2.One * 20;
            Collider c = new Hitbox(40, 40, pos.X, pos.Y);
            Vector2 p = c.HalfSize - Collider.HalfSize.Floor() - Vector2.One;
            Action action = new(() =>
            {
                RenderPartsAt(p);
            });
            Scene.Add(new BitrailAbsorb(action, c, Color.Green, Color.LightBlue, this));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(sprite);
            sprite.Fixed();
            Collider = new Hitbox(sprite.ColliderWidth, sprite.ColliderHeight);
            Position -= new Vector2(6, 4);
            sprite.AddRoutines(false);
            OrigPosition = Position;
            Vision = new CalidusVision(this);
            scene.Add(Vision);
        }

        public void ApproachScaleOne()
        {
            Vector2 newScale = sprite.Scale;
            newScale.X = Calc.Approach(newScale.X, 1f, 1.75f * Engine.DeltaTime);
            newScale.Y = Calc.Approach(newScale.Y, 1f, 1.75f * Engine.DeltaTime);
            SetScale(newScale);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (FlyEnabled) FlyToggled = true;
            Components.RemoveAll<PlayerSprite>();
            Components.RemoveAll<PlayerHair>();
            Components.RemoveAll<StateMachine>();
            Add(StateMachine = CreateStateMachine());
            Add(new Coroutine(GlowFlicker()));
            Collider = new Hitbox(7, 7, 0, 0);
            Glow.Position = -Vector2.One * Glow.Size / 2f + Collider.HalfSize;
        }
        public bool OnGround()
        {
            Vector2 check = Position + Vector2.UnitY * Math.Sign(GravityMult);
            if (!CollideCheck<Solid>(check))
            {
                if (GravityMult >= 0 && !IgnoreJumpThrus)
                {
                    return CollideCheckOutside<JumpThru>(Position + Vector2.UnitY);
                }
                return false;
            }
            return true;
        }
        public override void Update()
        {
            if (Scene is not Level level) return;
            SetBaseUpgradeLighting(level);
            moveX = Input.MoveX.Value;
            Glow.Floats = !CanSee && FlyEnabled && FlyToggled;
            if (moveX != 0)
            {
                Facing = (Facings)moveX;
            }
            if (!RoboInventory.Fly)
            {
                FlyToggled = false;
            }
            /*            if (noGravChangeTimer > 0)
                        {
                            noGravChangeTimer -= Engine.DeltaTime;
                        }
                        else
                        {

                            noGravChangeTimer = 0;
                            if (Input.Jump.Check && Speed.Y <= 0)
                            {
                                if (CollideCheck<Solid>(TopCenter - Vector2.UnitY))
                                {
                                    GravityMult = -1;
                                }
                            }
                        }*/



            onGround = OnGround();
            if (onGround)
            {
                RefillDash();
                RefillStamina();
                if (Math.Sign(Speed.Y) == Math.Sign(GravityMult))
                {
                    RefillJumps();
                }
            }
            int xDir = Math.Sign(Speed.X);
            if (chainedJumpTimer > 0 && !Blipping)
            {
                chainedJumpTimer -= Engine.DeltaTime;
            }
            if (onGround && CanSee)
            {
                JumpsUsed = 0;
                jumpGraceTimer = 0;
                if (jumping)
                {
                    chainedJumpTimer = (5f - Calc.Max(chainedJumps, 2)) * Engine.DeltaTime;
                }
                jumping = false;
                float mult = Calc.Clamp(Math.Abs(Speed.X), 0, 90f) / 90f;
                sprite.RollRotation += xDir * mult * MaxRollRate;
                sprite.RollRotation = (float)Math.Round(sprite.RollRotation) % 90;
                if (Visible && xDir != 0 && Scene.OnInterval(10f / (Blipping ? 2f : 1f) / 60f))
                {
                    Dust.Burst(BottomCenter - Vector2.UnitX * (xDir * 3), (Vector2.UnitX * -xDir).Angle(), 1, null);
                }
            }
            else if (wasOnGround && !jumping)
            {
                jumpGraceTimer = 0.1f;
            }
            if (jumpGraceTimer > 0)
            {
                jumpGraceTimer -= Engine.DeltaTime;
            }
            Depth = 0;
            JustRespawned = !(JustRespawned && Speed != Vector2.Zero);
            dashAttackTimer = Calc.Max(0, dashAttackTimer - Engine.DeltaTime);
            dashCooldownTimer = Calc.Max(0, dashCooldownTimer - Engine.DeltaTime);
            NoRailTimer = Calc.Max(0, NoRailTimer - Engine.DeltaTime);
            lastAim = Input.GetAimVector(Facing);

            Vector2 spriteOffset = SpriteOffset;
            if (Shaking)
            {
                shakeTimer -= Engine.DeltaTime;
                spriteOffset += Calc.Random.ShakeVector();
            }
            sprite.HasHead = HasHead;
            sprite.HasEye = CanSee;
            sprite.HasArms = BlipEnabled;

            if (!onGround || (onGround && Speed.X == 0))
            {
                sprite.RollRotation = Calc.Approach(sprite.RollRotation, 0, MaxRollRate * 2f);
            }
            if (!onGround)
            {
                GravityMult = Calc.Approach(GravityMult, 1, 250f * Engine.DeltaTime);
            }

            Position += spriteOffset;
            Components.Update();
            Position -= spriteOffset;
            ApproachScaleOne();

            if (State != DummyState)
            {
                if (Moving)
                {
                    sprite.LookDir = Looking.Target;
                    sprite.LookTarget = Center + Calc.SafeNormalize(new Vector2(Input.MoveX.Value, Input.MoveY.Value)) * sprite.OrbSprite.Width / 4f;
                }
                else
                {
                    sprite.LookDir = Looking.Center;
                }
            }
            sprite.UpdateEye(GetLookOffset());

            if (FlyEnabled && MInput.Keyboard.Pressed(Keys.F))
            {
                FlyToggled = !FlyToggled;
            }

            if (varJumpTimer > 0f)
            {
                varJumpTimer -= Engine.DeltaTime;
            }
            LiftSpeed = Vector2.Zero;
            if (liftSpeedTimer > 0f)
            {
                liftSpeedTimer -= Engine.DeltaTime;
                if (liftSpeedTimer <= 0f)
                {
                    lastLiftSpeed = Vector2.Zero;
                }
            }

            PreviousPosition = Position;
            if (StateMachine.State != RailState)
            {
                MoveH(Speed.X * SpeedMult * Engine.DeltaTime, newOnCollideH);
            }

            if (StateMachine.State != RailState)
            {
                MoveV(Speed.Y * Engine.DeltaTime, newOnCollideV);
            }
            if (FlyToggled)
            {
                Moving = Input.MoveX.Value != 0 || Input.MoveY.Value != 0;
            }
            else
            {
                Moving = Input.MoveX.Value != 0;
            }

            SpriteBox.Position = (Center - SpriteBox.HalfSize).Floor();

            foreach (Trigger entity in Scene.Tracker.GetEntities<Trigger>())
            {
                if (CollideCheck(entity))
                {
                    if (!entity.Triggered)
                    {
                        entity.Triggered = true;
                        triggersInside.Add(entity);
                        entity.OnEnter(this);
                    }

                    entity.OnStay(this);
                }
                else if (entity.Triggered)
                {
                    triggersInside.Remove(entity);
                    entity.Triggered = false;
                    entity.OnLeave(this);
                }
            }

            if (StateMachine.State != DummyState || ForceCameraUpdate)
            {
                Vector2 position = level.Camera.Position;
                Vector2 cameraTarget = CameraTarget;
                float num = ((StateMachine.State == 20) ? 8f : 1f);
                level.Camera.Position = position + (cameraTarget - position) * (1f - (float)Math.Pow(0.01f / num, Engine.DeltaTime));
            }

            if (StateMachine.State != DummyState && !Dead && EnforceLevelBounds)
            {
                level.EnforceBounds(this);
            }
            wasOnGround = onGround;

            if (level != null)
            {
                if (level.CanPause && framesAlive < int.MaxValue)
                {
                    framesAlive++;
                }

                if (framesAlive >= 8)
                {
                    diedInGBJ = 0;
                }
            }
            if (!Dead && State != RailState)
            {
                foreach (PlayerCollider component2 in base.Scene.Tracker.GetComponents<PlayerCollider>())
                {
                    if (component2.Check(this))
                    {
                        return;
                    }
                }
            }
        }

        public void BeforeRender()
        {
            Glow.BeforeRender();
        }

        public void blipBegin()
        {
            sprite.LookSpeed = 100;
            calledDashEvents = false;
            dashStartedOnGround = false;
            launched = false;
            canCurveDash = true;
            if (Engine.TimeRate > 0.25f)
            {
                Celeste.Freeze(0.05f);
            }
            dashCooldownTimer = 0.2f;
            dashRefillCooldownTimer = 0.1f;
            StartedDashing = true;
            wallSlideTimer = 1.2f;
            dashTrailTimer = 0f;
            dashTrailCounter = 0;
            if (!SaveData.Instance.Assists.DashAssist)
            {
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            }

            dashAttackTimer = 0.3f;
            gliderBoostTimer = 0.55f;
            if (SaveData.Instance.Assists.SuperDashing)
            {
                dashAttackTimer += 0.15f;
            }

            beforeDashSpeed = Speed;
            Speed = Vector2.Zero;
            DashDir = Vector2.Zero;
        }

        public IEnumerator BlipCoroutine()
        {
            yield return null;

            if (SaveData.Instance.Assists.DashAssist)
            {
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            }
            level.Displacement.AddBurst(Center, 0.4f, 8f, 64f, 0.5f, Ease.QuadOut, Ease.QuadOut);

            Vector2 value = lastAim;
            if (OverrideDashDirection.HasValue)
            {
                value = OverrideDashDirection.Value;
            }

            value = CorrectDashPrecision(value);
            Vector2 speed = value * 240f;
            if (Math.Sign(beforeDashSpeed.X) == Math.Sign(speed.X) && Math.Abs(beforeDashSpeed.X) > Math.Abs(speed.X))
            {
                speed.X = beforeDashSpeed.X;
            }

            Speed = speed;
            if (CollideCheck<Water>())
            {
                Speed *= 0.75f;
            }
            gliderBoostDir = (DashDir = value);
            SceneAs<Level>().DirectionalShake(DashDir, 0.2f);
            if (DashDir.X != 0f)
            {
                Facing = (Facings)Math.Sign(DashDir.X);
            }

            CallDashEvents();
            /*            if (BlipEnabled)
                        {*/

            //todo: Add cool custom burst effect here
            Visible = false;
            if (TryGetBlipTarget(Position, Speed * 16, out Vector2 target))
            {
                Vector2 prev = Position;
                if (target != prev)
                {
                    Speed = Vector2.Zero;
                }
                Position = target.Floor();
                yield return 0.15f;
                level.Displacement.AddBurst(Center, 0.4f, 8f, 64f, 0.5f, Ease.QuadOut, Ease.QuadOut);
                Visible = true;
            }
            else
            {
                yield return 0.15f;
                Visible = true;
            }
            /*}
                        else
                        {
                            SlashFx.Burst(Center, DashDir.Angle());
                            LastDashOrBlipDir = DashDir;
                            CreateTrail();
                            yield return 0.15f;
                            CreateTrail();
                            Speed /= 2f;
                        }*/
            StateMachine.State = NormalState;
        }

        public void blipEnd()
        {
            CallBlipEvents();
            demoDashed = false;
            sprite.LookSpeed = 0.5f;
        }

        public int blipUpdate()
        {
            StartedBlipping = false;
            if (dashTrailTimer > 0f)
            {
                dashTrailTimer -= Engine.DeltaTime;
                if (dashTrailTimer <= 0f)
                {
                    CreateTrail();
                    dashTrailCounter--;
                    if (dashTrailCounter > 0)
                    {
                        dashTrailTimer = 0.1f;
                    }
                }
            }
            if (CanJump)
            {
                CalidusJump();
                return NormalState;
            }
            if (SaveData.Instance.Assists.SuperDashing && CanBlip)
            {
                StartBlip();
                StateMachine.ForceState(BlipState);
                return BlipState;
            }
            //dash particles
            /*            if (Speed != Vector2.Zero && level.OnInterval(0.02f))
                        {
                            ParticleType type = P_DashA;
                            level.ParticlesFG.Emit(type, Center + Calc.Random.Range(Vector2.One * -2f, Vector2.One * 2f), DashDir.Angle());
                        }
            */
            return BlipState;
        }
        private float noGravChangeTimer;
        public override bool IsRiding(Solid solid)
        {
            if (State == RailState) return false;
            return base.IsRiding(solid);
        }
        public void CalidusJump(bool particles = true, bool playSfx = true)
        {

            bool fromFloor = GravityMult >= 0;
            sprite.RollRotation = 0;
            Jumps--;
            JumpsUsed++;
            jumping = true;
            Input.Jump.ConsumeBuffer();
            jumpGraceTimer = 0f;
            varJumpTimer = 0.2f;
            AutoJump = false;
            dashAttackTimer = 0f;
            gliderBoostTimer = 0f;
            wallSlideTimer = 1.2f;
            wallBoostTimer = 0f;
            if (fromFloor && CollideCheck<Solid>(Position - Vector2.UnitY))
            {
                GravityMult = -1;
            }
            chainedJumps = !FlyToggled && chainedJumpTimer > 0 && onGround && HasHead ? chainedJumps + 1 : 0;
            bool boosted = HasHead && chainedJumps > 1 && onGround;
            bool blipping = Blipping;
            float speedY = (boosted ? blipping ? -220 : -190 : -135) * RoboInventory.JumpMult;
            Speed.Y = speedY * GravityMult;
            Speed.X += 40f * moveX * (boosted && blipping ? 2.3f : 1);
            Vector2 lift = LiftBoost;
            Speed.X += lift.X;
            Speed.Y += lift.Y * GravityMult;
            varJumpSpeed = Speed.Y;
            if (!fromFloor)
            {
                GravityMult = 1;
                noGravChangeTimer = 0.2f;
            }
            if (boosted)
            {
                ShakeFor(0.2f);
                level.Add(Engine.Pooler.Create<SpeedRing>().Init(Center, -Vector2.UnitY.Angle(), Color.White));
                chainedJumps = -1;
            }
            if (playSfx)
            {
                if (launched)
                {
                    Play("event:/char/madeline/jump_assisted");
                }

                if (dreamJump)
                {
                    Play("event:/char/madeline/jump_dreamblock");
                }
                else
                {
                    Play("event:/char/madeline/jump");
                }
            }
            SetScale(new Vector2(0.6f - (boosted ? 0.2f : 0), 1.4f));
            if (particles)
            {
                if (onGround)
                {
                    int index = -1;
                    Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position + Vector2.UnitY, temp));
                    if (platformByPriority != null)
                    {
                        index = platformByPriority.GetLandSoundIndex(this);
                    }
                    ParticleType type = DustParticleFromSurfaceIndex(index);
                    int amount = (boosted ? 4 : 2) / (HasHead ? 1 : 2);
                    if (boosted)
                    {
                        float speedMin = type.SpeedMin;
                        float speedMax = type.SpeedMax;
                        type.SpeedMin *= 4;
                        type.SpeedMax *= 4;
                        Dust.Burst(BottomLeft, -165f.ToRad(), amount, type);
                        Dust.Burst(BottomRight, -15f.ToRad(), amount, type);
                        type.SpeedMin = speedMin;
                        type.SpeedMax = speedMax;
                    }
                    Dust.Burst(BottomCenter, -(float)Math.PI / 2f, amount, type);

                }
            }
            SaveData.Instance.TotalJumps++;
        }

        public void CallBlipEvents()
        {
            if (calledDashEvents)
            {
                return;
            }

            calledDashEvents = true;
            if (CurrentBooster == null)
            {
                SaveData.Instance.TotalDashes++;
                level.Session.Dashes++;
                Stats.Increment(Stat.DASHES);
                bool flag = DashDir.Y < 0f || (DashDir.Y == 0f && DashDir.X > 0f);
                if (DashDir == Vector2.Zero)
                {
                    flag = Facing == Facings.Right;
                }
                if (DashDir.X == 0 && DashDir.Y != 0)
                {
                    SetScale(new Vector2(0.6f, 1));
                }
                if (flag)
                {
                    //Play("event:/char/calidus/blip_right");
                }
                else
                {
                    //Play("event:/char/calidus/blip_left");
                }

                foreach (DashListener component in base.Scene.Tracker.GetComponents<DashListener>())
                {
                    if (component.OnDash != null)
                    {
                        component.OnDash(DashDir);
                    }
                }
            }
            else
            {
                CurrentBooster.PlayerBoosted(this, DashDir);
                CurrentBooster = null;
            }
        }

        public StateMachine CreateStateMachine()
        {
            StateMachine stateMachine = new StateMachine(27);
            stateMachine.SetCallbacks(NormalState, normalUpdate, null, normalBegin, normalEnd);
            stateMachine.SetCallbacks(DummyState, dummyUpdate, null, dummyBegin);
            stateMachine.SetCallbacks(RailState, railUpdate, null, railBegin, railEnd);
            stateMachine.SetCallbacks(BlipState, blipUpdate, BlipCoroutine, blipBegin, blipEnd);

            return stateMachine;
        }

        public override void DebugRender(Camera camera)
        {
            Draw.HollowRect(SpriteBox, Color.SlateBlue);
            Draw.HollowRect(Collider, Color.Red);
            Draw.HollowRect(LastBlip, 7, 7, Color.White);
            Draw.Point(Position + Light.Position, Color.Yellow);
            Draw.Point(Position, Color.Magenta);
        }

        public void dummyBegin()
        {
            DummyBegin();
        }

        public IEnumerator DummyMoveTo(float x, float speedMultiplier = 1)
        {
            if (StateMachine.State != DummyState)
            {
                StateMachine.State = DummyState;
            }
            DummyMoving = true;
            while (Math.Abs(x - X) > 4f && Scene != null && (!CollideCheck<Solid>(Position + Vector2.UnitX * Math.Sign(x - X))))
            {
                Speed.X = Calc.Approach(Speed.X, (float)Math.Sign(x - X) * 64f * speedMultiplier, 1000f * Engine.DeltaTime);
                yield return null;
            }
            DummyMoving = false;

        }

        public int dummyUpdate()
        {
            if (!DummyMoving)
            {
                if (Math.Abs(Speed.X) > 90f && DummyMaxspeed)
                {
                    Speed.X = Calc.Approach(Speed.X, 90f * (float)Math.Sign(Speed.X), 2500f * Engine.DeltaTime);
                }

                if (DummyFriction)
                {
                    Speed.X = Calc.Approach(Speed.X, 0f, 1000f * Engine.DeltaTime);
                }
            }
            return DummyState;
        }

        public void Emotion(Mood mood)
        {
            sprite.Emotion(mood);
        }

        public void Look(Looking dir)
        {
            sprite.Look(dir);
        }

        public void MidAirJumpEvents()
        {
            sprite.ThrowArmDown();
        }

        public PlayerDeadBody neworig_Die(Vector2 direction, bool evenIfInvincible = false, bool registerDeathInStats = true)
        {
            Session session = level.Session;
            bool flag = !evenIfInvincible && SaveData.Instance.Assists.Invincible;
            if (!Dead && !flag && StateMachine.State != 18)
            {
                Stop(wallSlideSfx);
                if (registerDeathInStats)
                {
                    session.Deaths++;
                    session.DeathsInCurrentLevel++;
                    SaveData.Instance.AddDeath(session.Area);
                }

                Strawberry goldenStrawb = null;
                foreach (Follower follower in Leader.Followers)
                {
                    if (follower.Entity is Strawberry && (follower.Entity as Strawberry).Golden && !(follower.Entity as Strawberry).Winged)
                    {
                        goldenStrawb = follower.Entity as Strawberry;
                    }
                }

                Dead = true;
                Leader.LoseFollowers();
                base.Depth = -1000000;
                Speed = Vector2.Zero;
                StateMachine.Locked = true;
                Collidable = false;
                Drop();
                if (LastBooster != null)
                {
                    LastBooster.PlayerDied();
                }

                level.InCutscene = false;
                level.Shake();
                Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
                PlayerDeadBody playerDeadBody = new PlayerDeadBody(this, direction);
                if (goldenStrawb != null)
                {
                    playerDeadBody.HasGolden = true;
                    playerDeadBody.DeathAction = delegate
                    {
                        Engine.Scene = new LevelExit(LevelExit.Mode.GoldenBerryRestart, session)
                        {
                            GoldenStrawberryEntryLevel = goldenStrawb.ID.Level
                        };
                    };
                }

                base.Scene.Add(playerDeadBody);
                base.Scene.Remove(this);
                base.Scene.Tracker.GetEntity<Lookout>()?.StopInteracting();
                return playerDeadBody;
            }

            return null;
        }

        public void normalBegin()
        {
        }

        public void normalEnd()
        {

        }

        public int normalUpdate()
        {
            if (CanBlip)
            {
                return StartBlip();
            }
            if (CanJump)
            {
                CalidusJump();
                return NormalState;
            }
            float maxMoveXMult = 0.5f;
            float maxMoveYMult = 0.5f;
            float speedXMult = 90f;
            float speedYMult = 90f;
            float xDir = Input.MoveX.Value;
            float yDir = 1;
            if (!FlyToggled)
            {
                speedYMult += 70f;
                speedXMult *= 0.9f;
                maxMoveYMult = 1.6f;
                maxMoveXMult = 1f;
            }
            else
            {
                yDir = Input.MoveY.Value;
            }


            float xmult = Math.Abs(Speed.X) > speedXMult && Math.Sign(Speed.X) == xDir ? 400f : 500f;
            float ymult = Math.Abs(Speed.Y) > speedYMult && Math.Sign(Speed.Y) == yDir ? 200f : 500f;
            Speed.X = Calc.Approach(Speed.X, speedXMult * xDir, xmult * maxMoveXMult * Engine.DeltaTime);
            Speed.Y = Calc.Approach(Speed.Y, speedYMult * yDir, ymult * maxMoveYMult * GravityMult * Engine.DeltaTime);
            if (varJumpTimer > 0f)
            {
                if (!FlyToggled && (AutoJump || Input.Jump.Check))
                {
                    Speed.Y = Math.Min(Speed.Y, varJumpSpeed);
                }
                else
                {
                    varJumpTimer = 0f;
                }
            }
            return NormalState;
        }

        public void railBegin()
        {
        }

        public void railEnd()
        {
        }

        public int railUpdate()
        {
            Speed = Vector2.Zero;
            return RailState;
        }

        public void RefillJumps()
        {
            Jumps = RoboInventory.Jumps;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Vision.RemoveSelf();
        }

        public override void Render()
        {
            if (!CanSee)
            {
                Glow.Render();
            }
            else if (StateMachine.State != RailState && !InRail)
            {
                RenderParts();
            }
        }

        public void RenderParts()
        {
            sprite.RenderAt(Position);
        }

        public void RenderPartsAt(Vector2 position)
        {
            sprite.RenderAt(position);
        }

        public void SetBaseUpgradeLighting(Level level)
        {
            Light.StartRadius = Calc.Approach(Light.StartRadius, 40, Engine.DeltaTime);
            Light.EndRadius = Calc.Approach(Light.EndRadius, 80, Engine.DeltaTime);
            if (!CanSee)
            {
                LightingShiftAmount = Calc.Approach(LightingShiftAmount, 1, Engine.DeltaTime);
            }
            else
            {
                LightingShiftAmount = Calc.Approach(LightingShiftAmount, 0, Engine.DeltaTime);
            }
            SetLighting(level, LightingShiftAmount);
            DigitalGrid.CalidusEyeDiff = LightingShiftAmount * 0.7f;
        }

        public void SetScale(Vector2 scale)
        {
            sprite.Scale = scale;
        }

        public IEnumerator SlogRoutine()
        {
            while (true)
            {
                SlugMult = 0;
                while (!Sluggish)
                {
                    yield return null;
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.5f)
                {
                    SlugMult = Calc.LerpClamp(0, 1, Ease.QuadIn(i));
                    yield return null;
                }
                SlugMult = 1;
                yield return 0.1f;

                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.8f)
                {
                    SlugMult = Calc.LerpClamp(1, 0, Ease.QuadIn(i));
                    yield return null;
                }
                while (!Moving)
                {
                    yield return null;
                }
                yield return 0.5f;
            }
        }

        public int StartBlip()
        {
            demoDashed = Input.CrouchDashPressed;
            Input.CrouchDash.ConsumeBuffer();
            sprite.LookSpeed = 5;
            if (!FlyToggled)
            {
                Dashes--;
            }
            return BlipState;

        }

        public bool TryGetBlipTarget(Vector2 from, Vector2 speed, out Vector2 target)
        {
            Vector2 dir = Calc.Sign(speed);
            if (speed == Vector2.Zero)
            {
                target = from;
                return true;
            }
            target = from + (speed * Engine.DeltaTime).Floor();
            float dist = Vector2.Distance(from, target);
            while (CollideCheck<Solid>(target) && dist > 0)
            {
                target += -dir;
                dist -= 1;
            }
            target.Floor();
            Level level = Scene as Level;
            LevelData data = level.Session.MapData.GetAt(target);
            LevelData thisData = level.Session.LevelData;
            if (dist > 0f && data != null &&
                (data.Equals(thisData) || (data.Spawns != null && data.Spawns.Count > 0
                && level.Tracker.GetEntity<BlipTransitionController>() == null)))
            {
                LastBlipDir = dir;
                LastDashOrBlipDir = dir;
                LastBlip = target;
                return true;
            }
            return false;
        }

        internal static void Load()
        {
            On.Celeste.Level.LoadNewPlayer += On_Level_LoadNewPlayer;
            On.Celeste.Level.LoadNewPlayerForLevel += On_Level_LoadNewPlayerForLevel;
            On.Celeste.Player.BeforeUpTransition += Player_BeforeUpTransition;
            On.Celeste.Player.BeforeDownTransition += Player_BeforeDownTransition;
            On.Celeste.Player.TransitionTo += Player_TransitionTo;
            On.Celeste.Player.Die += Player_Die;
        }

        internal static void Unload()
        {
            On.Celeste.Level.LoadNewPlayer -= On_Level_LoadNewPlayer;
            On.Celeste.Level.LoadNewPlayerForLevel -= On_Level_LoadNewPlayerForLevel;
            On.Celeste.Player.BeforeUpTransition -= Player_BeforeUpTransition;
            On.Celeste.Player.BeforeDownTransition -= Player_BeforeDownTransition;
        }

        private static Player On_Level_LoadNewPlayer(On.Celeste.Level.orig_LoadNewPlayer orig, Vector2 position, PlayerSpriteMode spriteMode)
        {
            if (Engine.Scene is Level level)
            {
                if (LevelContainsSpawner(level, out CalidusSpawnerData data))
                {
                    _ = orig(position, spriteMode);
                    return new PlayerCalidus(position, data);
                }
            }
            return orig(position, spriteMode);
        }

        private static Player On_Level_LoadNewPlayerForLevel(On.Celeste.Level.orig_LoadNewPlayerForLevel orig, Vector2 position, PlayerSpriteMode spriteMode, Level lvl)
        {
            if (LevelContainsSpawner(lvl, out CalidusSpawnerData data))
            {
                _ = orig(position, spriteMode, lvl);
                return new PlayerCalidus(position + new Vector2(2, -3), data);
            }
            return orig(position, spriteMode, lvl);
        }

        private static void Player_BeforeDownTransition(On.Celeste.Player.orig_BeforeDownTransition orig, Player self)
        {
            if (self is PlayerCalidus c)
            {
                if (!c.InRail) c.Speed.Y = Math.Max(0f, c.Speed.Y);
                foreach (Entity entity in c.Scene.Tracker.GetEntities<Platform>())
                {
                    if (entity is not SolidTiles && c.CollideCheckOutside(entity, c.Position + Vector2.UnitY * c.Height))
                    {
                        entity.Collidable = false;
                    }
                }
            }
            else
            {
                orig(self);
            }
        }

        private static void Player_BeforeUpTransition(On.Celeste.Player.orig_BeforeUpTransition orig, Player self)
        {
            if (self is PlayerCalidus c)
            {
                c.Speed.X = 0f;
                c.dashCooldownTimer = 0.2f;
            }
            else
            {
                orig(self);
            }
        }

        private static PlayerDeadBody Player_Die(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            if (self is PlayerCalidus pC)
            {
                Level level = self.Scene as Level;
                PlayerDeadBody playerDeadBody = pC.neworig_Die(direction, evenIfInvincible, registerDeathInStats);
                if (playerDeadBody != null)
                {
                    Everest.Events.Player.Die(self);
                    if (pC.framesAlive < 6 && level != null)
                    {
                        diedInGBJ++;
                        if (diedInGBJ != 0 && diedInGBJ % 2 == 0 && level.Session.Area.GetLevelSet() != "Celeste" && !CoreModule.Settings.DisableAntiSoftlock)
                        {
                            level.Pause();
                            return null;
                        }
                    }
                }
                return playerDeadBody;
            }
            else return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        private static bool Player_TransitionTo(On.Celeste.Player.orig_TransitionTo orig, Player self, Vector2 target, Vector2 direction)
        {
            if (self is PlayerCalidus c)
            {
                c.NoRailTimer = Engine.DeltaTime * 15;
                Rectangle rect = new Rectangle((int)c.LastBlip.X, (int)c.LastBlip.Y, 7, 7);
                Rectangle bounds = self.SceneAs<Level>().Bounds;
                if (c.Blipping)
                {
                    bool condition = false;
                    if (direction.Y > 0) condition = bounds.Top > rect.Top;
                    else if (direction.Y < 0) condition = bounds.Bottom < rect.Bottom;
                    else if (direction.X > 0) condition = bounds.Left > rect.Left;
                    else if (direction.X < 0) condition = bounds.Right < rect.Right;
                    if (condition)
                    {
                        if (direction.Y != 0) target.Y -= 4;
                        if (direction.X != 0) target.X -= 4;
                    }
                }
                else
                {
                    if (direction.Y != 0) target.Y -= 4;
                    if (direction.X != 0) target.X -= 4;
                }
                c.MoveTowardsX(target.X, 60f * Engine.DeltaTime);
                c.MoveTowardsY(target.Y, 60f * Engine.DeltaTime);
                if (c.Position == target)
                {
                    c.ZeroRemainderX();
                    c.ZeroRemainderY();
                    c.Speed.X = (int)Math.Round(c.Speed.X);
                    c.Speed.Y = (int)Math.Round(c.Speed.Y);
                    return true;
                }
                return false;
            }
            else
            {
                return orig(self, target, direction);
            }
        }

        private IEnumerator GlowFlicker()
        {
            float startFade = Light.StartRadius + 8;
            float endFade = Light.EndRadius + 16;
            float duration;
            float alpha = 1;
            while (true)
            {
                while (CanSee) yield return null;

                float stFrom = startFade;
                float endFrom = endFade;
                float alphaFrom = alpha;

                startFade = StartFadeDefault + Calc.Random.Range(-2, 2f);
                endFade = startFade * 2;
                duration = Calc.Random.Range(0.1f, 0.3f);
                alpha = 0.6f + (0.4f * MathHelper.Distance(startFade, StartFadeDefault) / 8f);
                for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
                {
                    float ease = Ease.SineIn(i);
                    Light.StartRadius = Calc.LerpClamp(stFrom, startFade, ease);
                    Light.EndRadius = Calc.LerpClamp(endFrom, endFade, ease);
                    Glow.Amplitude = Light.Alpha;
                    yield return null;
                }
            }
        }

        private void newOnCollideH(CollisionData data)
        {
            if (StateMachine.State == RailState) return;
            if (data.Hit != null && data.Hit.OnCollide != null)
            {
                data.Hit.OnCollide(data.Direction);
            }
            Speed.X = 0f;
        }

        private void newOnCollideV(CollisionData data)
        {
            if (StateMachine.State == RailState) return;
            if (Speed.Y < 0f)
            {
                int num3 = 4;

                if (Speed.X <= 0.01f)
                {
                    for (int j = 1; j <= num3; j++)
                    {
                        if (!CollideCheck<Solid>(Position + new Vector2(-j, -1f)))
                        {
                            Position += new Vector2(-j, -1f);
                            return;
                        }
                    }
                }

                if (Speed.X >= -0.01f)
                {
                    for (int k = 1; k <= num3; k++)
                    {
                        if (!CollideCheck<Solid>(Position + new Vector2(k, -1f)))
                        {
                            Position += new Vector2(k, -1f);
                            return;
                        }
                    }
                }
            }
            if (data.Hit != null && data.Hit.OnCollide != null)
            {
                data.Hit.OnCollide(data.Direction);
            }
            Speed.Y = 0f;
        }

        public struct CalidusInventory
        {
            public static readonly CalidusInventory Nothing = new CalidusInventory(canFly: true);
            public static readonly CalidusInventory Grounded = new CalidusInventory();
            public static readonly CalidusInventory Slowed = new CalidusInventory(slowAmount: 0.3f);
            public static readonly CalidusInventory Weakened = new CalidusInventory(slowAmount: 0.7f, weakened: true);
            public static readonly CalidusInventory EyeUpgrade = new CalidusInventory(canSee: true, jumps: 1, jumpMult: 0.2f);
            public static readonly CalidusInventory HeadUpgrade = new CalidusInventory(canSee: true, jumps: 1, jumpMult: 0.75f, canStick: true);
            public static readonly CalidusInventory ArmUpgrade = new CalidusInventory(canSee: true, canStick: true, jumps: 1, jumpMult: 0.75f, canBlip: true);
            public static readonly CalidusInventory FullUpgrade = new CalidusInventory(canSee: true, canStick: true, jumps: 2, jumpMult: 1f, canFly: true, canBlip: true);
            public bool Sticky;
            public bool Blip;
            public bool Fly;
            public bool CanSee;
            public float JumpMult;
            public int Jumps;
            public float Slowness;
            public bool Weak;
            public CalidusInventory(float slowAmount = 0, bool weakened = false, bool canSee = false, int jumps = 0, float jumpMult = 0, bool canStick = false, bool canFly = false, bool canBlip = false)
            {
                CanSee = canSee;
                Jumps = jumps;
                Sticky = canStick;
                Fly = canFly;
                Blip = canBlip;
                JumpMult = jumpMult;
                Slowness = slowAmount;
                Weak = weakened;
            }
        }

        [TrackedAs(typeof(ShaderOverlay))]
        public class CalidusVision : ShaderOverlay
        {
            public PlayerCalidus Calidus;
            public CalidusVision(PlayerCalidus calidus) : base("PuzzleIslandHelper/Shaders/CalidusVision")
            {
                Calidus = calidus;
                Tag |= Tags.Persistent | Tags.TransitionUpdate;
            }
            public override void ApplyParameters()
            {
                base.ApplyParameters();
                Effect.Parameters["PlayerCenter"]?.SetValue(Calidus.Center + Vector2.UnitY * Calidus.Glow.FloatOffset);
            }

            public override bool ShouldRender()
            {
                return base.ShouldRender() && (!Calidus.CanSee || Amplitude > 0);
            }

            public override void Update()
            {
                base.Update();
                if (Calidus.CanSee)
                {
                    Amplitude = Calc.Approach(Amplitude, 0, Engine.DeltaTime);
                }
                else
                {
                    Amplitude = 1;
                }
            }
        }
        #region Inherited from Calidus.cs
        public static Vector2 RotatePoint(Vector2 pointToRotate, Vector2 centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Vector2
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

        public void FloatTo(Vector2 position, float time, Ease.Easer ease = null)
        {
            Add(new Coroutine(FloatToRoutine(position, time, ease)));
        }

        public IEnumerator FloatToRoutine(Vector2 position, float time, Ease.Easer ease = null)
        {
            Vector2 from = Position;
            ease ??= Ease.Linear;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                Position = Vector2.Lerp(from, position, ease(i));
                yield return null;
            }
        }

        public void MoveTo(Vector2 position)
        {
            Position = position;
        }

        public IEnumerator Say(string id, string emotion, params Func<IEnumerator>[] events)
        {
            sprite.Emotion(emotion);
            yield return Textbox.Say(id, events);
        }

        public void ShakeFor(float time)
        {
            shakeTimer = time;
        }

        public void StartShaking()
        {
            ForceShake = true;
        }

        public void StopShaking()
        {
            ForceShake = false;
            shakeTimer = 0;
        }

        private Vector2 GetLookOffset()
        {
            return sprite.LookDir switch
            {
                Looking.Left => -Vector2.UnitX * 4,
                Looking.Right => Vector2.UnitX * 4,
                Looking.Up => -Vector2.UnitY * 4,
                Looking.Down => Vector2.UnitY * 4,
                Looking.UpLeft => Vector2.One * -4,
                Looking.UpRight => new Vector2(4, -4),
                Looking.DownLeft => new Vector2(-4, 4),
                Looking.DownRight => Vector2.One * 4,
                Looking.Target => RotatePoint(sprite.OrbSprite.Center.XComp(), Vector2.Zero, Calc.Angle(Center, sprite.LookTarget).ToDeg()),
                Looking.Center => Vector2.Zero,
                _ => Vector2.Zero
            };
        }
        #endregion
    }
}