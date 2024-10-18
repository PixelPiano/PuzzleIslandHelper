using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;


namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public class ShaderThing : Entity
    {
        public Effect Effect;
        private string effectName;
        public VirtualRenderTarget Target;
        public VirtualRenderTarget Mask;
        public float Amplitude = 1;
        public ShaderThing(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Effect = ShaderHelperIntegration.GetEffect(effectName); //if we do this here, we only need to get the effect once. You can also do this in a static method if the shader parameters don't depend on any individual entity.
            Target = VirtualContent.CreateRenderTarget("idkWhatTheNameDoesButMakeItUniqueAnywaysShrug", 320, 180); //320 = width of screen, 180 = height of screen
            Mask = VirtualContent.CreateRenderTarget("idkWhatTheNameDoesButMakeItUniqueAnywaysShrugThisTimeItsAMask", 320, 180); //make the mask the same size as the target
            //the uv value for each pixel in the shader is represented by a value from 0-1 for x and y
            //since our target is 320x180, (0.5, 0.5) would refer to any pixel at/close enough to the center of the target.
            //(1,1) would be the bottom right, and (0,0) is the top left.

            Add(new BeforeRenderHook(BeforeRender)); //Entity doesn't have a BeforeRender method by default, so we sneak our own method in when the rest are called
        }
        private void BeforeRender()
        {
            Target.DrawThenMask(DrawMask, DrawToTarget, Matrix.Identity);
        }
        public void DrawMask()
        {
            //draw whatever you want removed from the target
            //We're using Matrix.Identity in BeforeRender, so imagine the target is set at (0,0) right now.
            Draw.Rect(0, 0, 30, 50, Color.White);
        }
        public void DrawToTarget()
        {
            //draw stuff to the target
            Draw.Rect(0, 0, 320, 180, Color.Red);
        }
        public override void Render()
        {
            Level level = Scene as Level;
            //If you don't want to restart the game every time you make a change to your shader, uncomment the next line. Change it back once you're happy with it, though.
            //Effect = ShaderHelperIntegration.GetEffect(effectName);
            if (Effect != null)
            {
                ApplyParameters(level, level.Camera.Matrix);
                EndSpriteBatch();
                BeginSpriteBatch(Effect, level.Camera.Matrix);
                Draw.SpriteBatch.Draw(Target, Position, Color.White);
                EndSpriteBatch();
                GameplayRenderer.Begin();
            }
        }
        public void ApplyParameters(Level level, Matrix matrix)
        {
            Effect.Parameters["DeltaTime"]?.SetValue(Engine.DeltaTime);
            Effect.Parameters["Time"]?.SetValue(Engine.Scene.TimeActive);
            Effect.Parameters["CamPos"]?.SetValue(level.Camera.Position);
            Effect.Parameters["Dimensions"]?.SetValue(new Vector2(320, 180)); //Change this to the dimensions of your render target, as it can be useful to have these values available in the shader.
            Effect.Parameters["ColdCoreMode"]?.SetValue(level.CoreMode == Session.CoreModes.Cold);
            Effect.Parameters["Amplitude"]?.SetValue(Amplitude); //that handy extra value i included in the shader code


            //you can ignore this next part it's scary matrix stuff
            Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
            Matrix halfPixelOffset = Matrix.Identity;
            Effect.Parameters["TransformMatrix"]?.SetValue(halfPixelOffset * projection);
            Effect.Parameters["ViewMatrix"]?.SetValue(matrix);
        }
        public void BeginSpriteBatch(Effect effect, Matrix matrix)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, effect, matrix);
        }
        public void EndSpriteBatch()
        {
            Draw.SpriteBatch.End();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            //Dispose of the RenderTarget and the Shader manually (DO NOT FORGET TO DO THIS)
            Effect.Dispose();
            Effect = null;
            Target.Dispose();
            Target = null;
            Mask.Dispose();
            Mask = null;
        }

    }
}