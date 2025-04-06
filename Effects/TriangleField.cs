using Celeste.Mod.Backdrops;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/TriangleField")]
    public class TriangleField : Backdrop
    {
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TriVertex : IVertexType
        {
            public Vector3 Position;

            public Color Color;

            public float Rotation;

            public static readonly VertexDeclaration VertexDeclaration;

            VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

            static TriVertex()
            {
                VertexDeclaration = new VertexDeclaration(
                    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                    new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                    new VertexElement(16, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0));
            }

            public TriVertex(Vector3 position, Color color, float rotation)
            {
                Position = position;
                Color = color;
                Rotation = rotation;
            }

            public override int GetHashCode()
            {
                return 0;
            }

            public override string ToString()
            {
                return "{{Position:" + Position.ToString() + " Color:" + Color.ToString() + " Rotation:" + Rotation + "}}";
            }

            public static bool operator ==(TriVertex left, TriVertex right)
            {
                if (left.Color == right.Color)
                {
                    return left.Position == right.Position;
                }

                return false;
            }

            public static bool operator !=(TriVertex left, TriVertex right)
            {
                return !(left == right);
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }

                if (obj.GetType() != GetType())
                {
                    return false;
                }

                return this == (TriVertex)obj;
            }
        }
        public abstract class Triangle
        {
            public TriangleField Parent;
            public Triangle(TriangleField parent, Vector3 center = default)
            {
                Parent = parent;
                Center = center;
            }
            public float X
            {
                get
                {
                    return Center.X;
                }
                set
                {
                    Center.X = value;
                }
            }
            public float Y
            {
                get
                {
                    return Center.Y;
                }
                set
                {
                    Center.Y = value;
                }
            }
            public Vector3 Center;
            public int VertexCount;
            public int VertexIndex;

            public abstract TriVertex UpdateVertex(TriVertex input, int index);
            public abstract void UpdateTriangle();
            public abstract bool SafeToRemove(TriVertex input);
            public abstract TriVertex[] CreateVertices(Color color, Vector2 position);
        }
        public class Equilateral : Triangle
        {
            public Vector2 CenterStart;
            public Vector2 CenterEnd;
            public float Size;
            public Directions Direction;
            public float RotateRate;
            public float Rotation;
            public float Speed;
            public Equilateral(TriangleField field) : base(field)
            {
                Size = field.sizeRange.Random();
                RotateRate = field.rotateRange.Random();
                Direction = field.Direction;
                Rotation = Calc.Random.NextAngle();
                Speed = field.speedRange.Random();
            }

            public override TriVertex[] CreateVertices(Color color, Vector2 position)
            {
                Vector2 center = CenterStart = Direction switch
                {
                    Directions.Up => new Vector2(Calc.Random.Range(-8, 329), 180 + Size * 2),
                    Directions.Down => new Vector2(Calc.Random.Range(-8, 329), -Size * 2),
                    Directions.Left => new Vector2(320 + Size * 2, Calc.Random.Range(-8, 189)),
                    Directions.Right => new Vector2(-Size * 2, Calc.Random.Range(-8, 189)),
                    _ => position
                };
                Center = new(center, 0);
                float pad = 8;
                CenterEnd = Direction switch
                {
                    Directions.Up => new Vector2(X, -Size * 2 - pad),
                    Directions.Down => new Vector2(X, 180 + Size * 2 + pad),
                    Directions.Left => new Vector2(-Size * 2 - pad, Y),
                    Directions.Right => new Vector2(320 + Size * 2 + pad, Y),
                    _ => center
                };
                float theta1 = GetAngle(0), theta2 = GetAngle(1), theta3 = GetAngle(2);
                return [new(new(center + GetAngleOffset(theta1), 0), color,theta1),
                        new(new(center + GetAngleOffset(theta2), 0), color,theta2),
                        new(new(center + GetAngleOffset(theta3), 0), color,theta3)];
            }
            public override TriVertex UpdateVertex(TriVertex input, int index)
            {
                float theta = GetAngle(index);

                input.Position = new Vector3(Center.XY() + GetAngleOffset(theta), 0);
                input.Rotation = theta;
                return input;
            }
            public override void UpdateTriangle()
            {
                Center = new Vector3(Calc.Approach(Center.XY(), CenterEnd, Speed * Engine.DeltaTime), 0);
                Rotation = Calc.WrapAngle(Rotation + Engine.DeltaTime * RotateRate);
            }
            public override string ToString()
            {
                return "{" + $"Position: {Center}, Rotation: {Rotation}" + "}";
            }
            public override bool SafeToRemove(TriVertex input)
            {
                Vector2 position = input.Position.XY();

                return Direction switch
                {
                    Directions.Up => position.Y < 0,
                    Directions.Down => position.Y > 180,
                    Directions.Left => position.X < 0,
                    Directions.Right => position.X > 320,
                    _ => true
                };
            }
            public float GetAngle(int index)
            {
                return Rotation + index * MathHelper.TwoPi / VertexCount;
            }
            public Vector2 GetAngleOffset(int index)
            {
                float theta = GetAngle(index);
                return new Vector2(x: (float)Math.Cos(theta) * Size, y: (float)Math.Sin(theta) * Size);
            }
            public Vector2 GetAngleOffset(float theta)
            {
                return new Vector2(x: (float)Math.Cos(theta) * Size, y: (float)Math.Sin(theta) * Size);
            }
        }
        public enum Directions
        {
            Up, Down, Left, Right
        }
        public Directions Direction;
        public enum TriangleTypes
        {
            Equilateral,
            Isosceles,
            Random,
        }
        public TriangleTypes TriType = TriangleTypes.Equilateral;
        public float MaxTriangles = 40;
        public int Triangles;
        private List<TriVertex> verticeList = [];

        private List<Triangle> Data = [];
        private List<int> toRemove = new();
        private static VirtualRenderTarget buffer;
        private NumRange sizeRange, alphaRange, rotateRange, speedRange;
        private Color[] colors;
        private float timer;

        public TriangleField(BinaryPacker.Element data) : base()
        {
            Scroll = Vector2.Zero;
            UseSpritebatch = false;
            Enum.TryParse(data.Attr("direction", "Left"), false, out Direction);
            Enum.TryParse(data.Attr("triangleType", "Equilateral"), false, out TriType);
            sizeRange = new(data.AttrFloat("minSize", 8), data.AttrFloat("maxSize", 16));
            alphaRange = new(data.AttrFloat("minAlpha", 0.5f), data.AttrFloat("maxAlpha", 1));
            rotateRange = new(data.AttrFloat("minRotateRate", 0.1f), data.AttrFloat("maxRotateRate", 0.6f));
            speedRange = new(data.AttrFloat("minSpeed", 8), data.AttrFloat("maxSpeed", 32));
            string c = data.Attr("colors", "FF0000,00FF00, 0000FF");
            string[] array = c.Replace(" ", "").Split(',');

            colors = new Color[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                colors[i] = Calc.HexToColor(array[i]);
            }
        }
        public void UpdateTriangles()
        {
            int index = 0;
            for (int i = 0; i < Data.Count; i++)
            {
                Triangle data = Data[i];
                data.VertexIndex = index;
                data.UpdateTriangle();

                bool allPassedLimit = true;
                for (int j = 0; j < data.VertexCount; j++)
                {
                    TriVertex v = data.UpdateVertex(verticeList[index + j], j);
                    verticeList[index + j] = v;
                    bool safe = data.SafeToRemove(v);
                    allPassedLimit &= safe;
                }
                if (allPassedLimit)
                {
                    addTriangleToBeRemoved(i);
                }
                index += data.VertexCount;
            }
        }
        public void AddTriangle(Vector2 position)
        {
            Color baseColor = colors.Random();
            float alpha = alphaRange.Random();

            Triangle newData = TriType switch
            {
                TriangleTypes.Equilateral => new Equilateral(this),
                _ => null
            };

            if (newData != null)
            {
                TriVertex[] array = newData.CreateVertices(baseColor * alpha, Vector2.Zero);
                newData.VertexCount = array.Length;
                newData.VertexIndex = verticeList.Count;

                verticeList.AddRange(array);
                Triangles += array.Length / 3;
                Data.Add(newData);
            }
        }
        public override void Update(Scene scene)
        {
            base.Update(scene);

            if (IsVisible(scene as Level))
            {
                if (timer > 0)
                {
                    timer -= Engine.DeltaTime;
                }
                else if (Triangles < MaxTriangles)
                {
                    timer = Calc.Random.Range(0.2f, 0.5f);
                    AddTriangle(Vector2.Zero);
                }
                UpdateTriangles();
            }
            removeTrisFromList();
        }
        public void addTriangleToBeRemoved(int tri)
        {
            if (toRemove.TryAdd(tri))
            {
                Triangles -= Data[tri].VertexCount / 3;
            }
        }
        private void removeTrisFromList()
        {
            if (toRemove.Count > 0)
            {
                List<TriVertex> verticesToRemove = new();
                List<Triangle> dataToRemove = new();
                foreach (int tri in toRemove)
                {
                    Triangle triangle = Data[tri];
                    verticesToRemove.AddRange(verticeList.GetRange(triangle.VertexIndex, triangle.VertexCount));
                    dataToRemove.Add(triangle);
                }
                foreach (TriVertex v in verticesToRemove)
                {
                    verticeList.Remove(v);
                }
                foreach (Triangle data in dataToRemove)
                {
                    Data.Remove(data);
                }
                int index = 0;
                foreach (Triangle data in Data)
                {
                    data.VertexIndex = index;
                    index += data.VertexCount;
                }
                toRemove.Clear();
            }
        }
        public override void Ended(Scene scene)
        {
            base.Ended(scene);
            Data.Clear();
            verticeList.Clear();
            toRemove.Clear();

        }
        [OnUnload]
        public static void Unload()
        {
            buffer?.Dispose();
            buffer = null;
        }
        [OnInitialize]
        public static void Initialize()
        {
            buffer = VirtualContent.CreateRenderTarget("TriangleField", 320, 180);
        }
        public bool Debug => PianoModule.Session.DEBUGBOOL1;
        public override void Render(Scene scene)
        {
            BackdropRenderer background = (scene as Level).Background;
            RenderTarget2D renderTarget2D = GameplayBuffers.Level;
            RenderTargetBinding[] renderTargets = Engine.Graphics.GraphicsDevice.GetRenderTargets();
            if (renderTargets.Length != 0)
            {
                renderTarget2D = (renderTargets[0].RenderTarget as RenderTarget2D) ?? renderTarget2D;
            }
            Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

            Effect effect = ShaderHelperIntegration.GetEffect("PuzzleIslandHelper/Shaders/triangleFieldShader");
            Engine.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            EffectParameterCollection parameters = effect.Parameters;
            Vector2 vector = new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height);
            Matrix matrix = Matrix.CreateScale(1f / vector.X * 2f, (0f - 1f / vector.Y) * 2, 1f);
            matrix *= Matrix.CreateTranslation(-1f, 1f, 0f);
            matrix *= Matrix.CreateRotationX((float)Math.PI / 3f);
            parameters["World"]?.SetValue(matrix);
            parameters["Time"]?.SetValue(scene.TimeActive);
            parameters["Dimensions"]?.SetValue(new Vector2(320, 180) * (GameplayBuffers.Gameplay.Width / 320f));
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            EffectTechnique effectTechnique = effect.Techniques[0];
            foreach (EffectPass pass in effectTechnique.Passes)
            {
                pass.Apply();
                Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, [.. verticeList], 0, Triangles);
            }

            Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            Engine.Graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            Engine.Instance.GraphicsDevice.SetRenderTarget(renderTarget2D);
            background.StartSpritebatch(BlendState.AlphaBlend);
            Draw.SpriteBatch.Draw((RenderTarget2D)buffer, Vector2.Zero, Color.White);
            background.EndSpritebatch();
        }
    }
}

