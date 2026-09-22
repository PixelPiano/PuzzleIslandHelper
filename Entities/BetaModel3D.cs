using Celeste.Mod.CommunalHelper;
using Celeste.Mod.CommunalHelper.Components;
using Celeste.Mod.CommunalHelper.Utils;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    [Tracked]
    internal class BetaModel3D : Entity
    {
        public static Dictionary<string, ModAsset> CreatedAssets = [];
        public BasicEffect Effect;
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
                    /*                    var viewport = Engine.Graphics.GraphicsDevice.Viewport;
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
                                        if (finalScale.Value < 0) finalScale = 1;*/

                }
            }
        }
        private BoundingBox Box;
        private ObjModel _model;
        public string Path;
        public Color Color = Color.White;
        public float Alpha = 1;
        public float Roll;
        public float Pitch;
        public float Yaw;
        public Vector3 Scale = Vector3.One;
        public MTexture MTexture;
        public Texture2D Texture;
        public Vector2 Scale2D => Scale.XY();
        public VirtualRenderTarget target;
        public bool ModelIsNull => Model == null;
        public MTexture FallbackTexture => GFX.Game["objects/PuzzleIslandHelper/missingTexture"];

        //todo: update this with Shape3D once CommunalHelper gets the public methods update
        public Shape3D TestShape;
        public BetaModel3D(string path, Vector2 position, MTexture texture = null)
            : this(path, position, texture, Vector2.One, 0, 0, 0) { }
        public BetaModel3D(string path, Vector2 position, MTexture texture, Vector2 scale)
            : this(path, position, texture, scale, 0, 0, 0) { }
        public BetaModel3D(string path, Vector2 position, MTexture texture, Vector2 scale, float roll, float pitch, float yaw) : base(position)
        {
            Path = path;
            Scale = new Vector3(scale, 1);
            MTexture = texture;
            Roll = roll;
            Pitch = pitch;
            Yaw = yaw;
            Collider = new Hitbox(32, 32);
            target = VirtualContent.CreateRenderTarget("target", 320, 180, true, false);
            Add(new BeforeRenderHook(BeforeRender));
            Effect = new(Engine.Graphics.GraphicsDevice)
            {
                TextureEnabled = true,
                View = Matrix.CreateLookAt(new(0, 0, 222), Vector3.Zero, Vector3.Up),
                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), Engine.Viewport.AspectRatio, 0.1f, 1000f),
            };
            KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.I, () =>
            {
                toggle = !toggle;
            });
        }
        private bool toggle;

        public override void Added(Scene scene)
        {
            MTexture ??= FallbackTexture;
            Texture = MTexture.Texture.Texture_Safe;
            if (!TryGetModelModAsset(Path, out ModAsset asset))
            {
                RemoveSelf();
            }
            else
            {
                Model = ObjModel.CreateFromStream(asset.Stream, Path);
            }

            if (ShapeHelper.TryGetObj(Path, out var mesh))
            {
                TestShape = new Shape3D(mesh)
                {
                    Matrix = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Width / 2, Height / 2, 0),
                    Texture = Texture
                };
                Add(TestShape);
            }
            base.Added(scene);
        }
        public virtual void BeforeRender()
        {
            target.SetAsTarget(true);
            if (Model != null)
            {
                RenderModel(Vector2.Zero);
            }
        }
        public override void Render()
        {
            if (toggle)
            {
                if (TestShape == null)
                {
                    Draw.Rect(Position, 8, 8, Color.Red);
                }
                base.Render();
            }
            else if (!toggle)
            {
                if (target != null && Scene is Level level)
                {
                    Draw.SpriteBatch.Draw(target, Center - new Vector2(160, 90), Color * Alpha);
                }
            }
        }
        public override void Update()
        {
            base.Update();
            if (TestShape != null)
            {
                TestShape.Texture = Texture;
                TestShape.Matrix = Matrix.CreateScale(Scale) * Matrix.CreateTranslation(Width / 2, Height / 2, 0);

            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Model?.Dispose();
            Model = null;
            target?.Dispose();
            target = null;
            Effect?.Dispose();
            Effect = null;
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
        public void RenderModel(Texture2D texture, Vector2 position)
        {
            RenderModel(texture, new Vector3(position, 0));
        }
        public void RenderModel(Texture2D texture, Vector3 position)
        {
            Effect.World = Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll)
                          * Matrix.CreateScale(Scale)
                          * Matrix.CreateTranslation(position);
            Effect.Texture = texture;
            var d = Engine.Graphics.GraphicsDevice.DepthStencilState;
            var r = Engine.Graphics.GraphicsDevice.RasterizerState;
            var b = Engine.Graphics.GraphicsDevice.BlendState;
            Engine.Graphics.GraphicsDevice.RasterizerState = MountainModel.MountainRasterizer;
            Engine.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            Engine.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Model.Draw(Effect);
            OnRenderModel(Effect);
            Engine.Graphics.GraphicsDevice.DepthStencilState = d;
            Engine.Graphics.GraphicsDevice.RasterizerState = r;
            Engine.Graphics.GraphicsDevice.BlendState = b;
        }
        public virtual void OnRenderModel(Effect effect)
        {

        }
        [OnUnload]
        public static void Unload()
        {
            CreatedAssets.Clear();
        }
        [Command("force_reload_all_models", "Forces the reload of all Model3D objects")]
        [OnLoadContent]
        public static void LoadContent()
        {
            AssetReloadHelper.Do("Reloading 3D Models", () =>
            {
                foreach (var pair in CreatedAssets)
                {
                    TryGetModelModAsset(pair.Key, out ModAsset asset, true);
                    CreatedAssets[pair.Key] = asset;
                }
            }, () =>
            {
                Logger.Log("Model3D", "Finished reloading models.");
            });
        }
        public static bool TryGetModelModAsset(string path, out ModAsset asset, bool forceReload = false)
        {
            asset = null;
            if (string.IsNullOrEmpty(path)) return false;
            asset = GetModel(path, forceReload);
            return asset != null;
        }
        public static ModAsset GetModel(string path, bool forceReload = false)
        {
            if (!CreatedAssets.TryGetValue(path, out ModAsset asset) || forceReload)
            {
                if (forceReload)
                {
                    Logger.Log(LogLevel.Info, "PuzzleIslandHelper.Model3D", $"Forcing reload at path {path}.");
                }
                else
                {
                    Logger.Log(LogLevel.Info, "PuzzleIslandHelper.Model3D", $"Model at path {path} not found in CreatedModels, attempting to load model manually.");
                }
                if (Everest.Content.TryGet(path, out ModAsset metadata))
                {
                    Logger.Log(LogLevel.Info, "PuzzleIslandHelper.Model3D", $"ModAsset at path {path} found! Attempting to create model...");
                    if (metadata != null)
                    {
                        asset = metadata;
                        if (!forceReload)
                        {
                            Logger.Log(LogLevel.Info, "PuzzleIslandHelper.Model3D", $"Model ({path}) creation complete!");
                            CreatedAssets.Add(path, metadata);
                        }
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warn, "PuzzleIslandHelper.Model3D", $"Failed to create model at path {path}.");
                    }
                    //model = ObjModel.CreateFromStream(metadata.Stream, path);
                    /*                    if (model != null)
                                        {
                                            Logger.Log(LogLevel.Info, "PuzzleIslandHelper.Model3D", $"Model ({path}) creation complete!");
                                            if (!forceReload)
                                            {
                                                CreatedModels.Add(path, model);
                                            }
                                        }
                                        else
                                        {
                                            Logger.Log(LogLevel.Warn, "PuzzleIslandHelper.Model3D", $"Failed to create model at path {path}.");
                                        }*/
                }
                else
                {
                    Logger.Log(LogLevel.Warn, "PuzzleIslandHelper.Model3D", $"Everest could not find the ModAsset at path {path}.");
                }
            }
            else
            {
                Logger.Log(LogLevel.Info, "PuzzleIslandHelper.Model3D", $"Model at path {path} found in model dictionary.");
            }
            return asset;
        }
    }
}
