using Celeste.Mod.Backdrops;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/DigitalGrid")]
    public class DigitalGrid : Backdrop
    {
        private float SpacingY;
        private float SpacingX;
        private bool MovingEnabled;
        private Vector2 Rate;
        private float renderOffsetY;
        private float renderOffsetX;
        private float DiagonalOffsetY;
        private float DiagonalOffsetX;
        private float LineWidth;
        private float LineHeight;
        private Color color;
        private bool VerticalLines;
        private bool HorizontalLines;
        private float seed;
        private float timer;
        private float amplitude;
        private float amount;
        private bool usesGlitch;
        private Vector2 compensation;
        private Effect effect;
        private float Opacity;

        private static VirtualRenderTarget _GridRenderTarget;

        public static VirtualRenderTarget GridRenderTarget => _GridRenderTarget ??=
                      VirtualContent.CreateRenderTarget("DigitalGrid", 320, 180);
        private static VirtualRenderTarget _DummyRenderTarget;

        public static VirtualRenderTarget DummyRenderTarget => _DummyRenderTarget ??=
                      VirtualContent.CreateRenderTarget("DigitalGrid", 320, 180);


        public override void Ended(Scene scene)
        {
            base.Ended(scene);
            _DummyRenderTarget?.Dispose();
            _DummyRenderTarget = null;
            _GridRenderTarget?.Dispose();
            _GridRenderTarget = null;
        }
        public DigitalGrid(BinaryPacker.Element data) : base()
        {
            Opacity = data.AttrFloat("Opacity", 1f);
            color = Calc.HexToColor(data.Attr("color", "00ff00")) * Opacity;
            amplitude = Calc.Random.Range(1f, 7f);
            amount = Calc.Random.Range(0.01f, 0.5f);
            usesGlitch = data.AttrBool("glitch", false);

            if (data.AttrBool("blur", true)) effect = GFX.FxGaussianBlur;

            LineWidth = data.AttrFloat("verticalLineWidth", 4);
            LineHeight = data.AttrFloat("horizontalLineHeight", 2);
            DiagonalOffsetY = data.AttrFloat("verticalLineAngle", 10);
            DiagonalOffsetX = data.AttrFloat("horizontalLineAngle", 10);
            SpacingX = data.AttrInt("xSpacing", 24);
            SpacingY = data.AttrInt("ySpacing", 24);
            MovingEnabled = data.AttrBool("moving", true);
            Rate = new Vector2(data.AttrFloat("rateX", 4), data.AttrFloat("rateY", 4));
            VerticalLines = data.AttrBool("verticalLines", true);
            HorizontalLines = data.AttrBool("horizontalLines", true);
            compensation = new Vector2(SpacingX * 4, SpacingY * 4);
            if (MovingEnabled)
            {
                renderOffsetY = Rate.Y;
                renderOffsetX = Rate.X;
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);
            timer += Engine.DeltaTime;
            if (scene.OnInterval(2 / 60f))
            {
                seed = Calc.Random.NextFloat();
            }
            if (scene.OnInterval(120 / 60f))
            {
                amplitude = Calc.Random.Range(1f, 7f);
                amount = Calc.Random.Range(0.01f, 0.5f);
            }
            if (MovingEnabled)
            {
                renderOffsetX += Rate.X * (Engine.DeltaTime * 2);
                renderOffsetX %= SpacingX;
                renderOffsetY += Rate.Y * (Engine.DeltaTime * 2);
                renderOffsetY %= SpacingY;
            }
        }
        public override void BeforeRender(Scene scene)
        {
            base.BeforeRender(scene);
        }
        public override void Render(Scene scene)
        {
            Level level = scene as Level;
            if (level.Session is null || level.Session.LevelData is null || (!HorizontalLines && !VerticalLines) || Opacity == 0)
            {
                base.Render(scene);
                return;
            }
            Vector2 cam = level.Camera.Position;
            Vector2 current = cam - compensation;
            GameplayRenderer.End();
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GridRenderTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            GameplayRenderer.Begin();
            if (HorizontalLines)
            {
                float startX = cam.X - compensation.X;
                float endX = cam.X + compensation.X + 320;
                for (float i = 0; i < 180 + compensation.Y + DiagonalOffsetY; i += SpacingY)
                {
                    Draw.Line(startX, current.Y + renderOffsetY, endX, current.Y + renderOffsetY - DiagonalOffsetX, color, LineHeight);
                    current.Y += SpacingY;

                }
            } //Draw to render Target
            if (VerticalLines)
            {
                float startY = cam.Y - compensation.Y;
                float endY = cam.Y + compensation.Y + 180;
                for (float i = 0; i < 320 + compensation.X + DiagonalOffsetX; i += SpacingX)
                {
                    Draw.Line(current.X + renderOffsetX, startY, current.X + renderOffsetX + DiagonalOffsetY, endY, color, LineWidth);
                    current.X += SpacingX;
                }
            }
            Draw.SpriteBatch.End();

            if (usesGlitch)
            {
                var glitchSave = Glitch.Value;
                Glitch.Value = amount;
                Glitch.Apply(GridRenderTarget, timer, seed, amplitude);
                Glitch.Value = glitchSave;
            }
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);

            Distort.Render(GridRenderTarget, (RenderTarget2D)GameplayBuffers.Displacement, true);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred,
                                   BlendState.AlphaBlend,
                                   SamplerState.PointWrap,
                                   DepthStencilState.None,
                                   RasterizerState.CullNone,
                                   effect,
                                   level.Camera.Matrix);
            Draw.SpriteBatch.Draw(GridRenderTarget, Vector2.Zero, Color.White); //Draw content of render Target to level
        }
    }
}

