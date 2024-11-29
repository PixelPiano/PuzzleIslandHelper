using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/InvertAuth")]
    [Tracked]
    public class InvertAuth : Entity
    {
        [TrackedAs(typeof(ShaderOverlay))]
        public class InvertOrb : ShaderOverlay
        {
            public ColorLine[] Lines = new ColorLine[8];
            public List<Color> Colors => Parent.Colors;
            public float Speed;
            public bool SpeedingUp;
            public List<Dir> UsedDirections => Parent.UsedDirections;
            public float Amount;
            public InvertAuth Parent;
            public bool Strong;
            public float Random;
            public Player player;
            public bool forcedVisibleState;
            public InvertOrb(InvertAuth parent) : base("PuzzleIslandHelper/Shaders/invertOrb", "", true)
            {
                Collider = new Hitbox(SIZE, SIZE);
                Parent = parent;
                float lineWidth = Width / 16f;
                for (int i = 0; i < 8; i++)
                {
                    Lines[i] = new(Color.Transparent, (i + 4) * lineWidth, (int)lineWidth, 20)
                    {
                        Index = i
                    };
                    Add(Lines[i]);
                }
            }

            public override void EffectRender()
            {
                base.EffectRender();
                if (Scene is not Level level) return;
                foreach (ColorLine line in Lines)
                {
                    line.RenderAt(Position - level.Camera.Position);
                }
            }
            public override void BeforeApply()
            {
                base.BeforeApply();
                if (player is null) return;
                player.Visible = false;
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                if(player is not null)
                {
                    player.Visible = true;
                }
            }
            public override void AfterApply()
            {
                base.AfterApply();
                if (player is null) return;
                Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);
                GameplayRenderer.Begin();
                player.Render();
                Draw.SpriteBatch.End();
            }
            public override void ApplyParameters()
            {
                base.ApplyParameters();
                if (Scene is not Level level || Effect is null || Effect.Parameters is null) return;
                Effect.Parameters["Size"]?.SetValue(0.005f);
                Effect.Parameters["Center"]?.SetValue((Center - level.Camera.Position) / new Vector2(320, 180));
                Effect.Parameters["Speed"]?.SetValue(Speed);
            }
            public Color GetStaticColor(Dir direction)
            {
                if (UsedDirections.Contains(direction))
                {
                    return Colors[(int)direction] * FadeInVals[direction];
                }
                else
                {
                    return Color.Transparent;
                }
            }
            public Dictionary<Dir, float> FadeInVals = new()
            {
                {Dir.Up,0 },{Dir.Down,0 },{Dir.Left,0 },{Dir.Right,0 },{Dir.UpLeft,0 },{Dir.UpRight,0 },{Dir.DownLeft,0 },{Dir.DownRight,0 }
            };
            public class ColorLine : Component
            {
                public Color Color;
                public Color StaticColor;
                public int Thickness = 1;
                public float FadeIn;
                public float XOffset;
                public float Speed;
                public int Index;
                public ColorLine(Color color, float startProgress = 0, int thickness = 1, float speed = 0) : base(true, true)
                {
                    XOffset = startProgress;
                    Color = color;
                    Thickness = thickness;
                    Speed = speed;
                }
                public void RenderAt(Vector2 position)
                {
                    Draw.Line(position + Vector2.UnitX * XOffset, position + new Vector2(XOffset, SIZE), Color, Thickness);
                }
            }
            public override void Update()
            {
                if (SpeedingUp)
                {
                    Speed += Engine.DeltaTime * 4;
                }
                player = Scene.GetPlayer();
                if (player != null)
                {
                    player.Visible = forcedVisibleState;
                }
                base.Update();
                Position = Parent.Position + Vector2.One * (Parent.Width - Width) / 2;
                Amount += Engine.DeltaTime;
                foreach (Dir d in UsedDirections)
                {
                    FadeInVals[d] = Calc.Approach(FadeInVals[d], 1, Engine.DeltaTime);
                }
                foreach (ColorLine line in Lines)
                {
                    Color from = line.StaticColor;
                    Dir dir = (Dir)((line.Index + 1) % 8);
                    Color to = GetStaticColor(dir);
                    line.Color = Color.Lerp(from, to, Amount);
                }
                if (Amount >= 1)
                {
                    Amount %= 1;
                    foreach (ColorLine line in Lines)
                    {
                        line.Index++;
                        line.Index %= 8;
                        line.StaticColor = GetStaticColor((Dir)line.Index);
                    }
                }
            }
        }

        [TrackedAs(typeof(ShaderEntity))]
        public class GlassStatic : ShaderEntity
        {
            public static MTexture Texture => GFX.Game["objects/PuzzleIslandHelper/invert/glassOrbFilled"];
            private GlassPiece piece;
            public float FlashAlpha;
            public GlassStatic(GlassPiece reference) : base(reference.TrueRenderPosition, "PuzzleIslandHelper/Shaders/glassStatic", Texture.Width, Texture.Height)
            {
                piece = reference;
                Alpha = 0;
                Depth = 2;
            }
            public override void Update()
            {
                base.Update();
                Position = piece.TrueRenderPosition;
            }
            public IEnumerator Reveal(float flashTime)
            {
                FlashAlpha = 1;
                yield return 0.1f;
                Alpha = 1;
                for (float i = 0; i < 1; i += Engine.DeltaTime / flashTime)
                {
                    FlashAlpha = Calc.LerpClamp(1, 0, Ease.ExpoOut(i));
                    yield return null;
                }
                FlashAlpha = 0;
            }
            public override void Render()
            {
                base.Render();
                if (FlashAlpha > 0)
                {
                    Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, Position, Color.White * FlashAlpha);
                }
            }
        }
        public class Arrow : Entity
        {
            public class ArrowParticle : AbsorbOrb
            {
                public Color Color;
                public ArrowParticle(Color color, Vector2 position, Entity into = null, Vector2? absorbTarget = null) : base(position, into, absorbTarget)
                {
                    sprite.Texture = GFX.Game["objects/PuzzleIslandHelper/invert/arrowParticle"];
                    Color = color;
                    sprite.Color = Color.White;
                }

                public override void Update()
                {
                    Vector2 vector = Vector2.Zero;
                    bool flag = false;
                    if (AbsorbInto != null)
                    {
                        vector = AbsorbInto.Center;
                        flag = AbsorbInto.Scene == null || (AbsorbInto is Player && (AbsorbInto as Player).Dead);
                    }
                    else if (AbsorbTarget.HasValue)
                    {
                        vector = AbsorbTarget.Value;
                    }
                    else
                    {
                        Player entity = base.Scene.Tracker.GetEntity<Player>();
                        if (entity != null)
                        {
                            vector = entity.Center;
                        }

                        flag = entity == null || entity.Scene == null || entity.Dead;
                    }

                    if (flag)
                    {
                        Position += burstDirection * burstSpeed * Engine.RawDeltaTime;
                        burstSpeed = Calc.Approach(burstSpeed, 800f, Engine.RawDeltaTime * 200f);
                        sprite.Rotation = burstDirection.Angle();
                        sprite.Scale = new Vector2(Math.Min(2f, 0.5f + burstSpeed * 0.02f), Math.Max(0.05f, 0.5f - burstSpeed * 0.004f));
                        sprite.Color = Color * (alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime));
                    }
                    else if (consumeDelay > 0f)
                    {
                        Position += burstDirection * burstSpeed * Engine.RawDeltaTime;
                        burstSpeed = Calc.Approach(burstSpeed, 0f, Engine.RawDeltaTime * 120f);
                        sprite.Rotation = burstDirection.Angle();
                        sprite.Scale = new Vector2(Math.Min(2f, 0.5f + burstSpeed * 0.02f), Math.Max(0.05f, 0.5f - burstSpeed * 0.004f));
                        consumeDelay -= Engine.RawDeltaTime;
                        if (consumeDelay <= 0f)
                        {
                            Vector2 position = Position;
                            Vector2 vector2 = vector;
                            Vector2 vector3 = (position + vector2) / 2f;
                            Vector2 vector4 = (vector2 - position).SafeNormalize().Perpendicular() * (position - vector2).Length() * (0.05f + Calc.Random.NextFloat() * 0.45f);
                            float value = vector2.X - position.X;
                            float value2 = vector2.Y - position.Y;
                            if ((Math.Abs(value) > Math.Abs(value2) && Math.Sign(vector4.X) != Math.Sign(value)) || (Math.Abs(value2) > Math.Abs(value2) && Math.Sign(vector4.Y) != Math.Sign(value2)))
                            {
                                vector4 *= -1f;
                            }

                            curve = new SimpleCurve(position, vector2, vector3 + vector4);
                            duration = 0.3f + Calc.Random.NextFloat(0.25f);
                            burstScale = sprite.Scale;
                        }
                    }
                    else
                    {
                        curve.End = vector;
                        if (percent >= 1f)
                        {
                            RemoveSelf();
                        }

                        percent = Calc.Approach(percent, 1f, Engine.RawDeltaTime / duration);
                        float num = Ease.CubeIn(percent);
                        Position = curve.GetPoint(num);
                        float num2 = Calc.YoYo(num) * curve.GetLengthParametric(10);
                        sprite.Scale = new Vector2(Math.Min(2f, 0.5f + num2 * 0.02f), Math.Max(0.05f, 0.5f - num2 * 0.004f));
                        sprite.Color = Color * (1f - num);
                        sprite.Rotation = Calc.Angle(Position, curve.GetPoint(Ease.CubeIn(percent + 0.01f)));
                    }
                }
            }

            public InvertAuth Parent;
            public Dir Facing;
            public static readonly Dictionary<Dir, SpriteEffects> ArrowEffectDict = new()
            {
                {Dir.Up, SpriteEffects.None},
                {Dir.Down, SpriteEffects.FlipVertically},
                {Dir.Left, SpriteEffects.FlipHorizontally},
                {Dir.Right, SpriteEffects.None},
                {Dir.UpLeft, SpriteEffects.FlipHorizontally},
                {Dir.DownLeft, SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally},
                {Dir.UpRight, SpriteEffects.None},
                {Dir.DownRight, SpriteEffects.FlipVertically},
            };
            public float WhiteAmount;
            public Sprite Sprite;
            public bool Activated;
            private Vector2 offset;
            private Color FadeColor;
            private BloomPoint bloom;
            public Vector2 DirectionalOffset;
            public Arrow(InvertAuth parent, Dir direction, Color fadeColor) : base()
            {
                Parent = parent;
                string type = Enum.GetName(typeof(Dir), direction).Length > 5 ? "B" : "A";
                string folderPath = "objects/PuzzleIslandHelper/invert/";
                SpriteEffects effects = ArrowEffectDict[direction];
                Facing = direction;
                Sprite = new Sprite(GFX.Game, folderPath)
                {
                    Effects = effects
                };

                Sprite.Add("anim", "arrowIn" + type, 0.03f, "empty");
                Sprite.AddLoop("empty", "empty", 0.1f);
                Sprite.AddLoop("idle", "arrow" + type, 0.1f);
                FadeColor = fadeColor;
                Sprite.Color = fadeColor;
                Add(Sprite);
                Collider = new Hitbox(Sprite.Width, Sprite.Height);
                Add(bloom = new BloomPoint(1, Width));
                bloom.Visible = false;
                if (direction is Dir.Up or Dir.Down)
                {
                    Sprite.Rotation = direction == Dir.Up ? -90f.ToRad() : 90f.ToRad();
                    Sprite.CenterOrigin();
                    Sprite.Position += Collider.HalfSize;
                }
                DirectionalOffset = Vector2.Normalize(GetOffset(direction));
                offset = DirectionalOffset * parent.Collider.HalfSize;
                Position = parent.Center + offset;
            }
            public override void Update()
            {
                Position = Parent.Center + offset - Collider.HalfSize;
                base.Update();
            }
            public IEnumerator FadeTo(float to, float duration)
            {
                float from = WhiteAmount;
                for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
                {
                    WhiteAmount = Calc.LerpClamp(from, to, Ease.SineInOut(i));
                    yield return null;
                }
                WhiteAmount = to;
            }
            public void FadeUp(float time)
            {
                Add(new Coroutine(FadeTo(0.4f, time)));
            }
            public void FadeDown(float time)
            {
                Add(new Coroutine(FadeTo(0, time)));
            }
            public override void Render()
            {
                Color color = Sprite.Color;
                Sprite.Color = Color.Lerp(color, Color.White, WhiteAmount);
                base.Render();
                Sprite.Color = color;
            }
            public void Activate(Dir direction)
            {
                if (!Activated && Scene is Level level)
                {
                    Parent.AddActiveDirection(direction);
                    Sprite.Play("anim");
                    Sprite.OnLastFrame = (string s) =>
                    {
                        if (s == "anim")
                        {
                            Add(new Coroutine(fadeIn(direction)));
                        }
                    };
                    Activated = true;
                }
            }
            private IEnumerator addAbsorbOrbs(Dir direction)
            {
                Parent.ShatterPiece(direction);
                Vector2 offset = Vector2.Normalize(GetOffset(Facing)) * Collider.HalfSize;
                List<ArrowParticle> particles = new();
                for (int i = 0; i < 10; i++)
                {
                    Vector2 position = Center + offset;
                    ArrowParticle p = new ArrowParticle(FadeColor, position, Parent.Orb, null);
                    Scene.Add(p);
                    particles.Add(p);
                }
                foreach (ArrowParticle p in particles)
                {
                    while (p.percent < 1) yield return null;
                }
                Parent.AddDirection(direction);
            }
            private IEnumerator fadeIn(Dir direction)
            {
                bloom.Alpha = 0;
                Sprite.Color *= 0;
                Add(new Coroutine(addAbsorbOrbs(direction)));
                yield return null;
                bloom.Visible = true;
                Sprite.Play("idle");
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.8f)
                {
                    Sprite.Color = Color.Lerp(FadeColor * 0, FadeColor, Ease.SineOut(i));
                    yield return null;
                }
                Sprite.Color = FadeColor;
            }
        }
        public class GlassPiece : Image
        {
            public static MTexture BaseTexture = GFX.Game["objects/PuzzleIslandHelper/invert/glassOrb"];
            public Dir Direction;
            public Vector2 offset;
            public float shakeTimer;
            public Vector2 shakeVector;
            public Arrow arrow;
            public InvertAuth Parent;
            public int SubtextureSize;
            public Vector2 TrueRenderPosition => RenderPosition + offset;
            public Vector2 GetPixelOffset(Dir direction)
            {
                return direction switch
                {
                    Dir.UpRight or Dir.Right => -Vector2.UnitX,
                    Dir.Down or Dir.DownLeft => -Vector2.UnitY,
                    Dir.DownRight => -Vector2.One,
                    _ => Vector2.Zero

                };
            }
            public GlassPiece(InvertAuth parent, Arrow arrow) : base(GFX.Game["objects/PuzzleIslandHelper/invert/glassOrb"], true)
            {
                this.arrow = arrow;
                Parent = parent;
                Direction = arrow.Facing;
                int size = SubtextureSize = (int)Math.Round(Texture.Width / 3f, 0);
                int last = Texture.Width - size;
                Rectangle r = Direction switch
                {
                    Dir.Up => new Rectangle(size, 0, size, size),
                    Dir.Down => new Rectangle(size, last, size, size),
                    Dir.Left => new Rectangle(0, size, size, size),
                    Dir.Right => new Rectangle(last, size, size, size),
                    Dir.UpLeft => new Rectangle(0, 0, size, size),
                    Dir.UpRight => new Rectangle(last, 0, size, size),
                    Dir.DownLeft => new Rectangle(0, last, size, size),
                    Dir.DownRight => new Rectangle(last, last, size, size),
                    _ => default
                };
                offset = new Vector2(r.X, r.Y) - new Vector2(12);
                Texture = Texture.GetSubtexture(r);
                Visible = false;
            }
            public void Shatter()
            {
                Visible = true;
                shakeTimer = 0.3f;
            }
            public override void Update()
            {
                base.Update();
                shakeVector = shakeTimer > 0 ? Calc.Random.ShakeVector() : Vector2.Zero;
                shakeTimer = Calc.Max(shakeTimer - Engine.DeltaTime, 0);
            }
            public override void Render()
            {
                if (Texture != null)
                {
                    Texture.Draw(TrueRenderPosition + shakeVector, Origin, Color, Scale, Rotation, Effects);
                }
            }
        }

        public Vector2 shakeVector => Pieces != null && Pieces[0] != null ? Pieces[0].shakeVector : Vector2.Zero;
        private static MTexture lonnTex = GFX.Game["objects/PuzzleIslandHelper/invert/lonn"];
        public enum Dir
        {
            Up, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft
        }
        public const int SIZE = 32;
        public float Amount;
        public string Flag;
        private bool prevState;
        private bool state;
        private Vector2 origPosition;
        private SoundSource whoosh;
        public Dictionary<Dir, Arrow> Arrows = new();
        public InvertOrb Orb;
        public GlassStatic Static;
        public Vector2 From;
        public Vector2 To;
        public List<Dir> UsedDirections = new();
        public List<Dir> ActiveDirections = new();
        public List<Color> Colors = new()
            {
                Color.Red, Color.Orange,Color.Yellow, Color.LightGreen, Color.Cyan, Color.Blue, Color.Magenta, Color.White
            };
        public bool Verified;
        public void RevealStatic()
        {
            Static.Add(new Coroutine(Static.Reveal(1.2f)));
            Verified = true;
            Orb.SpeedingUp = true;
        }

        public InvertAuth(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            From = data.NodesOffset(offset)[0];
            To = Position;
            origPosition = Position = From;
            Flag = data.Attr("flag");
            Collider = new Hitbox(lonnTex.Width, lonnTex.Height);
            Add(new Coroutine(FadeRoutine()));
            DashListener listener = new DashListener(OnDash);
            Add(listener);
            //todo: make whoosh in/out sound
            //Add(whoosh = new SoundSource());
        }
        public IEnumerator ArrowFadeCycle(float time)
        {
            Dir? lastDirection = null;
            while (true)
            {
                foreach (Dir d in Enum.GetValues(typeof(Dir)))
                {
                    Arrows[d].FadeUp(time);
                    if (lastDirection.HasValue)
                    {
                        Arrows[lastDirection.Value].FadeDown(time);
                    }
                    yield return time * 0.9f;
                    lastDirection = d;
                }
            }
        }
        private void OnDash(Vector2 dir)
        {
            if (!state) return;
            Dir direction = VectorToDir(dir);
            Arrows[direction].Activate(direction);
            if (!Verified)
            {
                foreach (Dir d in Enum.GetValues(typeof(Dir)))
                {
                    if (!ActiveDirections.Contains(d))
                    {
                        return;
                    }
                }
                RevealStatic();
            }
        }
        public static Dir VectorToDir(Vector2 vec)
        {
            return vec.X < 0 ? vec.Y < 0 ? Dir.UpLeft : vec.Y > 0 ? Dir.DownLeft : Dir.Left : vec.X > 0 ? vec.Y < 0 ? Dir.UpRight : vec.Y > 0 ? Dir.DownRight : Dir.Right : vec.Y > 0 ? Dir.Down : Dir.Up;
        }
        public IEnumerator FadeRoutine()
        {
            while (true)
            {
                if (Scene is Level level)
                {
                    prevState = state;
                    state = level.Session.GetFlag(Flag);
                    if (prevState != state)
                    {
                        WhooshSfx(state);
                        if (state)
                        {
                            for (float i = 0; i < 1; i += Engine.DeltaTime / 1.5f)
                            {
                                Amount = i;
                                Position = Vector2.Lerp(From, To, Ease.CubeInOut(i));
                                yield return null;
                            }
                            Amount = 1;
                            Position = To;
                        }
                        else
                        {
                            for (float i = 0; i < 1; i += Engine.DeltaTime / 1.5f)
                            {
                                Amount = 1 - i;
                                Position = Vector2.Lerp(To, From, Ease.CubeInOut(i));
                                yield return null;
                            }
                            Amount = 0;
                            Position = From;
                        }
                    }
                }
                yield return null;
            }
        }
        public override void Update()
        {
            base.Update();
            Orb.Amplitude = Calc.ClampedMap(Amount, 0.5f, 1);
        }
        public void WhooshSfx(bool fadeIn)
        {
            if (whoosh != null && !whoosh.Playing)
            {
                whoosh.Play("put something cool here");
            }
        }
        public void ShatterPiece(Dir direction)
        {
            foreach (GlassPiece p in Pieces)
            {
                if (p.Direction == direction)
                {
                    p.Shatter();
                    AddGlassShards(p, 7);
                }
            }
        }
        public void AddDirection(Dir direction)
        {
            if (!UsedDirections.Contains(direction))
            {
                UsedDirections.Add(direction);
            }
        }
        public void AddActiveDirection(Dir direction)
        {
            if (!ActiveDirections.Contains(direction))
            {
                ActiveDirections.Add(direction);
            }
        }
        public ParticleType GlassShards = new ParticleType()
        {
            Size = 1,
            Color = Color.Gray,
            Color2 = Color.White,
            Acceleration = Vector2.UnitY * 100f,
            SpeedMin = 40f,
            SpeedMax = 100f,
            SpeedMultiplier = 0.25f,
            ColorMode = ParticleType.ColorModes.Choose,
            LifeMin = 2,
            LifeMax = 4,
            DirectionRange = 45f.ToRad(),
            Direction = 90f.ToRad()
        };
        public void AddGlassShards(GlassPiece piece, int shards)
        {
            if (Scene is not Level level) return;
            Vector2 position = piece.TrueRenderPosition;
            float size = piece.SubtextureSize;
            Vector2 increment = Vector2.Zero;
            switch (piece.Direction)
            {
                case Dir.Up:
                    increment = Vector2.UnitX;
                    break;
                case Dir.Down:
                    increment = Vector2.UnitX;
                    position.Y += size;
                    break;
                case Dir.Left:
                    increment = Vector2.UnitY;
                    break;
                case Dir.Right:
                    increment = Vector2.UnitY;
                    position.X += size;
                    break;
                case Dir.UpLeft:
                    increment = new(1, -1);
                    position.Y += size;
                    break;
                case Dir.DownLeft:
                    increment = new(1, 1);
                    break;
                case Dir.UpRight:
                    increment = new(1, 1);
                    break;
                case Dir.DownRight:
                    increment = new(1, -1);
                    position.Y += size;
                    break;
            }
            float space = size / shards;
            for (int i = 0; i < shards; i++)
            {
                GlassShards.Direction = Calc.Random.Range(0, 360f).ToRad();
                level.ParticlesBG.Emit(GlassShards, position);
                position += increment * space;
            }
        }
        public List<GlassPiece> Pieces = new();
        public override void Added(Scene scene)
        {
            base.Added(scene);
            foreach (Dir dir in Enum.GetValues(typeof(Dir)))
            {
                Arrow arrow = new Arrow(this, dir, Colors[(int)dir]);
                Arrows.Add(dir, arrow);
                scene.Add(arrow);
            }
            GlassPiece reference = null;
            foreach (Arrow arrow in Arrows.Values)
            {
                GlassPiece piece = new GlassPiece(this, arrow);
                Pieces.Add(piece);
                Add(piece);
                if (arrow.Facing == Dir.UpLeft)
                {
                    reference = piece;
                }
            }

            Depth = 1;
            if (reference != null)
            {
                Static = new GlassStatic(reference);
                scene.Add(Static);
            }
            Orb = new InvertOrb(this);
            scene.Add(Orb);
        }
        public override void Render()
        {
            base.Render();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (Arrow a in Arrows.Values)
            {
                scene.Remove(a);
            }
            scene.Remove(Orb);
        }
        public static Vector2 GetOffset(Dir dir)
        {
            return dir switch
            {
                Dir.Up => new(0f, -0.5f),
                Dir.Down => new(0f, 0.5f),
                Dir.Left => new(-0.5f, 0f),
                Dir.Right => new(0.5f, 0),
                Dir.UpLeft => new(-0.5f),
                Dir.UpRight => new(0.5f, -0.5f),
                Dir.DownLeft => new(-0.5f, 0.5f),
                Dir.DownRight => new(0.5f),
                _ => new(0.5f)
            };
        }
    }
}