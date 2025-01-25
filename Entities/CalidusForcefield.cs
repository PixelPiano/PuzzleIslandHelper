using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/CalidusForcefield")]
    public class CalidusForcefield : Entity
    {
        [Tracked]
        public class ForcefieldPoint : Component
        {
            public static Color[] Colors = [Color.Orange, Color.Red, Color.Yellow, Color.OrangeRed, Color.YellowGreen, Color.White];
            public Vector2[] Points;
            private Vector2[] origPoints;
            private int[] origIndices;
            public VertexPositionColor[] Vertices;
            public int[] Indices;
            public string Flag;
            public bool FlagState;
            public Vector2 Scale;
            public ForcefieldPoint(Vector2 position, Vector2[] points, int[] indices, Vector2 scaleFactor, string flag) : base(true, true)
            {
                Scale = scaleFactor;
                origPoints = points;
                origIndices = indices;
                Flag = flag;
            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                List<Vector2> p = [.. origPoints];
                List<int> ind = [.. origIndices];
                /*              for (int i = 1; i < origPoints.Length; i++)
                              {
                                  Vector2 prev = origPoints[i - 1];
                                  Vector2 current = origPoints[i];
                                  for(int r = 0; r<Calc.Random.Range(0, 4); i++)
                                  {
                                      Vector2 inter = 
                                  }
                              }*/
                Points = [.. p.Select(item => item * Scale)];
                Indices = [.. ind];
                Vertices = new VertexPositionColor[Points.Length];
                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vertices[i] = new VertexPositionColor(new(Points[i], 0), Colors.Random());
                }
            }
            public override void Update()
            {
                base.Update();
                FlagState = Flag.GetFlag();
                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vertices[i].Position = new Vector3(Entity.Position + Points[i] + Calc.Random.ShakeVector() * 2, 0);
                }
            }
            public void Draw(Level level)
            {
                if (FlagState)
                {
                    DrawLines(level.Camera.Matrix, Vertices, Vertices.Length, Indices,Indices.Length - 1);
                }
            }
            public void DrawLines<T>(Matrix matrix, T[] vertices, int vertexCount, int[] indices, int primitiveCount, Effect effect = null, BlendState blendState = null, Color? solidColor = null) where T : struct, IVertexType
            {
                Effect obj = ((effect != null) ? effect : GFX.FxPrimitive);
                BlendState blendState2 = ((blendState != null) ? blendState : BlendState.AlphaBlend);
                Vector2 vector = new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height);
                matrix *= Matrix.CreateScale(1f / vector.X * 2f, (0f - 1f / vector.Y) * 2f, 1f);
                matrix *= Matrix.CreateTranslation(-1f, 1f, 0f);
                Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                Engine.Instance.GraphicsDevice.BlendState = blendState2;
                obj.Parameters["World"]?.SetValue(matrix);
                if (solidColor.HasValue)
                {
                    obj.Parameters["Color"]?.SetValue(solidColor.Value.ToVector4());
                    obj.Parameters["Shift"]?.SetValue(1);
                }
                else
                {
                    obj.Parameters["Shift"]?.SetValue(0);
                }
                foreach (EffectPass pass in obj.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, vertices, 0, vertexCount, indices, 0, primitiveCount);
                }
            }
        }
        public List<ForcefieldPoint> Points;
        public string Prefix;
        public ForcefieldPoint LeftPoint, RightPoint, BottomPoint, Eye, LeftEdge, RightEdge;
        public CalidusForcefield(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Prefix = data.Attr("flagPrefix");
            Collider = new Hitbox(data.Width, data.Height);
        }
        public override void Update()
        {
            base.Update();
            bool value = false;
            foreach (var p in Points)
            {
                value |= p.FlagState;
            }
            "CalidusForcefieldActivated".SetFlag(value);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Vector2[] left = [new(2, 13), new(7, 13), new(12, 13), new(12, 7), new(12, 2), new(12, 19), new(12, 24)];
            int[] tInd = [0, 1, 2, 3, 5, 4, 6];
            Vector2[] right = [.. left.Select(item => new Vector2(64 - item.X, item.Y))];
            Vector2[] lower = [.. left.Select(item => item.RotateAround(new Vector2(32, 32), -90))];
            Vector2[] eye = [new(28, 7), new(35, 7), new(38, 13), new(35, 20), new(28, 20), new(25, 13)];
            int[] eyeInd = [0, 1, 4, 5, 2, 3, 0];
            List<Vector2> bottomLeft = [];
            List<Vector2> bottomRight = [];
            int[] lowerInd = new int[8];
            for (int i = 0; i < 7; i++)
            {
                lowerInd[i] = i;
                bottomLeft.Add(new Vector2(i * 5, 26 + i * 5));
                bottomLeft.Add(new Vector2(i * 5, 34 + i * 5));
                bottomRight.Add(new Vector2(63 - i * 5, 26 + i * 5));
                bottomRight.Add(new Vector2(63 - i * 5, 34 + i * 5));
            }
            lowerInd[^1] = 0;
            Vector2 scaleFactor = Collider.Size / new Vector2(64);
            LeftPoint = new(Vector2.Zero, left, tInd, scaleFactor, Prefix + "1");
            RightPoint = new(Vector2.Zero, right, tInd, scaleFactor, Prefix + "2");
            BottomPoint = new(Vector2.Zero, lower, tInd, scaleFactor, Prefix + "3");
            Eye = new(Vector2.Zero, eye, eyeInd, scaleFactor, Prefix + "4");
            LeftEdge = new(Vector2.Zero, [.. bottomLeft], lowerInd, scaleFactor, Prefix + "5");
            RightEdge = new(Vector2.Zero, [.. bottomRight], lowerInd, scaleFactor, Prefix + "6");
            Add(LeftPoint, RightPoint, BottomPoint, Eye, LeftEdge, RightEdge);
            Points = [LeftPoint, RightPoint, BottomPoint, Eye, LeftEdge, RightEdge];
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level) return;
            Draw.SpriteBatch.End();
            foreach (var p in Points)
            {
                p.Draw(level);
            }
            GameplayRenderer.Begin();
        }
    }
}