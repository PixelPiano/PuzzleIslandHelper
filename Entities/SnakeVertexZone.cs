using Celeste.Mod.CommunalHelper.Utils;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static PianoUtils;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/SnakeLine")]
    [Tracked]
    public class SnakeVertexZone : Entity
    {
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SnakeVertex : IVertexType
        {
            public Vector3 Position;

            public Color Color;

            public float Ease;

            public static readonly VertexDeclaration VertexDeclaration;

            VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

            static SnakeVertex()
            {
                VertexDeclaration = new VertexDeclaration(
                    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                    new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                    new VertexElement(16, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0));
            }

            public SnakeVertex(Vector3 position, Color color)
            {
                Position = position;
                Color = color;
                Ease = 0;
            }
            public void SetEase(float ease)
            {
                Ease = ease;
            }
            public override int GetHashCode()
            {
                return 0;
            }

            public override string ToString()
            {
                return "{{Position:" + Position.ToString() + " Color:" + Color.ToString() + " Ease:" + Ease.ToString() + "}}";
            }

            public static bool operator ==(SnakeVertex left, SnakeVertex right)
            {
                if (left.Ease == right.Ease && left.Color == right.Color)
                {
                    return left.Position == right.Position;
                }

                return false;
            }

            public static bool operator !=(SnakeVertex left, SnakeVertex right)
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

                return this == (SnakeVertex)obj;
            }
        }
        public static Effect Effect;
        private Vector2[] points;
        private SnakeVertex[] vertices;

        private int[] indices;
        private float cellWidth;
        private float cellHeight;
        private float rate;
        private float easeTime = 0.4f;
        private Color[] colors;
        private int lines;
        private List<int> usedIndices = [];
        public SnakeVertexZone(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            cellWidth = data.Float("cellWidth");
            cellHeight = data.Float("cellHeight");
            rate = data.Float("rate");
            string[] c = data.Attr("colors").Replace(" ", "").Split(',');
            colors = [.. c.Select(s => Calc.HexToColor(s))];
            Collider = new Hitbox(data.Width, data.Height);
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            SnakeVertex last = vertices[0];
            for (int i = 1; i < vertices.Length; i++)
            {
                Vector2 pos = last.Position.XY();
                SnakeVertex v = vertices[i];
                Draw.Point(pos, Color.Lerp(Color.White, Color.Red, last.Ease));
                Draw.Line(pos, v.Position.XY(), Color.Blue * 0.5f);
                last = v;
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            CreateVertices();
            Add(new Coroutine(catalyst()));
        }
        private IEnumerator catalyst()
        {
            for(int i = 0; i<3; i++)
            {
                EaseLine(Calc.Random.Range(0, vertices.Length), Ease.Linear);
                yield return Calc.Random.Range(Engine.DeltaTime, 0.3f);
            }
        }
        public override void Update()
        {
            base.Update();

        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level) return;
            Effect effect = ShaderHelperIntegration.TryGetEffect("PuzzleIslandHelper/Shaders/snakeLineShader");
            if (effect != null)
            {
                Draw.SpriteBatch.End();
                Celeste_DrawVertices(effect, level.Camera.Matrix);
                GameplayRenderer.Begin();
            }

        }
        public void EaseLine(int index, Ease.Easer ease)
        {
            if(usedIndices.Count >= vertices.Length) return;
            while (usedIndices.Contains(index))
            {

                index = Calc.Random.Range(0, vertices.Length);
            }
            int next = (index + 1) % vertices.Length;
            usedIndices.Add(index);
            Tween.Set(this, Tween.TweenMode.Oneshot, easeTime, ease, t => vertices[index].SetEase(t.Eased),
                t =>
                {
                    vertices[index].SetEase(1);
                    EaseLine(next, ease);
                    Tween.Set(this, Tween.TweenMode.Oneshot, easeTime * 1.5f, ease, t => vertices[index].SetEase(1 - t.Eased), t => {vertices[index].SetEase(0); usedIndices.Remove(index);});
                });


        }
        public void Celeste_DrawVertices(Effect effect, Matrix? m = null)
        {
            Matrix matrix = m ?? Matrix.Identity;
            Effect obj = ((effect != null) ? effect : GFX.FxPrimitive);
            BlendState blendState2 = BlendState.AlphaBlend;
            Vector2 vector = new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height);
            matrix *= Matrix.CreateScale(1f / vector.X * 2f, (0f - 1f / vector.Y) * 2f, 1f);
            matrix *= Matrix.CreateTranslation(-1f, 1f, 0f);
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Engine.Instance.GraphicsDevice.BlendState = blendState2;
            obj.Parameters["World"]?.SetValue(matrix);
            obj.Parameters["Time"]?.SetValue(Scene.TimeActive);
            applyParams(obj);
            PrimitiveType type = PianoModule.Session.DEBUGBOOL4 ? PrimitiveType.LineList : PrimitiveType.LineStrip;

            foreach (EffectPass pass in obj.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(type, vertices, 0, vertices.Length, indices, 0, lines);
            }
        }
        private Effect applyParams(Effect ineffect)
        {
            return ineffect;
        }
        private Vector2 getOffset(int x, int y)
        {
            return new Vector2(Calc.Random.Range(-cellWidth / 2f, cellWidth / 2f), Calc.Random.Range(-cellHeight / 2f, cellHeight / 2f));
        }
        public void CreateVertices()
        {
            List<Vector2> _points = GetGridPoints(Width, Height, cellWidth, cellHeight, getOffset);
            _points.Shuffle();
            points = [.. _points];
            vertices = new SnakeVertex[points.Length];
            List<int> _indices = [];
            indices = new int[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                vertices[i] = new SnakeVertex(new(Position + points[i], 0), colors.Random());
            }
            for (int i = 1; i < points.Length; i += 2)
            {
                _indices.Add(i - 1);
                _indices.Add(i);
                lines++;
            }
            indices = [.. _indices];


        }
    }
}