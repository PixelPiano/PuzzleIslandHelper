using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Components;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/RuneFirfilShape")]
    public class RuneFirfilShape : FirfilShape
    {
        public RuneDisplay Rune;
        public string RuneID;
        private bool centerRuneX;
        private bool centerRuneY;
        private bool centerGlowX;
        private bool centerGlowY;
        private Vector2 runeOffset;
        private Vector2 glowOffset;
        private Vector2 runeSize;
        public RuneFirfilShape(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
        {
            RuneID = data.Attr("runeID");
            runeSize = new Vector2(data.Int("runeWidth"), data.Int("runeHeight"));
            runeOffset = new Vector2(data.Int("runeOffsetX"), data.Int("runeOffsetY"));
            glowOffset = new Vector2(data.Int("glowOffsetX"), data.Int("glowOffsetY"));
            centerRuneX = data.Bool("centerRuneX");
            centerRuneY = data.Bool("centerRuneY");
            centerGlowX = data.Bool("centerGlowX");
            centerGlowY = data.Bool("centerGlowY");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Rune = new RuneDisplay(Position, (int)runeSize.X, (int)runeSize.Y, RuneID, true);
            if (centerRuneX) Rune.CenterX = CenterX;
            if (centerRuneY) Rune.CenterY = CenterY;
            Rune.Position += runeOffset;

            if (centerGlowX) GlowImage.X += Bg.Width / 2 - GlowImage.Width / 2;
            if (centerGlowY) GlowImage.Y += Bg.Height / 2 - GlowImage.Height / 2;
            GlowImage.Position += glowOffset;

            if ((scene as Level).Session.GetFlag(Flag))
            {
                Finish();
            }
        }
        public override void Finish()
        {
            base.Finish();
            Scene.Add(Rune);
        }
    }
    [Tracked(true)]
    public class FirfilShape : Entity
    {
        public List<FirfilDetector> Detectors = new();
        public const int MaxDetectors = 20;
        public Image Bg;
        public Image GlowImage;
        private string bgPath, glowPath;
        public Vector2[] Nodes;
        public float Alpha;
        public bool Finished;
        public EntityID ID;
        public const float PlayerTime = 3;
        private float playerTimer = PlayerTime;
        public string Flag => "FirfilShape:" + ID.ToString();
        public BetterShaker Shaker;
        public FirfilShape(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, id, data.Attr("texture"), data.Attr("glow"), data.NodesOffset(offset))
        {
        }
        public FirfilShape(Vector2 position, EntityID id, string texture, string glow, params Vector2[] nodes) : base(position)
        {
            Depth = 5;
            ID = id;
            bgPath = texture;
            glowPath = glow;
            Nodes = nodes;
            Tag |= Tags.TransitionUpdate;
            Add(Shaker = new BetterShaker(v =>
            {
                Position += v;
                foreach (FirfilDetector d in Detectors)
                {
                    d.AddPosition(v.X, v.Y);
                }
            }));
        }
        public void Destroy(float fadeTime)
        {
            DebrisSpawner.SpawnDebrisBox(Scene, Position, Center, Width, Height, '3', true);
            Bg.Visible = false;
            Detectors.RemoveSelves();
            Tween.Set(this, Tween.TweenMode.Oneshot, fadeTime, Ease.SineOut, t =>
            {
                Alpha = 1 - t.Eased;
            }, t =>
            {
                GlowImage.Visible = false;
            });
        }
        public void SnapDestroy()
        {
            Detectors.RemoveSelves();
            Bg.Visible = false;
            GlowImage.Visible = false;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Bg = new Image(GFX.Game["decals/" + bgPath]);
            GlowImage = new Image(GFX.Game["decals/" + glowPath]);
            Add(Bg, GlowImage);
            GlowImage.Color = Color.Transparent;
            Collider = Bg.Collider();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (Vector2 n in Nodes)
            {
                FirfilDetector d = new FirfilDetector(n, 6, OnDetect, OnRelease, OnDetectUpdate, OnEmptyUpdate)
                {
                    FollowState = FirfilDetector.FirfilFollowStates.Following
                };
                Detectors.Add(d);
                Scene.Add(d);
            }
            DisableAll();
        }
        public static void OnDetect(FirfilDetector d)
        {
            d.Firfil.Lock();
            d.Firfil.OffsetMult = 0;
            d.Firfil.Speed = Vector2.Zero;
            d.Firfil.PauseScaleTimer();
        }
        public static void OnDetectUpdate(FirfilDetector d)
        {
            d.Firfil.Scale = Calc.Approach(d.Firfil.Scale, 3, Engine.DeltaTime);
            d.Percent = Math.Min(d.Percent + Engine.DeltaTime, 1);
            d.Firfil.Center = d.Center;
            d.Firfil.Center = Calc.Approach(d.Firfil.Center, d.Center, 5 * Engine.DeltaTime);
        }
        public static void OnRelease(FirfilDetector d)
        {
            d.Firfil.OffsetMult = 1;
            d.Firfil.ResumeScaleTimer();
            d.Firfil.Unlock();
        }
        public static void OnEmptyUpdate(FirfilDetector d)
        {
            d.Percent = Math.Max(d.Percent - Engine.DeltaTime, 0);
        }
        public void ArrangeDetectorsInfinity()
        {
            Vector2 center = Center;
            float rate = 1f / MaxDetectors;
            for (float i = 0; i < 1; i += rate)
            {
                Vector2 pos = lemniscate(center, Width / 2, Height, 0, i * MathHelper.TwoPi);
                FirfilDetector d = new(pos, 4);
                Detectors.Add(d);
                Scene.Add(d);
            }
        }
        public bool AllActivated()
        {
            foreach (FirfilDetector d in Detectors)
            {
                if (!d.Activated)
                {
                    return false;
                }
            }
            return true;
        }
        public virtual void Finish()
        {
            Finished = true;
            Alpha = 0;
            SnapDestroy();
        }
        private bool inCutscene;
        private class cutscene : CutsceneEntity
        {
            public FirfilShape Parent;
            public cutscene(FirfilShape parent) : base()
            {
                Parent = parent;
            }
            public override void OnBegin(Level level)
            {
                level.GetPlayer()?.DisableMovement();
                Add(new Coroutine(routine()));
            }
            private IEnumerator routine()
            {
                yield return CameraTo(new Vector2(Parent.CenterX - 160, Level.Camera.Y), 1, Ease.CubeOut, 1);
                yield return Level.ZoomTo(Parent.Center - Level.Camera.Position, 1.5f, 1);
                yield return 1;
                foreach (var d in Parent.Detectors)
                {
                    if (d.Firfil != null)
                    {
                        d.Firfil.ScalePulse();
                        yield return null;
                        d.Firfil.CirclePulseIn(Color.Black);
                        yield return 0.05f;
                    }
                }
                Parent.Shaker.StartShaking(1);
                yield return 1;
                Parent.Destroy(3);
                yield return 3;
                yield return Level.ZoomBack(1);
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                level.GetPlayer()?.EnableMovement();
                Level.ResetZoom();
                Parent.Finish();
            }
        }
        public void DisableAll()
        {
            foreach (FirfilDetector d in Detectors)
            {
                d.Enabled = false;
            }
        }
        public void EnableAll()
        {
            foreach (FirfilDetector d in Detectors)
            {
                d.Enabled = true;
            }
        }
        private float confirmTimer;

        public override void Update()
        {
            base.Update();
            if (!inCutscene)
            {
                if (!Finished)
                {
                    if (CollideCheck<Player>())
                    {
                        if (playerTimer > 0)
                        {
                            playerTimer -= Engine.DeltaTime;
                            if (playerTimer <= 0)
                            {
                                EnableAll();
                            }
                            else
                            {
                                confirmTimer = 0;
                                DisableAll();
                            }
                        }
                    }
                    else
                    {
                        playerTimer = PlayerTime;
                        foreach (FirfilDetector d in Detectors)
                        {
                            if (d.Firfil != null)
                            {
                                d.ReleaseFirfil();
                            }
                        }
                        confirmTimer = 0;
                        DisableAll();
                    }
                    float percent = 0;
                    foreach (FirfilDetector d in Detectors)
                    {
                        percent += d.Percent;
                    }
                    percent /= Detectors.Count;
                    Alpha = percent;
                    if (Alpha == 1 && confirmTimer == 0)
                    {
                        confirmTimer = 1.4f;
                    }
                }
                if (Alpha >= 1 && AllActivated())
                {
                    if (confirmTimer > 0)
                    {
                        confirmTimer -= Engine.DeltaTime;
                    }
                    if (confirmTimer <= 0 && !inCutscene)
                    {
                        Scene.Add(new cutscene(this));
                        inCutscene = true;
                        foreach (FirfilDetector d in Detectors)
                        {
                            d.Locked = true;
                        }
                    }
                }
            }
            GlowImage.Color = Color.White * Alpha;
        }

        private static Vector2 lemniscate(Vector2 center, float radius, float rotation, float t)
            => lemniscate(radius, t).Rotate(rotation) + center;
        private static Vector2 lemniscate(Vector2 center, float width, float height, float rotation, float t)
            => lemniscate(width, height, t).Rotate(rotation) + center;
        private static Vector2 lemniscate(float radius, float t)
            => new(x: radius * (float)Math.Cos(t) / (1 + (float)Math.Pow(Math.Sin(t), 2)),
                y: radius * (float)Math.Sin(t) * (float)Math.Cos(t) / (1 + (float)Math.Pow(Math.Sin(t), 2)));
        private static Vector2 lemniscate(float wideness, float tallness, float t)
            => new(x: wideness * (float)Math.Cos(t) / (1 + (float)Math.Pow(Math.Sin(t), 2)),
                    y: tallness * (float)Math.Sin(t) * (float)Math.Cos(t) / (1 + (float)Math.Pow(Math.Sin(t), 2)));
    }
    [Tracked]
    public class FirfilDetector : Entity
    {
        public bool Locked;
        public bool Activated => Percent >= 1;
        public bool Enabled = true;
        public float Percent;
        public Firfil Firfil;
        public bool HasFirfil => Firfil != null;
        public Action<FirfilDetector> OnDetect;
        public Action<FirfilDetector> OnRelease;
        public Action<FirfilDetector> OnDetectUpdate;
        public Action<FirfilDetector> OnEmptyUpdate;
        public enum FirfilFollowStates
        {
            Either,
            Following,
            NotFollowing
        }
        public FirfilFollowStates FollowState = FirfilFollowStates.Either;
        public FirfilDetector(Vector2 position, float size, Action<FirfilDetector> onDetect = null, Action<FirfilDetector> onRelease = null, Action<FirfilDetector> onDetectUpdate = null, Action<FirfilDetector> onEmptyUpdate = null) : base(position)
        {
            OnDetect = onDetect;
            OnRelease = onRelease;
            OnDetectUpdate = onDetectUpdate;
            OnEmptyUpdate = onEmptyUpdate;
            Collider = new Hitbox(size, size, -size / 2, -size / 2);
            Add(new TransitionListener()
            {
                OnOutBegin = () =>
                {
                    if (Firfil != null)
                    {
                        ReleaseFirfil();
                    }
                }
            });
        }
        public void FirfilDetected(Firfil firfil)
        {
            Firfil = firfil;
            OnDetect?.Invoke(this);
        }
        public void ReleaseFirfil()
        {
            OnRelease?.Invoke(this);
            Firfil = null;
        }
        public override void Update()
        {
            base.Update();
            if (Locked) return;
            if (Enabled)
            {
                if (Firfil == null && CollideFirst<Firfil>() is Firfil f)
                {
                    switch (FollowState)
                    {
                        case FirfilFollowStates.Following:
                            if (f.FollowingPlayer)
                            {
                                FirfilDetected(f);
                            }
                            break;
                        case FirfilFollowStates.NotFollowing:
                            if (!f.FollowingPlayer)
                            {
                                FirfilDetected(f);
                            }
                            break;
                        default:
                            FirfilDetected(f);
                            break;
                    }
                }

                if (Firfil != null)
                {
                    OnDetectUpdate?.Invoke(this);
                }
                else
                {
                    OnEmptyUpdate?.Invoke(this);
                }
            }
            else
            {
                OnEmptyUpdate?.Invoke(this);
                if (Firfil != null)
                {
                    ReleaseFirfil();
                }
            }
        }
        public void AddPosition(float x, float y)
        {
            X += x;
            Y += y;
            if (Firfil != null)
            {
                Firfil.X += x;
                Firfil.Y += y;
            }
        }
        public override void Render()
        {
            base.Render();
            Draw.Rect(Center - Vector2.One, 3, 3, Color.Black * 0.4f);
        }
        public override void Removed(Scene scene)
        {
            if (Firfil != null)
            {
                ReleaseFirfil();
            }
            base.Removed(scene);
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Rect(Collider, Color.Green * Percent);
            Draw.HollowRect(Collider, Color.Red);
        }
    }
}