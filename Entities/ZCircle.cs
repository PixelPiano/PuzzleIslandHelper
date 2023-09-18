using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
// PuzzleIslandHelper.FadeWarp
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ZCircle")]
    [Tracked]
    public class ZCircle : Entity
    {
        public struct DepthPoint
        {
            public Vector2 orig_Point;
            public bool MovingLeft;
            public float X
            {
                get
                {
                    return XYZ.X;
                }
                set
                {
                    X = value;
                }
            }
            public float Y
            {
                get
                {
                    return XYZ.Y;
                }
                set
                {
                    Y = value;
                }
            }
            public float Z
            {
                get
                {
                    return XYZ.Z;
                }
                set
                {
                    Z = value;
                }
            }
            public Vector2 Position
            {
                get
                {
                    return new Vector2(X, Y);
                }
                set
                {
                    X = value.X;
                    Y = value.Y;
                }
            }
            private Vector3 XYZ;
            public Color orig_Color;
            public Color Color;

            public DepthPoint(float x, float y, float z, Color color)
            {
                XYZ = new Vector3(x, y, z);
                orig_Point = new Vector2(x, y);
                Color = color;
                orig_Color = color;
            }
        }
        private float SpinTime = 3;
        private List<DepthPoint> Points = new();
        private float Size;
        private int BaseDepth;
        private int NumPoints;
        private Color Color;
        private float Eased = 1;
        private float Mult = 1;

        public ZCircle(EntityData data, Vector2 offset) : this(data.Position + offset, data.Float("size"), data.Int("depth"), data.Int("points")) { }
        public ZCircle(Vector2 Position, float size, int baseDepth, int points) : base(Position)
        {
            Size = size;
            BaseDepth = baseDepth;
            NumPoints = points;
            Color = Color.Green;
            Collider = new Hitbox(size, size);

            Add(new BeforeRenderHook(BeforeRender));
        }
        private IEnumerator Spin()
        {
            while (true)
            {
                for (float i = 0; i < SpinTime; i += Engine.DeltaTime)
                {
                    Eased = i / SpinTime;
                    yield return null;
                }
            }
        }
        public void SetPoints()
        {
            for (int i = 0; i < NumPoints; i++)
            {
                Vector2 xyPosition = Center + Calc.AngleToVector((360f / NumPoints * i).ToRad(), (int)Size / 2).ToInt();
                Points.Add(new DepthPoint(xyPosition.X, xyPosition.Y, BaseDepth, Color));

            }
        }
        public void ChangePoints()
        {
            for (int i = 0; i < NumPoints; i++)
            {
                Vector2 xyPosition = Center + Calc.AngleToVector((360f / NumPoints * i).ToRad(), Vector2.Distance(Points[i].Position,Center)).ToInt();
                Points[i] = new DepthPoint(xyPosition.X, xyPosition.Y, BaseDepth, Color);

            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            SetPoints();
            Add(new Coroutine(Spin()));
        }
        private void BeforeRender()
        {

        }
        public override void Render()
        {
            base.Render();
            DrawLines();
        }
        private void SetPointProgress()
        {

        }
        public override void Update()
        {
            base.Update();
            ChangePoints();
        }
        private Color GetPointColor(int index)
        {

            return Points[index].Color;
        }
        private void DrawLines()
        {
            //Circle(Position);
            for (int i = 1; i < Points.Count; i++)
            {
                Draw.Line(Points[i - 1].Position, Points[i].Position, GetPointColor(i - 1));
            }
            Draw.Line(Points[0].Position, Points[Points.Count - 1].Position, GetPointColor(0));
        }
        public void Circle(Vector2 position)
        {
            float radius = Size / 2;
            Vector2 vector = Vector2.UnitX * radius;
            Vector2 vector2 = vector.Perpendicular();
            for (int i = 1; i <= Points.Count; i++)
            {
                Color color = GetPointColor(i - 1);
                Vector2 vector3 = Calc.AngleToVector(i * ((float)Math.PI / 2f) / Points.Count, radius);
                Vector2 vector4 = vector3.Perpendicular();
                Draw.Line(position + vector, position + vector3, color);
                Draw.Line(position - vector, position - vector3, color);
                Draw.Line(position + vector2, position + vector4, color);
                Draw.Line(position - vector2, position - vector4, color);
                vector = vector3;
                vector2 = vector4;
            }
        }

    }
}