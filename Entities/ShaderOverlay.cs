using Celeste.Mod.Entities;
using FrostHelper;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ShaderOverlay")]
    [Tracked]
    public class ShaderOverlay : Entity
    {
        private readonly string flag;
        public Effect Effect;
        private string path;
        public float Alpha = 1;
        public bool ForceLevelRender;
        public float Amplitude;
        public bool UsesFlag;
        public bool UseRawDeltaTime;
        private float dummyTimer;
        public bool State
        {
            get
            {
                if (ForceLevelRender)
                {
                    return true;
                }
                return FlagState(Scene as Level);
            }
        }
        public ShaderOverlay(string path, string flag = "", bool forceRender = false, float alpha = 1) : base(Vector2.Zero)
        {
            this.path = path;
            Effect = ShaderHelper.TryGetEffect(path, true);
            this.flag = flag;
            Alpha = alpha;
            ForceLevelRender = forceRender;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Effect?.Dispose();
            Effect = null;
        }
        public virtual void ApplyParameters(bool identity)
        {
            Level level = FrostModule.GetCurrentLevel();
            if (Effect is null)
            {
                return;
            }
            if (UseRawDeltaTime)
            {
                dummyTimer += Engine.RawDeltaTime;
            }
            Effect.Parameters["DeltaTime"]?.SetValue(UseRawDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime);
            Effect.Parameters["Time"]?.SetValue(UseRawDeltaTime ? dummyTimer : Engine.Scene.TimeActive);
            Effect.Parameters["Dimensions"]?.SetValue(new Vector2(320f, 180f) * HDlesteCompat.Scale);
            Effect.Parameters["Amplitude"]?.SetValue(Amplitude);
            Effect.Parameters["CamPos"]?.SetValue(level.Camera.Position);
            Effect.Parameters["ColdCoreMode"]?.SetValue(level.CoreMode == Session.CoreModes.Cold);
            Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
            Matrix matrix = Matrix.CreateOrthographicOffCenter(0f, viewport.Width, viewport.Height, 0f, 0f, 1f);
            Matrix matrix2 = ((FrostModule.Framework == FrameworkType.FNA) ? Matrix.Identity : Matrix.CreateTranslation(-0.5f, -0.5f, 0f));
            Effect.Parameters["TransformMatrix"]?.SetValue(matrix2 * matrix);
            Effect.Parameters["ViewMatrix"]?.SetValue(identity ? Matrix.Identity : level.Camera.Matrix);
        }
        public static void Load()
        {
            //Note: don't add onContentUpdate, it messes with things. Just use runCompiler.bat
            On.Celeste.Glitch.Apply += Apply_HOOK;
        }
        public static void Unload()
        {
            On.Celeste.Glitch.Apply -= Apply_HOOK;

        }
        public static void Apply_HOOK(On.Celeste.Glitch.orig_Apply orig, VirtualRenderTarget source, float timer, float seed, float amplitude)
        {
            orig(source, timer, seed, amplitude);
            Level level = FrostModule.GetCurrentLevel();
            using List<Entity>.Enumerator enumerator = level.Tracker.SafeGetEntities<ShaderOverlay>().GetEnumerator();
            List<Entity> overlays = level.Tracker.GetEntities<ShaderOverlay>();
            if (!enumerator.MoveNext())
            {
                return;
            }
            foreach (ShaderOverlay so in overlays)
            {
                if (so != null && so.State)
                {
                    Apply(source, source, so, false, so.Alpha);
                }
            }
        }
        public static void Apply(VirtualRenderTarget source, VirtualRenderTarget target, ShaderOverlay overlay = null, bool clear = false, float alpha = 1)
        {
            if (source is null || target is null)
            {
                return;
            }
            overlay?.ApplyParameters(true);
            VirtualRenderTarget tempA = GameplayBuffers.TempA;
            Engine.Instance.GraphicsDevice.SetRenderTarget(tempA);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);

            Draw.SpriteBatch.Draw((RenderTarget2D)source, Vector2.Zero, Color.White);
            GameplayRenderer.End();
            Engine.Instance.GraphicsDevice.SetRenderTarget(target);
            if (clear)
            {
                Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            }

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, overlay?.Effect);
            Draw.SpriteBatch.Draw((RenderTarget2D)tempA, Vector2.Zero, Color.White * alpha);
            Draw.SpriteBatch.End();
        }
        public static void Apply(VirtualRenderTarget source, VirtualRenderTarget target, Rectangle destinationRectangle, ShaderOverlay overlay = null, bool clear = false, float alpha = 1)
        {
            if (source is null || target is null)
            {
                return;
            }
            overlay?.ApplyParameters(true);
            VirtualRenderTarget tempA = GameplayBuffers.TempA;
            Engine.Instance.GraphicsDevice.SetRenderTarget(tempA);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);

            Draw.SpriteBatch.Draw((RenderTarget2D)source, Vector2.Zero, Color.White);
            GameplayRenderer.End();
            Engine.Instance.GraphicsDevice.SetRenderTarget(target);
            if (clear)
            {
                Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            }

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, overlay?.Effect);
            Draw.SpriteBatch.Draw((RenderTarget2D)tempA, destinationRectangle, Color.White * alpha);
            Draw.SpriteBatch.End();
        }
        public static void Apply(VirtualRenderTarget source, VirtualRenderTarget target, Rectangle destinationRectangle, Rectangle? sourceRectangle, ShaderOverlay overlay = null, bool clear = false, float alpha = 1)
        {
            if (source is null || target is null)
            {
                return;
            }
            overlay?.ApplyParameters(true);
            VirtualRenderTarget tempA = GameplayBuffers.TempA;
            Engine.Instance.GraphicsDevice.SetRenderTarget(tempA);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);

            Draw.SpriteBatch.Draw((RenderTarget2D)source, Vector2.Zero, Color.White);
            GameplayRenderer.End();
            Engine.Instance.GraphicsDevice.SetRenderTarget(target);
            if (clear)
            {
                Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
            }

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, overlay?.Effect);
            Draw.SpriteBatch.Draw((RenderTarget2D)tempA, destinationRectangle, sourceRectangle, Color.White * alpha);
            Draw.SpriteBatch.End();
        }
        public bool FlagState(Level level)
        {
            if (!UsesFlag)
            {
                return false;
            }
            return string.IsNullOrEmpty(flag) || level.Session.GetFlag(flag);
        }
    }
}

