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
        private float currentY;
        private float currentX;
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
            timer = 0;
            seed = 0;
            amplitude = Calc.Random.Range(1f, 7f);
            amount = Calc.Random.Range(0.01f, 0.5f);
            usesGlitch = data.AttrBool("glitch", false);
            if (data.AttrBool("blur", true)) { effect = GFX.FxGaussianBlur; }
            else { effect = null; }
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
            else
            {
                renderOffsetY = 0;
                renderOffsetX = 0;
            }
            currentY = 0;
            currentX = 0;
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
        public override void Render(Scene scene)
        {
            if ((!HorizontalLines && !VerticalLines) || Opacity == 0)
            {
                return;
            }
            Level level = scene as Level;
            if (level is null) return;
            currentY = level.Bounds.Top - compensation.Y;
            currentX = level.Bounds.Left - compensation.X;
            GameplayRenderer.End();
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GridRenderTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            GameplayRenderer.Begin();
            if (HorizontalLines)
            {
                for (float i = 0; i < level.Bounds.Height + compensation.Y + DiagonalOffsetY; i += SpacingY)
                {
                    Draw.Line(level.Bounds.Left - compensation.X,
                              currentY + renderOffsetY,
                              level.Bounds.Right + compensation.X,
                              currentY + renderOffsetY - DiagonalOffsetX,
                              color, LineHeight);

                    currentY += SpacingY;

                }
            } //Draw to render target
            if (VerticalLines)
            {
                for (float i = 0; i < level.Bounds.Width + compensation.X + DiagonalOffsetX; i += SpacingX)
                {
                    Draw.Line(currentX + renderOffsetX,
                              level.Bounds.Top - compensation.Y,
                              currentX + renderOffsetX + DiagonalOffsetY,
                              level.Bounds.Bottom + compensation.Y,
                              color, LineWidth);

                    currentX += SpacingX;
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
            Draw.SpriteBatch.Draw(GridRenderTarget, Vector2.Zero, Color.White); //Draw content of render target to level
        }
    }
}

