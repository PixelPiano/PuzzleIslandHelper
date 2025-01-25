using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/Polygon")]
    public class Polygon : Entity
    {
        public VertexPositionColor[] Vertices;
        public Vector2[] Points;
        public Color[] Colors;
        public int[] Indices;
        public int Triangles;
        public float[] FloatMults;
        public float[] FloatAmount;
        public float TargetFloat;
        private bool randomizeStartFloat;
        private float floatTime;

        public Polygon(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            TargetFloat = data.Float("maxFloat", 8);
            randomizeStartFloat = data.Bool("randomizeStartFloat");
            floatTime = data.Float("floatTime");
            var d = data.Attr("indices").Replace(" ", "").Split(',');
            List<int> list = [];
            foreach (string s in d)
            {
                if (!string.IsNullOrEmpty(s) && int.TryParse(s, NumberStyles.Integer, null, out int result))
                {
                    list.Add(result);
                }
            }
            var d2 = data.Attr("floatMults").Replace(" ", "").Split(',');
            List<float> list2 = [];
            foreach (string s2 in d2)
            {
                if (!string.IsNullOrEmpty(s2) && float.TryParse(s2, NumberStyles.AllowDecimalPoint, null, out float result2))
                {
                    list2.Add(result2);
                }
            }
            var c = data.Attr("colors").Replace(" ", "").Split(',');
            List<Color> list3 = [];
            foreach (string s3 in c)
            {
                if (s3.Length >= 5)
                {
                    if (s3.Length > 7)
                    {
                        char x = s3[6];
                        if (x is 'x' or 'X' && int.TryParse(s3.AsSpan(7), NumberStyles.Integer, null, out int loops))
                        {
                            for (int j = 0; j < loops; j++)
                            {
                                list3.Add(Calc.HexToColor(s3[..6]));
                            }
                        }
                    }
                    else
                    {
                        list3.Add(Calc.HexToColor(s3));
                    }
                }
            }
            Colors = [.. list3];
            Points = data.NodesWithPosition(offset);
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] -= Position;
            }
            Vertices = Points.Select(item => new VertexPositionColor(new Vector3(Position + item, 0), Color.White)).ToArray();
            if (Colors.Length > 0)
            {
                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vertices[i].Color = Colors[i % Colors.Length];
                }
            }
            Indices = [.. list];
            for (int i = FloatMults.Length; i < Vertices.Length; i++)
            {
                list2.Add(0);
            }
            FloatMults = [.. list2];
            FloatAmount = new float[FloatMults.Length];
            Triangles = Indices.Length / 3;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            for (int i = 0; i < FloatAmount.Length; i++)
            {
                int j = i;
                Tween tween = Tween.Set(this, Tween.TweenMode.YoyoLooping, floatTime / 2, Ease.SineInOut, t => FloatAmount[j] = Calc.LerpClamp(0, TargetFloat, t.Eased));
                if (randomizeStartFloat)
                {
                    tween.Randomize();
                }
            }
        }
        private bool onScreen;
        public override void Update()
        {
            base.Update();
            onScreen = false;
            for (int i = 0; i < Vertices.Length; i++)
            {
                
                Vertices[i].Position.Y = Position.Y + Points[i].Y + (FloatAmount[i] * TargetFloat) * FloatMults[i];
                Vertices[i].Position.X = Position.X + Points[i].X;
                Vertices[i].Color = Colors[i];
                Vector2 p = Vertices[i].Position.XY();
                if (!onScreen && p.OnScreen(4))
                {
                    onScreen = true;
                }
            }
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level || !onScreen) return;
            Draw.SpriteBatch.End();
            GFX.DrawIndexedVertices(level.Camera.Matrix, Vertices, Vertices.Length, Indices, Triangles);
            GameplayRenderer.Begin();
        }
    }
}