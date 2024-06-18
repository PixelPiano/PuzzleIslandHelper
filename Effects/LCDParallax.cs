using Celeste.Mod.Backdrops;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;


namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/LCDParallax")]
    public class LCDParallax : Parallax
    {
        private readonly string flag;
        private bool DrawBase;
        private bool UsesAreas;


        private bool Simple;
        private int AreaCount;
        private Rectangle[] ClipAreas;

        private float ClipAreaTime;
        private float clipAreaTimer;
        private float JitterAmount;
        private Vector3 Jitter = new();
        private float Frequency = 0.02f;
        private float timer;
        private readonly VirtualRenderTarget Target = VirtualContent.CreateRenderTarget("LCDParallaxTarget", 320, 180);
        private Vector3 ColorOffset;
        private float Opacity = 1;

        public LCDParallax(MTexture texture) : base(texture) { }

        public LCDParallax(BinaryPacker.Element data)
          : this(GFX.Game[data.Attr("texture")])
        {
            Scroll = new(data.AttrFloat("scrollX", 1f), data.AttrFloat("scrollY", 1f));
            Speed = new(data.AttrFloat("speedX"), data.AttrFloat("speedY"));
            Color = Calc.HexToColor(data.Attr("color", "ffffff")) * data.AttrFloat("alpha");
            FlipX = data.AttrBool("flipX");
            FlipY = data.AttrBool("flipY");
            LoopX = data.AttrBool("loopX", true);
            LoopY = data.AttrBool("loopY", true);
            InstantIn = data.AttrBool("instantIn");
            InstantOut = data.AttrBool("instantOut");
            DoFadeIn = data.AttrBool("fadeIn");
            WindMultiplier = data.AttrFloat("wind");
            Visible = false;
            ColorOffset = new(data.AttrFloat("redOffset"), data.AttrFloat("greenOffset"), data.AttrFloat("blueOffset"));
            JitterAmount = data.AttrFloat("jitterAmount");

            DrawBase = data.AttrBool("drawBase");
            Simple = data.AttrBool("simple");
            if (!Simple)
            {
                AreaCount = data.AttrInt("clipAreas");
                UsesAreas = AreaCount > 0;
                if (UsesAreas)
                {
                    ClipAreas = RandomizeAreas();
                    ClipAreaTime = data.AttrFloat("clipAreaTime");
                }

            }
        }

        private Rectangle[] RandomizeAreas()
        {
            Rectangle[] r = new Rectangle[AreaCount];
            for (int i = 0; i < r.Length; i++)
            {
                r[i] = RandomizeArea();
            }
            return r;
        }

        public override void BeforeRender(Scene scene)
        {
            base.BeforeRender(scene);
            Level level = scene as Level;
            ApplyParameters(level);
            ApplyStandardParameters(level);
            if (Simple)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone,null, Matrix.Identity);
                base.Render(scene);
                Draw.SpriteBatch.End();
            }
            else
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.TempA);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
                DrawMask(scene);
                Draw.SpriteBatch.End();

                Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, ShaderFX.LCD, Matrix.Identity);
                base.Render(scene);
                Draw.SpriteBatch.End();

                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, EasyRendering.AlphaMaskBlendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ShaderFX.SineLines, Matrix.Identity);
                Draw.SpriteBatch.Draw(GameplayBuffers.TempA, Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();
            }
        }
        public override void Render(Scene scene)
        {
            Draw.SpriteBatch.Draw(Target, Vector2.Zero, Color);
        }
        private Rectangle RandomizeArea()
        {
            int x = Calc.Random.Range(0, 640);
            int y = Calc.Random.Range(0, 360);
            return new Rectangle(x, y, Calc.Random.Range(10, 60), Calc.Random.Range(10, 60));
        }
        private void DrawMask(Scene scene)
        {
            Camera camera = (scene as Level).Camera;
            for (int i = 0; i < ClipAreas.Length; i++)
            {
                Rectangle screen = ClipAreas[i];
                screen.Y -= (int)(((scene as Level).Camera.Y + CameraOffset.Y) * Scroll.Y);
                screen.Y %= 360;
                if (screen.Y < 0f)
                {
                    screen.Y += 360;
                }

                screen.X -= (int)(((scene as Level).Camera.X + CameraOffset.X) * Scroll.X);
                screen.X %= 640;
                if (screen.X < 0f)
                {
                    screen.X += 640;
                }
                Draw.Rect(screen, Color.White * Opacity);
            }
        }
        public override void Update(Scene scene)
        {
            base.Update(scene);
            timer += Engine.DeltaTime;
            if (!Simple)
            {

                clipAreaTimer += Engine.DeltaTime;
                if (clipAreaTimer > ClipAreaTime)
                {
                    clipAreaTimer = 0;
                    ClipAreas = RandomizeAreas();
                }

            }
            if (JitterAmount != 0)
            {
                if (timer > Frequency)
                {
                    float abs = Math.Abs(JitterAmount);
                    Jitter.X = Calc.Random.Range(0, abs);
                    Jitter.Y = Calc.Random.Range(0, abs);
                    Jitter.Z = Calc.Random.Range(0, abs);
                    timer = 0;
                }
            }
        }
        public void ApplyParameters(Level level)
        {
            Matrix? camera = level.Camera.Matrix;
            ShaderFX.LCD.Parameters["RedOffset"]?.SetValue(ColorOffset.X + Math.Sign(ColorOffset.X) * Jitter.X);
            ShaderFX.LCD.Parameters["GreenOffset"]?.SetValue(ColorOffset.Y + Math.Sign(ColorOffset.Y) * Jitter.Y);
            ShaderFX.LCD.Parameters["BlueOffset"]?.SetValue(ColorOffset.Z + Math.Sign(ColorOffset.Z) * Jitter.Z);
            ShaderFX.LCD.Parameters["DeltaTime"]?.SetValue(Engine.DeltaTime);
            ShaderFX.LCD.Parameters["Time"]?.SetValue(Engine.Scene.TimeActive);
            ShaderFX.LCD.Parameters["CamPos"]?.SetValue(level.Camera.Position);
            ShaderFX.LCD.Parameters["Dimensions"]?.SetValue(new Vector2(320, 180) * (GameplayBuffers.Gameplay.Width / 320));
            ShaderFX.LCD.Parameters["ColdCoreMode"]?.SetValue(level.CoreMode == Session.CoreModes.Cold);

            Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;

            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            // from communal helper
            Matrix halfPixelOffset = Matrix.Identity;

            ShaderFX.LCD.Parameters["TransformMatrix"]?.SetValue(halfPixelOffset * projection);

            ShaderFX.LCD.Parameters["ViewMatrix"]?.SetValue(Matrix.Identity);
        }
        public void ApplyStandardParameters(Level level)
        {
            Matrix? camera = level.Camera.Matrix;
            ShaderFX.SineLines.Parameters["DeltaTime"]?.SetValue(Engine.DeltaTime);
            ShaderFX.SineLines.Parameters["Time"]?.SetValue(Engine.Scene.TimeActive);
            ShaderFX.SineLines.Parameters["CamPos"]?.SetValue(level.Camera.Position);
            ShaderFX.SineLines.Parameters["Dimensions"]?.SetValue(new Vector2(320, 180) * (GameplayBuffers.Gameplay.Width / 320));
            ShaderFX.SineLines.Parameters["ColdCoreMode"]?.SetValue(level.CoreMode == Session.CoreModes.Cold);

            Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;

            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            // from communal helper
            Matrix halfPixelOffset = Matrix.Identity;

            ShaderFX.SineLines.Parameters["TransformMatrix"]?.SetValue(halfPixelOffset * projection);

            ShaderFX.SineLines.Parameters["ViewMatrix"]?.SetValue(Matrix.Identity);
        }
    }
}

