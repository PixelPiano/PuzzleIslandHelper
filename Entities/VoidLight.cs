using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using Color = Microsoft.Xna.Framework.Color;

// PuzzleIslandHelper.VoidLight
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    internal class VoidLightHelperEntity : Entity
    {
        public bool State => Parent.State;
        public VoidLight Parent;
        internal VoidLightHelperEntity(VoidLight parent, Vector2 position, float width, float height) : base(position)
        {
            Parent = parent;
            Collider = new Hitbox(width, height);
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
        private VoidLightHelperEntity helper;
        public string FlagName
        {
            get
            {
                return "voidlight_" + id.Key;
            }
        }
        private bool collided;
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
        public VoidLight(EntityData data, Vector2 offset, EntityID id)
        : base(data.Position + offset)
        {
            this.id = id;
            node = data.Nodes[0];
            Tag |= Tags.TransitionUpdate;
            Depth = 1;
            radius = data.Int("radius");
            opacity = data.Float("alpha");
            startFade = radius - radius / 3;
            endFade = radius;
            color = data.HexColor("color", Color.White);
            helper = new(this, Position, data.Width, data.Height);
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
            Add(sprite);
            Add(bloom = new BloomPoint(offset, opacity * 0.5f, radius));
            Add(light = new VertexLight(offset, color, opacity, startFade, endFade));
            sprite.Position += offset - new Vector2(sprite.Width / 2, sprite.Height / 2);
            sprite.Color = Color.Lerp(Color.White, Color.Black, 0.3f);
            State = scene is Level level && level.Session.GetFlag(FlagName);
            if (State)
            {
                sprite.Play("idle");
            }
            else
            {
                bloom.Visible = false;
                light.Visible = false;
                sprite.Play("off");
            }
            Collider = new Hitbox(sprite.Width, sprite.Height, sprite.Position.X, sprite.Position.Y);
            scene.Add(helper);
            Add(new PlayerCollider(OnPlayer));
        }
        private bool wasColliding;
        public void OnPlayer(Player player)
        {
            if (!State)
            {
                Audio.Play("event:/game/05_mirror_temple/torch_activate", sprite.RenderPosition);
                bloom.Visible = true;
                light.Visible = true;
                Collidable = false;
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BackOut, 1f, start: true);
                tween.OnUpdate = delegate (Tween t)
                {
                    light.Color = Color.Lerp(Color.White, color, t.Eased);
                    light.StartRadius = startFade + (1f - t.Eased) * 32f;
                    light.EndRadius = endFade + (1f - t.Eased) * 32f;
                    bloom.Alpha = 0.5f + 0.5f * (1f - t.Eased);
                };
                Add(tween);
                SceneAs<Level>().Session.SetFlag(FlagName);
                State = true;
            }
            if (!inRoutine && !wasColliding)
            {
                DoSpinThing(player);
            }
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
            base.Render();
            if (State)
            {
                DrawCircles(Color.White);
            }
            sprite.DrawSimpleOutline();
            sprite.Render();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            helper.RemoveSelf();
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
            wasColliding = collided;
            collided = false;
        }
        public void DoSpinThing(Player player)
        {
            tweensAnimating = true;
            constantTween.Active = false;
            sprite.Play("spin");
            DiamondParticles();
            alphaAdd = circleSizeAdd = 0.1f;
            spinTween.Start();
            Add(new Coroutine(SpeedSpin(player)));
        }
        private IEnumerator SpeedSpin(Player player)
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
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            targetRadius = radius / 1.2f;

            for (int i = 0; i < Shapes; i++)
            {
                sizes[i] = i * thickness + i * 10;
                initialSizes[i] = sizes[i];
            }
            constantTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, 2, true);
            spinTween = Tween.Create(Tween.TweenMode.Persist, Ease.SineIn, 0.05f, false);
            Tween spinTweenB = Tween.Create(Tween.TweenMode.Persist, Ease.SineIn, 0.7f, false);
            Tween bigCircleTween = Tween.Create(Tween.TweenMode.Persist, Ease.SineIn, 0.8f, false);
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
            spinTweenB.OnComplete = (t) =>
            {
                alphaAdd = 0;
                circleSizeAdd = 0;
                constantTween.Active = true;
            };
            bigCircleTween.OnComplete = (t) =>
            {
                circleScale.X = 1;
                spinCircleSize = 0;
                spinCircleAlpha = 0;
            };
            spinTween.OnStart = (t) =>
            {
                bigCircleTween.Start();
            };
            Add(constantTween, spinTween, spinTweenB, bigCircleTween);
            scene.Add(system = new ParticleSystem(Depth, 20));
            system.Position += Vector2.One * 6;
        }
    }
}
