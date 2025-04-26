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
        public static Dictionary<string, ObjModel> FallbackModels = [];
        public ObjModel Model;
        public float Roll;
        public float Pitch;
        public float Yaw;
        public Vector2 Scale = Vector2.One;
        public static BasicEffect Effect;
        public MTexture Texture;
        public VirtualRenderTarget testTarget;
        public readonly static RasterizerState MountainRasterizer = new RasterizerState
        {
            CullMode = CullMode.CullClockwiseFace,
            MultiSampleAntiAlias = true
        };
        public readonly static RasterizerState CullNoneRasterizer = new RasterizerState
        {
            CullMode = CullMode.None,
            MultiSampleAntiAlias = true,
            FillMode = FillMode.Solid
        };
        public readonly static RasterizerState CullCCRasterizer = new RasterizerState
        {
            CullMode = CullMode.CullCounterClockwiseFace,
            MultiSampleAntiAlias = false
        };
        public readonly static RasterizerState CullCRasterizer = new RasterizerState
        {
            CullMode = CullMode.CullClockwiseFace,
            MultiSampleAntiAlias = false
        };
        public Model3D(string folderPath, string filePath, Vector2 position, MTexture texture, Vector2 scale, float roll, float pitch, float yaw)
            : this(folderFilePathifyStrings(folderPath, filePath), position, texture, scale, roll, pitch, yaw) { }
        public Model3D(string folderPath, string filePath, Vector2 position, MTexture texture)
            : this(folderFilePathifyStrings(folderPath, filePath), position, texture) { }
        public Model3D(string path, Vector2 position, MTexture texture)
            : this(path, position, texture, Vector2.One, 0, 0, 0) { }
        public Model3D(string folderPath, string filePath, Vector2 position, MTexture texture, Vector2 scale)
           : this(folderFilePathifyStrings(folderPath, filePath), position, texture, scale) { }
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
            testTarget = VirtualContent.CreateRenderTarget("gkshdfgkjsh", 320, 180);
            Add(new BeforeRenderHook(BeforeRender));
        }
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
        public void RenderModel()
        {
            RenderModel(Texture);
        }
        public void RenderModel(Vector2 position)
        {
            RenderModel(Texture, position);
        }
        public void RenderModel(Vector3 position)
        {
            RenderModel(Texture, position);
        }
        public void RenderModel(MTexture texture)
        {
            RenderModel(texture, Position);
        }
        public void RenderModel(MTexture texture, Vector2 position)
        {
            Matrix world =
               Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll)
               * Matrix.CreateScale(Width * Scale.X, Height * Scale.Y, 0)
               * Matrix.CreateTranslation(new Vector3(position, 0));
            Effect.World = world;
            Effect.Texture = texture.Texture.Texture_Safe;
            try
            {
                Model.Draw(Effect);
            }
            catch
            {
                Draw.Rect(Vector2.Zero, 40, 40, Color.Red);
            }
        }
        public void RenderModel(MTexture texture, Vector3 position)
        {
            Matrix world =
               Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll)
               * Matrix.CreateScale(Scale.X, Scale.Y, 0)
               * Matrix.CreateTranslation(position);
            //MTexture tex = GFX.Game["objects/PuzzleIslandHelper/noise"];
            //Engine.Graphics.GraphicsDevice.RasterizerState = CullNoneRasterizer;
            Effect.World = world;
            Effect.Texture = texture.Texture.Texture_Safe;
            RasterizerState state = Engine.Graphics.GraphicsDevice.RasterizerState;
            Engine.Graphics.GraphicsDevice.RasterizerState = CullCRasterizer;
            Model.Draw(Effect);
            Engine.Graphics.GraphicsDevice.RasterizerState = state;
        }
        public static bool TryGetModel(string fullFilePath, out ObjModel model)
        {
            model = null;
            if (string.IsNullOrEmpty(fullFilePath)) return false;
            model = GetModel(fullFilePath);
            return model != null;
        }
        public static ObjModel GetModel(string path)
        {
            if (!FallbackModels.TryGetValue(path, out ObjModel model))
            {
                model = ObjModel.Create(path);
                if (model != null)
                {
                    FallbackModels.Add(path, model);
                }
            }
            return model;
        }
        private static string folderFilePathifyStrings(string folderPath, string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return folderPath;

            if (!filePath.EndsWith(".obj")) folderPath += ".obj";
            string fileName = Path.Combine(Engine.AssemblyDirectory, folderPath.TrimEnd('/', '\\').Replace('/', '\\'), filePath);
            return fileName;
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
            foreach (var pair in FallbackModels)
            {
                pair.Value?.Dispose();
            }
            FallbackModels.Clear();
            Effect?.Dispose();
            Effect = null;
        }
    }
    [CustomEntity("PuzzleIslandHelper/WarpNode3D")]
    [TrackedAs(typeof(Model3D))]
    public class WarpNode3D : Model3D
    {
        private RasterizerState[] states =
            [null, CullCCRasterizer, CullCRasterizer, CullNoneRasterizer, MountainRasterizer];
        private int stateIndex = 0;
        public WarpNode3D(EntityData data, Vector2 offset) : base("Mods\\PuzzleIslandHelper\\Models\\WarpNode.obj", data.Position + offset, GFX.Game["objects/PuzzleIslandHelper/testuv"])
        {
            Collider = new Hitbox(16, 16);
            Depth = -1000000;
            Add(new DebugComponent(Keys.Up, delegate { stateIndex = (stateIndex + 1) % states.Length; }, true));
            Add(new DebugComponent(Keys.Down, delegate
            {
                if (stateIndex - 1 < 0) stateIndex = states.Length - 1;
                else stateIndex--;
            }, true));
        }
        public override void BeforeRender()
        {
            var state = Engine.Graphics.GraphicsDevice.RasterizerState;
            Engine.Graphics.GraphicsDevice.RasterizerState.CullMode = CullMode.None;
            base.BeforeRender();
            Engine.Graphics.GraphicsDevice.RasterizerState = state;
        }
        public override void Update()
        {
            base.Update();
            Pitch += Engine.DeltaTime * 2;
            Roll += Engine.DeltaTime;
        }
    }
}
