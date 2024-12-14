using Microsoft.Xna.Framework;
using Monocle;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities
{

    [Tracked]
    public class CalidusGlow : GraphicsComponent
    {
        public VirtualRenderTarget Buffer;
        public Effect Shader;
        public int Size;
        public float Amplitude = 1;
        public float FloatOffset;
        public float FloatHeight = 3;
        public float FloatAmount = 0;
        public bool Floats;
        public CalidusGlow(int size) : base(true)
        {
            Size = size;
            Buffer = VirtualContent.CreateRenderTarget("calidusglowtarget", size, size);
            Shader = ShaderHelper.TryGetEffect("CalidusGlow");
        }
        public override void Update()
        {
            base.Update();
            FloatAmount = Calc.Approach(FloatAmount, Floats ? 1 : 0, Engine.DeltaTime);
            FloatOffset = (float)Math.Sin(Scene.TimeActive) * FloatHeight * FloatAmount;
        }
        public void BeforeRender()
        {
            Buffer.SetAsTarget(Color.White);
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level) return;
            Shader.ApplyParameters(level, level.Camera.Matrix, Amplitude);
            if (Entity is PlayerCalidus calidus)
            {
                Shader.Parameters["Weakened"]?.SetValue(calidus.RoboInventory.Weak);
            }

            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap,
                DepthStencilState.None, RasterizerState.CullNone, Shader, level.Camera.Matrix);
            Draw.SpriteBatch.Draw(Buffer, (RenderPosition + Vector2.UnitY * FloatOffset).Floor(), Color.White);
            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
        }
        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            Buffer?.Dispose();
            Buffer = null;
            Shader?.Dispose();
            Shader = null;
        }
    }
}
