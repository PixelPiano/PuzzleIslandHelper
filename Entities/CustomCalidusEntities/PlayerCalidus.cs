using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities.CalidusDeadBody;
using Looking = Celeste.Mod.PuzzleIslandHelper.Entities.Calidus.Looking;
using Mood = Celeste.Mod.PuzzleIslandHelper.Entities.Calidus.Mood;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities
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
            Vision,
            Jumping,
            Sticky,
            Rail,
            Blip,
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
            {Upgrades.Vision,CalidusInventory.Vision},

            {Upgrades.Jumping,CalidusInventory.HigherJump},
            {Upgrades.Sticky, CalidusInventory.StickyUpgrade},
            {Upgrades.Rail, CalidusInventory.RailUpgrade},
            {Upgrades.Blip,CalidusInventory.FullUpgrade},
        };
        public int Jumps;
        public int JumpsUsed;
        public float GravityMult = 1;
        private int chainedJumps;
        private float noGravityTimer;
        public int State
        {
            get
            {
                return StateMachine.State;
            }
            set
            {
                StateMachine.State = value;
            }
        }
        public float SpeedMult => Math.Max(1 * (1 - SlugMult) - RoboInventory.Slowness, 0);
        public bool HasBlip => RoboInventory.CanBlip;
        public bool Blipping => State == BlipState;
        public bool CanSee => RoboInventory.CanSee;
        public float BlipDist
        {
            get
            {
                int dist = 0;
                if (HasHead) dist += 8;
                if (HasEye) dist += 16;
                if (HasArms) dist += 8;
                if (HasBlip) dist += 16;
                return dist;
            }
        }
        public VirtualButton Zoomies;
        public bool CanBlip => HasBlip && Dashes > 0 && Input.CrouchDashPressed && dashCooldownTimer <= 0f && (TalkComponent.PlayerOver == null || !Input.Talk.Pressed);

        public bool CanZip => ZipSpeedMult == MaxZipMult && onGround && (TalkComponent.PlayerOver == null || !Input.Talk.Pressed);

        public bool CanJump => !Blipping && !FlyToggled && (onGround || jumpGraceTimer > 0) && RoboInventory.Jumps > 0 && Jumps > 0 && Input.Jump.Pressed;

        public bool FlyEnabled => RoboInventory.CanFly;
        public bool Shaking => ForceShake || shakeTimer > 0;
        public bool Sluggish => State != DummyState && State != RailState && RoboInventory.Weak;
        public Collider SpriteBox => sprite.SpriteBox;
        public CalidusInventory RoboInventory => PianoModule.SaveData.CalidusInventory;

        public const float MaxZipMult = 1.1f;
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
        public bool HasHead => RoboInventory.HasHead;
        public bool HasEye => RoboInventory.HasEye;
        public bool HasArms => RoboInventory.HasArms;
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

        private float railWindowTimer;
        public const float RailWindowTime = 0.8f;
        public float RailWindowAmount;
        public bool AvailableForRail;
        public bool JumpReleasedAfterJumping;
        public bool StickyPressedWhileJumping;
        public bool Sticky => RoboInventory.Sticky;
        public Hitbox HurtBox;
        public Hitbox RegularHitbox;
        public Hitbox GravCheckBox;
        public Hitbox AboveBox;
        private float gravSwitchTimer;
        private bool emitGravParticles;
        private Coroutine EyeFlashCoroutine;

        public PianoModuleSettings.StickyHoldMode HoldMode = PianoModule.Settings.ToggleSticky;


        private bool gravityToggled;
        public bool DashEnabled;
        public bool CanNormalDash => DashEnabled && Dashes > 0 && Input.DashPressed && dashCooldownTimer <= 0f && (TalkComponent.PlayerOver == null || !Input.Talk.Pressed);

        public ParticleType P_GravDetect = new()
        {
            Color = Color.White * 0.9f,
            ScaleOut = true,
            Size = 1,
            SpeedMin = 15f,
            SpeedMax = 15f,
            LifeMin = 0.3f,
            LifeMax = 0.3f,
            RotationMode = ParticleType.RotationModes.SameAsDirection,
            FadeMode = ParticleType.FadeModes.Late,
            Source = GFX.Game[CalidusSprite.PartPath + "gravSwitchParticle"],
        };

        public float PreZipTimer;
        public float ZipSpeedMult;
        public float JumpMult => RoboInventory.BaseJumpMult;// + (Boosts * BoostAmount);
        public float BoostAmount = 0.05f;
        public int Boosts;
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
            Add(new Coroutine(SlugRoutine()));
            Depth = 0;
            Data = data;
            Tag |= Tags.Persistent;
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
                if (StateMachine.State == RailState)
                {
                    StateMachine.Locked = false;
                }
            };
            Add(t);
            Add(new BeforeRenderHook(BeforeRender));
            Add(EyeFlashCoroutine = new Coroutine(false));
            Light.Position = new Vector2(3);
        }
        public void newPostCtor()
        {
            StateMachine.SetStateName(0, "Normal");
            StateMachine.SetStateName(2, "Blip");
            StateMachine.SetStateName(11, "Dummy");
            StateMachine.SetStateName(26, "Bitrail");
            StateMachine.SetStateName(14, "IntroRespawn");
            //Everest.Events.Player.RegisterStates(this);
        }
        public void EmitGravParticle(Vector2 dir)
        {

            /*          float angle = dir.Angle();
                      P_GravDetect.Direction = angle;
                      Vector2 offset = RotatePoint(Vector2.UnitX, Vector2.Zero, angle) * 3;
                      level.ParticlesFG.Emit(P_GravDetect, Center + offset);
                      P_GravDetect.Direction = (float)(Math.PI + angle);
                      level.ParticlesFG.Emit(P_GravDetect, Center - offset);*/
            float prev = P_GravDetect.Direction;
            P_GravDetect.Direction = 0;
            Vector2 offset = Vector2.UnitX * 3;
            level.ParticlesFG.Emit(P_GravDetect, Center + offset);
            P_GravDetect.Direction = (float)Math.PI;
            level.ParticlesFG.Emit(P_GravDetect, Center - offset - Vector2.UnitX);
            P_GravDetect.Direction = prev;

        }
        private IEnumerator GravParticleRoutine(Vector2 dir)
        {
            for (int i = 0; i < 3; i++)
            {
                EmitGravParticle(dir);
                yield return 3f * Engine.DeltaTime;
            }
        }
        public IEnumerator EyeFlashRoutine()
        {
            float from = sprite.EyeSprite.FlashAlpha;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
            {
                sprite.EyeSprite.FlashAlpha = Calc.LerpClamp(from, 1f, Ease.CubeOut(i));
                yield return null;
            }
            from = sprite.EyeSprite.FlashAlpha;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.3f)
            {
                sprite.EyeSprite.FlashAlpha = Calc.LerpClamp(from, 0, Ease.SineIn(i));
                yield return null;
            }
            sprite.EyeSprite.FlashAlpha = 0;
        }

        public override void Added(Scene scene)
        {
            Add(sprite);
            sprite.Fixed();
            Position -= new Vector2(6, 4);
            sprite.AddRoutines(false);
            OrigPosition = Position;
            Vision = new CalidusVision(this);
            scene.Add(Vision);
            if (FlyEnabled) FlyToggled = true;
            Components.RemoveAll<StateMachine>();
            Add(StateMachine = CreateStateMachine());
            newPostCtor();
            Add(new Coroutine(GlowFlicker()));
            RegularHitbox = new Hitbox(7, 7, 0, 0);
            GravCheckBox = new Hitbox(7, 16, 0, -16);
            HurtBox = new Hitbox(5, 5, 1, 0);
            AboveBox = new Hitbox(7, 7, 0, -7);
            Collider = RegularHitbox;
            glowPosition = Glow.Position = -Vector2.One * Glow.Size / 2f + Collider.HalfSize;
            base.Added(scene);
        }
        private float jumpGrace => jumpGraceTimer;
        private Vector2 glowPosition;
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Components.RemoveAll<PlayerSprite>();
            Components.RemoveAll<PlayerHair>();
        }

        public Vector2 DebugVec;
        private int lastGravity;
        private bool useAntiGravJumpGrace;

        private float zoomRollRate;
        public bool ZoomRolling;
        public Facings facing;
        private float zoomRollAmount;
        public override void Update()
        {
            if (Scene is not Level level || Dead) return;


            SetBaseUpgradeLighting(level);
            moveX = Input.MoveX.Value;
            Glow.Floats = !CanSee && FlyEnabled && FlyToggled;
            if (moveX != 0)
            {
                facing = Facing = (Facings)moveX;
            }
            if (!RoboInventory.CanFly)
            {
                FlyToggled = false;
            }
            if (Input.CrouchDash.Check)
            {
                zoomRollRate = 0;
                ZoomRolling = false;
            }
            if (Zoomies != null && Zoomies.Check)
            {
                if (zoomRollRate < 0.2f)
                {
                    zoomRollRate = Calc.Approach(zoomRollRate, 0.2f, Engine.DeltaTime / 2f);
                }
                else
                {
                    ZoomRolling = true;
                }
            }
            else
            {
                ZoomRolling = false;
                zoomRollRate = Calc.Approach(zoomRollRate, 0, Engine.DeltaTime);
            }
            zoomRollAmount = zoomRollRate / 0.2f;
            sprite.ZoomieAlpha = zoomRollAmount;
            if (Input.Dash.Check)
            {
                railWindowTimer = Calc.Approach(railWindowTimer, RailWindowTime, Engine.DeltaTime);
            }
            else
            {
                railWindowTimer = Calc.Max(railWindowTimer - Engine.DeltaTime, 0);
            }
            RailWindowAmount = railWindowTimer / RailWindowTime;
            AvailableForRail = RoboInventory.CanUseRails && Input.Dash.Check && !InRail && State != RailState;

            onGround = OnGround();
            noGravityTimer = Calc.Max(noGravityTimer - Engine.DeltaTime, 0);
            if (onGround)
            {
                noGravityTimer = 0;
                useAntiGravJumpGrace = GravityMult < 0;
                noGravChangeTimer = 0;
                StickyPressedWhileJumping = false;
                RefillDash();
                RefillStamina();
                RefillJumps();
            }
            int xDir = Math.Sign(Speed.X);
            if (chainedJumpTimer > 0 && !Blipping)
            {
                chainedJumpTimer -= Engine.DeltaTime;
            }
            sprite.RollRotation += zoomRollRate;
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
                    Vector2 pos = GravityMult < 0 ? TopCenter : BottomCenter;
                    Dust.Burst(pos - Vector2.UnitX * (xDir * 3), (Vector2.UnitX * -xDir).Angle(), 1, null);
                }
            }
            else if (wasOnGround && !jumping)
            {
                //jumpGraceTimer = useAntiGravJumpGrace ? 0.15f : 0.1f;
                jumpGraceTimer = 0.1f;
            }
            if (jumpGraceTimer > 0)
            {
                jumpGraceTimer -= Engine.DeltaTime;
            }

            if (Input.Grab.Pressed)
            {
                if (jumping)
                {
                    StickyPressedWhileJumping = true;
                }
            }
            Depth = 0;
            JustRespawned = !(JustRespawned && Speed != Vector2.Zero);
            dashAttackTimer = Calc.Max(0, dashAttackTimer - Engine.DeltaTime);
            dashCooldownTimer = Calc.Max(0, dashCooldownTimer - Engine.DeltaTime);
            NoRailTimer = Calc.Max(0, NoRailTimer - Engine.DeltaTime);
            lastAim = Input.GetAimVector(facing);

            Vector2 spriteOffset = SpriteOffset;
            if (Shaking)
            {
                shakeTimer -= Engine.DeltaTime;
                spriteOffset += Calc.Random.ShakeVector();
            }
            sprite.HasHead = HasHead;
            sprite.HasEye = HasEye;
            sprite.HasArms = HasArms;

            if (!onGround || onGround && Speed.X == 0)
            {
                sprite.RollRotation = Calc.Approach(sprite.RollRotation, 0, MaxRollRate * 2f);
            }
            if (!FlyToggled && Sticky)
            {
                if (noGravChangeTimer > 0)
                {
                    noGravChangeTimer -= Engine.DeltaTime;
                }
                else
                {
                    noGravChangeTimer = 0;
                    if (!onGround)
                    {
                        Collider c = Collider;
                        Collider = GravCheckBox;
                        if (GravityMult < 0)
                        {
                            if (!CollideCheck<Solid>())
                            {
                                Speed.Y = 0;
                                ChangeGravity(1);
                            }
                            else
                            {
                                Speed.Y = -180f;
                            }
                        }
                        else
                        {
                            Collider = AboveBox;
                            if (Speed.Y <= 0 && CollideCheck<Solid>() && (StickyPressedWhileJumping || Input.Grab.Check))
                            {
                                ChangeGravity(-1);
                            }
                        }
                        Collider = c;
                    }
                }
            }
            Position += spriteOffset;
            Components.Update();
            Position -= spriteOffset;
            ApproachScaleOne();

            if (State != DummyState)
            {
                sprite.LookDir = Looking.Target;
                sprite.LookTarget = Center + new Vector2(Moving ? Input.MoveX.Value : 0, Input.MoveY.Value).SafeNormalize() * sprite.OrbSprite.Width / 4f;
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
                MoveH(Speed.X * (SpeedMult + ZipSpeedMult) * Engine.DeltaTime, newOnCollideH);
            }

            if (StateMachine.State != RailState)
            {
                MoveV(Speed.Y * Engine.DeltaTime, newOnCollideV);
            }
            /*            if (Input.Grab.Check && Math.Abs(Speed.X) > 30)
                        {
                            PreZipTimer += Engine.DeltaTime;
                        }
                        else
                        {
                            ZipSpeedMult = 0;
                            PreZipTimer = 0;
                        }
                        if (PreZipTimer > 1.2)
                        {
                            //ZipSpeedMult = Calc.Approach(ZipSpeedMult, MaxZipMult, Engine.DeltaTime / 2f);
                        }
                        else
                        {
                            ZipSpeedMult = 0;
                        }*/


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
                float num = StateMachine.State == 20 ? 8f : 1f;
                level.Camera.Position = position + (cameraTarget - position) * (1f - (float)Math.Pow(0.01f / num, Engine.DeltaTime));
            }

            if (StateMachine.State != DummyState && !Dead && EnforceLevelBounds)
            {
                level.EnforceBounds(this);
            }
            wasOnGround = onGround;

/*            if (level != null)
            {
                if (level.CanPause && framesAlive < int.MaxValue)
                {
                    framesAlive++;
                }

                if (framesAlive >= 8)
                {
                    diedInGBJ = 0;
                }
            }*/
            if (!Dead && State != RailState)
            {
                Collider collider = Collider;
                Collider = HurtBox;
                foreach (PlayerCollider component2 in Scene.Tracker.GetComponents<PlayerCollider>())
                {
                    if (component2.Check(this) && Dead)
                    {
                        Collider = collider;
                        return;
                    }
                }
                if (Collider == HurtBox)
                {
                    Collider = collider;
                }
            }
        }
        public void FlashEye(int yDir)
        {
            sprite.EyeSprite.FlashTexture = GFX.Game[CalidusSprite.PartPath + (yDir > 0 ? "downFlash" : "upFlash")];
            EyeFlashCoroutine.Replace(EyeFlashRoutine());
        }
        public void ChangeGravity(int yDir)
        {
            gravityToggled = yDir < 0;
            lastGravity = Math.Sign(GravityMult);
            GravityMult = yDir;
            FlashEye(yDir);
        }
        public static bool LevelContainsSpawner(Level level, out CalidusSpawnerData data)
        {
            return PianoMapDataProcessor.CalidusSpawners[level.GetAreaKey()].TryGetValue(level.Session.Level, out data);
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
        public BitrailAbsorb Absorb;
        public void AddBitrailAbsorb(BitrailNode node)
        {
            Visible = false;
            Vector2 pos = Position + Vector2.One * 4 - Vector2.One * 20;
            Collider c = new Hitbox(40, 40, pos.X, pos.Y);
            Vector2 p = c.HalfSize - Collider.HalfSize.Floor() - Vector2.One;
            Action action = new(() =>
            {
                RenderPartsAt(p);
            });
            if (Absorb != null && !Absorb.Finished)
            {
                Absorb.RemoveSelf();
            }
            Scene.Add(Absorb = new BitrailAbsorb(action, c, Color.Green, Color.LightBlue, this));
        }

        public void ApproachScaleOne()
        {
            Vector2 newScale = sprite.Scale;
            newScale.X = Calc.Approach(newScale.X, 1f, 1.75f * Engine.DeltaTime);
            newScale.Y = Calc.Approach(newScale.Y, 1f, 1.75f * Engine.DeltaTime);
            SetScale(newScale);
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
            if (CrouchDashed && Math.Sign(beforeDashSpeed.X) == Math.Sign(speed.X) && Math.Abs(beforeDashSpeed.X) > Math.Abs(speed.X))
            {
                speed.X = beforeDashSpeed.X;
            }

            Speed = speed;
            if (CollideCheck<Water>())
            {
                Speed *= 0.75f;
            }
            gliderBoostDir = DashDir = value;
            level.DirectionalShake(DashDir, 0.2f);
            /*            if (DashDir.X != 0f)
                        {
                            Facing = (Facings)Math.Sign(DashDir.X);
                        }*/
            CallDashEvents();
            if (CrouchDashed)
            {
                //todo: Add cool custom burst effect here
                Visible = false;
                if (!CollideCheck<DisableCalidusBlip>() && TryGetBlipTarget(Position, Speed * 16, out Vector2 target))
                {
                    AddAfterImage();
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
            }
            else
            {
                SlashFx.Burst(Center, DashDir.Angle());
                LastDashOrBlipDir = DashDir;
                CreateTrail();
                yield return 0.15f;
                CreateTrail();
                Speed /= 2f;
            }
            CrouchDashed = false;
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
            bool canStick = Sticky;
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
            if (canStick && fromFloor && CollideCheck<Solid>(Position - Vector2.UnitY))
            {
                ChangeGravity(-1);
            }
            //chainedJumps = !FlyToggled && chainedJumpTimer > 0 && onGround && HasHead ? chainedJumps + 1 : 0;
            bool boosted = false;//HasHead && chainedJumps > 1 && onGround;
            bool blipping = Blipping;
            float speedY = boosted ? blipping ? -220 : -190 : -135;

            float mult = GravityMult * JumpMult;
            Speed.Y = speedY * mult + Math.Sign(mult) * 8f;
            if (!fromFloor)
            {
                Speed.Y += 30f;
            }
            Speed.X += 38f * moveX * (boosted && blipping ? 2.3f : 1);
            Speed.Y += LiftBoost.Y;
            varJumpSpeed = Speed.Y;
            if (canStick && !fromFloor)
            {
                ChangeGravity(1);
                noGravChangeTimer = 0.2f;
            }
            chainedJumps = -1;
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
                    if (GravityMult >= 0)
                    {
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
                    else
                    {
                        Dust.Burst(TopCenter, -(float)Math.PI / 2f * 3f, amount, type);
                    }
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
                bool flag = DashDir.Y < 0f || DashDir.Y == 0f && DashDir.X > 0f;
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

                foreach (DashListener component in Scene.Tracker.GetComponents<DashListener>())
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
            stateMachine.SetCallbacks(14, null, null, newIntroRespawnBegin, newIntroRespawnEnd);
            return stateMachine;
        }
        public void newIntroRespawnBegin()
        {
            Play("event:/char/madeline/revive");
            base.Depth = -1000000;
            introEase = 1f;
            Vector2 from = Position;
            from.X = MathHelper.Clamp(from.X, (float)level.Bounds.Left + 40f, (float)level.Bounds.Right - 40f);
            from.Y = MathHelper.Clamp(from.Y, (float)level.Bounds.Top + 40f, (float)level.Bounds.Bottom - 40f);
            deadOffset = from;
            from -= Position;
            respawnTween = Tween.Create(Tween.TweenMode.Oneshot, null, 0.6f, start: true);
            respawnTween.OnUpdate = delegate (Tween t)
            {
                deadOffset = Vector2.Lerp(from, Vector2.Zero, t.Eased);
                introEase = 1f - t.Eased;
            };
            respawnTween.OnComplete = delegate
            {
                if (StateMachine.State == 14)
                {
                    StateMachine.State = 0;
                    sprite.Scale = new Vector2(1.5f, 0.5f);
                }
            };
            Add(respawnTween);
        }
        public void newIntroRespawnEnd()
        {
            base.Depth = 0;
            deadOffset = Vector2.Zero;
            Remove(respawnTween);
            respawnTween = null;
        }

        public override void DebugRender(Camera camera)
        {
            Draw.HollowRect(SpriteBox, Color.SlateBlue);
            Draw.HollowRect(Collider, Color.Red);
            Collider prev = Collider;
            Collider = HurtBox;
            Draw.HollowRect(Collider, Color.Lime);
            if (!FlyToggled && Sticky && noGravChangeTimer <= 0 && !onGround)
            {
                Collider = GravityMult < 0 ? GravCheckBox : AboveBox;
                Draw.HollowRect(Collider, Color.Blue);
            }
            Collider = prev;
            //Draw.HollowRect(DebugVec, 8, 8, debugBool ? Color.Lime : Color.Red);
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
            while (Math.Abs(x - X) > 4f && Scene != null && !CollideCheck<Solid>(Position + Vector2.UnitX * Math.Sign(x - X)))
            {
                Speed.X = Calc.Approach(Speed.X, Math.Sign(x - X) * 64f * speedMultiplier, 1000f * Engine.DeltaTime);
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
                    Speed.X = Calc.Approach(Speed.X, 90f * Math.Sign(Speed.X), 2500f * Engine.DeltaTime);
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

        public CalidusDeadBody neworig_Die(Vector2 direction, bool evenIfInvincible = false, bool registerDeathInStats = true)
        {
            Session session = level.Session;
            bool flag = !evenIfInvincible && SaveData.Instance.Assists.Invincible;
            if (!Dead && !flag && StateMachine.State != 18)
            {
                //Stop(wallSlideSfx);
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
                Depth = -100000;
                Speed = Vector2.Zero;
                StateMachine.Locked = true;
                Collidable = false;

                level.InCutscene = false;
                level.Shake();
                Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
                CalidusDeadBody playerDeadBody = new CalidusDeadBody(this, direction, RemoveSelf);
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

                Scene.Add(playerDeadBody);
                //sprite.RemoveSelf();
                //Scene.Remove(this);
                Scene.Tracker.GetEntity<Lookout>()?.StopInteracting();
                return playerDeadBody;
            }

            return null;
        }
        public void AddAfterImage()
        {
            Vector2 padding = Vector2.One * 4;
            AfterImage image = new AfterImage(sprite.SpriteBox.Position - padding, sprite.SpriteBox.Width + padding.X * 2, sprite.SpriteBox.Height + padding.Y * 2, delegate { sprite.RenderAt((Position - sprite.SpriteBox.Position + padding).Floor()); });
            Scene.Add(image);
        }
        public void normalBegin()
        {
        }

        public void normalEnd()
        {

        }
        public float LastValidXAim;
        public Vector2 SwooshPoint;
        public Vector2 SwooshCheck;
        public Vector2 SwooshTarget;
        public void TrySwoosh()
        {
            float aim = LastValidXAim;
            Vector2 check = new Vector2(Center.X - Center.X % 8, Center.Y - Center.Y % 8) - Vector2.One * 8;
            SwooshPoint = check;
            SwooshCheck = check + new Vector2(aim * 8, Math.Sign(GravityMult) * 8);
            if (!CollideCheck<Solid>(SwooshCheck))
            {
                for (int i = 1; i < 4; i++)
                {
                    float dist = aim * 8 * i;
                    if (CollideCheck<Solid>(SwooshCheck + Vector2.UnitX * dist))
                    {
                        Vector2 target = SwooshPoint + Vector2.UnitX * (dist + aim * 8);
                        if (!CollideCheck<Solid>(new Vector2(target.X, Position.Y)))
                        {
                            Input.Grab.ConsumeBuffer();
                            AddAfterImage();
                            SwooshTarget = target;
                            MoveToX(SwooshTarget.X, newOnCollideH);
                        }
                        break;
                    }
                }
            }
        }
        public const float FlyMoveMult = 0.4f;
        public int normalUpdate()
        {
            if (GravityMult < 0 && CollideFirst<FallingBlock>(Position - Vector2.UnitY) is FallingBlock block && !block.Triggered)
            {
                block.Triggered = true;
            }
            if (Input.MoveX.Value != 0)
            {
                LastValidXAim = Input.MoveX.Value;
            }
            if (FlyToggled)
            {
                GravityMult = 1;
            }
            if (CanBlip)
            {
                return StartBlip();
            }
            if (CanZip)
            {
                TrySwoosh();
                return NormalState;
            }
            if (CanJump)
            {
                CalidusJump();
                return NormalState;
            }
            float yDir = FlyToggled ? Input.MoveY.Value : 1;
            float num2 = onGround ? 1f : 0.75f;
            if (onGround && level.CoreMode == Session.CoreModes.Cold)
            {
                num2 *= 0.3f;
            }
            if (SaveData.Instance.Assists.LowFriction && lowFrictionStopTimer <= 0f)
            {
                num2 *= onGround ? 0.35f : 0.5f;
            }
            if (FlyToggled)
            {
                num2 *= FlyMoveMult;
            }

            float num3 = 90f;

            if (level.InSpace)
            {
                num3 *= 0.6f;
            }
            int zoomDir = (int)LastValidXAim;
            if (zoomRollRate > 0)
            {
                float num8 = 3 * zoomRollAmount;
                if (!ZoomRolling)
                {
                    num8 *= 0.3f;
                }
                Speed.X = Calc.Approach(Speed.X, num3 * num8 * zoomDir, 400f * num2 * Engine.DeltaTime);
            }
            else if (Math.Abs(Speed.X) > num3 && Math.Sign(Speed.X) == moveX)
            {
                Speed.X = Calc.Approach(Speed.X, num3 * moveX, 400f * num2 * Engine.DeltaTime);
            }
            else
            {
                Speed.X = Calc.Approach(Speed.X, num3 * moveX, 1000f * num2 * Engine.DeltaTime);
            }

            float num4 = 160f;
            float num5 = 240f;
            if (level.InSpace)
            {
                num4 *= 0.6f;
                num5 *= 0.6f;
            }
            if (!FlyToggled)
            {
                if ((float)Input.MoveY == 1f && Speed.Y >= num4)
                {
                    maxFall = Calc.Approach(maxFall, num5, 300f * Engine.DeltaTime);
                    float num6 = num4 + (num5 - num4) * 0.5f;
                    if (Speed.Y >= num6)
                    {
                        float amount = Math.Min(1f, (Speed.Y - num6) / (num5 - num6));
                        sprite.Scale.X = MathHelper.Lerp(1f, 0.5f, amount);
                        sprite.Scale.Y = MathHelper.Lerp(1f, 1.5f, amount);
                    }
                }
                else
                {
                    maxFall = Calc.Approach(maxFall, num4, 300f * Engine.DeltaTime);
                }
            }
            float grav = noGravityTimer > 0 ? 0 : Math.Abs(Speed.Y) < 40f && (Input.Jump.Check || AutoJump) ? 0.5f : 1f;
            float yTarget = maxFall;
            if (FlyToggled)
            {
                Speed.Y = Calc.Approach(Speed.Y, yTarget * yDir * GravityMult, 900f * grav * FlyMoveMult * Engine.DeltaTime);
            }
            else if (!onGround)
            {
                Speed.Y = Calc.Approach(Speed.Y, yTarget * yDir, 900f * grav * Engine.DeltaTime);
            }
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
        public void NoGravityFor(float seconds)
        {
            noGravityTimer = seconds;
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
            if (StateMachine.State == 14)
            {
                CalidusDeathEffect.Draw(Center + deadOffset, Color.Lime, introEase, -1);
            }
            else
            {
                if (!CanSee)
                {
                    Glow.Render();
                }
                else if (StateMachine.State != RailState && !InRail && !Dead)
                {
                    RenderParts();
                }
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
            if (!CanSee)
            {
                LightingShiftAmount = Calc.Approach(LightingShiftAmount, 1, Engine.DeltaTime);
            }
            else
            {
                LightingShiftAmount = Calc.Approach(LightingShiftAmount, 0, Engine.DeltaTime);
            }
            SetLighting(level, LightingShiftAmount);
            //DigitalGrid.CalidusEyeDiff = LightingShiftAmount * 0.7f;
        }

        public void SetScale(Vector2 scale)
        {
            sprite.Scale = scale;
        }
        public IEnumerator StruggleFly()
        {
            noGravityTimer = 10;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Speed.X = 0;
                yield return null;
            }
        }
        public IEnumerator SlugRoutine()
        {
            while (true)
            {
                SlugMult = 0;
                while (!Sluggish)
                {
                    yield return null;
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.7f)
                {
                    SlugMult = Calc.LerpClamp(0, 1, Ease.QuadIn(i));
                    yield return null;
                }
                ShakeFor(0.1f);
                SlugMult = 1;
                yield return 0.05f;

                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.5f)
                {
                    SlugMult = Calc.LerpClamp(0.5f, 0, Ease.QuadIn(i));
                    yield return null;
                }
                while (!Moving)
                {
                    yield return null;
                }
                yield return 0.5f;
            }
        }
        public bool CrouchDashed;
        public int StartBlip()
        {
            CrouchDashed = true;
            demoDashed = Input.CrouchDashPressed;
            Input.CrouchDash.ConsumeBuffer();
            Input.Dash.ConsumeBuffer();
            sprite.LookSpeed = 5;
            if (!FlyToggled)
            {
                Dashes--;
            }
            return BlipState;

        }
        public bool TryGetBlipTarget(Vector2 from, int dist, out Vector2 target)
        {
            Vector2 dir = lastAim;
            if (dist == 0)
            {
                target = from;
                return true;
            }
            target = from + dist * dir;
            float distance = dist;
            while (CollideCheck<Solid>(target) && distance > 0)
            {
                target += -dir;
                distance -= 1;
            }
            target.Floor();
            Level level = Scene as Level;
            LevelData data = level.Session.MapData.GetAt(target);
            LevelData thisData = level.Session.LevelData;
            if (dist > 0f && data != null &&
                (data.Equals(thisData) || data.Spawns != null && data.Spawns.Count > 0
                && level.Tracker.GetEntity<BlipTransitionController>() == null))
            {
                LastBlipDir = dir;
                LastDashOrBlipDir = dir;
                LastBlip = target;
                return true;
            }
            return false;
        }
        public bool TryGetBlipTarget(Vector2 from, Vector2 speed, out Vector2 target)
        {
            Vector2 dir = speed.Sign();
            if (speed == Vector2.Zero)
            {
                target = from;
                return true;
            }
            target = from + speed * Engine.DeltaTime;
            float dist = Vector2.Distance(from, target);
            return TryGetBlipTarget(from, (int)dist, out target);
            /*            while (CollideCheck<Solid>(target) && dist > 0)
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
                        return false;*/
        }
        [OnLoad]
        internal static void Load()
        {
            On.Celeste.Level.LoadNewPlayer += On_Level_LoadNewPlayer;
            On.Celeste.Level.LoadNewPlayerForLevel += On_Level_LoadNewPlayerForLevel;
            On.Celeste.Player.BeforeUpTransition += Player_BeforeUpTransition;
            On.Celeste.Player.BeforeDownTransition += Player_BeforeDownTransition;
            On.Celeste.Player.TransitionTo += Player_TransitionTo;
            On.Celeste.Player.Die += Player_Die;
        }
        [OnUnload]
        internal static void Unload()
        {
            On.Celeste.Level.LoadNewPlayer -= On_Level_LoadNewPlayer;
            On.Celeste.Level.LoadNewPlayerForLevel -= On_Level_LoadNewPlayerForLevel;
            On.Celeste.Player.BeforeUpTransition -= Player_BeforeUpTransition;
            On.Celeste.Player.BeforeDownTransition -= Player_BeforeDownTransition;
            On.Celeste.Player.TransitionTo -= Player_TransitionTo;
            On.Celeste.Player.Die -= Player_Die;
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
/*            if (self is PlayerCalidus pC)
            {
                Level level = self.Scene as Level;
                CalidusDeadBody playerDeadBody = pC.neworig_Die(direction, evenIfInvincible, registerDeathInStats);
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
            else*/ return orig(self, direction, evenIfInvincible, registerDeathInStats);
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
            float defaultStart = Light.StartRadius;
            float defaultEnd = Light.EndRadius;
            float defaultAlpha = Light.Alpha;
            float startFade = Light.StartRadius + 8;
            float endFade = Light.EndRadius + 16;
            float duration;
            float alpha = 1;
            while (true)
            {
                Light.StartRadius = defaultStart;
                Light.EndRadius = defaultEnd;
                Light.Alpha = defaultAlpha;
                while (CanSee)
                {
                    yield return null;
                }

                float stFrom = startFade;
                float endFrom = endFade;
                float alphaFrom = alpha;

                startFade = StartFadeDefault + Calc.Random.Range(-2, 2f);
                endFade = startFade * 2;
                duration = Calc.Random.Range(0.1f, 0.3f);
                alpha = 0.6f + 0.4f * MathHelper.Distance(startFade, StartFadeDefault) / 8f;
                for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
                {
                    if (CanSee) break;
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
            if (ZoomRolling)
            {
                int dir = (int)LastValidXAim;
                Vector2 pos = (TopLeft + Collider.HalfSize).Mod(8) + new Vector2(-8 + dir * 8, -16);
                DebugVec = pos;
                if (CollideCheck<Solid>(pos))
                {
                    ZoomRolling = false;
                    zoomRollRate = 0;
                    debugBool = false;
                }
                else
                {
                    MoveV(-8);
                    MoveH(dir);
                    debugBool = true;
                    return;
                }

            }
            Speed.X = 0f;
        }
        private bool debugBool;
        private void newOnCollideV(CollisionData data)
        {
            if (StateMachine.State == RailState) return;
            if (Speed.Y * GravityMult < 0f)
            {
                int num3 = 2;

                if (Speed.X <= 0.01f)
                {
                    for (int j = 1; j <= num3; j++)
                    {
                        if (!CollideCheck<Solid>(Position + new Vector2(-j, -1f * GravityMult)))
                        {
                            Position += new Vector2(-j, -1f * GravityMult);
                            return;
                        }
                    }
                }

                if (Speed.X >= -0.01f)
                {
                    for (int k = 1; k <= num3; k++)
                    {
                        if (!CollideCheck<Solid>(Position + new Vector2(k, -1f * GravityMult)))
                        {
                            Position += new Vector2(k, -1f * GravityMult);
                            return;
                        }
                    }
                }
                if (varJumpTimer < 0.15f)
                {
                    varJumpTimer = 0f;
                }
            }
            if (data.Hit != null && data.Hit.OnCollide != null)
            {
                data.Hit.OnCollide(data.Direction);
            }
            dashAttackTimer = 0f;
            gliderBoostTimer = 0f;
            Speed.Y = 0f;

        }

        public struct CalidusInventory
        {
            public const float EyeJumpMult = 0.53f;
            public const float HeadJumpMult = 0.8f;
            public static readonly CalidusInventory Nothing = new CalidusInventory(canFly: true);
            public static readonly CalidusInventory Grounded = new CalidusInventory();
            public static readonly CalidusInventory Slowed = new CalidusInventory(slowAmount: 0.3f);
            public static readonly CalidusInventory Weakened = new CalidusInventory(slowAmount: 0.7f, weakened: true);
            public static readonly CalidusInventory Vision = new()
            {
                CanSee = true,
                Jumps = 1,
                BaseJumpMult = EyeJumpMult,
                HasEye = true
            };

            public static readonly CalidusInventory HigherJump = new()
            {
                CanSee = true,
                Jumps = 1,
                BaseJumpMult = HeadJumpMult,
                HasEye = true,
                HasHead = true
            };
            public static readonly CalidusInventory StickyUpgrade = new()
            {
                CanSee = true,
                Jumps = 1,
                BaseJumpMult = HeadJumpMult,
                HasEye = true,
                HasHead = true,
                Sticky = true,
            };
            public static readonly CalidusInventory RailUpgrade = new()
            {
                CanSee = true,
                Jumps = 1,
                BaseJumpMult = HeadJumpMult,
                HasEye = true,
                HasHead = true,
                Sticky = true,
                HasArms = true,
                CanUseRails = true
            };
            public static readonly CalidusInventory FullUpgrade = new()
            {
                CanSee = true,
                CanFly = true,
                CanBlip = true,
                CanUseRails = true,
                Sticky = true,
                Jumps = 1,
                BaseJumpMult = HeadJumpMult,
                HasEye = true,
                HasHead = true,
                HasArms = true
            };
            public bool Sticky;
            public bool CanBlip;
            public bool CanFly;
            public bool CanSee;
            public bool CanUseRails;
            public bool HasEye;
            public bool HasHead;
            public bool HasArms;


            public int Jumps;
            public float Slowness;
            public bool Weak;
            public float BaseJumpMult;


            public CalidusInventory(float slowAmount = 0, bool weakened = false, bool canSee = false, int jumps = 0, float baseJumpMult = 0, bool canStick = false, bool canUseRails = false, bool canFly = false, bool canBlip = false)
            {
                CanSee = canSee;
                Jumps = jumps;
                Sticky = canStick;
                CanFly = canFly;
                CanBlip = canBlip;
                BaseJumpMult = baseJumpMult;
                Slowness = slowAmount;
                Weak = weakened;
                CanUseRails = canUseRails;
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
            public override void ApplyParameters(Level level)
            {
                base.ApplyParameters(level);
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
            Vector2 vec = sprite.LookDir switch
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
            if (!Moving) return vec.YComp();
            return vec;
        }
        #endregion
    }
}