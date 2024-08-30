using Celeste.Mod.Core;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Looking = Celeste.Mod.PuzzleIslandHelper.Entities.Calidus.Looking;
using Mood = Celeste.Mod.PuzzleIslandHelper.Entities.Calidus.Mood;
using Part = Celeste.Mod.PuzzleIslandHelper.Entities.Calidus.Part;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class CalidusSprite : Sprite
    {
        public Part OrbSprite;
        public Part EyeSprite;
        public Part[] Arms = new Part[2];
        public Part BrokenParts;
        public Part Symbols;

        public Looking LookDir;
        public Mood CurrentMood;
        public bool ForceBlink;
        public bool Blinking;
        private float[] ArmOffsets = new float[2];
        public const string PartPath = "characters/PuzzleIslandHelper/Calidus/";
        private int ColorTweenLoops = 2;
        private int ArmLoopCount;
        private int ArmBuffer = 60;
        private int heeheeBuffer = 2;
        public float FloatHeight = 6;
        public float LookSpeed = 1;
        public bool HasHead;
        public bool HasEye;
        public bool HasArms;


        public float RollRotation;
        private Player player;
        private ParticleSystem system;

        public Vector2 OrigPosition;
        public Vector2 LookTarget;
        public Vector2 EyeOffset;
        private float OutlineOpacity;
        public float FloatTarget;
        public float FloatAmount;
        public bool Continue;
        public bool EyeInstant;
        public bool LookTargetEnabled;
        public bool CanFloat = true;
        public bool EyeExpressionOverride;
        private bool RenderStar;
        public bool Broken;
        public float ColliderWidth => OrbSprite.Width;
        public float ColliderHeight => OrbSprite.Height;
        public Collider SpriteBox;

        public List<Part> Parts = new();
        private MTexture Star;
        private ParticleType HeeHee = new ParticleType
        {
            Source = GFX.Game["objects/PuzzleIslandHelper/particles/heehee00"],
            Size = 1f,
            Color = Color.Gray,
            Color2 = Color.White,
            ColorMode = ParticleType.ColorModes.Choose,
            LifeMin = 1f,
            LifeMax = 1f,
            SpeedMin = 12,
            SpeedMax = 12,
            Direction = Calc.Up,
            DirectionRange = 45f.ToRad(),
            FadeMode = ParticleType.FadeModes.Linear,
        };
        public class Part : Sprite
        {
            public Vector2 OrigPosition;
            public float ReturnDelay;
            public bool Assembled = true;
            public float FallSpeed;
            public float RotationRate;
            public Vector2 Offset;
            public float OutlineOpacity = 1;
            public bool OnGround;
            public float FallOffset;
            public CalidusSprite Parent;
            public bool UseOwnScale;
            public Vector2 OutlineOffset;
            public Vector2 SpriteScale;
            public new Vector2 RenderPosition
            {
                get
                {
                    return ((Parent == null) ? Vector2.Zero : Parent.RenderPosition) + Position;
                }
                set
                {
                    Position = value - ((Parent == null) ? Vector2.Zero : Parent.RenderPosition);
                }
            }
            public Part(Atlas atlas, string path) : base(atlas, path)
            {

            }

            public override void Added(Entity entity)
            {
                base.Added(entity);
                CenterOrigin();
                Position += new Vector2(Width / 2, Height / 2).Floor();
            }
            public void DrawOutlineAt(Vector2 position)
            {
                DrawOutlineAt(position + Offset, Color.Black * OutlineOpacity);
            }
            public void DrawOutlineAt(Vector2 position, Color color, int offset = 1)
            {
                Color color2 = Color;
                Color = color;
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        if (i != 0 || j != 0)
                        {
                            RenderAt(position + new Vector2(i * offset, j * offset) + OutlineOffset);
                        }
                    }
                }
                Color = color2;
            }
            public void RenderAt(Vector2 at)
            {
                if (Texture != null)
                {
                    Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, at + Offset, null, Color, Rotation, Origin, UseOwnScale ? Scale : SpriteScale, Effects, 0);
                }
            }
            public override void Render()
            {
            }
        }

        public void UpdateEye(Vector2 lookOffset)
        {
            if (!EyeExpressionOverride && HasHead)
            {
                EyeOffset = EyeInstant ? lookOffset : Calc.Approach(EyeOffset, lookOffset, LookSpeed);
            }
            EyeSprite.Color = Blinking || ForceBlink ? Color.Lerp(Color.White, Color.Black, 0.3f) : Color.White;
            EyeSprite.Position = Calc.Approach(EyeSprite.Position, OrbSprite.Position + EyeOffset, LookSpeed);
        }
        public CalidusSprite(Vector2 position, bool broken = false, bool startFloating = true, Looking look = Looking.Center, Mood mood = Mood.Normal) : base(GFX.Game, PartPath)
        {
            Position = position;
            LookDir = look;
            CanFloat = startFloating;
            Broken = broken;
            OutlineOpacity = broken ? 0 : 1;
            string path = PartPath;
            OrbSprite = new Part(GFX.Game, path)
            {
                FallSpeed = -48,
                RotationRate = 8,
                Visible = !Broken,
                ReturnDelay = 0.3f
            };
            EyeSprite = new Part(GFX.Game, path)
            {
                Visible = !Broken
            };
            Symbols = new Part(GFX.Game, path)
            {
                Visible = !Broken,
            };
            Arms[0] = new Part(GFX.Game, path)
            {
                RotationRate = -5,
                FallSpeed = -32,
                Visible = !Broken,
                ReturnDelay = 0,
                FallOffset = 8
            };
            Arms[1] = new Part(GFX.Game, path)
            {
                RotationRate = 6,
                FallSpeed = -32,
                Visible = !Broken,
                ReturnDelay = 0.6f,
                FallOffset = 8
            };
            BrokenParts = new Part(GFX.Game, path)
            {
                Color = Color.Lerp(Color.Black, Color.White, 0.5f),
                Visible = Broken
            };
            Star = GFX.Game[path + "star00"];
            Symbols.AddLoop("exclamation", "surprisedSymbol", 0.1f);
            Symbols.AddLoop("anger", "anger", 0.1f);
            Symbols.AddLoop("eughIdle", "eughSymbol", 0.1f, 3);
            Symbols.Add("eugh", "eughSymbol", 0.2f);
            OrbSprite.AddLoop("idle", "orbIdle", 0.1f);
            EyeSprite.AddLoop("neutral", "eyeFront", 0.1f);
            EyeSprite.AddLoop("happy", "eyeHappy", 0.1f);
            EyeSprite.AddLoop("stern", "eyeStern", 0.1f);
            EyeSprite.AddLoop("closed", "eyeClosed", 0.1f);
            EyeSprite.AddLoop("surprised", "eyeSurprised", 0.1f);
            EyeSprite.AddLoop("wink", "eyeWink", 0.1f);
            for (int i = 0; i < 2; i++)
            {
                Arms[i].AddLoop("idle", "armSpinH", 0.1f, 0);
                Arms[i].Add("spinH", "armSpinH", 0.07f, "idle");
                Arms[i].Add("spinV", "armSpinV", 0.1f, "idle");
                Arms[i].FlipX = i == 1;
            }
            Symbols.Visible = false;
            BrokenParts.AddLoop("broken", "formation", 0.1f, 0);
            BrokenParts.AddLoop("jitter", "jitter", 0.05f);
            BrokenParts.Add("assemble", "formation", 0.12f);
            CurrentMood = mood;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            entity.Add(BrokenParts, OrbSprite, Symbols, Arms[0], Arms[1], EyeSprite);
            Parts = new()
            {
                BrokenParts, OrbSprite,Symbols,Arms[0],Arms[1],EyeSprite
            };
            Emotion(CurrentMood);
        }
        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            entity.Remove(OrbSprite, Arms[0], Arms[1], EyeSprite);
        }
        public override void Render()
        {
            base.Render();
            RenderAt(RenderPosition);
        }
        public void RenderAt(Vector2 position)
        {
            foreach (Part p in Parts)
            {
                if (p.Visible)
                {
                    p.DrawOutlineAt(position + p.Position);
                }
            }
            foreach (Part p in Parts)
            {
                if (p.Visible)
                {
                    p.RenderAt(position + p.Position);
                }
            }
        }
        public void ThrowArmDown()
        {

        }
        public override void Update()
        {
            base.Update();
            OrbSprite.Visible = HasHead;
            EyeSprite.Visible = HasEye;
            Arms[0].Visible = Arms[1].Visible = HasArms;
            if (HasHead)
            {
                OrbSprite.Rotation = RollRotation.ToRad();
            }
            Arms[0].Offset.X = ArmOffsets[0];
            Arms[1].Offset.X = ArmOffsets[1];
            LookTargetEnabled = HasHead && LookDir == Looking.Target;
            foreach (Part p in Parts)
            {
                p.SpriteScale = Scale;
            }
        }
        private IEnumerator BlinkInterval(int times, float interval, bool endState)
        {
            for (int i = 0; i < times; i++)
            {
                ForceBlink = !ForceBlink;
                yield return interval;
            }
            ForceBlink = endState;
        }
        private IEnumerator BlinkIntervalRandom(int times, bool endState)
        {
            for (int i = 0; i < times; i++)
            {
                ForceBlink = !ForceBlink;
                yield return Calc.Random.Range(Engine.DeltaTime, Engine.DeltaTime * 8);
            }
            ForceBlink = endState;
        }
        private IEnumerator SpriteBlink(Part sprite, int times, float interval, bool endState)
        {
            Color prev = sprite.Color;
            bool off = false;
            for (int i = 0; i < times; i++)
            {
                sprite.Color = Color.Lerp(prev, Color.Black, off ? 0.3f : 0);
                yield return interval;
            }
            sprite.Color = Color.Lerp(prev, Color.Black, endState ? 0.3f : 0);
        }
        public void LerpOutline(bool instant = false)
        {
            if (instant)
            {
                BrokenParts.Color = Color.White;
                OutlineOpacity = 1;
            }
            else
            {
                Tween Outline = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, 1);
                Outline.OnUpdate = (Tween t) =>
                {
                    BrokenParts.Color = Color.Lerp(Color.Black, Color.White, 0.5f + (t.Eased / 2));
                    OutlineOpacity = t.Eased;
                };
                Outline.OnComplete = delegate { BrokenParts.Color = Color.White; };
                Entity?.Add(Outline);
                Outline.Start();
            }
        }
        public bool AutoAnimateArms = true;
        public void Fixed()
        {
            BrokenParts.Visible = false;
            OrbSprite.Play("idle");
            EyeSprite.Play("neutral");
            Arms[0].Play("idle");
            Arms[1].Play("idle");
            Arms[0].OnLastFrame = (string s) =>
            {
                if (AutoAnimateArms)
                {
                    if (s == "idle")
                    {
                        ArmLoopCount++;
                    }
                    else if (s == "spinH" || s == "spinV")
                    {
                        Arms[1].Play(Calc.Random.Chance(0.5f) ? "spinH" : "spinV");
                    }
                    if (ArmLoopCount == ArmBuffer)
                    {
                        Arms[0].Play(Calc.Random.Chance(0.5f) ? "spinH" : "spinV");
                        ArmLoopCount = 0;
                    }
                }
            };
            OrbSprite.Position -= Vector2.One * 2;
            EyeSprite.Position += OrbSprite.Center;
            int armOffset = 4;
            Arms[0].Position = OrbSprite.Position - Vector2.UnitX * (Arms[0].Width + armOffset);
            Arms[1].Position = OrbSprite.Position + Vector2.UnitX * (OrbSprite.Width / 2 + armOffset - 1);
            SpriteBox = new Hitbox(Arms[0].Width * 2 + 3 + OrbSprite.Width, OrbSprite.Height, Arms[0].X - 5);
        }
        public void AddRoutines(bool addDuckCheck = true)
        {
            Alarm alarm = Alarm.Create(Alarm.AlarmMode.Looping,
                delegate { Entity?.Add(new Coroutine(ColorFlash())); }, 2.5f, true);
            Entity?.Add(alarm);
            Entity?.Add(new Coroutine(FloatLoop()));
            Entity?.Add(new Coroutine(ArmLoop()));
            Entity?.Add(new Coroutine(Blink()));
            if (addDuckCheck) Entity?.Add(new Coroutine(DuckBounceCheck()));

        }
        private void PlayerFace(Player player)
        {
            player.Facing = Position.X > player.Position.X ? Facings.Right : Facings.Left;
        }
        public IEnumerator Say(string id, string emotion, params Func<IEnumerator>[] events)
        {
            if (Broken)
            {
                yield break;
            }
            Emotion(emotion);
            if (player is not null)
            {
                PlayerFace(player);
            }
            yield return Textbox.Say(id, events);
        }
        public IEnumerator Say(string id, string emotion, bool useSymbol, params Func<IEnumerator>[] events)
        {
            if (Broken)
            {
                yield break;
            }
            Emotion(emotion);
            if (player is not null)
            {
                PlayerFace(player);
            }
            yield return Textbox.Say(id, events);
        }
        public IEnumerator Test()
        {
            yield return null;
        }
        public void Eugh()
        {
            CurrentMood = Mood.Eugh;
            Symbols.Position = OrbSprite.Position + Vector2.One;
            Entity?.Add(new Coroutine(EughRoutine()));
        }
        private IEnumerator EughRoutine()
        {

            EyeSprite.Play("stern");
            yield return 0.1f;
            Symbols.Visible = true;
            Symbols.Play("eugh");
            while (CurrentMood == Mood.Eugh || !Continue)
            {
                Symbols.Position = OrbSprite.Position + Vector2.One;
                yield return null;
            }
            Symbols.Visible = false;
            yield return null;
        }
        public void Surprised(bool useSymbol = true)
        {
            CurrentMood = Mood.Surprised;
            Symbols.Position = OrbSprite.Position + new Vector2(4, -11);
            Entity?.Add(new Coroutine(SurprisedRoutine(useSymbol)));
        }
        private IEnumerator SurprisedRoutine(bool useSymbol)
        {
            if (useSymbol)
            {
                Symbols.Visible = true;
                Symbols.Play("exclamation");
            }
            else
            {
                Symbols.Stop();
            }

            EyeSprite.Play("surprised");
            yield return 2f;
            Symbols.Stop();
            Symbols.Visible = false;
            while (CurrentMood == Mood.Surprised)
            {
                yield return null;
            }
            yield return null;
        }
        public void Angry()
        {
            CurrentMood = Mood.Angry;
            Symbols.Position -= Vector2.One * 2;
            Entity?.Add(new Coroutine(AngryRoutine()));
        }
        private IEnumerator AngryRoutine()
        {
            EyeInstant = true;
            Symbols.Visible = true;
            Symbols.Play("anger");
            float delay = 0.05f;
            while (CurrentMood == Mood.Angry && !EyeExpressionOverride)
            {
                yield return 3;
                if (!(CurrentMood == Mood.Angry && !EyeExpressionOverride)) break;
                for (int i = 0; i < 2; i++)
                {
                    EyeOffset.X = -1;
                    if (!(CurrentMood == Mood.Angry && !EyeExpressionOverride))
                    {
                        EyeOffset.X = 0;
                        break;
                    }
                    yield return delay;
                    EyeOffset.X = 1;
                    yield return delay;
                }
                EyeOffset.X = 0;

            }
            EyeInstant = false;
            Symbols.Visible = false;
            yield return null;
        }
        public void CloseEye()
        {
            if (EyeSprite.CurrentAnimationID != "closed") EyeSprite.Play("closed");
            CurrentMood = Mood.Closed;
        }
        public void Happy()
        {
            if (EyeSprite.CurrentAnimationID != "happy") EyeSprite.Play("happy");
            Symbols.Visible = false;
            CurrentMood = Mood.Happy;
        }
        public void Stern()
        {
            if (EyeSprite.CurrentAnimationID != "stern") EyeSprite.Play("stern");
            Symbols.Visible = false;
            CurrentMood = Mood.Stern;
        }
        public void Normal()
        {
            if (EyeSprite.CurrentAnimationID != "neutral") EyeSprite.Play("neutral");
            Symbols.Visible = false;
            CurrentMood = Mood.Normal;
        }
        public void ShakeHead()
        {
            CurrentMood = Mood.Shakers;
            Entity?.Add(new Coroutine(ShakeHeadRoutine(false)));
        }
        public void NodHead()
        {
            CurrentMood = Mood.Nodders;
            Entity?.Add(new Coroutine(ShakeHeadRoutine(true)));
        }

        private IEnumerator ShakeHeadRoutine(bool Nod)
        {
            float speed = 7;
            float amount = 1.5f;
            bool prevLookState = LookTargetEnabled;
            EyeExpressionOverride = false;
            LookTargetEnabled = false;
            for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
            {
                if (Nod)
                {
                    EyeOffset.Y = Calc.LerpClamp(0, -amount, i);
                }
                else
                {
                    EyeOffset.X = Calc.LerpClamp(0, -amount, i);
                }
                yield return null;
            }
            for (int j = 0; j < 2; j++)
            {
                for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
                {
                    if (Nod)
                    {
                        EyeOffset.Y = Calc.LerpClamp(-amount, amount, i);
                    }
                    else
                    {
                        EyeOffset.X = Calc.LerpClamp(-amount, amount, i);
                    }
                    yield return null;
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
                {
                    if (Nod)
                    {
                        EyeOffset.Y = Calc.LerpClamp(amount, -amount, i);
                    }
                    else
                    {
                        EyeOffset.X = Calc.LerpClamp(amount, -amount, i);
                    }
                    yield return null;
                }
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
            {
                if (Nod)
                {
                    EyeOffset.Y = Calc.LerpClamp(EyeOffset.Y, 0, i);
                }
                else
                {
                    EyeOffset.X = Calc.LerpClamp(EyeOffset.X, 0, i);
                }
                yield return null;
            }
            while (Nod ? CurrentMood == Mood.Nodders : CurrentMood == Mood.Shakers)
            {
                yield return null;
            }
            LookTargetEnabled = prevLookState;
            yield return null;
        }

        public void RollEye()
        {
            CurrentMood = Mood.RollEye;
            Entity?.Add(new Coroutine(RollEyeRoutine()));
        }
        public IEnumerator RollEyeRoutine()
        {
            EyeExpressionOverride = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Vector2 Rotated = PlayerCalidus.RotatePoint(Vector2.Zero, EyeSprite.Center, Ease.SineOut(i) * 90);
                EyeSprite.Position = Calc.Approach(EyeSprite.Position, Rotated, LookSpeed);
                yield return null;
            }
            while (CurrentMood == Mood.RollEye)
            {
                yield return null;
            }
            EyeExpressionOverride = false;
            yield return null;
        }

        private IEnumerator WaitForEmotionChange(Mood Current)
        {
            while (CurrentMood == Current)
            {
                yield return null;
            }
            yield return null;

        }
        public void Emotion(int mood)
        {
            Action Action = mood switch
            {
                (int)Mood.Happy => Happy,
                (int)Mood.Stern => Stern,
                (int)Mood.Normal => Normal,
                (int)Mood.RollEye => RollEye,
                (int)Mood.Shakers => ShakeHead,
                (int)Mood.Nodders => NodHead,
                (int)Mood.Closed => CloseEye,
                (int)Mood.Angry => Angry,
                (int)Mood.Surprised => delegate { Surprised(false); }
                ,
                (int)Mood.Eugh => Eugh,
                _ => null
            };
            if (Action is not null)
            {
                Action.Invoke();
            }
        }

        public void Emotion(Mood mood)
        {
            Emotion((int)mood);
        }
        public void Emotion(string mood)
        {
            foreach (Mood @enum in Enum.GetValues(typeof(Mood)).Cast<Mood>())
            {
                string enumName = @enum.ToString();
                if (mood.Equals(enumName, StringComparison.OrdinalIgnoreCase))
                {
                    Emotion(@enum);
                    break;
                }
            }
        }
        public void Look(Looking dir)
        {
            LookDir = dir;
        }
        public void Look(string direction)
        {
            foreach (Looking @enum in Enum.GetValues(typeof(Looking)).Cast<Looking>())
            {
                string enumName = @enum.ToString();
                if (direction.Equals(enumName, StringComparison.OrdinalIgnoreCase))
                {
                    Look(@enum);
                    break;
                }
            }
        }
        private IEnumerator ColorFlash()
        {

            float delay = Engine.DeltaTime * 2;
            float[] values = new float[] { 0.2f, 0.7f, 0.2f, 0 };
            for (int j = 0; j < ColorTweenLoops; j++)
            {
                foreach (float f in values)
                {
                    Arms[0].Color = Color.Lerp(Color.White, Color.Green, f);
                    Arms[1].Color = Color.Lerp(Color.White, Color.Green, f);
                    yield return delay;
                }
            }
        }
        private IEnumerator Blink()
        {
            while (true)
            {
                yield return Calc.Random.Range(1, 10f);
                Blinking = true;
                EyeSprite.Color = Color.Lerp(Color.White, Color.Black, 0.3f);

                yield return 0.1f;
                Blinking = false;
                EyeSprite.Color = Color.White;
            }
        }
        private IEnumerator DuckDuck()
        {
            int offset = 5;
            float Lookspeed = LookSpeed;
            LookSpeed = offset;
            if (CurrentMood == Mood.Angry)
            {
                Symbols.Visible = false;
            }
            for (int i = 0; i < 15; i++)
            {

                Scale = new Vector2(1, 0.6f);

                OrbSprite.Position += Vector2.UnitY * offset;
                EyeOffset.Y = 4;
                Arms[0].Position += Vector2.UnitY * offset;
                Arms[1].Position += Vector2.UnitY * offset;
                yield return 0.1f;
                Scale = Vector2.One;
                OrbSprite.Position -= Vector2.UnitY * offset;
                EyeOffset.Y = 0;
                Arms[0].Position -= Vector2.UnitY * offset;
                Arms[1].Position -= Vector2.UnitY * offset;
                yield return 0.1f;
            }
            if (CurrentMood == Mood.Angry)
            {
                Symbols.Visible = true;
            }
            LookSpeed = Lookspeed;
            //Emotion(m);
            yield return null;
        }
        private IEnumerator DuckBounceCheck()
        {
            while (true)
            {
                if (player is null)
                {
                    player = Scene.GetPlayer();
                    yield return null;
                    continue;
                }
                int count = 0;
                bool wasUnducked = false;

                for (int i = 0; i < 300; i++)
                {
                    if (wasUnducked && player.Ducking)
                    {
                        count++;
                    }
                    wasUnducked = !player.Ducking;
                    if (count > 8)
                    {
                        yield return DuckDuck();
                        break;
                    }
                    yield return null;
                }
            }
        }
        private IEnumerator FloatLoop()
        {
            if (FloatHeight == 0) yield break;
            float percent = 0.5f;
            float time = 1;
            float to = FloatHeight;
            while (true)
            {
                for (float i = percent; i < 1; i += Engine.DeltaTime / time)
                {
                    while (!CanFloat)
                    {
                        yield return null;
                    }
                    FloatTarget = Calc.LerpClamp(-to, to, Ease.SineInOut(i));
                    yield return null;
                }
                to = -to;
                percent = 0;
                time = 2;
            }
        }
        private IEnumerator ArmLoop()
        {
            ArmOffsets[0] = ArmOffsets[1] = 0;
            while (true)
            {
                if (CanFloat)
                {
                    Color target = Color.Lerp(Color.White, Color.Black, 0.2f);
                    float delay = Engine.DeltaTime / 2;

                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        OrbSprite.Color = Color.Lerp(target, Color.White, Ease.SineInOut(i));
                        yield return delay;
                    }
                    ArmOffsets[0] = -1;
                    Arms[0].OutlineOffset.X = 1;
                    ArmOffsets[1] = 1;
                    Arms[1].OutlineOffset.X = -1;
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        OrbSprite.Color = Color.Lerp(Color.White, target, Ease.SineInOut(i));
                        yield return delay;
                    }
                    ArmOffsets[0] = ArmOffsets[1] = 0;
                    Arms[0].OutlineOffset.X = Arms[1].OutlineOffset.X = 0;
                }
                yield return null;
            }
        }

        public bool AllPartsAssembled()
        {

            foreach (Part p in Parts)
            {
                if (!p.Assembled) return false;
            }
            return true;
        }
        public bool AllPartsFallen()
        {
            foreach (Part p in Parts)
            {
                if (!p.OnGround) return false;
            }
            return true;
        }
    }
}