using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using VivHelper;
using VivHelper.Entities;
using VivHelper.Polygon;

namespace Celeste.Mod.PuzzleIslandHelper
{
    public class ShapeStorage
    {
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
