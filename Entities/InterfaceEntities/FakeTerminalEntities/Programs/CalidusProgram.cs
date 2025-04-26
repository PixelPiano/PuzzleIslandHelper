using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs
{
    [CustomProgram("Calidus")]
    public class CalidusProgram : TerminalProgram
    {
        public CalidusProgram(FakeTerminal terminal) : base(terminal)
        {
            LinePrefix = "";
            Welcome = "BITCH HELLO";
            terminal.TypingEnabled = false;
        }
        public override IEnumerator Help(string input)
        {
            yield return TextConfirm("--Welcome to AAAAAAAAAAAAAA!--", Color.Orange);
            AddSpace();
            yield return TextConfirm("The WSM (World Shift Machine) is to only be" +
                                    "{n}operated by qualifying laboratory members.", Color.Yellow);
            AddSpace();
            yield return TextConfirm("{n}Any attempt to use this machine without" +
                                    "{n}sufficient knowledge could put you and " +
                                    "{n}others at a high risk of danger.", Color.Yellow);
            AddSpace();
            yield return TextConfirm("If you would like a demonstration, please" +
                                    "{n}contact the scientist.", Color.Yellow);
            AddSpace();
            yield return TextConfirm("Note: WSM cannot procede until area scan" +
                                    "{n}procedure has been initiated and" +
                                    "{n}completed safely.", Color.Orange);
            yield return null;
        }
        public override void BeforeBegin()
        {
        }
        public override void AfterWelcome()
        {
            base.AfterWelcome();
        }
        public override bool Continue() => true;
        public override void OnClose()
        {

        }
    }

}