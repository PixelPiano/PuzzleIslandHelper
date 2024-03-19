using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.FancyTileEntities;
using System;
using System.Collections.Generic;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Celeste.Mod.PandorasBox;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [Tracked]
    public class ShiftAreaRenderer : Entity
    {
        private float testval;
        private static VirtualRenderTarget _BGTarget;
        private static VirtualRenderTarget _FGTarget;
        public static VirtualRenderTarget BGTarget => _BGTarget ??= VirtualContent.CreateRenderTarget("ShiftAreaBGTarget", 320, 180);
        public static VirtualRenderTarget FGTarget => _FGTarget ??= VirtualContent.CreateRenderTarget("ShiftAreaFGTarget", 320, 180);
        public bool FG;
        public VirtualRenderTarget Target => FG ? FGTarget : BGTarget;
        public ShiftAreaRenderer(bool fg) : base(Vector2.Zero)
        {
            FG = fg;
            Depth = fg ? -10001 : 9999;
            //Tag |= Tags.TransitionUpdate;
            Add(new BeforeRenderHook(BeforeRender));
        }
        private void DrawShiftAreaMasks(Scene scene)
        {
            Level level = scene as Level;
            foreach (ShiftArea area in level.Tracker.GetEntities<ShiftArea>())
            {
                area.DrawMask();
            }
        }
        private void DrawShiftAreaTiles(Scene scene, Vector2 position)
        {
            Level level = scene as Level;
            ShiftArea area = level.Tracker.GetEntity<ShiftArea>();
            if (area is not null)
            {
                if (FG) area.RenderFG(position);
                else area.RenderBG(position);
            }
        }
        public static void Unload()
        {
            _FGTarget?.Dispose();
            _BGTarget?.Dispose();
            _FGTarget = null;
            _BGTarget = null;
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level) return;
            Draw.SpriteBatch.Draw(Target, level.LevelOffset, Color.White);
        }
        public void BeforeRender()
        {
            if (Scene is not Level level) return;
            Matrix matrix = Matrix.Identity;

            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.TempA);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap,
                DepthStencilState.None, RasterizerState.CullNone, null, matrix);

            foreach (ShiftArea area in level.Tracker.GetEntities<ShiftArea>())
            {
                area.DrawMask();
            }

            Draw.SpriteBatch.End();

            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap,
                DepthStencilState.None, RasterizerState.CullNone, null, matrix);

            ShiftArea singlearea = level.Tracker.GetEntity<ShiftArea>();
            if (singlearea is not null)
            {
                if (FG) singlearea.RenderFG(Vector2.Zero);
                else singlearea.RenderBG(Vector2.Zero);
            }

            Draw.SpriteBatch.End();

            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, EasyRendering.AlphaMaskBlendState, SamplerState.PointClamp,
                DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
            Draw.SpriteBatch.Draw(GameplayBuffers.TempA, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }
    }

    [CustomEntity("PuzzleIslandHelper/ShiftArea")]
    [Tracked]
    public class ShiftArea : Entity
    {
        public LevelTiles Tiles;
        private char bgFrom, bgTo, fgFrom, fgTo;
        private Collider ClipBox;
        private bool setRect;
        private int[] indices;
        public static Dictionary<string, LevelTiles> TilesLookup => PianoModule.Session.TilesLookup;
        public struct LevelTiles
        {
            public TileGrid BgTiles;
            public TileGrid FgTiles;
        }
        public Vector2[] Points;
        public Vector2[] CurrentPoints;
        public Vector2[] PreviousPoints;
        private Rectangle ClipRect;
        public VertexPositionColor[] Vertices;
        private Vector2 start;
        public ShiftArea(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            bgFrom = data.Char("bgFrom", ' ');
            bgTo = data.Char("bgTo", ' ');
            fgFrom = data.Char("fgFrom", ' ');
            fgTo = data.Char("fgTo", ' ');
            Depth = -10001;
            Collider = new Hitbox(data.Width, data.Height);
            Tag |= Tags.TransitionUpdate;
            start = Position;
            Points = data.NodesWithPosition(Vector2.Zero);
            PreviousPoints = CurrentPoints = Points;
            Vertices = new VertexPositionColor[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                Vertices[i] = new VertexPositionColor(new Vector3(Points[i], 0), Color.White);
            }
            indices = new int[] { 0, 1, 2 };
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            ClipBox = Points != null && Points.Length > 1 ? PianoUtils.Boundaries(Points) : Collider; //if entity uses nodes, get collider with all nodes inside
            if (PianoUtils.SeekController<ShiftAreaRenderer>(scene) == null)
            {
                scene.Add(new ShiftAreaRenderer(true));
                scene.Add(new ShiftAreaRenderer(false));
            }
        }
        public override void Update()
        {
            base.Update();
            PreviousPoints = CurrentPoints;
            for (int i = 0; i < Points.Length; i++)
            {
                Vector2 newPosition = Points[i] + (Position - start);
                Vertices[i].Position = new Vector3(newPosition, 0);
                CurrentPoints[i] = newPosition;
            }
            ClipBox = Points != null && Points.Length > 1 ? PianoUtils.Boundaries(Points) : Collider; //if entity uses nodes, get collider with all nodes inside

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
        public void RenderBG(Vector2 offset)
        {
            RenderAt(Tiles.BgTiles, offset - (Tiles.BgTiles.VisualExtend * Vector2.One * 8));
        }
        public void RenderFG(Vector2 offset)
        {
            RenderAt(Tiles.FgTiles, offset - (Tiles.FgTiles.VisualExtend * Vector2.One * 8));
        }
        public void RenderAt(TileGrid grid, Vector2 offset)
        {
            {
                if (grid.Alpha <= 0f)
                {
                    return;
                }
                if (!setRect || Moved())
                {
                    ClipRect = GetClippedRenderTiles(grid, offset, Position);
                    setRect = true;
                }
                Rectangle clippedRenderTiles = ClipRect;
                int tileWidth = grid.TileWidth;
                int tileHeight = grid.TileHeight;
                Color color = grid.Color * grid.Alpha;
                Vector2 position2 = new Vector2(offset.X + (float)(clippedRenderTiles.Left * tileWidth), offset.Y + (float)(clippedRenderTiles.Top * tileHeight));
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
                    position2.Y = offset.Y + (float)(clippedRenderTiles.Top * tileHeight);
                }
            }
        }
        public bool Moved()
        {
            return PreviousPoints != CurrentPoints;
        }
        public Rectangle GetClippedRenderTiles(TileGrid grid, Vector2 offset, Vector2 position)
        {
            int left, top, bottom, right;
            Vector2 leveloffset = (Scene is Level level) ? position - level.LevelOffset : Vector2.Zero; //entity offset from level offset
            Collider collider = ClipBox; //if entity uses nodes, get collider with all nodes inside

            Vector2 vector = collider.AbsolutePosition + offset - leveloffset;
            int extend = 0;//grid.VisualExtend;
            left = (int)Math.Max(0.0, Math.Floor((collider.AbsoluteLeft - vector.X) / grid.TileWidth) - extend);
            top = (int)Math.Max(0.0, Math.Floor((collider.AbsoluteTop - vector.Y) / grid.TileHeight) - extend);
            right = (int)Math.Min(grid.TilesX, Math.Ceiling((collider.AbsoluteRight - vector.X) / grid.TileWidth) + extend);
            bottom = (int)Math.Min(grid.TilesY, Math.Ceiling((collider.AbsoluteBottom - vector.Y) / grid.TileHeight) + extend);

            left = Math.Max(left, 0);
            top = Math.Max(top, 0);
            right = Math.Min(right, grid.TilesX);
            bottom = Math.Min(bottom, grid.TilesY);

            return new Rectangle(left, top, right - left, bottom - top);
        }
        public void DrawMask()
        {
            GFX.DrawIndexedVertices(Matrix.Identity, Vertices, 3, indices, 1);
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            TilesLookup.Clear();
        }

    }
}
