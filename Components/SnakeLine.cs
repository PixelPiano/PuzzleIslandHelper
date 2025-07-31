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
            public float ColorLerp;
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
            public Color ColorA
            {
                get
                {
                    return ColorLerp switch
                    {
                        0 => colora * Alpha,
                        1 => AltColorA * Alpha,
                        _ => Color.Lerp(colora, AltColorA, ColorLerp) * Alpha,
                    };
                }
                set => colora = value;
            }
            public Color ColorB
            {
                get
                {
                    return ColorLerp switch
                    {
                        0 => colorb * Alpha,
                        1 => AltColorB * Alpha,
                        _ => Color.Lerp(colorb, AltColorB, ColorLerp) * Alpha,
                    };
                }
                set => colorb = value;
            }
            private Color colora;
            private Color colorb;
            public Color AltColorA;
            public Color AltColorB;
            public float Alpha = 1;
            public bool Visible = true;
            public float Length;
            public Line(Vector2 start, Vector2 end, Color colorA, Color? colorB = null, Color? altColorA = null, Color? altColorB = null)
            {
                a = LineStart = LineEnd = start;
                B = end;
                ColorA = colorA;
                ColorB = colorB ?? Color.Transparent;
                AltColorA = altColorA ?? Color.Transparent;
                AltColorB = altColorB ?? Color.Transparent;
            }
        }
        public List<Line> Lines = [];
        public List<Vector2> Nodes = [];
        public bool AdjustingNode;
        public float LineLength;
        public float StartFade;
        public float EndFade;
        public Color ColorA
        {
            get => colora;
            set
            {
                colora = value;
                foreach (Line line in Lines)
                {
                    line.ColorA = value;
                }
            }
        }
        public float ColorLerp
        {
            get => _colorlerp;
            set
            {
                _colorlerp = value;
                foreach (Line line in Lines)
                {
                    line.ColorLerp = value;
                }
            }
        }
        private float _colorlerp;
        public Color ColorB
        {
            get => colorb;
            set
            {
                colorb = value;
                foreach (Line line in Lines)
                {
                    line.ColorB = value;
                }
            }
        }
        private Color colora;
        private Color colorb = Color.Transparent;
        public Color AltColorA
        {
            get => altColorA;
            set
            {
                altColorA = value;
                foreach (Line line in Lines)
                {
                    line.AltColorA = value;
                }
            }
        }
        public Color AltColorB
        {
            get => altColorB;
            set
            {
                altColorB = value;
                foreach (Line line in Lines)
                {
                    line.AltColorB = value;
                }
            }
        }
        private Color altColorA;
        private Color altColorB;
        public float ColorAAlpha = 1;
        public float ColorBAlpha = 1;
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
        public float Alpha
        {
            get => _alpha;
            set
            {
                _alpha = value;
                foreach (Line line in Lines)
                {
                    line.Alpha = value;
                }
            }
        }
        private float _alpha = 1;
        public bool Debug;
        public float LineOffset;
        public SnakeLine(Vector2 offset, List<Vector2> nodes, float start, float length, float startFade, float endFade, float speed,
            Color color, Color? colorB = null, Color? altColorA = null, Color? altColorB = null) : base(true)
        {
            Speed = speed;
            Position = offset;
            LineStart = start;
            colora = color;
            colorb = colorB ?? Color.Transparent;
            LineLength = length;
            StartFade = startFade;
            EndFade = endFade;
            Nodes = nodes;
            Vector2 prev = Nodes[0];
            for (int i = 1; i < Nodes.Count; i++)
            {
                Vector2 current = Nodes[i];
                Line line = new Line(prev, current, color, colorB, altColorA, altColorB);
                Lines.Add(line);
                prev = current;
            }
            determineClosest();
        }

        public override void Render()
        {
            base.Render();
            if(Alpha <= 0 || (ColorAAlpha <= 0 && ColorBAlpha <= 0)) return;
            DrawLine(RenderPosition, getFadeColor);
        }
        private Color getFadeColor(float position, Line currentLine)
        {
            float lineLength = LineLength / 2f;
            float fade = MathHelper.Distance(position, lineLength) / lineLength;
            return Color.Lerp(currentLine.ColorA * ColorAAlpha, currentLine.ColorB * ColorBAlpha, fade);
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
            Color prevB = ColorB;
            float prevF1 = ColorAAlpha;
            float prevF2 = ColorBAlpha;
            if (Debug)
            {
                ColorAAlpha = 1;
                ColorBAlpha = 1;
                ColorA = Color.Cyan;
                ColorB = Color.Magenta;
            }
            while (pos < LineLength)
            {
                while (p != currentLine.B && pos < LineLength)
                {
                    if (currentLine.Visible)
                    {
                        Color c = getColor(pos, currentLine);
                        if (c.A > 0)
                        {
                            //testing size changes - currently looks bad
                            /*  if (halfSize > 0)
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
                              }*/
                            Draw.Point(p + position, c);
                        }
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
                    if (!WrapAround)
                    {
                        ColorA = prevA;
                        ColorB = prevB;
                        ColorAAlpha = prevF1;
                        ColorBAlpha = prevF2;
                        Debug = false;
                        return;
                    }
                    i %= Lines.Count;
                }
                currentLine = Lines[i];
                p = currentLine.A;
            }
            ColorA = prevA;
            ColorB = prevB;
            ColorAAlpha = prevF1;
            ColorBAlpha = prevF2;
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
            speedOffset += Speed * Engine.DeltaTime;

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
                    Lines.Add(new Line(Lines[^1].B, Lines[^1].B, Color.White, Color.White));
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
                    Lines.Add(new Line(Lines[^1].B, Lines[^1].B, Color.White, Color.White));
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
