using FrostHelper;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class ShaderEntity : Entity
    {
        public VirtualRenderTarget Mask;
        public VirtualRenderTarget RenderTarget;
        private string path;
        public Effect Shader;
        public Color Color = Color.White;
        public float Alpha = 1;

        public static readonly BlendState AlphaMaskBlendState = new()
        {
            ColorSourceBlend = Blend.Zero,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.SourceColor,
            AlphaSourceBlend = Blend.Zero,
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.SourceColor
        };
        public ShaderEntity(Vector2 position, string path, int width, int height)
            : base(position)
        {
            this.path = path;
            Shader = ShaderHelperIntegration.GetEffect(path);
            Collider = new Hitbox(width, height);
            RenderTarget = VirtualContent.CreateRenderTarget("ShaderEntityTarget", (int)Width, (int)Height);
            Mask = VirtualContent.CreateRenderTarget("ShaderEntityMask", (int)Width, (int)Height);
            Add(new BeforeRenderHook(BeforeRender));
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Shader?.Dispose();
            Shader = null;
            Mask?.Dispose();
            Mask = null;
            RenderTarget?.Dispose();
            RenderTarget = null;
        }
        private void BeforeRender()
        {
            if (Scene is not Level level || Alpha <= 0) return;

            Camera camera = level.Camera;
            Engine.Instance.GraphicsDevice.SetRenderTarget(Mask);
            int num = GameplayBuffers.Gameplay.Width / 320;
            Matrix camMat = camera.Matrix * Matrix.CreateScale(num);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            MaskRender(level);
            GameplayRenderer.End();

            Engine.Instance.GraphicsDevice.SetRenderTarget(RenderTarget);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            ContentRender(level);
            GameplayRenderer.End();

            Shader.ApplyStandardParameters();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, EasyRendering.AlphaMaskBlendState, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, Shader, Matrix.Identity);
            Draw.SpriteBatch.Draw((RenderTarget2D)Mask, Vector2.Zero, Color.White);
            GameplayRenderer.End();
        }

        public override void Render()
        {
            base.Render();
            if (Alpha > 0)
            {
                Draw.SpriteBatch.Draw(RenderTarget, Position, Color * Alpha);
            }
        }
        public virtual void ApplyParameters()
        {
            Level level = Scene as Level;
            EffectParameterCollection parameters = Shader.Parameters;
            parameters["DeltaTime"]?.SetValue(Engine.DeltaTime);
            parameters["Time"]?.SetValue(Engine.Scene.TimeActive);
            parameters["Dimensions"]?.SetValue(new Vector2(320f, 180f) * HDlesteCompat.Scale);
            parameters["CamPos"]?.SetValue(level.Camera.Position);
            parameters["ColdCoreMode"]?.SetValue(level.CoreMode == Session.CoreModes.Cold);
            Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
            Matrix matrix = Matrix.CreateOrthographicOffCenter(0f, viewport.Width, viewport.Height, 0f, 0f, 1f);
            Matrix matrix2 = ((FrostModule.Framework == FrameworkType.FNA) ? Matrix.Identity : Matrix.CreateTranslation(-0.5f, -0.5f, 0f));
            parameters["TransformMatrix"]?.SetValue(matrix2 * matrix);
            parameters["ViewMatrix"]?.SetValue(Matrix.Identity);
        }
        public virtual void ContentRender(Level level)
        {
            Draw.SpriteBatch.Draw(ShaderEntityTest.Texture.Texture.Texture_Safe, Vector2.Zero, Color.White);
        }
        public virtual void MaskRender(Level level)
        {
            Draw.SpriteBatch.Draw(ShaderEntityTest.Texture.Texture.Texture_Safe, Vector2.Zero, Color.White);
        }
        private void DrawLine(Vector2 topRight, float percent, int thickness)
        {
            percent *= 2;
            Draw.Line(topRight + Vector2.UnitX * Width * percent, topRight + Vector2.UnitY * Width * percent, Color.Black, thickness);
        }
    }
}

