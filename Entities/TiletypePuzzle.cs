using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TiletypePuzzle")]
    [Tracked(false)]
    public class TiletypePuzzle : Entity
    {
        //Wip class while I think of a better context for the puzzle
        //todo: think of the better context you nerd
        public string FlagOnComplete;
        public string NodeFlagPrefix;
        public Dictionary<EntityID, List<int>> Cache => PianoModule.Session.TiletypePuzzleCache;
        [Tracked]
        [CustomEntity("PuzzleIslandHelper/TileshiftBlock")]
        public class TileshiftBlock : Solid
        {
            public TileGrid Grid;
            public TileInterceptor Interceptor;
            public TileGrid Overlay;
            public TileInterceptor OverlayInterceptor;
            public VertexLight Light;
            public char[] Tiles;
            public char CorrectTile;
            public char CurrentTile;
            public bool IsCorrect => CurrentTile == CorrectTile;
            public int Index;
            private Coroutine coroutine;
            private bool isForPuzzle;
            private FlagList invalidFlag;
            private struct flagTilePair
            {
                public char Tile;
                public FlagList Flag;
            }
            private List<flagTilePair> pairs = [];
            public TileshiftBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
            {
                invalidFlag = data.FlagList("flagWhenInvalid");
                string attr = data.Attr("flagTilePairs");
                string[] array = attr.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (string s in array)
                {
                    string[] pair = s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    flagTilePair flagTilePair = default;
                    if (pair.Length > 0)
                    {
                        if (pair[0].Length == 0)
                        {
                            flagTilePair.Tile = pair[0][0];
                        }
                        else
                        {
                            continue;
                        }
                        if (pair.Length > 1)
                        {
                            flagTilePair.Flag = new FlagList(pair[1]);
                        }
                    }
                    pairs.Add(flagTilePair);
                }
            }
            public TileshiftBlock(Vector2 position, float width, float height, char[] tiles, char correctTile, int startIndex) : base(position, width, height, false)
            {
                isForPuzzle = true;
                Index = startIndex;
                Tiles = tiles;
                CorrectTile = correctTile;
                Add(coroutine = new Coroutine(false));
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                if (!isForPuzzle)
                {
                    foreach (var pair in pairs)
                    {
                        if (pair.Flag)
                        {
                            CurrentTile = pair.Tile;
                        }
                    }
                }
                else
                {
                    CurrentTile = Tiles[Index];
                }
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                GenerateGrid(CurrentTile);
                Add(new LightOcclude());
                Light = new VertexLight(Color.White, 0.9f, (int)Width / 2 - 8, (int)Width / 2 + 8);
                Light.Position = Collider.HalfSize;
                Add(Light);
            }
            private bool random;
            private float randomTimer;
            public override void Update()
            {
                base.Update();
                Light.InSolid = false;
                Light.InSolidAlphaMultiplier = 1;
                if (!isForPuzzle)
                {
                    bool wasRandom = random;
                    bool atLeastOneTrue = false;
                    foreach (var p in pairs)
                    {
                        if (p.Flag)
                        {
                            atLeastOneTrue = true;
                            if (CurrentTile != p.Tile)
                            {
                                AdvanceTo(p.Tile);
                                break;
                            }
                        }
                    }
                    random = !atLeastOneTrue;
                    if (random)
                    {
                        if (randomTimer > 0)
                        {
                            randomTimer -= Engine.DeltaTime;
                            if (randomTimer <= 0)
                            {
                                randomTimer = 0;
                                GenerateRandomTopLayerGrid();
                            }
                        }
                        if (!wasRandom)
                        {
                            randomTimer = 0.2f;
                        }
                    }
                    else if(wasRandom)
                    {
                        randomTimer = 0;
                    }
                }
            }
            public void GenerateGrid(char tile)
            {
                Grid?.RemoveSelf();
                Interceptor?.RemoveSelf();
                Grid = GFX.FGAutotiler.GenerateBox(tile, (int)(Width / 8), (int)(Height / 8)).TileGrid;
                Interceptor = new TileInterceptor(Grid, false);
                Add(Grid, Interceptor);
            }
            public void GenerateRandomTopLayerGrid()
            {
                Overlay?.RemoveSelf();
                OverlayInterceptor?.RemoveSelf();
                Overlay = GFX.FGAutotiler.GenerateCustomBox(0, 0, (int)(Width / 8), (int)(Height / 8), default, Condition).TileGrid;
                OverlayInterceptor = new TileInterceptor(Overlay, false);
                Add(Overlay, OverlayInterceptor);
            }
            public void GenerateTopLayerGrid(char tile, int swap)
            {
                Overlay?.RemoveSelf();
                OverlayInterceptor?.RemoveSelf();
                next = tile;
                Overlay = GFX.FGAutotiler.GenerateCustomBox(0, 0, (int)(Width / 8), (int)(Height / 8), swap, swap, default, Condition).TileGrid;
                OverlayInterceptor = new TileInterceptor(Overlay, false);
                Add(Overlay, OverlayInterceptor);
            }
            public char Condition(int x, int y, int swapX, int swapY)
            {
                if ((x + y) / 2 < swapX)
                {
                    return next;
                }
                return '0';
            }
            public char Condition(int x, int y)
            {
                if (!isForPuzzle && random)
                {
                    return pairs.Random().Tile;
                }
                else
                {
                    return CurrentTile;
                }
            }
            private char next;
            public void Advance()
            {
                if (Tiles.Length < 1) return;
                next = Tiles[(Index + 1) % Tiles.Length];
                coroutine.Replace(routine(CurrentTile, next));
                Index++;
                Index %= Tiles.Length;
            }
            public void AdvanceTo(char c)
            {
                if (CurrentTile != c && pairs.Find(p => p.Tile == c) is var pair && pair.Tile != default)
                {
                    next = pair.Tile;
                    coroutine.Replace(routine(CurrentTile, next));
                }
            }
            private IEnumerator routine(char prev, char next)
            {
                char current = prev;
                int min = (int)(Math.Min(Width, Height) / 8);
                for (int i = 0; i < min; i++)
                {
                    GenerateTopLayerGrid(next, i);
                    yield return 0.1f;
                }
                Overlay.RemoveSelf();
                OverlayInterceptor.RemoveSelf();
                GenerateGrid(next);
            }
        }
        public List<TileshiftBlock> Nodes = [];
        public EntityID ID;
        public TiletypePuzzle(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            ID = id;
            FlagOnComplete = data.Attr("flagOnComplete");
            NodeFlagPrefix = data.Attr("nodeFlagPrefix");
            Vector2[] nodes = data.NodesOffset(offset);
            char[] tiles = data.Attr("tiletypes").ToCharArray();
            char[] sequence = data.Attr("solution").ToCharArray();
            float width = data.Float("nodeWidth");
            float height = data.Float("nodeHeight");
            bool isnew = false;
            if (!Cache.TryGetValue(ID, out var value))
            {
                value = ([]);
                Cache.Add(ID, value);
                isnew = true;
            }
            for (int i = 0; i < nodes.Length && i < sequence.Length; i++)
            {
                if (isnew || value.Count <= i)
                {
                    value.Add(0);
                }
                TileshiftBlock node = new(nodes[i], width, height, tiles, sequence[i], value[i]);
                Nodes.Add(node);
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            foreach (TileshiftBlock node in Nodes)
            {
                scene.Add(node);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            for (int i = 0; i < Nodes.Count; i++)
            {
                Cache[ID][i] = Nodes[i].Index;
                Nodes[i].RemoveSelf();
            }
        }
        public override void Update()
        {
            base.Update();
            Level level = Scene as Level;
            bool allGood = !string.IsNullOrEmpty(FlagOnComplete);
            for (int i = 0; i < Nodes.Count; i++)
            {
                TileshiftBlock node = Nodes[i];
                if (level.Session.GetFlag(NodeFlagPrefix + i))
                {
                    node.Advance();
                    level.Session.SetFlag(NodeFlagPrefix + i, false);
                }
                allGood &= node.IsCorrect;
            }
            if (allGood)
            {
                level.Session.SetFlag(FlagOnComplete);
            }
        }
    }
}