using System;
using Celeste.Mod.CommunalHelper;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [TrackedAs(typeof(FancyLine))]
    public class FancyLineAngle : FancyLine
    {
        public float Angle;
        public float Length;
        public Vector2 EndOffset;
        public FancyLineAngle(Vector2 start, float length, float angle, Color color, float thickness = 1) : base(start, Calc.AngleToVector(angle, length) + start, color, thickness)
        {
            Angle = angle;
            Length = length;
        }
        public override void Update()
        {
            base.Update();
            EndOffset = RenderStart + Calc.AngleToVector(Angle, Length) + Offset;
        }
        public FancyLineAngle(Vector2 start, float length, float angle, Color color, float thickness, Color color2, ColorModes colorMode, float interval) : base(start, Calc.AngleToVector(angle, length) + start, color, thickness, color2, colorMode, interval)
        {
            Angle = angle;
            Length = length;
        }
        public void Render(Vector2 offset)
        {
            start += offset;
            end += offset;
            Render();
            start -= offset;
            end -= offset;
        }
        public override void Render()
        {
            end = start + Calc.AngleToVector(Angle, Length);
            base.Render();
        }
    }
}
