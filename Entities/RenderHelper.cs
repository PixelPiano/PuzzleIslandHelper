using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.PuzzleIslandHelper.Entities //Replace with your mod's namespace
{
    [Tracked]
    public class RenderHelper : Entity
    {
        public static readonly BlendState AlphaMaskBlendState = new()
        {
            ColorSourceBlend = Blend.Zero,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.SourceColor,
            AlphaSourceBlend = Blend.Zero,
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.SourceColor
        };
        private static VirtualRenderTarget _ComponentRender;
        public static VirtualRenderTarget ComponentRender => _ComponentRender ??=
              VirtualContent.CreateRenderTarget("MaskRenderHookTarget", 320, 180);
        private static VirtualRenderTarget _ComponentMask;
        public static VirtualRenderTarget ComponentMask => _ComponentMask ??=
              VirtualContent.CreateRenderTarget("MaskRenderHookMask", 320, 180);
        public RenderHelper() : base(Vector2.Zero)
        {
            AddTag(Tags.Global);
            AddTag(Tags.TransitionUpdate);
            Add(new BeforeRenderHook(BeforeRender));
        }

        private void BeforeRender()
        {
            if (Scene is not Level level)
            {
                return;
            }
            bool drew = false;
            Engine.Graphics.GraphicsDevice.SetRenderTarget(ComponentMask);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Engine.Graphics.GraphicsDevice.SetRenderTarget(ComponentRender);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

            foreach (MaskRenderHook component in level.Tracker.GetComponents<MaskRenderHook>())
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(ComponentMask);
                if (component.Visible && component.RenderMask != null)
                {
                    Matrix matrix = component.CameraMatrix ? level.Camera.Matrix : Matrix.Identity;
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
                    component.RenderMask.Invoke();
                    Draw.SpriteBatch.End();
                }
                Engine.Graphics.GraphicsDevice.SetRenderTarget(ComponentRender);
                if (component.Visible && component.RenderDrawing != null)
                {
                    Matrix matrix = component.CameraMatrix ? level.Camera.Matrix : Matrix.Identity;
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, component.Effect, matrix);

                    component.RenderDrawing.Invoke();
                    Draw.SpriteBatch.End();
                }
                drew = true;
            }

            if (drew)
            {
                Draw.SpriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    AlphaMaskBlendState,
                    SamplerState.PointClamp,
                    DepthStencilState.Default,
                    RasterizerState.CullNone,
                    null, Matrix.Identity);
                Draw.SpriteBatch.Draw(ComponentMask, Vector2.Zero, Color.White);
                Draw.SpriteBatch.End();
            }

        }

        public override void Render()
        {
            base.Render();
            if (Scene is not Level level)
            {
                return;
            }
            Draw.SpriteBatch.Draw(ComponentRender, level.Camera.Position, Color.White);
        }
        internal static void Load()
        {
           On.Celeste.LevelLoader.ctor += LevelLoader_ctor;
        }
        internal static void Unload()
        {
            On.Celeste.LevelLoader.ctor -= LevelLoader_ctor;
        }

        private static void LevelLoader_ctor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition)
        {
            orig(self, session, startPosition);
            self.Level.Add(new RenderHelper());
        }
    }

}