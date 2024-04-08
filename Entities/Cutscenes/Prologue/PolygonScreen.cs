using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue
{
    [Tracked]

    public class PolygonScreen : Entity
    {
        public static FieldInfo lookupFieldInfo = typeof(Autotiler).GetField("lookup", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo terrainTypeInfo = typeof(Autotiler).GetField("TerrainType", BindingFlags.Instance | BindingFlags.NonPublic);
        public static List<char> validBgTiles = new()
        {
            '8','9','t','f','o','p','q','r','E','a','D','b','c','d','e','L','T','P','R','Y'
        };
        public float ConnectAmount;
        public int ConnectingArea;
        public int FadingArea;
        public int FadingAreaLine;
        public bool Broken;
        public List<char> BgCache = new();
        
        public const int MaxPolygons = 25;
        public Vector2[] Points;
        public List<Triangle> Areas = new();

        public int IndexedAreas;
        public bool HasStarted;
        public bool WaitingForAreas;
        public class AreaData
        {
            public List<Vector2> Vertices = new();
            public List<int> Indices = new();
            public char BgTo;
            public int Count;
            public AreaData(char bgTo)
            {
                BgTo = bgTo;
            }
            public void AddNodes(Vector2[] nodes)
            {
                foreach (Vector2 node in nodes)
                {
                    Vertices.Add(node);
                    Indices.Add(Count);
                    Count++;
                }
            }
        }
        public Dictionary<char, AreaData> Data = new();
        public Dictionary<Vector2, Vector2> Offsets = new();
        public class Triangle : ShiftArea
        {
            public Triangle(Vector2 position, Vector2 offset, char bgFrom, char bgTo, char fgFrom, char fgTo, Vector2[] nodes, int[] indices) : base(position, offset, bgFrom, bgTo, fgFrom, fgTo, nodes, indices)
            {
                AreaDepth = -1;
                LineAlpha = 1;
                Add(new Coroutine(lineAlphaFade()));
            }
            private IEnumerator lineAlphaFade()
            {
                
                float start = Calc.Random.Range(0, 1f);
                while (true)
                {
                    
                    for (float i = start; i < 1; i += Engine.DeltaTime)
                    {
                        LineAlpha = Calc.LerpClamp(0.3f, 0.5f, Ease.SineInOut(i));
                        yield return null;
                    }
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        LineAlpha = Calc.LerpClamp(0.5f, 0.3f, Ease.SineInOut(i));
                        yield return null;
                    }
                    start = 0;
                }
            }
        }

        public static void cacheValidTiles()
        {
            if (validBgTiles == null)
            {
                IDictionary dictionary = lookupFieldInfo.GetValue(GFX.BGAutotiler) as IDictionary;
                validBgTiles = dictionary.Keys.Cast<char>().ToList();
            }
        }
        public PolygonScreen() : base(Vector2.Zero)
        {
            AddTag(Tags.TransitionUpdate);
            int size = 30;
            GetScreenPoints(size, size, 1);
        }

        private IEnumerator LineConnect()
        {
            float delay = 0.1f;
            float time = 0.5f;
            for (int i = 0; i < Areas.Count; i++)
            {
                Areas[i].ConnectLines = true;
                Areas[i].FadeConnectBegin(time, delay);
                yield return 0.1f;
            }
            for (int i = 0; i < Areas.Count; i++)
            {
                while (Areas[i].Fading)
                {
                    yield return null;
                }
                Areas[i].ConnectLines = false;
            }
            yield return null;
        }
        public static char RandTile()
        {
            cacheValidTiles();
            return Calc.Random.Choose(validBgTiles);
        }

        public List<char> validTiles()
        {
            List<char> value = new();
            cacheValidTiles();
            foreach (char c in validBgTiles)
            {
                value.Add(c);
            }
            return value;
        }
        public void Start()
        {
            Add(new Coroutine(LineConnect()));
            foreach (PolyScreenShiftArea area in SceneAs<Level>().Tracker.GetEntities<PolyScreenShiftArea>())
            {
                area.Start();
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = scene as Level;
            char bgfrom = '0';
            for (int i = 2; i < Points.Length; i++)
            {
                char bgto = RandTile();
                if (!Data.ContainsKey(bgto))
                {
                    Data.Add(bgto, new AreaData(bgto));
                }

                Vector2[] nodes = new Vector2[3];

                nodes[0] = Points[i - 2];
                nodes[1] = Points[i - 1];
                nodes[2] = Points[i];

                if (nodes.Contains(-Vector2.One))
                {
                    continue;
                }
                if (Broken)
                {
                    for (int n = 0; n < 3; n++)
                    {
                        if (!Offsets.ContainsKey(nodes[n] + level.LevelOffset))
                        {
                            CreateOffset(nodes[n] + level.LevelOffset);
                        }
                        nodes[n] += Offsets[nodes[n] + level.LevelOffset];
                    }
                }
                Data[bgto].AddNodes(nodes);
            }

            foreach (KeyValuePair<char, AreaData> pair in Data)
            {
                CreateAndAddArea(pair.Value.BgTo, level, pair.Value.Vertices, pair.Value.Indices);
            }

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }
        public void FadeInAt(int index)
        {
            if (Areas.Count > index)
            {
                Areas[index].IntoPolyscreen();
            }
        }
        public Color GetRandomColor()
        {
            return Calc.Random.Choose(Color.White, Color.LightBlue);
        }
        public void CreateAndAddArea(char bgto, Level level, List<Vector2> nodes, List<int> indices)
        {
            Triangle area = new(Vector2.Zero, level.LevelOffset, '0', bgto, '0', '0', nodes.ToArray(), indices.ToArray())
            {
                FollowCamera = true,
                DisableBreathing = true,
                PolygonScreenIndex = BgCache.Count,
                VerticeColor = PianoUtils.Initialize(GetRandomColor, Points.Length),
                PointLineAmount = PianoUtils.Initialize(0f, Points.Length),
                PointLineAlphas = PianoUtils.Initialize(0f, Points.Length),
                VerticeAlpha = PianoUtils.Initialize(0f, Points.Length)
            };
            Areas.Add(area);
            level.Add(area);
            BgCache.Add(bgto);
        }
        public void CreateOffset(Vector2 position)
        {
            Vector2 value = Vector2.Zero;
            float range = Calc.Random.Range(20, 50);
            value.X = Calc.Random.Range(-range, range);
            value.Y = Calc.Random.Range(-range, range);
            Offsets[position] = value;
        }
        public void GetScreenPoints(float height, float width, int extend)
        {
            List<Vector2> screenpoints = new List<Vector2>();
            float top = extend * -height;
            float left = extend * -width;
            float right = 320 + extend * width;
            float bottom = 180 + extend * height;
            bool offsetHeight = false;
            bool offsetRow = false;
            float adjustY = height / 2;
            float adjustX = 0;
            for (float j = top; j < bottom; j += height)
            {
                for (float i = left; i < right; i += width / 2)
                {
                    screenpoints.Add(new Vector2(i + adjustX, (offsetHeight ? -1 : 1) * adjustY + j));
                    offsetHeight = !offsetHeight;
                }
                offsetHeight = false;
                adjustX = offsetRow ? width / 2 : 0;
                offsetRow = !offsetRow;
                screenpoints.Add(-Vector2.One);
            }
            Points = screenpoints.ToArray();
        }
    }
}
