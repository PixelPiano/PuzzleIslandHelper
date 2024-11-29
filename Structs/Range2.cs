using Microsoft.Xna.Framework;

namespace Celeste.Mod.PuzzleIslandHelper.Structs
{
    public struct Range2
    {
        public Range X;
        public Range Y;
        public static Range2 Scalar = new Range2(Range.Scalar, Range.Scalar);
        public static Range2 ScalarX = new Range2(Range.Scalar, default);
        public static Range2 ScalarY = new Range2(default, Range.Scalar);
        public Range2(Range x, Range y) : this(x.Min, x.Max, y.Min, y.Max) { }
        public Range2(float minX, float maxX, float minY, float maxY)
        {
            X = new Range(minX, maxX);
            Y = new Range(minY, maxY);
        }
        public Range2(float min, float max)
        {
            X = new Range(min, max);
            Y = new Range(min, max);
        }
        public Vector2 Random()
        {
            return new Vector2(X.Random(), Y.Random());
        }
    }
}
