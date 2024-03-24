using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/ShapeThing")]
    [Tracked]
    public class ShapeThing : Entity
    {
        private Level level;
        public float LookSpeed = 1;
        private int NumPoints = 6;
        private float rate = 1.5f;
        private float GlitchAmount;
        public float angleX, angleY, angleZ;

        public float angleSpeedX, angleSpeedY, angleSpeedZ;
        private List<Vector3> RandomPoints = new();
        private List<Vector3> DestinationPoints = new();
        private Vector3[] ShapePoints;
        private bool Speeding;
        private float Speed = 0.01f;
        private float Scale = 1;
        private float ScaleMult = 1;
        private readonly bool ConnectAllPoints;
        private bool rotateX, rotateY, rotateZ;
        private Color Color;
        private float LineThickness;
        private bool IsRandom
        {
            get
            {
                return PointShape == Shape.RandomStatic || PointShape == Shape.RandomMorphing;
            }
        }
        private bool RandomizeLimit;
        private bool InGlitch;
        private VirtualRenderTarget Target = VirtualContent.CreateRenderTarget("3dshape", 320, 180);
        private enum GlitchState
        {
            None,
            Gradual,
            Flashy
        }
        private struct Line3D
        {
            public Vector3 Start;
            public Vector3 End;
            public float AverageZ
            {
                get
                {
                    return (Start.Z + End.Z) / 2;
                }
            }
            public Line3D(Vector3 start, Vector3 end)
            {
                Start = start;
                End = end;
            }
        }

        private enum Shape
        {
            Cube,
            Tetrahedron,
            Octahedron,
            RandomStatic,
            RandomMorphing
        }
        private readonly Shape PointShape;
        private readonly GlitchState GlitchMethod;
        public ShapeThing(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Tag |= Tags.TransitionUpdate;
            Collider = new Hitbox(data.Width, data.Width);
            PointShape = data.Enum("shape", Shape.Cube);
            GlitchMethod = data.Enum("glitch", GlitchState.None);
            NumPoints = data.Int("randomPoints");
            ConnectAllPoints = data.Bool("connectAllPoints");
            RandomizeLimit = data.Bool("randomizePointLimit");
            rotateX = data.Bool("rotateX");
            rotateY = data.Bool("rotateY");
            rotateZ = data.Bool("rotateZ");
            angleSpeedX = data.Float("xSpeed");
            angleSpeedY = data.Float("ySpeed");
            angleSpeedZ = data.Float("zSpeed");
            Color = data.HexColor("color");
            LineThickness = data.Float("lineThickness");
            Add(new BeforeRenderHook(BeforeRender));

            if (!IsRandom)
            {
                if (ConnectAllPoints)
                {
                    ShapePoints = PointShape switch
                    {
                        Shape.Cube => ShapeStorage.CubeSimple,
                        Shape.Tetrahedron => ShapeStorage.TetrahedronSimple,
                        Shape.Octahedron => ShapeStorage.OctahedronSimple,
                        _ => null
                    };
                }
                else
                {
                    ShapePoints = PointShape switch
                    {
                        Shape.Cube => ShapeStorage.CubeWire,
                        Shape.Tetrahedron => ShapeStorage.TetrahedronWire,
                        Shape.Octahedron => ShapeStorage.OctahedronWire,
                        _ => null
                    };
                }
            }
        }
        private Vector3[] GetArrayUsed()
        {
            return IsRandom ? RandomPoints.ToArray() : ShapePoints;
        }
        private List<Line3D> GetAllConnections(Vector3[] list)
        {
            List<Line3D> result = new();
            Dictionary<Vector3, bool> dict = new();
            List<bool> Conditions;
            List<Vector3> Vectors;
            foreach (Vector3 v in list)
            {
                try
                {
                    dict.Add(v, false);
                }
                catch (Exception)
                {
                    continue;
                }
            }
            Vectors = dict.Keys.ToList();
            Conditions = dict.Values.ToList();
            for (int i = 0; i < Vectors.Count; i++)
            {
                if (!Conditions[i])
                {
                    for (int j = 0; j < Vectors.Count; j++)
                    {
                        if (!Conditions[j] && Vectors[i] != Vectors[j])
                        {
                            result.Add(new Line3D(Vectors[i], Vectors[j]));
                        }
                    }
                }
                Conditions[i] = true;
            }
            return result;
        }
        private void Drawing()
        {
            Matrix matrix = GetMatrix();
            //Matrix matrix = Matrix.CreateRotationX(angleX) * Matrix.CreateRotationY(angleY);
            var toRender = GetArrayUsed().Select(vert =>
            {
                Vector3 rotated = Vector3.Transform(vert, matrix);
                return rotated;
            }).ToArray();


            List<Line3D> Lines = new();
            if (ConnectAllPoints)
            {
                Lines = GetAllConnections(toRender.ToArray());
            }
            else
            {
                if (toRender.Length > 1)
                {
                    for (int i = 1; i < toRender.Length; i += 2)
                    {
                        Lines.Add(new Line3D(toRender[i - 1], toRender[i]));
                    }
                }
            }
            Lines = Lines.OrderBy(item => item.AverageZ).ToList();

            foreach (Line3D line in Lines)
            {
                DrawEdge(line, line.AverageZ * LineThickness, Color);
            }
        }
        private IEnumerator Glitch()
        {
            while (true)
            {
                if (GlitchMethod == GlitchState.Gradual)
                {
                    InGlitch = false;
                    yield return Calc.Random.Range(4, 10f);

                    InGlitch = true;
                    float orig = rate;
                    for (float i = 0; i < 2; i += Engine.DeltaTime)
                    {
                        rate = Calc.LerpClamp(orig, orig * 2, Ease.CubeIn(i / 2));
                        GlitchAmount = Ease.CubeIn(i / 2);
                        yield return null;
                    }
                    for (float i = 0; i < 2; i += Engine.DeltaTime)
                    {
                        rate = Calc.LerpClamp(orig * 2, orig, Ease.CubeOut(i / 2));
                        GlitchAmount = 1 - Ease.CubeIn(i / 2);
                        yield return null;
                    }
                    rate = orig;
                }
                else
                {
                    yield return Calc.Random.Range(3, 10f);
                    float orig = rate;
                    InGlitch = true;
                    GlitchAmount = 0.7f;
                    rate = -orig * 2;
                    yield return Engine.DeltaTime * Calc.Random.Range(1, 30);
                    InGlitch = false;
                    GlitchAmount = 0;
                    rate = orig;
                    yield return null;
                }
            }

        }
        private void BeforeRender()
        {
            EasyRendering.DrawToObject(Target, Drawing, level, true);
            if (InGlitch)
            {
                float random = Calc.Random.Choose(0, 0, 1, 1, 2, 5);
                EasyRendering.AddGlitch(Target, GlitchAmount, random * 200);
            }

        }
        public override void Render()
        {
            base.Render();
            Draw.SpriteBatch.Draw(Target, level.Camera.Position, Color.White);

        }

        private void DrawEdge(Line3D Line, float thickness, Color color)
        {
            DrawEdge(Line.Start, Line.End, thickness, color);
        }
        private void DrawOutline(List<Line3D> list, float thickness, Color color)
        {
            foreach (Line3D line in list)
            {
                DrawEdge(line.Start, line.End, thickness, color);
            }
        }
        public void DrawEdge(Vector3 a, Vector3 b, float thickness, Color color)
        {
            Vector2 a1 = new Vector2(a.X, a.Y);
            Vector2 b1 = new Vector2(b.X, b.Y);
            float realThick = thickness < 1 ? 1 : thickness;
            Color linecolor;
            Color orig = color;
            float z = (a.Z + b.Z) / 2;
            if (z > 0f)
            {
                linecolor = Color.Lerp(orig, Color.White, Calc.Min(z, 0.4f));
            }
            else
            {
                linecolor = Color.Lerp(orig, Color.Black, Calc.Min(Math.Abs(z), 0.5f));
            }
            Vector2 offset = Vector2.One * Width / 2;
            Draw.Line(Position + offset + a1 * (Width * ScaleMult / 2), Position + offset + b1 * (Width * ScaleMult / 2), linecolor, realThick);
        }

        private Matrix GetMatrix()
        {

            Matrix x = Matrix.CreateRotationX(angleX);
            Matrix y = Matrix.CreateRotationY(angleY);
            Matrix z = Matrix.CreateRotationZ(angleZ);
            if (rotateX)
            {
                if (rotateY)
                {
                    return rotateZ ? x * y * z : x * y;
                }
                else if (rotateZ)
                {
                    return x * z;
                }
                else
                {
                    return x;
                }
            }
            else if (rotateY)
            {
                if (rotateZ)
                {
                    return y * z;
                }
                else
                {
                    return y;
                }
            }
            else
            {
                return z;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = scene as Level;
            if (IsRandom)
            {
                Random r = new Random((int)level.Session.Time);
                for (int i = 0; i < NumPoints; i++)
                {
                    float x = r.Range(-1f, 1);
                    float y = r.Range(-1f, 1);
                    float z = r.Range(-1f, 1);
                    RandomPoints.Add(new Vector3(x, y, z));
                }
            }
            var curve = new BezierCurve(0.83f, 0.11f, 1, 0.63f);
            Tween tw = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.ElasticIn, 1, false);
            Add(tw);
            tw.OnUpdate = (t) =>
            {
                Speed = Calc.LerpClamp(0.05f, 0.1f, t.Eased);

            };
            tw.Start();
            if (PointShape == Shape.RandomMorphing)
            {
                Add(new Coroutine(LerpPoints()));
                Add(new Coroutine(RandomScale()));
            }
            if (GlitchMethod != GlitchState.None)
            {
                Add(new Coroutine(Glitch()));
            }
        }
        private IEnumerator RandomScale()
        {
            while (true)
            {
                yield return Calc.Random.Range(1f, 3f);
                float target = Calc.Random.Range(-1, 1f);
                float orig = Scale;
                float speed = Calc.Random.Range(3, 9);
                for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
                {
                    Scale = Calc.LerpClamp(orig, target, Ease.SineIn(i));
                    yield return null;
                }
                yield return null;
            }
        }
        private IEnumerator LerpPoints()
        {
            while (true)
            {
                yield return Calc.Random.Range(1, 4);
                Speeding = false;
                if (RandomizeLimit)
                {
                    NumPoints = Calc.Random.Range(1, 9);
                    if (RandomPoints.Count < NumPoints)
                    {
                        while (RandomPoints.Count != NumPoints)
                        {
                            RandomPoints.Add(Vector3.Zero);
                        }
                    }
                }
                yield return null;
                DestinationPoints.Clear();
                for (int i = 0; i < NumPoints; i++)
                {
                    float x = Calc.Random.Range(-1f, 1);
                    float y = Calc.Random.Range(-1f, 1);
                    float z = Calc.Random.Range(-1f, 1);
                    DestinationPoints.Add(new Vector3(x, y, z));
                }
                Speeding = true;
                for (float i = 0; i < 1; i += Engine.DeltaTime * 5)
                {
                    ScaleMult = Calc.LerpClamp(1, 0.1f, Ease.QuintIn(i));
                    yield return null;
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime * 5)
                {
                    ScaleMult = Calc.LerpClamp(0.1f, 1, Ease.QuintOut(i));
                    yield return null;
                }
            }

        }
        public override void Update()
        {
            base.Update();
            if (rotateX)
            {
                angleSpeedX += rate;
                angleSpeedX %= 360;
                angleX = (30 + angleSpeedX).ToRad();
            }
            if (rotateY)
            {
                angleSpeedY += rate;
                angleSpeedY %= 360;
                angleY = (30 + angleSpeedY).ToRad();
            }
            if (rotateZ)
            {
                angleSpeedZ += rate;
                angleSpeedZ %= 360;
                angleZ = (30 + angleSpeedZ).ToRad();
            }


            if (Speeding)
            {
                ApproachPoints(Speed);
            }


        }
        private void LerpPoints(float Lerp)
        {
            for (int i = 0; i < NumPoints; i++)
            {
                float x = Calc.LerpClamp(RandomPoints[i].X, DestinationPoints[i].X, Lerp);
                float y = Calc.LerpClamp(RandomPoints[i].Y, DestinationPoints[i].Y, Lerp);
                float z = Calc.LerpClamp(RandomPoints[i].Z, DestinationPoints[i].Z, Lerp);
                RandomPoints[i] = new Vector3(x, y, z);
            }
        }
        private void ApproachPoints(float Speed)
        {
            for (int i = 0; i < NumPoints; i++)
            {
                if (Speeding)
                {
                    float x = Calc.Approach(RandomPoints[i].X, DestinationPoints[i].X, Speed);
                    float y = Calc.Approach(RandomPoints[i].Y, DestinationPoints[i].Y, Speed);
                    float z = Calc.Approach(RandomPoints[i].Z, DestinationPoints[i].Z, Speed);
                    RandomPoints[i] = new Vector3(x, y, z);
                }
            }
        }
    }
}


