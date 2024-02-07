using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.PianoEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Programs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System.Linq.Expressions;

// PuzzleIslandHelper.LabDoor
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    public class LifePixel
    {
        public Point Point;
        public int X => Point.X;
        public int Y => Point.Y;
        public bool Alive;
        public Vector2 Position;
        public int AliveNeighbors;
        public bool LastState;
        private int size = 8;
        public void Reset()
        {
            AliveNeighbors = 0;
            LastState = false;
            Alive = false;
        }
        public void SetState(bool alive)
        {
            LastState = Alive;
            Alive = alive;
        }
        public LifePixel(Point point)
        {
            Point = point;
            Position = new Vector2(point.X, point.Y);
        }
        public LifePixel(int x, int y) : this(new Point(x, y)) { }

    }
    //[CustomEntity("PuzzleIslandHelper/Life")]
    //[Tracked]
    public class Life
    {

    }
}
