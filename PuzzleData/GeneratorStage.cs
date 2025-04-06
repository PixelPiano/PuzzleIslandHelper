using System;
using static Celeste.Mod.PuzzleIslandHelper.Entities.LabGeneratorPuzzle.PuzzleOverlay;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.PuzzleData
{
    public class GeneratorStage
    {
        public void ParseData()
        {
            PieceData = new string[Columns, Rows];
            ColData = new int[Columns, Rows];
            RowData = new int[Columns, Rows];
            RotateData = new int[Columns, Rows];
            TypeData = new Node.Type[Columns, Rows];

            Side = Direction.ToLower() switch
            {
                "up" => Side.Up,
                "down" => Side.Down,
                "left" => Side.Left,
                "right" => Side.Right,
                _ => Side.Up
            };
            for (int i = 0; i < Rows; i++)
            {
                string[] nodes = Nodes[i].Split(',');
                for (int k = 0; k < nodes.Length; k++)
                {
                    string node = nodes[k].Replace(" ", "");
                    string piece = "r";
                    int rotations = -1;
                    bool foundNumber = false;
                    bool foundLetter = false;
                    Node.Type foundType = Node.Type.Default;
                    foreach (char c in node)
                    {
                        if (c == '(')
                        {
                            string sub = node.Substring(node.IndexOf('('));
                            foreach (Node.Type @enum in Enum.GetValues(typeof(Node.Type)).Cast<Node.Type>())
                            {
                                if (@enum != Node.Type.Default)
                                {
                                    string enumName = @enum.ToString();
                                    if (sub.Contains(enumName.ToLower()))
                                    {
                                        foundType = @enum;
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                        else if (char.IsNumber(c))
                        {
                            foundNumber = true;
                            rotations = c - '0';
                        }
                        else if (char.IsLetter(c))
                        {
                            foundLetter = true;
                            piece = c.ToString();
                        }

                    }
                    PieceData[k, i] = foundLetter ? piece : "r";
                    ColData[k, i] = k;
                    RowData[k, i] = i;
                    RotateData[k, i] = foundNumber ? rotations : -1;
                    TypeData[k, i] = foundType;
                }
            }
        }
        public Side Side { get; set; }
        public string Name { get; set; }
        public List<string> Nodes { get; set; }
        public Node.Type[,] TypeData { get; set; }
        public string[,] PieceData { get; set; }
        public int[,] ColData { get; set; }
        public int[,] RowData { get; set; }
        public int[,] RotateData { get; set; }
        public int GoalX { get; set; }
        public int GoalY { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public string Direction { get; set; }
    }
 

}
