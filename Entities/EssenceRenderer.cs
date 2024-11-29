
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Structs;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class EssenceRenderer : Entity
    {
        private static VirtualRenderTarget _target;
        public static VirtualRenderTarget Target => _target ??= VirtualContent.CreateRenderTarget("EssenceRendererTarget", 320, 180);
        public Effect Shader;
        private Vector2[] positions;
        private Vector2[] radiuses;
        private bool[] inUse;
        private int breakOffIndex;
        public EssenceRenderer() : base()
        {
            Shader = ShaderHelperIntegration.GetEffect("PuzzleIslandHelper/Shaders/essenceShader");
            Tag |= Tags.Global | Tags.TransitionUpdate;
            Depth = -100000;
            Add(new BeforeRenderHook(BeforeRender));
        }
        public override void Update()
        {
            base.Update();
            int index = breakOffIndex = 0;
            for (int i = 0; i < 64; i++)
            {
                inUse[i] = false;
            }
            foreach (MemoryEssence essence in SceneAs<Level>().Tracker.GetEntities<MemoryEssence>())
            {
                if (index >= 64) break;
                if (essence.OnScreen && essence.Active)
                {
                    positions[index] = essence.UVPosition;
                    radiuses[index] = essence.UVRadius;
                    inUse[index] = true;
                    breakOffIndex = index;
                }
                index++;
            }
        }
        public void ApplyParameters(Level level)
        {
            Shader.ApplyCameraParams(level);
            Shader.Parameters["Positions"]?.SetValue(positions);
            Shader.Parameters["Radiuses"]?.SetValue(radiuses);
            Shader.Parameters["InUse"]?.SetValue(inUse);
            Shader.Parameters["BreakOffIndex"]?.SetValue(breakOffIndex);
        }
        public void BeforeRender()
        {
            Target.SetAsTarget(Color.White);
        }
        public static Color DebugColor = Color.Magenta;
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level || breakOffIndex == 0) return;
            //Draw.Rect(level.Camera.Position, 16, 16, Color.Red);
            Shader = ShaderHelperIntegration.GetEffect("PuzzleIslandHelper/Shaders/essenceShader");
            ApplyParameters(level);
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.StandardBegin(level.Camera.Matrix, Shader);
            Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);
            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
        }
        public static MemoryEssence Add(Vector2 position, float direction, float directionRange, float radius, Vector2 accel, float friction, Range lifeRange, Range speedRange)
        {
            if (Engine.Scene is Level level)
            {
                float num = direction - directionRange / 2f + Calc.Random.NextFloat() * directionRange;
                MemoryEssence essence = new MemoryEssence(position, Calc.AngleToVector(num, speedRange.Random()), lifeRange.Random(), radius, accel, friction);
                level.Add(essence);
                return essence;
            }
            return null;
        }
        public override void SceneBegin(Scene scene)
        {
            base.SceneBegin(scene);
            Initialize();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Dispose();
        }
        public void Dispose()
        {
            positions = null;
            radiuses = null;
            inUse = null;
            breakOffIndex = 64;
            Shader?.Dispose();
            Shader = null;
            _target?.Dispose();
            _target = null;
        }
        public void Initialize()
        {
            positions = new Vector2[64];
            radiuses = new Vector2[64];
            inUse = new bool[64];
        }
        [OnLoad]
        public static void Load()
        {
            Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
        }

        [OnUnload]
        public static void Unload()
        {
            Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
        }
        private static void LevelLoader_OnLoadingThread(Level level)
        {
            EssenceRenderer r = new EssenceRenderer();
            level.Add(r);
        }
    }
}