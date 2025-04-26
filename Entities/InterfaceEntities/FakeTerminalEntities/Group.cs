using Microsoft.Xna.Framework;
using Monocle;
using System;
using static Celeste.Mod.PuzzleIslandHelper.Entities.LabGeneratorPuzzle.PuzzleOverlay;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities
{
    [Tracked]
    public class Group : Entity
    {
        public static class GroupTags
        {
            
            [OnInitialize]
            public static void Initialize()
            {

            }
        }
        public const int LINEHEIGHT = 6;
        public TerminalRenderer Renderer => Terminal.Renderer;
        public FakeTerminal Terminal;
        public bool Halt;
        public bool IsCurrentIndex;
        public bool WasCurrentIndex;
        public int Index;
        public float Alpha = 1;
        public bool Entered;
        public bool OnlyEnterOnce;
        public bool EnterDisabled;
        public string ID;
        public bool UseWaitRoutine;
        public virtual IEnumerator WaitRoutine()
        {
            yield return null;
        }
        public Group(FakeTerminal terminal) : base()
        {
            Terminal = terminal;
            ID = Guid.NewGuid().ToString();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Position = Terminal.Renderer.TextPosition;
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
        public virtual void TerminalRender(Level level, Vector2 renderAt, PixelFont font)
        {
        }
        public virtual void Move(float amount)
        {
            Position.Y += amount;
        }
    }

}