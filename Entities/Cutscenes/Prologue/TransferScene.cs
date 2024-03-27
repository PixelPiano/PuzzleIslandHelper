using Celeste.Mod.CommunalHelper;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Celeste.Mod.PuzzleIslandHelper.Triggers;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue
{
    [Tracked]
    public class TransferScene : CutsceneEntity
    {
        public static FieldInfo lookupFieldInfo = typeof(Autotiler).GetField("lookup", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo terrainTypeInfo = typeof(Autotiler).GetField("TerrainType", BindingFlags.Instance | BindingFlags.NonPublic);
        public static List<char> validBgTiles;

        public class Fader : Entity
        {
            public float Target;
            public bool Ended;
            public float fade;
            public Fader()
            {
                Depth = -1000000;
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
                Camera camera = (base.Scene as Level).Camera;
                if (fade > 0f)
                {
                    Draw.Rect(camera.X - 10f, camera.Y - 10f, 340f, 200f, Color.Black * fade);
                }

                Player entity = base.Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    entity.Render();
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
                    if(collided) continue;
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
                cacheValidTiles();
                char result = '0';
                while (result == '0')
                {
                    result = validBgTiles[Calc.Random.Range(0, validBgTiles.Count)];
                }
                return result;
            }
            public static void cacheValidTiles()
            {
                if (validBgTiles == null)
                {
                    IDictionary dictionary = lookupFieldInfo.GetValue(GFX.BGAutotiler) as IDictionary;
                    validBgTiles = dictionary.Keys.Cast<char>().ToList();
                }
            }
        }
        public Fader fader;
        public Player player;
        private SineWave wave;
        private CutsceneShiftAreas Helper;
        public TransferScene() : base(true, true)
        {
            Add(wave = new SineWave(1));
        }

        public override void OnBegin(Level level)
        {
            if (level.GetPlayer() is Player player)
            {
                this.player = player;
            }
            else
            {
                return;
            }
            level.Add(fader = new Fader());
            Add(new Coroutine(Cutscene(level)));
        }
        private IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            player.DummyAutoAnimate = false;
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
            Helper = new(10);
            level.Add(Helper);
            float value = Glitch.Value;
            while (true)
            {
                Glitch.Value = value + (0.02f * wave.Value);
                yield return null;
            }

            EndCutscene(level);
        }
        public override void OnEnd(Level level)
        {
            throw new NotImplementedException();
        }
    }
}
