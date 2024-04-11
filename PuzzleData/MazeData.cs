using System;
using static Celeste.Mod.PuzzleIslandHelper.Entities.PuzzleEntities.LabGeneratorPuzzle.LGPOverlay;
using System.Collections.Generic;
using System.Linq;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.PuzzleData
{
    public class MazeData
    {
        public string Grid { get; set; }
        public int[,] Data { get; set; }
        public int Columns { get; set; }
        public int Rows { get; set; }
        public void ParseData()
        {
            Data = new int[Columns, Rows];
            int column = 0;
            int row = 0;

            foreach (char c in Grid.Replace(" ", ""))
            {
                if (char.IsDigit(c))
                {
                    if (row >= Rows) break;
                    if (column >= Columns)
                    {
                        row++;
                        column = 0;
                    }
                    Data[column, row] = Calc.Clamp((int)char.GetNumericValue(c), 0, 1);
                }
            }
        }
    }
}
