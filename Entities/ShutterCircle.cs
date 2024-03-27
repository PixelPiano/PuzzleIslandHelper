
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ShutterCircle")]
    [Tracked]
    public class ShutterCircle : Actor
    {

        public float LookSpeed = 1;
        private const int NumPoints = 8;
        private const int MaxScale = 5;
        private float InnerScale = 1;
        private ShapePoint[] Outer = new ShapePoint[NumPoints];
        private ShapePoint[] Inner = new ShapePoint[NumPoints];
        private struct ShapePoint
        {
            public float X
            {
                get
                {
                    return Position.X;
                }
                set
                {
                    Position.X = value;
                }
            }
            public float Y
            {
                get
                {
                    return Position.Y;
                }
                set
                {
                    Position.Y = value;
                }
            }
            public float Z;
            public Color PointColor;
            private Color orig_Color;
            public Vector2 Position;
            public float Speed = 1;
            public Vector2 Destination;

            public ShapePoint(Vector2 Position, Color Color)
            {
                this.Position = Position;
                Destination = Position;
                PointColor = Color;
                orig_Color = Color;
            }
            public void Update()
            {
                Position = Calc.Approach(Position, Destination, Speed);
                PointColor = ZColor();
            }
            public Color ZColor()
            {
                if (Z < 0)
                {
                    return Color.Lerp(orig_Color, Color.Black, Z / -100);
                }
                else
                {
                    return Color.Lerp(orig_Color, Color.White, Z / 100);
                }

            }
        }


        public ShutterCircle(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Tag |= Tags.TransitionUpdate;
        }
        private Color MixColor(ShapePoint a, ShapePoint b)
        {
            return Color.Lerp(a.PointColor, b.PointColor, 0.5f);
        }
        private void DrawPoints(ShapePoint[] array)
        {
            for (int i = 1; i < array.Length; i++)
            {
                Draw.Line(array[i - 1].Position, array[i].Position, MixColor(array[i - 1], array[i]));
            }
            Draw.Line(array[NumPoints - 1].Position, array[0].Position, MixColor(array[NumPoints - 1], array[0]));
        }
        public override void Render()
        {
            base.Render();
            DrawPoints(Outer);
            DrawPoints(Inner);
            for (int i = 0; i < NumPoints; i++)
            {
                Draw.Line(Outer[i].Position, Inner[i].Position, MixColor(Outer[i], Inner[i]));
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            float Radius = 30;
            Color color = Color.Green;
            for (int i = 0; i < NumPoints; i++)
            {
                float angle = ((i + (360f / Outer.Length * i)) % 360).ToRad();
                Outer[i] = new ShapePoint(Position + Calc.AngleToVector(angle, Radius), color);
                Inner[i] = new ShapePoint(Position + Calc.AngleToVector(angle, Radius / 2), color);
            }
            Tween ScaleTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.Linear, 2);
            ScaleTween.OnUpdate = (Tween t) =>
            {
                InnerScale = Calc.LerpClamp(-MaxScale, MaxScale, MaxScale * t.Eased);
                for (int i = 0; i < NumPoints; i++)
                {
                    Inner[i].Z = Calc.LerpClamp(-100, 100, t.Eased);
                }
            };
            Add(ScaleTween);
            ScaleTween.Start();
            Add(new Coroutine(AdjustPoints()));
        }
        public override void Update()
        {
            base.Update();
            for (int i = 0; i < NumPoints; i++)
            {
                Outer[i].Update();
                Inner[i].Update();
            }
        }
        private IEnumerator AdjustPoints()
        {
            float Radius = 30;
            float distance = Width;
            while (true)
            {
                for (int i = 0; i < 360; i += 2)
                {

                    for (int j = 0; j < NumPoints; j++)
                    {
                        float angle = ((i + (360f / Outer.Length * j)) % 360).ToRad();
                        float length = distance + Radius;
                        Vector2 target = Position + Calc.AngleToVector(angle, length);
                        Vector2 endPos = target + new Vector2(Width / 4, Height / 4);
                        Outer[j].Destination = endPos;
                        Inner[j].Destination = endPos + Calc.AngleToVector(angle, length / 2 * InnerScale);
                    }
                    yield return null;
                }
            }
        }
    }
}


