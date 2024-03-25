using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using System.Linq;
using static Celeste.Mod.PuzzleIslandHelper.Triggers.SceneSwitch;
using System.Security.Policy;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{

    [CustomEntity("PuzzleIslandHelper/ShiftArea")]
    [Tracked]
    public class ShiftArea : Entity
    {
        private char bgFrom, bgTo, fgFrom, fgTo;
        private int[] indices;
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
        public class VertexBreath : Component
        {
            public float Interval;
            public float MoveDistance;
            public float Amount
            {
                get
                {
                    return GetAmountAt(timer / Interval);
                }
            }
            private float timer;
            public VertexBreath(float interval, float moveDistance) : base(true, false)
            {
                Interval = interval;
                timer = Calc.Random.Range(0, Interval * 0.8f);
                MoveDistance = moveDistance;
            }
            public override void Update()
            {
                base.Update();
                timer = (timer + Engine.DeltaTime) % Interval;

            }
            public float GetPercentAt(float percent)
            {
                float amount = (0.5f - MathHelper.Distance(percent, 0.5f) * 2) * MoveDistance;
                return amount;
            }
            public float GetAmountAt(float percent)
            {
                percent = 0.5f - MathHelper.Distance(percent, 0.5f);
                float amount = Ease.SineInOut(percent * 2) * MoveDistance;
                return amount;
            }
        }
        public VertexBreath[] VertexBreathing;
        public Vector2 origLevelOffset;
        public Collider Box;
        public ShiftArea(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            bgFrom = data.Char("bgFrom", '0');
            bgTo = data.Char("bgTo", '0');
            fgFrom = data.Char("fgFrom", '0');
            fgTo = data.Char("fgTo", '0');
            string[] ind = data.Attr("indices").Replace(" ", "").Split(',');
            List<int> temp = new();
            foreach (string s in ind)
            {
                if (int.TryParse(s, out int parsed))
                {
                    temp.Add(parsed);
                }
            }
            indices = temp.ToArray();
            Depth = -10001;
            Collider = new Hitbox(data.Width, data.Height);
            start = Position;
            Points = data.NodesWithPosition(Vector2.Zero);
            UsesNodes = Points is not null && Points.Length > 1;
            Vertices = new VertexPositionColor[Points.Length];
            VertexBreathing = new VertexBreath[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                Vertices[i] = new VertexPositionColor(new Vector3(Points[i], 0), Color.White);
                VertexBreathing[i] = new VertexBreath(Calc.Random.Range(4, 8f), Calc.Random.Range(1f, 8));
            }
            Box = PianoUtils.Boundaries(Points, offset);
            Add(VertexBreathing);
            Tag |= Tags.TransitionUpdate;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Tiles = GetTiles(scene);
            origLevelOffset = (scene as Level).LevelOffset;
            UpdateVertices();
        }
        public override void Update()
        {
            UpdateVertices();
            base.Update();
        }
        public void UpdateVertices()
        {
            if (Scene is not Level level) return;
            for (int i = 0; i < Points.Length; i++)
            {
                Vector2 entityDifference = Position - start;
                Vector2 levelDifference = level.Camera.Position - origLevelOffset;
                Vector2 newPosition = Points[i] + entityDifference - levelDifference;
                Vertices[i].Position = new Vector3(newPosition.X, newPosition.Y - VertexBreathing[i].Amount, 0);
            }
        }
        public void RenderTiles(bool fg, Level level)
        {
            TileGrid grid = fg ? Tiles.FgTiles : Tiles.BgTiles;
            if (grid is null || grid.Alpha <= 0f) return;
            Vector2 offset = origLevelOffset - level.Camera.Position;

            Rectangle clippedRenderTiles = GetClippedRenderTiles(grid, Box);
            offset -= grid.VisualExtend * Vector2.One * 8;
            int tileWidth = grid.TileWidth;
            int tileHeight = grid.TileHeight;
            Color color = grid.Color * grid.Alpha;
            Vector2 position2 = new Vector2(offset.X + clippedRenderTiles.Left * tileWidth, offset.Y + clippedRenderTiles.Top * tileHeight);
            position2 = position2.Round();
            for (int i = clippedRenderTiles.Left; i < clippedRenderTiles.Right; i++)
            {
                for (int j = clippedRenderTiles.Top; j < clippedRenderTiles.Bottom; j++)
                {
                    MTexture mTexture = grid.Tiles[i, j];
                    if (mTexture != null)
                    {
                        Draw.SpriteBatch.Draw(mTexture.Texture.Texture_Safe, position2, mTexture.ClipRect, color);
                    }

                    position2.Y += tileHeight;
                }

                position2.X += tileWidth;
                position2.Y = offset.Y + clippedRenderTiles.Top * tileHeight;
            }
        }
        public Rectangle GetClippedRenderTiles(TileGrid grid, Collider box)
        {
            int left, top, bottom, right;
            Vector2 vector = box.AbsolutePosition - (box.AbsolutePosition - origLevelOffset) - grid.VisualExtend * Vector2.One * 8;;
            int extend = grid.VisualExtend;
            left = (int)Math.Max(0.0, Math.Floor((box.Left - vector.X) / grid.TileWidth) - extend);
            top = (int)Math.Max(0.0, Math.Floor((box.Top - vector.Y) / grid.TileHeight) - extend);
            right = (int)Math.Min(grid.TilesX, Math.Ceiling((box.Right - vector.X) / grid.TileWidth) + extend);
            bottom = (int)Math.Min(grid.TilesY, Math.Ceiling((box.Bottom - vector.Y) / grid.TileHeight) + extend);

            left = Math.Max(left, 0);
            top = Math.Max(top, 0);
            right = Math.Min(right, grid.TilesX);
            bottom = Math.Min(bottom, grid.TilesY);

            return new Rectangle(left, top, right - left, bottom - top);
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(Box, Color.Blue);
        }
        public void DrawMask(Matrix matrix)
        {
            GFX.DrawIndexedVertices(matrix, Vertices, Vertices.Length, indices, indices.Length / 3);
        }
        public LevelTiles GetTiles(Scene scene)
        {
            Level level = scene as Level;
            int ox = (int)Math.Round((float)level.LevelSolidOffset.X);
            int oy = (int)Math.Round((float)level.LevelSolidOffset.Y);
            int tw = (int)Math.Ceiling(level.Bounds.Width / 8f);
            int th = (int)Math.Ceiling(level.Bounds.Height / 8f);

            bool[] allowAir = { fgFrom == '0' || fgTo == '0', bgFrom == '0' || bgTo == '0' };
            if (fgFrom != fgTo)
            {
                VirtualMap<char> fgData = level.SolidsData;
                VirtualMap<MTexture> fgTexes = level.SolidTiles.Tiles.Tiles;
                VirtualMap<char> newFgData = new VirtualMap<char>(tw + 2, th + 2, '0');
                for (int x4 = ox - 1; x4 < ox + tw + 1; x4++)
                {
                    for (int y3 = oy - 1; y3 < oy + th + 1; y3++)
                    {
                        if (x4 > 0 && x4 < fgTexes.Columns && y3 > 0 && y3 < fgTexes.Rows && (allowAir[0] || fgData[x4, y3] != '0'))
                        {
                            char c = fgData[x4, y3];
                            newFgData[x4 - ox + 1, y3 - oy + 1] = c == fgFrom ? fgTo : c == fgTo ? fgFrom : c;
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
                        if (x2 > 0 && x2 < bgTexes.Columns && y > 0 && y < bgTexes.Rows && (allowAir[1] || bgData[x2, y] != '0'))
                        {
                            char c = bgData[x2, y];
                            newBgData[x2 - ox + 1, y - oy + 1] = c == bgFrom ? bgTo : c == bgTo ? bgFrom : c;
                        }
                    }
                }
                Autotiler.Generated newBgTiles = GFX.BGAutotiler.GenerateMap(newBgData, paddingIgnoreOutOfLevel: true);
                Tiles.BgTiles = newBgTiles.TileGrid;
                Tiles.BgTiles.VisualExtend = 1;
                Tiles.BgTiles.Visible = false;
            }
            return Tiles;
        }
    }
}