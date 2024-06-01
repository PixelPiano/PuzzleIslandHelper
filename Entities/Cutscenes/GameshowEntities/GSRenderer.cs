using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
{
    [Tracked]
    public class GSRenderer : Entity
    {
        public static BlendState BlendState = BlendState.Additive;
        public static bool Invert;
        public static Color Color = Color.White;
        public GSRenderer() : base(Vector2.Zero)
        {
            Tag |= Tags.Global | Tags.TransitionUpdate;
            Depth = -100001;
            Add(new BeforeRenderHook(BeforeRender));
        }
        private static VirtualRenderTarget _Mask, _Buffer;
        public static VirtualRenderTarget Mask => _Mask ??= VirtualContent.CreateRenderTarget("Mask", 320, 180);
        public static VirtualRenderTarget Buffer => _Buffer ??= VirtualContent.CreateRenderTarget("Buffer", 320, 180);

        public static BlendState RemoveMask = new()
        {
            ColorSourceBlend = Blend.Zero,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.One,
            AlphaSourceBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.ReverseSubtract,
            AlphaDestinationBlend = Blend.One
        };
        public static void Unload()
        {
            _Mask?.Dispose();
            _Buffer?.Dispose();
            _Mask = null;
            _Buffer = null;
        }
        public void DrawLights(Level level, Matrix matrix, Color? color = null, Vector2? offset = null)
        {
            foreach (GameshowSpotlight light in level.Tracker.GetEntities<GameshowSpotlight>())
            {
                light.RenderLight(matrix, color, offset);
            }
        }
        public void DrawCircles(Level level, Color? color = null, Vector2? offset = null)
        {
            foreach (GameshowSpotlight light in level.Tracker.GetEntities<GameshowSpotlight>())
            {
                light.RenderCircle(color, offset);
            }
        }
        public void DrawSpotlights(Level level, Matrix matrix, Color? color = null, Vector2? offset = null)
        {
            DrawLights(level, matrix, color, offset);
            DrawCircles(level, color, offset);
        }
        public void BeforeRender()
        {
            if (Scene is not Level level) return;
            bool lightsExist = level.Tracker.GetEntities<GameshowSpotlight>().Count > 0;
            if (lightsExist && Invert)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(Mask);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, level.Camera.Matrix);
                DrawSpotlights(level, level.Camera.Matrix, Color.White);
                Draw.SpriteBatch.End();
            }
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Buffer);
            Engine.Graphics.GraphicsDevice.Clear(Invert ? Color.Black : Color.Transparent);
            if (lightsExist)
            {
                if (Invert)
                {
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, RemoveMask, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, level.Camera.Matrix);
                    Draw.SpriteBatch.Draw(Mask, level.Camera.Position, Color.White);
                    Draw.SpriteBatch.End();
                }
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, level.Camera.Matrix);
                DrawSpotlights(level, level.Camera.Matrix);
                Draw.SpriteBatch.End();
            }
        }
        public override void Render()
        {
            base.Render();
            if (Scene is Level level)
            {
                Draw.SpriteBatch.Draw(Buffer, level.Camera.Position, Color);
                foreach (GameshowSpotlight light in level.Tracker.GetEntities<GameshowSpotlight>())
                {
                    light.RenderLed();
                }
            }
        }
    }
}
