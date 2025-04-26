using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class SnakeLine : GraphicsComponent
    {
        public class Line
        {
            public Vector2 A
            {
                get { return a; }
                set
                {
                    a = value;
                    Length = (B - A).Length();
                }
            }
            public Vector2 B
            {
                get { return b; }
                set
                {
                    b = value;
                    Length = (B - A).Length();
                }
            }
            private Vector2 a, b;
            public Vector2 LineStart;
            public Vector2 LineEnd;
            public Color Color;
            public bool Visible;
            public float Length;
            public Line(Vector2 start, Vector2 end, Color color)
            {
                a = LineStart = LineEnd = start;
                B = end;
                Color = color;
            }

            public void Render()
            {
                if (LineEnd == A) return;
                Draw.Line(LineStart, LineEnd, Color, 2);
            }
        }
        public List<Line> Lines = [];
        public List<Vector2> Nodes = [];
        public bool AdjustingNode;
        public float LineLength;
        public float StartFade;
        public float EndFade;
        public Color ColorA;
        public Color? ColorB;
        private int closestIndex;
        public int Size = 1;
        public float LineStart
        {
            get
            {
                return linestart + speedOffset;
            }
            set
            {
                linestart = value - speedOffset;
            }
        }
        private float linestart;
        private float distanceFromClosestToStart;
        public bool WrapAround;
        public float Speed = 1;
        private float speedOffset;
        public float Alpha = 1;
        public bool Debug;
        public SnakeLine(Vector2 offset, Vector2[] nodes, float start, float length, float startFade, float endFade, float speed, Color color, Color? secondaryColor = null) : base(true)
        {
            Speed = speed;
            Position = offset;
            LineStart = start;
            ColorA = color;
            ColorB = secondaryColor;
            LineLength = length;
            StartFade = startFade;
            EndFade = endFade;
            Nodes = [.. nodes];
            Vector2 prev = Nodes[0];
            for (int i = 1; i < Nodes.Count; i++)
            {
                Vector2 current = Nodes[i];
                Lines.Add(new Line(prev, current, Color.White));
                prev = current;
            }
            determineClosest();
        }

        public override void Render()
        {
            base.Render();
            DrawLine(RenderPosition, ColorB.HasValue ? getFadeColor : getSingleColor);
        }
        public void SetLineColor(int index, Color color)
        {
            if (Lines.Count > index)
            {
                Lines[index].Color = color;
            }
        }
        private Color getSingleColor(float position, Line currentLine)
        {
            return ColorA * Alpha;
        }
        private Color getFadeColor(float position, Line currentLine)
        {
            float r = position;
            float d = MathHelper.Distance(r, LineLength / 2);
            float fade = d / (LineLength / 2f);
            return Color.Lerp(ColorA, ColorB.Value, fade) * Alpha;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Debug = true;
            Vector2 p = RenderPosition;
            foreach (Line line in Lines)
            {
                Draw.Line(p + line.A, p + line.B, Color.Orange);
            }
        }
        private void DrawLine(Vector2 position, Func<float, Line, Color> getColor)
        {
            float pos = 0;
            Line currentLine = Lines[closestIndex];
            Vector2 p = Calc.Approach(currentLine.A, currentLine.B, distanceFromClosestToStart);
            Vector2 prev = p;
            float halfSize = Size / 2;
            int i = closestIndex;
            Color prevA = ColorA;
            Color? prevB = ColorB;
            if (Debug)
            {
                ColorA = Color.Cyan;
                ColorB = Color.Magenta;
            }
            while (pos < LineLength)
            {
                while (p != currentLine.B && pos < LineLength)
                {
                    Color c = getColor(pos, currentLine);
                    //testing size changes - currently looks bad
                    if (halfSize > 0)
                    {
                        Vector2 perp = Calc.Perpendicular(p - prev).FourWayNormal() * halfSize;
                        Vector2 start = p + perp;
                        Vector2 target = p - perp;
                        while (start != target)
                        {
                            Draw.Point(start + position, c);
                            start = Calc.Approach(start, target, 1);
                        }
                    }
                    else
                    {
                        Draw.Point(p + position, c);
                    }
                    prev = p;
                    p = Calc.Approach(p, currentLine.B, 1);
                    pos = Calc.Approach(pos, LineLength, 1);
                    /* really cool effect - could be worth keeping around, don't delete!!!!
                    Draw.Line(prev + position, p + position,getColor(pos, currentLine));*/
                }
                i++;
                if (i >= Lines.Count)
                {
                    if (!WrapAround) return;
                    i %= Lines.Count;
                }
                currentLine = Lines[i];
                p = currentLine.A;
            }
            ColorA = prevA;
            ColorB = prevB;
            Debug = false;
        }
        private void determineClosest()
        {
            float length = 0;
            int i = 0;
            while (i < Lines.Count || (WrapAround && LineStart > 0))
            {
                int ii = i % Lines.Count;
                Line line = Lines[ii];
                length += line.Length;
                if (length > LineStart)
                {
                    
                    closestIndex = ii;
                    distanceFromClosestToStart = LineStart - (length - line.Length);
                    break;
                }
                i++;
            }
        }
        public override void Update()
        {
            base.Update();
            determineClosest();
            float length = 0;
            speedOffset += Speed;
            foreach (Line line in Lines)
            {
                length += line.Length;
            }
            speedOffset %= (int)length;
            //experiment
            //AnalyzePlayerMovement();
        }
        private void AnalyzePlayerMovement()
        {
            if (AdjustingNode) speedOffset++;
            float length = 0;
            foreach (Line line in Lines)
            {
                length += line.Length;
            }
            speedOffset %= (int)length;
            if (Input.MoveX.Value != 0)
            {
                if (Input.MoveX.PreviousValue == 0 && !AdjustingNode)
                {
                    AdjustingNode = true;
                    Lines.Add(new Line(Lines[^1].B, Lines[^1].B, Color.White));
                }
                else if (AdjustingNode)
                {
                    Lines[^1].B += Vector2.UnitX * Input.MoveX.Value;
                }
            }
            else if (Input.MoveY.Value != 0)
            {
                if (Input.MoveY.PreviousValue == 0 && !AdjustingNode)
                {
                    AdjustingNode = true;
                    Lines.Add(new Line(Lines[^1].B, Lines[^1].B, Color.White));
                }
                else if (AdjustingNode)
                {
                    Lines[^1].B += Vector2.UnitY * Input.MoveY.Value;
                }
            }
            else
            {
                AdjustingNode = false;
            }
        }
    }
}
