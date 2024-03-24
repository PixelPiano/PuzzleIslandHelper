using Celeste.Mod.Backdrops;
using Celeste.Mod.CommunalHelper.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Helpers;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/Cube")]
    [Tracked]
    public class BetaCube : Entity
    {
        private static BasicEffect shader;
        private VirtualRenderTarget buffer;

        private readonly Mesh<VertexPositionColorTexture> cube;
        private readonly MTexture face;
        private readonly float scale;
        private readonly float size = 32;
        private float[] Z;
        public float Yaw;
        public float Pitch;
        public float Roll;

        public Matrix RotationMatrix
        {
            get
            {
                return Matrix.CreateRotationX(Yaw) * Matrix.CreateRotationY(Pitch) * Matrix.CreateRotationZ(Roll);
            }
        }
        public BetaCube(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Tag |= Tags.TransitionUpdate;
            face = GFX.Game["objects/PuzzleIslandHelper/cube/betacube"];
            scale = 1;
            cube = Shapes.Box(size, size, size, face);
            Z = new float[cube.Vertices.Length];
            Pitch = -1;
            Add(new BeforeRenderHook(BeforeRender));
        }

        private void UpdateColors()
        {
            Matrix matrix = shader.World;
            for (int i = 0; i < cube.Vertices.Length; i++)
            {
                cube.Vertices[i].Color = GetZColor(Vector3.Transform(cube.Vertices[i].Position, matrix), Color.Blue);
            }
        }
        public float ClosestMultiple(double num, float factor)
        {
            return (float)(Math.Round(num / (double)factor, MidpointRounding.AwayFromZero) * factor);
        }

        public Color GetZColor(Vector3 vector, Color color)
        {
            float avg = vector.Z / size;
            Color to = avg < 0 ? Color.Transparent : Color.White;
            float lerp = Math.Abs(avg);
            return Color.Lerp(color, to, lerp);
        }
        public override void Update()
        {
            base.Update();
            UpdateColors();
            Roll = Scene.TimeActive * 0.6f;
        }

        public void BeforeRender()
        {
            buffer = PianoUtils.PrepareRenderTarget(buffer, "cube");

            Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            shader.Texture = face.Texture.Texture_Safe;

            float time = Engine.Scene.TimeActive * 0.6f;
            RenderCube(0, -1, time);
        }
        public void RenderCube(float yaw, float pitch, float roll)
        {
            Matrix rotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
            shader.World = rotation * Matrix.CreateScale(scale);

            foreach (EffectPass pass in shader.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, cube.Vertices, 0, cube.VertexCount, cube.Indices, 0, cube.Triangles);
            }
        }


        public override void Render()
        {
            base.Render();
            if (Scene is not Level level) return;
            if (buffer is not null && !buffer.IsDisposed) Draw.SpriteBatch.Draw(buffer, level.Camera.Position, Color.White);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (buffer is not null)
            {
                buffer.Dispose();
                buffer = null;
            }
            cube.Dispose();
        }

        internal static void Initialize()
        {
            shader = new(Engine.Graphics.GraphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
                View = Matrix.CreateLookAt(new(0, 0, 160), Vector3.Zero, Vector3.Up),
                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), Engine.Viewport.AspectRatio, 0.1f, 1000f),
            };
        }

        internal static void Unload()
        {
            shader.Dispose();
        }
    }
}





