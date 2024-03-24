using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

// PuzzleIslandHelper.DecalEffects
namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.Prologue
{
    [CustomEntity("PuzzleIslandHelper/PrologueBird")]
    [Tracked]
    public class PrologueBird : Actor
    {
        public static ParticleType P_Feather;


        public static string FlownFlag = "bird_fly_away_";

        public Facings Facing = Facings.Left;

        public Sprite Sprite;

        public Vector2 StartPosition;

        public VertexLight Light;

        public bool AutoFly;

        public EntityID EntityID;

        public bool FlyAwayUp = true;

        public float WaitForLightningPostDelay;

        public bool DisableFlapSfx;
        public Coroutine tutorialRoutine;

        public BirdTutorialGui gui;

        public Level level;

        public Vector2[] nodes;

        public StaticMover staticMover;

        public PrologueBird(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            EntityID = new EntityID(data.Level.Name, data.ID);
            nodes = data.NodesOffset(offset);
            Add(Sprite = GFX.SpriteBank.Create("bird"));
            Sprite.Scale.X = (float)Facing;
            Sprite.UseRawDeltaTime = true;
            Sprite.OnFrameChange = delegate (string spr)
            {
                if (level != null && X > level.Camera.Left + 64f && X < level.Camera.Right - 64f && (spr.Equals("peck") || spr.Equals("peckRare")) && Sprite.CurrentAnimationFrame == 6)
                {
                    Audio.Play("event:/game/general/bird_peck", Position);
                }

                if (level != null && level.Session.Area.ID == 10 && !DisableFlapSfx)
                {
                    FlapSfxCheck(Sprite);
                }
            };
            Add(Light = new VertexLight(new Vector2(0f, -8f), Color.White, 1f, 8, 32));
            StartPosition = Position;
            SetMode();
        }

        public void SetMode()
        {
            if (tutorialRoutine != null)
            {
                tutorialRoutine.RemoveSelf();
            }

            Add(tutorialRoutine = new Coroutine(DashingTutorial()));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }

        public override bool IsRiding(Solid solid)
        {
            return Scene.CollideCheck(new Rectangle((int)X - 4, (int)Y, 8, 2), solid);
        }

        public override void Update()
        {
            Sprite.Scale.X = (float)Facing;
            base.Update();
        }

        public IEnumerator Caw()
        {
            Sprite.Play("croak");
            while (Sprite.CurrentAnimationFrame < 9)
            {
                yield return null;
            }

            Audio.Play("event:/game/general/bird_squawk", Position);
        }

        public IEnumerator ShowTutorial(BirdTutorialGui gui, bool caw = false)
        {
            if (caw)
            {
                yield return Caw();
            }

            this.gui = gui;
            gui.Open = true;
            Scene.Add(gui);
            while (gui.Scale < 1f)
            {
                yield return null;
            }
        }

        public IEnumerator HideTutorial()
        {
            if (gui != null)
            {
                gui.Open = false;
                while (gui.Scale > 0f)
                {
                    yield return null;
                }

                Scene.Remove(gui);
                gui = null;
            }
        }

        public IEnumerator StartleAndFlyAway()
        {
            Depth = -1000000;
            level.Session.SetFlag(FlownFlag + level.Session.Level);
            yield return Startle("event:/game/general/bird_startle");
            yield return FlyAway();
        }
        public bool Enabled;

        public IEnumerator FlyAway(float upwardsMultiplier = 1f)
        {
            if (staticMover != null)
            {
                staticMover.RemoveSelf();
                staticMover = null;
            }

            Sprite.Play("fly");
            Facing = (Facings)(0 - Facing);
            Vector2 speed = new Vector2((int)Facing * 20, -40f * upwardsMultiplier);
            while (Y > level.Bounds.Top)
            {
                speed += new Vector2((int)Facing * 140, -120f * upwardsMultiplier) * Engine.DeltaTime;
                Position += speed * Engine.DeltaTime;
                yield return null;
            }

            RemoveSelf();
        }
        public IEnumerator DashingTutorial()
        {
            Y = level.Bounds.Top;
            X += 32f;
            yield return 1f;
            Player player = Scene.Tracker.GetEntity<Player>();
            PrologueBridge bridge = Scene.Entities.FindFirst<PrologueBridge>();
            while (!Enabled)
            {
                yield return null;
            }

            Scene.Add(new PIPrologueEnding(player, this, bridge));
        }


        public IEnumerator WaitRoutine()
        {
            while (!AutoFly)
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && Math.Abs(entity.X - X) < 120f)
                {
                    break;
                }

                yield return null;
            }

            yield return Caw();
            while (!AutoFly)
            {
                Player entity2 = Scene.Tracker.GetEntity<Player>();
                if (entity2 != null && (entity2.Center - Position).Length() < 32f)
                {
                    break;
                }

                yield return null;
            }

            yield return StartleAndFlyAway();
        }

        public IEnumerator Startle(string startleSound, float duration = 0.8f, Vector2? multiplier = null)
        {
            if (!multiplier.HasValue)
            {
                multiplier = new Vector2(1f, 1f);
            }

            if (!string.IsNullOrWhiteSpace(startleSound))
            {
                Audio.Play(startleSound, Position);
            }

            Dust.Burst(Position, -(float)Math.PI / 2f, 8, null);
            Sprite.Play("jump");
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, duration, start: true);
            tween.OnUpdate = delegate (Tween t)
            {
                if (t.Eased < 0.5f && Scene.OnInterval(0.05f))
                {
                    level.Particles.Emit(P_Feather, 2, Position + Vector2.UnitY * -6f, Vector2.One * 4f);
                }

                Vector2 vector = Vector2.Lerp(new Vector2(100f, -100f) * multiplier.Value, new Vector2(20f, -20f) * multiplier.Value, t.Eased);
                vector.X *= 0 - Facing;
                Position += vector * Engine.DeltaTime;
            };
            Add(tween);
            while (tween.Active)
            {
                yield return null;
            }
        }

        public IEnumerator FlyTo(Vector2 target, float durationMult = 1f, bool relocateSfx = true)
        {
            Sprite.Play("fly");
            if (relocateSfx)
            {
                Add(new SoundSource().Play("event:/new_content/game/10_farewell/bird_relocate"));
            }

            int num = Math.Sign(target.X - X);
            if (num != 0)
            {
                Facing = (Facings)num;
            }

            Vector2 position = Position;
            Vector2 vector = target;
            SimpleCurve curve = new SimpleCurve(position, vector, position + (vector - position) * 0.75f - Vector2.UnitY * 30f);
            float duration = (vector - position).Length() / 100f * durationMult;
            for (float p = 0f; p < 0.95f; p += Engine.DeltaTime / duration)
            {
                Position = curve.GetPoint(Ease.SineInOut(p)).Floor();
                Sprite.Rate = 1f - p * 0.5f;
                yield return null;
            }

            Dust.Burst(Position, -(float)Math.PI / 2f, 8, null);
            Position = target;
            Facing = Facings.Left;
            Sprite.Rate = 1f;
            Sprite.Play("idle");
        }

        public IEnumerator MoveToNodesRoutine()
        {
            int index = 0;
            while (true)
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity == null || !((entity.Center - Position).Length() < 80f))
                {
                    yield return null;
                    continue;
                }

                Depth = -1000000;
                yield return Startle("event:/new_content/game/10_farewell/bird_startle", 0.2f);
                if (index < nodes.Length)
                {
                    yield return FlyTo(nodes[index], 0.6f);
                    index++;
                    continue;
                }

                Tag = Tags.Persistent;
                Add(new SoundSource().Play("event:/new_content/game/10_farewell/bird_relocate"));

                Sprite.Play("fly");
                Facing = Facings.Right;
                Vector2 speed = new Vector2((int)Facing * 20, -40f);
                while (Y > level.Bounds.Top - 200)
                {
                    speed += new Vector2((int)Facing * 140, -60f) * Engine.DeltaTime;
                    Position += speed * Engine.DeltaTime;
                    yield return null;
                }

                RemoveSelf();
            }
        }

        public override void SceneEnd(Scene scene)
        {
            Engine.TimeRate = 1f;
            base.SceneEnd(scene);
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            float x = StartPosition.X - 92f;
            float x2 = level.Bounds.Right;
            float y = StartPosition.Y - 20f;
            float y2 = StartPosition.Y - 10f;
            Draw.Line(new Vector2(x, y), new Vector2(x, y2), Color.Aqua);
            Draw.Line(new Vector2(x, y), new Vector2(x2, y), Color.Aqua);
            Draw.Line(new Vector2(x2, y), new Vector2(x2, y2), Color.Aqua);
            Draw.Line(new Vector2(x, y2), new Vector2(x2, y2), Color.Aqua);
            float x3 = StartPosition.X - 60f;
            float x4 = level.Bounds.Right;
            float y3 = StartPosition.Y;
            float y4 = StartPosition.Y + 34f;
            Draw.Line(new Vector2(x3, y3), new Vector2(x3, y4), Color.Aqua);
            Draw.Line(new Vector2(x3, y3), new Vector2(x4, y3), Color.Aqua);
            Draw.Line(new Vector2(x4, y3), new Vector2(x4, y4), Color.Aqua);
            Draw.Line(new Vector2(x3, y4), new Vector2(x4, y4), Color.Aqua);
        }

        public static void FlapSfxCheck(Sprite sprite)
        {
            if (sprite.Entity != null && sprite.Entity.Scene != null)
            {
                Camera camera = (sprite.Entity.Scene as Level).Camera;
                Vector2 renderPosition = sprite.RenderPosition;
                if (renderPosition.X < camera.X - 32f || renderPosition.Y < camera.Y - 32f || renderPosition.X > camera.X + 320f + 32f || renderPosition.Y > camera.Y + 180f + 32f)
                {
                    return;
                }
            }

            string currentAnimationID = sprite.CurrentAnimationID;
            int currentAnimationFrame = sprite.CurrentAnimationFrame;
            if (currentAnimationID == "hover" && currentAnimationFrame == 0 || currentAnimationID == "hoverStressed" && currentAnimationFrame == 0 || currentAnimationID == "fly" && currentAnimationFrame == 0)
            {
                Audio.Play("event:/new_content/game/10_farewell/bird_wingflap", sprite.RenderPosition);
            }
        }
    }

}
