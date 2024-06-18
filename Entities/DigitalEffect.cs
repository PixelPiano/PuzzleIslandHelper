using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DigitalEffect")]
    [Tracked]

    public class DigitalEffect : Entity
    {

        public class GlitchTexture : Image
        {
            public const string DefaultPath = "objects/PuzzleIslandHelper/digitalEffect/artifacts/";

            public class Timer
            {
                public float Min;
                public float Range;
                private float time;
                private float timer;
                public Action OnComplete;

                public Timer(float min, float range, Action onComplete)
                {
                    Min = min;
                    Range = range;
                    OnComplete = onComplete;
                }
                public void Update()
                {
                    if (timer < time)
                    {
                        timer += Engine.DeltaTime;
                    }
                    else
                    {
                        OnComplete.Invoke();
                        timer = 0;
                        time = Min + Calc.Random.Range(0, Range);
                    }
                }
            }
            public List<Timer> Timers = new();

            public float WaitRange = 1.0f;
            public float ColorIntervalRange = 0.2f;
            public float VariantCycleRange = 4;
            public float ScaleRange = 0.5f;
            public float PositionRange = 0.5f;

            private float scaleTime;
            private float waitTime;
            private float colorTime;
            private float variantCycles;

            private float scaleTimer;
            private float waitTimer;
            private float colorTimer;
            private int cycle;

            private const int MaxFrames = 21;
            public const float WaitMin = 0.5f;
            public const float ColorMin = 0.1f;
            public const int CycleMin = 1;
            public const float ScaleMin = 0.5f;
            private Vector2 offset;
            public string[] Variants = new string[] { "", "Blur", "Drunk", "Rock" };
            public int Variant;
            public GlitchTexture() : base(GFX.Game[DefaultPath + "artifact00"], true)
            {
                Timers = new()
                {
                    new Timer(0.5f, 1, RandomizeTexture), //image
                    new Timer(0.1f,0.2f,RandomizeColor), //Color
                    new Timer(0.2f, 0.1f, RandomizeScale), //Scale
                    new Timer(0.1f, 1f, RandomizeOffset) //position
                };
            }
            public override void Update()
            {
                base.Update();
                foreach (Timer t in Timers)
                {
                    t.Update();
                }
            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                RandomizeTexture();
                CenterOrigin();
                Position += new Vector2(Width / 2, Height / 2);
            }

            public override void Render()
            {
                if (Scene is not Level level || level.GetPlayer() is not Player player) return;
                if (Texture != null)
                {
                    Texture.Draw(player.Position - Vector2.UnitY * Texture.Height / 2 + offset, Origin, Color, Scale, Rotation, Effects);
                }
            }
            private void RandomizeScale()
            {
                Scale = Vector2.One * (ScaleMin + Calc.Random.Range(0, ScaleRange));
            }
            private void RandomizeOffset()
            {
                Vector2 halfSize = new Vector2(Width, Height) / 2;
                offset = Calc.Random.Range(-halfSize, halfSize);
            }
            private void RandomizeTexture()
            {
                if (cycle >= variantCycles)
                {
                    Variant = Calc.Random.Range(0, 4);
                    variantCycles = CycleMin + Calc.Random.Range(0, variantCycles);
                    cycle = 0;
                }
                else
                {
                    cycle++;
                }
                int frame = Calc.Random.Range(0, MaxFrames);
                string path = DefaultPath + "artifact" + Variants[Variant] + (frame < 10 ? "0" + frame : frame);
                Texture = GFX.Game[path];
                Rotation = Calc.Random.Range(0, 360f).ToRad();
            }
            private void RandomizeColor()
            {
                Color = Color.Random(true, true, true, false);
            }

        }
        public static bool IgnoreHair;
        private Color background;

        private Color[] lineColors = new Color[4];

        private float yOffset;

        private bool backFlicker;

        private bool lineFlicker;
        private bool started;

        private float currentLineOpacity = 1;

        private float currentBackOpacity = 1;
        public static bool ForceStop;

        private GlitchTexture[] Glitches = new GlitchTexture[3];

        private static VirtualRenderTarget _MaskRenderTarget;
        private static VirtualRenderTarget _ObjectRenderTarget;

        public static VirtualRenderTarget MaskRenderTarget => _MaskRenderTarget ??=
                      VirtualContent.CreateRenderTarget("PlayerDigital", 320, 180);

        public static VirtualRenderTarget ObjectRenderTarget => _ObjectRenderTarget ??=
                      VirtualContent.CreateRenderTarget("DigitalObject", 320, 180);


        private string flag;
        private bool inverted;
        public bool State
        {
            get
            {
                if(Scene is not Level level || string.IsNullOrEmpty(flag)) return true;
                return level.Session.GetFlag(flag) == !inverted;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            UseEffect = false;
            _MaskRenderTarget?.Dispose();
            _MaskRenderTarget = null;
            _ObjectRenderTarget?.Dispose();
            _ObjectRenderTarget = null;
        }

        public static readonly BlendState AlphaMaskBlendState = new()
        {
            ColorSourceBlend = Blend.Zero,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.SourceColor,
            AlphaSourceBlend = Blend.Zero,
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.SourceColor
        };
        public static bool UseEffect;
        public DigitalEffect(EntityData data, Vector2 offset)
          : base(data.Position + offset)
        {
            Tag |= Tags.TransitionUpdate;
            flag = data.Attr("flag");
            inverted = data.Bool("inverted");
            //IgnoreHair = data.Bool("leaveOutHair", true);
            background = Calc.HexToColor("008801");
            lineColors[0] = Calc.HexToColor("00FF00");
            lineColors[1] = Calc.HexToColor("00E800");
            lineColors[2] = Calc.HexToColor("00FF00");
            lineColors[3] = Calc.HexToColor("07ED07");
            backFlicker = data.Bool("backgroundFlicker", true);
            lineFlicker = data.Bool("lineFlicker", true);
            Depth = -1;

            //Add(new BeforeRenderHook(BeforeRender));

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            UseEffect = true;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (scene.GetPlayer() is not Player player) return;
            Start(player);
        }
        public void Start(Player player)
        {
            started = true;
            IgnoreHair = true;
            Collider = new Hitbox(player.Width + 40, player.Height + 32, -(player.Width / 2) - (player.Facing == Facings.Right ? 19 : 18), player.Height - 22);
            for (int i = 0; i < Glitches.Length; i++)
            {
                Glitches[i] = new GlitchTexture()
                {
                    ColorIntervalRange = 0.4f * (i + 1),
                    WaitRange = 0.2f * (i + 1),
                    VariantCycleRange = i + 1,
                    Position = player.Center - Center,
                    Visible = false
                };
            }
            Add(Glitches);
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            DrawTextures();
        }
        public void DrawRect()
        {
            Draw.Rect(Collider, background);
        }
        public void DrawTextures()
        {
            foreach (GlitchTexture tex in Glitches)
            {
                tex.Render();
            }
        }
        public void DrawLines()
        {
            for (int i = 0; i < 2; i++)
            {
                float y = Collider.AbsoluteY + Height - (Height / 4 * i) + yOffset - (Height / 2);
                Draw.Line(new Vector2(Collider.AbsoluteX, y), new Vector2(Collider.AbsoluteX + Width, y), lineColors[i] * currentLineOpacity);
            }
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level || ForceStop || !State || level.GetPlayer() is not Player player || !player.Visible) return;
            Draw.SpriteBatch.End();

            Engine.Graphics.GraphicsDevice.SetRenderTarget(MaskRenderTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            GameplayRenderer.Begin();
            Color haircolor = player.Hair.Color;
            if (IgnoreHair)
            {
                player.Hair.Visible = false;
            }
            player.Render();
            if (IgnoreHair)
            {
                player.Hair.Visible = true;
            }
            Draw.SpriteBatch.End();
            Engine.Graphics.GraphicsDevice.SetRenderTarget(ObjectRenderTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

            GameplayRenderer.Begin();
            DrawRect();
            DrawLines();
            DrawTextures();

            Draw.SpriteBatch.End();
            Engine.Graphics.GraphicsDevice.SetRenderTarget(ObjectRenderTarget);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, AlphaMaskBlendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
            Draw.SpriteBatch.Draw(MaskRenderTarget, Vector2.Zero, Color.White);
            GameplayRenderer.End();
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
            GameplayRenderer.Begin();
            Draw.SpriteBatch.Draw(ObjectRenderTarget, level.Camera.Position, Color.White);

        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            if (!started)
            {
                Start(player);
            }
            Position = player.Position;
            Collider.Position = new Vector2(-(player.Width / 2) - (player.Facing == Facings.Right ? 19 : 18), -player.Height - 22);
            currentLineOpacity = lineFlicker && Calc.Random.Range(0, 2) == 0 ? Calc.Random.Range(0, 2) == 0 ? 0.2f : 0.5f : 1;
            currentBackOpacity = backFlicker && Calc.Random.Range(0, 2) == 0 ? Calc.Random.Range(0, 2) == 0 ? 0.8f : 0.9f : 1;
            background = Color.Lerp(Color.LightGreen, Calc.HexToColor("008801"), currentBackOpacity);
            yOffset = (yOffset + 0.2f) % (Height / 4);
        }
        public static void Unload()
        {
            UseEffect = false;
            _MaskRenderTarget?.Dispose();
            _ObjectRenderTarget?.Dispose();
            On.Celeste.PlayerHair.Render -= PlayerHair_Render;
        }
        public static void Load()
        {
            IgnoreHair = false;
            On.Celeste.PlayerHair.Render += PlayerHair_Render;
        }
        private static void PlayerHair_Render(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self)
        {

            if ((IgnoreHair && UseEffect) || !UseEffect)
            {
                orig(self);
            }
        }
    }
}
