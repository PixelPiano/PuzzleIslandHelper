using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System;
using System.Collections.Generic;
using FrostHelper.ModIntegration;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class Model3D : Entity
    {
        public static Dictionary<string, ObjModel> CreatedModels = [];
        public static BasicEffect Effect;
        public ObjModel Model;
        public float Roll;
        public float Pitch;
        public float Yaw;
        public Vector2 Scale = Vector2.One;
        public MTexture Texture;
        public VirtualRenderTarget testTarget;
        public Model3D(string path, Vector2 position, MTexture texture)
            : this(path, position, texture, Vector2.One, 0, 0, 0) { }
        public Model3D(string path, Vector2 position, MTexture texture, Vector2 scale)
            : this(path, position, texture, scale, 0, 0, 0) { }
        public Model3D(string path, Vector2 position, MTexture texture, Vector2 scale, float roll, float pitch, float yaw) : base(position)
        {
            TryGetModel(path, out Model);
            Texture = texture;
            Scale = scale;
            Roll = roll;
            Pitch = pitch;
            Yaw = yaw;
            Collider = new Hitbox(16, 16);
            testTarget = VirtualContent.CreateRenderTarget("gkshdfgkjsh", 320, 180, true, false);
            Add(new BeforeRenderHook(BeforeRender));
        }
        private int bbbb;
        public virtual void BeforeRender()
        {
            testTarget.SetAsTarget(true);
            RenderModel(Vector2.Zero);

        }
        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Draw(testTarget, SceneAs<Level>().Camera.Position, Color.White);

        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Model?.Dispose();
            Model = null;
            testTarget?.Dispose();
            testTarget = null;
        }
        public void RenderModel(Vector2 position)
        {
            RenderModel(Texture, position);
        }
        public void RenderModel(Vector3 position)
        {
            RenderModel(Texture, position);
        }
        public void RenderModel(MTexture texture, Vector2 position)
        {
            RenderModel(texture, new Vector3(position, 0));
        }
        public void RenderModel(MTexture texture, Vector3 position)
        {
            Matrix world =
               Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll)
               * Matrix.CreateScale(Width * Scale.X, Height * Scale.Y, 1)
               * Matrix.CreateTranslation(position);
            Effect.World = world;
            Texture2D tex = texture.Texture.Texture_Safe;
            Effect.Texture = tex;

            var d = Engine.Graphics.GraphicsDevice.DepthStencilState;
            var r = Engine.Graphics.GraphicsDevice.RasterizerState;
            var b = Engine.Graphics.GraphicsDevice.BlendState;
            Engine.Graphics.GraphicsDevice.RasterizerState = MountainModel.MountainRasterizer;
            Engine.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Engine.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Model.Draw(Effect);
            Engine.Graphics.GraphicsDevice.DepthStencilState = d;
            Engine.Graphics.GraphicsDevice.RasterizerState = r;
            Engine.Graphics.GraphicsDevice.BlendState = b;
        }
        public static bool TryGetModel(string path, out ObjModel model)
        {
            model = null;
            if (string.IsNullOrEmpty(path)) return false;
            model = GetModel(path);
            return model != null;
        }
        public static ObjModel GetModel(string path)
        {
            if (!CreatedModels.TryGetValue(path, out ObjModel model))
            {
                if (Everest.Content.TryGet(path, out ModAsset metadata))
                {
                    model = ObjModel.CreateFromStream(metadata.Stream, path);
                    if (model != null)
                    {
                        CreatedModels.Add(path, model);
                    }
                }
            }
            return model;
        }
        [OnInitialize]
        public static void Initialize()
        {
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
            foreach (var pair in CreatedModels)
            {
                pair.Value?.Dispose();
            }
            CreatedModels.Clear();
            Effect?.Dispose();
            Effect = null;
        }
    }
    [CustomEntity("PuzzleIslandHelper/WarpNode3D")]
    [TrackedAs(typeof(Model3D))]
    public class WarpNode3D : Model3D
    {
        public WarpNode3D(EntityData data, Vector2 offset) : base("Models/PuzzleIslandHelper/WarpNode", data.Position + offset, GFX.Game["objects/PuzzleIslandHelper/testuv"])
        {
            Collider = new Hitbox(16, 16);
            Depth = -1000000;
        }
        public override void BeforeRender()
        {
            base.BeforeRender();
        }
        public override void Update()
        {
            base.Update();
            //Pitch += Engine.DeltaTime * 3;
            //Roll += Engine.DeltaTime;
            Yaw += Engine.DeltaTime;
        }
    }
}
