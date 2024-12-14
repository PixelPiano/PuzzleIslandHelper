using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct HouseVertex : IVertexType
    {
        public Vector3 Position;

        public Color Color;

        public Vector2 Float;

        public static readonly VertexDeclaration VertexDeclaration;

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

        static HouseVertex()
        {
            VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0), new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0), new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));
        }

        public HouseVertex(Vector3 position, Color color, Vector2 mult)
        {
            Position = position;
            Color = color;
            Float = mult;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "{{Position:" + Position.ToString() + " Color:" + Color.ToString() + "}}";
        }

        public static bool operator ==(HouseVertex left, HouseVertex right)
        {
            if (left.Color == right.Color)
            {
                return left.Position == right.Position;
            }

            return false;
        }

        public static bool operator !=(HouseVertex left, HouseVertex right)
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

            return this == (HouseVertex)obj;
        }
    }
    [CustomEntity("PuzzleIslandHelper/FormativeHouse")]
    [Tracked]
    public class FormativeHouse : Entity
    {

        public const int BufferExtend = 16;
        public Vector2 BufferOffset => new Vector2(BufferExtend);
        public Rectangle Bounds;
        public VirtualRenderTarget Target;
        internal HouseVertex[] Vertices;
        public Vector2[] Points;
        public int[] indices;
        public FormativeHouse(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
            Target = VirtualContent.CreateRenderTarget("formative-house-target", data.Width + BufferExtend * 2, data.Height + BufferExtend * 2);
            Bounds = new Rectangle((int)Position.X - BufferExtend, (int)Position.Y - BufferExtend, data.Width + BufferExtend * 2, data.Height * BufferExtend * 2);
        }
        public bool OnScreen;
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Target.Dispose();
        }
        public override void Update()
        {
            base.Update();
            OnScreen = Bounds.OnScreen(SceneAs<Level>(), 16);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            List<Vector2> points = new();
            List<int> indices = new();
            int xOffset = 8;
            for (int y = 0; y < Height; y += 16)
            {
                for (int x = xOffset; x < Width; x += 16)
                {
                    int count = indices.Count;
                    points.Add(new(x, y));
                    points.Add(new(x + 16, y));
                    points.Add(new(x - 8, y + 16));
                    points.Add(new(x - 8, y + 16));
                    points.Add(new(x + 16, y));
                    points.Add(new(x + 8, y + 16));
                    indices.Add(count++);
                    indices.Add(count++);
                    indices.Add(count++);
                    indices.Add(count++);
                    indices.Add(count++);
                    indices.Add(count++);
                }
                xOffset = xOffset == 0 ? 8 : 0;
            }
            Points = [.. points];
            Vertices = new HouseVertex[Points.Length];
            for (int i = 0; i < Points.Length; i += 3)
            {
                int r = Calc.Random.Range(0, 3);
                Color random = Calc.Random.Choose(Color.Green, Color.Lime, Color.DarkGreen, Color.Turquoise, Color.DarkOliveGreen, Color.Red);
                for (int j = 0; j < 3; j++)
                {
                    Vector2 point = Points[i + j];
                    float mult = point.Y == Height || point.Y == 0 ? 0 : 1;
                    Color color = Color.Lerp(random, Color.White, j == r ? 0.3f : 0);
                    Vertices[i + j] = new HouseVertex(new Vector3(point + BufferOffset, 0), color, Vector2.One * mult);
                }
            }
            this.indices = indices.ToArray();
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level) return;
            Draw.SpriteBatch.Draw(Target, Position - BufferOffset, Color.White);
        }
    }
    [ConstantEntity("PuzzleIslandHelper/FormativeHouseRenderer")]
    internal class FormativeHouseRenderHelper : Entity
    {
        public List<Entity> Entities = new();
        private bool render;
        public FormativeHouseRenderHelper() : base()
        {
            Tag |= Tags.Global;
            Add(new BeforeRenderHook(BeforeRender));
        }
        public override void Update()
        {
            base.Update();
            Entities = Scene.Tracker.GetEntities<FormativeHouse>();
        }
        public void BeforeRender()
        {
            if (Scene is not Level level) return;

            Draw.SpriteBatch.StandardBegin(Matrix.Identity);
            Effect effect = ShaderHelperIntegration.GetEffect("PuzzleIslandHelper/Shaders/formativeHouseShader");
            EffectParameterCollection parameters = effect.Parameters;
            Vector2 vector = new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height);
            Matrix matrix = Matrix.CreateScale(1f / vector.X * 2f, (0f - 1f / vector.Y) * 2f, 1f);
            matrix *= Matrix.CreateTranslation(-1f, 1f, 0f);
            matrix *= Matrix.CreateRotationX((float)Math.PI / 3f);
            parameters["World"]?.SetValue(matrix);
            parameters["Time"]?.SetValue(level.TimeActive);
            foreach (FormativeHouse h in Entities)
            {
                if (h.OnScreen)
                {
                    h.Target.SetAsTarget(true);
                    GFX.DrawIndexedVertices(Matrix.Identity, h.Vertices, h.Vertices.Length, h.indices, h.indices.Length / 3, effect);
                }
            }
            Draw.SpriteBatch.End();

        }

    }
}