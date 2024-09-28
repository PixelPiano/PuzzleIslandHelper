using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using Color = Microsoft.Xna.Framework.Color;

// PuzzleIslandHelper.VoidLight
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class PulsingCircle : GraphicsComponent
    {
        public float Alpha;
        public float Radius;
        public float Thickness;
        public int Resolution;
        private float alphaAdd, radiusAdd, thicknessAdd;
        private int resAdd;
        public Tween Tween;
        public Action<Tween> OnTweenUpdate;
        public PulsingCircle(Vector2 position, float radius, float alpha, float thickness, int resolution) : base(true)
        {
            Position = position;
            Radius = radius;
            Alpha = alpha;
            Thickness = thickness;
            Resolution = resolution;
        }
        public override void Render()
        {
            base.Render();
            DrawCircle(RenderPosition, Radius + radiusAdd, Color * (Alpha + alphaAdd), Thickness + thicknessAdd, Resolution + resAdd, Scale);
        }
        public void Pulse(Ease.Easer ease, float duration, float multiplier)
        {
            if (Entity != null)
            {
                if (Tween != null)
                {
                    Tween.Active = false;
                }
                float aFrom = Alpha;
                float rFrom = Radius;
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, ease, duration, true);
                tween.OnUpdate = t =>
                {
                    Alpha = Calc.LerpClamp(aFrom * multiplier, aFrom, t.Eased);
                    Radius = Calc.LerpClamp(rFrom * multiplier, rFrom, t.Eased);
                };
                tween.OnComplete = delegate { Tween.Active = true; };
                Entity.Add(tween);
            }

        }
        public void StartTween(Ease.Easer ease, float duration, float alphaAdd = 0, float radiusAdd = 0, float thicknessAdd = 0, int resolutionAdd = 0)
        {
            if (Entity != null)
            {
                Tween?.RemoveSelf();
                Tween = Tween.Create(Tween.TweenMode.YoyoLooping, ease, duration, true);
                Tween.OnUpdate = t =>
                {
                    this.alphaAdd = t.Eased * alphaAdd;
                    this.radiusAdd = t.Eased * radiusAdd;
                    this.thicknessAdd = t.Eased * thicknessAdd;
                    resAdd = (int)(t.Eased * resolutionAdd);
                };
                Entity.Add(Tween);
            }
        }
        private void DrawCircle(Vector2 position, float radius, Color color, float thickness, int resolution, Vector2 scale)
        {
            Vector2 vector = Vector2.UnitX * radius;
            Vector2 vector2 = vector.Perpendicular();
            for (int i = 1; i <= resolution; i++)
            {
                float t = thickness * Math.Abs(scale.X);
                Vector2 vector3 = Calc.AngleToVector((float)i * ((float)Math.PI / 2f) / (float)resolution, radius);
                Vector2 vector4 = vector3.Perpendicular();
                Draw.Line(position + vector * scale, position + vector3, color, t);
                Draw.Line(position - vector * scale, position - vector3, color, t);
                Draw.Line(position + vector2, position + vector4 * scale, color, t);
                Draw.Line(position - vector2, position - vector4 * scale, color, t);
                vector = vector3;
                vector2 = vector4;
            }
        }
    }
    [Tracked]
    public class VoidSafeZone : Component
    {
        public bool IsSafe;
        public float Width;
        public float Height;
        public Vector2 Position;
        public bool Inverted;
        private Rectangle rectangle;
        public Vector2 Center => Position + new Vector2(rectangle.Width, rectangle.Height) / 2f;
        public VoidSafeZone(Vector2 position, float width, float height, bool inverted) : base(true, true)
        {
            IsSafe = true;
            //IsSafe = !Inverted;
            Position = position;
            Width = width;
            Height = height;
            Inverted = inverted;
            rectangle = new Rectangle((int)position.X, (int)position.Y, (int)width, (int)height);
        }
        public bool Colliding(Entity entity)
        {
            return Collide.CheckRect(entity, rectangle);
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(rectangle, Color.Orange);
        }
        public override void Update()
        {
            base.Update();
            if (Entity != null)
            {
                Vector2 p = Entity.Position + Position;
                rectangle.X = (int)p.X;
                rectangle.Y = (int)p.Y;
                rectangle.Width = (int)Width;
                rectangle.Height = (int)Height;
            }
        }
        public static bool Check(Entity entity)
        {
            foreach (VoidSafeZone zone in entity.Scene.Tracker.GetComponents<VoidSafeZone>())
            {
                if (zone.Colliding(entity))
                {
                    return zone.IsSafe;
                }
            }
            return false;
        }
    }
    [CustomEntity("PuzzleIslandHelper/VoidLight")]
    [Tracked]
    public class VoidLight : Entity
    {
        public const int Shapes = 2;
        private int startFade;
        private int endFade;
        private int radius;
        private int thickness = 3;

        private float spinCircleSize;
        private float circleSizeAdd;
        private float alphaAdd;
        private float opacity;
        private float spinCircleAlpha;
        private float targetRadius;


        private float[] sizes = new float[Shapes];
        private float[] alphas = new float[Shapes];
        private float[] initialSizes = new float[Shapes];

        private bool tweensAnimating;
        public bool State;

        private Vector2 circleScale = Vector2.One;
        private Vector2 node;
        private Color color;
        private EntityID id;
        private Tween constantTween;
        private Tween spinTween;
        private Sprite sprite;
        private VertexLight light;
        private BloomPoint bloom;
        private ParticleSystem system;
        public string FlagName
        {
            get
            {
                return "voidlight_" + id.Key;
            }
        }
        private bool collided;
        private bool startLit;
        private ParticleType diamond = new ParticleType
        {
            Size = 1f,
            Color = Color.Purple,
            Color2 = Color.MediumPurple,
            ColorMode = ParticleType.ColorModes.Choose,
            Direction = -MathHelper.PiOver2,
            DirectionRange = MathHelper.TwoPi,
            LifeMin = 0.2f,
            LifeMax = 1,
            SpeedMin = 20f,
            SpeedMax = 30f,
            SpeedMultiplier = 0.5f,
            FadeMode = ParticleType.FadeModes.Late,
            Friction = 6f
        };
        public bool Persistent;
        public static bool OnlySpinWhenFirstLit = true;
        private float outlineSize;
        private float outlineAlpha = 1;
        public VoidSafeZone SafeZone;
        public CritterLight Mask;
        public VoidLight(EntityData data, Vector2 offset, EntityID id)
        : base(data.Position + offset)
        {
            this.id = id;
            node = data.Nodes[0];
            Tag |= Tags.TransitionUpdate;
            Depth = 1;
            radius = data.Int("radius");
            opacity = data.Float("alpha");
            startFade = radius - Math.Min(8, (int)(radius / 3f));
            endFade = radius;
            color = data.HexColor("color", Color.White);
            startLit = data.Bool("startLit");
            Add(SafeZone = new VoidSafeZone(Vector2.Zero, data.Width, data.Height, false));

        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Vector2 levelPosition = (scene as Level).LevelOffset;
            Vector2 offset = levelPosition + node - Position + Vector2.One * 12;
            sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/voidLight/");
            sprite.AddLoop("idle", "centerIdle", 0.1f);
            sprite.Add("spin", "centerSpin", 0.12f, "idle");
            sprite.AddLoop("off", "centerOff", 0.1f);
            Add(bloom = new BloomPoint(offset, opacity * 0.5f, radius));
            Add(light = new VertexLight(offset, color, opacity, startFade, endFade));
            sprite.Position += offset - new Vector2(sprite.Width / 2, sprite.Height / 2);
            sprite.Color = Color.Lerp(Color.White, Color.Black, 0.3f);
            Level level = scene as Level;
            if (startLit)
            {
                level.Session.SetFlag(FlagName);
            }
            State = level.Session.GetFlag(FlagName);

            if (State)
            {
                SafeZone.IsSafe = true;
                sprite.Play("idle");
            }
            else
            {
                SafeZone.IsSafe = false;
                bloom.Visible = false;
                light.Visible = false;
                sprite.Play("off");
            }
            Collider = new Hitbox(sprite.Width, sprite.Height, sprite.Position.X, sprite.Position.Y);
            //scene.Add(helper);
        }
        private bool wasColliding;
        private Color outlineColor;
        public void LightSelf()
        {
            if (!State)
            {
                Audio.Play("event:/game/05_mirror_temple/torch_activate", sprite.RenderPosition);
                bloom.Visible = true;
                light.Visible = true;
                Collidable = false;
                outlineTween.Active = false;
                float sizeFrom = outlineSize;
                float alphaFrom = outlineAlpha;
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BackOut, 1f, start: true);
                tween.OnUpdate = delegate (Tween t)
                {
                    light.Color = Color.Lerp(Color.White, color, t.Eased);
                    light.StartRadius = startFade + (1f - t.Eased) * 32f;
                    light.EndRadius = endFade + (1f - t.Eased) * 32f;
                    outlineSize = sizeFrom + (1f - t.Eased) * 16f;
                    outlineAlpha = alphaFrom + (1 - alphaFrom) * (1f - t.Eased);
                    bloom.Alpha = 0.5f + 0.5f * (1f - t.Eased);
                };
                tween.OnComplete = delegate { outlineTween.Active = true; };
                Add(tween);
                SceneAs<Level>().Session.SetFlag(FlagName);
                State = true;
                if (OnlySpinWhenFirstLit)
                {
                    DoSpinThing();
                }
            }

            if (!OnlySpinWhenFirstLit && !inRoutine && !wasColliding)
            {
                DoSpinThing();
            }
        }
        public void OnPlayer(Player player)
        {
            LightSelf();
            collided = true;
        }
        private void DrawCircles(Color color)
        {
            for (int i = 0; i < Shapes; i++)
            {
                DrawCircle(light.Center, sizes[i] + circleSizeAdd, color * (alphas[i] + alphaAdd), thickness, 1, circleScale);
            }
            if (spinCircleSize > 0 && spinCircleAlpha > 0)
            {
                DrawCircle(light.Center, spinCircleSize, color * spinCircleAlpha, thickness, 2, Vector2.One);
            }
        }
        private void DrawCircle(Vector2 position, float radius, Color color, float thickness, int resolution, Vector2 scale)
        {
            Vector2 vector = Vector2.UnitX * radius;
            Vector2 vector2 = vector.Perpendicular();
            for (int i = 1; i <= resolution; i++)
            {
                float t = thickness * Math.Abs(scale.X);
                Vector2 vector3 = Calc.AngleToVector((float)i * ((float)Math.PI / 2f) / (float)resolution, radius);
                Vector2 vector4 = vector3.Perpendicular();
                Draw.Line(position + vector * scale, position + vector3, color, t);
                Draw.Line(position - vector * scale, position - vector3, color, t);
                Draw.Line(position + vector2, position + vector4 * scale, color, t);
                Draw.Line(position - vector2, position - vector4 * scale, color, t);
                vector = vector3;
                vector2 = vector4;
            }
        }
        public override void Render()
        {
            if (State)
            {
                DrawCircles(Color.White);
                DrawCircle(light.Center, outlineSize, color * outlineAlpha, thickness, 3, Vector2.One);
                DrawCircle(light.Center, Calc.Max(outlineSize - 4, 1f), color * outlineAlpha * 0.8f, 1, 3, Vector2.One);
            }
            else
            {
                DrawCircle(light.Center, outlineSize * 0.7f, color * outlineAlpha * 0.7f, thickness, 1, Vector2.One);
                DrawCircle(light.Center, Calc.Max((outlineSize * 0.7f) - 4, 1f), color * outlineAlpha * 0.5f, 1, 1, Vector2.One);
            }
            sprite.DrawSimpleOutline();
            base.Render();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (!startLit && !Persistent)
            {
                (scene as Level).Session.SetFlag(FlagName, false);
            }
            //helper.RemoveSelf();
        }
        private void DiamondParticles()
        {
            for (int i = 0; i < 8; i++)
            {
                system.Emit(diamond, Position + light.Position + Vector2.One);
            }
        }
        public void SetAlpha(float amount)
        {
            for (int i = 0; i < Shapes; i++)
            {
                alphas[i] = Calc.LerpClamp(0f, 0.3f, amount);
            }
        }
        public void SetCircleSize(float amount)
        {
            for (int i = 0; i < Shapes; i++)
            {
                sizes[i] = Calc.LerpClamp(initialSizes[i], targetRadius, amount);
            }
        }
        public override void Update()
        {
            base.Update();
            if (Scene is Level level)
            {
                State = level.Session.GetFlag(FlagName);
            }
            if (!State && CollideCheck<Actor>())
            {
                LightSelf();
            }
            SafeZone.IsSafe = Mask.Enabled = State;
            Mask.GradientBoost = radius / outlineSize * 1.3f;
            wasColliding = collided;
            collided = false;
        }
        public void DoSpinThing()
        {
            tweensAnimating = true;
            constantTween.Active = false;
            sprite.Play("spin");
            DiamondParticles();
            alphaAdd = circleSizeAdd = 0.1f;
            spinTween.Start();
            Add(new Coroutine(SpeedSpin()));
        }
        private IEnumerator SpeedSpin()
        {
            inRoutine = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.8f)
            {
                circleScale.X = Calc.LerpClamp(1f, -1f, Ease.SineOut(i));
                yield return null;
            }
            circleScale.X = 1;
            inRoutine = false;
        }
        private bool inRoutine;
        private Tween outlineTween;
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            targetRadius = radius / 1.2f;
            Mask = new CritterLight(targetRadius, light);
            Add(Mask);
            for (int i = 0; i < Shapes; i++)
            {
                sizes[i] = i * thickness + i * 10;
                initialSizes[i] = sizes[i];
            }
            constantTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, 2, true);
            spinTween = Tween.Create(Tween.TweenMode.Persist, Ease.SineIn, 0.05f, false);
            Tween spinTweenB = Tween.Create(Tween.TweenMode.Persist, Ease.SineIn, 0.7f, false);
            Tween bigCircleTween = Tween.Create(Tween.TweenMode.Persist, Ease.SineIn, 0.8f, false);
            outlineTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, 2, true);
            outlineTween.OnUpdate = (t) =>
            {
                float amount = Calc.LerpClamp(0.1f, 0.7f, t.Eased);
                outlineSize = Calc.LerpClamp(radius / 1.5f, radius, amount);
                outlineAlpha = Calc.LerpClamp(0.2f, 0.6f, amount);
            };
            constantTween.OnUpdate = (t) =>
            {
                SetCircleSize(Calc.LerpClamp(0.1f, 0.7f, t.Eased));
                SetAlpha(Calc.LerpClamp(0.1f, 0.3f, t.Eased));
            };
            spinTween.OnUpdate = (t) =>
            {
                alphaAdd = Calc.LerpClamp(0.01f, 0.3f, t.Eased);
                circleSizeAdd = Calc.LerpClamp(0.01f, 7f, t.Eased);
            };
            spinTweenB.OnUpdate = (t) =>
            {
                alphaAdd = Calc.LerpClamp(0.3f, 0, t.Eased);
                circleSizeAdd = Calc.LerpClamp(7f, 0, t.Eased);
            };
            bigCircleTween.OnUpdate = (t) =>
            {
                spinCircleSize = Calc.LerpClamp(10, targetRadius * 1.1f, t.Eased);
                spinCircleAlpha = Calc.LerpClamp(0, 1f, (t.Eased > 0.5f ? 0.5f - (t.Eased - 0.5f) : t.Eased) / 0.5f);
            };
            spinTween.OnComplete = (t) =>
            {
                spinTweenB.Start();
            };
            spinTweenB.OnComplete = delegate
            {
                alphaAdd = 0;
                circleSizeAdd = 0;
                constantTween.Active = true;
            };
            bigCircleTween.OnComplete = delegate
            {
                circleScale.X = 1;
                spinCircleSize = 0;
                spinCircleAlpha = 0;
            };
            spinTween.OnStart = delegate
            {
                bigCircleTween.Start();
            };
            Add(outlineTween, constantTween, spinTween, spinTweenB, bigCircleTween, sprite);
            scene.Add(system = new ParticleSystem(Depth, 20));
            system.Position += Vector2.One * 6;
        }
    }
}
