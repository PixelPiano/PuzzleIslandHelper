using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue;
using Celeste.Mod.PuzzleIslandHelper.Components;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{

    public class LineMesh<T> : IDisposable where T : struct, IVertexType
    {
        private List<T> _vertices = new List<T>();

        private List<int> _indices = new List<int>();

        public T[] Vertices { get; private set; }

        public int[] Indices { get; private set; }

        public int VertexCount => Baked ? Vertices.Length : _vertices.Count;

        public int Lines { get; private set; }

        public bool Baked { get; private set; }

        public void AddVertex(T vertex)
        {
            if (Baked)
            {
                throw new InvalidOperationException("Cannot add a vertex to a baked mesh!");
            }

            _vertices.Add(vertex);
        }

        public void AddVertices(params T[] vertices)
        {
            if (Baked)
            {
                throw new InvalidOperationException("Cannot add vertices to a baked mesh!");
            }

            _vertices.AddRange(vertices);
        }
        public void AddLine(int a, int b)
        {
            if (Baked)
            {
                throw new InvalidOperationException("Cannot add indices to a baked mesh!");
            }

            _indices.Add(a);
            _indices.Add(b);
            Lines++;
        }
        public void AddTriangle(int a, int b, int c)
        {
            if (Baked)
            {
                throw new InvalidOperationException("Cannot add indices to a baked mesh!");
            }

            _indices.Add(a);
            _indices.Add(b);
            _indices.Add(a);
            _indices.Add(c);
            _indices.Add(b);
            _indices.Add(c);
            Lines += 3;
        }
        public void Bake()
        {
            if (Baked)
            {
                throw new InvalidOperationException("Cannot bake mesh that was already baked!");
            }

            Vertices = _vertices.ToArray();
            Indices = _indices.ToArray();
            Baked = true;
        }

        public void Celeste_DrawVertices(Effect effect, Matrix? m = null)
        {
            if (!Baked)
            {
                throw new InvalidOperationException("A mesh must be baked in order for its vertices to be drawn!");
            }
            Matrix matrix = m ?? Matrix.Identity;
            Effect obj = ((effect != null) ? effect : GFX.FxPrimitive);
            BlendState blendState2 = BlendState.AlphaBlend;
            Vector2 vector = new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height);
            matrix *= Matrix.CreateScale(1f / vector.X * 2f, (0f - 1f / vector.Y) * 2f, 1f);
            matrix *= Matrix.CreateTranslation(-1f, 1f, 0f);
            Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            Engine.Instance.GraphicsDevice.BlendState = blendState2;
            obj.Parameters["World"].SetValue(matrix);
            foreach (EffectPass pass in obj.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, Vertices, 0, Vertices.Length, Indices, 0, Lines);
            }
        }

        public void Draw()
        {
            if (!Baked)
            {
                throw new InvalidOperationException("A mesh must be baked in order for its vertices to be drawn!");
            }

            if (VertexCount != 0)
            {
                Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, Vertices, 0, Vertices.Length, Indices, 0, Lines);
            }
        }

        public void Dispose()
        {
            _vertices = null;
            Vertices = null;
            _indices = null;
            Indices = null;
        }
    }
}
