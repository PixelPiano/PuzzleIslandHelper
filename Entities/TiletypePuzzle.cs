using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
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
        public class TiletypeNode : Solid
        {
            public TileGrid Grid;
            public TileInterceptor Interceptor;
            public VertexLight Light;
            public char[] Tiles;
            public char CorrectTile;
            public char CurrentTile => Tiles[Index];
            public bool IsCorrect => CurrentTile == CorrectTile;
            public int Index;
            public TiletypeNode(Vector2 position, float width, float height, char[] tiles, char correctTile, int startIndex) : base(position, width, height, false)
            {
                Index = startIndex;
                Tiles = tiles;
                CorrectTile = correctTile;
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
            public override void Update()
            {
                base.Update();
                Light.InSolid = false;
                Light.InSolidAlphaMultiplier = 1;
            }
            public void GenerateGrid(char tile)
            {
                //WHY THE HELL DOES THIS WORK??????????????????????????
                //WHY DO I HAVE TO REMOVE THE GRID AND INTERCEPTOR AND REPLACE THEM BOTH WHY WHYWHYWHYWHYWHYWHYWYHWHYWHYWHWHYWYHWHYW
                Grid?.RemoveSelf();
                Interceptor?.RemoveSelf();
                Grid = GFX.FGAutotiler.GenerateBox(tile, (int)Width / 8, (int)Height / 8).TileGrid;
                Interceptor = new TileInterceptor(Grid, false);
                Add(Grid, Interceptor);
            }
            public void Advance()
            {
                if (Tiles.Length < 1) return;
                Index++;
                Index %= Tiles.Length;
                GenerateGrid(CurrentTile);
            }
        }
        public List<TiletypeNode> Nodes = [];
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
                TiletypeNode node = new(nodes[i], width, height, tiles, sequence[i], value[i]);
                Nodes.Add(node);
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            foreach (TiletypeNode node in Nodes)
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
                TiletypeNode node = Nodes[i];
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