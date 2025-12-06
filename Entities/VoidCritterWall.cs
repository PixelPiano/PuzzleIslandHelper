using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

// PuzzleIslandHelper.VoidCritters
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/VoidCritterWall")]
    [Tracked]
    public class VoidCritterWall : Entity
    {
        public bool OnScreen;
        public VirtualRenderTarget Target;
        public VirtualRenderTarget Light;
        public static Vector2 Offset = Vector2.One * 2;
        public EntityID ID;
        public bool Simple => VoidCritterWallHelper.Simple;
        public FlagList Flag;
        public VoidCritterWall(EntityData data, Vector2 offset, EntityID id)
        : base(data.Position + offset)
        {
            ID = id;
            Tag |= Tags.TransitionUpdate;
            Collider = new Hitbox(data.Width, data.Height);
            Target = VirtualContent.CreateRenderTarget("voidCritterWallTarget", data.Width + (int)Offset.X * 2, data.Height + (int)Offset.Y * 2);
            Light = VirtualContent.CreateRenderTarget("voidCritterLightTarget", data.Width + (int)Offset.X * 2, data.Height + (int)Offset.Y * 2);
            Add(new BeforeRenderHook(BeforeRender));
            Flag = data.FlagList("flag");
            Flag.Inverted = data.Bool("inverted");
            Depth = data.Int("depth");
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
            Light.SetAsTarget(Color.Transparent);
            if (Scene is not Level level || Simple) return;
            Draw.SpriteBatch.StandardBegin();
            {
                Draw.SpriteBatch.Draw(VoidCritterWallHelper.Lights, level.Camera.Position - Position - Offset, Color.White);
            }
            Draw.SpriteBatch.End();
            Target.SetAsTarget(Color.Black);
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level || !OnScreen || Simple || !Flag) return;
            Effect effect = ShaderHelperIntegration.GetEffect("PuzzleIslandHelper/Shaders/voidCritterWall");
            if (effect != null)
            {
                Draw.SpriteBatch.End();
                effect.ApplyCameraParams(level);
                effect.Parameters["Dimensions"]?.SetValue(Collider.Size);
                Engine.Graphics.GraphicsDevice.Textures[1] = Light.Target;
                Draw.SpriteBatch.StandardBegin(level.Camera.Matrix, effect);
                Draw.SpriteBatch.Draw(Target, Position - Offset, Color.White);
                Draw.SpriteBatch.End();
                GameplayRenderer.Begin();
            }

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (PianoUtils.SeekController<VoidCritterWallHelper>(scene) == null)
            {
                scene.Add(new VoidCritterWallHelper());
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
            OnScreen = Collider.Bounds.Colliding(level.Camera.GetBounds(), 16);
            if (!Flag) return;
            foreach (CritterLight cl in level.Tracker.GetComponents<CritterLight>())
            {
                if (cl.Colliding(this))
                {
                    cl.CollidedWall = this;
                }
            }
        }
        public override void Removed(Scene scene)
        {
            if (scene.Tracker.GetEntities<VoidCritterWall>().Count == 1)
            {
                foreach (VoidCritterWallHelper helper in scene.Tracker.GetEntities<VoidCritterWallHelper>())
                {
                    helper.RemoveSelf();
                }
            }
            base.Removed(scene);
            Target?.Dispose();
            Target = null;
            Light?.Dispose();
            Light = null;
        }
    }
}