using Microsoft.Xna.Framework;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public class ShapeStorage
    {
        public static readonly Vector2[] NinePointSquare =
        {
            new(-1,-1),
            new(0,-1),
            new(1,-1),
            new(-1,0),
            new(0,0),
            new(1,0),
            new(-1,1),
            new(0,1),
            new(1,1)
        };
        public static readonly int[] NinePointSquareIndices =
        {
            0, 1, 3,  3, 1, 4, //topleft quad
            1, 2, 4,  4, 2, 5, //topright quad
            3, 4, 6,  6, 4, 7, //bottomleft quad
            4, 5, 7,  7, 5, 8  //bottomright quad
        };
        public static Vector3[] CubeWire = {
    new(-1, -1, -1),
    new(1, -1, -1),
    new(-1, 1, -1),
    new(1, 1, -1),
    new(-1, -1, 1),
    new(1, -1, 1),
    new(-1, 1, 1),
    new(1, 1, 1),
    new(-1, -1, -1),
    new(-1, 1, -1),
    new(1, -1, -1),
    new(1, 1, -1),
    new(-1, -1, 1),
    new(-1, 1, 1),
    new(1, -1, 1),
    new(1, 1, 1),
    new(-1, -1, -1),
    new(-1, -1, 1),
    new(-1, 1, -1),
    new(-1, 1, 1),
    new(1, -1, -1),
    new(1, -1, 1),
    new(1, 1, -1),
    new(1, 1, 1),
  };
        public static Vector3[] CubeSimple =
    {
                new(-1, -1, -1),
                new(1, -1, -1),
                new(-1, 1, -1),
                new(1, 1, -1),
                new(-1, -1, 1),
                new(1, -1, 1),
                new(-1, 1, 1),
                new(1, 1, 1),
    };
        public static Vector3[] TetrahedronWire =
        {
            new(1,1,1), new(1,-1,-1),
            new(1,1,1), new(-1,1,-1),
            new(1,1,1), new(-1,-1,1),
            new(1,-1,-1), new(-1,1,-1),
            new(1,-1,-1), new(-1,-1,1),
            new(-1,1,-1),new(-1,-1,1),
        };
        public static Vector3[] TetrahedronSimple =
        {
            new(1,1,1),
            new(1,-1,-1),
            new(-1,1,-1),
            new(-1,-1,1),

        };
        public static Vector3[] OctahedronWire =
        {
            new(1,0,0),new(-1,0,0),
            new(1,0,0),new(0,1,0),
            new(1,0,0),new(0,-1,0),
            new(1,0,0),new(0,-1,0),
            new(1,0,0),new(0,0,1),
            new(1,0,0),new(0,0,-1),

            new(-1,0,0),new(0,1,0),
            new(-1,0,0),new(0,-1,0),
            new(-1,0,0),new(0,0,1),
            new(-1,0,0),new(0,0,-1),

            new(0,1,0), new(0,-1,0),
            new(0,1,0), new(0,0,1),
            new(0,1,0), new(0,0,-1),

            new(0,-1,0),new(0,0,1),
            new(0,-1,0),new(0,0,-1),
            new(0,0,1), new(0,0,-1)

        };
        public static Vector3[] OctahedronSimple =
        {
            new(1,0,0),
            new(-1,0,0),
            new(0,1,0),
            new(0,-1,0),
            new(0,0,1),
            new(0,0,-1),

        };
    }
}
