using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public struct NumRange
    {
        public float Min;
        public float Max;
        public static NumRange Scalar = new NumRange(0, 1);
        public static NumRange Sine = new NumRange(-1, 1);
        public NumRange(float min, float max)
        {
            Min = min;
            Max = max;
        }
        public float Lerp(float percent)
        {
            return Calc.LerpClamp(Min, Max, percent);
        }
        public float Lerp(float percent, Ease.Easer ease)
        {
            ease ??= Ease.Linear;
            return Calc.LerpClamp(Min, Max, ease(percent));
        }
        public float Random()
        {
            return Min == Max ? Min : Calc.Random.Range(Min, Max);
        }
    }
}
