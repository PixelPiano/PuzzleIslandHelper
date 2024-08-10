using Microsoft.Xna.Framework;
using Monocle;
using Microsoft.Xna.Framework.Input;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs
{
    [TerminalProgram("Test")]
    public class TestProgram : TerminalProgram
    {
        public TestProgram(FakeTerminal terminal) : base(terminal)
        {
            Start();
        }
        public override void Start()
        {
            base.Start();
            Add(new Coroutine(Routine()));
        }
        public IEnumerator Routine()
        {
            AddText("Welcome to the text program!");
            yield return 1;
            AddText("Please test the user input function.");
            AddText("Reply with Y (Yes) or N (No)");
            UserInput input = AddUserInput(Color.Cyan, OnSubmit);
            yield return input.WaitForSubmit();
            switch (input.Text)
            {
                case "y" or "yes" or "Y":
                    AddText("User has responded with \"yes\". Thank you!");
                    break;
                case "n" or "no" or "N":
                    AddText("User has responded with \"no\". Thank you!");
                    break;
                default:
                    AddText("Unidentified response. Thank you!");
                    break;
            }
            yield return 1;
            AddText("End of program");
        }
        public bool OnSubmit(string text)
        {
            return true;
        }
    }

}