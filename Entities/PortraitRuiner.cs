using FrostHelper;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked(false)]
    public class PortraitRuiner : Entity
    {
        private readonly string flag;
        public Effect Effect;
        private string path;
        public bool ForceVisible;
        public float Amplitude;
        private static VirtualRenderTarget _Target;
        public static VirtualRenderTarget Target => _Target ??= VirtualContent.CreateRenderTarget("PortraitRuiner", 320, 180);
        public PortraitRuiner(string path, string flag = "") : base(Vector2.Zero)
        {
            Tag = Tags.HUD;
            this.path = path;
            Effect = ShaderHelper.TryGetEffect(path, true);
            this.flag = flag;
            Amplitude = 1;
        }
        private static void Textbox_Render(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(
                    MoveType.Before,
                    instr => instr.Match(OpCodes.Ldarg_0),
                    instr => instr.Match(OpCodes.Ldfld),
                    instr => instr.Match(OpCodes.Callvirt),
                    instr => instr.Match(OpCodes.Ldarg_0),
                    instr => instr.Match(OpCodes.Ldfld),
                    instr => instr.Match(OpCodes.Brfalse_S)
                    ))
            {
                cursor.EmitDelegate(StartSpriteBatch);
                if (cursor.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Callvirt)))
                {
                    cursor.EmitDelegate(EndSpriteBatch);
                }
            }
        }
        public static void StartSpriteBatch()
        {
            if (Engine.Scene is not Level level) return;
            PortraitRuiner ruiner = level.Tracker.SafeGetEntity<PortraitRuiner>();
            if (ruiner is null)
            {
                return;
            }
            Draw.SpriteBatch.End();

            if (ruiner != null && (ruiner.FlagState(level) || ruiner.ForceVisible))
            {
                ruiner.ApplyParameters(true);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, ruiner.Effect, Matrix.Identity);
            }
        }
        public static void EndSpriteBatch()
        {
            if (Engine.Scene is not Level level) return;
            PortraitRuiner ruiner = level.Tracker.SafeGetEntity<PortraitRuiner>();
            if (ruiner is null)
            {
                return;
            }
            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
        }
        public static void Load()
        {
            IL.Celeste.Textbox.Render += Textbox_Render;
        }

        public static void Unload()
        {
            IL.Celeste.Textbox.Render -= Textbox_Render;
        }

        public static void Apply_HOOK(On.Celeste.Glitch.orig_Apply orig, VirtualRenderTarget source, float timer, float seed, float amplitude)
        {
            orig(source, timer, seed, amplitude);
            Level level = FrostModule.GetCurrentLevel();
            using List<Entity>.Enumerator enumerator = level.Tracker.SafeGetEntities<ShaderOverlay>().GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return;
            }

            ShaderOverlay shaderOverlay = enumerator.Current as ShaderOverlay;
            if (shaderOverlay != null && shaderOverlay.ShouldRender())
            {
                Apply(source, source, shaderOverlay, false);
            }
        }
        public virtual void ApplyParameters(bool identity)
        {
            Level level = FrostModule.GetCurrentLevel();
            Effect.Parameters["DeltaTime"]?.SetValue(Engine.DeltaTime);
            Effect.Parameters["Time"]?.SetValue(Engine.Scene.TimeActive);
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
        public static void Apply(VirtualRenderTarget source, VirtualRenderTarget target, ShaderOverlay overlay, bool clear = false)
        {
            overlay.ApplyParameters();
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

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, overlay.Effect);
            Draw.SpriteBatch.Draw((RenderTarget2D)tempA, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }
        public bool FlagState(Level level)
        {
            return string.IsNullOrEmpty(flag) || level.Session.GetFlag(flag);
        }
    }
}

