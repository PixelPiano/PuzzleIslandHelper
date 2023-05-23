using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ArtifactSlot")]
    public class ArtifactSlot : Entity
    {

        private int mode;

        private readonly Sprite sprite;

        private Entity pop;

        private Sprite popSprite;

        private static VirtualRenderTarget _buffer;

        public static VirtualRenderTarget buffer => _buffer ??=
                      VirtualContent.CreateRenderTarget("buffer", 320, 180);

        private bool start;

        public Player player;

        private ParticleType Sparks = new ParticleType
        {
            Size = 1f,
            Color = Calc.HexToColor("fa7f00"),
            Color2 = Calc.HexToColor("ffbc47"),
            ColorMode = ParticleType.ColorModes.Choose,
            Direction = -MathHelper.Pi/2f,
            DirectionRange = MathHelper.PiOver2,
            LifeMin = 0.06f,
            LifeMax = 0.5f,
            SpeedMin = 20f,
            SpeedMax = 30f,
            SpeedMultiplier = 0.25f,
            FadeMode = ParticleType.FadeModes.Late,
            Friction = 2f
        };

        private ParticleType Download = new ParticleType
        {
            Size = 1f,
            Color = Calc.HexToColor("67ff6e"),
            Color2 = Calc.HexToColor("35b37b"),
            ColorMode = ParticleType.ColorModes.Choose,
            Direction = -(float)Math.PI / 2f,
            DirectionRange = (float)Math.PI / 6f,
            LifeMin = 0.06f,
            LifeMax = 1f,
            SpeedMin = 25f,
            SpeedMax = 30f,
            SpeedMultiplier = 0.40f,
            FadeMode = ParticleType.FadeModes.Late,
            Friction = 2f
        };

        public bool playerNear;

        private Entity artifact;

        private Entity artifactGlow;

        private Entity arms;

        private Entity glitch;

        private Sprite glitchSprite;

        private Sprite armSprite;

        private Sprite glowSprite;

        private Sprite artifactSprite;

        private Vector2 tempScale;

        private void AppearParticles(bool sparks)
        {
            ParticleSystem particlesBG = SceneAs<Level>().ParticlesBG;
            if (sparks)
            {
                particlesBG.Depth = -10500;
                for (int i = 0; i < 120; i += 30)
                {
                    particlesBG.Emit(Sparks, 2, Center + new Vector2(9, 16), Vector2.One * 7f, MathHelper.Pi / 2);
                }
            }
            else
            {
                particlesBG.Depth = Depth + 1;
                for (int i = 0; i < 360; i += 30)
                {
                    particlesBG.Emit(Download, 1, Center + new Vector2(9, 9), Vector2.One * 2f, i * (MathHelper.Pi / 180f));
                }
            }
        
            /*for (int i = 0; i < 10; i++)
            {
                particlesBG.Emit(LockIn, 1,base.Center + new Vector2(9,9), Vector2.One + new Vector2(i,i) * 5f, MathHelper.Pi/2);
            }*/
        }
        public ArtifactSlot(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/access/"));
            mode = data.Int("cutsceneNumber",1);
            sprite.AddLoop("idle", "artifactHolder", 0.1f);
            sprite.AddLoop("dead", "artifactHolderDead", 0.1f);
            start = true;
            Position += new Vector2(0, 1);
        }
        public IEnumerator toSlot()
        {

            player = Scene.Tracker.GetEntity<Player>();
            player.Facing = Facings.Right;

            Vector2 from = player.Center + new Vector2(0, 8);
            Vector2 target = Position + new Vector2(8, artifactSprite.Height / 2 - 1);

            Vector2 scaleFrom = artifactSprite.Scale;
            Vector2 scaleTarget = tempScale;
            Vector2 scaleTarget2 = tempScale;

            float rotateFrom = artifactSprite.Rotation;
            float rotateTarget = MathHelper.TwoPi * 2;

            artifact.Position = from;
            artifactSprite.Play("idle");

            var xcurve = new BezierCurve(1, -1.64f, 0.42f, 2.97f);
            var ycurve = new BezierCurve(0.5f, 0.48f, 0.42f, 2.3f);
            var rotatecurve = new BezierCurve(.56f, -1.47f, .38f, 3.35f);
            var endcurve = new BezierCurve(0.83f, 0.11f, 1, 0.63f);

            bool yHalf = false;
            bool yDone = false;

            Tween[] tweens = new Tween[]
            {
              Tween.Create(Tween.TweenMode.Oneshot, xcurve.Anim, 2, false), //x [0]
              Tween.Create(Tween.TweenMode.Oneshot, ycurve.Anim, 2, false), //y [1]
              Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, 2, false),//rotate [2]
              Tween.Create(Tween.TweenMode.Oneshot, rotatecurve.Anim, 2.5f, false),//rotate back [3]
              Tween.Create(Tween.TweenMode.Oneshot, Ease.SineIn, 2, false), //scale [4]
              Tween.Create(Tween.TweenMode.Oneshot, Ease.ExpoOut, 1.5f, false), //ejectY [5]
              Tween.Create(Tween.TweenMode.Oneshot, endcurve.Anim, 2f, false) //ending [6]
            };

            tweens[0].OnUpdate = (Tween t) =>
            {
                artifact.Position.X = MathHelper.Lerp(from.X, target.X, t.Eased);
            };
            tweens[1].OnUpdate = (Tween t) =>
            {
                artifact.Position.Y = MathHelper.Lerp(from.Y, target.Y, t.Eased);
                if (artifact.Position.Y <= target.Y)
                {
                    if (!yDone && !yHalf)
                    {
                        yHalf = true;
                    }
                }
                else
                {
                    if (!yDone && yHalf && artifact.Position.Y == target.Y)
                    {
                        yDone = true;
                    }
                }
            };
            tweens[2].OnUpdate = (Tween t) =>
            {
                artifactSprite.Rotation = MathHelper.Lerp(rotateFrom, rotateTarget, t.Eased);
            };
            tweens[3].OnUpdate = (Tween t) =>
            {
                artifactSprite.Rotation = MathHelper.Lerp(rotateTarget, rotateFrom, t.Eased);
            };
            tweens[4].OnUpdate = (Tween t) =>
            {
                artifactSprite.Scale.X = MathHelper.Lerp(scaleFrom.X, scaleTarget2.X, t.Eased);
                artifactSprite.Scale.Y = MathHelper.Lerp(scaleFrom.Y, scaleTarget2.Y, t.Eased);
            };
            tweens[5].OnUpdate = (Tween t) =>
            {
                artifact.Position.Y = MathHelper.Lerp(from.Y, target.Y, t.Eased);
            };
            tweens[6].OnUpdate = (Tween t) =>
            {
                artifact.Position.X = MathHelper.Lerp(from.X, target.X, t.Eased);
                artifact.Position.Y = MathHelper.Lerp(from.Y, target.Y, t.Eased);
                artifactSprite.Rotation = MathHelper.Lerp(rotateFrom, rotateTarget, t.Eased);
                artifactSprite.Scale.X = MathHelper.Lerp(scaleFrom.X, scaleTarget.X, t.Eased);
                artifactSprite.Scale.Y = MathHelper.Lerp(scaleFrom.Y, scaleTarget.Y, t.Eased);
            };

            bool sixComplete = false;

            tweens[6].OnComplete = (t) =>
            {
                sixComplete = true;
                tweens[6].RemoveSelf();
            };

            Add(tweens);
            tweens[1].Start();
            tweens[2].Start();
            tweens[4].Start();

            while (true)
            {
                if (yHalf)
                {
                    tweens[0].Start();
                    tweens[3].Start();
                    break;
                }

                yield return null;

            }

            while (artifact.Position != target)
            { yield return null; }
            
            while (artifactSprite.Rotation != rotateFrom)
            { yield return null; }

            AppearParticles(true);

            yield return 1;

            artifactSprite.Play("coverOn");

            while (artifactSprite.CurrentAnimationID != "coverIdle")
            { yield return null; }

            artifactGlow.Position = artifact.Position;
            glowSprite.Play("flash");

            yield return 0.5f;

            artifactSprite.Play("download");
            armSprite.Play("glow");
            glowSprite.Play("flicker");
            for(int i = 0; i<180; i++)
            {
                if (i % 10 == 0)
                {
                    AppearParticles(false);
                }
                yield return null;
            }
            
            //yield return 3;

            armSprite.Stop();
            arms.RemoveSelf();

            glowSprite.Play("flash");
            artifactSprite.Play("coverIdle");

            yield return 0.3f;

            artifactSprite.Play("coverOff");

            while (artifactSprite.CurrentAnimationID == "coverOff")
            { yield return null; }

            yield return 0.2f;

            from.Y = artifact.Position.Y;
            target.Y = artifact.Position.Y - 9;
            sprite.Play("dead");
            AppearParticles(true);
            tweens[5].Start();

            while (tweens[5].Active)
            { yield return null; }

            from = artifact.Position;
            target = player.Center;
            scaleFrom = artifactSprite.Scale;
            scaleTarget = Vector2.Zero;
            rotateFrom = artifactSprite.Rotation;
            rotateTarget = MathHelper.TwoPi * 2;

            tweens[6].Start();

            while (!sixComplete)
            { yield return null; }

            pop.Position = player.Center - new Vector2(player.Width * 1.5f, player.Height);
            popSprite.Play("pop");
            //glitch sequence
            SceneAs<Level>().Session.SetFlag("digiRumble", true);
            //rumble and shake
            SceneAs<Level>().Session.SetFlag("cutsceneProceed", true);
            if (mode == 2)
            {
                while (SceneAs<Level>().Session.GetFlag("inSlot2") != true)
                {
                    yield return null;
                }
                if (true)
                {
                    popSprite.Visible = true;
                    pop.Depth = glitch.Depth - 1;
                    pop.Position = glitch.Position;
                    for (int i = 0; i < 10; i++)
                    {
                        Glitch.Value = 0.1f;
                        popSprite.Play("cover");
                        glitchSprite.Play("glitch");
                        yield return null;
                    }
                    popSprite.Stop();
                    popSprite.Visible = false;
                    glitchSprite.Stop();
                    glitchSprite.Visible = false;
                    Glitch.Value = 0;
                    yield return 1f;
                    glitchSprite.Visible = true;
                    for (int i = 0; i < 15; i++)
                    {
                        Glitch.Value = 0.1f;
                        glitchSprite.Play("glitch");
                        yield return null;
                    }
                    glitchSprite.Stop();
                    glitchSprite.Visible = false;
                    Glitch.Value = 0;
                    yield return 1.5f;
                    glitchSprite.Visible = true;
                    for (int i = 0; i < 5; i++)
                    {
                        Glitch.Value = 0.1f;
                        glitchSprite.Play("glitch");
                        yield return null;
                    }
                    glitchSprite.Stop();
                    glitchSprite.Visible = false;
                    Glitch.Value = 0;
                    yield return 0.5f;
                    glitchSprite.Visible = true;
                    for (int i = 0; i < 30; i++)
                    {
                        Glitch.Value = 0.1f;
                        glitchSprite.Play("glitch");
                        yield return null;
                    }
                    glitchSprite.Stop();
                    glitchSprite.Visible = false;
                    Glitch.Value = 0;
                    yield return 0.5f;
                } //purely just so I can collapse this section lul
                glitchSprite.Play("glitch");
                glitchSprite.Visible = true;
                for (int i = 0; i < 200; i++)
                {
                    if (i < 50)
                    {
                        Glitch.Value = 0.05f;
                        glitchSprite.FlipX = true;
                        glitchSprite.FlipY = true;
                    }
                    else if (i < 100)
                    {
                        Glitch.Value = 0.1f;
                        glitchSprite.FlipX = false;
                        glitchSprite.FlipY = false;
                    }
                    else
                    {
                        Glitch.Value = 0.15f;
                        glitchSprite.FlipX = true;
                        glitchSprite.FlipY = true;
                    }
                    yield return null;
                }
                for (int i = 0; i < 180; i++)
                {
                    Glitch.Value = Calc.Random.Range(0.3f, 1f);
                    yield return null;
                }
                ScreenWipe.WipeColor = Color.White;
                FadeWipe fadeWipe = new FadeWipe(Scene as Level, wipeIn: false);
                fadeWipe.Duration = 5f;
                yield return 5f;
            }
            SceneAs<Level>().Session.SetFlag("fadingToWhite", true);
            yield return null;
        }
        private void Interact(Player player)
        {
            artifact.Position = player.Position;
            Coroutine coroutine = new(toSlot(), true)
            {
                RemoveOnComplete = true
            };
            Add(coroutine);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            sprite.Play("idle");
            SceneAs<Level>().Session.SetFlag("slotSequence", false);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            player = Scene.Tracker.GetEntity<Player>();
            Glitch.Value = 0;
            scene.Add(artifact = new Entity(Position));
            artifact.Add(artifactSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/access/"));

            artifactSprite.AddLoop("idle", "artifact", 0.1f, 0);
            artifactSprite.AddLoop("coverIdle", "cover", 0.1f, 5);
            artifactSprite.Add("coverOn", "cover", 0.1f, "coverIdle");
            artifactSprite.Add("coverOff", "coverRemove", 0.1f, "idle");
            artifactSprite.AddLoop("download", "download", 0.05f);

            artifact.Depth = -10500;
            tempScale = artifactSprite.Scale;
            artifactSprite.Scale = Vector2.Zero;
            artifactSprite.Origin += new Vector2(artifactSprite.Width / 2, artifactSprite.Height / 2 - 1);


            scene.Add(artifactGlow = new Entity(Position));
            artifactGlow.Add(glowSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/access/"));
            glowSprite.Add("flash", "glow", 0.1f);
            glowSprite.AddLoop("flicker", "flicker", 0.1f);
            glowSprite.Origin = artifactSprite.Origin;
            glowSprite.Rate *= 2;
            artifactGlow.Depth = -10501;

            scene.Add(arms = new Entity(Position));
            arms.Add(armSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/access/"));
            armSprite.AddLoop("glow", "armGlow", 0.1f);
            arms.Depth = -10501;
            Level level = Scene as Level;

            scene.Add(glitch = new Entity(new Vector2(level.Bounds.X,level.Bounds.Y)));
            glitch.Add(glitchSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/access/"));
            glitchSprite.AddLoop("glitch", "screenGlitch", 0.05f);
            glitch.Depth = -11000;
            artifactSprite.Play("idle");

            scene.Add(pop = new Entity(new Vector2(artifact.Position.X - 4, artifact.Position.Y - 4)));
            pop.Add(popSprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/access/"));
            popSprite.AddLoop("cover", "screenGlitchCover", 0.1f);
            popSprite.Add("pop", "pop", 0.07f);
            pop.Depth = -10500;
        }
        public override void Update()
        {
            base.Update();
            player = Scene.Tracker.GetEntity<Player>();
            artifact.Collider = new Hitbox(artifactSprite.Width, artifactSprite.Height, artifactSprite.X - artifactSprite.Width / 2, artifactSprite.Y - artifactSprite.Height / 2);
            if (SceneAs<Level>().Session.GetFlag("slotSequence") && start)
            {
                start = false;
                Interact(player);
            }
        }
    }
}