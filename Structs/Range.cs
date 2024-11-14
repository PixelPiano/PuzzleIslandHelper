using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Structs
{
    public struct Range
    {
        public float Min;
        public float Max;
        public Range(float min, float max)
        {
            Min = min;
            Max = max;
        }
        public float Random()
        {
            return Calc.Random.Range(Min, Max);
        }
    }
}
