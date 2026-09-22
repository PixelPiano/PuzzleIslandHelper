using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Reflection.Metadata;

// PuzzleIslandHelper.PassiveSecurity
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/PassiveSecurity")]
    [Tracked]
    public class PassiveSecurity : Entity
    {
        public float MaxAngle = MathHelper.TwoPi;
        public float MinAngle = 0;
        private bool Aiming;
        //attributes
        private FlagList Flag;
        private Vector2 Direction => Calc.AngleToVector(Angle, 1);
        private float Angle;
        private Sprite shade;
        private Sprite reveal;
        private Sprite panel;
        private Sprite IntroGun;
        private Sprite Gun;
        private Sprite stand;

        private static ParticleType PSparks = new ParticleType
        {
            Size = 1,
            Color = Color.Orange,
            Color2 = Color.Yellow,
            ColorMode = ParticleType.ColorModes.Choose,
            Direction = -MathHelper.Pi / 2f,
            DirectionRange = MathHelper.PiOver2,
            LifeMin = 0.06f,
            LifeMax = 0.5f,
            SpeedMin = 20f,
            SpeedMax = 30f,
            SpeedMultiplier = 0.25f,
            FadeMode = ParticleType.FadeModes.Late,
            Friction = 2f
        };
        private Vector2 StationaryDirection;
        private Vector2 CircleCenter = Vector2.Zero;
        public bool ResetFlagOnRemoved = true;
        private Sprite BulletSprite;
        private int Bullets;
        public float ShootInterval;
        public PassiveSecurity(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            ShootInterval = data.Float("shootInterval", 0.5f);
            MinAngle = data.Float("minAngle", 0).ToRad();
            MaxAngle = data.Float("maxAngle", 360).ToRad();
            float xDir = data.Float("stationaryDirectionX");
            float yDir = data.Float("stationaryDirectionY");
            if (xDir != 0 || yDir != 0)
            {
                StationaryDirection = Vector2.Normalize(new Vector2(xDir, yDir));
            }
            Bullets = data.Int("bulletsPerShot");
            Flag = data.FlagList("flag");
            Depth = -10002;
            Add(coroutine = new Coroutine(false));
            Add(shakeCoroutine = new Coroutine(false));
        }

        private bool prevState;
        public override void Update()
        {
            base.Update();
            Gun.Position = gunOrig + GunPositionOffset;
            if (StationaryDirection != Vector2.Zero)
            {
                Angle = Math.Clamp(StationaryDirection.Angle() % MathHelper.TwoPi, MinAngle, MaxAngle);
            }
            else if (Scene.GetPlayer() is Player player)
            {
                Angle = Calc.Angle(Position + Gun.Position, player.Center);
            }
            bool flag = Flag;
            if (flag)
            {
                if (!prevState)
                {
                    Aiming = false;
                    ResetSprites();
                    coroutine.Replace(IntroRoutine());
                }
                if (Aiming)
                {
                    BulletSprite.Rotation = Gun.Rotation = Calc.AngleLerp(Gun.Rotation, Angle + MathHelper.Pi, Engine.DeltaTime * 2);
                }
            }
            else if (prevState && Aiming)
            {
                Aiming = false;
            }
            prevState = flag;

        }
        private Coroutine coroutine, shakeCoroutine;
        private Vector2 gunOrig;
        public override void Added(Scene scene)
        {
            base.Added(scene);

            shade = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/passiveSecurity/");
            reveal = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/passiveSecurity/");
            panel = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/passiveSecurity/");
            IntroGun = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/passiveSecurity/");
            stand = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/passiveSecurity/");
            Gun = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/passiveSecurity/");

            stand.AddLoop("idle", "stand", 0.1f);
            shade.Add("fadeIn", "shade", 0.04f);
            reveal.Add("moveDown", "slideDown", 0.02f, "down");
            reveal.Add("tilePress", "reveal", 0.04f, "moveDown");
            reveal.AddLoop("down", "down", 1f);
            panel.AddLoop("idle", "panel", 1f);
            IntroGun.AddLoop("idle", "verySafe", 0.1f);
            IntroGun.AddLoop("raised", "gunRaise", 0.1f, 4);
            IntroGun.Add("rise", "gunRaise", 0.07f, "raised");

            Gun.AddLoop("idle", "gun", 0.1f);

            BulletSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/passiveSecurity/bullet/");
            BulletSprite.AddLoop("grow", "grow", 0.06f);
            BulletSprite.AddLoop("standby", "standby", 0.1f);
            IntroGun.Position.Y -= 8;
            stand.Position.Y -= 8;
            gunOrig = new Vector2(8 + Gun.Width / 2, -3 + Gun.Height / 2);
            Gun.Position = gunOrig;
            Gun.CenterOrigin();
            BulletSprite.CenterOrigin();
            BulletSprite.Position = Gun.Position;
            Add(panel);
            Add(stand);
            Add(IntroGun);
            Add(Gun);
            Add(BulletSprite);
            Add(shade);
            Add(reveal);
            if (Flag)
            {
                Activate();
                if (StationaryDirection != Vector2.Zero)
                {
                    Angle = Math.Clamp(StationaryDirection.Angle() % MathHelper.TwoPi, MinAngle, MaxAngle);
                }
                BulletSprite.Rotation = Gun.Rotation = Angle;

            }
            prevState = Flag;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (ResetFlagOnRemoved && !Flag.Empty)
            {
                Flag.State = false;
            }
        }
        private void EmitSparks()
        {
            for (int i = 0; i < 6; i++)
            {
                SceneAs<Level>().ParticlesFG.Emit(PSparks, Gun.RenderPosition);
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Collider = new Hitbox(panel.Width, panel.Height);
        }
        private Vector2 CircleCoords(float theta)
        {
            CircleCenter = Position + Gun.Position - Vector2.One * 4;
            double x = CircleCenter.X + Gun.Width / 2 * Math.Cos(theta);
            double y = CircleCenter.Y + Gun.Width / 2 * Math.Sin(theta);
            return new Vector2((float)x, (float)y);
        }
        private IEnumerator ShootRoutine()
        {
            yield return 0.2f;
            Level level = Scene as Level;
            while (true)
            {
                while (!Aiming) yield return null;
                float duration = ShootInterval;
                #region sprite
                if (duration - 0.3f > 0f)
                {
                    duration -= 0.3f;
                    BulletSprite.Color = Color.White * 0.6f;
                    BulletSprite.Play("grow");
                    yield return 0.3f;
                    BulletSprite.Color = Color.Transparent;
                }
                #endregion
                if (Bullets > 1)
                {
                    float dir = MathHelper.PiOver2 / Bullets;
                    for (int i = 0; i < Bullets; i++)
                    {
                        Bullet bullet = new Bullet(
                            CircleCoords(Angle) + new Vector2(2, 2),
                            Direction.Rotate(dir * (i - Bullets / 2)));

                        level.Add(bullet);
                    }
                }
                else
                {
                    Bullet bullet = new Bullet(
                        CircleCoords(Angle) + new Vector2(2, 2),
                        Direction);

                    level.Add(bullet);
                }
                shakeCoroutine.Replace(ShakeGun(Bullets));
                yield return duration;
            }
        }
        private IEnumerator IntroRoutine()
        {
            reveal.Play("tilePress");
            while (reveal.CurrentAnimationID != "down") yield return null;
            panel.Play("idle");
            IntroGun.Play("idle");
            shade.Play("fadeIn");
            while (!LastFrame(shade)) yield return null;
            IntroGun.Play("rise");
            while (!LastFrame(IntroGun)) yield return null;
            IntroGun.Visible = false;
            Activate();
        }
        public void ResetSprites()
        {
            reveal.Stop();
            panel.Stop();
            shade.Stop();
            IntroGun.Stop();
            Gun.Stop();
            stand.Stop();
            reveal.Texture = panel.Texture = shade.Texture = IntroGun.Texture = Gun.Texture = stand.Texture = null;
        }
        public void Activate()
        {
            reveal.Play("down");
            panel.Play("idle");
            shade.Play("fadeIn");
            shade.SetAnimationFrame(shade.Animations["fadeIn"].Frames.Length - 1);
            IntroGun.Play("rise");
            IntroGun.SetAnimationFrame(IntroGun.Animations["rise"].Frames.Length - 1);
            IntroGun.Visible = false;
            stand.Play("idle");
            Gun.Play("idle");
            Aiming = true;
            coroutine.Replace(ShootRoutine());
        }
        public void Deactivate()
        {
            Aiming = false;
            coroutine.Cancel();
            ResetSprites();
        }
        public Vector2 GunPositionOffset;
        private IEnumerator ShakeGun(int bullets)
        {
            int limit = bullets < 6 ? 2 : 4;
            if (bullets >= 5)
            {
                EmitSparks();
            }
            for (int i = 0; i < 4; i++)
            {
                GunPositionOffset = new Vector2(Calc.Random.Range(-limit, limit), Calc.Random.Range(-limit, limit));
                yield return null;
                GunPositionOffset = default;
            }
        }
        private bool LastFrame(Sprite sprite)
        {
            return sprite.CurrentAnimationFrame == sprite.CurrentAnimationTotalFrames - 1;
        }
    }

}