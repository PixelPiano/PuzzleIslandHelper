using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Tower;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
// PuzzleIslandHelper.CutsceneHeart
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TBlock3D")]
    [Tracked]
    internal class TBlock3D : Model3D
    {
        private class extend
        {
            public int CellX, CellY;
            public bool Left, Right, Top, Bottom;
            public extend(int cellX, int cellY, bool l = false, bool r = false, bool t = false, bool b = false)
            {
                Left = l;
                Right = r;
                Top = t;
                Bottom = b;
            }
            public void DrawOutline(int x, int y, int stroke, Color color)
            {
                bool drawLeft = !Left;
                bool drawRight = !Right;
                bool drawUp = !Top;
                bool drawDown = !Bottom;
                bool tlDrwn = false, trDrwn = false, blDrwn = false, brDrwn = false;
                int left = x;
                int right = x + 32;
                int top = y;
                int bottom = y + 32;
                if (drawLeft) //draw left side
                {
                    Draw.Rect(left, top, stroke, 32, color);
                }
                else
                {
                    if (!drawUp)
                    {
                        Draw.Rect(left, top, stroke, stroke, color);
                        tlDrwn = true;
                    }
                    if (!drawDown)
                    {
                        Draw.Rect(left, bottom - stroke, stroke, stroke, color);
                        blDrwn = true;
                    }
                }
                if (drawRight)
                {
                    Draw.Rect(right - stroke, top, stroke, 32, color);
                }
                else
                {
                    if (!drawUp)
                    {
                        Draw.Rect(right - stroke, top, stroke, stroke, color);
                        trDrwn = true;
                    }
                    if (!drawDown)
                    {
                        Draw.Rect(right - stroke, bottom - stroke, stroke, stroke, color);
                        brDrwn = true;
                    }
                }
                if (drawUp)
                {
                    int xo = 0, wo = 0;
                    if (tlDrwn)
                    {
                        xo = stroke;
                        wo = stroke;
                    }
                    if (trDrwn) wo += stroke;
                    Draw.Rect(left + xo, top, 32 - wo, stroke, color);
                }
                else
                {
                    if (!drawLeft && !tlDrwn)
                    {
                        Draw.Rect(left, top, stroke, stroke, color);
                    }
                    if (!drawRight && !trDrwn)
                    {
                        Draw.Rect(right - stroke, top, stroke, stroke, color);
                    }
                }
                if (drawDown)
                {
                    int xo = 0, wo = 0;
                    if (blDrwn)
                    {
                        xo = stroke;
                        wo = stroke;
                    }
                    if (brDrwn) wo += stroke;
                    Draw.Rect(left + xo, bottom - stroke, 32 - wo, stroke, color);
                }
                else
                {
                    if (!drawLeft && !blDrwn)
                    {
                        Draw.Rect(left, bottom - stroke, stroke, stroke, color);
                    }
                    if (!drawRight && !brDrwn)
                    {
                        Draw.Rect(right - stroke, bottom - stroke, stroke, stroke, color);
                    }
                }
            }
        }
        private VirtualRenderTarget outputTarget, outlineTarget, gridTarget, whiteOutTarget;

        private bool redrawOutline = true, redrawGrids = true;
        public bool ReapplyTargets = true;
        private class TileGridCollection : VirtualMap<TileGrid>
        {
            public char TileType;
            public TileGridCollection(char tileType, int columns, int rows) : base(columns, rows, null)
            {
                TileType = tileType;
            }
            public void Generate(Level level, Entity entity, int x1, int y1)
            {
                VirtualMap<char> solidsData2 = new VirtualMap<char>(level.SolidsData.Columns, level.SolidsData.Rows, '0');
                List<(int x, int y)> toChange = [];
                for (int x = 0; x < EdgeMap.Columns; x++)
                {
                    for (int y = 0; y < EdgeMap.Rows; y++)
                    {
                        extend e = EdgeMap[x, y];
                        if (e == null) continue;
                        TileGrid grid = null;
                        if (e.Left || e.Right || e.Top || e.Bottom)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                for (int i = 0; i < 4 + j; i++)
                                {
                                    if (e.Left) toChange.Add((x1 - j, y1 + i - j));
                                    if (e.Right) toChange.Add((x1 + 3 + j, y1 + i - j));
                                    if (e.Top) toChange.Add((x1 + i - j, y1 - j));
                                    if (e.Bottom) toChange.Add((x1 + i - j, y1 + 3 + j));
                                }
                            }
                            foreach (var pair in toChange)
                            {
                                solidsData2[pair.x, pair.y] = TileType;
                            }
                            grid = GFX.FGAutotiler.GenerateOverlay(TileType, x1, y1, 4, 4, solidsData2).TileGrid;
                            foreach (var pair in toChange)
                            {
                                solidsData2[pair.x, pair.y] = '0';
                            }
                            toChange.Clear();
                        }
                        else
                        {
                            grid = GFX.FGAutotiler.GenerateBox(TileType, 4, 4).TileGrid;
                        }
                        if (grid != null)
                        {
                            entity.Add(grid);
                            this[x, y] = grid;
                            grid.Visible = false;
                        }
                    }
                }
            }
            public void Render(Rectangle clip)
            {
                for (int x = 0; x < Columns; x++)
                {
                    for (int y = 0; y < Rows; y++)
                    {
                        orig_get_Item(x, y)?.RenderAt(new Vector2(x, y) * 32, clip);
                    }
                }
            }
        }
        private List<TileGridCollection> Grids = [];
        private static VirtualMap<extend> EdgeMap;
        private Wiggler wiggler;
        public BetterShaker Shaker;
        private Coroutine joltCoroutine;
        private char tileType = '3';
        private float yOffset;
        private float sinMult = 0.7f;
        private float timer;
        private Vector2 shakeOffset;
        private Rectangle clip;
        public float BaseShakeMult, JoltShakeMult, JoltShakeMax = 2;
        public float DetailAlphaMult = 1;//0.25f;
        public Color WireFrameColor;
        public Color GlowColor;
        public float GlowAmount; //make glow sprites
        public float WaveAmount;
        public bool BeamActive;
        public bool OnlyRenderTarget;
        public float Flash;
        public int OutlineStroke
        {
            get => stroke;
            set
            {
                stroke = value;
                redrawOutline = true;
                ReapplyTargets = true;
            }
        }
        private int stroke;
        private string tileTypes;
        public float StartYaw = MathHelper.Pi;
        public float StartPitch;
        public float StartRoll;
        public Vector2 Offset => shakeOffset * BaseShakeMult + shakeOffset * JoltShakeMax * JoltShakeMult + Vector2.UnitY * yOffset;
        public TBlock3D(EntityData data, Vector2 offset) : this(data.Position + offset)
        {
            tileType = data.Char("typetype", '3');
            tileTypes = tileType + data.Attr("tilesets");
        }
        public TBlock3D(Vector2 position) : base("Models/PuzzleIslandHelper/TBlock", position, null, Vector2.One * 32)
        {
            Add(new BeforeRenderHook(BeforeRender));
            int w = 6 * 32, h = 6 * 32;
            outputTarget = VirtualContent.CreateRenderTarget("tblock final target", w, h);
            gridTarget = VirtualContent.CreateRenderTarget("tblock grid target", w, h);
            outlineTarget = VirtualContent.CreateRenderTarget("tblock outline target", w, h);
            whiteOutTarget = VirtualContent.CreateRenderTarget("tblock whiteOut target", w, h);
            Yaw = StartYaw;
            Roll = StartRoll;
            Pitch = StartPitch;
            //Yaw is horizontal 
            //Pitch is rotation
            //Roll is vertical
            KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.D1, Reset);
            KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.D2, Spin);
            Collider = new Hitbox(64, 96);
            JustifyOrigin(0.5f, 0.5f);
            Add(wiggler = Wiggler.Create(0.5f, 4f, (f) => Scale = Vector3.One * (32f + f * 2)));
            Add(Shaker = new BetterShaker(OnShake));
            Add(joltCoroutine = new Coroutine(false));
           // Add(new AfterRenderHook(() => Shape.Position -= new Vector3(Offset, 0)));
        }
        public override Matrix GetMatrix()
        {
            return base.GetMatrix() * Matrix.CreateTranslation(Offset.X, -Offset.Y, 0);
        }
        public void OnShake(Vector2 amount)
        {
            shakeOffset += amount;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level level = scene as Level;
            Rectangle tileBounds = level.Session.MapData.TileBounds;
            int x1 = (int)(X / 8f) - tileBounds.Left;
            int y1 = (int)(Y / 8f) - tileBounds.Top;
            clip = new Rectangle(0, 0, 4, 4);
            foreach (char c in tileTypes)
            {
                TileGridCollection collection = new TileGridCollection(c, EdgeMap.Columns, EdgeMap.Rows);
                Grids.Add(collection);
                collection.Generate(level, this, x1, y1);
            }
            redrawOutline = redrawGrids = ReapplyTargets = true;
        }
        public int GridIndex
        {
            get => gridIndex;
            set
            {
                if (gridIndex != value)
                {
                    redrawGrids = true;
                }
                gridIndex = value;
            }
        }
        private int gridIndex;
        public void RerollActiveTileType()
        {
            int prevIndex = GridIndex;
            while (GridIndex == prevIndex)
            {
                GridIndex = Calc.Random.Range(0, Grids.Count);
            }
        }
        public void BeforeRender()
        {
            if (redrawOutline)
            {
                outlineTarget.SetAsTarget(true);
                if (OutlineStroke > 0)
                {
                    Draw.SpriteBatch.Begin();
                    for (int i = 0; i < EdgeMap.Rows; i++)
                    {
                        for (int j = 0; j < EdgeMap.Columns; j++)
                        {
                            EdgeMap[j, i]?.DrawOutline(j * 32, i * 32, OutlineStroke, Color.Red);
                        }
                    }
                    Draw.SpriteBatch.End();
                }
                redrawOutline = false;
            }
            if (redrawGrids)
            {
                gridTarget.SetAsTarget(true);
                Draw.SpriteBatch.Begin();
                if (Grids != null)
                {
                    Grids[GridIndex].Render(clip);
                }
                Draw.SpriteBatch.End();
                redrawGrids = false;
            }
            if (ReapplyTargets)
            {
                outputTarget.SetAsTarget(true);
                Draw.SpriteBatch.Begin();
                if (Flash < 1)
                {
                    Draw.SpriteBatch.Draw(gridTarget, Vector2.Zero, Color.White);
                    Draw.SpriteBatch.Draw(outlineTarget, Vector2.Zero, Color.White);
                }
                if (Flash > 0) Draw.Rect(Vector2.Zero, gridTarget.Width, gridTarget.Height, Color.White * Flash);
                Draw.SpriteBatch.End();
                Texture = outputTarget.Target;
                ReapplyTargets = false;
            }
        }
        private float wobbleRate;
        public override void Update()
        {
            base.Update();
            float sin = (float)Math.Sin(timer * sinMult);
            yOffset = sin * 8;
            timer += Engine.DeltaTime;
            if (rerollTimer > 0)
            {
                rerollTimer -= Engine.DeltaTime;
                if (rerollTimer <= 0)
                {
                    rerollTimer = 0;
                    if (rerollsLeft > 0)
                    {
                        rerollTimer = 0.1f;
                        rerollsLeft--;
                        if (rerollsLeft == 0)
                        {
                            GridIndex = 0;
                        }
                        else
                        {
                            RerollActiveTileType();
                        }
                    }
                }
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            outputTarget?.Dispose();
            gridTarget?.Dispose();
            outlineTarget?.Dispose();
            whiteOutTarget?.Dispose();
            outlineTarget = null;
            outputTarget = null;
            gridTarget = null;
            whiteOutTarget = null;
        }
        public void Reset()
        {
            Components.RemoveAll<Coroutine>();
            wiggler.Stop();
            Yaw = StartYaw;
            Roll = StartRoll;
            Pitch = StartPitch;
            sinMult = 1;
        }
        public void Spin()
        {
            Components.RemoveAll<Coroutine>();
            wiggler.Start();
            Add(new Coroutine(spinRoutine(30f, 10f, 0f, MathHelper.Pi + MathHelper.PiOver2, -MathHelper.PiOver4)));
            Tween t = Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.ExpoOut, t =>
            {
                Pitch = Calc.LerpClamp(0, -MathHelper.PiOver4, t.Eased) + StartPitch;
            });
        }
        private IEnumerator spinRoutine(float targetSpeed, float approach, float wait, float endYaw, float pitchOffset)
        {
            Tween t = Tween.Set(this, Tween.TweenMode.Oneshot, 1, Ease.ExpoOut, t =>
            {
                Pitch = Calc.LerpClamp(0, pitchOffset, t.Eased) + StartPitch;
            });
            float speed = 0;
            while (speed != targetSpeed)
            {
                speed = Calc.Approach(speed, targetSpeed, approach * Engine.DeltaTime);
                Yaw += speed * Engine.DeltaTime;
                yield return null;
            }
            speed = targetSpeed;
            int stepsToEnd = 0;
            float yaw = Yaw;
            float prevSpeed = speed;
            float dec = 0;
            float deltaTime = Engine.DeltaTime;
            while (speed > 0)
            {
                yaw += speed * Engine.DeltaTime;
                dec = Calc.Approach(dec, 1, deltaTime);
                speed = Calc.Approach(speed, 0, dec * approach * deltaTime);
                stepsToEnd++;
            }
            yaw %= MathHelper.TwoPi;
            endYaw %= MathHelper.TwoPi;
            float diff = endYaw - yaw;
            float variable = diff / stepsToEnd;

            speed = prevSpeed;
            dec = 0;
            for (int i = 0; i < stepsToEnd; i++)
            {
                Yaw += (speed + variable) * deltaTime;
                dec = Calc.Approach(dec, 1, deltaTime);
                speed = Calc.Approach(speed, 0, dec * approach * deltaTime);
                yield return null;
            }
            Yaw = MathHelper.TwoPi - endYaw;

            yield return 0.7f;
            BaseShakeMult = 0;
            JoltShakeMult = 0;
            Shaker.ShakeFor();
            //glow radius grows as shaking intensifies
            float timer = 2;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 3)
            {
                float ease = Ease.SineIn(i);
                BaseShakeMult = ease * 4;
                GlowAmount = ease;
                timer -= Engine.DeltaTime;
                if (timer < 0)
                {
                    DetailAlphaMult = Math.Min(1, DetailAlphaMult + 0.25f);
                    joltCoroutine.Replace(JoltRoutine());
                    timer = 1f;
                }
                yield return null;
            }
            rerollTimer = 0;
            rerollsLeft = 0;
            GridIndex = 0;
            Shaker.StopShaking();
            BaseShakeMult = 0;
            DetailAlphaMult = 1;
            //white flash
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
            {
                Flash = i;
                yield return null;
            }
            Flash = 1;
            yield return null;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.1f)
            {
                Flash = 1 - i;
                yield return null;
            }
            Flash = 0;
            //tile shift
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.4f)
            {
                WaveAmount = Ease.QuintInOut(i);
                yield return null;
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.4f)
            {
                Pitch = Calc.LerpClamp(pitchOffset, 0, Ease.CubeOut(i)) + StartPitch;
                sinMult = Calc.LerpClamp(1, 0, i);
                BaseShakeMult = i;
                yield return null;
            }
            ActivateBeam();
            Pitch = StartPitch;
            BaseShakeMult = 2;
            Shaker.ShakeFor();
            yield return null;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                BaseShakeMult = 2 * Ease.SineInOut(1 - i);
                yield return null;
            }
            Shaker.StopShaking();
        }
        public void ActivateBeam()
        {
            BeamActive = true;
            Entity placeholder = new();
            placeholder.Add(new Image(GFX.Game["objects/PuzzleIslandHelper/wipLaserTexture"]));
            placeholder.Position = Center + Vector2.UnitX * (aim * Width / 2 + 32);
            Scene.Add(placeholder);
            Alarm.Set(this, 1.2f, placeholder.RemoveSelf);
        }
        private int aim = -1;
        private float rerollTimer;
        private int rerollsLeft;
        private IEnumerator JoltRoutine()
        {
            JoltShakeMult = 0;
            DetailAlphaMult = 0;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.3f)
            {
                JoltShakeMult = 2 * Ease.CubeOut(i);
                DetailAlphaMult = Ease.CubeOut(i);
                yield return null;
            }
            Tween.Set(this, Tween.TweenMode.Oneshot, 0.3f, Ease.SineIn, t =>
            {
                OutlineStroke = (int)(3 * t.Eased);
            }, t =>
            {
                Tween.Set(this, Tween.TweenMode.Oneshot, 0.4f, Ease.SineInOut, t =>
                {
                    OutlineStroke = (int)(3 * (1 - t.Eased));
                });
            });
            rerollTimer = 0.1f;
            rerollsLeft = 7;
            JoltShakeMult = 2;
            DetailAlphaMult = 1;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.7f)
            {
                JoltShakeMult = Calc.LerpClamp(2, 0, Ease.SineIn(i));
                DetailAlphaMult = Math.Max(0, 1 - Ease.SineIn(i) * 3);
                yield return null;
            }
            JoltShakeMult = 0;
            DetailAlphaMult = 0;
            yield return null;
            rerollTimer = 0;
            rerollsLeft = 0;
            GridIndex = 0;
        }
        private IEnumerator rubberbandYawTo(float spins, float slowAt = -1, float slowMult = 4)
        {
            double factor = 0.0099999997764825821;
            //0 = facing away
            //6 = full rotation
            float target = spins * MathHelper.TwoPi;
            float result = Yaw.RubberbandApproach(target, 0.01f);
            while (result != target)
            {
                Yaw = result;
                yield return null;
                if (slowAt > 0 && result - slowAt <= 0)
                {
                    factor = 0.0099999997764825821 * (1 + (1 - (result / slowAt)) * slowMult);
                }
                result = Yaw.RubberbandApproach(target, 0.01f, factor);
            }
            Yaw = target;
            yield return null;
        }
        [OnLoad]
        public static void Load2()
        {
            /*            EdgeMap = new VirtualMap<extend>(4, 6);
                        for (int i = 0; i < 6; i++)
                        {
                            EdgeMap[0, i] = new();
                        }
                        EdgeMap[1, 0] = new();
                        EdgeMap[1, 1] = new(d: true);
                        EdgeMap[1, 2] = new(u: true, d: true);
                        EdgeMap[1, 3] = new(u: true);

                        EdgeMap[2, 1] = new(r: true);
                        EdgeMap[3, 0] = new(d: true);
                        EdgeMap[3, 1] = new(l: true, u: true, d: true);
                        EdgeMap[3, 2] = new(u: true);

                        EdgeMap[3, 4] = new(l: true);
                        EdgeMap[2, 3] = new(d: true);
                        EdgeMap[2, 4] = new(r: true, u: true, d: true);
                        EdgeMap[2, 5] = new(u: true);*/


            EdgeMap = new VirtualMap<extend>(4, 6);
            for (int i = 0; i < 6; i++)
            {
                EdgeMap[0, i] = new(0, i);
            }
            EdgeMap[1, 5] = new(1, 5);
            EdgeMap[1, 4] = new(1, 4, t: true);
            EdgeMap[1, 3] = new(1, 3, t: true, b: true);
            EdgeMap[1, 2] = new(1, 2, b: true);

            EdgeMap[3, 5] = new(3, 5, t: true);
            EdgeMap[3, 4] = new(3, 4, l: true, t: true, b: true);
            EdgeMap[3, 3] = new(3, 3, b: true);
            EdgeMap[2, 4] = new(2, 4, r: true);

            EdgeMap[2, 0] = new(2, 0, b: true);
            EdgeMap[2, 1] = new(2, 1, r: true, t: true, b: true);
            EdgeMap[2, 2] = new(2, 2, t: true);
            EdgeMap[3, 1] = new(3, 1, l: true);

        }
        [OnUnload]
        public static void Unload2()
        {
            EdgeMap = null;
        }
    }
}

