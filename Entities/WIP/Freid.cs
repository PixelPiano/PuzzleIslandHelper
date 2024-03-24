using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/Freid")]
    [Tracked]
    public class Freid : Actor
    {
        public enum Mood
        {
            Happy,
            Stern,
            Normal,
            Laughing,
            Closed,
            Angry,
            Surprised,
            Wink,
            Eugh,
            Scared,
            Drop,
            Dizzy,
            Plead
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
            Forward
        }
        public bool FullyRecovered;
        public int RecoveredEyes;
        public Looking LookDir = Looking.Forward;
        public Mood CurrentMood = Mood.Normal;
        private int ColorTweenLoops = 2;
        private int buffer = 2;
        private const float FloatHeight = 8;
        public float LookSpeed = 1;
        private float ColorFlashTimer = 0;
        private int GrowRadiusMult = 4;
        private int ShrinkRadiusMult = 4;

        private const int EyeCount = 8;
        private Sprite Head;
        public Sprite[] Eyes = new Sprite[EyeCount];
        private Vector2[] StarPositions = new Vector2[EyeCount];
        private Sprite Symbols;
        public Sprite Base;
        private Player player;
        private ParticleSystem system;

        public Vector2 Scale = Vector2.One;
        public Vector2? LookTarget;
        public Vector2 EyeOffset = Vector2.Zero;

        public bool Continue;
        public bool EyeInstant;
        public bool CanFloat = true;
        public bool LookAtPlayer;
        public bool EyeExpressionOverride;

        private Vector2 EyeScale = Vector2.One;
        private float StarRotation;
        private bool RenderStar;

        private MTexture[] Stars = new MTexture[EyeCount];


        private int SpinSpeedAdd = 0;
        private bool AddSpin;
        private float SpinSpeed = 2;
        private Vector2 SpinScale = Vector2.One;
        private Vector2 LookScale = Vector2.One;
        private int DetouredEye = -1;
        private bool inDetour;
        private float[] Angles = new float[EyeCount];
        private float Radius;
        private float MinusRadius;
        private int DetourBuffer;
        private int DetourSpinRequirement = 15;
        private Color[] orig_Colors = new Color[EyeCount];
        private bool Rotating
        {
            get
            {
                return CurrentMood != Mood.Plead;
            }
        }

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
        public override void Render()
        {
            Head.DrawSimpleOutline();
            foreach (Sprite Eye in Eyes)
            {
                Eye.DrawSimpleOutline();
            }

            Base.DrawSimpleOutline();
            if (Symbols.Visible)
            {
                Symbols.DrawSimpleOutline();
            }
            base.Render();
            if (RenderStar)
            {
                for (int i = 0; i < Stars.Length; i++)
                {
                    Stars[i].Draw(StarPositions[i], Stars[i].Center, Color.Yellow, Vector2.One, StarRotation);
                }
            }
        }
        public Freid(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Tag |= Tags.TransitionUpdate;
            string path = "characters/PuzzleIslandHelper/Freid/";
            Head = new Sprite(GFX.Game, path);
            Symbols = new Sprite(GFX.Game, "characters/PuzzleIslandHelper/Calidus/");
            Base = new Sprite(GFX.Game, path);
            Base.AddLoop("idle", "baseSpinH", 0.1f, 0);
            Base.Add("spinH", "baseSpinH", 0.1f, "idle");
            Base.Add("spinV", "baseSpinV", 0.1f, "idle");
            Symbols.AddLoop("exclamation", "surprisedSymbol", 0.1f);
            Symbols.AddLoop("anger", "anger", 0.1f);
            Symbols.AddLoop("eughIdle", "eughSymbol", 0.1f, 3);
            Symbols.Add("eugh", "eughSymbol", 0.2f);
            Head.AddLoop("idle", "head", 0.1f);
            for (int i = 0; i < Eyes.Length; i++)
            {
                Eyes[i] = new Sprite(GFX.Game, "characters/PuzzleIslandHelper/Calidus/");
                Eyes[i].AddLoop("neutral", "eyeFront", 0.1f);
                Eyes[i].AddLoop("happy", "eyeHappy", 0.1f);
                Eyes[i].AddLoop("stern", "eyeStern", 0.1f);
                Eyes[i].AddLoop("closed", "eyeClosed", 0.1f);
                Eyes[i].AddLoop("surprised", "eyeSurprised", 0.1f);
                Eyes[i].AddLoop("wink", "eyeWink", 0.1f);
                Eyes[i].AddLoop("detour", "eyeDetour", 0.1f, 1, 2, 3, 4);
                Eyes[i].Add("detourStart", "eyeDetour", 1f, "detour", 0);
                Eyes[i].AddLoop("plead", "eyePlead", 0.1f);
                Stars[i] = GFX.Game["characters/PuzzleIslandHelper/Calidus/star00"];

            }
            for (int i = 0; i < Eyes.Length; i++)
            {

                Eyes[i].SetColor(/*i % 2 == 0 ? Color.Pink :*/ Color.White);
                orig_Colors[i] = Eyes[i].Color;
            }

            Symbols.Visible = false;
            Add(Head, Symbols);
            for (int i = 0; i < Eyes.Length; i++)
            {
                Add(Eyes[i]);
            }

            Head.Play("idle");
            EyesPlay("neutral");

            Position -= new Vector2(6, 5);
            Collider = new Hitbox(Head.Width + 1, Head.Y + Head.Height + 1, Head.X - 1, Base.Y - 1);

        }
        private void EyesPlay(string anim, bool condition = true)
        {
            if (!Eyes[0].Animations.ContainsKey(anim) || !condition)
            {
                return;
            }
            foreach (Sprite s in Eyes)
            {
                if (s.CurrentAnimationID != anim)
                {
                    s.Play(anim);
                }
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(new Coroutine(SpinSpeedLerp()));
            Add(new Coroutine(ColorFlash()));
            Add(new Coroutine(Float()));
            Add(new Coroutine(EyeSpin()));
            Add(new Coroutine(SpinScaleLerp()));
            (scene as Level).Add(system = new ParticleSystem(Depth + 1, 500));
        }
        #region Actions
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

                Head.RenderPosition += Vector2.UnitY * offset;
                EyeOffset.Y = 4;
                Base.RenderPosition += Vector2.UnitY * offset;

                yield return 0.1f;
                Scale = Vector2.One;
                Head.RenderPosition -= Vector2.UnitY * offset;
                EyeOffset.Y = 0;
                Base.RenderPosition -= Vector2.UnitY * offset;

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
            float target = player.Position.X + Head.Position.X + offset;

            while (Position.X != target)
            {
                if (updateTarget) target = player.Position.X + Head.Position.X + offset;
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

        private IEnumerator SpinScaleLerp()
        {
            while (true)
            {
                while (!Rotating)
                {
                    yield return null;
                }
                yield return Calc.Random.Range(2f, 5f);
                bool scaleWidth = Calc.Random.Chance(0.5f);
                Vector2 BaseSpin = SpinScale;
                if (scaleWidth)
                {
                    for (float i = 0; i < 1; i += Engine.DeltaTime * SpinSpeed)
                    {
                        SpinScale.X = Calc.LerpClamp(BaseSpin.X, -BaseSpin.X, Ease.SineInOut(i));
                        yield return null;
                    }
                }
                else
                {
                    for (float i = 0; i < 1; i += Engine.DeltaTime * SpinSpeed)
                    {
                        SpinScale.Y = Calc.LerpClamp(BaseSpin.Y, -BaseSpin.Y, Ease.SineInOut(i));
                        yield return null;
                    }
                }
                yield return null;
            }

        }
        private IEnumerator EyeDetour(int index, float angle)
        {
            Eyes[index].Play("detourStart");
            Symbols.Visible = true;
            Symbols.Play("exclamation");
            Vector2 BasePos = Eyes[index].Position;
            Vector2 Extend = Calc.AngleToVector(angle + 90f.ToRad(), 80) * SpinScale;
            DetouredEye = index;
            for (float i = 0; i < 1; i += Engine.DeltaTime * 0.4f)
            {
                Eyes[index].Position = Calc.LerpSnap(BasePos, BasePos + Extend, Ease.SineOut(i));
                Symbols.Position = Eyes[index].Position + new Vector2(3, -2 - Symbols.Height);
                yield return null;
            }
            Vector2 JumpPos = Eyes[index].Position;
            Eyes[index].Play("neutral");
            for (float i = 0; i < 1; i += 0.1f)
            {
                Eyes[index].Position.Y = JumpPos.Y - i * 6;
                yield return null;
                yield return null;
            }
            JumpPos = Eyes[index].Position;
            for (float i = 1; i > 0; i -= 0.2f)
            {
                Eyes[index].Position.Y = JumpPos.Y + i * 6;
                yield return null;
            }
            yield return 0.2f;
            Symbols.Visible = false;
            DetouredEye = -1;
            inDetour = false;
            yield return null;
        }
        private IEnumerator EyeSpin()
        {
            while (true)
            {
                while (!Rotating)
                {
                    yield return null;
                }
                float distance = Head.Width;
                for (int i = 0; i < 360; i += 2)
                {
                    bool Surprised = CurrentMood == Mood.Surprised;
                    bool Scared = CurrentMood == Mood.Scared;
                    for (int j = 0; j < Eyes.Length; j++)
                    {
                        float angle = ((i + 360f / Eyes.Length * j) % 360).ToRad();
                        float length = distance;

                        if (Scared)
                        {
                            length *= 0.5f;
                        }
                        else
                        {
                            length += Radius - MinusRadius;
                            Angles[j] = angle;
                        }

                        Vector2 extend = Calc.AngleToVector(Angles[j], length);
                        Vector2 target = extend;
                        if (!Surprised && !Scared)
                        {
                            target *= SpinScale * LookScale;
                        }
                        Vector2 endPos = target + new Vector2(Head.Width / 4, Head.Height / 4) + EyeOffset + GetLookOffset();
                        Vector2 approachPos = Calc.Approach(Eyes[j].Position, endPos, SpinSpeed);

                        if (j == DetouredEye)
                        {
                            continue;
                        }
                        Eyes[j].Position.X = (float)Math.Round(approachPos.X);
                        Eyes[j].Position.Y = (float)Math.Round(approachPos.Y);
                    }
                    if (AddSpin && CurrentMood != Mood.Surprised)
                    {
                        i += SpinSpeedAdd;
                    }

                    //Rotate the eye positions around the entity's center
                    yield return null;
                }

                yield return null;
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
                    Head.Color = Color.Lerp(Color.White, Color.Green, f);
                    yield return delay;
                }
            }
        }
        private IEnumerator SpinSpeedLerp()
        {
            while (true)
            {
                yield return 7f;
                AddSpin = true;
                float speed = 5;
                Color Target = Color.Green;
                for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
                {
                    while (!Rotating)
                    {
                        yield return null;
                    }
                    SpinSpeedAdd = (int)Math.Round(Calc.LerpClamp(0, 10, i));
                    for (int j = 0; j < Eyes.Length; j++)
                    {
                        Eyes[j].Color = Color.Lerp(orig_Colors[j], Target, i);
                    }
                    MinusRadius = i * ShrinkRadiusMult;
                    yield return null;
                }
                MinusRadius = 1;
                if (DetourBuffer == DetourSpinRequirement && !inDetour)
                {
                    DetourBuffer = 0;
                    int selected = Calc.Random.Range(0, Eyes.Length);
                    Add(new Coroutine(EyeDetour(selected, Angles[selected])));
                    inDetour = true;
                }
                else
                {
                    DetourBuffer++;
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
                {
                    while (!Rotating)
                    {
                        yield return null;
                    }
                    SpinSpeedAdd = (int)Calc.LerpClamp(10, 0, Ease.SineOut(i));
                    for (int j = 0; j < Eyes.Length; j++)
                    {
                        Eyes[j].Color = Color.Lerp(Target, orig_Colors[j], i);
                    }
                    MinusRadius = ShrinkRadiusMult - i * ShrinkRadiusMult;
                    yield return null;
                }
                MinusRadius = 0;
                AddSpin = false;
            }
        }
        private IEnumerator Float()
        {
            while (true)
            {
                if (CanFloat)
                {
                    Color target = Color.Lerp(Color.White, Color.Black, 0.2f);
                    float posY = Position.Y;
                    float delay = Engine.DeltaTime / 2;
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        Position.Y = (float)Math.Round(Calc.LerpClamp(posY, posY - FloatHeight, Ease.SineInOut(i)));
                        Head.Color = Color.Lerp(target, Color.White, Ease.SineInOut(i));
                        Radius = i * GrowRadiusMult;
                        yield return delay;
                    }
                    posY = Position.Y;
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        Position.Y = (float)Math.Round(Calc.LerpClamp(posY, posY + FloatHeight, Ease.SineInOut(i)));
                        Head.Color = Color.Lerp(Color.White, target, Ease.SineInOut(i));
                        Radius = GrowRadiusMult - i * GrowRadiusMult;
                        yield return delay;
                    }
                }
                else
                {
                    yield return null;
                }
            }
        }
        private void PlayerFace(Player player)
        {
            player.Facing = Position.X > player.Position.X ? Facings.Right : Facings.Left;
        }
        public IEnumerator Say(string id, string emotion, params Func<IEnumerator>[] events)
        {
            Emotion(emotion);
            if (player is not null)
            {
                PlayerFace(player);
            }
            yield return Textbox.Say(id, events);
        }
        #endregion
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = (scene as Level).Tracker.GetEntity<Player>();
        }

        public IEnumerator Test()
        {
            yield return null;
        }
        public void Emotion(int mood)
        {
            Action Action = mood switch
            {
                (int)Mood.Happy => Happy,
                (int)Mood.Stern => Stern,
                (int)Mood.Normal => Normal,
                (int)Mood.Laughing => Laugh,
                (int)Mood.Closed => CloseEye,
                (int)Mood.Angry => Angry,
                (int)Mood.Surprised => Surprised,
                (int)Mood.Wink => Wink,
                (int)Mood.Eugh => Eugh,
                (int)Mood.Scared => Scared,
                (int)Mood.Plead => Plead,
                _ => null
            };
            if (Action is not null)
            {
                Action.Invoke();
            }
        }
        public void SetLookTarget(Vector2? target)
        {
            LookAtPlayer = false;
            LookTarget = target;
        }
        public override void Update()
        {

            base.Update();

            if (LookAtPlayer)
            {
                LookTarget = player.Center;
            }
            if (RenderStar)
            {
                StarRotation += Engine.DeltaTime * 3;
            }
            else
            {
                StarRotation = 0;
            }

            UpdateSprites();
        }

        private void UpdateSprites()
        {
            Base.Scale = Scale;
            Head.Scale = Scale;
            #region Arm Update
            if (ColorFlashTimer < 2.5f)
            {
                ColorFlashTimer += Engine.DeltaTime;
            }
            else
            {
                ColorFlashTimer = 0;
                Add(new Coroutine(ColorFlash()));
            }
            #endregion
        }
        private Vector2 GetLookOffset()
        {
            if (LookTarget.HasValue)
            {
                Vector2 norm = Vector2.Normalize(LookTarget.Value - Center);
                if (norm.X != 0)
                {
                    LookScale.X = norm.X * 0.6f;
                }
                else if (norm.Y != 0)
                {
                    LookScale.Y = norm.Y * 0.6f;
                }
                //LookScale =(new Vector2(0.6f) * norm);
                Vector2 result = norm * new Vector2(4);
                return result;
            }
            else
            {
                LookScale = Vector2.One;
            }
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
                _ => Vector2.Zero
            };
        }
        #region Emotions
        private void HeeHeeParticles()
        {
            if (buffer <= 0)
            {
                system.Emit(HeeHee, 1, TopCenter - Vector2.UnitY * 8, Vector2.UnitX * 4);
                buffer = 1;
            }
            else
            {
                buffer--;
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

        public void Scared()
        {
            CurrentMood = Mood.Scared;
            EyesPlay("surprised");
            Add(new Coroutine(ScaredRoutine()));
        }
        private IEnumerator ScaredRoutine()
        {
            while (true)
            {
                yield return 2f;
                for (int i = 0; i < 4; i++)
                {
                    if (CurrentMood != Mood.Scared)
                    {
                        break;
                    }
                    EyeOffset.X = -1;
                    yield return 0.05f;
                    if (CurrentMood != Mood.Scared)
                    {
                        break;
                    }
                    EyeOffset.X = 1;
                    yield return 0.05f;
                }
                EyeOffset.X = 0;
                if (CurrentMood != Mood.Scared)
                {
                    break;
                }
            }
        }
        public void Eugh()
        {
            CurrentMood = Mood.Eugh;
            Symbols.Position = Vector2.One;
            Add(new Coroutine(EughRoutine()));
        }
        private IEnumerator EughRoutine()
        {

            EyesPlay("stern");
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
            EyesPlay("wink");

            RenderStar = true;
            int x = 10;
            int y = 8;
            Vector2[] vecs = new Vector2[EyeCount];
            float speed = 4;
            for (int k = 0; k < Eyes.Length; k++)
            {
                vecs[k] = Eyes[k].Position + Position + Vector2.UnitX * 8;
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
            {
                for (int j = 0; j < Stars.Length; j++)
                {

                    StarPositions[j].X = Calc.LerpClamp(vecs[j].X, vecs[j].X + x / 2, i);
                    StarPositions[j].Y = Calc.LerpClamp(vecs[j].Y, vecs[j].Y - y / 2, Ease.SineInOut(i));
                }
                yield return null;
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
            {
                for (int j = 0; j < Stars.Length; j++)
                {
                    StarPositions[j].X = Calc.LerpClamp(vecs[j].X + x / 2, vecs[j].X + x, i);
                    StarPositions[j].Y = Calc.LerpClamp(vecs[j].Y - y / 2, vecs[j].Y + y, Ease.SineInOut(i));
                }
                yield return null;
            }
            RenderStar = false;
            yield return null;
        }
        public void StopRotating()
        {
            //Rotating = false;
        }
        public void StartRotating()
        {
            //Rotating = true;
        }
        public void Plead()
        {
            CurrentMood = Mood.Plead;
            EyesPlay("plead");
        }
        public void Surprised()
        {
            CurrentMood = Mood.Surprised;
            Symbols.Position = Head.Position + new Vector2(4, -20);
            Add(new Coroutine(SurprisedRoutine()));
        }
        private IEnumerator SurprisedRoutine()
        {
            Symbols.Visible = true;
            Symbols.Play("exclamation");
            EyesPlay("surprised");

            yield return 2f;
            Symbols.Stop();
            Symbols.Visible = false;
            while (CurrentMood == Mood.Surprised)
            {
                yield return null;
            }
            //Rotating = true;
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
                for (int i = 0; i < 2; i++)
                {
                    EyeOffset.X = -1;
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
            EyesPlay("closed");

            CurrentMood = Mood.Closed;
        }
        public void Happy()
        {
            EyesPlay("happy");
            CurrentMood = Mood.Happy;
        }
        public void Stern()
        {
            EyesPlay("stern");
            CurrentMood = Mood.Stern;
        }
        public void Normal()
        {
            EyesPlay("neutral");

            CurrentMood = Mood.Normal;
        }

        public void Laugh()
        {
            CurrentMood = Mood.Laughing;
            Add(new Coroutine(LaughRoutine()));
        }
        private IEnumerator LaughRoutine()
        {
            EyesPlay("happy");

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
        #endregion
    }
}


