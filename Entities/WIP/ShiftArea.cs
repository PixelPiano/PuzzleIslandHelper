using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [Tracked]
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
        public VertexBreath(float interval, float moveDistance, bool randomizeTimerStart = true) : base(true, false)
        {
            Interval = interval;
            if (randomizeTimerStart)
            {
                RandomizeTimer();
            }
            MoveDistance = moveDistance;
        }
        public void RandomizeTimer()
        {
            timer = Calc.Random.Range(0, Interval * 0.8f);
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
    [CustomEntity("PuzzleIslandHelper/ShiftArea")]
    [Tracked(true)]
    public class ShiftArea : Entity
    {
        public bool CullOffScreen = true;
        public int PolygonScreenIndex = -1;
        public int AreaDepth;
        public int Extend;
        public float Alpha = 1;
        public float LineAlpha = 1;
        public float Scale = 1;
        public float GrowTime;
        public float GlitchAmount = 15;
        public float GlitchAmp = 0.4f;
        public float ShineAmount;

        public bool TilesCached = false;
        public bool Grows;
        public bool DisableBreathing;
        public bool DrawTiles = true;
        public bool OnScreen
        {
            get
            {
                return !CullOffScreen || onscreen;
            }
            set
            {
                onscreen = value;
            }
        }
        private bool onscreen;
        public bool UsesNodes;
        public bool Glitchy;
        public bool ConnectLines;
        public bool WasFixed;
        public bool SwapBG;
        public bool SwapFG;
        public bool Shrinking;
        public bool LinesConnected;
        public bool FollowCamera;
        public bool Fading;
        public bool ExpandLinesOnScreen;
        public bool FillIn;
        public Color FillColor = Color.Black;
        public float FillAlpha = 1;
        public bool HasFG => FGTiles is not null;
        public bool HasBG => BGTiles is not null;

        public char BgFrom, BgTo, FgFrom, FgTo;

        public Vector2 origLevelOffset;
        public Vector2 Origin;
        public Vector2 ExtendOffset;
        public Rectangle FurthestBounds;
        public Rectangle ClipRect;
        public Color AreaColor = Color.White;

        public TileGrid BGTiles, FGTiles;
        public Collider Box;
        public VirtualRenderTarget Mask, FillMask, FillTarget, BGTarget, FGTarget, BGCache, FGCache;

        public int[] indices;
        public float[] VerticeAlpha;
        public float[] PointLineAlphas;
        public float[] PointLineAmount;
        public Vector2[] Points;
        public Color[] VerticeColor;
        public Vector2[] FillPoints;
        public VertexPositionColor[] Vertices;
        public VertexPositionColor[] FillVertices;
        public VertexBreath[] VertexBreathing;
        public SineWave LineAlphaFader;
        public string Flag;
        public bool Inverted;
        public int LineThickness = 1;
        private float cacheWindow = 1;
        public bool State
        {
            get
            {
                if (Scene is not Level level || string.IsNullOrEmpty(Flag)) return true;
                return level.Session.GetFlag(Flag) == !Inverted;
            }
        }
        public EntityID ID;
        public ShiftArea(EntityData data, Vector2 offset, EntityID id) : this(data.Position, offset, data.Char("bgFrom", '0'), data.Char("bgTo", '0'), data.Char("fgFrom", '0'), data.Char("fgTo", '0'), data.NodesWithPosition(Vector2.One * 4), GetIndices(data, false), data.Bool("swapBg"), data.Bool("swapFg"), data.Attr("flag"), data.Bool("inverted"))
        {
            ID = id;
        }
        public ShiftArea(Vector2 position, Vector2 offset, char bgFrom, char bgTo, char fgFrom, char fgTo, Vector2[] nodes, int[] indices, bool swapBG = true, bool swapFG = true, string flag = "", bool inverted = false) : this(position + offset)
        {
            Flag = flag;
            Inverted = inverted;
            SwapBG = swapBG;
            SwapFG = swapFG;
            BgFrom = bgFrom;
            BgTo = bgTo;
            FgFrom = fgFrom;
            FgTo = fgTo;
            this.indices = indices;
            CreateArrays(nodes);
            Add(VertexBreathing);
            Collider = new Hitbox(10, 10);
            Box = PianoUtils.Boundaries(Points, offset);
        }
        public ShiftArea(Vector2 position) : base(position)
        {
            Depth = -10001;
            Tag |= Tags.TransitionUpdate;
        }
        public void FillSolid(Color color)
        {
            Color[] colors = VerticeColor;
            float[] alphas = VerticeAlpha;
            SetVertexColor(color);
            SetVertexAlpha(1);
            GFX.DrawIndexedVertices(Matrix.Identity, Vertices, Vertices.Length, indices, indices.Length / 3);
            VerticeColor = colors;
            VerticeAlpha = alphas;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            int extend = 1;
            Vector2 topLeft = Box.Position - (Vector2.One * extend * 8);
            Vector2 bottomRight = Box.BottomRight + (Vector2.One * extend * 8);
            FurthestBounds = PianoUtils.CreateRectangle(topLeft, bottomRight);
            int width = FurthestBounds.Width;
            int height = FurthestBounds.Height;
            try
            {
                Mask = VirtualContent.CreateRenderTarget("ShiftAreaMask", width, height);
                FillMask = VirtualContent.CreateRenderTarget("ShiftAreaFillMask", width, height);
                FillTarget = VirtualContent.CreateRenderTarget("ShiftAreaFillTarget", width, height);
                BGTarget = VirtualContent.CreateRenderTarget("ShiftAreaBGTarget", width, height);
                FGTarget = VirtualContent.CreateRenderTarget("ShiftAreaFGTarget", width, height);
                BGCache = VirtualContent.CreateRenderTarget("ShiftAreaBGCache", width, height);
                FGCache = VirtualContent.CreateRenderTarget("ShiftAreaFGCache", width, height);
            }
            catch
            {
                RemoveSelf();
            }
        }
        public int TileExtend = 1;
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = scene as Level;
            GetTiles(scene);
            Extend = (int)Calc.Max(HasFG ? FGTiles.VisualExtend : 0, HasBG ? BGTiles.VisualExtend : 0);
            ExtendOffset = Extend * Vector2.One;
            Position = FurthestBounds.Location.ToVector2();
            origLevelOffset = level.LevelOffset;
            UpdateVertices();
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);

            Draw.HollowRect(Box, Color.Blue);
            Draw.HollowRect(FurthestBounds, Color.Cyan);

            for (int i = 2; i < Vertices.Length; i += 3)
            {

                Vector2[] points = new Vector2[3];
                points[0] = Vertices[i - 2].Position.XY() + Position;
                points[1] = Vertices[i - 1].Position.XY() + Position;
                points[2] = Vertices[i].Position.XY() + Position;
                Draw.Line(points[0], points[1], Color.Pink);
                Draw.Line(points[1], points[2], Color.Pink);
                Draw.Line(points[2], points[0], Color.Pink);
                for (int j = 0; j < 3; j++)
                {
                    Draw.HollowRect(points[j] - Vector2.One * 2, 4, 4, Color.Red);
                }
            }
        }
        public override void Update()
        {
            if (cacheWindow > 0)
            {
                cacheWindow -= Engine.DeltaTime;
            }
            UpdateVertices();
            base.Update();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            FGTarget?.Dispose();
            FGTarget = null;
            BGTarget?.Dispose();
            BGTarget = null;
            BGCache?.Dispose();
            BGCache = null;
            FGCache?.Dispose();
            FGCache = null;
            Mask?.Dispose();
            Mask = null;
        }
        public void AddOffset(Vector2 offset)
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Position += new Vector3(offset, 0);
                FillVertices[i].Position += new Vector3(offset, 0);
            }
        }
        public void SubtractOffset(Vector2 offset)
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Position -= new Vector3(offset, 0);
                FillVertices[i].Position -= new Vector3(offset, 0);
            }
        }
        public virtual void BeforeRender(bool fg, bool bg)
        {
            if (!OnScreen || !Visible || !State) return;
            bool useBg = bg && HasBG;
            bool useFg = fg && HasFG;
            float glitchAmount = GlitchAmount;
            float glitchAmp = GlitchAmp;
            GlitchAmount = Calc.Random.Range(GlitchAmount / 5, glitchAmount);
            GlitchAmp = Calc.Random.Range(GlitchAmp / 5, glitchAmp);
            Matrix matrix = Matrix.Identity;
            RenderMask(matrix);
            if (FillIn)
            {
                RenderFillToMask(matrix);
                RenderFillTarget(matrix);
                RenderFillMaskTarget(matrix);
            }
            if (useBg)
            {
                if (!TilesCached) RenderCache(false, matrix);
                if (DrawTiles)
                {
                    RenderTilesToTarget(false, matrix);
                    RenderMaskTarget(matrix);
                }
            }
            if (useFg)
            {
                if (!TilesCached) RenderCache(true, matrix);
                if (DrawTiles)
                {
                    RenderTilesToTarget(true, matrix);
                    RenderMaskTarget(matrix);
                }
            }


            if (!DrawTiles)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(fg ? FGTarget : BGTarget);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            }

            if (ConnectLines || FillIn)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
                if (FillIn)
                {
                    Draw.SpriteBatch.Draw(FillTarget, Vector2.Zero, Color.White);
                }
                if (ConnectLines) DrawLines(Vector2.Zero);
                Draw.SpriteBatch.End();
            }
            if (cacheWindow <= 0) TilesCached = true;
            GlitchAmount = glitchAmount;
            GlitchAmp = glitchAmp;
        }
        public void CreateArrays(Vector2[] nodes)
        {
            Points = nodes;

            UsesNodes = Points is not null && Points.Length > 1;
            Vertices = new VertexPositionColor[Points.Length];
            FillVertices = new VertexPositionColor[Points.Length];
            VertexBreathing = new VertexBreath[Points.Length];
            VerticeColor = new Color[Points.Length];
            VerticeAlpha = new float[Points.Length];
            PointLineAmount = new float[Points.Length];
            PointLineAlphas = new float[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                Vertices[i] = new VertexPositionColor(new Vector3(Points[i], 0), Color.White);
                FillVertices[i] = new VertexPositionColor(new Vector3(Points[i], 0), FillColor);
                VertexBreathing[i] = new VertexBreath(Calc.Random.Range(4, 8f), Calc.Random.Range(1f, 8));
                VerticeColor[i] = Color.White;
                VerticeAlpha[i] = 1;
                PointLineAmount[i] = 1;
                PointLineAlphas[i] = 1;
            }
        }
        public void DrawLines(Vector2 offset)
        {
            if (Vertices is null || Vertices.Length <= 0) return;

            for (int i = 2; i < Vertices.Length; i += 3)
            {
                Vector2 p1 = Vertices[i].Position.XY() + offset;
                Vector2 p2 = Vertices[i - 1].Position.XY() + offset;
                Vector2 p3 = Vertices[i - 2].Position.XY() + offset;
                Draw.Line(p1, Calc.LerpSnap(p1, p2, PointLineAmount[i]), Color.Lerp(Color.Black, Color.Gray, PointLineAlphas[i] * LineAlpha), LineThickness);
                Draw.Line(p2, Calc.LerpSnap(p2, p3, PointLineAmount[i - 1]), Color.Lerp(Color.Black, Color.Gray, PointLineAlphas[i - 1] * LineAlpha), LineThickness);
                Draw.Line(p3, Calc.LerpSnap(p3, p1, PointLineAmount[i - 2]), Color.Lerp(Color.Black, Color.Gray, PointLineAlphas[i - 2] * LineAlpha), LineThickness);
            }
        }
        public void SetThickness(int thickness)
        {
            LineThickness = (int)Calc.Max(thickness, 1);
        }
        public void SetVertexAlpha(float amount)
        {
            VerticeAlpha = VerticeAlpha.Select(item => item = amount).ToArray();
        }
        public void SetVertexColor(Color color)
        {
            VerticeColor = VerticeColor.Select(item => item = color).ToArray();
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

        public virtual void DrawVertices(Matrix matrix)
        {
            GFX.DrawIndexedVertices(matrix, Vertices, Vertices.Length, indices, indices.Length / 3);
        }
        public virtual void DrawFill(Matrix matrix)
        {
            GFX.DrawIndexedVertices(matrix, FillVertices, FillVertices.Length, indices, indices.Length / 3);
        }
        public virtual void RenderTileCache(bool fg, Matrix matrix)
        {
            Draw.SpriteBatch.Draw(fg ? FGCache : BGCache, Vector2.Zero, Color.White);
        }
        public void RenderMask(Matrix matrix)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Mask);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
            DrawVertices(matrix);
            Draw.SpriteBatch.End();
            if (Glitchy)
            {
                Mask = EasyRendering.AddGlitch(Mask, GlitchAmount / 4, GlitchAmp / 4);
            }
        }
        public void RenderFillToMask(Matrix matrix)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(FillMask);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
            for (int i = 0; i < FillVertices.Length; i++)
            {
                FillVertices[i].Color = Color.White;
            }
            DrawFill(matrix);
            for (int i = 0; i < FillVertices.Length; i++)
            {
                FillVertices[i].Color = FillColor;
            }
            Draw.SpriteBatch.End();
            if (Glitchy)
            {
                FillMask = EasyRendering.AddGlitch(FillMask, GlitchAmount / 4, GlitchAmp / 4);
            }
        }
        public void RenderCache(bool fg, Matrix matrix)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(fg ? FGCache : BGCache);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
            RenderTiles(fg, origLevelOffset - Position);
            Draw.SpriteBatch.End();
        }
        public void RenderTilesToTarget(bool fg, Matrix matrix)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(fg ? FGTarget : BGTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
            RenderTileCache(fg, matrix);
            if (ShineAmount > 0)
            {
                Draw.Rect(Vector2.Zero, FurthestBounds.Width, FurthestBounds.Height, Color.White * ShineAmount);
            }
            Draw.SpriteBatch.End();
            if (Glitchy)
            {
                if (fg)
                {
                    FGTarget = EasyRendering.AddGlitch(FGTarget, GlitchAmount, GlitchAmp);
                }
                else
                {
                    BGTarget = EasyRendering.AddGlitch(BGTarget, GlitchAmount, GlitchAmp);
                }
            }
        }
        public void RenderFillTarget(Matrix matrix)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(FillTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
            DrawFill(matrix);
            Draw.SpriteBatch.End();
        }
        public void RenderMaskTarget(Matrix matrix)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, EasyRendering.AlphaMaskBlendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
            Draw.SpriteBatch.Draw(Mask, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }
        public void RenderFillMaskTarget(Matrix matrix)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, EasyRendering.AlphaMaskBlendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
            Draw.SpriteBatch.Draw(FillMask, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }

        public static int[] GetIndices(EntityData data, bool grows)
        {
            if (grows) return new int[] { 0, 1, 2 };
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
        public void RenderTiles(bool fg, Vector2 offset)
        {
            TileGrid grid = fg ? FGTiles : BGTiles;
            if (grid is null || grid.Alpha <= 0f) return;

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
        public virtual void AfterRender(Level level)
        {

        }
        public Vector2 Offset;
        public void UpdateVertices()
        {
            if (Scene is not Level level) return;
            OnScreen = FollowCamera;
            Vector2 levelDifference = Position - origLevelOffset;
            if (Points.Length < 3) return;
            Vector2 center = ((Points[0] + Points[1] + Points[2]) / 3);
            for (int i = 0; i < Points.Length; i++)
            {

                Vector2 newPosition = Points[i] - levelDifference;
                if (!DisableBreathing)
                {
                    newPosition.Y -= VertexBreathing[i].Amount;
                }
                if (Grows || Shrinking)
                {
                    Vector2 dist = newPosition - center;
                    newPosition = (dist * Scale) + center;
                    newPosition.X -= FurthestBounds.Width * (1 - Scale);
                }
                FillVertices[i].Position = Vertices[i].Position = new Vector3(newPosition, 0);

                Vertices[i].Color = VerticeColor[i] * VerticeAlpha[i];
                FillVertices[i].Color = FillColor * FillAlpha;

                if (!OnScreen)
                {
                    OnScreen = level.Camera.GetBounds().Intersects(new Rectangle(FurthestBounds.X, FurthestBounds.Y, FurthestBounds.Width, FurthestBounds.Height));
                }
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
        public IEnumerator FadeAlphaTo(float alpha, float time, Ease.Easer ease)
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                Alpha = ease(i) * alpha;
                yield return null;
            }
        }
        public void OnTouched()
        {
            Add(new Coroutine(ShineOut()));
        }
        public void IntoPolyscreen()
        {
            Add(new Coroutine(ShineIn()));
        }
        public IEnumerator ShineOut()
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                ShineAmount = i;
                yield return null;
            }
            yield return 0.1f;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                SetVertexAlpha(1 - i);
                yield return null;
            }
            SetVertexAlpha(0);
            if (PolygonScreenIndex >= 0 && Scene is Level level && level.Tracker.GetEntity<PolygonScreen>() is PolygonScreen screen)
            {
                ExpandLinesOnScreen = true;
                screen.FadeInAt(PolygonScreenIndex);
            }
            while (ExpandLinesOnScreen)
            {
                yield return null;
            }
            RemoveSelf();
        }
        public IEnumerator ShineIn()
        {
            ShineAmount = 1;
            yield return null;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                SetVertexAlpha(i);
                yield return null;
            }
            SetVertexAlpha(1);
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                ShineAmount = 1 - i;
                yield return null;
            }
            ShineAmount = 0;
        }
        public void FadeConnectBegin(float time, float startDelay)
        {
            Add(new Coroutine(LinesRoutine(time, startDelay, true)));
        }
        private IEnumerator LinesRoutine(float time, float startDelay, bool start)
        {
            for (int i = 0; i < Points.Length; i += 3)
            {
                Add(new Coroutine(PrimitaveLineFade(i, time, startDelay, start)));
                yield return startDelay;
            }
        }
        private IEnumerator PrimitaveLineFade(int index, float time, float startDelay, bool start)
        {
            if (index + 2 >= Points.Length) yield break;
            float offset = -startDelay * 2;
            float[] mults = { 1, 0.5f, 0 };

            for (float j = offset; j < 1; j += Engine.DeltaTime / time)
            {
                for (int i = 0; i < 3; i++)
                {
                    Fading = true;
                    float amount = Ease.SineIn(j - (offset * mults[i]));
                    if (amount > 0.9f) amount = 1;
                    PointLineAlphas[index + i] = Calc.LerpClamp(0, 1, Calc.Clamp(amount, 0, 1));
                    PointLineAmount[index + i] = Calc.LerpClamp(0, 1, Calc.Clamp(amount, 0, 1));
                }
                yield return null;
            }
            for (int i = 0; i < 3; i++)
            {
                PointLineAlphas[index + i] = PointLineAmount[index + i] = 1;
            }
            LinesConnected = true;
            while (!WasFixed)
            {
                yield return null;
            }
            if (start)
            {
                for (float j = 0; j < 1; j += Engine.DeltaTime)
                {
                    Fading = true;
                    for (int i = 0; i < 3; i++)
                    {
                        VerticeAlpha[index + i] = j;
                    }
                    yield return null;
                }
            }
            for (float j = offset; j < 1; j += Engine.DeltaTime / (time / 2))
            {
                for (int i = 0; i < 3; i++)
                {
                    Fading = true;
                    float amount = Ease.CubeIn(j - (offset * mults[i]));
                    if (amount < 0.1f) amount = 0;
                    PointLineAlphas[index + i] = Calc.LerpClamp(1, 0, Calc.Clamp(amount, 0, 1));
                    PointLineAmount[index + i] = Calc.LerpClamp(1, 0, Calc.Clamp(amount, 0, 1));
                }
                yield return null;
            }
            for (int i = 0; i < 3; i++)
            {
                PointLineAlphas[index + i] = PointLineAmount[index + i] = 0;
            }
            Color to = Color.PaleGoldenrod;
            for (float j = 0; j < 1; j += Engine.DeltaTime / (time / 2))
            {
                for (int i = 0; i < 3; i++)
                {
                    VerticeColor[index + i] = Color.Lerp(Color.White, to, j);
                }
                yield return null;
            }
            yield return 0.3f;
            for (float j = 0; j < 1; j += Engine.DeltaTime / (time / 2))
            {
                for (int i = 0; i < 3; i++)
                {
                    VerticeColor[index + i] = Color.Lerp(to, Color.White, j);
                }
                yield return null;
            }
            Fading = false;
        }
        public void GetTiles(Scene scene)
        {
            Level level = scene as Level;
            int ox = (int)Math.Round((float)level.LevelSolidOffset.X);
            int oy = (int)Math.Round((float)level.LevelSolidOffset.Y);
            int tw = (int)Math.Ceiling(level.Bounds.Width / 8f);
            int th = (int)Math.Ceiling(level.Bounds.Height / 8f);
            if (FgFrom != FgTo)
            {
                bool allowAir = FgFrom == '0' || FgTo == '0';
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
                            newFgData[x4 - ox + 1, y3 - oy + 1] = c == FgFrom ? FgTo : c == FgTo && SwapFG ? FgFrom : c;
                        }
                    }
                }
                Autotiler.Generated newFgTiles = GFX.FGAutotiler.GenerateMap(newFgData, paddingIgnoreOutOfLevel: true);
                FGTiles = newFgTiles.TileGrid;
                FGTiles.VisualExtend = TileExtend;
                FGTiles.Visible = false;
            }

            if (BgTo != BgFrom)
            {
                bool allowAir = BgFrom == '0' || BgTo == '0';
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
                                    newBgData[cX, cY] = c == BgFrom ? BgTo : c == BgTo && SwapBG ? BgFrom : c;
                                }
                            }
                        }
                    }
                }
                Autotiler.Generated newBgTiles = GFX.BGAutotiler.GenerateMap(newBgData, paddingIgnoreOutOfLevel: true);
                BGTiles = newBgTiles.TileGrid;
                BGTiles.VisualExtend = TileExtend;
                BGTiles.Visible = false;
            }
        }


    }
}