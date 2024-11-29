using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Structs
{
    public struct Range
    {
        public float Min;
        public float Max;
        public static Range Scalar = new Range(0, 1);
        public Range(float min, float max)
        {
            Min = min;
            Max = max;
        }
        
        public float Random()
        {
            if (Min == Max)
            {
                return Min;
            }
            return Calc.Random.Range(Min, Max);
        }
    }
}
