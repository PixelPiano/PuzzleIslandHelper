using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;

// PuzzleIslandHelper.VoidCritters
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/VoidCritterWall")]
    [Tracked]
    public class VoidCritterWall : Entity
    {
        private bool InLight;
        public bool OnScreen;
        public static Effect Shader;
        public VirtualRenderTarget Target;
        public VirtualRenderTarget Light;
        private VoidCritterWallHelper Helper;
        public HashSet<CritterLight> Colliding = new();
        public static Vector2 Offset = Vector2.Zero;
        public EntityID ID;
        public bool Simple => VoidCritterWallHelper.Simple;
        public VoidCritterWall(EntityData data, Vector2 offset, EntityID id)
        : base(data.Position + offset)
        {
            ID = id;
            Tag |= Tags.TransitionUpdate;
            Collider = new Hitbox(data.Width, data.Height);
            Target = VirtualContent.CreateRenderTarget("voidCritterWallTarget", data.Width + (int)Offset.X * 2, data.Height + (int)Offset.Y * 2);
            Light = VirtualContent.CreateRenderTarget("voidCritterLightTarget", data.Width + (int)Offset.X * 2, data.Height + (int)Offset.Y * 2);
            Add(new BeforeRenderHook(BeforeRender));

        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (!Simple)
            {
                Draw.SpriteBatch.Draw(Light, Position - Offset, Color.Red * 0.5f);
            }
        }
        public void BeforeRender()
        {
            Light.SetRenderTarget(Color.Transparent);
            if (Scene is not Level level || Simple) return;
            Draw.SpriteBatch.StandardBegin();
            {
                Draw.SpriteBatch.Draw(VoidCritterWallHelper.Lights, level.Camera.Position - Position - Offset, Color.White);
            }
            Draw.SpriteBatch.End();
            Target.SetRenderTarget(Color.Black);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Helper = scene.Tracker.GetEntity<VoidCritterWallHelper>();
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level || !OnScreen || Simple) return;
            Effect effect = ShaderHelperIntegration.GetEffect("PuzzleIslandHelper/Shaders/voidCritterWall");
            if (effect != null)
            {
                Draw.SpriteBatch.End();
                effect.ApplyCameraParams(level);
                effect.Parameters["lights_texture"]?.SetValue(Light);
                Draw.SpriteBatch.StandardBegin(effect, level.Camera.Matrix);
                Draw.SpriteBatch.Draw(Target, Position - Offset, Color.White);
                Draw.SpriteBatch.End();
                GameplayRenderer.Begin();
            }

        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level)
            {
                OnScreen = false;
                return;
            }
            if (level.GetPlayer() is not Player player) return;
            Rectangle c = level.Camera.GetBounds();
            Rectangle r = new Rectangle(c.Left - 8, c.Top - 8, c.Width + 16, c.Height + 16);
            OnScreen = Collider.Bounds.Colliding(r);
            Colliding.Clear();
            foreach (CritterLight cl in level.Tracker.GetComponents<CritterLight>())
            {
                if (cl.Colliding(this))
                {
                    Colliding.Add(cl);
                }
            }
            if (!Helper.PlayerSafe)
            {
                player.Die(Vector2.Normalize(Center - player.Center));
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target?.Dispose();
            Target = null;
            Light?.Dispose();
            Light = null;
        }
        [OnInitialize]
        public static void Initialize()
        {
            Shader = ShaderHelper.TryGetEffect("voidCritterWall");
        }
        [OnUnload]
        public static void Unload()
        {
            Shader?.Dispose();
            Shader = null;
        }
    }
}