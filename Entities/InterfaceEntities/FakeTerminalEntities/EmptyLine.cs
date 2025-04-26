using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities
{
    [TrackedAs(typeof(TextLine))]
    public class EmptyLine : TextLine
    {
        public EmptyLine(FakeTerminal terminal) : base(terminal, "", Color.Black)
        {
        }
        public override void TerminalRender(Level level, Vector2 renderAt, PixelFont font)
        {
        }
    }
}