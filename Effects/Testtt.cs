using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.Backdrops;
using Microsoft.Xna.Framework;
using Celeste.Mod.CommunalHelper;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/Testtt")]
    public class Testtt : Backdrop
    {

        private Level level;
        public static Effect Shader;
        public static int Blocks = 7;
        public static float Buffer = 10;
        public static float BlendTime;
        public static float CurrentBlendTime;
        public static float CurrentBufferTime;
        public static bool Blending;

        private float seed;
        private static VirtualRenderTarget _Target;
        public static VirtualRenderTarget Target => _Target ??=
                      VirtualContent.CreateRenderTarget("GlitchBlockTarget", 320, 180);
      
        public Testtt(BinaryPacker.Element data) : base()
        {
            Shader = ShaderHelper.TryGetEffect("curvedScreen");
        }
       public static bool CanRender;

       
       
        public override void BeforeRender(Scene scene)
        {
            base.BeforeRender(scene);
            if(scene is not Level level)
            {
                return;
            }
            if (!CanRender)
            {
                return;
            }
            Shader.ApplyScreenSpaceParameters(level);


            Engine.Graphics.GraphicsDevice.SetRenderTarget(Target);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointWrap, null, null, Shader);

                Draw.Rect(level.Camera.GetBounds(),Color.Black);

            Draw.SpriteBatch.End();
            
        }

        public override void Render(Scene scene)
        {
            base.Render(scene);

            if (CanRender)
            {
                //Draw.SpriteBatch.Draw(GameplayBuffers.Gameplay, Vector2.Zero, Color.White);

                Draw.SpriteBatch.Draw(Target, Vector2.Zero, Color.White);
            }
        }
    }
}
