using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using System.Linq;
using Celeste.Mod.FancyTileEntities;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Calidus")]
    [Tracked]
    public class Calidus : Actor
    {
        public enum Mood
        {
            Happy,
            Stern,
            Normal,
            RollEye,
            Laughing,
            Shakers,
            Nodders,
            Closed,
            Angry,
            Surprised,
            Wink,
            Eugh
        }

        public enum Looking
        {
            Left,
            Right,
            Up,
            Down,
            UpRight,
            UpLeft,
            DownRight,
            DownLeft,
            Center,
            Player
        }
        public Looking LookDir = Looking.Center;
        public Mood CurrentMood = Mood.Normal;
        private int ColorTweenLoops = 2;
        private int ArmLoopCount;
        private int ArmBuffer = 60;
        private int heeheeBuffer = 2;
        public const float FloatHeight = 6;
        public float LookSpeed = 1;

        public Part BrokenParts;
        public Part OrbSprite;
        public Part EyeSprite;
        public Sprite Symbols;
        public Part[] Arms = new Part[2];
        private Player player;
        private ParticleSystem system;

        public Vector2 OrigPosition;
        public Vector2 Scale = Vector2.One;
        private Vector2 EyeScale = Vector2.One;
        public Vector2 LookTarget;
        public Vector2 EyeOffset;
        private Vector2 StarPos;
        private float StarRotation;
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
        public bool Talkable;

        public List<Part> Parts = new();
        private MTexture Star;
        public TalkComponent Talk;

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
        public void FloatTo(Vector2 position, float time, Ease.Easer ease = null)
        {
            Add(new Coroutine(FloatToRoutine(position, time, ease)));
        }
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
            public Part(Atlas atlas, string path) : base(atlas, path)
            {
            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                CenterOrigin();
                Position += new Vector2(Width / 2, Height / 2);
            }
            public void DrawOutline()
            {
                DrawOutline(Color.Black * OutlineOpacity);
            }
            public override void Render()
            {
                if (Texture != null)
                {
                    Texture.Draw(RenderPosition + Offset, Origin, Color, Scale, Rotation, Effects);
                }
            }
            public void Fall(float distanceToFloor)
            {
                Assembled = false;
                Entity?.Add(new Coroutine(FallRoutine(distanceToFloor)));
            }
            public void Return()
            {
                Entity?.Add(new Coroutine(ReturnRoutine()));
            }
            private IEnumerator FallRoutine(float distanceToFloor)
            {
                distanceToFloor += FallOffset;
                Assembled = false;
                OnGround = false;
                float orig = Y + Height;
                float ySpeed = FallSpeed;
                float rot = RotationRate.ToRad();
                OrigPosition = Position;
                int sign = 1;
                int bouncesLeft = 2;
                while (true)
                {
                    Y += ySpeed * Engine.DeltaTime;
                    float num = (Math.Abs(ySpeed) < 40f ? 0.5f : 1f);
                    ySpeed = Calc.Approach(ySpeed, 160f, 450f * num * Engine.DeltaTime);
                    Rotation += rot * num * sign;
                    if (Y >= orig + distanceToFloor)
                    {
                        if (bouncesLeft > 0)
                        {
                            ySpeed = -32 * (bouncesLeft / 2f);
                            bouncesLeft--;
                        }
                        else break;
                    }
                    yield return null;
                }
                Y = orig + distanceToFloor;
                OnGround = true;
            }
            public IEnumerator ReturnRoutine()
            {
                OnGround = true;
                Vector2 p = Position;
                int increment = 1;
                for (int i = 0; i < 8; i++)
                {
                    Position.X += increment;
                    increment = -increment;
                    yield return Engine.DeltaTime * 2;
                }
                Position = p;
                yield return ReturnDelay;
                Vector2 from = Position;
                float rotFrom = Rotation;
                OnGround = false;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    Position = Vector2.Lerp(from, OrigPosition, Ease.Follow(Ease.CubeIn, Ease.ElasticOut)(i));
                    Rotation = Calc.LerpClamp(rotFrom, 0, Ease.CubeIn(i + 0.1f));
                    yield return null;
                }
                Assembled = true;
                Reset();
            }
            public void Reset()
            {
                OnGround = false;
                Assembled = true;
                Rotation = 0;
                Position = OrigPosition;
            }
        }
        public Calidus(EntityData data, Vector2 offset) : this(data.Position + offset, data.Bool("broken"), data.Bool("startFloating", true), data.Enum("looking", Looking.Center), data.Enum("mood", Mood.Normal))
        {
        }
        public Calidus(Vector2 Position, bool broken = false, bool startFloating = true, Looking look = Looking.Center, Mood mood = Mood.Normal) : base(Position)
        {
            LookDir = look;
            CanFloat = startFloating;
            Broken = broken;
            OutlineOpacity = broken ? 0 : 1;
            string path = "characters/PuzzleIslandHelper/Calidus/";
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
            Symbols = new Sprite(GFX.Game, path)
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

            Add(BrokenParts, OrbSprite, Symbols, Arms[0], Arms[1], EyeSprite);
            CurrentMood = mood;
            Emotion(mood);
        }
        public void Interact(Player player)
        {
            Scene.Add(new CalidusCutscene(CalidusCutscene.Cutscenes.SecondA));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!Broken)
            {
                Fixed();
            }
            else
            {
                BrokenParts.Play("broken");
                Position -= new Vector2(BrokenParts.Width / 2, BrokenParts.Height / 2);
                Collider = new Hitbox(BrokenParts.Width, BrokenParts.Height);
            }
            OrigPosition = Position;
            if ((scene as Level).Session.Level == "digiRuinsLabB3")
            {
                Add(Talk = new TalkComponent(new Rectangle((int)Collider.Position.X, 0, (int)Width, (int)Height + 24), OrbSprite.Center.XComp(), Interact));
            }
            (scene as Level).Add(system = new ParticleSystem(Depth + 1, 500));
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = (scene as Level).Tracker.GetEntity<Player>();
            foreach (Part part in Components.GetAll<Part>())
            {
                Parts.Add(part);
            }
            Parts.Remove(EyeSprite);
        }
        public override void Update()
        {
            if (Talk != null)
            {
                if (CalidusCutscene.GetCutsceneFlag(Scene, CalidusCutscene.Cutscenes.Second))
                {
                    Talk.Enabled = !CalidusCutscene.GetCutsceneFlag(Scene, CalidusCutscene.Cutscenes.SecondA);
                }
                else
                {
                    Talk.Enabled = false;
                }
            }

            Vector2 shakeVector = Vector2.Zero;
            if (Shaking)
            {
                shakeTimer -= Engine.DeltaTime;
                shakeVector = Calc.Random.ShakeVector();
            }
            FloatAmount = Calc.Approach(FloatAmount, CanFloat ? FloatTarget : 0, Engine.DeltaTime * 2);
            if (CanFloat)
            {
                foreach (Part part in Parts)
                {
                    part.Offset.Y = -FloatAmount;
                }
                EyeSprite.Offset.Y = -FloatAmount;
                Arms[0].Offset.X = ArmOffsets[0];
                Arms[1].Offset.X = ArmOffsets[1];

            }
            BrokenParts.Visible = Broken;
            BrokenParts.OutlineOpacity = OutlineOpacity;
            Position += shakeVector;
            base.Update();
            Position -= shakeVector;

            if (Broken) return; //////////////////////////////////

            if (LookDir == Looking.Player)
            {
                LookTargetEnabled = true;
                LookTarget = player.Center;
            }
            StarRotation = RenderStar ? 0 : StarRotation + Engine.DeltaTime * 3;
            Arms[0].Scale = Arms[1].Scale = OrbSprite.Scale = Scale;
            EyeSprite.Scale = EyeScale;
            Vector2 centerOffset = GetLookOffset();
            if (!EyeExpressionOverride)
            {
                EyeOffset = EyeInstant ? centerOffset : Calc.Approach(EyeOffset, centerOffset, LookSpeed);
            }
            EyeSprite.Color = Blinking || ForceBlink ? Color.Lerp(Color.White, Color.Black, 0.3f) : Color.White;
            EyeSprite.RenderPosition = Calc.Approach(EyeSprite.RenderPosition, OrbSprite.RenderPosition + EyeOffset, LookSpeed);
        }
        private Vector2 GetLookOffset()
        {
            return LookDir switch
            {
                Looking.Left => -Vector2.UnitX * 4,
                Looking.Right => Vector2.UnitX * 4,
                Looking.Up => -Vector2.UnitY * 4,
                Looking.Down => Vector2.UnitY * 4,
                Looking.UpLeft => Vector2.One * -4,
                Looking.UpRight => new Vector2(4, -4),
                Looking.DownLeft => new Vector2(-4, 4),
                Looking.DownRight => Vector2.One * 4,
                Looking.Player => RotatePoint(OrbSprite.Center.XComp(), Vector2.Zero, Calc.Angle(Center, LookTarget).ToDeg()),
                Looking.Center => Vector2.Zero,
                _ => Vector2.Zero
            };
        }
        public override void Render()
        {
            foreach (Part p in Parts)
            {
                if (p.Visible)
                {
                    p.DrawOutline();
                }
            }
            if (Symbols.Visible)
            {
                Symbols.DrawSimpleOutline();
            }
            EyeSprite.DrawOutline();
            base.Render();
            if (RenderStar)
            {
                Star.Draw(StarPos, Star.Center, Color.Yellow, Vector2.One, StarRotation);
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
        public bool FallenApart;
        public void FallApart()
        {
            CanFloat = false;
            foreach (Part p in Parts)
            {
                p.Fall(12 + FloatTarget - Height);
            }
        }
        public IEnumerator FallApartRoutine()
        {
            FallApart();
            Add(new Coroutine(BlinkInterval(3, Engine.DeltaTime * 2, true)));
            while (!AllPartsFallen())
            {
                yield return null;
            }
            FallenApart = true;
            yield return null;
        }
        public void Reassemble()
        {
            Add(new Coroutine(ReassembleRoutine()));
        }
        public IEnumerator ReassembleRoutine()
        {
            Look(Looking.Center);
            ReturnParts();
            while (!AllPartsAssembled())
            {
                yield return null;
            }
            FallenApart = false;
            CanFloat = true;
            EyeExpressionOverride = false;
            ForceBlink = false;
        }
        public IEnumerator WaitForReassemble()
        {
            while (FallenApart)
            {
                yield return null;
            }
        }
        public IEnumerator WaitForFallenApart()
        {
            while (!FallenApart)
            {
                yield return null;
            }
        }
        public void ReturnParts()
        {
            foreach (Part part in Parts)
            {
                part.Return();
            }
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
                Add(Outline);
                Outline.Start();
            }
        }
        public void FixSequence(bool instant = false)
        {
            if (instant)
            {
                Fixed();
                Position += new Vector2(BrokenParts.Width / 2, BrokenParts.Height / 2);
                Broken = false;
                LerpOutline(true);
            }
            else
            {
                BrokenParts.Play("broken");
                Part Glint = new Part(GFX.Game, "characters/PuzzleIslandHelper/Calidus/");
                Glint.Add("shine", "brokenShine", 0.05f);
                Glint.Add("gleam", "brokenGleam", 0.1f, "shine");
                Add(Glint);

                Glint.Play("gleam");
                Glint.OnLastFrame = (string s) =>
                {
                    if (s == "shine")
                    {
                        BrokenParts.Play("assemble");
                        LerpOutline();
                        BrokenParts.OnLastFrame = (string s) =>
                        {
                            if (s == "assemble")
                            {
                                Fixed();
                                Position += new Vector2(BrokenParts.Width / 2, BrokenParts.Height / 2);
                                Broken = false;
                            }
                        };
                        Glint.Stop();
                        Remove(Glint);
                    }
                };
            }
        }
        private void Fixed()
        {
            bool fromBroken = BrokenParts.Visible;
            BrokenParts.Visible = false;
            OrbSprite.Play("idle");
            EyeSprite.Play("neutral");
            Arms[0].Play("idle");
            Arms[1].Play("idle");
            Arms[0].OnLastFrame = (string s) =>
            {
                if (!CanFloat) return;
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
            };
            EyeSprite.Position += OrbSprite.Center;
            Arms[0].Position.X += -(Arms[0].Width + 1);
            Arms[1].Position.X += OrbSprite.Width;
            Collider = new Hitbox(Arms[0].Width * 2 + 3 + OrbSprite.Width, OrbSprite.Height, Arms[0].X - 5);
            Position -= new Vector2(6, 4);
            Add(new Coroutine(AddRoutines(fromBroken)));
        }
        private IEnumerator AddRoutines(bool fromBroken)
        {
            if (fromBroken)
            {
                float y = Position.Y;
                for (float i = 0; i < 1; i += Engine.DeltaTime * 2)
                {
                    MoveTowardsY(y - 8, 8 * Engine.DeltaTime * 2);
                    yield return null;
                }
            }
            Alarm alarm = Alarm.Create(Alarm.AlarmMode.Looping,
                delegate { if (CanFloat) Add(new Coroutine(ColorFlash())); }, 2.5f, true);
            Add(alarm);
            Add(new Coroutine(FloatLoop()));
            Add(new Coroutine(ArmLoop()));
            Add(new Coroutine(Blink()));
            Add(new Coroutine(DuckBounceCheck()));
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
            Add(new Coroutine(EughRoutine()));
        }
        private IEnumerator EughRoutine()
        {

            EyeSprite.Play("stern");
            yield return 0.1f;
            Symbols.Visible = true;
            Symbols.Play("eugh");
            while (CurrentMood == Mood.Eugh || !Continue)
            {
                yield return null;
            }
            Symbols.Visible = false;
            yield return null;
        }
        public void Wink()
        {
            CurrentMood = Mood.Wink;
            Add(new Coroutine(WinkRoutine()));
        }
        private IEnumerator WinkRoutine()
        {
            EyeSprite.Play("wink");

            RenderStar = true;
            int x = 10;
            int y = 8;
            StarPos = EyeSprite.Position + Position + Vector2.UnitX * 8;

            Vector2 start = StarPos;
            float speed = 4;
            for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
            {
                StarPos.X = Calc.LerpClamp(start.X, start.X + x / 2, i);
                StarPos.Y = Calc.LerpClamp(start.Y, start.Y - y / 2, Ease.SineInOut(i));
                yield return null;
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
            {
                StarPos.X = Calc.LerpClamp(start.X + x / 2, start.X + x, i);
                StarPos.Y = Calc.LerpClamp(start.Y - y / 2, start.Y + y, Ease.SineInOut(i));
                yield return null;
            }
            RenderStar = false;
            yield return null;
        }
        public void Surprised(bool useSymbol = true)
        {
            CurrentMood = Mood.Surprised;
            Symbols.Position = OrbSprite.Position + new Vector2(4, -11);
            Add(new Coroutine(SurprisedRoutine(useSymbol)));
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
            Add(new Coroutine(AngryRoutine()));
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
            Add(new Coroutine(ShakeHeadRoutine(false)));
        }
        public void NodHead()
        {
            CurrentMood = Mood.Nodders;
            Add(new Coroutine(ShakeHeadRoutine(true)));
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
            Add(new Coroutine(RollEyeRoutine()));
        }

        public void Laugh()
        {
            CurrentMood = Mood.Laughing;
            Add(new Coroutine(LaughRoutine()));
        }
        private IEnumerator LaughRoutine()
        {
            EyeSprite.Play("happy");
            float delay = 0.1f;
            while (CurrentMood == Mood.Laughing && !EyeExpressionOverride)
            {
                HeeHeeParticles();
                EyeOffset.Y = -2;
                yield return delay;
                EyeOffset.Y = 0;
                yield return delay;
            }
            yield return null;
        }

        public IEnumerator RollEyeRoutine()
        {
            EyeExpressionOverride = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Vector2 Rotated = RotatePoint(Vector2.Zero, EyeSprite.Center, Ease.SineOut(i) * 90);
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

        public bool ForceShake;
        public bool Shaking => ForceShake || shakeTimer > 0;
        private float shakeTimer;
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

        #region ///////////////////////////////// Finished \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
        private void HeeHeeParticles()
        {
            if (heeheeBuffer <= 0)
            {
                system.Emit(HeeHee, 1, TopCenter - Vector2.UnitY * 8, Vector2.UnitX * 4);
                heeheeBuffer = 1;
            }
            else
            {
                heeheeBuffer--;
            }
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
                (int)Mood.Laughing => Laugh,
                (int)Mood.Shakers => ShakeHead,
                (int)Mood.Nodders => NodHead,
                (int)Mood.Closed => CloseEye,
                (int)Mood.Angry => Angry,
                (int)Mood.Surprised => delegate { Surprised(false); }
                ,
                (int)Mood.Wink => Wink,
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
        public bool ForceBlink;
        public bool Blinking;
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
        private IEnumerator Testing()
        {

            /*            for (int i = 0; i < 3; i++)
                        {
                            Emotion(i);
                            yield return 1;
                            LookDir = Looking.Left;
                            yield return 1;
                            LookDir = Looking.Up;
                            yield return 1;
                            LookDir = Looking.Right;
                            yield return 1;
                            LookDir = Looking.Down;
                            yield return 1;
                            LookDir = Looking.Center;
                            yield return 1;
                        }*/
            yield return null;
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

                OrbSprite.RenderPosition += Vector2.UnitY * offset;
                EyeOffset.Y = 4;
                Arms[0].RenderPosition += Vector2.UnitY * offset;
                Arms[1].RenderPosition += Vector2.UnitY * offset;
                yield return 0.1f;
                Scale = Vector2.One;
                OrbSprite.RenderPosition -= Vector2.UnitY * offset;
                EyeOffset.Y = 0;
                Arms[0].RenderPosition -= Vector2.UnitY * offset;
                Arms[1].RenderPosition -= Vector2.UnitY * offset;
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
        public IEnumerator MoveToPlayerX(Player player, float offset = 0, bool updateTarget = true)
        {
            float target = player.Position.X + OrbSprite.Position.X + offset;

            while (Position.X != target)
            {
                if (updateTarget) target = player.Position.X + OrbSprite.Position.X + offset;
                Position.X = Calc.Approach(Position.X, target, 1);
                yield return null;
            }
        }
        public IEnumerator MoveToPlayerY(Player player, float offset = 0, bool updateTarget = true)
        {
            float target = player.Position.Y + offset;
            while (Position.Y != target)
            {
                if (updateTarget) target = player.Position.Y + offset;
                Position.Y = Calc.Approach(Position.Y, target, 1);
                yield return null;
            }
        }
        private IEnumerator FloatLoop()
        {
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
        private float[] ArmOffsets = new float[2];
        private IEnumerator ArmLoop()
        {
            ArmOffsets[0] = ArmOffsets[1] = 0;
            while (true)
            {
                if (CanFloat)
                {
                    Color target = Color.Lerp(Color.White, Color.Black, 0.2f);
                    float armPosX = ArmOffsets[0];
                    float armRightPosX = ArmOffsets[1];
                    float delay = Engine.DeltaTime / 2;
                    int armDistance = 1;
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        ArmOffsets[0] = (int)Math.Round(Calc.LerpClamp(armPosX, armPosX - armDistance, Ease.SineInOut(i)));
                        ArmOffsets[1] = (int)Math.Round(Calc.LerpClamp(armRightPosX, armRightPosX + armDistance, Ease.SineInOut(i)));
                        OrbSprite.Color = Color.Lerp(target, Color.White, Ease.SineInOut(i));
                        yield return delay;
                    }
                    armPosX = ArmOffsets[0];
                    armRightPosX = ArmOffsets[1];
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        ArmOffsets[0] = (int)Math.Round(Calc.LerpClamp(armPosX, armPosX + armDistance, Ease.SineInOut(i)));
                        ArmOffsets[1] = (int)Math.Round(Calc.LerpClamp(armRightPosX, armRightPosX - armDistance, Ease.SineInOut(i)));
                        OrbSprite.Color = Color.Lerp(Color.White, target, Ease.SineInOut(i));
                        yield return delay;
                    }
                }
                yield return null;
            }
        }
        #endregion
        static Vector2 RotatePoint(Vector2 pointToRotate, Vector2 centerPoint, double angleInDegrees)
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
