using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    //[CustomEntity("PuzzleIslandHelper/WipEntity")]
    [Tracked]
    public class Bitter : Actor
    {
        public Vector2[] Points = new Vector2[] { new(0, 0.5f),new(0.25f,0f),new(0.25f,1f),new(0.5f,0.5f),new(1, 0.25f), new(1, 0.75f) };
        public int[] Indices = new int[] { 0, 1, 2,  1, 3, 2,  3, 4, 5 };

        public VertexPositionColor[] Vertices;
        public Facings Facing = Facings.Right;
        public Vector2 Speed;
        public static bool Optimize = true;
        public bool OnScreen;
        public Vector2 Size = new Vector2(14, 8);
        public Bitter(EntityData data, Vector2 offset) : this(data.Position + offset)
        {

        }
        public Bitter(Vector2 position) : base(position)
        {
            Depth = 1;
            Vertices = new VertexPositionColor[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                Vertices[i] = new();
            }
            Collider = new Hitbox(Size.X, Size.Y);
            AddTag(Tags.TransitionUpdate);
        }

        public override void Update()
        {
            base.Update();

            if (Scene is not Level level || level.GetPlayer() is not Player player)
            {
                OnScreen = false;
                return;
            }
            OnScreen = level.Camera.GetBounds().Intersects(Collider.Bounds);
            UpdateVertices();
        }
        public void UpdateVertices()
        {
            Vector2 add = Position;
            for (int i = 0; i < Points.Length; i++)
            {
                Vertices[i].Position = new Vector3((Points[i] * Size + add).Round(), 0);
                Vertices[i].Color = Color.Lime;
            }
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level || !OnScreen) return;
            Draw.SpriteBatch.End();
            GFX.DrawIndexedVertices(level.Camera.Matrix, Vertices, Vertices.Length, Indices, 3);
            GameplayRenderer.Begin();
        }
    }
}
