using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LCDArea")]
    [Tracked]
    public class LCDArea : Entity
    {
        public static Effect Shader;
        private Vector3 ColorOffset;
        private float JitterAmount;
        private Vector3 Jitter = new();
        private float Frequency = 0.02f;
        private float timer;
        private static VirtualRenderTarget _Target;
        public static VirtualRenderTarget Target => _Target ??=
                      VirtualContent.CreateRenderTarget("LCDAreaTarget", 320, 180);

        private static VirtualRenderTarget _BgTarget;
        public static VirtualRenderTarget BgTarget => _BgTarget ??=
                      VirtualContent.CreateRenderTarget("LCDAreaBgTarget", 320, 180);
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            _Target?.Dispose();
            _Target = null;
            _BgTarget?.Dispose();
            _BgTarget = null;
        }
        public LCDArea(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            ColorOffset = new(0.001f, 0.0005f, -0.0005f);
            JitterAmount = 0.001f;
            Collider = new Hitbox(data.Width, data.Height);
            Add(new BeforeRenderHook(BeforeRender));
        }
        public void ApplyParameters(Level level)
        {
            Matrix? camera = level.Camera.Matrix;
            Shader.Parameters["RedOffset"]?.SetValue(ColorOffset.X + Math.Sign(ColorOffset.X) * Jitter.X);
            Shader.Parameters["GreenOffset"]?.SetValue(ColorOffset.Y + Math.Sign(ColorOffset.Y) * Jitter.Y);
            Shader.Parameters["BlueOffset"]?.SetValue(ColorOffset.Z + Math.Sign(ColorOffset.Z) * Jitter.Z);
            Shader.Parameters["DeltaTime"]?.SetValue(Engine.DeltaTime);
            Shader.Parameters["Time"]?.SetValue(Engine.Scene.TimeActive);
            Shader.Parameters["CamPos"]?.SetValue(level.Camera.Position);
            Shader.Parameters["Dimensions"]?.SetValue(new Vector2(320, 180) * (GameplayBuffers.Gameplay.Width / 320));
            Shader.Parameters["ColdCoreMode"]?.SetValue(level.CoreMode == Session.CoreModes.Cold);

            Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;

            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            // from communal helper
            Matrix halfPixelOffset = Matrix.Identity;

            Shader.Parameters["TransformMatrix"]?.SetValue(halfPixelOffset * projection);

            Shader.Parameters["ViewMatrix"]?.SetValue(Matrix.Identity);
        }
        private void BeforeRender()
        {
            if (Scene is not Level level)
            {
                return;
            }
            //ApplyParameters(level);
            Shader.ApplyStandardParameters(level);


            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null, Shader);

            Draw.SpriteBatch.Draw(GameplayBuffers.Level, level.Camera.Position, Collider.Bounds, Color.White);
            Draw.SpriteBatch.End();
        }
        public override void Update()
        {
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
                else
                {
                    timer+=Engine.DeltaTime;
                }
            }
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level)
            {
                return;
            }
            //Draw.SpriteBatch.Draw(BgTarget, Vector2.Zero, From.Lerp(From.White, From.Black, 0.6f));

            //Draw.SpriteBatch.Draw(GameplayBuffers.Gameplay, Vector2.Zero, From.White);

            Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);
        }
        public static void Unload()
        {
            Shader?.Dispose();
            Target?.Dispose();
        }
    }
}