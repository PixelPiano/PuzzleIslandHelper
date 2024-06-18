using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.Backdrops;
using Celeste.Mod.CommunalHelper.Utils;
using Celeste.Mod.PuzzleIslandHelper.Helpers;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/BackgroundShape")]
    public class BackgroundShape : Backdrop
    {
        public static BasicEffect Shader;
        private static VirtualRenderTarget buffer;
        public static VirtualRenderTarget Buffer => buffer ??= VirtualContent.CreateRenderTarget("background_shape_renderer", 320, 180);
        private float cubeSize;
        private Color color;
        private float alpha;
        private string path;
        private Vector3 position;
        private MTexture face;
        private float size;
        private float Roll, Pitch, Yaw;

        public Mesh<VertexPositionColorTexture> Mesh;
        public BackgroundShape(BinaryPacker.Element data) : base()
        {
            size = data.AttrFloat("size", 80);
            alpha = data.AttrFloat("alpha", 1);
            color = Calc.HexToColor(data.Attr("color", "FFFFFF")) * alpha;
            path = data.Attr("texturePath");

            position = new Vector3(160 - size, -30, 0);
            face = GFX.Game[path];
            Shader.Texture = face.Texture.Texture_Safe;
            Mesh = Shapes.Box(size, size, size, face);
            size = cubeSize;
            Yaw = -35f.ToRad();
            Pitch = -65.5f.ToRad();

        }
        public override void Update(Scene scene)
        {
            base.Update(scene);
            Roll = scene.TimeActive / 4f;
            Shader.World = Matrix.CreateFromYawPitchRoll(Yaw, Pitch, Roll) * Matrix.CreateTranslation(position);
            UpdateVertices();
            Shader.World *= Matrix.CreateScale(new Vector3(1, 1, 0));

        }
        public void UpdateVertices()
        {
            for (int i = 0; i < Mesh.Vertices.Length; i++)
            {
                Mesh.Vertices[i].Color = GetZColor(Vector3.Transform(Mesh.Vertices[i].Position, Shader.World), Color);
            }
        }
        public Color GetZColor(Vector3 vector, Color color)
        {
            return Color.Lerp(Color.Black, color, vector.Z / 100f);
        }

        public void RenderShape()
        {
            if (Mesh is null) return;
            foreach (EffectPass pass in Shader.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Mesh.Vertices, 0, Mesh.VertexCount, Mesh.Indices, 0, Mesh.Triangles);
            }
        }

        public override void Ended(Scene scene)
        {
            base.Ended(scene);
            buffer?.Dispose();
            buffer = null;
            Mesh?.Dispose();
            Mesh = null;
        }
        public override void BeforeRender(Scene scene)
        {
            base.BeforeRender(scene);
            Engine.Graphics.GraphicsDevice.SetRenderTarget(Buffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            RenderShape();
        }
        internal static void Initialize()
        {
            Shader = new(Engine.Graphics.GraphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
                View = Matrix.CreateLookAt(new(0, 0, 160), Vector3.Zero, Vector3.Up),
                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), Engine.Viewport.AspectRatio, 0.1f, 1000f),
            };
        }

        internal static void Unload()
        {
            Shader?.Dispose();
            Shader = null;
        }
        public override void Render(Scene scene)
        {
            base.Render(scene);
            Draw.SpriteBatch.Draw(Buffer, Vector2.Zero, Color.White);
        }
    }
}