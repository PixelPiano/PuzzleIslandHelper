using Celeste.Mod.corkr900Graphics.EfficientRendering;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VivHelper.Entities;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    //[CustomEntity("PuzzleIslandHelper/Cube")]
    //[Tracked]
    public class CubeTest : Entity
    {
        private float depth;
        private float rate = 1.5f;
        public float angleX, angleY, angleZ;
        public float angleSpeedX, angleSpeedY, angleSpeedZ;
        private float Scale;
        private Matrix xMatrix, yMatrix, zMatrix;
        public static readonly Vector3 DDD = new(-1, -1, -1);
        public static readonly Vector3 UDD = new(1, -1, -1);
        public static readonly Vector3 DUD = new(-1, 1, -1);
        public static readonly Vector3 DDU = new(-1, -1, 1);
        public static readonly Vector3 UUD = new(1, 1, -1);
        public static readonly Vector3 DUU = new(-1, 1, 1);
        public static readonly Vector3 UDU = new(1, -1, 1);
        public static readonly Vector3 UUU = new(1, 1, 1);
        public static int Triangles = 12;
        private float track;
        public Matrix RotationMatrix
        {
            get
            {
                return GetMatrix();
            }
        }
        public VertexPositionColor[] VertexBuffer = new VertexPositionColor[]
            {
            new VertexPositionColor(DDD, Color.White),
            new VertexPositionColor(DDU, Color.White),
            new VertexPositionColor(UDU, Color.White),
            new VertexPositionColor(UDD, Color.White),
            new VertexPositionColor(DUD, Color.White),
            new VertexPositionColor(DUU, Color.White),
            new VertexPositionColor(UUU, Color.White),
            new VertexPositionColor(UUD, Color.White)
            };
        public VertexPositionColor[] Vertices;
        public float[] zCache = new float[8];
        public int[] Indices = new int[]
            {
                3, 2, 1, 3, 1, 0,
                0, 1, 5, 0, 5, 4,
                4, 6, 5, 4, 7, 6,
                3, 2, 6, 3, 6, 7,
                6, 5, 1, 6, 1, 2,
                7, 0, 3, 7, 4, 0
            };
        public Vector3 Offset
        {
            get
            {
                return new Vector3(Position.X, Position.Y, 0);
            }
        }
        private int index;
        public IEnumerator routine()
        {
            float limit = 5;
            while (true)
            {
                while (depth < limit)
                {
                    depth += Engine.DeltaTime;
                    yield return null;
                }
                while (depth > -limit)
                {
                    depth -= Engine.DeltaTime;
                    yield return null;
                }
            }
        }
        public CubeTest(EntityData data, Vector2 offset)
: this(data.Position + offset, data.Width)
        {
        }
        public CubeTest(Vector2 position, float size) : base(position)
        {
            Collider = new Hitbox(size, size);
            Scale = size;
            xMatrix = Matrix.Identity;
            yMatrix = Matrix.Identity;
            zMatrix = Matrix.Identity;
            Vertices = new VertexPositionColor[VertexBuffer.Length];
            for (int i = 0; i < VertexBuffer.Length; i++)
            {
                Vertices[i] = new VertexPositionColor(VertexBuffer[i].Position, VertexBuffer[i].Color);
            }
            //Add(new Coroutine(routine()));
        }

        public override void Update()
        {
            base.Update();
            UpdateAngles();
            UpdateVertices();
        }
        public void UpdateAngles()
        {
            angleSpeedY = (angleSpeedY + rate) % 360;
            angleSpeedX = (angleSpeedX + rate) % 360;
            angleY = (30 + angleSpeedY).ToRad();
            angleX = 15f.ToRad();
        }
        public void UpdateVertices()
        {
            if (Vertices is null || Vertices.Length == 0) return;
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Position = Vector3.Transform(VertexBuffer[i].Position * Scale, RotationMatrix);
                Vertices[i].Color = GetZColor(Vertices[i].Position.Z, Color.Blue);
            }
        }

        public override void Render()
        {
            base.Render();
            if (Scene is not Level level) return;
            GameplayRenderer.End();
            for (int k = 0; k < Vertices.Length; k++) //store the z value and set the z value to 0 so everything gets rendered
            {
                float z = Vertices[k].Position.Z;
                zCache[k] = z;
                float mult = z < 0 ? 0.5f : 1;
                Vertices[k].Position.Z = MathHelper.Distance(z, 1) / Scale * mult;
                Vertices[k].Position += Offset;
            }
            GFX.DrawIndexedVertices(level.Camera.Matrix, Vertices, Vertices.Length, Indices, Triangles);

            for (int l = 0; l < Vertices.Length; l++)
            {
                Vertices[l].Position.Z = zCache[l];
                Vertices[l].Position -= Offset;
            }
            GameplayRenderer.Begin();
        }
        public void RenderTwo()
        {
            //base.Render();
            if (Scene is not Level level) return;
            GameplayRenderer.End();

            // Rotation
            Matrix rotation = RotationMatrix;
            // Scale
            Matrix scale = Matrix.CreateScale(Scale);
            Matrix clamp = Matrix.CreateScale(new Vector3(1, 1, 0));
            Matrix translation = Matrix.CreateTranslation(Offset);

            // The transformation matrix transforms the object space to screen space
            // Transformations are applied left-to-right in the multiplication order, so make sure translation is last before the screen space transform
            Matrix transform = rotation * scale * translation * clamp * level.Camera.Matrix;

            GFX.DrawIndexedVertices(transform, Vertices, 8, Indices, 12);
            GameplayRenderer.Begin();
        }


        private Matrix GetMatrix()
        {
            Matrix.CreateRotationX(angleX, out xMatrix);
            Matrix.CreateRotationY(angleY, out yMatrix);
            Matrix.CreateRotationZ(angleZ, out zMatrix);
            return xMatrix * yMatrix * zMatrix;
        }
        public float ClosestMultiple(double num, float factor)
        {
            return (float)(Math.Round(num / (double)factor, MidpointRounding.AwayFromZero) * factor);
        }

        public Color GetZColor(float z, Color color)
        {
            Color to = z < 0 ? Color.Transparent : Color.White;
            float lerp = Math.Abs(z) / Scale;
            return Color.Lerp(color, to, ClosestMultiple(lerp,0.5f));
        }
    }
}