using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
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

        public DigitalGrid(float lineWidth, float lineHeight, float ratex, float ratey, int xSpacing, int ySpacing,
                            string color, bool movingEnabled, float diagY, float diagX,
                            float opacity, bool verticalLines, bool horizontalLines, bool blur,bool glitch)
        {
            this.color = Calc.HexToColor(color) * opacity;
            Opacity = opacity;
            timer = 0;
            seed = 0;
            amplitude = Calc.Random.Range(1f, 7f);
            amount = Calc.Random.Range(0.01f, 0.5f);
            usesGlitch = glitch;
            if (blur) { effect = GFX.FxGaussianBlur; }  
            else { effect = null; }
            LineWidth = lineWidth;
            LineHeight = lineHeight;
            DiagonalOffsetY = diagY;
            DiagonalOffsetX = diagX;
            SpacingX = xSpacing;
            SpacingY = ySpacing;
            MovingEnabled = movingEnabled;
            Rate = new Vector2(ratex, ratey);
            VerticalLines = verticalLines;
            HorizontalLines = horizontalLines;
            compensation = new Vector2(xSpacing * 4, ySpacing * 4);
            if (movingEnabled)
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
            if (scene.OnInterval(2/60f))
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
            if((!HorizontalLines && !VerticalLines) || Opacity == 0)
            {
                return;
            }
            var level = scene as Level;
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
                              level.Bounds.Right+compensation.X,
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
                              level.Bounds.Bottom+compensation.Y,
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

