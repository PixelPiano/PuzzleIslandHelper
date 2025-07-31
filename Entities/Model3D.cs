using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System;
using System.Collections.Generic;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class Model3D : Entity
    {
        public static Dictionary<string, ObjModel> CreatedModels = [];
        public static BasicEffect Effect;
        public ObjModel Model
        {
            get => _model;
            set
            {
                _model = value;
                if (_model != null)
                {
                    float min = float.PositiveInfinity;
                    float max = float.NegativeInfinity;

                    float[] maxPoint = [max, max, max];
                    float[] minPoint = [min, min, min];

                    Vector3 o = Vector3.Zero;
                    Vector3 e = Vector3.Zero;
                    foreach (var v in _model.verts)
                    {
                        float[] boundingBoxVectors = new float[3];
                        boundingBoxVectors[0] = v.Position.X;
                        boundingBoxVectors[1] = v.Position.Y;
                        boundingBoxVectors[2] = v.Position.Z;
                        for (int vec = 0; vec < 3; vec++)
                        {
                            if (boundingBoxVectors[vec] > maxPoint[vec])
                            {
                                maxPoint[vec] = boundingBoxVectors[vec];
                            }
                            if (boundingBoxVectors[vec] < minPoint[vec])
                            {
                                minPoint[vec] = boundingBoxVectors[vec];
                            }
                        }
                    }
                    Box = new BoundingBox((Vector3)minPoint.ToVector3(), (Vector3)maxPoint.ToVector3());
                    finalScale = null;
                }
            }
        }
       
        private BoundingBox Box;
        private ObjModel _model;
        public Color Color = Color.White;
        public float Alpha = 1;
        public float Roll;
        public float Pitch;
        public float Yaw;
        public Vector3 Scale = new Vector3(1, 1, 0);
        public Vector3 ModelSize;
        public MTexture Texture;
        public VirtualRenderTarget target;
        public bool OnlyRenderWhenOnScreen = true;
        public Model3D(string path, Vector2 position, MTexture texture = null)
            : this(path, position, texture, Vector2.One, 0, 0, 0) { }
        public Model3D(string path, Vector2 position, MTexture texture, Vector2 scale)
            : this(path, position, texture, scale, 0, 0, 0) { }
        public Model3D(string path, Vector2 position, MTexture texture, Vector2 scale, float roll, float pitch, float yaw) : base(position)
        {
            TryGetModel(path, out var model);
            Model = model;
            Scale = new Vector3(scale, 0);
            Texture = texture ?? GFX.Game["objects/PuzzleIslandHelper/catSnug"];
            Roll = roll;
            Pitch = pitch;
            Yaw = yaw;
            Collider = new Hitbox(32, 32);
            target = VirtualContent.CreateRenderTarget("gkshdfgkjsh", 320, 180, true, false);
            Add(new BeforeRenderHook(BeforeRender));
        }

        public virtual void BeforeRender()
        {
            target.SetAsTarget(true);
            if (Scene is Level level)
            {
                RenderModel(Vector2.Zero);
            }
        }
        public override void Render()
        {
            base.Render();
            if (target != null && Scene is Level level)
            {
                Draw.SpriteBatch.Draw(target, Center - new Vector2(160, 90), Color * Alpha);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Model?.Dispose();
            Model = null;
            target?.Dispose();
            target = null;
        }
        public void RenderModel(Vector2 position)
        {
            RenderModel(Texture, position);
        }
        private float? finalScale = null;
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
            var viewport = Engine.Graphics.GraphicsDevice.Viewport;
            if (!finalScale.HasValue)
            {
                Vector3[] boundsCorners = Box.GetCorners();
                float maxX = float.MinValue, maxY = float.MinValue;
                float minX = float.MaxValue, minY = float.MaxValue;
                foreach (var corner in boundsCorners)
                {
                    Vector3 projected = viewport.Project(corner,
                        Effect.Projection,
                        Effect.View,
                        Matrix.Identity);
                    minX = Math.Min(minX, projected.X);
                    maxX = Math.Max(maxX, projected.X);
                    minY = Math.Min(minY, projected.Y);
                    maxY = Math.Max(maxY, projected.Y);
                }

                float projectedWidth = maxX - minX;
                float projectedHeight = maxY - minY;

                float scaleX = Collider.Width / projectedWidth;
                float scaleY = Collider.Height / projectedHeight;
                finalScale = Math.Min(scaleX, scaleY);
                if (finalScale.Value < 0) finalScale = 1;
            }
            Effect.World = Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll)
                          * Matrix.CreateScale(finalScale.Value * Scale)
                          * Matrix.CreateTranslation(position);
            Texture2D tex = texture.Texture.Texture_Safe;
            
            Effect.Texture = texture.Texture.Texture_Safe;
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
        public override void Update()
        {
            base.Update();
            //Pitch += Engine.DeltaTime * 3;
            //Roll += Engine.DeltaTime;
            Yaw += Engine.DeltaTime;
        }
    }
}
