using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class DiamondPulse : Component
    {
        public static readonly Vector2[] Points =
        {
            new(-1,-1), new(0,-1),new(1,-1),
            new(-1,0),            new(1,0),
            new(-1,1),  new(0,1),  new(1,1),

                        new(0,-1),
            new(-1,0),  new(0,0),  new(1,0),
                        new(0,1)
        };
        public static readonly int[] Indices =
        {
            0, 1, 3,  1, 2, 4,
            3, 6, 5,  6, 4, 7,
            9, 8, 10, 8, 11, 10,
            9, 10, 12, 10, 11, 12
        };
        public static readonly Vector2[] LineCache =
        {
            new(-1, 0), new(0, -1), new(1, 0), new(0,1)
        };
        public enum Modes
        {
            Fill,
            Line
        }
        public Modes Mode;
        public VertexPositionColor[] Vertices;
        public Vector2[] Lines;
        public Vector2 Scale = Vector2.One;
        public Vector2 Position;
        public float LineScaleAddition;
        public float Alpha = 1;
        public int Size;

        public float PulseTime;
        public float FadeOutAfter;
        public float Expand;
        public Vector2 ScaleOffset
        {
            get
            {
                float add = Mode is Modes.Fill ? 0 : LineScaleAddition;
                return Scale * ((Size / 2) + add);
            }
        }
        public Vector2 RenderPosition
        {
            get
            {
                return ((Entity == null) ? Vector2.Zero : Entity.Position) + Position;
            }
            set
            {
                Position = value - ((Entity == null) ? Vector2.Zero : Entity.Position);
            }
        }
        public DiamondPulse(int size) : base(true, true)
        {
            Size = size;
            Vertices = new VertexPositionColor[Points.Length];
            Lines = new Vector2[LineCache.Length];

            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = new VertexPositionColor(new Vector3(Points[i], 0), i < 8 ? Color.Transparent : Color.White);
            }

        }
        public void UpdateVertices(Vector2 position)
        {
            Vector2 scaleOffset = Scale * (Size / 2);

            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Position = new Vector3((Points[i] * scaleOffset) + position, 0);
                Vertices[i].Color = i < 8 ? Color.Transparent : Color.White * Alpha;
            }

        }
        public override void Update()
        {
            base.Update();
            UpdateVertices(RenderPosition);
        }
        public void DrawSquare(Level level)
        {
            GFX.DrawIndexedVertices(level.Camera.Matrix, Vertices, Vertices.Length, Indices, 8);
        }
        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.End();
            DrawSquare(SceneAs<Level>());
            GameplayRenderer.Begin();
        }
    }
}
