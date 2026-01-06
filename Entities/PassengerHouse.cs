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
    [CustomEntity("PuzzleIslandHelper/FormativeHouse", "PuzzleIslandHelper/PassengerHouse")]
    [Tracked]
    public class PassengerHouse : Entity
    {
        public int Primitives;
        public const int BufferExtend = 16;
        public Vector2 BufferOffset => new Vector2(BufferExtend);
        public Rectangle Bounds;
        public VirtualRenderTarget Target;
        public VirtualRenderTarget BgTarget;
        internal HouseVertex[] Vertices;
        private VertexPositionColor[] roofVertices = new VertexPositionColor[8];
        private VertexPositionColor[] shadeVertices = new VertexPositionColor[4];
        private static int[] roofIndices = [0, 1, 2, 2, 1, 3, 4, 5, 6, 6, 5, 7];
        private static int[] shadeIndices = [0, 1, 2, 2, 1, 3];
        private static (Color top, Color bottom) roofDepthColors = (Color.Lerp(Color.Lime, Color.Black, 0.4f), Color.Lerp(Color.DarkGreen, Color.Black, 0.4f));
        private static (Color top, Color bottom) roofColors = (Color.Lime, Color.DarkGreen);
        public Vector2[] Points;
        public int[] indices;
        public JumpThru Platform;
        public Color BetaColor;
        public bool OnScreen;
        public int DepthChangePoint = 3;
        public bool RoofExtendLeft = true, RoofExtendRight = true;
        public float DepthYOffset;
        public Facings Facing = Facings.Right;
        public class Connector : Component
        {
            public Vector2 Position;
            public float Height;
            public Entity ConnectedTo;
            public Connector(Entity connectTo, Vector2 offset) : base(true, false)
            {
                Position = offset;
                ConnectedTo = connectTo;
            }
            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);

            }
        }
        [Tracked]
        public class PatternBlocker : Component
        {
            public PatternBlocker() : base(false, false)
            {

            }
        }
        public List<Connector> Connections = [];
        private FormativeHouseRenderHelper helper;
        private bool bgRendered;
        public PassengerHouse(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Facing = data.Enum("facing", Facings.Right);
            Depth = 10;
            Tag |= Tags.TransitionUpdate;
            Collider = new Hitbox(data.Width, data.Height);
            Target = VirtualContent.CreateRenderTarget("formative-house-target", data.Width + BufferExtend * 2, data.Height + BufferExtend * 2);
            BgTarget = VirtualContent.CreateRenderTarget("formative-house-target-bg", Target.Width, Target.Height);
            Bounds = new Rectangle((int)Position.X - BufferExtend, (int)Position.Y - BufferExtend, data.Width + BufferExtend * 2, data.Height * BufferExtend * 2);
            BetaColor = Color.Lerp(Color.Lime, Calc.Random.Choose(Color.White, Color.Black), Calc.Random.Range(0, 0.4f));
            DepthChangePoint = Math.Min((int)(Width / 3 / 16), 2);
            for (int i = 0; i < roofVertices.Length; i++)
            {
                roofVertices[i].Position = new Vector3(0, 0, 0);
            }
            for (int i = 0; i < shadeVertices.Length; i++)
            {
                shadeVertices[i].Position = new Vector3(0, 0, 0);
            }
            shadeVertices[0].Color = Color.Black * 0.5f;
            shadeVertices[1].Color = Color.Black * 0.5f;
            shadeVertices[2].Position.Y += 8;
            shadeVertices[3].Position.Y += 8;
            shadeVertices[0].Position.Y += 4;
            shadeVertices[1].Position.Y += 4;

            roofVertices[2].Position.Y += 4;
            roofVertices[3].Position.Y += 4;
            roofVertices[6].Position.Y += 4;
            roofVertices[7].Position.Y += 4;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(Platform = new JumpThru(Position, (int)Width, true));
            helper = PianoUtils.SeekController(scene, () => { return new FormativeHouseRenderHelper(); });
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Collidable = false;
            bool facingRight = Facing == Facings.Right;
            for (int i = 0; i < Height; i++)
            {
                PassengerHouse collided = Scene.CollideFirst<PassengerHouse>(Position + new Vector2(facingRight ? -1 : (int)Width + 1, i));
                if (collided != null && collided != this)
                {
                    if (i < 4)
                    {
                        RoofExtendLeft = false;
                        collided.RoofExtendRight = false;
                    }
                    DepthYOffset = i;
                    bool makeNewConnector = true;
                    foreach (Connector c in Connections)
                    {
                        if (c.ConnectedTo == collided)
                        {
                            makeNewConnector = false;
                            c.Height++;
                        }
                    }
                    if (makeNewConnector)
                    {
                        float x = facingRight ? 0 : Width - DepthChangePoint * 16;
                        Connections.Add(new Connector(collided, new Vector2(x, i + 1)));
                    }
                }
            }
            Color depthColor = new(0, 50, 0);
            Color frontColor = new(0, 100, 0);
            Vector2 bufferOffset = BufferOffset;
            float depthLength = DepthChangePoint * 16;
            Rectangle front = default, back = default;
            front = new Rectangle(BufferExtend + (int)depthLength, BufferExtend, (int)(Width - depthLength), (int)Height);
            back = new Rectangle(BufferExtend + (int)DepthYOffset, BufferExtend, (int)depthLength, (int)(Height - DepthYOffset));
            if (!facingRight)
            {
                front.X = BufferExtend;
                back.X = BufferExtend + (int)(Width - depthLength);
            }

            Add(new BeforeRenderHook(() =>
            {
                if (bgRendered) return;
                BgTarget.SetAsTarget(true);
                Draw.SpriteBatch.Begin();
                Draw.Rect(back, depthColor);
                Draw.Rect(front, frontColor);
                foreach (Connector c in Connections)
                {
                    Draw.Rect(bufferOffset + c.Position, depthLength, c.Height, frontColor);
                }
                Draw.SpriteBatch.End();
                bgRendered = true;
            }));

            Collidable = true;
            List<Vector2> points = new();
            List<int> indices = new();
            int xOffset = 8;
            int depthPoint = DepthChangePoint * 16;
            for (int y = 0; y < Height; y += 16)
            {
                bool wasOnSideOfHouse = true;
                bool onSideOfHouse = true;
                for (int x = xOffset; x < Width; x += 16)
                {
                    int count = indices.Count;

                    float yMax = Height;
                    float yMin = 0;
                    int sign = Math.Sign(x - depthPoint);
                    foreach (Connector c in Connections)
                    {
                        if (c.Height > 0 && y >= c.Position.Y && y <= c.Position.Y + c.Height)
                        {
                            sign = 1;
                            yMax = c.Position.Y + c.Height;
                            yMin = c.Position.Y;
                            break;
                        }
                    }

                    float highY = Math.Clamp(y, yMin, yMax);
                    float lowY = Math.Clamp(y + 12, yMin, yMax);
                    onSideOfHouse = x < depthPoint;
                    if (wasOnSideOfHouse && !onSideOfHouse)
                    {
                        wasOnSideOfHouse = onSideOfHouse;
                        continue;
                    }
                    wasOnSideOfHouse = onSideOfHouse;
                    float xO = !onSideOfHouse ? -4 : 0;
                    Vector2[] facePoints = new Vector2[6];
                    if (onSideOfHouse)
                    {
                        facePoints[0] = new(x, highY); //topLeft
                        facePoints[1] = new(x + 8, highY); //topRight
                        facePoints[2] = new(x + sign * 4, lowY); //bottomMiddle

                        facePoints[3] = new(x + sign * 4, lowY); //bottomMiddle
                        facePoints[4] = new(x + 8, highY); //topRight
                        facePoints[5] = new(x + 8, lowY); //bottomRight
                    }
                    else
                    {
                        facePoints[0] = new(x + xO, highY); //topLeft
                        facePoints[1] = new(x + 8 + xO, highY);//topRight
                        facePoints[2] = new(x + sign * 4 + xO, lowY);//bottomMiddle
                        facePoints[3] = new(x + sign * 4 + xO, lowY); //bottomMiddle
                        facePoints[4] = new(x + xO, highY); //topLeft
                        facePoints[5] = new(x - 4 + xO, lowY); //bottomLeft
                        bool abort = false;
                        int hitboxSize = 3;
                        foreach (PatternBlocker component in Scene.Tracker.GetComponents<PatternBlocker>())
                        {
                            if (component.Entity != null && component.Entity.Collider != null && component.Entity.Collidable)
                            {
                                foreach (Vector2 v in facePoints)
                                {
                                    Vector2 point = v;
                                    if (!facingRight)
                                    {
                                        point.X = (Width / 2) + (Width / 2) - point.X;
                                    }
                                    Rectangle r = new Rectangle((int)(X + point.X) - hitboxSize / 2, (int)(Y + point.Y) - hitboxSize / 2, hitboxSize, hitboxSize);
                                    if (component.Entity.CollideRect(r))
                                    {
                                        abort = true;
                                        break;
                                    }
                                }
                            }
                            if (abort) break;
                        }
                        if (abort) continue;

                    }
                    points.AddRange(facePoints);
                    indices.Add(count++);
                    indices.Add(count++);
                    indices.Add(count++);
                    indices.Add(count++);
                    indices.Add(count++);
                    indices.Add(count++);
                }

                xOffset = xOffset == 0 ? 8 : 0;
            }
            (Color top, Color bottom) = RoofExtendLeft ? roofDepthColors : roofColors;
            roofVertices[0].Color = roofVertices[1].Color = top;
            roofVertices[2].Color = roofVertices[3].Color = bottom;
            roofVertices[4].Color = roofVertices[5].Color = roofColors.top;
            roofVertices[6].Color = roofVertices[7].Color = roofColors.bottom;
            roofVertices[1].Position.X = roofVertices[3].Position.X = roofVertices[4].Position.X = roofVertices[6].Position.X = DepthChangePoint * 16;
            roofVertices[0].Position.X = -(RoofExtendLeft ? 4 : 0);
            roofVertices[7].Position.X = (int)Width + (RoofExtendRight ? 4 : 0);
            roofVertices[5].Position.X = (int)Width + 8;
            shadeVertices[1].Position.X = shadeVertices[3].Position.X = (int)Width;
            if (!facingRight)
            {
                for (int i = 0; i < roofVertices.Length; i++)
                {
                    roofVertices[i].Position.X = Width / 2 + ((Width / 2) - roofVertices[i].Position.X);
                }
            }
            for (int i = 0; i < roofVertices.Length; i++)
            {
                roofVertices[i].Position.X += BufferExtend;
                roofVertices[i].Position.Y += BufferExtend;
            }
            for (int i = 0; i < shadeVertices.Length; i++)
            {
                shadeVertices[i].Position.X += BufferExtend;
                shadeVertices[i].Position.Y += BufferExtend;
            }
            Points = [.. points];
            Vertices = new HouseVertex[Points.Length];
            bool darkside = false;
            (Color, Color) colors = (Color.Lime, Color.Green);

            for (int i = 0; i < Points.Length; i += 3)
            {
                int r = Calc.Random.Range(0, 3);
                Color faceBase = colors.Item1;
                for (int j = 0; j < 3; j++)
                {
                    Vector2 point = Points[i + j];
                    float yMult = point.Y == Height || point.Y == 0 ? 0 : 1;

                    Color color = faceBase;
                    if (j == 2)
                    {
                        color = Color.Lerp(faceBase, darkside ? Color.Black : Color.White, 0.3f);
                    }
                    float yMax = Height;
                    float yMin = 0;
                    Connector connector = null;
                    foreach (Connector c in Connections)
                    {
                        if (c.Height > 0 && point.Y >= c.Position.Y && point.Y <= c.Position.Y + c.Height)
                        {
                            connector = c;
                            yMax = connector.Position.Y + connector.Height;
                            yMin = connector.Position.Y;
                            break;
                        }
                    }

                    if (point.X < DepthChangePoint * 16 && connector == null)
                    {
                        color = Color.Lerp(color, Color.Black, 0.5f);
                    }
                    if (point.X == DepthChangePoint * 16 || yMax - point.Y < 4 || point.Y - yMin < 0)
                    {
                        yMult = 0;
                    }
                    if (Facing == Facings.Left)
                    {
                        point.X = (Width / 2) + ((Width / 2) - point.X);
                    }
                    Vector2 pos = new Vector2(Calc.Clamp(point.X + BufferOffset.X, 0, Target.Width), point.Y + BufferOffset.Y);

                    Vertices[i + j] = new HouseVertex(new Vector3(pos, 0), color, Vector2.One * yMult);
                }
                colors = (colors.Item2, colors.Item1);
                darkside = !darkside;
            }
            this.indices = indices.ToArray();
        }
        public override void Update()
        {
            base.Update();
            OnScreen = Bounds.OnScreen(SceneAs<Level>(), 16);
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level) return;
            Draw.SpriteBatch.Draw(BgTarget, Position - BufferOffset, Color.White);
            Draw.SpriteBatch.Draw(Target, Position - BufferOffset, Color.White);

        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Platform.RemoveSelf();
            Target.Dispose();
            BgTarget.Dispose();
        }

        [Tracked]
        internal class FormativeHouseRenderHelper : Entity
        {
            public List<Entity> Entities = new();
            private static int[] roofIndices = [0, 1, 2, 2, 1, 3, 4, 5, 6, 6, 5, 7];
            private static int[] shadeIndices = [0, 1, 2, 2, 1, 3];

            public FormativeHouseRenderHelper() : base()
            {
                Tag |= Tags.TransitionUpdate;
                Add(new BeforeRenderHook(BeforeRender));
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                Entities = Scene.Tracker.GetEntities<PassengerHouse>();
            }
            public override void Update()
            {
                base.Update();
                Entities = Scene.Tracker.GetEntities<PassengerHouse>();
            }
            public void BeforeRender()
            {
                if (Scene is not Level level || Entities.Count == 0) return;
                Draw.SpriteBatch.StandardBegin(Matrix.Identity);
                Effect effect = ShaderHelperIntegration.GetEffect("PuzzleIslandHelper/Shaders/formativeHouseShader");
                EffectParameterCollection parameters = effect.Parameters;
                Vector2 vector = new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height);
                Matrix matrix = Matrix.CreateScale(1f / vector.X * 2f, (0f - 1f / vector.Y) * 2f, 1f);
                matrix *= Matrix.CreateTranslation(-1f, 1f, 0f);
                matrix *= Matrix.CreateRotationX((float)Math.PI / 3f);
                parameters["World"]?.SetValue(matrix);
                parameters["Time"]?.SetValue(level.TimeActive);
                foreach (PassengerHouse h in Entities)
                {

                    if (h.OnScreen)
                    {
                        h.Target.SetAsTarget(true);
                        GFX.DrawIndexedVertices(Matrix.Identity, h.Vertices, h.Vertices.Length, h.indices, h.indices.Length / 3, effect);
                        GFX.DrawIndexedVertices(Matrix.Identity, h.roofVertices, 8, roofIndices, 4);
                        GFX.DrawIndexedVertices(Matrix.Identity, h.shadeVertices, 4, shadeIndices, 2);
                    }
                }
                Draw.SpriteBatch.End();

            }

        }
    }
}