using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities
{

    [CustomEntity("PuzzleIslandHelper/GearMachine")]
    [Tracked]
    public class GearHolder : Entity
    {
        [Tracked]
        public class GearHolderRenderer : Entity
        {
            public const string Path = "objects/PuzzleIslandHelper/gear/";
            public MTexture MaskImage = GFX.Game[Path + "mask"];
            public MTexture Insides = GFX.Game[Path + "inside"];
            public MTexture Stem = GFX.Game[Path + "center"];
            public MTexture Outline = GFX.Game[Path + "outline"];
            public Vector2 Origin = Vector2.One * 12;
            public Vector2 Offset = Vector2.One * 8;
            private bool noHolders;
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
                if (Scene is not Level level || noHolders) return;
                Draw.SpriteBatch.Draw(Target, level.Camera.Position.Floor(), Color.White);
                foreach (GearHolder holder in level.Tracker.GetEntities<GearHolder>())
                {
                    Draw.SpriteBatch.Draw(Outline.Texture.Texture_Safe, holder.RenderPosition + Offset, null, holder.BackColor, 0, Origin, 1, SpriteEffects.None, 0);
                }
            }

            public void BeforeRender()
            {
                if (Scene is not Level level) return;
                noHolders = level.Tracker.GetEntities<GearHolder>().Count == 0;
                if (noHolders) return;
                Matrix m = Matrix.Identity;
                Mask.DrawToObject(RenderMask, m, true);
                GameplayBuffers.TempA.DrawToObject(RenderInsides, m, true);
                GameplayBuffers.TempA.MaskToObject(Mask);
                Target.DrawToObject(RenderImages, m, true);
            }

            private void RenderMask()
            {
                if (Scene is not Level level) return;
                foreach (GearHolder holder in level.Tracker.GetEntities<GearHolder>())
                {
                    Draw.SpriteBatch.Draw(MaskImage.Texture.Texture_Safe, holder.RenderPosition - level.Camera.Position + Offset, null, Color.White, 0, Origin, 1, SpriteEffects.None, 0);
                }
            }
            private void RenderInsides()
            {
                if (Scene is not Level level) return;
                foreach (GearHolder holder in level.Tracker.GetEntities<GearHolder>())
                {
                    Draw.SpriteBatch.Draw(Insides.Texture.Texture_Safe, holder.RenderPosition - level.Camera.Position + Offset, null, holder.BackColor, holder.Rotation.ToRad(), Origin, 1, SpriteEffects.None, 0);
                }
            }
            private void RenderImages()
            {
                Draw.SpriteBatch.Draw(GameplayBuffers.TempA, Vector2.Zero, Color.White * 0.5f);
            }
        }
        public Sprite Sprite;
        private Color Color;
        public Color BackColor = Color.Cyan;
        public float Rotation;
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
        private float rate;
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
        public MTexture Stem = GFX.Game["objects/PuzzleIslandHelper/gear/center"];
        public MTexture Middle = GFX.Game["objects/PuzzleIslandHelper/Gear/holder"];
        public MTexture FakeGear = GFX.Game["objects/PuzzleIslandHelper/Gear/fakeGear"];
        public float TimeLimit;
        private float timer;
        public bool HoldingGear;
        public EntityID EntityID;
        public bool HasTime
        {
            get
            {
                return TimeLimit == -1 || timer < TimeLimit;
            }
        }
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
        public bool CanUseGear(Gear gear) => gear != null && !HasGear && !gear.Hold.IsHeld && (!OnlyOnce || !UsedOnce) && !preventGrab;
        public ParticleType GearSparks = new()
        {
            Color = Color.Orange,
            Color2 = Color.Yellow,
            DirectionRange = (float)Math.PI / 2,
            ColorMode = ParticleType.ColorModes.Choose,
            FadeMode = ParticleType.FadeModes.Late,
            Friction = 1,
            Size = 1,
            SpeedMin = 10,
            SpeedMax = 50,
            LifeMin = 0.7f,
            LifeMax = 1.8f
        };
        public GearHolder(Vector2 position, bool onlyOnce, Color color, EntityID entityId, float rotateRate = 10f, int id = -1, string flag = "", float stopAfter = -1) : base(position)
        {
            EntityID = entityId;
            TimeLimit = stopAfter;
            Depth = 2;
            Color = Color.White;
            OnlyOnce = onlyOnce;
            ID = id;
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/Gear/");
            Sprite.AddLoop("idle", "holder", 0.1f);
            Sprite.Position += new Vector2(Sprite.Width / 2, Sprite.Height / 2);
            int offset = 2;
            Collider = new Hitbox(Sprite.Width - offset, Sprite.Height - offset, offset / 2, offset / 2);
            rate = rotateRate;
            this.flag = flag;
            BackColor = color;
            Light = new VertexLight(Color.White, 0.4f, 24, 40);
            Light.Position = Vector2.One * 12;
            Add(Light);
            Bloom = new BloomPoint(0.5f, 24);
            Bloom.Position = Vector2.One * 12;
            Add(Bloom);
            Add(SlotCoroutine = new Coroutine(false));
        }
        public GearHolder(EntityData data, Vector2 offset, EntityID entityID)
            : this(data.Position + offset - Vector2.One * 8, data.Bool("onlyOnce"), Color.White, entityID, 10, data.Int("holderId"), data.Attr("flagOnFinish"), 1)
        {
        }
        public void EmitSparks(float speed, int amount)
        {
            ParticleSystem system = SceneAs<Level>().ParticlesBG;
            GearSparks.SpeedMin = speed;
            GearSparks.SpeedMax = speed + 1;
            for (int i = 0; i < amount; i++)
            {
                float angle = Calc.Random.Range((float)-Math.PI, (float)Math.PI);
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                system.Emit(GearSparks, Center + offset * SparkOffset, angle);
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (PianoUtils.SeekController<GearHolderRenderer>(scene) is null)
            {
                scene.Add(new GearHolderRenderer());
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (PianoModule.Session.GearData.HasHolder(EntityID))
            {
                SwitchToFake();
            }
        }
        public bool IsBaseHolder()
        {
            return !GetType().IsSubclassOf(typeof(GearHolder));
        }
        public override void Render()
        {
            base.Render();
            Vector2 offset = -Vector2.One * 4;
            Draw.SpriteBatch.Draw(Stem.Texture.Texture_Safe, RenderPosition + offset, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            if (useFakeGearImage)
            {
                Vector2 gearOffset = new Vector2(FakeGear.Width / 2, FakeGear.Height / 2).Floor();
                FakeGear.DrawOutline(RenderPosition + offset + gearOffset, gearOffset, Color.White, 1, Rotation.ToRad());
            }
        }
        public bool HasGear;
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
        public virtual IEnumerator WhileSpinning(Gear gear)
        {
            yield return null;
        }
        private IEnumerator SlotRoutine(Gear gear)
        {
            if (useFakeGearImage) yield break;
            gear?.OnEnterHolder();
            StartSpinning();
            float lerp = 0;
            Coroutine whileSpinning = new Coroutine(WhileSpinning(gear));
            if (!DropGear) Add(whileSpinning);
            while (gear != null && gear.InSlot && !StopSpin && HasTime)
            {
                timer += Engine.DeltaTime;
                Spinning = true;
                Rotation += RotateRate * lerp;
                lerp = Calc.Min(1, lerp + Engine.DeltaTime);
                yield return null;
            }
            if (!whileSpinning.Finished) whileSpinning.Cancel();
            gear.Sprite.Play("flash");
            Spinning = false;

            yield return WindBack(gear, DropGear);
        }
        public void LockGear(Gear gear)
        {
            if (Scene is null || PianoModule.Session.GearData.HasHolder(EntityID)) return; //if holder already has a gear locked in place, return
            PianoModule.Session.GearData.AddGear(gear);
            PianoModule.Session.ContinuousGearIDs.Remove(gear.ContinuityID);
            UsedOnce = true;
            gear.RemoveSelf();
            SwitchToFake();
        }
        public void SwitchToFake()
        {
            Spinning = false;
            useFakeGearImage = true;
            HasGear = true;
        }
        public virtual void OnWindBack(Gear gear, bool drop)
        {

        }
        private bool preventGrab;
        public void PreventRegrab(float time)
        {
            preventGrab = true;
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
        public IEnumerator WindBack(Gear gear, bool drop)
        {
            OnWindBack(gear, drop);
            float time = 0.3f;
            bool lockgear = IsBaseHolder() && gear is not null && gear.InSlot && !UsedOnce;
            float from = gear is not null ? gear.Light.Alpha : 0;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                if (lockgear)
                {
                    gear.Light.Alpha = Calc.LerpClamp(from, 0, Ease.SineOut(i));
                }
                Rotation += RotateRate * (1 - i);
                yield return null;
            }
            Add(new Coroutine(easeBack(1.4f, Ease.SineInOut)));
            if (lockgear)
            {
                LockGear(gear);
            }
            else if (drop)
            {
                PreventRegrab(WaitTime);
                if (gear is not null)
                {
                    HasGear = false;
                    gear.DropFromHolder();
                    gear.InSlot = false;
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
        public virtual void OnShake(Vector2 amount)
        {
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
            if (OnlyOnce && UsedOnce)
            {
                Color = Color.Lerp(Color, Color.Gray, Engine.DeltaTime);
            }
            Rotation = (float)Math.Round(Rotation);
            Rotations = Rotation / 360;
        }
        public Coroutine SlotCoroutine;
        public void StartRoutine(Gear gear)
        {
            if (gear is null) return;
            HasGear = true;
            InGearRoutine = true;
            DropGear = false;
            gear.InSlot = true;
            SlotCoroutine.Replace(SlotRoutine(gear));
            //Add(new Coroutine(SlotRoutine(gear)));
        }
        public bool InReach(Gear gear, float distance)
        {
            float result = Vector2.Distance(Center, gear.Center);
            return result <= distance;
        }
    }
}