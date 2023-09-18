using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using System.Reflection;
using Microsoft.SqlServer.Server;
using System.Linq;
using System.Collections.Generic;

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
            Forward
        }
        public Looking LookDir = Looking.Forward;
        public Mood CurrentMood = Mood.Normal;


        private int ColorTweenLoops = 2;
        private int ArmLoopCount;
        private int ArmBuffer = 60;
        private int buffer = 2;
        private const float FloatHeight = 8;
        public float LookSpeed = 1;
        private float ColorFlashTimer = 0;

        private Sprite OrbSprite;
        public Sprite EyeSprite;
        private Sprite Symbols;
        public Sprite[] Arms = new Sprite[2];
        private Player player;
        private ParticleSystem system;

        private Vector2 EyeFront;
        public Vector2 Scale = Vector2.One;
        private Vector2 EyeScale = Vector2.One;
        public Vector2 LookTarget;
        public Vector2 EyeOffset = Vector2.Zero;
        private Vector2 EyeDirection;
        private Vector2 StarPos;
        private float StarRotation;

        public bool Continue;
        public bool EyeInstant;
        public bool LookTargetEnabled;
        public bool CanFloat = true;
        public bool LookAtPlayer;
        public bool EyeExpressionOverride;
        private bool RenderStar;

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
        public Calidus(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            string path = "characters/PuzzleIslandHelper/Calidus/";
            OrbSprite = new Sprite(GFX.Game, path);
            EyeSprite = new Sprite(GFX.Game, path);
            Symbols = new Sprite(GFX.Game, path);

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
                Arms[i] = new Sprite(GFX.Game, path);
                Arms[i].AddLoop("idle", "armSpinH", 0.1f, 0);
                Arms[i].Add("spinH", "armSpinH", 0.07f, "idle");
                Arms[i].Add("spinV", "armSpinV", 0.1f, "idle");
                Arms[i].FlipX = i == 1;
            }
            Symbols.Visible = false;
            Add(OrbSprite, Symbols, Arms[0], Arms[1], EyeSprite);

            OrbSprite.Play("idle");
            EyeSprite.Play("neutral");
            Arms[0].Play("idle");
            Arms[1].Play("idle");
            Arms[0].OnLastFrame = (string s) =>
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
            };

            EyeFront = Vector2.One * (OrbSprite.Width / 2) - Vector2.One * EyeSprite.Width / 2;
            EyeSprite.Position = EyeFront;

            Arms[0].Position.X -= Arms[0].Width + 1;
            Arms[1].Position.X += OrbSprite.Width;
            Position -= new Vector2(6, 5);
            Collider = new Hitbox(Arms[0].Width * 2 + 2 + OrbSprite.Width, OrbSprite.Height, Arms[0].X);

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
        public override void Render()
        {
            OrbSprite.DrawSimpleOutline();
            Arms[0].DrawSimpleOutline();
            Arms[1].DrawSimpleOutline();
            EyeSprite.DrawSimpleOutline();
            if (Symbols.Visible)
            {
                Symbols.DrawSimpleOutline();
            }
            base.Render();
            if (RenderStar)
            {
                Star.Draw(StarPos, Star.Center, Color.Yellow, Vector2.One, StarRotation);

            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = (scene as Level).Tracker.GetEntity<Player>();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            (scene as Level).Add(system = new ParticleSystem(Depth + 1, 500));
            Add(new Coroutine(Float()));
            Add(new Coroutine(Blink()));
            Add(new Coroutine(DuckBounceCheck()));
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
                (int)Mood.RollEye => RollEye,
                (int)Mood.Laughing => Laugh,
                (int)Mood.Shakers => ShakeHead,
                (int)Mood.Nodders => NodHead,
                (int)Mood.Closed => CloseEye,
                (int)Mood.Angry => Angry,
                (int)Mood.Surprised => Surprised,
                (int)Mood.Wink => Wink,
                (int)Mood.Eugh => Eugh,
                _ => null
            };
            if (Action is not null)
            {
                Action.Invoke();
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
        public void Surprised()
        {
            CurrentMood = Mood.Surprised;
            Symbols.Position = OrbSprite.Position + new Vector2(4, -11);
            Add(new Coroutine(SurprisedRoutine()));
        }
        private IEnumerator SurprisedRoutine()
        {
            Symbols.Visible = true;
            Symbols.Play("exclamation");
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
            if (EyeSprite.CurrentAnimationID != "closed") EyeSprite.Play("closed");
            CurrentMood = Mood.Closed;
        }
        public void Happy()
        {
            if (EyeSprite.CurrentAnimationID != "happy") EyeSprite.Play("happy");
            CurrentMood = Mood.Happy;
        }
        public void Stern()
        {
            if (EyeSprite.CurrentAnimationID != "stern") EyeSprite.Play("stern");
            CurrentMood = Mood.Stern;
        }
        public void Normal()
        {
            if (EyeSprite.CurrentAnimationID != "neutral") EyeSprite.Play("neutral");
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


        public override void Update()
        {
            base.Update();
            if (LookAtPlayer)
            {
                LookTargetEnabled = true;
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

            UpdateSprites(GetLookOffset());
        }
        private void UpdateSprites(Vector2 eyeOffset)
        {
            Arms[0].Scale = Scale;
            Arms[1].Scale = Scale;
            OrbSprite.Scale = Scale;
            EyeSprite.Scale = EyeScale;
            #region Eye Update
            if (!EyeExpressionOverride) //Use to stop constant eye position updates here (used for stuff like RollEye)
            {
                if (LookTargetEnabled)
                {
                    Vector2 Rotated = RotatePoint(Vector2.Zero, EyeSprite.Center, (Calc.Angle(Center, LookTarget) + 90).ToDeg());
                    EyeDirection = Vector2.Normalize(Rotated - EyeSprite.Center);
                    if (!EyeInstant)
                    {
                        EyeSprite.Position = Calc.Approach(EyeSprite.Position, Rotated + EyeOffset, LookSpeed);

                    }
                    else
                    {
                        EyeSprite.Position = Rotated + EyeOffset;
                    }
                    //Rotate the eye position around the entity's center

                }
                else
                {
                    //Center the eye in the middle of the Entity
                    if (!EyeInstant)
                    {
                        EyeSprite.Position = Calc.Approach(EyeSprite.Position, EyeFront + eyeOffset + EyeOffset, LookSpeed);

                    }
                    else
                    {
                        EyeSprite.Position = EyeFront + eyeOffset + EyeOffset;
                    }
                }
            }
            #endregion
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

        #region ///////////////////////////////// Finished \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
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
            yield return null;
            while (true)
            {
                yield return Calc.Random.Range(1, 10f);
                EyeSprite.Color = Color.Lerp(Color.White, Color.Black, 0.3f);
                yield return 0.1f;
                EyeSprite.Color = Color.White;
            }
        }
        private IEnumerator Testing()
        {
            for (int i = 0; i < 3; i++)
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
                LookDir = Looking.Forward;
                yield return 1;
            }
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
        private IEnumerator Float()
        {
            while (true)
            {
                if (CanFloat)
                {


                    Color target = Color.Lerp(Color.White, Color.Black, 0.2f);
                    float posY = Position.Y;
                    float armPosX = Arms[0].Position.X;
                    float armRightPosX = Arms[1].Position.X;
                    float delay = Engine.DeltaTime / 2;
                    int armDistance = 1;
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        Position.Y = (float)Math.Round(Calc.LerpClamp(posY, posY - FloatHeight, Ease.SineInOut(i)));
                        Arms[0].Position.X = (float)Math.Round(Calc.LerpClamp(armPosX, armPosX - armDistance, Ease.SineInOut(i)));
                        Arms[1].Position.X = (float)Math.Round(Calc.LerpClamp(armRightPosX, armRightPosX + armDistance, Ease.SineInOut(i)));
                        OrbSprite.Color = Color.Lerp(target, Color.White, Ease.SineInOut(i));
                        yield return delay;
                    }
                    posY = Position.Y;
                    armPosX = Arms[0].Position.X;
                    armRightPosX = Arms[1].Position.X;
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        Position.Y = (float)Math.Round(Calc.LerpClamp(posY, posY + FloatHeight, Ease.SineInOut(i)));
                        Arms[0].Position.X = (float)Math.Round(Calc.LerpClamp(armPosX, armPosX + armDistance, Ease.SineInOut(i)));
                        Arms[1].Position.X = (float)Math.Round(Calc.LerpClamp(armRightPosX, armRightPosX - armDistance, Ease.SineInOut(i)));
                        OrbSprite.Color = Color.Lerp(Color.White, target, Ease.SineInOut(i));
                        yield return delay;
                    }
                }
                else
                {
                    yield return null;
                }
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
                _ => Vector2.Zero
            };
        }
    }
}
