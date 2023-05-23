using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Entities //Replace with your mod's namespace
{
    public class EasyRendering : Entity
    {
        //All methods assume Draw.SpriteBatch.End() was called beforehand
        public static readonly BlendState AlphaMaskBlendState = new()
        {
            ColorSourceBlend = Blend.Zero,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.SourceColor,
            AlphaSourceBlend = Blend.Zero,
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.SourceColor
        };
        public static VirtualRenderTarget ApplyEffect(VirtualRenderTarget obj, Effect effect)
        {

            return obj;
        }
        public static float seed = 0;
        public static float timer = 0;
        public override void Update()
        {
            base.Update();
            timer += Engine.DeltaTime;
            seed = Calc.Random.NextFloat();
        }
        public static VirtualRenderTarget ObjectToObject(VirtualRenderTarget output, VirtualRenderTarget drawing, Level l)
        {
            Draw.SpriteBatch.End();
            Engine.Graphics.GraphicsDevice.SetRenderTarget(output);
            Draw.SpriteBatch.Begin();
            Draw.SpriteBatch.Draw(drawing, Vector2.Zero, Color.White);
            return output;
        }
        public static VirtualRenderTarget AddGlitch(VirtualRenderTarget obj, [Optional] float glitchAmount, [Optional] float glitchAmplitude)
        {
            float glitchSave = Glitch.Value;
            float val1 = glitchAmount == 0 ? Calc.Random.Range(0.2f, 1.0f) : glitchAmount;
            float val2 = glitchAmplitude == 0 ? Calc.Random.Range(2, 30) : glitchAmplitude;
            Glitch.Value = val1;
            Glitch.Apply(obj, timer, seed, val2);
            Glitch.Value = glitchSave;
            return obj;
        }
        /// <summary>  
        /// Draws a VirtualRenderTarget to the GameplayBuffers.Gameplay render target  
        /// </summary>  
        public static void DrawToGameplay(VirtualRenderTarget obj, Level l, [Optional] Color color)
        {
            Draw.SpriteBatch.End();
            Color col = color == null ? Color.White : color;
            Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
            GameplayRenderer.Begin();
            Draw.SpriteBatch.Draw(obj, l.Camera.Position, col);
        }
        /// <summary>  
        /// Draws a sprite to the specified target  
        /// </summary>  
        public static VirtualRenderTarget SetRenderMask(VirtualRenderTarget RenderTarget, Sprite sprite, Level l)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(RenderTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, l.Camera.Matrix);
            sprite.Render();
            Draw.SpriteBatch.End();
            return RenderTarget;
        }
        public static VirtualRenderTarget SetRenderMask(VirtualRenderTarget RenderTarget, Entity entity, Level l)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(RenderTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, l.Camera.Matrix);
            entity.Render();
            Draw.SpriteBatch.End();
            return RenderTarget;
        }
        public static VirtualRenderTarget SetRenderMask(VirtualRenderTarget RenderTarget, Image image, Level l)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(RenderTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, l.Camera.Matrix);
            image.Render();
            Draw.SpriteBatch.End();
            return RenderTarget;
        }
        public static VirtualRenderTarget SetRenderMask(VirtualRenderTarget RenderTarget, Action drawing, Level l)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(RenderTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, l.Camera.Matrix);
            drawing.Invoke();
            Draw.SpriteBatch.End();
            return RenderTarget;
        }

        /// <summary>  
        /// Masks over the obj target with the mask target's contents 
        /// </summary>  
        public static VirtualRenderTarget MaskToObject(VirtualRenderTarget obj, VirtualRenderTarget mask)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(obj);
            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                AlphaMaskBlendState,
                SamplerState.PointClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone,
                null, Matrix.Identity);
            Draw.SpriteBatch.Draw(mask, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
            return obj;
        }
        public static VirtualRenderTarget MaskToObject(VirtualRenderTarget obj, VirtualRenderTarget mask, BlendState blendState)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(obj);
            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                blendState,
                SamplerState.PointClamp,
                DepthStencilState.Default,
                RasterizerState.CullNone,
                null, Matrix.Identity);
            Draw.SpriteBatch.Draw(mask, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
            return obj;
        }
        /// <summary>  
        /// Draws whatever is in the draw method to the obj target and then masks over the obj target with the mask target's contents 
        /// </summary>  
        public static VirtualRenderTarget MaskToObject(VirtualRenderTarget obj, VirtualRenderTarget mask, Action drawing)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(obj);
            GameplayRenderer.Begin();
            drawing.Invoke();
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, AlphaMaskBlendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Matrix.Identity);
            Draw.SpriteBatch.Draw(mask, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
            return obj;
        }
        /// <summary>  
        /// Draws whatever is in the draw method to the obj target
        /// </summary>  
        public static VirtualRenderTarget DrawToObject(VirtualRenderTarget obj, Action drawing, Level l)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(obj);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, l.Camera.Matrix);
            drawing.Invoke();
            Draw.SpriteBatch.End();
            return obj;
        }
        public static VirtualRenderTarget DrawToObject(VirtualRenderTarget obj, Entity entity, Level l)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(obj);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin();
            entity.Render();
            Draw.SpriteBatch.End();
            return obj;
        }
        public static VirtualRenderTarget DrawToObject(VirtualRenderTarget obj, Sprite sprite, Level l)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(obj);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin();
            sprite.Render();
            Draw.SpriteBatch.End();
            return obj;
        }
    }
}