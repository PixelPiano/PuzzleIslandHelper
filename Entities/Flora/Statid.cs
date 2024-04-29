using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using ExtendedVariants.Entities.ForMappers;
using FrostHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.PuzzleIslandHelper.Components.Segment;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [CustomEntity("PuzzleIslandHelper/Statid")]
    [Tracked]
    public class Statid : Entity
    {
        public class Petal : Component
        {
            public Vector2[] PetalPoints = new Vector2[] { new(0, 0), new(-2, -4), new(2, -4), new(0, -8) };
            public int[] PetalIndices = new int[] { 0, 1, 2, 1, 3, 2 };
            public VertexPositionColor[] Vertices;
            public Vector2 Scale;
            public float Rotation;
            public Vector2 RenderPosition
            {
                get
                {
                    return ((base.Entity == null) ? Vector2.Zero : base.Entity.Position) + Position;
                }
                set
                {
                    Position = value - ((base.Entity == null) ? Vector2.Zero : base.Entity.Position);
                }
            }
            public Vector2 Position;

            public Petal(float rotation) : base(true, true)
            {
                Rotation = rotation;
                Vertices = new VertexPositionColor[PetalPoints.Length];
                for (int i = 0; i < PetalPoints.Length; i++)
                {
                    Vertices[i] = new VertexPositionColor(new Vector3(PetalPoints[i], 0), Color.White);
                }
            }
            public override void Update()
            {
                base.Update();
                UpdateVertices();
            }
            public void UpdateVertices()
            {
                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vertices[i].Position = new Vector3(RenderPosition + PianoUtils.RotatePoint(PetalPoints[i], Vector2.Zero, Rotation), 0);
                }
            }
            public override void Render()
            {
                base.Render();
                if (Scene is not Level level) return;
                GFX.DrawIndexedVertices(level.Camera.Matrix, Vertices, 4, PetalIndices, 2);
            }
        }
        public Petal[] Petals;
        public int PetalCount;

        public Statid(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Collider = new Hitbox(16, 24);
            PetalCount = data.Int("petals");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if(PetalCount <= 0) RemoveSelf();

            Petals = new Petal[PetalCount];
            for (int i = 0; i < PetalCount; i++)
            {
                Petals[i] = new Petal(360f / PetalCount * i);
                Petals[i].Visible = false;
            }
            Add(Petals);
        }
        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.End();
            foreach(Petal p in Petals)
            {
                p.Render();
            }
            GameplayRenderer.Begin();
        }
    }
}
