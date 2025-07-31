using Celeste.Mod.Entities;
using FrostHelper;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.IO;
using static Celeste.Mod.PuzzleIslandHelper.Entities.InvertAuth;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ShaderOverlay")]
    [Tracked]
    public class ShaderOverlay : Entity
    {
        public static List<string> DefaultShaderPaths = [];
        private static int debugShaderPathIndex;
        public static ShaderOverlay DebugOverlay;
        private readonly FlagList flag;
        public Effect Effect;
        private string path;
        private bool fullpath;
        public float Alpha = 1;
        public bool ForceLevelRender;
        public float Amplitude;
        public bool UsesFlag;
        public bool UseRawDeltaTime;
        private float dummyTimer;
        public bool UseIdentityMatrix = true;
        public bool FlagState => flag;
        public ShaderOverlay(string path, string flag = "", bool forceRender = false, float alpha = 1, bool fullpath = true)
            : this(ShaderHelper.TryGetEffect(path, fullpath), flag, forceRender, alpha)
        {
            this.fullpath = fullpath;
            this.path = path;
        }
        public ShaderOverlay(Effect effect, string flag = "", bool forceRender = false, float alpha = 1) : base()
        {
            Effect = effect;
            this.flag = new FlagList(flag);
            Alpha = alpha;
            ForceLevelRender = forceRender;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!string.IsNullOrEmpty(path) && Effect == null)
            {
                Effect = ShaderHelper.TryGetEffect(path, fullpath);
            }
        }
        public virtual bool ShouldRender => (ForceLevelRender || FlagState) && Effect != null && Effect.Parameters != null;
        public virtual void AfterApply()
        {

        }
        public virtual void BeforeApply()
        {

        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Effect?.Dispose();
            Effect = null;
        }
        public virtual void EffectRender()
        {

        }
        public void ParamBool(string name, bool value)
        {
            if (Effect != null && Effect.Parameters != null)
            {
                Effect.Parameters[name]?.SetValue(value);
            }
        }
        public void ParamInt(string name, int value)
        {
            if (Effect != null && Effect.Parameters != null)
            {
                Effect.Parameters[name]?.SetValue(value);
            }
        }
        public void ParamFloat(string name, float value)
        {
            if (Effect != null && Effect.Parameters != null)
            {
                Effect.Parameters[name]?.SetValue(value);
            }
        }
        public void ParamVector2(string name, Vector2 value)
        {
            if (Effect != null && Effect.Parameters != null)
            {
                Effect.Parameters[name]?.SetValue(value);
            }
        }
        private bool TryApplyParameters()
        {
            if (Scene is not Level level || Effect is null || Effect.Parameters == null) return false;
            else ApplyParameters(level);
            return true;
        }
        public void ApplyParameters()
        {
            ApplyParameters(SceneAs<Level>());
        }
        public virtual void ApplyParameters(Level level)
        {
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
            Effect.Parameters["ViewMatrix"]?.SetValue(UseIdentityMatrix ? Matrix.Identity : level.Camera.Matrix);
        }
        [Tracked]
        private class shaderTitle : Entity
        {
            private string name;
            public ShaderOverlay overlay;
            public shaderTitle(string name, ShaderOverlay overlay) : base()
            {
                this.overlay = overlay;
                this.name = name;
                Tag |= TagsExt.SubHUD;
            }
            public override void Render()
            {
                base.Render();
                ActiveFont.DrawOutline(name, Vector2.Zero, Vector2.Zero, Vector2.One, Color.White, 5, Color.Black);
                if (overlay != null)
                {
                    ActiveFont.DrawOutline($"Amplitude: {overlay.Amplitude}", Vector2.UnitY * (ActiveFont.LineHeight + 20), Vector2.Zero, Vector2.One, Color.Red, 5, Color.Black);
                }
            }
        }
        [Tracked]
        private class debugShaderController : Entity
        {
            private float timer;
            public debugShaderController() : base() { Tag |= Tags.Persistent; }
            public override void Update()
            {
                base.Update();
                Scene.GetPlayer().DisableMovement();
                if (timer <= 0)
                {
                    if (Input.MoveX == 1)
                    {
                        NextShader();
                        timer = 0.3f;
                    }
                    else if (Input.MoveX == -1)
                    {
                        PrevShader();
                        timer = 0.3f;
                    }
                }
                if (Input.MoveY.Value != 0 && DebugOverlay != null)
                {
                    DebugOverlay.Amplitude -= Input.MoveY.Value * Engine.DeltaTime * 2;
                }
                timer = Calc.Max(timer - Engine.DeltaTime, 0);
            }
        }
        [Command("next_shader", "")]
        public static void NextShader(string flag = "", bool forceRender = false, float alpha = 1)
        {
            debugShaderPathIndex = debugShaderPathIndex.Wrap(DefaultShaderPaths, 1);
            reloadDebugShader(flag, forceRender, alpha);
        }
        [Command("prev_shader", "")]
        public static void PrevShader(string flag = "", bool forceRender = false, float alpha = 1)
        {
            debugShaderPathIndex = debugShaderPathIndex.Wrap(DefaultShaderPaths, -1);
            reloadDebugShader(flag, forceRender, alpha);
        }
        private static void reloadDebugShader(string flag = "", bool forceRender = false, float alpha = 1)
        {
            DebugShader(DefaultShaderPaths[debugShaderPathIndex], flag, forceRender, alpha);
        }
        [Command("debug_shader", "Lets you cycle through shaders located in or in a sub folder of DebugShaderPath (PuzzleIslandHelper mod settings).\nUse arrow keys to change shaders/adjust the Amplitude of the shader (provided it has that value).")]
        public static void DebugShader(string path, string flag = "", bool forceRender = false, float alpha = 1)
        {
            if (string.IsNullOrEmpty(path))
            {
                NextShader(flag, forceRender, alpha);
                return;
            }
            if (Engine.Scene != null)
            {
                DebugOverlay?.RemoveSelf();
                DebugOverlay = null;
                Engine.Scene.Add(DebugOverlay = new ShaderOverlay(path, flag, forceRender, alpha, false));
                foreach (shaderTitle title in Engine.Scene.Tracker.GetEntities<shaderTitle>())
                {
                    title.RemoveSelf();
                }
                if (Engine.Scene.Tracker.GetEntity<debugShaderController>() == null)
                {
                    Engine.Scene.Add(new debugShaderController());
                }
                shaderTitle title2 = new(path, DebugOverlay);
                Engine.Scene.Add(title2);
                DebugOverlay.AddTag(Tags.Persistent);
            }
        }
        [Command("stop_debug_shader", "")]
        public static void StopDebugShader()
        {
            DebugOverlay?.RemoveSelf();
            DebugOverlay = null;
            foreach (shaderTitle title in Engine.Scene.Tracker.GetEntities<shaderTitle>())
            {
                title.RemoveSelf();
            }
            foreach (debugShaderController controller in Engine.Scene.Tracker.GetEntities<debugShaderController>())
            {
                controller.RemoveSelf();
            }
            Engine.Scene.GetPlayer()?.EnableMovement();
        }
        private static string DebugShaderPath
        {
            get
            {
                foreach (string s in Directory.GetDirectories(Engine.AssemblyDirectory + "\\Mods"))
                {
                    string p = s + "\\Effects\\" + PianoModule.Settings.DebugShaderFolder;
                    if (Directory.Exists(p)) return p;
                    if (Directory.Exists(p + ".zip")) return p + ".zip";
                }
                return null;
            }
        }
        [OnLoad]
        public static void Load()
        {
            DebugOverlay = null;
            //Note: don't add onContentUpdate, it messes with things. Just use runCompiler.bat
            On.Celeste.Glitch.Apply += Apply_HOOK;
            ReloadShaderPaths();
        }
        [Command("reload_shaders", "")]
        public static void ReloadShaderPaths()
        {
            DefaultShaderPaths.Clear();
            string path = DebugShaderPath;
            if (!string.IsNullOrEmpty(path))
            {
                if (!Directory.Exists(path)) return;
                void checkAllIn(string path2)
                {
                    if (Directory.Exists(path2))
                    {
                        try
                        {
                            string[] array = Directory.GetDirectories(path2);
                            if (array != null)
                            {
                                foreach (string s in array)
                                {
                                    checkAllIn(s);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Engine.Commands.Log("Error looking through directories:" + e.Message);
                        }
                        try
                        {
                            string[] files = Directory.GetFiles(path2, "*.cso");
                            if (files != null)
                            {
                                foreach (string s in files)
                                {
                                    string name = Path.GetFileName(s);
                                    if (!DefaultShaderPaths.Contains(name))
                                    {
                                        DefaultShaderPaths.Add(name);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Engine.Commands.Log("Error looking through files:" + e.Message);
                        }
                    }
                }
                checkAllIn(path);
            }
        }
        [OnUnload]
        public static void Unload()
        {
            DebugOverlay = null;
            DefaultShaderPaths.Clear();
            On.Celeste.Glitch.Apply -= Apply_HOOK;

        }
        public static void Apply_HOOK(On.Celeste.Glitch.orig_Apply orig, VirtualRenderTarget source, float timer, float seed, float amplitude)
        {
            orig(source, timer, seed, amplitude);
            Level level = Engine.Scene as Level;
            if (level == null) return;
            List<Entity> overlays = level.Tracker.GetEntities<ShaderOverlay>();
            foreach (ShaderOverlay so in overlays)
            {
                if (so != null && so.ShouldRender)
                {
                    so.BeforeApply();
                }
            }
            foreach (ShaderOverlay so in overlays)
            {
                if (so != null && so.ShouldRender)
                {
                    Apply(source, source, so, false, so.Alpha);
                }
            }
            foreach (ShaderOverlay so in overlays)
            {
                if (so != null && so.ShouldRender)
                {
                    so.AfterApply();
                }
            }
        }
        public static void Apply(VirtualRenderTarget source, VirtualRenderTarget target, ShaderOverlay overlay = null, bool clear = false, float alpha = 1)
        {
            if (source is null || target is null || !overlay.TryApplyParameters())
            {
                return;
            }

            VirtualRenderTarget tempA = GameplayBuffers.TempA;
            Engine.Instance.GraphicsDevice.SetRenderTarget(tempA);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            Engine.Instance.GraphicsDevice.Clear(Color.Transparent);

            Draw.SpriteBatch.Draw((RenderTarget2D)source, Vector2.Zero, Color.White);
            overlay?.EffectRender();
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
            if (source is null || target is null || !overlay.TryApplyParameters())
            {
                return;
            }
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
            if (source is null || target is null || !overlay.TryApplyParameters())
            {
                return;
            }
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
    }
}

