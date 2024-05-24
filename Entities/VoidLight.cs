using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using Color = Microsoft.Xna.Framework.Color;

// PuzzleIslandHelper.VoidLight
namespace Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities
{
    [CustomEntity("PuzzleIslandHelper/VoidLight")]
    [Tracked]
    public class VoidLight : Entity
    {
        private Sprite Sprite;
        private int Radius;
        private float Opacity;
        private int StartFade;
        private int EndFade;
        private Color Color;
        private Player player;
        private VertexLight Light;
        private bool spriteTracker;
        private ParticleSystem system;
        private Tween CircleTween;
        private float TargetRadius;
        private float[] Sizes = new float[Shapes];
        private float[] InitialSizes = new float[Shapes];
        private float[] InitialAlphas = new float[Shapes];
        private float[] Alphas = new float[Shapes];
        private static int Shapes = 2;
        private int Thickness = 3;
        public bool InRadius
        {
            get
            {
                if (Scene as Level is null || player is null)
                {
                    return false;
                }
                return player.CollideCheckByComponent<VertexLight>();
            }
        }
        private ParticleType Diamond = new ParticleType
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
        public VoidLight(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/voidLight/");
            Sprite.AddLoop("idle", "centerIdle", 0.1f);
            Sprite.Add("spin", "centerSpin", 0.12f, "idle");
            Add(Sprite);
            Tag |= Tags.TransitionUpdate;
            Depth = 1;
            Vector2 _offset = Vector2.One * 12;
            Radius = data.Int("radius");
            Opacity = data.Float("Alpha");
            StartFade = Radius - Radius / 3;
            EndFade = Radius;
            Color = data.HexColor("color", Color.White);
            Add(new BloomPoint(_offset, Opacity, Radius));
            Add(Light = new VertexLight(_offset, Color, Opacity, StartFade, EndFade));
            Collider = new Hitbox(Radius + 10, Radius + 10, _offset.X - Radius / 2 - 5, _offset.Y - Radius / 2 - 5);
            Sprite.Position += new Vector2(Sprite.Width / 2, Sprite.Height / 2);
            Sprite.Color = Color.Lerp(Color.White, Color.Black, 0.3f);
        }
        private void DrawCircles(Color color)
        {
            for (int i = 0; i < Shapes; i++)
            {
                Draw.Circle(Light.Center, Sizes[i], color * Alphas[i], Thickness, 1);
            }
        }
        public override void Render()
        {
            base.Render();
            DrawCircles(Color.White);
            Sprite.DrawSimpleOutline();
            Sprite.Render();
        }
        private void DiamondParticles()
        {
            for (int i = 0; i < 5; i++)
            {
                system.Emit(Diamond, Position + new Vector2(12, 12));
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = (scene as Level).Tracker.GetEntity<Player>();
            TargetRadius = Radius / 1.7f;

            for (int i = 0; i < Shapes; i++)
            {
                Sizes[i] = i * Thickness + i * 10;
                InitialSizes[i] = Sizes[i];
            }
            CircleTween = Tween.Create(Tween.TweenMode.Looping, null, 1);
            CircleTween.OnUpdate = (t) =>
            {
                for (int i = 0; i < Shapes; i++)
                {
                    Sizes[i] = Calc.LerpClamp(InitialSizes[i], TargetRadius, t.Eased);
                    //Alphas[i] = Calc.LerpClamp(0.3f, 0, t.Eased);
                }

            };
            Tween OpacityTween = Tween.Create(Tween.TweenMode.YoyoLooping, null, 0.5f);
            OpacityTween.OnUpdate = (t) =>
            {
                for (int i = 0; i < Shapes; i++)
                {
                    Alphas[i] = Calc.LerpClamp(0f, 0.3f, t.Eased);
                }
            };
            OpacityTween.OnStart = (t) =>
            {
                if (!spriteTracker)
                {
                    Sprite.Play("spin");
                    DiamondParticles();
                }
                spriteTracker = !spriteTracker;
            };
            Add(OpacityTween);
            Add(CircleTween);
            scene.Add(system = new ParticleSystem(Depth, 20));
            system.Position += Vector2.One * 6;
            CircleTween.Start();
            OpacityTween.Start();
        }
        public override void Update()
        {
            base.Update();
            /*                Size += 2f;
                            Size %= (int)TargetRadius;
                            PulseOpacity = 1 - (1 / MathHelper.Distance(Size, TargetRadius));
                            if (Size == 0)
                            {
                                PulseOpacity = BaseAlpha;
                            }*/
        }
    }
}
