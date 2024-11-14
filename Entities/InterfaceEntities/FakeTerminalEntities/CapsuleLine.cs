using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities
{
    [TrackedAs(typeof(TextLine))]
    public class CapsuleLine : TextLine
    {
        public WarpProgram Program;
        public WarpCapsuleData Data => PianoMapDataProcessor.WarpLinks[Text.Replace(" ", "").ToLower()];
        public bool Unlocked => PianoModule.Session.LoggedCapsules[Text];
        public CapsuleLine(FakeTerminal terminal, WarpProgram program, string text) : base(terminal, text, PianoModule.Session.LoggedCapsules[text] ? Color.Lime : Color.Red)
        {
            Program = program;
            OnlyEnterOnce = true;
        }
        public override void Update()
        {
            base.Update();
            Color = Unlocked ? Color.Lime : Color.Red;
        }
        public override void OnEnter()
        {
            string text = Text.Replace(" ", "").ToLower();
            if (!Unlocked)
            {
                Program.Error("Cannot connect, missing password.");
                return;
            }
            Program.SetTarget(Text.Replace(" ", "").ToLower());
        }
    }
}