using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Structs
{
    public struct NumRange2
    {
        public NumRange X;
        public NumRange Y;
        public static NumRange2 Scalar = new NumRange2(NumRange.Scalar, NumRange.Scalar);
        public static NumRange2 ScalarX = new NumRange2(NumRange.Scalar, default);
        public static NumRange2 ScalarY = new NumRange2(default, NumRange.Scalar);
        public NumRange2(NumRange x, NumRange y) : this(x.Min, x.Max, y.Min, y.Max) { }
        public NumRange2(float minX, float maxX, float minY, float maxY)
        {
            X = new NumRange(minX, maxX);
            Y = new NumRange(minY, maxY);
        }
        public NumRange2(float min, float max)
        {
            X = new NumRange(min, max);
            Y = new NumRange(min, max);
        }
        public Vector2 Lerp(float percent)
        {
            return new Vector2(X.Lerp(percent), Y.Lerp(percent));
        }
        public Vector2 Lerp(float percent, Ease.Easer ease)
        {
            return new Vector2(X.Lerp(percent, ease), Y.Lerp(percent, ease));
        }
        public Vector2 Lerp(float percent, Ease.Easer easeX, Ease.Easer easeY)
        {
            return new Vector2(X.Lerp(percent, easeX), Y.Lerp(percent, easeY));
        }
        public Vector2 Random()
        {
            return new Vector2(X.Random(), Y.Random());
        }
    }
}
