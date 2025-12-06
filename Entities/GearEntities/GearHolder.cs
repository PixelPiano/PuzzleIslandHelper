using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities
{

    [CustomEntity("PuzzleIslandHelper/GearMachine")]
    [Tracked]
    public class GearHolder : Entity
    {
        public const string Path = "objects/PuzzleIslandHelper/gear/";
        public static MTexture MaskImage => GFX.Game[Path + "mask" + (RenderSmall ? "Small" : "")];
        public static MTexture Insides => GFX.Game[Path + "inside" + (RenderSmall ? "Small" : "")];
        //public static MTexture Stem => GFX.Game[Path + "center" + (RenderSmall ? "Small" : "")];
        public static MTexture Outline => GFX.Game[Path + "outline" + (RenderSmall ? "Small" : "")];
        public static Vector2 Origin => MaskImage.HalfSize();
        public static Vector2 Offset => Vector2.One * 8;
        public static bool RenderSmall = true;
        [Tracked]
        public class GearHolderRenderer : Entity
        {
            private static VirtualRenderTarget _Target;
            public static VirtualRenderTarget Target => _Target ??= VirtualContent.CreateRenderTarget("GearHolderRendererTarget", 320, 180);
            private static VirtualRenderTarget _Mask;
            public static VirtualRenderTarget Mask => _Mask ??= VirtualContent.CreateRenderTarget("GearHolderRendererMask", 320, 180);
            public GearHolderRenderer() : base(Vector2.Zero)
            {
                Depth = 3;
                Tag |= Tags.TransitionUpdate | Tags.Persistent;
                Add(new BeforeRenderHook(BeforeRender));
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                _Target?.Dispose();
                _Mask?.Dispose();
                _Target = null;
                _Mask = null;
            }
            public override void Render()
            {
                base.Render();
                if (Scene is not Level level || holders.Count <= 0) return;
                Draw.SpriteBatch.Draw(Target, level.Camera.Position.Floor(), Color.White);
                var outline = Outline.Texture.Texture_Safe;
                foreach (GearHolder holder in holders)
                {
                    Draw.SpriteBatch.Draw(outline, holder.RenderPosition + Offset, null, holder.BackColor, 0, Origin, 1, SpriteEffects.None, 0);
                }
            }
            private List<Entity> holders;
            public void BeforeRender()
            {
                if (Scene is not Level level) return;
                Matrix m = Matrix.Identity;

                holders = level.Tracker.GetEntities<GearHolder>();
                if (holders.Count <= 0) return;
                Mask.ApplyDraw(RenderMask, m, true);
                GameplayBuffers.TempA.ApplyDraw(RenderInsides, m, true);
                GameplayBuffers.TempA.ApplyMask(Mask);
                Target.ApplyDraw(RenderImages, m, true);
            }

            private void RenderMask()
            {
                if (Scene is not Level level) return;
                var m = MaskImage.Texture.Texture_Safe;
                foreach (GearHolder holder in holders)
                {
                    Draw.SpriteBatch.Draw(m, holder.RenderPosition - level.Camera.Position + Offset, null, Color.White, 0, Origin, 1, SpriteEffects.None, 0);
                }
            }
            private void RenderInsides()
            {
                if (Scene is not Level level) return;
                var i = Insides.Texture.Texture_Safe;
                foreach (GearHolder holder in holders)
                {
                    Draw.SpriteBatch.Draw(i, holder.RenderPosition - level.Camera.Position + Offset, null, holder.BackColor, holder.Rotation.ToRad(), Origin, 1, SpriteEffects.None, 0);
                }
            }
            private void RenderImages()
            {
                Draw.SpriteBatch.Draw(GameplayBuffers.TempA, Vector2.Zero, Color.White);
            }
        }
        public Sprite Sprite;
        private Color Color;
        public Color BackColor = Color.Cyan;

        private float dustRate = 45.5f;
        private float rotationVelocity;
        public float Rotation
        {
            get => rotation;
            set
            {
                float prev = rotation;
                rotation = value;
                if (ReallyGrindMyGear && Scene is Level level)
                {
                    float dist = Math.Abs(rotation - prev);
                    rotationVelocity += dist;
                    if (rotationVelocity > dustRate)
                    {
                        StartShaking(Engine.DeltaTime * 2);
                        for (; rotationVelocity > dustRate; rotationVelocity -= dustRate)
                        {
                            Vector2 p = Center + Vector2.UnitX * (1 + Calc.Random.Range(-4, 5));
                            level.ParticlesBG.Emit(SpinDust, p);
                            EmitSparks(140f, 1);
                        }
                    }
                }
            }
        }
        private float rotation;
        public bool InGearRoutine;
        public bool ForceCanHold;
        public float RotateRate
        {
            get
            {
                return rate * SpinDirection;
            }
            set
            {
                rate = value;
            }
        }
        public float rate;
        public bool DropGear;
        public float Rotations;
        public float WaitTime = 1;
        public bool OnlyOnce;
        public bool UsedOnce;
        public int SpinDirection = 1;
        public bool Spinning;
        public float SparkOffset = 3;
        private bool useFakeGearImage;
        public bool Fixed => useFakeGearImage;
        public bool Persistent;
        public bool StopSpin;
        public int ID;
        private string flag;
        public static List<EntityID> PersistentHolders = new();
        public VertexLight Light;
        public BloomPoint Bloom;
        public MTexture Middle = GFX.Game["objects/PuzzleIslandHelper/Gear/holder"];
        public float TimeLimit;
        private float timer;
        public bool HoldingGear;
        public EntityID EntityID;
        public bool HasTime => TimeLimit < 0 || timer < TimeLimit;
        public float shakeTimer;
        public Vector2 shakeAmount;
        protected bool Shaking;
        public float ShakeMult = 1;
        public Vector2 RealShakeAmount
        {
            get
            {
                return shakeAmount * ShakeMult;
            }
            set
            {
                shakeAmount = value;
            }
        }
        public Vector2 RenderPosition => Position + RealShakeAmount;
        public bool IsBaseHolder;
        public bool HasGear;
        private bool preventGrab;
        public Coroutine SlotCoroutine;
        public float targetRotateRate;
        public bool SwitchesColors = true;
        public Color GearColor = Color.White;
        public bool Interruptable;
        public float LockedTimer;
        public bool ReallyGrindMyGear;
        public Image FakeGear;
        public ParticleSystem SparkSystem;
        public ParticleSystem SparkFlashSystem;
        public bool CanUseGear(Gear gear) => gear != null && !HasGear && !gear.Hold.IsHeld && (!OnlyOnce || !UsedOnce) && LockedTimer <= 0 && !preventGrab;
        public ParticleType GearSparks = new()
        {
            Color = Color.Orange,
            Color2 = Color.Yellow,
            DirectionRange = (float)Math.PI / 2,
            ColorMode = ParticleType.ColorModes.Choose,
            FadeMode = ParticleType.FadeModes.Late,
            Friction = 5,
            Size = 1,
            SpeedMin = 40,
            SpeedMax = 120,
            LifeMin = 0.05f,
            LifeMax = 0.1f
        };
        public ParticleType GearSparkFlash = new()
        {
            Color = Color.White,
            FadeMode = ParticleType.FadeModes.None,
            Size = 1,
            LifeMin = 0.05f,
            LifeMax = 0.1f
        };
        public ParticleType SpinDust = new()
        {
            Color = Color.LightGray * 0.8f,
            Color2 = Color.Gray * 0.8f,
            DirectionRange = 30f.ToRad(),
            Direction = (float)(Math.PI / 2f),
            ColorMode = ParticleType.ColorModes.Choose,
            FadeMode = ParticleType.FadeModes.Linear,
            Friction = 3,
            Acceleration = -Vector2.UnitY * 4,
            Size = 1,
            SpeedMin = 5,
            SpeedMax = 15,
            LifeMin = 1f,
            LifeMax = 2f
        };
        public GearHolder(Vector2 position, bool onlyOnce, Color color, EntityID entityId, float rotateRate = 10f, int id = -1, bool playerCanInterrupt = true, string flag = "", float stopAfter = -1) : base(position)
        {
            Interruptable = playerCanInterrupt;
            IsBaseHolder = !GetType().IsSubclassOf(typeof(GearHolder));
            if (IsBaseHolder)
            {
                SwitchesColors = false;
                ReallyGrindMyGear = true;
            }
            this.flag = flag;
            EntityID = entityId;
            TimeLimit = stopAfter;
            OnlyOnce = onlyOnce;
            ID = id;
            targetRotateRate = rotateRate;
            BackColor = color;
            Depth = 2;
            Color = Color.White;
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/Gear/");
            Sprite.AddLoop("idle", "holder", 0.1f);
            int offset = 2;
            Collider = new Hitbox(Sprite.Width - offset, Sprite.Height - offset, offset / 2, offset / 2);
            Add(Light = new VertexLight(Color.White, 0.4f, 24, 40) { Position = Vector2.One * 12 });
            Add(Bloom = new BloomPoint(0.5f, 24) { Position = Vector2.One * 12 });
            Add(SlotCoroutine = new Coroutine(false));
            Add(FakeGear = new Image(GFX.Game["objects/PuzzleIslandHelper/Gear/fakeGear"]));
            FakeGear.Visible = false;
            FakeGear.CenterOrigin();
            FakeGear.RenderPosition = Center;
        }
        public GearHolder(EntityData data, Vector2 offset, EntityID entityID)
            : this(data.Position + offset - Vector2.One * 8, data.Bool("onlyOnce"), Color.White, entityID, 10, data.Int("holderId"), false, data.Attr("flagOnFinish"), 1)
        {
        }
        public virtual void OnGearRelease()
        {
            HasGear = false;
        }
        public void EmitSparks(float speed, int amount)
        {
            GearSparks.SpeedMin = speed;
            GearSparks.SpeedMax = speed + 1;
            for (int i = 0; i < amount; i++)
            {
                float angle = Calc.Random.NextAngle();
                Vector2 angleVector = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Vector2 position = Center + SparkOffset * angleVector;
                SparkSystem.Emit(GearSparks, position, angle + Calc.Random.NextAngle() * 0.2f);
                Vector2 flashPosition = position + (Width / 3f * angleVector);
                SparkFlashSystem.Emit(GearSparkFlash, flashPosition);
                VertexLight light = new VertexLight(flashPosition - Position, Color.White, 0.6f, 3, 5);
                Add(light);
                Alarm.Set(this, 0.2f, () => Remove(light));
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            string texturePath = Path + "center";
            if (RenderSmall)
            {
                texturePath += "Small";
                if (Interruptable)
                {
                    texturePath += "NoGrab";
                }
            }
            Add(new Image(GFX.Game[texturePath]));
            if (PianoUtils.SeekController<GearHolderRenderer>(scene) is null)
            {
                scene.Add(new GearHolderRenderer());
            }
            SparkSystem = new ParticleSystem(Depth + 1, 20);
            SparkFlashSystem = new ParticleSystem(Depth - 1, 20);
            scene.Add(SparkSystem, SparkFlashSystem);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(SparkSystem, SparkFlashSystem);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (PianoModule.Session.GearData.HasHolder(EntityID))
            {
                flag.SetFlag();
                SwitchToFake();
            }
        }
        public override void Render()
        {
            base.Render();
            if (useFakeGearImage)
            {
                FakeGear.DrawSimpleOutline();
                FakeGear.Render();
            }
        }
        public override void Update()
        {
            if (Shaking)
            {
                if (Scene.OnInterval(0.04f))
                {
                    Vector2 vector = RealShakeAmount;
                    RealShakeAmount = Calc.Random.ShakeVector();
                    OnShake(RealShakeAmount - vector);
                }

                if (shakeTimer > 0f)
                {
                    shakeTimer -= Engine.DeltaTime;
                    if (shakeTimer <= 0f)
                    {
                        Shaking = false;
                        StopShaking();
                    }
                }
            }
            base.Update();
            LockedTimer = Math.Max(LockedTimer - Engine.DeltaTime, 0);
            if (OnlyOnce && UsedOnce)
            {
                Color = Color.Lerp(Color, Color.Gray, Engine.DeltaTime);
            }
            Rotation = (float)Math.Round(Rotation + RotateRate);
            Rotations = Rotation / 360;
            FakeGear.Rotation = Rotation.ToRad();
        }
        public virtual void StopSpinning(bool drop = true)
        {
            if (drop)
            {
                DropGear = true;
            }
            StopSpin = true;
            timer = 0;
        }
        public virtual void StartSpinning()
        {
            DropGear = false;
            StopSpin = false;
            timer = 0;
        }
        public virtual void OnWindBack(Gear gear, bool drop)
        {

        }
        public virtual void SpinningUpdate(Gear gear, float rotateLerp)
        {
            rate = targetRotateRate * rotateLerp;
        }
        private IEnumerator SlotRoutine(Gear gear)
        {
            gear?.OnEnterHolder();
            StartSpinning();
            bool lockgear = IsBaseHolder && gear is not null && gear.InSlot && !UsedOnce;
            if (lockgear)
            {
                gear.Hold.Active = false;
                if (!(Scene is null || PianoModule.Session.GearData.HasHolder(EntityID)))
                {
                    PianoModule.Session.GearData.AddGear(gear);
                    PianoModule.Session.ContinuousGearIDs.Remove(gear.ContinuityID);
                    UsedOnce = true;
                }
            }
            Coroutine whileSpinning = new Coroutine(WhileSpinning(gear));
            if (!DropGear) Add(whileSpinning);
            float lerp = 0;
            while (gear != null && gear.InSlot && !StopSpin && HasTime)
            {
                if (TimeLimit >= 0) timer += Engine.DeltaTime;
                Spinning = true;
                SpinningUpdate(gear, lerp);
                lerp = Calc.Min(1, lerp + Engine.DeltaTime);
                yield return null;
            }
            if (!whileSpinning.Finished) whileSpinning.Cancel();
            gear.Sprite.Play("flash");
            Spinning = false;
            OnWindBack(gear, DropGear);
            float time = 0.3f;
            float from = gear is not null ? gear.Light.Alpha : 0;
            yield return lockgear ?
                PianoUtils.Lerp(Ease.SineOut, time, f => gear.Light.Alpha = Calc.LerpClamp(from, 0, f)) :
                PianoUtils.Lerp(Ease.Linear, time, f => rate = targetRotateRate * (1 - f));
            if (lockgear)
            {
                gear.RemoveSelf();
                SwitchToFake();
            }
            else
            {
                rate = 0;
                Add(new Coroutine(easeBack(1.4f, Ease.SineInOut)));
                if (DropGear)
                {
                    PreventRegrab(WaitTime);
                    if (gear is not null)
                    {
                        HasGear = false;
                        gear.DropFromHolder();
                        gear.Hold.Active = true;
                        gear.InSlot = false;
                    }
                }
            }
            UsedOnce = true;
            InGearRoutine = false;
            if (!string.IsNullOrEmpty(flag))
            {
                SceneAs<Level>().Session.SetFlag(flag);
            }
            yield return null;
        }
        public virtual IEnumerator WhileSpinning(Gear gear)
        {
            yield return null;
        }
        public virtual void OnShake(Vector2 amount)
        {
        }
        public void SwitchToFake(bool constantSpinning = true)
        {
            if (constantSpinning)
            {
                rate = targetRotateRate;
            }
            Spinning = false;
            useFakeGearImage = true;
            HasGear = true;
        }
        public void PreventRegrab(float time)
        {
            preventGrab = true;
            if (time < 0)
            {
                preventGrab = true;
                ForceCanHold = false;
                return;
            }
            bool cancelled = false;
            Tween.Set(this, Tween.TweenMode.Oneshot, time, Ease.Linear, t =>
            {
                if (ForceCanHold)
                {
                    cancelled = true;
                    preventGrab = false;
                    ForceCanHold = false;
                }
            }, t =>
            {
                if (!cancelled)
                {
                    ForceCanHold = false;
                    preventGrab = false;
                }
            });
        }
        public void StopShaking()
        {
            Shaking = false;

            if (shakeAmount != Vector2.Zero)
            {
                OnShake(-RealShakeAmount);
                RealShakeAmount = Vector2.Zero;
            }
        }
        public void StartShaking(float time)
        {
            shakeTimer = time;
            Shaking = true;
        }
        public void StartRoutine(Gear gear)
        {
            if (gear is null) return;
            HasGear = true;
            InGearRoutine = true;
            DropGear = false;
            gear.InSlot = true;
            if (!useFakeGearImage)
            {
                SlotCoroutine.Replace(SlotRoutine(gear));
            }
            //Add(new Coroutine(SlotRoutine(gear)));
        }
        public bool InReach(Gear gear, float distance) => Vector2.Distance(Center, gear.Center) <= distance;
        private IEnumerator easeBack(float time, Ease.Easer ease)
        {
            float start = Rotation;
            yield return 0.05f;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                if (Spinning) yield break;
                Rotation = Calc.LerpClamp(start, 0, ease(i));
                yield return null;
            }
            Rotation = 0;
        }
    }
}