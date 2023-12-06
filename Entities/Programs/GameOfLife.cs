using Celeste.Mod.CommunalHelper;
using Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.PianoEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Programs
{
    [Tracked]
    public class LifeGrid : Component
    {
        public bool[,] Data;
        private bool[,] lastData;
        public int W
        {
            get
            {
                return Data.GetLength(0);
            }
        }
        public int H
        {
            get
            {
                return Data.Rank;
            }
        }
        public Vector2 Position = Vector2.Zero;
        public MTexture Texture = GFX.Game["objects/PuzzleIslandHelper/gameOfLife/cell"];
        public const int CellSize = 8;
        public Vector2 RenderPosition
        {
            get
            {
                return Entity.Position + Position;
            }
        }
        public bool this[int x, int y]
        {
            get
            {
                if (x >= 0 && y >= 0 && x < W && y < H)
                {
                    return Data[x, y];
                }

                return false;
            }
            set
            {
                Data[x, y] = value;
            }
        }
        public void Randomize()
        {
            for (int i = 0; i < W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    Data[i, j] = Calc.Random.Chance(0.2f);
                }
            }
        }
        public override void Update()
        {
            for (int i = 0; i < W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    lastData[i, j] = Data[i, j];
                }
            }
            base.Update();
            if (!BetterWindow.Drawing)
            {
                return;
            }

            if (Scene.OnInterval(5 / 60f))
            {
                for (int i = 0; i < W; i++)
                {
                    for (int j = 0; j < H; j++)
                    {
                        Vector2[] check = { new(i - 1, j), new(i + 1, j), new(i, j + 1), new(i, j - 1), new(i + 1, j + 1), new(i - 1, j + 1), new(i + 1, j - 1), new(i - 1, j - 1) };
                        int neighbors = 0;
                        for (int k = 0; k < check.Length; k++)
                        {
                            if (this[(int)check[k].X, (int)check[k].Y])
                            {
                                neighbors++;
                            }
                        }
                        if (!lastData[i, j] && neighbors == 3)
                        {
                            Data[i, j] = true;
                        }
                        else if (lastData[i, j] && neighbors != 2 && neighbors != 3)
                        {
                            Data[i, j] = false;
                        }
                    }
                }
            }
        }
        public LifeGrid(int width, int height) : base(true, true)
        {
            Data = new bool[width, height];
            lastData = new bool[width, height];
        }
        public override void Render()
        {
            base.Render();
            if (!BetterWindow.Drawing)
            {
                return;
            }
            for (int i = 0; i < W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, RenderPosition + (new Vector2(i, j) * new Vector2(CellSize)), Color.White);

                }
            }
        }
    }
    [Tracked]
    public class GameOfLife : WindowContent
    {
        public int W
        {
            get
            {
                //return (int)Width / 8;
                return 30;
            }
        }
        public int H
        {
            get
            {
                //return (int)Height / 8;
                return 30;
            }
        }
        public LifeGrid Grid;
        public GameOfLife(BetterWindow window) : base(window)
        {
            Name = "life";
            Add(Grid = new LifeGrid(30, 30));
            Grid.Randomize();
            /*            Pixels = new LifePixel[W, H];

                        for (int i = 0; i < W; i++)
                        {
                            for (int j = 0; j < H; j++)
                            {
                                Pixels[i, j] = new LifePixel(new Point(i, j));
                            }
                        }*/
            //Randomize();
        }

        public bool Simulating;
        public const int CellSize = 8;
        public override void Update()
        {
            Position = BetterWindow.DrawPosition.ToInt();
            base.Update();
            if (!BetterWindow.Drawing)
            {
                return;
            }
            if (!Simulating)
            {
                /*                Vector2 cPos = Interface.Position;
                                Grid grid;
                                if (Collider.Bounds.Contains((int)cPos.X, (int)cPos.Y) && Cursor.LeftClicked)
                                {
                                    int x, y;
                                    x = (int)(cPos.X - Collider.Position.X) / CellSize;
                                    y = (int)(cPos.Y - Collider.Position.Y) / CellSize;
                                    if (x < W && x >= 0 && y < H && y >= 0)
                                    {
                                        Pixels[x, y].Alive = true;
                                    }
                                }
                                return;*/
                return;
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Rect(Collider, Color.Blue);
        }
        public void AddCell(int x, int y)
        {
           // Pixels[x, y].Alive = true;
        }

    }
}
