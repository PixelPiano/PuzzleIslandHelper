using Microsoft.Xna.Framework;
using Monocle;
using System;

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

        public float Alpha = 1;
        public bool Entered;
        public bool OnlyEnterOnce;
        public bool EnterDisabled;
        public string ID;
        public Group(FakeTerminal terminal) : base(terminal.Renderer.TextPosition)
        {
            Terminal = terminal;
            ID = Guid.NewGuid().ToString();
        }
        public virtual void OnEnter()
        {

        }
        public virtual void OnLeft()
        {

        }
        public virtual void OnRight()
        {

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