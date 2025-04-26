using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities
{
    [TrackedAs(typeof(TextLine))]
    public class Choices : MultiGroup
    {
        public class Choice : TextLine
        {
            public bool Selected;
            public Choice(FakeTerminal terminal, string text, Color color) : base(terminal, text, color)
            {
            }
            public override void Update()
            {
                base.Update();
                DrawsSquare = Selected;
            }
            public override void DrawText(PixelFont font, Vector2 position, Color color)
            {
                if (Selected)
                {
                    Draw.Rect(position, Terminal.Width, LineHeight, color);
                    base.DrawText(font, position, color.Invert());
                }
                else
                {
                    base.DrawText(font, position, color);
                }
            }
        }
        public Choices(FakeTerminal terminal, List<TextLine> lines) : base(terminal, lines)
        {
        }

        public Choices(FakeTerminal terminal, string text, Color color) : base(terminal, text, color)
        {
        }
    }
}