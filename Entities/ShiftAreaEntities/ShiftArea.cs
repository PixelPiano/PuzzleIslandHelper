using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.ShiftAreaEntities
{

    [CustomEntity("PuzzleIslandHelper/ShiftArea")]
    [Tracked]
    public class ShiftArea : Entity
    {
        private char bgFrom, bgTo, fgFrom, fgTo;
        private int[] indices;
        public static Dictionary<string, LevelTiles> TilesLookup => PianoModule.Session.TilesLookup;
        public struct LevelTiles
        {
            public TileGrid BgTiles;
            public TileGrid FgTiles;
        }
        public LevelTiles Tiles;
        public Vector2[] Points;
        public VertexPositionColor[] Vertices;
        private Vector2 start;
        public bool UsesNodes;
        public ShiftArea(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            bgFrom = data.Char("bgFrom", ' ');
            bgTo = data.Char("bgTo", ' ');
            fgFrom = data.Char("fgFrom", ' ');
            fgTo = data.Char("fgTo", ' ');
            string[] ind = data.Attr("indices").Replace(" ", "").Split(',');
            List<int> temp = new();
            foreach (string s in ind)
            {
                if (int.TryParse(s, out int parsed))
                {
                    temp.Add(parsed);
                }
                else break;
            }
            indices = temp.ToArray();
            Depth = -10001;
            Collider = new Hitbox(data.Width, data.Height);
            Tag |= Tags.TransitionUpdate;
            start = Position;
            Points = data.NodesWithPosition(Vector2.Zero);
            UsesNodes = Points is not null && Points.Length > 1;
            Vertices = new VertexPositionColor[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                Vertices[i] = new VertexPositionColor(new Vector3(Points[i], 0), Color.White);
            }
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level) return;
            for (int i = 0; i < Points.Length; i++)
            {
                Vector2 newPosition = Points[i] + (Position - start) - (level.Camera.Position - level.LevelOffset);
                Vertices[i].Position = new Vector3(newPosition, 0);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            SetUpTiles(scene);
        }
        public void SetUpTiles(Scene scene)
        {
            string name = (scene as Level).Session.Level;
            if (TilesLookup.ContainsKey(name))
            {
                Tiles = TilesLookup[name];
            }
            else
            {
                GetTiles(scene);
                TilesLookup.Add(name, Tiles);
            }
            Add(Tiles.BgTiles, Tiles.FgTiles);
            if (PianoUtils.SeekController<ShiftAreaRenderer>(scene) == null)
            {
                scene.Add(new ShiftAreaRenderer(true, Tiles.FgTiles));
                scene.Add(new ShiftAreaRenderer(false, Tiles.BgTiles));
            }
        }
        public void DrawMask()
        {
            GFX.DrawIndexedVertices(Matrix.Identity, Vertices, Vertices.Length, indices, indices.Length / 3);
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            TilesLookup.Clear();
        }
        public void GetTiles(Scene scene)
        {

            Level level = scene as Level;
            int ox = (int)Math.Round((float)level.LevelSolidOffset.X);
            int oy = (int)Math.Round((float)level.LevelSolidOffset.Y);
            int tw = (int)Math.Ceiling(level.Bounds.Width / 8f);
            int th = (int)Math.Ceiling(level.Bounds.Height / 8f);
            if (fgFrom != fgTo)
            {
                VirtualMap<char> fgData = level.SolidsData;
                VirtualMap<MTexture> fgTexes = level.SolidTiles.Tiles.Tiles;
                VirtualMap<char> newFgData = new VirtualMap<char>(tw + 2, th + 2, '0');
                for (int x4 = ox - 1; x4 < ox + tw + 1; x4++)
                {
                    for (int y3 = oy - 1; y3 < oy + th + 1; y3++)
                    {
                        if (x4 > 0 && x4 < fgTexes.Columns && y3 > 0 && y3 < fgTexes.Rows && fgData[x4, y3] != '0')
                        {
                            newFgData[x4 - ox + 1, y3 - oy + 1] = fgData[x4, y3];
                            if (fgData[x4, y3] == fgFrom)
                            {
                                newFgData[x4 - ox + 1, y3 - oy + 1] = fgTo;
                            }
                        }
                    }
                }
                Autotiler.Generated newFgTiles = GFX.FGAutotiler.GenerateMap(newFgData, paddingIgnoreOutOfLevel: true);
                Tiles.FgTiles = newFgTiles.TileGrid;
                Tiles.FgTiles.VisualExtend = 1;
                Tiles.FgTiles.Visible = false;
            }

            if (bgTo != bgFrom)
            {
                VirtualMap<char> bgData = level.BgData;
                VirtualMap<MTexture> bgTexes = level.BgTiles.Tiles.Tiles;
                VirtualMap<char> newBgData = new VirtualMap<char>(tw + 2, th + 2, '0');
                for (int x2 = ox - 1; x2 < ox + tw + 1; x2++)
                {
                    for (int y = oy - 1; y < oy + th + 1; y++)
                    {
                        if (x2 > 0 && x2 < bgTexes.Columns && y > 0 && y < bgTexes.Rows && bgData[x2, y] != '0')
                        {
                            newBgData[x2 - ox + 1, y - oy + 1] = bgData[x2, y];
                            if (bgData[x2, y] == bgFrom)
                            {
                                newBgData[x2 - ox + 1, y - oy + 1] = bgTo;
                            }
                        }
                    }
                }
                Autotiler.Generated newBgTiles = GFX.BGAutotiler.GenerateMap(newBgData, paddingIgnoreOutOfLevel: true);
                Tiles.BgTiles = newBgTiles.TileGrid;
                Tiles.BgTiles.VisualExtend = 1;
                Tiles.BgTiles.Visible = false;
            }
        }

    }
}
