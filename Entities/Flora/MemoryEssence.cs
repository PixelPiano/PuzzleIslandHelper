using Celeste.Mod.CommunalHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [Tracked]
    public class MemoryEssence : Actor
    {
        public bool OnScreen;
        public Vector2 UVPosition;
        public float Life;
        public Vector2 Direction;
        public Vector2 Acceleration;
        public Vector2 Speed;
        public float StartLife;
        public ParticleType.FadeModes FadeMode;
        public Color StartColor = Color.Goldenrod; //218 165 32
        public Color Color = Color.Goldenrod;
        public float Friction;
        public float Radius;
        public float StartRadius;
        public Vector2 UVRadius;
        public float IntroLife = 0.5f;
        public float StartIntroLife;
        public Rectangle Bounds;
        public MemoryEssence(Vector2 position, Vector2 speed, float life, float radius, Vector2 accel = default, float friction = 0) : base(position)
        {
            Collider = new Hitbox(1, 1);
            Speed = speed;
            StartLife = Life = life;
            StartRadius = Radius = radius;
            Acceleration = accel;
            Friction = friction;
            Tag |= Tags.TransitionUpdate | Tags.Persistent;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            IntroLife = StartIntroLife = Calc.Random.Range(0.5f, 1);

        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Point(Position, EssenceRenderer.DebugColor);
        }
        public override void Update()
        {
            base.Update();

            if (Scene is not Level level) return;
            Bounds.X = (int)(Position.X - Radius);
            Bounds.Y = (int)(Position.Y - Radius);
            Bounds.Width = Bounds.Height = (int)Radius * 2;
            float num2;
            if (IntroLife > 0)
            {
                num2 = 1 - (IntroLife / StartIntroLife);
                IntroLife -= Engine.DeltaTime;
            }
            else
            {
                num2 = Life / StartLife;
                Life -= Engine.DeltaTime;
                if (Life <= 0f)
                {
                    RemoveSelf();
                    Active = false;
                    return;
                }
            }
            Radius = Calc.LerpClamp(0, StartRadius, num2);
            OnScreen = Bounds.Colliding(level.Camera.GetBounds(), new Vector2(-8, 16));
            UVPosition = (Position - level.Camera.Position) / new Vector2(320f, 180f);
            UVRadius = Vector2.One * Radius / new Vector2(320f, 180f);
            float num3 = FadeMode switch
            {
                ParticleType.FadeModes.Linear => num2,
                ParticleType.FadeModes.Late => Math.Min(1f, num2 / 0.25f),
                ParticleType.FadeModes.InAndOut => (num2 > 0.75f) ? (1f - (num2 - 0.75f) / 0.25f) : ((!(num2 < 0.25f)) ? 1f : (num2 / 0.25f)),
                _ => 1f
            };
            Color = num3 == 0 ? Color.Transparent : Color.Lerp(Color, StartColor, num2) * Math.Min(num3, 1);
            MoveH(Speed.X * Engine.DeltaTime, delegate { Speed.X = 0; });
            MoveV(Speed.Y * Engine.DeltaTime, delegate { Speed.Y = 0; });
            Speed += Acceleration * Engine.DeltaTime;
            Speed = Calc.Approach(Speed, Vector2.Zero, Friction * Engine.DeltaTime);
        }
    }
}
