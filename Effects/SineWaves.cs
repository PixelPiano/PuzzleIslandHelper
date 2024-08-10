using Celeste.Mod.Backdrops;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/SineWaves")]
    public class SineWaves : Backdrop
    {
        public struct SineLine
        {
            public float Length;
            public float CurveOffset;
            public float Offset;
            public bool Vertical;
            public float Drift;
            private float driftTime;
            private Color color;
            private float maxLength;
            public SineLine(float length, float curveOffset, bool vertical, float maxLength)
            {
                this.maxLength = maxLength;
                Offset = Calc.Random.Range(Math.Abs(curveOffset), (Vertical ? 320f : 180f) - Math.Abs(curveOffset));
                Length = length;
                CurveOffset = curveOffset;
                Vertical = vertical;
                driftTime = Calc.LerpClamp(1, 4f, Length / maxLength);
                color = Color.Lerp(Color.Black, Color.White, Length / maxLength);
            }
            public void Render()
            {
                Color outline = Color.Black;
                Color sineColor = color;
                Drift += Engine.DeltaTime / driftTime;
                Drift %= Length;
                if (Vertical)
                {
                    Vector2 curve = Vector2.UnitX * CurveOffset;

                    Vector2 d = new Vector2(0, Drift);
                    Vector2 from = new Vector2(Offset, -Length) + d;
                    for (float i = 0; i < 180 + Length; i += Length)
                    {
                        Vector2 to = new Vector2(Offset, i) + d;

                        DrawCurve(from, (from + to) / 2, curve, outline, 2);
                        DrawCurve((from + to) / 2, to, -curve, outline, 2);
                        DrawCurve(from, (from + to) / 2, curve, sineColor, 1);
                        DrawCurve((from + to) / 2, to, -curve, sineColor, 1);
                        from = to;
                    }
                }
                else
                {
                    Vector2 curve = Vector2.UnitY * CurveOffset;

                    Vector2 d = new Vector2(Drift, Drift);
                    Vector2 from = new Vector2(-Length, Offset) + d;
                    for (float i = 0; i < 320 + Length; i += Length)
                    {
                        Vector2 to = new Vector2(i, Offset) + d;
                        DrawCurve(from, (from + to) / 2, curve, outline, 2);
                        DrawCurve((from + to) / 2, to, -curve, outline, 2);
                        DrawCurve(from, (from + to) / 2, curve, sineColor, 1);
                        DrawCurve((from + to) / 2, to, -curve, sineColor, 1);
                        from = to;
                    }
                }
            }
            public void DrawCurve(Vector2 from, Vector2 to, Vector2 control, Color color, int thickness)
            {
                from = from.Round();
                to = to.Round();
                SimpleCurve curve = new SimpleCurve(from, to, (from + to) / 2f + control);
                Vector2 vector = curve.Begin;
                int steps = (int)Vector2.Distance(from, to);
                for (int j = 1; j <= steps; j++)
                {
                    float percent = (float)j / steps;
                    Vector2 point = curve.GetPoint(percent).Round();
                    Draw.Line(vector, point, color, thickness);
                    vector = point + (vector - point).SafeNormalize();
                }
            }
        }
        public float VerticalChance;
        public float MinOffset;
        public float MaxOffset;
        public float MinLength;
        public float MaxLength;
        private int MaxLines;
        public List<SineLine> Lines = new();
        public SineWaves(BinaryPacker.Element data) : base()
        {
            VerticalChance = data.AttrFloat("verticalChance");
            MinOffset = data.AttrFloat("minCurve");
            MaxOffset = data.AttrFloat("maxCurve");
            MinLength = data.AttrFloat("minWaveLength");
            MaxLength = data.AttrFloat("maxWaveLength");
            MaxLines = data.AttrInt("maxLines");
            for (int i = 0; i < MaxLines; i++)
            {
                float length = Calc.Random.Range(MinLength, MaxLength);
                float offset = Calc.Random.Range(MinOffset, MaxOffset);
                Lines.Add(new SineLine(length, offset, Calc.Random.Chance(VerticalChance), MaxLength));
            }
        }

        public override void Update(Scene scene)
        {
            base.Update(scene);

        }
        public override void Render(Scene scene)
        {
            base.Render(scene);
            foreach (SineLine line in Lines)
            {
                line.Render();
            }
        }
    }
}

