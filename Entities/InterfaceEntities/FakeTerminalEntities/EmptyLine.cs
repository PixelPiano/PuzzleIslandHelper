using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using static Celeste.Overworld;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities
{
    [TrackedAs(typeof(TextLine))]
    public class EmptyLine : TextLine
    {
        public EmptyLine(FakeTerminal terminal, int index) : base(terminal, "", index, Color.Black)
        {
        }
        public override void TerminalRender(Level level, Vector2 renderAt)
        {
        }
    }
}