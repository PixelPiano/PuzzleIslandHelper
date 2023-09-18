using Celeste.Mod.Entities;
using Celeste.Mod.FancyTileEntities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Windows;

// PuzzleIslandHelper.PassiveSecurity
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/PassiveSecurity")]
    [Tracked]
    public class PassiveSecurity : Entity
    {
        private Player player;
        public enum Mode
        {
            LaserActivated,
            Monitoring,
            Stationary
        }
        public Mode mode;
        private bool RealisticAim;
        public bool ForcedState;
        private bool State
        {
            get
            {
                Level level = Scene as Level;

                if (level is null)
                {
                    return false;
                }
                if (ForceState)
                {
                    return ForcedState;
                }
                switch (mode)
                {
                    case Mode.LaserActivated:
                        if (!string.IsNullOrEmpty(LaserID))
                        {
                            return SecurityLaser.Alert;
                        }
                        else
                        {
                            return false;
                        }
                    case Mode.Monitoring:
                        if (inverted)
                        {
                            return !level.Session.GetFlag(flag);
                        }
                        else
                        {
                            return level.Session.GetFlag(flag);
                        }
                    case Mode.Stationary:
                        if (inverted)
                        {
                            return !level.Session.GetFlag(flag);
                        }
                        else
                        {
                            return level.Session.GetFlag(flag);
                        }
                }
                return false;



            }
        }
        private bool Set;
        private bool Activated;
        private float Delay;
        private float Progress;
        private bool XFlip
        {
            get
            {
                if (mode == Mode.LaserActivated)
                {
                    return false;
                }
                return _XFlip;
            }
        }
        //attributes
        private string flag;
        private bool inverted;
        private Vector2 Direction;
        //
        private Sprite shade;
        private Sprite reveal;
        private Sprite panel;
        private Sprite IntroGun;
        private Sprite Gun;
        private Sprite stand;

        private ParticleType Sparks = new ParticleType
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
        private ParticleSystem system;
        public string LaserID;
        private bool Cancelled;
        private Coroutine shootRoutine;
        public bool ForceState;
        private bool StopRotating;
        private bool _XFlip;
        private float RotateRange;
        private float ViewRange;
        private float LookTime;
        private bool SawPlayer;
        private Vector2 MonitorPosition;
        private bool StartState;
        private Vector2 CircleCenter = Vector2.Zero;
        private Bullet.BulletType BulletType;

        private Sprite BulletSprite;
        private int GrowLoops;
        private int Bullets;

        private bool BulletCollideWithSolid;
        public PassiveSecurity(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            BulletCollideWithSolid = data.Bool("bulletsCollideWithSolids");
            Bullets = data.Int("bulletsPerShot");
            BulletType = data.Enum("bulletType", Bullet.BulletType.Default);
            RealisticAim = data.Bool("realisticAim");
            mode = data.Enum("mode", Mode.LaserActivated);
            _XFlip = data.Bool("flipX");
            switch (mode)
            {
                case Mode.LaserActivated:
                    LaserID = data.Attr("laserID");
                    break;
                case Mode.Monitoring:
                    RotateRange = data.Float("rotateRange", 5);
                    ViewRange = data.Float("viewRange", 5);
                    LookTime = data.Float("lookTime", 4);
                    break;
            }
            Delay = Calc.Random.Range(0.2f, 0.5f);
            Progress = Delay;
            flag = data.Attr("flag");
            inverted = data.Bool("inverted");
            Depth = -10002;
        }
        private void CheckState()
        {
            if (shootRoutine is not null && shootRoutine.Active && !Cancelled)
            {
                if ((XFlip && player.Position.X < Center.X) || (!XFlip && player.Position.X > Center.X))
                {
                    shootRoutine.Cancel();
                    Remove(shootRoutine);
                    Progress = Delay;
                    Cancelled = true;
                }
            }
            if (Cancelled)
            {
                if ((XFlip && player.Position.X >= Center.X) || (!XFlip && player.Position.X <= Center.X))
                {
                    Add(shootRoutine = new Coroutine(ShootRoutine()));
                    Cancelled = false;
                }
            }
        }
        public override void Update()
        {
            base.Update();
            if (!State)
            {
                return;
            }
            if (RealisticAim)
            {
                if (mode != Mode.LaserActivated)
                {
                    CheckState();
                }
            }
            if ((!Activated && !Set) || StartState)
            {
                Add(new Coroutine(IntroRoutine(StartState)));
            }
            if (Set)
            {
                Vector2 start = Position + Gun.Position;
                Vector2 end = Vector2.Zero;
                if (mode == Mode.LaserActivated)
                {
                    end = player.Center;
                }
                Direction = Vector2.Normalize(end - start);
                Gun.Rotation = Calc.AngleLerp(Gun.Rotation, Direction.Angle() + MathHelper.Pi, Engine.DeltaTime * 4f);
                BulletSprite.Rotation = Gun.Rotation;
            }

        }

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
            shade.AddLoop("wait", "shade", 1f, 0);
            reveal.Add("tilePress", "reveal", 0.04f);
            reveal.AddLoop("down", "down", 1f);
            reveal.Add("moveDown", "slideDown", 0.02f, "down");
            panel.AddLoop("idle", "panel", 1f);
            IntroGun.AddLoop("idle", "verySafe", 0.1f);
            IntroGun.AddLoop("raised", "gunRaise", 0.1f, 4);
            IntroGun.Add("rise", "gunRaise", 0.07f, "raised");

            Gun.AddLoop("idle", "gun", 0.1f);

            BulletSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/passiveSecurity/bullet/");
            BulletSprite.AddLoop("grow", "grow", 0.06f);
            BulletSprite.AddLoop("standby", "standby", 0.1f);

            if (XFlip)
            {
                BulletSprite.FlipX = true;
                BulletSprite.Position.X -= 6;
            }
            BulletSprite.OnLastFrame = delegate
            {
                if (BulletSprite.CurrentAnimationID == "grow")
                {
                    GrowLoops++;
                }
                else
                {
                    GrowLoops = 0;
                }
            };
            IntroGun.Position.Y -= 8;
            stand.Position.Y -= 8;
            Gun.Position += new Vector2(8 + Gun.Width / 2, -3 + Gun.Height / 2);
            stand.FlipX = shade.FlipX = reveal.FlipX = panel.FlipX = IntroGun.FlipX = Gun.FlipX = XFlip;
            if (XFlip)
            {
                Gun.Position.X -= 10;
                stand.Position.X -= 4;
                IntroGun.Position.X -= 1;
            }
            Gun.CenterOrigin();

            BulletSprite.CenterOrigin();
            BulletSprite.Position = Gun.Position;
            if (XFlip)
            {
                MonitorPosition = Position + Gun.Position + new Vector2(Gun.Width, 2);
            }
            else
            {
                MonitorPosition = Position + Gun.Position + new Vector2(Gun.Width, 2);
            }
            Add(panel);
            Add(stand);
            Add(IntroGun);
            Add(Gun);
            Add(BulletSprite);
            Add(shade);
            Add(reveal);
            if (mode == Mode.Stationary)
            {
                StartState = true;
            }
            system = new ParticleSystem(Depth, 10);
            scene.Add(system);
        }
        private void EmitSparks()
        {
            for(int i = 0; i<6; i++)
            {
                system.Emit(Sparks, Position + Gun.Position);
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            player = (scene as Level).Tracker.GetEntity<Player>();
            Collider = new Hitbox(panel.Width, panel.Height);
            if (mode == Mode.Monitoring)
            {
                Add(new Coroutine(RotateRoutine()));
            }
        }
        private IEnumerator RotateRoutine()
        {
            while (true)
            {
                for (float i = 0; i < LookTime; i += Engine.DeltaTime)
                {
                    Gun.Rotation = Calc.LerpClamp(-RotateRange / 2, RotateRange / 2, Ease.SineInOut(i / LookTime));
                    yield return null;
                }
                for (float i = 0; i < LookTime; i += Engine.DeltaTime)
                {
                    Gun.Rotation = Calc.LerpClamp(RotateRange / 2, -RotateRange / 2, Ease.SineInOut(i / LookTime));
                    yield return null;
                }
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (mode == Mode.Monitoring)
            {
                Draw.LineAngle(VisionCoords(Direction.Angle().ToDeg(), true), Gun.Rotation + 180f, 20, Color.Magenta);
                //Draw.Line(Position + (Gun.Position * Gun.Rotation), player.Center, Color.Magenta);
            }

        }
        private Vector2 VisionCoords(float theta, bool top)
        {
            Vector2 CircleCenter = Position + Gun.Position;
            double x = CircleCenter.X + (Gun.Width / 2) * Math.Cos(theta * Math.PI / 180);
            double y = CircleCenter.Y + (Gun.Width / 2) * Math.Sin(theta * Math.PI / 180);
            return new Vector2((float)x, (float)y);
        }
        private Vector2 CircleCoords(float theta)
        {
            CircleCenter = Position + Gun.Position - Vector2.One * 4;
            double x = CircleCenter.X + (Gun.Width / 2) * Math.Cos(theta * Math.PI / 180);
            double y = CircleCenter.Y + (Gun.Width / 2) * Math.Sin(theta * Math.PI / 180);
            return new Vector2((float)x, (float)y);
        }
        private IEnumerator ShootRoutine()
        {
            while (!Set)
            {
                yield return null;
            }
            StopRotating = true;
            if (mode == Mode.Monitoring)
            {
                while (!SawPlayer)
                {
                    yield return null;
                }
                //Gun.Play("alert") or something
                yield return 0.1f;
            }
            while (true)
            {
                if (Scene as Level is null || !State)
                {
                    yield break;
                }
                Level level = Scene as Level;
                #region sprite
                BulletSprite.Color = Color.White * 0.6f;
                float delay = Calc.Random.Range(0f, 0.3f);
                BulletSprite.Play("grow");
                while (delay > 0)
                {
                    delay -= Engine.DeltaTime;
                    yield return null;
                }
                delay = Calc.Random.Range(0.1f, 0.4f);
                BulletSprite.Play("standby");
                while (delay > 0)
                {
                    delay -= Engine.DeltaTime;
                    yield return null;
                }
                BulletSprite.Color = Color.White * 0;

                Vector2 BulletPosition = new Vector2(3, -2) + Position;
                if (XFlip)
                {
                    BulletPosition.X += 23;
                }
                #endregion
                if(Bullets > 1)
                {

                    float Dir = MathHelper.PiOver2 / Bullets;
                    for (int i = 0; i < Bullets; i++)
                    {
                        Bullet bullet = new Bullet(
                            CircleCoords(Direction.Angle().ToDeg()),
                            Direction.Rotate(Dir * (i-(Bullets/2))),
                            XFlip,
                            BulletCollideWithSolid,
                            BulletType);

                        level.Add(bullet);
                    }
                }
                else
                {
                    Bullet bullet = new Bullet(
                        CircleCoords(Direction.Angle().ToDeg()),
                        Direction,
                        XFlip,
                        BulletCollideWithSolid,
                        BulletType);

                    level.Add(bullet);
                }
                Add(new Coroutine(Recoil(Direction)));
                Add(new Coroutine(ShakeGun(Bullets)));
                while (Progress > 0)
                {
                    Progress -= Engine.DeltaTime;
                    yield return null;
                }
                Progress = Calc.Random.Range(0.1f, 0.4f);
            }
        }
        private IEnumerator IntroRoutine(bool skip)
        {
            Activated = true;
            StartState = false;
            if (!skip)
            {
                reveal.Play("tilePress");

                while (!LastFrame(reveal))
                {
                    yield return null;
                }
                reveal.Play("moveDown");
                while (!LastFrame(reveal))
                {
                    yield return null;
                }

                panel.Play("idle");
                IntroGun.Play("idle");
                shade.Play("fadeIn");
                while (!LastFrame(shade))
                {
                    yield return null;
                }
                IntroGun.Play("rise");

                while (!LastFrame(IntroGun))
                {
                    yield return null;
                }
                Remove(IntroGun);
            }
            else
            {
                reveal.Play("down");
                panel.Play("idle");
            }
            stand.Play("idle");
            Gun.Play("idle");
            //SETUP COMPLETE
            Set = true;

            Add(shootRoutine = new Coroutine(ShootRoutine()));
            yield return null;
        }
        private IEnumerator ShakeGun(int Bullets)
        {
            int limit = Bullets < 6 ? Bullets : 5; 
            Vector2 Pos = Gun.Position;
            if(Bullets >= 5)
            {
                EmitSparks();
            }
            for(int i = 0; i < 4; i++)
            {
                Vector2 Random = new Vector2(Calc.Random.Range(-limit,limit),Calc.Random.Range(-limit,limit));
                Gun.Position = Pos + Random;
                yield return null;
                Gun.Position = Pos;
            }
            Gun.Position = Pos;
        }
        private IEnumerator Recoil(Vector2 direction)
        {
            Vector2 Pos = Gun.Position;
            Vector2 BPos = BulletSprite.Position;
            //float direction = XFlip ? -3 : 3;
            int limit = 3;

            for (float i = 0; i < limit; i += Engine.DeltaTime * limit * 10)
            {
                Gun.Position = Pos - direction * i;
                BulletSprite.Position = BPos - direction * i;
                yield return null;
            }
            Pos = Gun.Position;
            BPos = BulletSprite.Position;
            for (float i = 0; i < limit; i += Engine.DeltaTime * limit * 10)
            {
                Gun.Position = Pos + direction * i;
                BulletSprite.Position = BPos + direction * i;
                yield return null;
            }
        }
        private bool LastFrame(Sprite sprite)
        {
            return sprite.CurrentAnimationFrame == sprite.CurrentAnimationTotalFrames - 1;
        }
    }

}