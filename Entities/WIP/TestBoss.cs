using Celeste.Mod.Entities;
using Celeste.Mod.Meta;
using Celeste.Mod.PuzzleIslandHelper.Components;
using FMOD.Studio;
using Iced.Intel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static Celeste.Mod.PuzzleIslandHelper.Entities.InvertAuth;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Ladder;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/TestBoss")]
    [Tracked]
    public class TestBoss : Solid
    {
        public struct Pattern
        {
            private static Vector2 u = -Vector2.UnitY;
            private static Vector2 d = Vector2.UnitY;
            private static Vector2 l = -Vector2.UnitX;
            private static Vector2 r = Vector2.UnitX;
            private static Vector2 z = Vector2.Zero;
            public static Pattern Default = new Pattern() { StartingBeat = 0, TotalBeats = 8, Paths = null };
            public static Pattern Revolve = new Pattern()
            {
                StartingBeat = 8,
                TotalBeats = 8,
                Paths = new()
                {
                    new(){u,z,l,z,d,z,r,z},
                    new(){d,z,r,z,u,z,l,z}
                }
            };
            public static Pattern Eight = new Pattern()
            {
                StartingBeat = 16,
                TotalBeats = 8,
                Paths = new()
                {
                    new(){u,z, r, z, d, z,l},
                    new(){u,z, l, z, d, z,r},
                }
            };
            public static Pattern Revolve2 = new Pattern()
            {
                StartingBeat = 24,
                TotalBeats = 8,
                Paths = new()
                {
                    new(){ d, z, u, z, l, z, r, z },
                    new(){u,z, l, z, d, z,r},
                    new(){d,z, r, z, u, z, l, z }
                }
            };
            public int StartingBeat;
            public int TotalBeats;
            public List<List<Vector2>> Paths;
            public static bool operator ==(Pattern left, Pattern right)
            {
                return left.StartingBeat == right.StartingBeat;
            }
            public static bool operator !=(Pattern left, Pattern right)
            {
                return left.StartingBeat != right.StartingBeat;
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
        public Pattern CurrentPattern
        {
            get
            {
                return pattern;
            }
            set
            {
                Split(value);
                pattern = value;
                patternBeat = 0;
                currentBeat = value.StartingBeat;
            }
        }
        private Entity pathfinder;
        private Pattern pattern;
        private int patternBeat;
        public TestBoss Parent;
        public Sprite Eye;
        public List<Image> Images = [];
        public Coroutine coroutine;
        private Vector2 nextTarget;
        private Vector2 prevTarget;
        public float Alpha = 1;
        public List<Vector2> ChildPath;
        public Vector2 Orig;
        public static string Event = "event:/PianoBoy/TestBossSoundtrack";
        public int currentBeat;
        public float beatTimer;
        public float tempoMult = 1;
        public float tempoAdd = 0.05f;
        public EventInstance sfx;
        public bool Enabled;
        private float? startTimer = null;
        private float health = 40;
        public List<TestBoss> Copies = [];
        private Image image;
        private Vector2 eyeOffset;
        public TestBoss(EntityData data, Vector2 offset) : base(data.Position + offset, (data.Width / 8) * 8, (data.Height / 8) * 8, false)
        {
            Orig = Position;
            prevTarget = Position;
            Add(coroutine = new Coroutine(false));
            OnDashCollide = (p, d) =>
            {
                if (Parent == null)
                {
                    if (!Enabled)
                    {
                        startTimer = 1;
                        Enabled = true;
                        StartShaking(1);
                    }
                    else
                    {
                        health--;
                        if (health < 0)
                        {

                            Audio.Play("event:/game/general/wall_break_stone", Position);

                            for (int i = 0; (float)i < base.Width / 8f; i++)
                            {
                                for (int j = 0; (float)j < base.Height / 8f; j++)
                                {
                                    Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + i * 8, 4 + j * 8), '8', true).BlastFrom(Center));
                                }
                            }

                            Collidable = false;
                            RemoveSelf();
                            return DashCollisionResults.Ignore;
                        }
                    }
                }
                return DashCollisionResults.Rebound;
            };
        }
        public TestBoss(TestBoss parent, List<Vector2> path) : base(parent.Position, parent.Width, parent.Height, false)
        {
            Orig = Position;
            prevTarget = Position;
            Add(coroutine = new Coroutine(false));
            Parent = parent;
            ChildPath = path;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            pathfinder?.RemoveSelf();
            if (Enabled)
            {
                sfx?.stop(STOP_MODE.ALLOWFADEOUT);
                Audio.PauseMusic = false;
            }
            if (Parent != null)
            {
                Parent.Copies.Remove(this);
            }
            else
            {
                DisposeCopies();
            }
        }
        public void DisposeCopies()
        {
            foreach (var c in Copies)
            {
                c.Add(new Coroutine(c.Fade()));
            }
            Copies.Clear();
        }
        public IEnumerator Fade()
        {
            coroutine?.Cancel();
            Collidable = false;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Alpha = Calc.LerpClamp(1, 0, i);
                yield return null;
            }
            RemoveSelf();
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Parent != null)
            {
                TravelPath(ChildPath);
            }
        }
        public void Split(Pattern pattern)
        {
            StartShaking(0.5f);
            DisposeCopies();
            if (!(pattern.Paths == null || pattern.Paths.Count == 0))
            {
                foreach (List<Vector2> path in pattern.Paths)
                {
                    TestBoss copy = new TestBoss(this, path);
                    Copies.Add(copy);
                    Scene.Add(copy);
                }
            }
        }
        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            image.Position += amount;
            Eye.Position += amount;
        }
        public override void Update()
        {
            base.Update();
            Eye.Color = Color.White * Alpha;
            image.Color = Color.White * Alpha;
            if (Enabled)
            {
                if (startTimer > 0)
                {
                    startTimer -= Engine.DeltaTime;
                }
                else if (sfx == null)
                {
                    sfx = Audio.CreateInstance(Event);
                    sfx?.start();
                    Audio.PauseMusic = true;
                    Open();
                    CurrentPattern = Pattern.Default;
                }
                else
                {
                    AdvanceMusic(Engine.DeltaTime * tempoMult);
                }
            }
        }
        private int beatIndex;
        private int beatIndexMax = 4;
        private int beatsPerTick = 4;
        public override void Render()
        {
            Eye.Position += eyeOffset;
            base.Render();
            Eye.Position -= eyeOffset;
        }
        public void AdvanceMusic(float time)
        {
            beatTimer += time;
            if (beatTimer < 1f / 6f)
            {
                return;
            }
            beatTimer -= 1f / 6f;
            beatIndex++;
            beatIndex %= beatIndexMax;
            if (beatIndex % beatsPerTick == 0)
            {
                foreach (TestBoss copy in Copies)
                {
                    copy.Advance = true;
                }
                currentBeat++;
                patternBeat++;
                if (patternBeat >= 8)
                {
                    if (CurrentPattern == Pattern.Default)
                    {
                        CurrentPattern = Calc.Random.Choose(Pattern.Eight, Pattern.Revolve, Pattern.Revolve2);
                        tempoMult += tempoAdd;
                    }
                    else
                    {
                        CurrentPattern = Pattern.Default;
                    }
                }
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            }
            sfx?.setParameterValue("beat", currentBeat);
        }
        public bool Advance;
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Line(prevTarget, nextTarget, Color.Yellow);
            Draw.Circle(prevTarget, 4, Color.Magenta, 8);
            Draw.Circle(nextTarget, 8, Color.Cyan, 8);
        }
        public IEnumerator PathRoutine(List<Vector2> path)
        {
            if (path != null && path.Count > 0)
            {
                while (true)
                {


                    for (int i = 0; i < path.Count; i++)
                    {
                        Vector2 next = path[i];
                        bool skip = next == Vector2.Zero;
                        if (!skip)
                        {
                            Rectangle bounds = SceneAs<Level>().Bounds;
                            bool exit = false;
                            pathfinder.Position = Position;
                            while (!pathfinder.CollideCheck<SolidTiles>(pathfinder.Position + next))
                            {
                                pathfinder.Position += next;
                                if (pathfinder.Top > bounds.Bottom || pathfinder.Bottom < bounds.Top || pathfinder.Left > bounds.Right || pathfinder.Right < bounds.Left)
                                {
                                    exit = true;
                                    break;
                                }
                            }
                            Vector2 target = pathfinder.Position;
                            if (exit)
                            {
                                break;
                            }
                            prevTarget = Position;
                            nextTarget = target;
                            yield return new SwapImmediately(SingleMoveRoutine(target, 0.35f));
                            while (!Advance)
                            {
                                yield return null;
                            }
                            Advance = false;
                        }
                        else
                        {
                            while (!Advance)
                            {
                                yield return null;
                            }
                            Advance = false;
                        }
                    }
                }
            }
            yield return Fade();
        }
        public void TravelPath(List<Vector2> path)
        {
            coroutine.Replace(PathRoutine(path));
        }
        public IEnumerator SingleMoveRoutine(Vector2 to, float time)
        {
            OpenIdle();
            StopShaking();
            Vector2 from = Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                MoveTo(Vector2.Lerp(from, to, i));
                yield return null;
            }
            MoveTo(to);
            Blink();
            StartShaking(0.2f);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            MTexture nineslice = GFX.Game["objects/PuzzleIslandHelper/testBossNineSlice"];
            Add(image = new Image(nineslice));
            Eye = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/");
            Eye.Add("blink", "testBossEye", 0.1f, "openIdle", 0, 1, 2, 1, 0);
            Eye.Add("close", "testBossEye", 0.1f, "closedIdle");
            Eye.Add("open", "testBossEye", 0.1f, "openIdle", 2, 1, 0);
            Eye.AddLoop("openIdle", "testBossEye", 0.1f, 0);
            Eye.AddLoop("closedIdle", "testBossEye", 0.1f, 2);
            Eye.Play("closedIdle");
            Eye.Position = (Collider.HalfSize - Eye.HalfSize()).Floor();
            Add(Eye);
            pathfinder = new Entity(Position);
            pathfinder.Collider = new Hitbox(Width, Height);
            Scene.Add(pathfinder);
        }
        public void Blink()
        {
            Eye.Play("blink");
        }
        public void Close()
        {
            Eye.Play("close");
        }
        public void ClosedIdle()
        {
            Eye.Play("closedIdle");
        }
        public void Open()
        {
            Eye.Play("open");
        }
        public void OpenIdle()
        {
            Eye.Play("openIdle");
        }
    }
}