using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities
{
    [TrackedAs(typeof(TextLine))]
    public class MultiGroup : TextLine
    {
        public List<TextLine> Lines = new();
        public int Index;
        private List<MultiGroup> links = new();
        public TextLine Selected => Lines[Index];
        public bool Parent;
        public MultiGroup(FakeTerminal terminal, List<TextLine> lines) : this(terminal, lines[0].Text, Color.White)
        {
            Lines = lines;

        }
        public MultiGroup(FakeTerminal terminal, string text, Color color) : base(terminal, text, color)
        {
        }
        public void AddLine(TextLine line)
        {
            line.Color = Color;
            Lines.Add(line);
        }
        public void LinkToChild(MultiGroup other)
        {
            links.Add(other);
        }
        public override void OnEnter()
        {
            base.OnEnter();
            Selected.OnEnter();
        }
        public override void OnLeft()
        {
            base.OnLeft();
            ShiftBy(-1);
        }
        public override void OnRight()
        {
            base.OnRight();
            ShiftBy(1);
        }
        public void ShiftBy(int value)
        {
            Index = Calc.Clamp(Index + value, 0, Lines.Count - 1);
            foreach (var g in links)
            {
                g.Index = Calc.Clamp(g.Index + value, 0, g.Lines.Count - 1);
            }
        }
        public override void Update()
        {
            base.Update();
            Text = Selected.Text;
            Color = Selected.Color;
        }
    }
}