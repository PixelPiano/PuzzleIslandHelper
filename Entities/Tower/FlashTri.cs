using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.DEBUG;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Tower
{
    public class FlashTri : Component
    {
        public int Index;
        public VertexPositionColor[] Vertices = new VertexPositionColor[3];
        private Color color;
        public Vector2 Anchor;
        public float Alpha
        {
            get => alpha;
            set
            {
                alpha = value;
                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vertices[i].Color = color * value;
                }
            }
        }
        public float AlphaRate = 15;
        private float alpha = 0;
        public FlashTri(Vector2 anchor, Vector2 p1, Vector2 p2, bool sortRight) : base(true, true)
        {
            color = (sortRight ? Color.Cyan : Color.Yellow) * 0.5f;
            Vertices[0].Position = new Vector3(Anchor, 0);
            Vertices[1].Position = new Vector3(p1, 0);
            Vertices[2].Position = new Vector3(p2, 0);
            Alpha = 0;
        }
        public override void Update()
        {
            base.Update();
            if (AlphaRate > 0 && Alpha != 0)
            {
                Alpha = Calc.Approach(Alpha, 0, AlphaRate * Engine.DeltaTime);
            }
        }
        public void DrawFlashTri(Matrix matrix)
        {
            if (Alpha > 0)
            {
                GFX.DrawVertices(matrix, Vertices, 3);
            }
        }
    }
}