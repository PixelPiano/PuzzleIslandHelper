using Celeste.Mod.CommunalHelper;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue
{
    [Tracked]
    public class TransferScene : CutsceneEntity
    {
        public static List<char> validBgTiles = new()
        {
            '8','9','t','f','o','p','q','r','E','a','D','b','c','d','e','L','T','P','R','Y'
        };
        public class ActorRenderer : Entity
        {
            public VirtualRenderTarget Helper;
            public ActorRenderer() : base()
            {
                Depth = -100003;
                Helper = VirtualContent.CreateRenderTarget("FaderHelper", 320, 180);
                Add(new BeforeRenderHook(BeforeRender));
            }
            public override void Render()
            {
                base.Render();
                Level level = Scene as Level;
                Camera camera = level.Camera;
                UnbornHusk husk = Scene.Tracker.GetEntity<UnbornHusk>();
                if (husk != null)
                {
                    level.SnapColorGrade("none");
                    if (husk.DoPulseGlitch)
                    {
                        Draw.SpriteBatch.Draw(Helper, camera.Position, Color.White);
                    }
                    if (!husk.RenderStuff)
                    {
                        husk.RenderAllPulses();
                    }
                    //level.SnapColorGrade("PuzzleIslandHelper/prologue");
                }

                Player entity = Scene.Tracker.GetEntity<Player>();
                entity?.Render();
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Helper?.Dispose();
                Helper = null;
            }
            public void BeforeRender()
            {
                UnbornHusk husk = Scene.Tracker.GetEntity<UnbornHusk>();
                if (husk is null || !husk.DoPulseGlitch) return;
                Helper.DrawToObject(husk.RenderAllPulsesForTarget, Matrix.Identity, true);
                Helper = EasyRendering.AddGlitch(Helper);
            }
        }
        public class DepthFade : Entity
        {
            public static Vector2[] Points = new Vector2[] { new(0, 0), new(1, 0), new(0, 1), new(1, 1) };
            public VertexPositionColor[] Vertices = new VertexPositionColor[4];
            public readonly int[] Indices = new int[] { 0, 1, 2, 2, 1, 3 };
            public float height = 180;
            public float Alpha = 0;
            public DepthFade() : base()
            {
                Depth = -100001;
                for (int i = 0; i < Points.Length; i++)
                {
                    Vertices[i] = new VertexPositionColor(Vector3.Zero, Color.Black);
                }
                Collider = new Hitbox(320, height);
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                UpdateVertices(scene as Level);
            }
            public override void Update()
            {
                base.Update();
                if (Scene is not Level level) return;
                UpdateVertices(level);
            }
            public void UpdateVertices(Level level)
            {
                for (int i = 0; i < Points.Length; i++)
                {
                    Vertices[i].Position = new Vector3(level.Camera.Position + Points[i] * Collider.Size, 0);
                    Vertices[i].Color = (Points[i].Y == 0 ? Color.Black : Color.Transparent) * Alpha;
                }
            }
            public override void Render()
            {
                base.Render();
                if (Scene is not Level level) return;
                Draw.SpriteBatch.End();
                GFX.DrawIndexedVertices(level.Camera.Matrix, Vertices, 4, Indices, 2);
                GameplayRenderer.Begin();
            }
        }
        public DepthFade ScreenFade;
        public class TempSolid : Solid
        {
            public Player Player;
            public TempSolid(Player player) : base(player.BottomLeft, 8, 8, true)
            {
                Player = player;
            }
        }
        public TempSolid Platform;
        private IEnumerator ScreenToFloor()
        {
            Vector2 screenCenter = new Vector2(160, 90);
            Level level = Scene as Level;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
            {
                float eased = Ease.SineOut(i);
                foreach (ShiftAreaRenderer r in level.Tracker.GetEntities<ShiftAreaRenderer>())
                {
                    r.RotationOrigin = new Vector3(160, 135, 0);
                    r.Offset.Z = 16 * eased;
                    r.XRotation = 70f.ToRad() * eased;
                    r.Scale = Vector2.One * (1 + eased / 2);
                    r.ScaleOrigin = screenCenter;
                }
                ScreenFade.Alpha = Calc.LerpClamp(0, 1f, Calc.Max(eased - 0.3f, 0) / 0.7f);
                yield return null;
            }
        }
        public class Fader : Entity
        {
            public float Target;
            public bool Ended;
            public float fade;

            public Fader()
            {
                Depth = -100000;
            }

            public override void Update()
            {
                fade = Calc.Approach(fade, Target, Engine.DeltaTime * 0.5f);
                if (Target <= 0f && fade <= 0f && Ended)
                {
                    RemoveSelf();
                }
                base.Update();
            }
            public override void Render()
            {
                Level level = Scene as Level;
                Camera camera = level.Camera;
                if (fade > 0f)
                {
                    Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, Color.Black * fade);
                }
            }
        }
        public class CutsceneShiftAreas : Entity
        {
            [TrackedAs(typeof(ShiftArea))]
            public class Area : ShiftArea
            {
                private bool[] fades;
                private bool fadeFromLeft;
                private int[] fadeOrder = new int[3];

                private float WaitTime;
                public Area(Vector2 position, Vector2 offset, char bgTo, Vector2[] nodes) : base(position, offset, '0', bgTo, '0', '0', nodes, new int[] { 0, 1, 2 })
                {
                    WaitTime = Calc.Random.Range(0.6f, 1.3f);
                    Alpha = 1;

                    fadeFromLeft = Calc.Random.Chance(0.5f);
                    float left = Vertices[0].Position.X, right = Vertices[0].Position.X;
                    int leftmost = 0, rightmost = 0;

                    for (int i = 0; i < 3; i++)
                    {
                        float p = Vertices[i].Position.X;
                        if (p < left)
                        {
                            left = p;
                            leftmost = i;
                        }
                        if (p > right)
                        {
                            right = p;
                            rightmost = i;
                        }
                        VerticeColor[i] = Color.Transparent;
                    }
                    fadeOrder[0] = fadeFromLeft ? leftmost : rightmost;
                    fadeOrder[2] = fadeFromLeft ? rightmost : leftmost;
                    fadeOrder[1] = (int)Calc.Min(2, 3 - leftmost - rightmost);
                    Add(new Coroutine(Routine()));
                }
                private IEnumerator CornerFade()
                {
                    float time = WaitTime / 3;
                    for (int j = 0; j < 2; j++)
                    {
                        Color from = j > 0 ? Color.White : Color.Transparent;
                        Color to = j > 0 ? Color.Transparent : Color.White;
                        for (int k = 0; k < 3; k++)
                        {
                            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
                            {
                                VerticeColor[fadeOrder[k]] = Color.Lerp(from, to, i);
                                yield return null;
                            }
                        }
                    }
                }
                private IEnumerator Routine()
                {
                    yield return CornerFade();
                    AddArea(SceneAs<Level>());
                    RemoveSelf();
                }
                public override void Awake(Scene scene)
                {
                    base.Awake(scene);
                    ShiftAreaRenderer.ChangeDepth(false, -1000001);
                }
            }
            public int MaxAreas;
            public CutsceneShiftAreas(int maxAreas) : base()
            {
                MaxAreas = maxAreas;
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                for (int i = 0; i < MaxAreas; i++)
                {
                    AddArea(scene as Level);
                }
            }
            public static void AddArea(Level level)
            {
                Area area = CreateArea(level);
                level.Add(area);
            }
            public static Area CreateArea(Level level)
            {
                Vector2 randomPos;
                Vector2[] points;
                int loops = 0;
                Area area = null;
                while (loops < 20)
                {
                    randomPos = randomPosition();
                    points = GetRandomPoints(randomPos);
                    area = new Area(randomPos, level.LevelOffset, RandTile(), points);
                    bool collided = false;
                    foreach (NoShiftAreaZone zone in level.Tracker.GetEntities<NoShiftAreaZone>())
                    {
                        if (area.Box.Collide(zone.Collider))
                        {
                            collided = true;
                            break;
                        }
                    }
                    if (collided) continue;
                    foreach (ShiftArea area2 in level.Tracker.GetEntities<ShiftArea>())
                    {
                        if (area2 is null) continue;
                        if (area.Intersects(area2))
                        {
                            collided = true;
                            break;
                        }
                    }
                    if (!collided) break;
                    loops++;
                }

                return area;
            }
            private static Vector2 randomPosition()
            {
                return new Vector2(Calc.Random.Range(-23, 274), Calc.Random.Range(-23, 134));
            }
            public static Vector2[] GetRandomPoints(Vector2 offset)
            {

                float width = Calc.Random.Range(16, 96);
                float height = Calc.Random.Range(16, 96);
                int[] ind = { 0, 0, 0 };
                Vector2[] points = new Vector2[3];
                for (int i = 0; i < 3; i++)
                {
                    int rand = Calc.Random.Range(1, 5);
                    if (ind.Contains(rand))
                    {
                        i--;
                        continue;
                    }
                    ind[i] = rand;
                    points[i] = rand switch
                    {
                        1 => new Vector2(Calc.Random.Range(8, width), 8) + offset,
                        2 => new Vector2(width, Calc.Random.Range(8, height)) + offset,
                        3 => new Vector2(Calc.Random.Range(8, width), height) + offset,
                        _ => new Vector2(8, Calc.Random.Range(8, height)) + offset,
                    };
                    if (i > 0)
                    {
                        points[i] += Vector2.One * 16 * i;
                    }
                }
                return points;

            }
            public static char RandTile()
            {
                char result = '0';
                while (result == '0')
                {
                    result = validBgTiles[Calc.Random.Range(0, validBgTiles.Count)];
                }
                return result;
            }
        }
        public Fader fader;
        public ActorRenderer renderer;
        public Player player;
        private SineWave wave;
        public PolygonScreen Screen;
        public TransferScene() : base(true, true)
        {
            Add(wave = new SineWave(1));
        }
        public override void OnBegin(Level level)
        {
            if (level.GetPlayer() is not Player player) return;
            this.player = player;
            Scene.Add(Platform = new TempSolid(player));
            if (level.Tracker.GetEntity<PolygonScreen>() is PolygonScreen screen)
            {
                Screen = screen;
            }
            else
            {
                RemoveSelf();
            }
            level.Add(fader = new Fader());
            level.Add(renderer = new ActorRenderer());
            level.Add(ScreenFade = new DepthFade());
            Add(new Coroutine(Cutscene(level)));
        }
        private IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            player.DummyAutoAnimate = false;
            player.DummyGravity = false;
            player.Dashes = 1;
            player.ForceStrongWindHair.X = 0f;
            fader.Target = 1f;
            yield return 2f;
            level.Camera.Position = player.Center - new Vector2(160, 90);
            player.Sprite.Play("sleep");
            yield return 1f;
            yield return level.ZoomBack(1f);

            if (level.Session.Area.Mode == AreaMode.Normal)
            {
                level.Session.ColorGrade = "PuzzleIslandHelper/prologue";
                for (float p2 = 0f; p2 < 1f; p2 += Engine.DeltaTime)
                {
                    Glitch.Value = p2 * 0.05f;
                    level.ScreenPadding = 32f * p2;
                    yield return null;
                }
            }
            Add(new Coroutine(GlitchRoutine()));

            yield return 1f;
            yield return HuskSequence(level);
            EndCutscene(level);
        }
        private IEnumerator HuskSequence(Level level)
        {
            UnbornHusk husk = new UnbornHusk(player.Position);
            level.Add(husk);
            yield return null;
            while (!husk.WaitingForPolygonScreen)
            {
                yield return null;
            }
            Screen.Start();
            yield return null;
            husk.State = UnbornHusk.States.InControl;
            while (!husk.Finished)
            {
                yield return null;
            }
            yield return ScreenToFloor();
            EndCutscene(Level);
        }
        private IEnumerator HuskToPlayer(UnbornHusk husk, Vector2 offset, float time)
        {
            Vector2 from = husk.Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                husk.MoveToX(Calc.LerpClamp(from.X, player.X + offset.X, Ease.CubeInOut(i)));
                husk.MoveToY(Calc.LerpClamp(from.Y, player.Y + offset.Y, Ease.CubeInOut(i)));
                yield return null;
            }
            husk.MoveToX(player.X + offset.X);
            husk.MoveToY(player.Y + offset.Y);
            yield return null;
        }
        private IEnumerator HuskDive(UnbornHusk husk)
        {
            Vector2 target = player.Position;
            Vector2 from = husk.Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1)
            {
                husk.MoveToY(from.Y - 16f * Ease.CubeInOut(i));
                yield return null;
            }
            yield return 0.8f;
            from = husk.Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1)
            {
                husk.MoveToY(Calc.LerpClamp(from.Y, target.Y, Ease.SineIn(i)));
                yield return null;
            }
            Level.Shake(0.1f);
            Level.Flash(Color.White, true);
            while (Level.flash > 0)
            {
                yield return null;
            }
            InvertOverlay.ForceState(true);
            yield return 1.2f;
            InvertOverlay.ForceState(false);
            player.Sprite.Play("bigFall");
            CenterTriangle centerTriangle = Scene.Tracker.GetEntity<CenterTriangle>();
            Vector2 targetPos = Level.Camera.Position + new Vector2(160, 90);
            while (player.Center.Y < targetPos.Y)
            {
                player.MoveTowardsY(targetPos.Y, 8);
                yield return null;
            }
            InvertOverlay.ForceState(true);
            centerTriangle.Shattering = true; //switch CenterTriangle rendering mode to only render shards
            yield return null;
            Celeste.Freeze(1);
            yield return 1;
            Level.Flash(Color.White, true);
            centerTriangle.Shatter(); //tell the shards to Start moving
            InvertOverlay.ResetState();
            Level.Shake(3);
            player.Visible = false;
        }
        private IEnumerator GlitchRoutine()
        {
            float value = Glitch.Value;
            while (true)
            {
                Glitch.Value = value + (0.02f * (wave.Value / 3));
                yield return null;
            }
        }
        public override void OnEnd(Level level)
        {
            InvertOverlay.ResetState();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            InvertOverlay.ResetState();
        }
    }
}
