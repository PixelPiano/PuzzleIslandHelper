using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities
{
    public class Bullet : Entity
    {
        public Sprite Sprite;
        private Level level;
        private Player player;

        public enum BulletType
        {
            Hot,
            Sticky,
            Bouncy,
            Default
        }
        private BulletType type;
        private bool BreakFromDash;
        private bool HitPlayer;
        private bool Collided;
        private float Rate = 3;
        public Vector2 direction;
        public Action OnShot;
        private Vector2 Direction;
        private bool CollideSolids;
        private bool SecondCollision;
        private bool CheckForCollision;
        private ParticleSystem system;
        private ParticleType Trail = new ParticleType
        {
            ColorMode = ParticleType.ColorModes.Choose,
            LifeMin = 0.6f,
            LifeMax = 2f,
            FadeMode = ParticleType.FadeModes.Linear,
            Size = 1
        };
        private ParticleType Burst = new ParticleType
        {
            ColorMode = ParticleType.ColorModes.Choose,
            LifeMin = 0.2f,
            LifeMax = 0.5f,
            FadeMode = ParticleType.FadeModes.Late,
            Size = 1,
            Direction = MathHelper.PiOver2,
            DirectionRange = MathHelper.Pi,
        };
        private void TrailEmit(bool condition)
        {
            if (!condition || player is null)
            {
                return;
            }
            system.Emit(Trail, 1, Center, Vector2.One * 3, (Position - Direction).Angle());
        }
        private void BurstEmit()
        {
            for (int i = 0; i < 360; i += 120)
            {
                system.Emit(Burst, 10, Center, new Vector2(Width, Height), Burst.Direction + MathHelper.ToDegrees(i));
            }
        }
        public Bullet(Vector2 Position, Vector2 direction, bool XFlip = false,
            bool CollideSolids = false, BulletType BulletType = BulletType.Default)
        {
            this.CollideSolids = CollideSolids;
            type = BulletType;
            this.Position = Position;
            Direction = direction;
            Depth = -10002;
            Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/passiveSecurity/bullet/");
            Sprite.AddLoop("ready", "ready", 0.1f);
            Sprite.Add("flash", "readyFlash", 0.06f, "ready");
            Sprite.Add("burst", "burst", 0.04f);
            Add(Sprite);
            Color BulletColor = Color.Blue;
            Trail.Color = BulletColor;
            Trail.Color2 = Color.Lerp(BulletColor, Color.White, 0.5f);
            Burst.Color = Color.Lerp(BulletColor, Color.White, 0.7f);
            Burst.Color2 = BulletColor;
            if (XFlip)
            {
                Sprite.FlipX = true;
                this.Position.X -= 6;
            }
            Collider = new Hitbox(Sprite.Width, Sprite.Height);
            system = new ParticleSystem(Depth + 1, 10);
            Trail.SpeedMax = Rate;
            Trail.SpeedMin = Rate / 3f;
            Burst.SpeedMax = Rate * 3;
            Burst.SpeedMin = Rate * 1.5f;

        }
        private void StartMotion()
        {
            if (player is null)
            {
                return;
            }
            Sprite.Play("flash");
            Vector2 start = Center;
            Vector2 end = player.Center;
            Position = start - Vector2.UnitY * 2;
        }
        private int PlayState()
        {
            //0: Moving in level
            //1: Went past the level bounds
            //2: Hit the Player
            Level level = Scene as Level;
            if (level is null)
            {
                return 0;
            }
            if (Position.X < level.Bounds.Left - Sprite.Width || Position.X > level.Bounds.Right + Sprite.Width || Position.Y < level.Bounds.Top - Sprite.Height || Position.Y > level.Bounds.Bottom + Sprite.Height)
            {
                return 1;
            }
            if (!HitPlayer)
            {

                if (CollideSolids)
                {
                    if (!CollideCheck<Solid>() && !CheckForCollision)
                    {
                        CheckForCollision = true;
                    }
                    else if (CollideCheck<Solid>() && CheckForCollision)
                    {
                        Depth = 1;
                        Collided = true;
                        BurstEmit();
                        return 2;
                    }
                }
                if (player is not null)
                {
                    if (CollideCheck<Player>())
                    {
                        HitPlayer = true;
                        BurstEmit();
                        player.Die(direction);
                        return 2;
                    }
                }
            }
            return 0;
        }
        public override void Render()
        {
            if (!HitPlayer && !(CollideSolids && SecondCollision))
            {
                Sprite.DrawSimpleOutline();
            }
            base.Render();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = scene as Level;
            player = level.Tracker.GetEntity<Player>();
            scene.Add(system);
            if (OnShot is not null)
            {
                OnShot();
            }
            StartMotion();
        }
        public override void Update()
        {
            base.Update();
            if (HitPlayer || Collided)
            {
                return;
            }

            switch (PlayState())
            {
                case 1:
                    RemoveSelf();
                    break;
                case 2:
                    Sprite.Color = Color.Violet;
                    Sprite.Play("burst");

                    Sprite.OnLastFrame = delegate
                    {
                        RemoveSelf();
                    };
                    return;
            }
            Position += Direction * Rate;
            TrailEmit(Scene.OnInterval(2f / 60f));
        }
    }

}