using Celeste.Mod.CommunalHelper;
using Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.PianoEntities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Programs
{
    [Tracked]
    public class GameOfLife : WindowContent
    {
        public int W
        {
            get
            {
                return 200 / CellSize;
            }
        }
        public int H
        {
            get
            {
                return 120 / CellSize;
            }
        }
        public GameOfLife(BetterWindow window) : base(window)
        {
            Name = "life";
            currentCells = new bool[W, H];
            newCells = new bool[W, H];
        }
        private bool[,] currentCells, newCells;
        public void Clear()
        {
            for (int i = 0; i < W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    currentCells[i,j] = false;
                }
            }
            Stop();
        }
        public void Stop()
        {
            Simulating = false;
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
                Vector2 cPos = Interface.cursor.WorldPosition;
                if (Cursor.LeftClicked && Collider.Bounds.Contains((int)cPos.X, (int)cPos.Y) && !Window.PressingButton)
                {
                    int x, y;
                    x = ((int)cPos.X - (int)BetterWindow.DrawPosition.X) / CellSize;
                    y = ((int)cPos.Y - (int)BetterWindow.DrawPosition.Y) / CellSize;
                    if (x >= 0 && y >= 0 && x < W && Y > H)
                    {
                        currentCells[x, y] = true;
                    }
                }
                return;
            }

            if (Scene.OnInterval(5 / 60f))
            {
                // game logic, run every frame
                for (int x = 0; x < W; x++)
                {
                    for (int y = 0; y < H; y++)
                    {
                        int aliveNeighbours = 0;

                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                if (dx == 0 && dy == 0) continue;
                                int nx = x + dx, ny = y + dy;

                                if (nx >= 0 && nx < W && ny >= 0 && ny < H && currentCells[nx, ny]) aliveNeighbours++;
                            }
                        }

                        newCells[x, y] = (currentCells[x, y] && aliveNeighbours == 2) || aliveNeighbours == 3;
                    }
                }
                (newCells, currentCells) = (currentCells, newCells);
            }
        }


        public override void Render()
        {
            base.Render();
            if (!BetterWindow.Drawing)
            {
                return;
            }
            MTexture texture = GFX.Game["objects/PuzzleIslandHelper/gameOfLife/cell"];
            for (int i = 0; i < W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    if (currentCells[i, j])
                    {
                        Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, Position + (new Vector2(i, j) * new Vector2(CellSize)), Color.White);
                    }
                }
            }
        }
        public void Randomize()
        {
            for (int i = 0; i < W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    currentCells[i, j] = Calc.Random.Chance(0.4f);
                }
            }
        }
    }
}
