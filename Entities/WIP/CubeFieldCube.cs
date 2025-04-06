using Celeste.Mod.CommunalHelper.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Celeste.Mod.PuzzleIslandHelper.Helpers;
using System;
using static Celeste.Mod.PuzzleIslandHelper.Effects.CubeField;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    public class CubeFieldCube : Entity
    {
        public Mesh<VertexPositionColorTexture> Mesh;
        public float Size = 32;
        public float Scale = 1;
        public float Yaw, Pitch, Roll;
        public int ZLayer, MaxLayers;
        public Color Color = Color.White;
        public float Alpha = 1;
        public const float MaxAddition = 40;
        public MTexture Face;
        public float AdditionalZ;
        public Vector3 CubePosition
        {
            get
            {
                return new Vector3(Position - new Vector2(160, 90), Z);
            }
            set
            {
                Position = new Vector2(value.X, value.Y);
                Z = value.Z;
            }
        }
        public float Z;
        public Matrix RotationMatrix
        {
            get
            {
                return Matrix.CreateRotationX(Yaw) * Matrix.CreateRotationY(Pitch) * Matrix.CreateRotationZ(Roll);
            }
        }

        public CubeFieldCube(Vector3 position, string path, float size) : base(position.XY())
        {
            CubePosition = position;
            Tag |= Tags.TransitionUpdate | Tags.Persistent;
            Face = GFX.Game[path];
            Mesh = Shapes.Box(size, size, 1, Face);
            Size = size;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            UpdateVertices(Shader.World);
            if (Calc.Random.Chance(0.2f))
            {
                Add(new Coroutine(blinkRoutine()));
            }
        }

        private IEnumerator blinkRoutine()
        {
            float interval = Calc.Random.Range(0.4f, 5);
            float blinkTime = Calc.Random.Range(0.1f, 0.7f);

            Color blinkColor = Calc.Random.Choose(Color.Blue, Color.Red, Color.Yellow, Color.Green, Color.White);
            while (true)
            {
                Color orig = Color;
                yield return interval;
                Color = Color.Lerp(orig, blinkColor, 1);
                yield return blinkTime;
                Color = orig;
            }
        }
        public void UpdateVertices(Matrix world)
        {
            for (int i = 0; i < Mesh.Vertices.Length; i++)
            {
                Mesh.Vertices[i].Color = GetZColor(Mesh.Vertices[i].Position, Color);
            }
        }


        public Color GetZColor(Vector3 vector, Color color)
        {
            float avg = vector.Z / Size;
            Color to = avg < 0 ? Color.Lerp(color, Color.Black, 0.5f) : Color.White;
            float lerp = Math.Abs(avg);
            Color newColor = Color.Lerp(color, to, lerp);
            return Color.Lerp(color, Color.Black, (float)ZLayer / (MaxLayers + 1));
        }
        public float ClosestMultiple(double num, float factor)
        {
            return (float)(Math.Round(num / (double)factor, MidpointRounding.AwayFromZero) * factor);
        }
        public override void Update()
        {
            base.Update();
            UpdateVertices(Shader.World);
            Roll = Scene.TimeActive;
        }

        public void RenderCube()
        {
            RenderCube(Yaw, Pitch, Roll);
        }
        public void RenderCube(float yaw, float pitch, float roll)
        {
            if (Mesh is null) return;
            Matrix rotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
            Shader.Texture = Face.Texture.Texture_Safe;
            Matrix world = Shader.World;
            Shader.World = rotation * Matrix.CreateScale(Scale) * Matrix.CreateTranslation(CubePosition + Vector3.UnitZ * AdditionalZ);
            foreach (EffectPass pass in Shader.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Mesh.Vertices, 0, Mesh.VertexCount, Mesh.Indices, 0, Mesh.Triangles);
            }
            Shader.World = Matrix.Identity;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Mesh?.Dispose();
            Mesh = null;
        }
    }
}





