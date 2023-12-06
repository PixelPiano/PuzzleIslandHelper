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
    [CustomEntity("PuzzleIslandHelper/Life")]
    [Tracked]
    public class Life : Entity
    {
        public LifePixel[,] Pixels;
        public bool Simulating;
        public bool Drawing;
        public Grid Grid;
        public const int CellSize = 8;
        public Life(Vector2 position, int width, int height) : base(position)
        {
            Depth = 1;
            Collider = new Hitbox(width, height);
            Pixels = new LifePixel[(int)Width, (int)Height];
            Grid = new Grid(CellSize, CellSize, new bool[(int)Width, (int)Height]);

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Pixels[i, j] = new LifePixel(new Point(i, j));
                }
            }
            //Randomize();
            Randomize();
        }
        public void Blinker()
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Pixels[i, j].Alive = false;
                }
            }
            for (int i = 3; i < 7; i++)
            {
                Pixels[i, 5].Alive = true;
            }
        }

        public override void Update()
        {
            base.Update();
            if (!BetterWindow.Drawing)
            {
                return;
            }
            Grid.Position = Position.ToInt();
            if (!Simulating)
            {
                if (Scene is Level level)
                {
                    Cursor cursor = level.Tracker.GetEntity<Cursor>();
                    if (cursor is not null)
                    {
                        if (Grid.Collide(cursor.Helper) && Cursor.LeftClicked)
                        {
                            int x, y;
                            Vector2 cPos = cursor.WorldPosition;
                            x = (int)(cPos.X - Grid.AbsolutePosition.X) / CellSize;
                            y = (int)(cPos.Y - Grid.AbsolutePosition.Y) / CellSize;
                            if (x < Width && x >= 0 && y < Height && y >= 0)
                            {
                                Pixels[x, y].Alive = true;
                                //AddCell(x, y);
                            }
                        }
                    }
                }
                return;
            }

            if (Scene.OnInterval(5 / 60f))
            {
                LifePixel[,] pixels = new LifePixel[(int)Width, (int)Height];
                for (int i = 0; i < Width; i++)
                {
                    for (int j = 0; j < Height; j++)
                    {
                        LifePixel p = Pixels[i, j];
                        LifePixel[] check = { new(p.X - 1, p.Y), new(p.X + 1, p.Y), new(p.X, p.Y + 1), new(p.X, p.Y - 1), new(p.X + 1, p.Y + 1), new(p.X - 1, p.Y + 1), new(p.X + 1, p.Y - 1), new(p.X - 1, p.Y - 1) };
                        p.AliveNeighbors = 0;
                        for (int k = 0; k < check.Length; k++)
                        {

                            if (check[k].X >= 0 && check[k].Y >= 0 &&
                                check[k].X < Width && check[k].Y < Height &&
                                Pixels[check[k].X, check[k].Y].Alive)
                            {
                                p.AliveNeighbors++;
                            }
                        }
                        if (!p.Alive && p.AliveNeighbors == 3)
                        {
                            p.Alive = true;
                        }
                        else if (p.Alive && p.AliveNeighbors != 2 && p.AliveNeighbors != 3)
                        {
                            p.Alive = false;
                        }

                        pixels[i, j] = p;
                    }
                }
                Pixels = (LifePixel[,])pixels.Clone();
            }
        }
        public Life(EntityData data, Vector2 offset) : this(data.Position + offset, 20, 20) { }
        public override void Render()
        {
            base.Render();
            if (!BetterWindow.Drawing)
            {
                return;
            }
            MTexture texture = GFX.Game["objects/PuzzleIslandHelper/gameOfLife/cell"];
            foreach (LifePixel pixel in Pixels)
            {
                if (pixel.Alive)
                {
                    Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, Position + (pixel.Position * new Vector2(CellSize)), Color.White);
                }
            }
        }
        public void AddCell(int x, int y)
        {
            Pixels[x, y].Alive = true;
        }
        public void Randomize()
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Pixels[i, j].Alive = Calc.Random.Chance(0.4f);
                }
            }
        }
    }
}
