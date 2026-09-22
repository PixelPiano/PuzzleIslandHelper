using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public class Bullet : Entity
    {
        public Sprite Sprite;
        private static float Rate = 180f;
        private Vector2 Direction;
        private BetterShaker shaker;
        private static ParticleType PTrail = new ParticleType
        {
            ColorMode = ParticleType.ColorModes.Choose,
            LifeMin = 0.4f,
            LifeMax = 0.6f,
            FadeMode = ParticleType.FadeModes.Linear,
            Size = 1,
            Color = Color.Blue,
            Color2 = Color.Cyan,
            SpeedMin = Rate * 0.1f,
            SpeedMax = Rate * 0.2f
        };
        private static ParticleType PBurst = new ParticleType
        {
            ColorMode = ParticleType.ColorModes.Choose,
            LifeMin = 0.1f,
            LifeMax = 0.2f,
            FadeMode = ParticleType.FadeModes.Linear,
            Size = 1,
            Direction = MathHelper.PiOver2,
            DirectionRange = MathHelper.Pi,
            Color = Color.LightBlue,
            Color2 = Color.Blue,
            SpeedMin = Rate * 0.1f,
            SpeedMax = Rate * 0.3f

        };
        private void TrailEmit()
        {
            SceneAs<Level>().ParticlesFG.Emit(PTrail, 1, Center, Vector2.One * 3, Direction.Angle());
        }
        private void BurstEmit()
        {
            for (float i = 0; i < MathHelper.TwoPi; i += MathHelper.TwoPi / 3f)
            {
                SceneAs<Level>().ParticlesFG.Emit(PBurst, 10, Center, new Vector2(Width, Height), PBurst.Direction + i);
            }
        }
        public Bullet(Vector2 position, Vector2 direction) : base(position)
        {
            Direction = direction;
            Depth = -10002;
            Collidable = false;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(shaker = new BetterShaker(v =>
            {
                Sprite.Position += v;
            }));
            shaker.Interval = Engine.DeltaTime;
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/passiveSecurity/bullet/");
            Sprite.AddLoop("ready", "ready", 0.1f);
            Sprite.Add("flash", "readyFlash", 0.06f, "ready");
            Sprite.Add("burst", "burst", 0.03f);
            Add(Sprite);
            Sprite.Play("flash");
            Sprite.CenterOrigin();
            Collider = Sprite.ColliderCentered();
            Collider.Width -= 2;
            Collider.Height -=2;
            Collider.Position += Vector2.One;
        }
        public override void Render()
        {
            Sprite.DrawSimpleOutline();
            base.Render();
        }
        public bool Bursting;
        public override void Update()
        {
            Level level = SceneAs<Level>();
            Player player = level?.GetPlayer();
            if (player != null && player.Dead) return;
            base.Update();
            if (Bursting)
            {
                Sprite.Scale += Vector2.One * 2f * Engine.DeltaTime;
                return;
            }
            Position += Direction * Rate * Engine.DeltaTime;
            if (!level.IsInBounds(this))
            {
                RemoveSelf();
            }
            else
            {
                if (Collidable)
                {
                    Depth = 1;
                    if (CollideCheck<Solid>())
                    {
                        Burst();
                    }
                }
                else
                {
                    Collidable = true;
                    if (!CollideCheck<Solid>())
                    {
                        Depth = 1;
                    }
                    else
                    {
                        Collidable = false;
                    }
                }
            }
            if (CollideCheck(player))
            {
                Burst();
                player.Die(Direction);
            }
            if (!Bursting)
            {
                if (Scene.OnInterval(0.05f))
                {
                    TrailEmit();
                }
            }
        }
        public void Burst()
        {
            Bursting = true;
            BurstEmit();
            shaker.ShakeFor(-1);
            Sprite.Play("burst");
            Sprite.OnLastFrame = (s) =>
            {
                if (s == "burst") RemoveSelf();
            };
        }

    }

}