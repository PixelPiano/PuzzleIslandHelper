using Microsoft.Xna.Framework;
using Monocle;
using System;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public struct LineNode
    {
        public int Length;
        public int X, Y;
        public char Side;
        public string Corner;
        public int ExtendMult;
        public Vector2 TopLeft;
        public LineNode(int cellX, int cellY, string corner, char side, int length, bool extendByStroke = false)
        {
            X = cellX * 32; Y = cellY * 32; Length = length * 32; Corner = corner; Side = side;
            ExtendMult = extendByStroke ? 1 : 0;
            TopLeft = new Vector2(X, Y);
        }
        public void DrawLine(int stroke, Color color, float offsetFromStart = 0, float offsetFromEnd = 0)
        {
            offsetFromStart = 8;
            offsetFromEnd = 16;
            Vector2 s = default;
            Vector2 size = Vector2.One * 32;
            float extend = ExtendMult * stroke;
            float length = Length - offsetFromStart - offsetFromEnd + extend;
            bool hor = false;
            switch (Corner)
            {
                case "TL":
                    s = TopLeft;
                    switch (Side)
                    {
                        case 'T':
                            s.X += offsetFromStart;
                            Draw.Rect(s, length, stroke, color);
                            break;
                        case 'L':
                            s.Y += offsetFromStart;
                            Draw.Rect(s, stroke, length, color);
                            break;
                    }
                    break;
                case "TR":
                    switch (Side)
                    {
                        case 'T':
                            s = TopLeft;
                            s.X -= length + offsetFromStart;
                            hor = true;
                            Draw.Rect(s, length, stroke, color);
                            break;
                        case 'R':
                            s = TopLeft + size.XComp();
                            s.X -= stroke;
                            s.Y += offsetFromStart;
                            Draw.Rect(s, stroke, length, color);
                            break;
                    }
                    break;
                case "BL":
                    s = TopLeft + size.YComp();
                    switch (Side)
                    {
                        case 'B':
                            s.Y -= stroke;
                            s.X += offsetFromStart;
                            Draw.Rect(s, length, stroke, color);
                            break;
                        case 'L':
                            s.Y -= length + offsetFromStart;
                            Draw.Rect(s, stroke, length, color);
                            break;
                    }
                    break;
                case "BR":
                    s = TopLeft + size;
                    switch (Side)
                    {
                        case 'B':
                            s.Y -= stroke;
                            s.X -= length + offsetFromStart;
                            Draw.Rect(s, length, stroke, color);
                            break;
                        case 'R':
                            s.X -= stroke;
                            s.Y -= length + offsetFromStart;
                            Draw.Rect(s, stroke, length, color);
                            break;
                    }
                    break;
            }
        }
    }
    public class LineNodeCollection
    {
        public static LineNodeCollection[] BetaCollections()
        {
            LineNode[] stripNodes =
            [
                new LineNode(0,0, "TL",'T',1),
                new LineNode(0,0,   "TR",'R',3),
                new LineNode(1, 5,  "TR",'R',1),
                new LineNode(0,3,   "TR",'R',3),
                new LineNode(0, 5,"BR",'B',1),
                new LineNode(0,5, "BL",'L',3),
                new LineNode(1, 5,"BL",'L',1),
                new LineNode(0,2,   "BL",'L',3)
            ];

            LineNode[] smallStripNodes =
            [
                new LineNode(1,2,"TL",'T',1),
                new LineNode(1, 2, "TR",'R',3),
                new LineNode(1, 4, "BR",'B',1),
                new LineNode(1,4,"BL",'L',3)
            ];
            LineNode[] tBlockANodes =
            [
                new LineNode(2,0,"TL",'T',1),
                new LineNode(2,0,"TR",'R',1,true), //extend by stroke
                new LineNode(3,1,"TL",'T',1),
                new LineNode(3,1,"TR",'R',1),
                new LineNode(3,1,"BR",'B',1, true), //extend by stroke
                new LineNode(2,2,"TR",'R',1),
                new LineNode(2,2,"BR",'B',1),
                new LineNode(2,2,"BL",'L',3),
            ];
            LineNode[] tBlockBNodes =
            [
                new LineNode(3,3,"TR",'T',1),
                new LineNode(3,3,"TL",'L',1, true), //extend by stroke
                new LineNode(2,4,"TR",'T',1),
                new LineNode(2,4,"TL",'L',1),
                new LineNode(2,4,"BL",'B',1, true), //extend by stroke
                new LineNode(3,5,"TL",'L',1),
                new LineNode(3,5,"BL",'B',1),
                new LineNode(3,5,"BR",'R',3),
                new LineNode(3,3,"TR",'T',2)
            ];
            return
            [
            new(stripNodes),
            new(smallStripNodes),
            new(tBlockANodes),
            new(tBlockBNodes)
            ];
            LineNode[] test =
                [
                new LineNode(0,0,"TL",'T',3),
                new LineNode(2,0,"TR",'R',3),
                new LineNode(2,2,"BR",'B',1),
                new LineNode(2,2,"BL",'L',2),
                new LineNode(1,1,"TR",'T',2),
                new LineNode(0,1,"TL",'L',3),
                new LineNode(0,3,"BL",'B',4),
                new LineNode(3,3,"BR",'R',2)
                ];
        }
        public LineNode[] Nodes;

        public LineNodeCollection(params LineNode[] nodes)
        {
            Nodes = nodes;
        }
        public int GetTotalLength(int stroke)
        {
            int total = 0;
            foreach (LineNode node in Nodes)
            {
                total += node.Length + node.ExtendMult * stroke;
            }
            return total;
        }

        public void DrawLines(float start, float end, int stroke, Color color, string logName = "")
        {
            float totalLength = GetTotalLength(stroke);
            float startLength = totalLength * start;
            float endLength = totalLength - (totalLength * end);
            float currentLength = 0;
            for (int i = 0; i < Nodes.Length; i++)
            {
                LineNode node = Nodes[i];
                float nextLength = node.Length + node.ExtendMult * stroke;
                if (nextLength >= startLength)
                {
                    if (endLength < currentLength) break;
                    node.DrawLine(stroke, i == Nodes.Length - 1 ? Color.Red : color, startLength, 0);
                }
                startLength = Math.Max(startLength - nextLength, 0);
                currentLength += nextLength;
            }
        }
    }
}

