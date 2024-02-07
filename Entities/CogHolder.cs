using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections;
using System;
using System.Collections.Generic;
using Celeste.Mod.CommunalHelper;
using System.IO;
using MonoMod.Cil;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [CustomEntity("PuzzleIslandHelper/CogMachine")]
    [Tracked]
    public class CogHolder : Entity
    {
        [Tracked]
        public class CogHolderRenderer : Entity
        {
            public const string Path = "objects/PuzzleIslandHelper/cog/";
            public MTexture MaskImage = GFX.Game[Path + "mask"];
            public MTexture Insides = GFX.Game[Path + "inside"];
            public MTexture Stem = GFX.Game[Path + "center"];
            public MTexture Outline = GFX.Game[Path + "outline"];
            public Vector2 Origin = Vector2.One * 12;
            public Vector2 Offset = Vector2.One * 8;
            private bool noHolders;
            private static VirtualRenderTarget _Target;
            public static VirtualRenderTarget Target => _Target ??= VirtualContent.CreateRenderTarget("CogHolderRendererTarget", 320, 180);
            private static VirtualRenderTarget _Mask;
            public static VirtualRenderTarget Mask => _Mask ??= VirtualContent.CreateRenderTarget("CogHolderRendererMask", 320, 180);

            public CogHolderRenderer() : base(Vector2.Zero)
            {
                Depth = 2;
                AddTag(Tags.Global);
                AddTag(Tags.TransitionUpdate);
                Add(new BeforeRenderHook(BeforeRender));
            }
            internal static void Load()
            {
                On.Celeste.LevelLoader.ctor += LevelLoader_ctor;
            }
            internal static void Unload()
            {
                On.Celeste.LevelLoader.ctor -= LevelLoader_ctor;
            }

            private static void LevelLoader_ctor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition)
            {
                orig(self, session, startPosition);
                self.Level.Add(new CogHolderRenderer());
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
                Draw.SpriteBatch.Draw(Target, level.Camera.Position.ToInt(), Color.White);
                foreach (CogHolder holder in level.Tracker.GetEntities<CogHolder>())
                {
                    Draw.SpriteBatch.Draw(Outline.Texture.Texture_Safe, holder.Position + Offset, null, holder.BackColor, 0, Origin, 1, SpriteEffects.None, 0);
                }
            }

            public void BeforeRender()
            {
                if (Scene is not Level level) return;
                noHolders = level.Tracker.GetEntities<CogHolder>().Count == 0;
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
                foreach (CogHolder holder in level.Tracker.GetEntities<CogHolder>())
                {
                    Draw.SpriteBatch.Draw(MaskImage.Texture.Texture_Safe, holder.Position - level.Camera.Position + Offset, null, Color.White, 0, Origin, 1, SpriteEffects.None, 0);
                }
            }
            private void RenderInsides()
            {
                if (Scene is not Level level) return;
                foreach (CogHolder holder in level.Tracker.GetEntities<CogHolder>())
                {
                    Draw.SpriteBatch.Draw(Insides.Texture.Texture_Safe, holder.Position - level.Camera.Position + Offset, null, holder.BackColor, holder.Rotation.ToRad(), Origin, 1, SpriteEffects.None, 0);
                }
            }
            private void RenderImages()
            {
                Draw.SpriteBatch.Draw(GameplayBuffers.TempA, Vector2.Zero, Color.White * 0.5f);
            }
        }
        public Sprite Sprite;
        public Cog Cog;
        private Color Color;
        public Color BackColor = Color.Cyan;
        public float Rotation;
        public bool InCogRoutine;
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
        public bool DropCog;
        public float cannotHoldTimer;
        public float Rotations;
        private bool timerActive;
        public const float WaitTime = 1;
        public bool OnlyOnce;
        public bool UsedOnce;
        public int SpinDirection = 1;
        public bool Spinning;
        public float SparkOffset = 3;

        public bool CogIsNull => Cog is null;
        public bool CanCogUpdate => !CogIsNull && InCogRoutine && Cog.Holder == this;
        private bool useFakeCogImage;
        public bool Persistent;
        public bool StopSpin;
        public int ID;
        private string flag;
        public static List<EntityID> PersistentHolders = new();
        public VertexLight Light;
        public BloomPoint Bloom;
        public MTexture Stem = GFX.Game["objects/PuzzleIslandHelper/cog/center"];
        public float TimeLimit;
        private float timer;
        public bool HoldingCog;
        public EntityID EntityID;
        public bool HasTime
        {
            get
            {
                return TimeLimit == -1 || timer < TimeLimit;
            }
        }
        public ParticleType CogSparks = new()
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
        public CogHolder(Vector2 position, bool onlyOnce, Color color, float rotateRate = 10f, int id = -1, string flag = "", float stopAfter = -1) : base(position)
        {
            TimeLimit = stopAfter;
            Depth = 1;
            Color = Color.White;
            OnlyOnce = onlyOnce;
            ID = id;
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/Cog/");
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
        }
        public CogHolder(EntityData data, Vector2 offset, EntityID entityID)
            : this(data.Position + offset, data.Bool("onlyOnce"), Color.White, 10, data.Int("holderId"), data.Attr("flagOnFinish"), 1)
        {
            EntityID = entityID;

        }
        public void EmitSparks(float speed, int amount)
        {
            ParticleSystem system = SceneAs<Level>().ParticlesBG;
            CogSparks.SpeedMin = speed;
            CogSparks.SpeedMax = speed + 1;
            for (int i = 0; i < amount; i++)
            {
                float angle = Calc.Random.Range((float)-Math.PI, (float)Math.PI);
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                system.Emit(CogSparks, Center + offset * SparkOffset, angle);
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            Cog = (scene as Level).Tracker.GetEntity<Cog>();
            if (PianoModule.SaveData.CogData.HasHolder(EntityID))
            {
                SwitchToFake();
            }
        }
        public bool IsBaseHolder()
        {
            return !GetType().IsSubclassOf(typeof(CogHolder));
        }
        private bool evaluateIDs(Cog cog)
        {
            return true;
            //return !GetType().IsSubclassOf(typeof(CogHolder));
            //return cog is not null && !string.IsNullOrEmpty(LinkID) && !string.IsNullOrEmpty(cog.LinkID) && cog.LinkID == LinkID;
        }
        public override void Render()
        {
            base.Render();
            MTexture tex = GFX.Game["objects/PuzzleIslandHelper/Cog/holder"];
            Vector2 offset = new Vector2(tex.Width / 2, tex.Height / 2);
            Draw.SpriteBatch.Draw(Stem.Texture.Texture_Safe, Position - Vector2.One * 4, null, Color.White,0 , Vector2.Zero, 1, SpriteEffects.None, 0);
            if (useFakeCogImage)
            {
                MTexture cog = GFX.Game["objects/PuzzleIslandHelper/Cog/fakeCog"];
                Vector2 cogOffset = new Vector2(cog.Width / 2, cog.Height / 2).ToInt();
                Draw.SpriteBatch.Draw(cog.Texture.Texture_Safe, Position - Vector2.One * 4 + cogOffset, null, Color.White, Rotation.ToRad(), cogOffset, 1, SpriteEffects.None, 0);
            }
        }
        public virtual void StopSpinning(bool drop = true)
        {
            if (drop)
            {
                DropCog = true;
            }
            StopSpin = true;
            timer = 0;
        }
        public virtual void StartSpinning()
        {
            DropCog = false;
            StopSpin = false;
        }
        public virtual IEnumerator WhileSpinning(Cog cog)
        {
            yield return null;
        }
        private IEnumerator SlotRoutine(Cog cog)
        {
            if (useFakeCogImage) yield break;
            cog?.OnEnterSlot();
            StartSpinning();
            float lerp = 0;
            Coroutine whileSpinning = new Coroutine(WhileSpinning(cog));
            if (!DropCog) Add(whileSpinning);
            while (cog != null && cog.InSlot && !StopSpin && HasTime)
            {
                timer += Engine.DeltaTime;
                Spinning = true;
                Rotation += RotateRate * lerp;
                lerp = Calc.Min(1, lerp + Engine.DeltaTime);
                yield return null;
            }
            if (!whileSpinning.Finished) whileSpinning.Cancel();
            Spinning = false;
            yield return WindBack(cog, DropCog);
        }
        public void LockCog(Cog cog)
        {
            if (Scene is null || PianoModule.SaveData.CogData.HasHolder(EntityID)) return; //if holder already has a cog locked in place, return
            PianoModule.SaveData.CogData.AddCog(cog);
            PianoModule.SaveData.ContinuousCogIDs.Remove(cog.ContinuityID);
            UsedOnce = true;
            Scene.Remove(cog);
            SwitchToFake();
        }
        public void SwitchToFake()
        {
            Spinning = false;
            useFakeCogImage = true;
        }
        public IEnumerator WindBack(Cog cog, bool drop)
        {
            float time = 0.3f;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                Rotation += RotateRate * (1 - i);
                yield return null;
            }
            Add(new Coroutine(easeBack(1.4f, Ease.SineInOut)));
            if (IsBaseHolder() && cog is not null && cog.InSlot && !UsedOnce)
            {
                LockCog(cog);
            }
            else if (drop)
            {
                timerActive = true;
                if (cog is not null)
                {
                    cog.DropFromSlot();
                    cog.InSlot = false;
                }
            }
            UsedOnce = true;
            InCogRoutine = false;
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

        public override void Update()
        {
            base.Update();
            if (OnlyOnce && UsedOnce)
            {
                Color = Color.Lerp(Color, Color.Gray, Engine.DeltaTime);
            }
            Rotation = (float)Math.Round(Rotation);
            Rotations = Rotation / 360;

            if (timerActive)
            {
                if (cannotHoldTimer < WaitTime && !ForceCanHold)
                {
                    cannotHoldTimer += Engine.DeltaTime;
                    return;
                }
                else
                {
                    ForceCanHold = false;
                    cannotHoldTimer = 0;
                    timerActive = false;
                }
            }
        }
        public void StartRoutine(Cog cog)
        {
            if (cog is null) return;
            InCogRoutine = true;
            DropCog = false;
            cog.InSlot = true;
            Add(new Coroutine(SlotRoutine(cog)));
        }
        public bool CanUseCog(Cog cog)
        {
            return cog is not null && !InCogRoutine && !cog.Hold.IsHeld && (!OnlyOnce || (OnlyOnce && !UsedOnce)) &&
                   !(timerActive && cannotHoldTimer < WaitTime) &&
                  InReach(cog, 8);
        }
        public bool InReach(Cog cog, float distance)
        {
            float result = Vector2.Distance(Center, cog.Center);
            return result <= distance;
        }
    }
}