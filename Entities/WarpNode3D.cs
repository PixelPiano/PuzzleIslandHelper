using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/WarpNode3D")]
    [Tracked]
    public class WarpNode3D : Entity
    {
        public static ObjModel Model;
        public VirtualRenderTarget Target;
        private float roll;
        private float pitch;
        private float yaw;
        public static BasicEffect Effect;
        public static RasterizerState MountainRasterizer = new RasterizerState
        {
            CullMode = CullMode.CullClockwiseFace,
            MultiSampleAntiAlias = true
        };

        public static RasterizerState CullNoneRasterizer = new RasterizerState
        {
            CullMode = CullMode.None,
            MultiSampleAntiAlias = true,
            FillMode = FillMode.WireFrame
        };

        public static RasterizerState CullCCRasterizer = new RasterizerState
        {
            CullMode = CullMode.CullCounterClockwiseFace,
            MultiSampleAntiAlias = false
        };

        public static RasterizerState CullCRasterizer = new RasterizerState
        {
            CullMode = CullMode.CullClockwiseFace,
            MultiSampleAntiAlias = false
        };
        public WarpNode3D(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(16, 16);
            Depth = -1000000;
            Target = VirtualContent.CreateRenderTarget("Test", 320, 180);
            Add(new BeforeRenderHook(BeforeRender));
        }
        [OnInitialize]
        public static void Load()
        {
            Model = ObjModel.Create(Path.Combine(Engine.AssemblyDirectory, "Mods\\PuzzleIslandHelper\\Models", "WarpNode.obj"));
            Effect = new(Engine.Graphics.GraphicsDevice)
            {
                TextureEnabled = true,
                View = Matrix.CreateLookAt(new(0, 0, 160), Vector3.Zero, Vector3.Up),
                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), Engine.Viewport.AspectRatio, 0.1f, 1000f),
            };
        }
        [OnUnload]
        public static void Unload()
        {
            Effect?.Dispose();
            Effect = null;
            Model?.Dispose();
            Model = null;
        }
        public void BeforeRender()
        {
            Target.SetAsTarget(true);
            Matrix translation = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) * Matrix.CreateScale(16, 16, 0);
            TestDraw(translation);
        }
        public void TestDraw(Matrix matrix)
        {
            MTexture tex = GFX.Game["objects/PuzzleIslandHelper/noise"];
            Texture prev = Engine.Graphics.GraphicsDevice.Textures[0];
            //Engine.Graphics.GraphicsDevice.RasterizerState = CullNoneRasterizer;
            Effect.World = matrix;
            Effect.Texture = tex.Texture.Texture_Safe;
            Model.Draw(Effect);
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level) return;
            Draw.SpriteBatch.Draw(Target, SceneAs<Level>().Camera.Position, Color.White);
        }
        public override void Update()
        {
            base.Update();
            pitch += Engine.DeltaTime * 2;
            roll += Engine.DeltaTime;
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target?.Dispose();
            Target = null;
        }
    }
}
