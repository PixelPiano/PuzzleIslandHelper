using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
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
        public VertexBreath[] Breaths;
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
            Indices = [.. data.Attr("indices").Replace(" ", "").Split(',').Select(item => int.TryParse(item, NumberStyles.Integer, null, out int result) ? result : 0)];
            List<float> list2 = [.. data.Attr("floatMult").Replace(" ", "").Split(',').Select(item => float.TryParse(item, NumberStyles.AllowDecimalPoint, null, out float result2) ? result2 : 0f)];
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
            Vertices = new VertexPositionColor[Points.Length];
            Breaths = new VertexBreath[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                Vertices[i] = new VertexPositionColor(new Vector3(Points[i], 0), Color.White);
                Points[i] -= Position;
            }
            if (Colors.Length > 0)
            {
                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vertices[i].Color = Colors[i % Colors.Length];
                }
            }
            for (int i = list2.Count; i < Vertices.Length; i++)
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
            for (int i = 0; i < Vertices.Length; i++)
            {
                Breaths[i] = new VertexBreath(floatTime, TargetFloat * FloatMults[i], randomizeStartFloat);
            }
            Add(Breaths);
/*            for (int i = 0; i < FloatAmount.Length; i++)
            {
                int j = i;
                Tween tween = Tween.Set(this, Tween.TweenMode.YoyoLooping, floatTime / 2, Ease.SineInOut, t => FloatAmount[j] = Calc.LerpClamp(0, TargetFloat, t.Eased));
                if (randomizeStartFloat)
                {
                    tween.Randomize();
                }
            }*/
            float left = int.MaxValue, right = int.MinValue, top = int.MaxValue, bottom = int.MinValue;
            for (int i = 0; i < Vertices.Length; i++)
            {
                var v = Vertices[i];
                left = Math.Min(left, v.Position.X);
                right = Math.Max(right, v.Position.X);
                top = Math.Min(top, v.Position.Y);
                bottom = Math.Max(bottom, v.Position.Y);
            }
            Collider = new Hitbox(right - left, bottom - top);
        }
        private bool onScreen;
        public override void Update()
        {
            base.Update();
            onScreen = false;
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Position.Y = Position.Y + Points[i].Y + Breaths[i].Amount;
                Vertices[i].Position.X = Position.X + Points[i].X;
                Vertices[i].Color = Colors[i % Colors.Length];
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