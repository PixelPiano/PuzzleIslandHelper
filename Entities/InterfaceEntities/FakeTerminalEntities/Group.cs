using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities
{
    [Tracked]
    public class Group : Entity
    {
        public const int LINEHEIGHT = 6;
        public TerminalRenderer Renderer => Terminal.Renderer;
        public FakeTerminal Terminal;
        public bool Halt;
        public bool IsCurrentIndex;
        public bool WasCurrentIndex;
        public int Index;
        public float Alpha = 1;
        public Group(FakeTerminal terminal, int index) : base(terminal.Renderer.TextPosition)
        {
            Terminal = terminal;
        }
        public virtual void TerminalRender(Level level, Vector2 renderAt)
        {
        }
        public virtual void Move(float amount)
        {
            Position.Y += amount;
        }
    }

}