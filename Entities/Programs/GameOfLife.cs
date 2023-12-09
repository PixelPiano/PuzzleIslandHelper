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
        public const int W = 22;
        public const int H = 11;
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
                    currentCells[i, j] = false;
                }
            }
            Stop();
        }
        public void Store()
        {
            PianoModule.SaveData.AddLifeGrid(currentCells);
        }
        public void LoadPreset()
        {
            bool wasSimulating = Simulating;
            Simulating = false;
            bool[,] temp = PianoModule.SaveData.GetLifeGrid();
            if (temp != null)
            {
                currentCells = (bool[,])temp.Clone();
            }
            Simulating = wasSimulating;
        }
        public void Stop()
        {
            Simulating = false;
        }
        public bool Simulating;
        public const int CellSize = 9;
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
                if (Collider.Bounds.Contains((int)cPos.X, (int)cPos.Y) && !Window.PressingButton)
                {
                    if (!Cursor.LeftClicked && !Cursor.RightClicked)
                    {
                        return;
                    }
                    int x, y;
                    x = ((int)cPos.X - (int)BetterWindow.DrawPosition.X) / CellSize;
                    y = ((int)cPos.Y - (int)BetterWindow.DrawPosition.Y) / CellSize;
                    if (x >= 0 && y >= 0 && x < W && y < H)
                    {
                        currentCells[x, y] = Cursor.LeftClicked && !Cursor.RightClicked;
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
            Vector2 offset = new Vector2(2,2);
            for (int i = 0; i < W + 1; i++) //Vertical lines (UD)
            {
                Vector2 start = Position + new Vector2(i * CellSize, 0);
                Vector2 end = new Vector2(start.X, start.Y + H * CellSize -1);
                Draw.Line(start + offset, end + offset, Color.Black);
            }
            for (int i = 0; i < H + 1; i++) //Horizontal lines (LR)
            {
                Vector2 start = Position + new Vector2(-1, -1 + i * CellSize);
                Vector2 end = new Vector2(start.X + W * CellSize + 1, start.Y); ;
                Draw.Line(start + offset, end + offset, Color.Black);
            }
            for (int i = 0; i < W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    if (currentCells[i, j])
                    {
                        Draw.SpriteBatch.Draw(texture.Texture.Texture_Safe, Position + offset + (new Vector2(i, j) * new Vector2(CellSize)), Color.White);
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
