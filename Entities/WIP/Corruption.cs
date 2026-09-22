using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using YamlDotNet.Core.Tokens;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [Tracked]
    internal class CorruptionRenderer : Entity
    {
        public static VirtualRenderTarget Target;
        public static VirtualRenderTarget AdditionalData;
        public static List<Corruption> CorruptionEntitiesOnScreen = [];
        [OnLoad]
        public static void Load()
        {
            On.Celeste.Glitch.Apply += Glitch_Apply;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.Glitch.Apply -= Glitch_Apply;
            Target?.Dispose();
            Target = null;
            AdditionalData?.Dispose();
            AdditionalData = null;
        }
        private static void Glitch_Apply(On.Celeste.Glitch.orig_Apply orig, VirtualRenderTarget source, float timer, float seed, float amplitude)
        {
            if (CorruptionEntitiesOnScreen == null || CorruptionEntitiesOnScreen.Count == 0 || Engine.Scene is not Level level) return;
            Target.SetAsTarget(true);
            Draw.SpriteBatch.Begin();
            foreach (Corruption c in CorruptionEntitiesOnScreen)
            {
                Draw.SpriteBatch.Draw(c.Texture, c.Position - level.Camera.Position + c.RenderOffset, null, Color.White, 0, Vector2.Zero, 1, c.SpriteEffects, 0);
            }
            Draw.SpriteBatch.End();
            VirtualRenderTarget polish = GameplayBuffers.TempB;
            polish.SetAsTarget(true);
            Draw.SpriteBatch.Begin();
            foreach (Corruption c in CorruptionEntitiesOnScreen)
            {
                if (c.LastTexture != null)
                {
                    Draw.SpriteBatch.Draw(c.LastTexture, c.Position - level.Camera.Position + c.ShakeVectorA, null, Color.Blue * c.LastTextureAlpha, 0, Vector2.Zero, 1, c.SpriteEffects, 0);
                    Draw.SpriteBatch.Draw(c.LastTexture, c.Position - level.Camera.Position + c.ShakeVectorB, null, Color.White * c.LastTextureAlpha * 0.5f, 0, Vector2.Zero, 1, c.SpriteEffects, 0);
                }
            }
            Draw.SpriteBatch.End();
            Effect effect = ShaderHelper.TryGetEffect("corruption");
            if (effect == null) return;
            Vector2 value = new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height);
            effect.Parameters["Dimensions"].SetValue(value);
            Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            Matrix halfPixelOffset = Matrix.Identity;
            effect.Parameters["TransformMatrix"]?.SetValue(halfPixelOffset * projection);
            effect.Parameters["ViewMatrix"]?.SetValue(Matrix.Identity);

            var t = Engine.Graphics.GraphicsDevice.Textures[1];
            var t2 = Engine.Graphics.GraphicsDevice.Textures[2];
            Engine.Graphics.GraphicsDevice.Textures[1] = Target;
            Engine.Graphics.GraphicsDevice.Textures[2] = polish;
            VirtualRenderTarget tempA = GameplayBuffers.TempA;

            tempA.SetAsTarget(true);
            Draw.SpriteBatch.StandardBegin(effect);
            Draw.SpriteBatch.Draw((RenderTarget2D)source, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();

            source.SetAsTarget(true);

            Draw.SpriteBatch.StandardBegin(effect);
            Draw.SpriteBatch.Draw((RenderTarget2D)tempA, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
            Engine.Graphics.GraphicsDevice.Textures[1] = t;
            Engine.Graphics.GraphicsDevice.Textures[2] = t2;
        }
        public float ProcessTime;
        public CorruptionRenderer() : base()
        {
            Tag |= Tags.Persistent | Tags.TransitionUpdate;
            Collider = new Hitbox(8, 8);
            Target = VirtualContent.CreateRenderTarget("CorruptionRenderer", 320, 180);
            AdditionalData = VirtualContent.CreateRenderTarget("AdditionalCorruptionRendererData", 320, 180);

        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            CorruptionEntitiesOnScreen.Clear();
            Target?.Dispose();
            Target = null;
            AdditionalData?.Dispose();
            AdditionalData = null;
        }
        public override void Update()
        {
            base.Update();
            CorruptionEntitiesOnScreen.Clear();
            if (Scene is not Level level || GameplayRenderer.RenderDebug || Engine.Commands.Open) return;
            Position = level.Camera.Position;
            foreach (Corruption c in Scene.Tracker.GetEntities<Corruption>())
            {
                if (c.IsVisible)
                {
                    CorruptionEntitiesOnScreen.Add(c);
                }
            }
        }
    }
    [CustomEntity("PuzzleIslandHelper/Corruption")]
    [Tracked]
    public class Corruption : Entity
    {
        public SpriteEffects SpriteEffects;
        public int MaxChunkSize;
        public FlagList ActiveFlag;
        public Color[] Grid;
        public int rows, columns;
        public bool OnScreen;
        public double ProcessTime;
        public Vector2 ShakeVectorA, ShakeVectorB;
        internal CorruptionRenderer Renderer;
        public Texture2D Texture, LastTexture;
        private float intervalOffset;
        public Vector2 RenderOffset;
        private bool valueBeforeFrameDelay;
        private int remainingFrames;
        public bool IsVisible
        {
            get
            {
                if (remainingFrames > 0)
                {
                    return valueBeforeFrameDelay && OnScreen;
                }
                else
                {
                    return ActiveFlag && OnScreen;
                }
            }
        }
        public Corruption(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Tag |= Tags.TransitionUpdate;
            Collider = new Hitbox(data.Width, data.Height);
            MaxChunkSize = data.Int("maxChunkSize");
            ActiveFlag = data.FlagList("flag");
            columns = data.Width;
            rows = data.Height;
            Grid = new Color[rows * columns];
            VisualUpdateDelay = data.Int("visualUpdateDelay", 1);
            Add(new PlayerCollider(p =>
            {
                if (ActiveFlag && remainingFrames == 0 && !p.Dead)
                {
                    p.Die(Vector2.Zero);
                }
            }));
        }
        private int rectOffsetX, rectOffsetY;
        private Color rectColor;
        private bool hollowRect;
        private bool drawHollowRectLine;
        private Vector2 hollowRectLineFlip;
        private int hollowRectLineXStart = 1;
        private int hollowRectLineXEnd = 0;
        private int hollowRectLineYStart;
        private int lightRadius;
        private float lightAlpha;
        public VertexLight Light;
        public override void Render()
        {
            if (IsVisible)
            {
                base.Render();
                if (drawRect)
                {
                    Vector2 p = new Vector2(X + rectOffsetX, Y + rectOffsetY);
                    if (hollowRect)
                    {
                        Draw.HollowRect(p, Width, Height, rectColor * 0.4f);
                        if (drawHollowRectLine)
                        {
                            Vector2 start = p + hollowRectLineFlip * Collider.Size, end = p + (Vector2.One - hollowRectLineFlip) * Collider.Size;
                            Draw.Line(start, end, rectColor);
                        }
                    }
                    else
                    {
                        Draw.Rect(p, Width, Height, rectColor * 0.4f);
                    }
                }
                if (LastTexture != null)
                {
                    Draw.SpriteBatch.Draw(LastTexture, Position + ShakeVectorB, Color.Blue * LastTextureAlpha);
                }
            }
        }
        public void AddOffset(float maxDist, float maxTime)
        {
            Vector2 offset = Calc.Random.Direction() * maxDist;
            RenderOffset += offset;
            Alarm.Set(this, Calc.Random.Range(0, maxTime), () =>
            {
                RenderOffset -= offset;
            });
        }
        public void AddBlueOffset(float angle, int left, int right, int top, int bottom)
        {
            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    byte offset = (byte)MathHelper.Clamp(angle * 255, Byte.MinValue, Byte.MaxValue);
                    int index = x + y * columns;
                    Grid[index].B += offset;
                    Alarm.Set(this, Calc.Random.Range(0.1f, 0.5f), () =>
                    {
                        Grid[index].B -= offset;
                    });
                }
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (OnScreen)
            {
                if (Texture != null)
                {
                    Draw.SpriteBatch.Draw(Texture, Position, Color.White);
                }
            }
        }
        public void CreateSwap(int size, int left, int right, int top, int bottom)
        {
            bool chunk1Found = FindChunk(Calc.Random.Range(left, right), Calc.Random.Range(top, bottom), size, out int r1, out int c1);
            bool chunk2Found = FindChunk(Calc.Random.Range(left, right), Calc.Random.Range(top, bottom), size, out int r2, out int c2);
            if (chunk1Found && chunk2Found)
            {
                Color color1 = new Color((c2 - c1), (r2 - r1), 0, 1);
                Color color2 = new Color((c1 - c2), (r1 - r2), 0, 1);
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        Grid[c1 + i + (r1 + j) * columns] = color1;
                        Grid[c2 + i + (r2 + j) * columns] = color2;
                    }
                }
            }
        }
        public void CreateSwap(int size)
        {
            bool chunk1Found = FindChunk(Calc.Random.Range(0, rows), Calc.Random.Range(0, columns), size, out int r1, out int c1);
            bool chunk2Found = FindChunk(Calc.Random.Range(0, rows), Calc.Random.Range(0, columns), size, out int r2, out int c2);
            if (chunk1Found && chunk2Found)
            {
                Color color1 = new Color((c2 - c1), (r2 - r1), 0, 1);
                Color color2 = new Color((c1 - c2), (r1 - r2), 0, 1);
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        Grid[c1 + i + (r1 + j) * columns] = color1;
                        Grid[c2 + i + (r2 + j) * columns] = color2;
                    }
                }
            }
        }
        public bool FindChunk(int startR, int startC, int chunkSize, out int r, out int c)
        {
            r = Math.Min(rows - chunkSize, startR);
            c = Math.Min(columns - chunkSize, startC);
            return true;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Renderer = PianoUtils.SeekController(scene, () => new CorruptionRenderer());
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            GenerateNewMap();
            stopwatch.Stop();
            ProcessTime = stopwatch.Elapsed.TotalSeconds;
            lightRadius = (int)Calc.Random.Range(0, Math.Max(Width, Height));
            lightAlpha = Calc.Random.NextFloat();
            Add(Light = new VertexLight(Color.White, 1, lightRadius, lightRadius * 2));
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            intervalOffset = Calc.Random.Range(0, 1f);
        }
        public void GenerateNewMap()
        {
            int chunkSize = MaxChunkSize;
            int maxChunks = (rows * columns) / MaxChunkSize;
            int minChunkSize = (int)Math.Max(1, (float)MaxChunkSize / 2);
            for (int i = 0; i < Grid.Length; i++)
            {
                Grid[i].R = 0;
                Grid[i].G = 0;
            }
            for (int i = 0; i < maxChunks; i++)
            {
                CreateSwap(Calc.Random.Range(minChunkSize, chunkSize));
            }
            for (int i = 0; i < Grid.Length; i++)
            {
                Color c = Grid[i];
                if (c.R == 0 && c.G == 0)
                {
                    c.R = (byte)MathHelper.Clamp(Calc.Random.Range(0.01f, 0.05f) * 255, Byte.MinValue, Byte.MaxValue);
                    c.G = (byte)MathHelper.Clamp(Calc.Random.Range(0.01f, 0.05f) * 255, Byte.MinValue, Byte.MaxValue);
                    Grid[i] = c;
                }
            }
            LastTexture = Texture;
            Texture2D t = new Texture2D(Engine.Graphics.GraphicsDevice, columns, rows);
            t.SetData(Grid);
            Texture = t;
        }
        public void GenerateNewMapArea(int left, int right, int top, int bottom)
        {
            int chunkSize = MaxChunkSize;
            int maxChunks = (rows * columns) / MaxChunkSize;
            int minChunkSize = (int)Math.Max(1, (float)MaxChunkSize / 2);
            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    Color c = Grid[x + y * columns];
                    Grid[x + y * columns] = new Color(0, 0, c.B, c.A);
                }
            }
            for (int i = 0; i < maxChunks; i++)
            {
                CreateSwap(Calc.Random.Range(minChunkSize, chunkSize));
            }
            for (int i = 0; i < Grid.Length; i++)
            {
                Color c = Grid[i];
                if (c.R == 0 && c.G == 0)
                {
                    c.R = (byte)MathHelper.Clamp(Calc.Random.Range(0.01f, 0.05f) * 255, Byte.MinValue, Byte.MaxValue);
                    c.G = (byte)MathHelper.Clamp(Calc.Random.Range(0.01f, 0.05f) * 255, Byte.MinValue, Byte.MaxValue);
                    Grid[i] = c;
                }
            }
            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    Color c = Grid[x + y * columns];
                    if (c.R == 0 && c.G == 0)
                    {
                        c.R = (byte)MathHelper.Clamp(Calc.Random.Range(0.01f, 0.05f) * 255, Byte.MinValue, Byte.MaxValue);
                        c.G = (byte)MathHelper.Clamp(Calc.Random.Range(0.01f, 0.05f) * 255, Byte.MinValue, Byte.MaxValue);
                        Grid[x + y * columns] = c;
                    }
                }
            }
            LastTexture = Texture;
            Texture2D t = new Texture2D(Engine.Graphics.GraphicsDevice, columns, rows);
            t.SetData(Grid);
            Texture = t;
        }
        private Tween t;
        public float LastTextureAlpha = 0;
        private bool flagWasActive;
        public int VisualUpdateDelay;
        private Alarm flipAlarm;
        public override void Update()
        {
            base.Update();
            bool isActive = ActiveFlag;
            if (remainingFrames > 0)
            {
                remainingFrames--;
            }
            if (flagWasActive != isActive)
            {
                remainingFrames = VisualUpdateDelay;
                valueBeforeFrameDelay = isActive;
            }
            ShakeVectorA = Calc.Random.ShakeVector() * 2;
            ShakeVectorB = Calc.Random.ShakeVector();
            OnScreen = CullHelper.IsRectangleVisible(X, Y, Width, Height, 0);
            if (IsVisible && Scene.OnInterval(Calc.Random.Range(0.05f, 0.2f), intervalOffset))
            {
                GenerateNewMap();
                Tween.Set(this, Tween.TweenMode.Oneshot, 0.2f, Ease.SineOut, t =>
                {
                    LastTextureAlpha = (1 - t.Eased) * 0.8f;
                });
                if (Calc.Random.Chance(0.3f))
                {
                    Light.Alpha = Calc.Random.NextFloat();
                    int r = Calc.Random.Range(0, (int)Math.Max(Width, Height));
                    Light.StartRadius = r;
                    Light.EndRadius = r * 2;
                }
                if (Calc.Random.Chance(0.18f))
                {
                    int left = Calc.Random.Range(0, columns - 1);
                    int right = left + Calc.Random.Range(1, columns - left);
                    int top = Calc.Random.Range(0, rows - 1);
                    int bottom = top + Calc.Random.Range(1, rows - top);
                    AddBlueOffset(Calc.Random.Range(0, 1f), left, right, top, bottom);
                }
                if (Calc.Random.Chance(0.05f))
                {
                    AddOffset(3, 0.3f);
                }
                if (Calc.Random.Chance(0.1f))
                {
                    SpriteEffects = Calc.Random.Choose(SpriteEffects.None, SpriteEffects.FlipHorizontally, SpriteEffects.FlipVertically, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically);
                }
                drawRect = Calc.Random.Chance(0.2f);
                drawHollowRectLine = Calc.Random.Chance(0.1f);

                if (drawRect)
                {
                    rectOffsetX = Calc.Random.Range(-1, 2);
                    rectOffsetY = Calc.Random.Range(-1, 2);
                    rectColor = PianoUtils.RandomColor(min: 100, max: 200) * Calc.Random.Range(0.3f, 0.7f);
                    hollowRect = Calc.Random.Chance(0.5f);
                }
            }
            flagWasActive = isActive;

        }
        private bool drawRect;
    }
}