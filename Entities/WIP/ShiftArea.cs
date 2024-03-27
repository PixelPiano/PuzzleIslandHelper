using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using Celeste.Mod.CommunalHelper;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{

    [CustomEntity("PuzzleIslandHelper/ShiftArea")]
    [Tracked]
    public class ShiftArea : Entity
    {

        public bool HasFG => FGTiles is not null;
        public bool HasBG => BGTiles is not null;

        private char bgFrom, bgTo, fgFrom, fgTo;
        private int[] indices;
        public float Alpha = 1;
        public TileGrid BGTiles;
        public TileGrid FGTiles;
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
        public bool Glitchy;
        public float[] VerticeAlpha;
        public Color[] VerticeColor;
        public Rectangle FurthestBounds;
        public Rectangle ClipRect;
        public VirtualRenderTarget Mask = VirtualContent.CreateRenderTarget("ShiftAreaMask", 320, 180);
        public VirtualRenderTarget BGTarget = VirtualContent.CreateRenderTarget("ShiftAreaBGTarget", 320, 180);
        public VirtualRenderTarget FGTarget = VirtualContent.CreateRenderTarget("ShiftAreaFGTarget", 320, 180);

        public VirtualRenderTarget BGCache = VirtualContent.CreateRenderTarget("ShiftAreaBGCache", 320, 180);
        public VirtualRenderTarget FGCache = VirtualContent.CreateRenderTarget("ShiftAreaFGCache", 320, 180);
        private bool RenderedTiles;
        public bool OnScreen;
        public bool OnlyDrawTilesOnce;
        public ShiftArea(Vector2 position, Vector2 offset, char bgFrom, char bgTo, char fgFrom, char fgTo, Vector2[] nodes, int[] indices) : base(position + offset)
        {
            this.bgFrom = bgFrom;
            this.bgTo = bgTo;
            this.fgFrom = fgFrom;
            this.fgTo = fgTo;
            this.indices = indices;
            Points = nodes;
            Box = PianoUtils.Boundaries(Points, offset);
            Depth = -10001;
            Collider = new Hitbox(8, 8);
            start = Position;
            UsesNodes = Points is not null && Points.Length > 1;
            Vertices = new VertexPositionColor[Points.Length];
            VerticeAlpha = new float[Points.Length];
            VertexBreathing = new VertexBreath[Points.Length];
            VerticeColor = new Color[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                Vertices[i] = new VertexPositionColor(new Vector3(Points[i], 0), Color.White);
                VertexBreathing[i] = new VertexBreath(Calc.Random.Range(4, 8f), Calc.Random.Range(1f, 8));
                VerticeAlpha[i] = 1;
                VerticeColor[i] = Color.White;
            }
            Add(VertexBreathing);
            Tag |= Tags.TransitionUpdate;
        }
        public bool Intersects(ShiftArea area)
        {
            if (area is null || area == this) return false;
            for (int i = 1; i < Points.Length; i += 2)
            {
                for (int j = 1; j < area.Points.Length; j += 2)
                {
                    Vector2 l1 = Points[i - 1];
                    Vector2 l2 = Points[i];

                    Vector2 l3 = area.Points[j - 1];
                    Vector2 l4 = area.Points[j];
                    if (Collide.LineCheck(l1, l2, l3, l4))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public ShiftArea(EntityData data, Vector2 offset) : this(data.Position, offset, data.Char("bgFrom", '0'), data.Char("bgTo", '0'), data.Char("fgFrom", '0'), data.Char("fgTo", '0'), data.NodesWithPosition(Vector2.Zero), GetIndices(data))
        {
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            GetTiles(scene);
            Level level = scene as Level;
            int extend = (int)Calc.Max(BGTiles != null ? BGTiles.VisualExtend : 0, FGTiles != null ? FGTiles.VisualExtend : 0);
            Vector2 topLeft = Box.Position - level.LevelOffset - (Vector2.One * extend * 8);
            Vector2 bottomRight = Box.BottomRight - level.LevelOffset + (Vector2.One * extend * 8);
            ClipRect = PianoUtils.CreateRectangle(topLeft, bottomRight);
            FurthestBounds = PianoUtils.CloneRectangle(ClipRect);
            FurthestBounds.X += (int)level.LevelOffset.X;
            FurthestBounds.Y += (int)level.LevelOffset.Y;
            origLevelOffset = level.LevelOffset;
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
            bool foundOnScreen = false;
            Vector2 entityDifference = Position - start;
            Vector2 levelDifference = level.Camera.Position - origLevelOffset;
            Vector2 lastPosition = Points[Points.Length - 1] + origLevelOffset;
            for (int i = 0; i < Points.Length; i++)
            {
                Vector2 newPosition = Points[i] + entityDifference - levelDifference - (Vector2.UnitY * VertexBreathing[i].Amount);
                Vertices[i].Position = new Vector3(newPosition, 0);
                Vertices[i].Color = VerticeColor[i] * Alpha;

                if (!foundOnScreen)
                {
                    OnScreen = false;
                    if (Collide.RectToLine(level.Camera.GetBounds(), lastPosition, Points[i] + origLevelOffset))
                    {
                        OnScreen = true;
                        foundOnScreen = true;
                    }
                }
                lastPosition = Points[i] + origLevelOffset;
            }
        }
        public void BeforeRender(bool fg, bool bg)
        {
            if (Scene is not Level level || !OnScreen) return;
            Matrix matrix = Matrix.Identity;

            //Draw vertices as triangles
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Mask);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
            GFX.DrawIndexedVertices(matrix, Vertices, Vertices.Length, indices, indices.Length / 3);
            Draw.SpriteBatch.End();

            //Draw the tiles ONCE (this reduces the load by around 14%)
            if (!RenderedTiles && OnlyDrawTilesOnce)
            {
                if (fg && HasFG)
                {
                    Engine.Graphics.GraphicsDevice.SetRenderTarget(FGCache);
                    Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
                    RenderTiles(true, level);
                    Draw.SpriteBatch.End();
                }
                if (bg && HasBG)
                {
                    Engine.Graphics.GraphicsDevice.SetRenderTarget(BGCache);
                    Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
                    RenderTiles(false, level);
                    Draw.SpriteBatch.End();
                }
            }
            if (bg && HasBG)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(BGTarget);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
                if (OnlyDrawTilesOnce)
                {
                    Draw.SpriteBatch.Draw(BGCache, Vector2.Zero, Color.White);
                }
                else
                {
                    RenderTiles(false, level);
                }
                Draw.SpriteBatch.End();

                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, EasyRendering.AlphaMaskBlendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
                Draw.SpriteBatch.Draw(Mask, Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();
            }
            if (fg && HasFG)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(FGTarget);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
                if (OnlyDrawTilesOnce)
                {
                    Draw.SpriteBatch.Draw(FGCache, Vector2.Zero, Color.White);
                }
                else
                {
                    RenderTiles(true, level);
                }
                Draw.SpriteBatch.End();

                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, EasyRendering.AlphaMaskBlendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
                Draw.SpriteBatch.Draw(Mask, Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();
            }
            RenderedTiles = true;
        }
        public static int[] GetIndices(EntityData data)
        {
            string[] ind = data.Attr("indices").Replace(" ", "").Split(',');
            List<int> temp = new();
            foreach (string s in ind)
            {
                if (int.TryParse(s, out int parsed))
                {
                    temp.Add(parsed);
                }
            }
            return temp.ToArray();
        }
        public IEnumerator FadeAlphaTo(float alpha, float time, Ease.Easer ease)
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                Alpha = ease(i) * alpha;
                yield return null;
            }
        }
        public void RenderTiles(bool fg, Level level)
        {
            TileGrid grid = fg ? FGTiles : BGTiles;
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
            Vector2 vector = box.AbsolutePosition - (box.AbsolutePosition - origLevelOffset) - grid.VisualExtend * Vector2.One * 8; ;
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
            Draw.HollowRect(FurthestBounds, Color.MediumBlue);
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
                bool allowAir = fgFrom == '0' || fgTo == '0';
                VirtualMap<char> fgData = level.SolidsData;
                VirtualMap<MTexture> fgTexes = level.SolidTiles.Tiles.Tiles;
                VirtualMap<char> newFgData = new VirtualMap<char>(tw + 2, th + 2, '0');
                for (int x4 = ox - 1; x4 < ox + tw + 1; x4++)
                {
                    for (int y3 = oy - 1; y3 < oy + th + 1; y3++)
                    {
                        if (x4 > 0 && x4 < fgTexes.Columns && y3 > 0 && y3 < fgTexes.Rows && (allowAir || fgData[x4, y3] != '0'))
                        {
                            char c = fgData[x4, y3];
                            newFgData[x4 - ox + 1, y3 - oy + 1] = c == fgFrom ? fgTo : c == fgTo ? fgFrom : c;
                        }
                    }
                }
                Autotiler.Generated newFgTiles = GFX.FGAutotiler.GenerateMap(newFgData, paddingIgnoreOutOfLevel: true);
                FGTiles = newFgTiles.TileGrid;
                FGTiles.VisualExtend = 1;
                FGTiles.Visible = false;
            }

            if (bgTo != bgFrom)
            {
                bool allowAir = bgFrom == '0' || bgTo == '0';
                VirtualMap<char> bgData = level.BgData;
                VirtualMap<MTexture> bgTexes = level.BgTiles.Tiles.Tiles;
                VirtualMap<char> newBgData = new VirtualMap<char>(tw + 2, th + 2, '0');
                for (int x2 = ox - 1; x2 < ox + tw + 1; x2++)
                {
                    for (int y = oy - 1; y < oy + th + 1; y++)
                    {
                        if (x2 > 0 && x2 < bgTexes.Columns && y > 0 && y < bgTexes.Rows && (allowAir || bgData[x2, y] != '0'))
                        {
                            char c = bgData[x2, y];
                            if (newBgData.Columns > x2 - ox + 1 && newBgData.Rows > y - oy + 1)
                            {
                                int cX, cY;
                                cX = x2 - ox + 1;
                                cY = y - oy + 1;
                                if (cX >= 0 && cY >= 0 && cX < newBgData.Columns && cY < newBgData.Rows)
                                {
                                    newBgData[cX, cY] = c == bgFrom ? bgTo : c == bgTo ? bgFrom : c;
                                }
                            }
                        }
                    }
                }
                Autotiler.Generated newBgTiles = GFX.BGAutotiler.GenerateMap(newBgData, paddingIgnoreOutOfLevel: true);
                BGTiles = newBgTiles.TileGrid;
                BGTiles.VisualExtend = 1;
                BGTiles.Visible = false;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            FGTarget?.Dispose();
            FGTarget = null;
            BGTarget?.Dispose();
            BGTarget = null;

        }
    }
}