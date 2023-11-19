using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using FrostHelper.ModIntegration;
using Celeste.Mod.PuzzleIslandHelper.Components;

namespace Celeste.Mod.PuzzleIslandHelper.Entities //Replace with your mod's namespace
{
    public static class EasyRenderingMethods
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
        private static VirtualRenderTarget _MaskRenderTarget;
        private static VirtualRenderTarget MaskRenderTarget => _MaskRenderTarget ??=
                      CreateRenderTargetAndLog("Mask", 320, 180);
        private static VirtualRenderTarget CreateRenderTargetAndLog(string Name, int Width, int Height)
        {
            Console.WriteLine("Creating RenderTarget \"" + Name + "\"");
            return VirtualContent.CreateRenderTarget(Name, Width, Height);
        }
        public static VirtualRenderTarget SimpleApply(this VirtualRenderTarget target, RenderTarget2D source, Effect eff)
        {
            eff.ApplyStandardParameters();
            Engine.Instance.GraphicsDevice.SetRenderTarget(target);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, eff);
            Draw.SpriteBatch.Draw(source, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
            return target;
        }
        private static void GetActionCollection(this ICollection collection)
        {
            if (collection.Count > 0)
            {
                foreach (var item in collection)
                {
                    (item switch
                    {
                        Sprite sprite => sprite.Render,
                        Action action => action,
                        Entity entity => entity.Render,
                        Image image => image.Render,
                        _ => null
                    })?.Invoke();
                }
            }
        }
        /// <summary>  
        /// Returns a render target with masked content
        /// </summary>  
        public static VirtualRenderTarget DrawThenMask(this VirtualRenderTarget obj, object MaskDraw, object Drawing, Matrix matrix, Effect effect = null)
        {
            Action DrawAction = null;
            Action MaskAction = null;
            DrawAction = Drawing switch
            {
                Sprite sprite => sprite.Render,
                Action action => action,
                Entity entity => entity.Render,
                Image image => image.Render,
                ICollection en => en.GetActionCollection,
                _ => null
            };
            MaskAction = MaskDraw switch
            {
                Sprite sprite => sprite.Render,
                Action action => action,
                Entity entity => entity.Render,
                Image image => image.Render,
                ICollection en => en.GetActionCollection,
                _ => null
            };

            if (DrawAction == null || MaskAction == null)
            {
                return obj;
            }
            Engine.Graphics.GraphicsDevice.SetRenderTarget(MaskRenderTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            MaskRenderTarget.SetRenderMask(MaskAction, matrix);
            obj.DrawToObject(DrawAction, matrix, true, effect);

            obj.MaskToObject(MaskRenderTarget);
            return obj;
        }
        /// <summary>  
        /// Returns a render target with masked content
        /// </summary>  
        public static VirtualRenderTarget DrawThenMask(this VirtualRenderTarget obj, Action MaskDraw, Action Drawing, Matrix matrix, Effect effect = null)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(MaskRenderTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            MaskRenderTarget.SetRenderMask(MaskDraw, matrix);
            obj.DrawToObject(Drawing, matrix, true, effect);

            obj.MaskToObject(MaskRenderTarget);
            return obj;
        }
        public static VirtualRenderTarget SetRenderMask(this VirtualRenderTarget RenderTarget, Action action, Matrix matrix)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(RenderTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, matrix);
            action?.Invoke();
            Draw.SpriteBatch.End();
            return RenderTarget;
        }
        public static VirtualRenderTarget DrawToObject(this VirtualRenderTarget obj, Action drawing, Matrix matrix, bool clear = false, Effect effect = null)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(obj);
            if (clear)
            {
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            }
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, effect, matrix);
            drawing.Invoke();
            Draw.SpriteBatch.End();
            return obj;
        }
        public static VirtualRenderTarget MaskToObject(this VirtualRenderTarget obj, VirtualRenderTarget mask)
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
    }
    public class EasyRendering : Entity
    {
        public override void Render()
        {
            base.Render();
        }
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
        /// Draws a Texture to the specified target  
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
        public static void Begin(Matrix matrix, Effect effect = null)
        {
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, effect, matrix);
        }
        public static VirtualRenderTarget SetRenderMask(VirtualRenderTarget RenderTarget, Action drawing, Level l, bool useIdentity = false)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(RenderTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, useIdentity ? Matrix.Identity : l.Camera.Matrix);
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
        public static VirtualRenderTarget DrawToObject(VirtualRenderTarget obj, Action drawing, Level l, bool clear = false, bool useIdentity = false)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(obj);
            if (clear)
            {
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            }
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, useIdentity ? Matrix.Identity : l.Camera.Matrix);
            drawing.Invoke();
            Draw.SpriteBatch.End();
            return obj;
        }
        public static VirtualRenderTarget DrawToObject(VirtualRenderTarget obj, Action drawing, Level l, BlendState blendState, bool clear = false, bool useIdentity = false)
        {
            BlendState blend = blendState == null ? BlendState.AlphaBlend : blendState;
            Engine.Graphics.GraphicsDevice.SetRenderTarget(obj);
            if (clear)
            {
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            }
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, blend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, useIdentity ? Matrix.Identity : l.Camera.Matrix);
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