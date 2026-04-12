using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// PuzzleIslandHelper.SecurityLaser
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/RaniFrequency")]
    [Tracked]
    public class RaniFrequency : Entity
    {
        public class Letter : GraphicsComponent
        {
            public const int ZMax = 10;
            public class LetterData
            {
                public void Render(Vector2 position, Color color)
                {
                    foreach (Line line in Lines)
                    {
                        line.Render(position, color);
                    }
                }
                public class Line
                {
                    public Vector3 From;
                    public Vector3 To;
                    public Vector3 FromOffset;
                    public Vector3 Center;
                    public Vector3 FromRotated;
                    public Vector3 ToRotated;
                    public Matrix Translation;
                    public Matrix CenterTranslation;
                    public Line(Vector3 from, Vector3 to, Vector3 origin)
                    {
                        From = from;
                        To = to;
                        Center = (from + to) / 2;
                        FromOffset = From - Center;
                        Translation = Matrix.CreateTranslation(Center);
                        CenterTranslation = Matrix.CreateTranslation(origin);
                    }
                    public void Render(Vector2 position, Color color)
                    {
                        Draw.Line(FromRotated.XY() + position, ToRotated.XY() + position, color);
                    }
                    public void Update(Matrix rotation, Matrix fromRotation, Matrix toRotation)
                    {
                        Vector3 fromOffsetRotated = Vector3.Transform(FromOffset, rotation * fromRotation);
                        Vector3 toOffsetRotated = Vector3.Transform(FromOffset, rotation * toRotation);
                        Vector3 centerRotated = Vector3.Transform(Center, rotation);
                        FromRotated = centerRotated + fromOffsetRotated;
                        ToRotated = centerRotated + toOffsetRotated;
                    }
                }
                public List<Line> Lines = [];
                public Vector3 Scale;
                public LetterData(float width, float height, params Vector3[] points)
                {
                    Scale = new Vector3(width, height, ZMax);
                    for (int i = 1; i < points.Length; i += 2)
                    {
                        Lines.Add(new Line(points[i - 1] * Scale, points[i] * Scale, new Vector3(0)));
                    }
                }
                public void Update(float rotation)
                {
                    Matrix z = Matrix.CreateRotationZ(rotation);
                    Matrix letterRotation = Matrix.CreateRotationY(rotation);
                    Matrix fromRotation = Matrix.CreateRotationX(rotation) * z;
                    Matrix toRotation = letterRotation * -z;
                    foreach (Line line in Lines)
                    {
                        line.Update(letterRotation, fromRotation, toRotation);
                    }
                }
            }
            public LetterData Data;
            public int TargetFrequency;
            public float CurrentFrequency;
            public float LineRotation;
            public float LetterRotation;
            public float LineRotationOffset;
            public float LetterRotationOffset;
            public float Width, Height;
            public List<Vector3> Points = [];
            public int Index;
            public Letter(char letter, int index, Vector2[][] points) : base(true)
            {
                Index = index;
                TargetFrequency = letter - 'A' + 1;
                CenterOn(TargetFrequency);
                Color = Color.White;
                Calc.PushRandom(letter);
                List<Vector3> vector3s = [];
                foreach (Vector2[] p in points)
                {
                    vector3s.Add(new Vector3(p[0] - new Vector2(0.5f, 0.5f), Calc.Random.NextFloat()));
                    vector3s.Add(new Vector3(p[1] - new Vector2(0.5f, 0.5f), Calc.Random.NextFloat()));
                    for (int i = 2; i < p.Length; i++)
                    {
                        vector3s.Add(new Vector3(p[i - 1] - new Vector2(0.5f, 0.5f), Calc.Random.NextFloat()));
                        vector3s.Add(new Vector3(p[i] - new Vector2(0.5f, 0.5f), Calc.Random.NextFloat()));
                    }

                }
                Points = vector3s;
                Calc.PopRandom();
            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                Width = entity.Width / 6f;
                Height = entity.Height * 0.8f;
                Position.X = entity.Width / 4f * Index + Width / 8f * (Index + 1);
                Position.Y = entity.Height * 0.1f;
                Data = new LetterData(Width, Height, [.. Points]);
            }
            public override void Render()
            {
                Vector2 renderPosition = Entity.Position + Position;
                Vector2 halfSize = new Vector2(Width / 2, Height / 2);
                Data.Render(renderPosition + halfSize, Color);
            }
            public void CenterOn(int frequency)
            {
                LetterRotationOffset = -MathHelper.ToRadians(360f * (frequency / FrequencyData.Max));
            }
            public override void Update()
            {
                float angle = MathHelper.ToRadians(360f * (CurrentFrequency / FrequencyData.Max));
                LetterRotation = LetterRotationOffset + angle;
                Color = Math.Abs(LetterRotation) < 0.05 ? Color.Red : Color.White;
                Data.Update(LetterRotation);
            }
        }
        private static Vector2[][] rPoints =
        {
            [new(0, 1),new(0,.5f),new(0, 0),new(1, 0),new(1,.5f),new(0,.5f),new(1, 1)],
        };
        private static Vector2[][] aPoints =
        {
            [new(0,1), new(0,0.5f),new(0,0),new(1,0),new(1,0.5f),new(1,1)],
            [new(1,0.5f),new(0,0.5f)]
        };
        private static Vector2[][] nPoints =
        {
            [new(0,1),new(0,0.5f),new(0,0),new(0.5f,0.5f),new(1,1),new(1,0.5f),new(1,0)]
        };
        private static Vector2[][] iPoints =
        {
            [new(0,0),new(0.5f,0f),new(1,0)],
            [new(0.5f,0),new(0.5f,0.5f),new(0.5f,1)],
            [new(0,1),new(0.5f,1),new(1,1)]
        };
        public Letter R;
        public Letter A;
        public Letter N;
        public Letter I;
        public RaniFrequency(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 10;
            Collider = new Hitbox(data.Width, data.Height);
            Add(R = new Letter('R', 0, rPoints));
            Add(A = new Letter('A', 1, aPoints));
            Add(N = new Letter('N', 2, nPoints));
            Add(I = new Letter('I', 3, iPoints));
        }
        public override void Update()
        {
            base.Update();
            float[] rates = FrequencyData.GetRates(Scene);
            R.CurrentFrequency = rates[0];
            A.CurrentFrequency = rates[1];
            N.CurrentFrequency = rates[2];
            I.CurrentFrequency = rates[3];
        }
        public override void Render()
        {
            Draw.Rect(Collider, Color.Black);
            base.Render();
        }
    }
}