using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using YamlDotNet.Core.Tokens;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Obsolete()]
    public class ShaderComponent : Component
    {
        public string Path
        {
            get => path;
            set
            {
                string prev = path;
                path = value;
                if (path != prev && AddedToRenderer)
                {
                    if (Renderer.GetShader(prev) is ShaderComponentRenderer.Shader shader)
                    {
                        shader.Components.Remove(this);
                    }
                    Renderer.AddMember(this);
                }
            }
        }
        private string path;
        public Dictionary<string, int> ParametersInt = [];
        public Dictionary<string, float> ParametersFloat = [];
        public Dictionary<string, bool> ParametersBool = [];
        public Dictionary<string, Vector2> ParametersVector2 = [];
        public Dictionary<string, Vector3> ParametersVector3 = [];
        public Dictionary<string, Vector4> ParametersVector4 = [];
        public Dictionary<string, Matrix> ParametersMatrix = [];
        public float Amplitude;
        public ShaderComponentRenderer Renderer;
        public bool AddedToRenderer;
        public Action UpdateParameters;
        public Action RenderToTarget;
        public ShaderComponent(string path, Action render, float amplitude = 0) : base(true, true)
        {
            this.path = path;
            Amplitude = amplitude;
            RenderToTarget = render;
        }
        public override void Update()
        {
            base.Update();
            if (Renderer == null && Scene != null)
            {
                Renderer = Scene.Tracker.GetEntity<ShaderComponentRenderer>();
            }
            UpdateParameters?.Invoke();
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            if (entity.Scene != null)
            {
                Renderer = entity.Scene.Tracker.GetEntity<ShaderComponentRenderer>();
                Renderer?.AddMember(this);
            }
        }
        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            if (!AddedToRenderer)
            {
                Renderer = scene.Tracker.GetEntity<ShaderComponentRenderer>();
                Renderer?.AddMember(this);
            }
        }
        public override void Removed(Entity entity)
        {
            ShaderComponentRenderer renderer = Renderer ?? entity.Scene.Tracker.GetEntity<ShaderComponentRenderer>();
            renderer?.RemoveMember(this);
            base.Removed(entity);

        }
    }
    //WIP
    //[ConstantEntity("PuzzleIslandHelper/ShaderComponentRenderer")]
    [Tracked]
    [Obsolete]
    public class ShaderComponentRenderer : Entity
    {
        public class Shader
        {
            public readonly string Path;
            public readonly Effect Effect;
            public List<ShaderComponent> Components = [];
            public VirtualRenderTarget Target;
            public Color? ClearColor = Color.Transparent;
            public Shader(string path, params ShaderComponent[] components)
            {
                Path = path;
                Effect = ShaderHelper.TryGetEffect(Path, true) ?? ShaderHelper.TryGetEffect(Path, false);
                Target = VirtualContent.CreateRenderTarget("ShaderTarget{" + path + '}', 320, 180);
                if (components != null)
                {
                    Components = [.. components];
                }
            }
            public void Dispose()
            {
                Target?.Dispose();
                Effect?.Dispose();
            }
            public void ApplyAdditionalParameters(ShaderComponent component)
            {
                if (Effect != null)
                {
                    EffectParameterCollection parameters = Effect.Parameters;
                    if (component.ParametersInt.Count > 0)
                    {
                        foreach (var pair in component.ParametersInt)
                        {
                            parameters[pair.Key]?.SetValue(pair.Value);
                        }
                    }
                    if (component.ParametersFloat.Count > 0)
                    {
                        foreach (var pair in component.ParametersFloat)
                        {
                            parameters[pair.Key]?.SetValue(pair.Value);
                        }
                    }
                    if (component.ParametersBool.Count > 0)
                    {
                        foreach (var pair in component.ParametersBool)
                        {
                            parameters[pair.Key]?.SetValue(pair.Value);
                        }
                    }
                    if (component.ParametersVector2.Count > 0)
                    {
                        foreach (var pair in component.ParametersVector2)
                        {
                            parameters[pair.Key]?.SetValue(pair.Value);
                        }
                    }
                    if (component.ParametersVector3.Count > 0)
                    {
                        foreach (var pair in component.ParametersVector3)
                        {
                            parameters[pair.Key]?.SetValue(pair.Value);
                        }
                    }
                    if (component.ParametersVector4.Count > 0)
                    {
                        foreach (var pair in component.ParametersVector4)
                        {
                            parameters[pair.Key]?.SetValue(pair.Value);
                        }
                    }
                    parameters["Amplitude"]?.SetValue(component.Amplitude);
                }
            }
            public void BeforeRender(Level level)
            {
                Vector2 position = level.Camera.Position;
                if (Effect != null)
                {
                    Target.SetAsTarget(ClearColor);
                    Draw.SpriteBatch.Begin();
                    foreach (ShaderComponent c in Components)
                    {
                        c.RenderToTarget?.Invoke();
                    }
                    Draw.SpriteBatch.End();

                    Effect.ApplyIdentityParameters(level, 0);
                    foreach (ShaderComponent c in Components)
                    {
                        ApplyAdditionalParameters(c);
                    }
                }
            }
        }
        public static List<Shader> Shaders = [];
        public static float Value = 1;

        public ShaderComponentRenderer() : base()
        {
            Tag |= Tags.Global | Tags.TransitionUpdate;
            Depth = int.MinValue;
            Add(new BeforeRenderHook(() =>
            {
                if (Scene is Level level)
                {
                    foreach (Shader shader in Shaders)
                    {
                        shader.BeforeRender(level);
                    }
                }
            }));
        }
        public override void Render()
        {
            base.Render();
            Camera c = SceneAs<Level>().Camera;
            Vector2 position = c.Position;
            foreach (Shader shader in Shaders)
            {
                Draw.SpriteBatch.Draw(shader.Target, position, Color.White);
            }
        }
        public void RemoveMember(ShaderComponent component)
        {
            if (!string.IsNullOrEmpty(component.Path) && GetShader(component.Path) is Shader shader)
            {
                shader.Components.Remove(component);
            }
            else
            {
                foreach (Shader s in Shaders)
                {
                    if (!s.Components.Contains(component))
                    {
                        continue;
                    }
                    s.Components.Remove(component);
                    break;
                }
            }
        }
        public void AddMember(ShaderComponent component)
        {
            if (GetShader(component.Path) is Shader shader)
            {
                shader.Components.Add(component);
            }
            else
            {
                Shaders.Add(new Shader(component.Path, component));
            }
            component.Renderer = this;
            component.AddedToRenderer = true;
        }
        public Shader GetShader(string path)
        {
            return Shaders.Find(item => item.Path == path);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (Shader s in Shaders)
            {
                s.Dispose();
            }
            Shaders.Clear();
        }
        [OnLoad]
        public static void Load()
        {
            On.Celeste.Glitch.Apply += Apply;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.Glitch.Apply -= Apply;
        }
        public static void Apply(On.Celeste.Glitch.orig_Apply orig, VirtualRenderTarget source, float timer, float seed, float amplitude)
        {
            if (Value > 0)
            {
                foreach (Shader shader in Shaders)
                {
                    Effect effect = shader.Effect;
                    VirtualRenderTarget tempA = GameplayBuffers.TempA;
                    Engine.Instance.GraphicsDevice.SetRenderTarget(tempA);
                    Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, effect);
                    Draw.SpriteBatch.Draw((RenderTarget2D)source, Vector2.Zero, Color.White);
                    Draw.SpriteBatch.End();
                    Engine.Instance.GraphicsDevice.SetRenderTarget(source);
                    Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
                    Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, effect);
                    Draw.SpriteBatch.Draw((RenderTarget2D)tempA, Vector2.Zero, Color.White);
                    Draw.SpriteBatch.End();
                }
            }
            orig(source, timer, seed, amplitude);
        }
    }
}