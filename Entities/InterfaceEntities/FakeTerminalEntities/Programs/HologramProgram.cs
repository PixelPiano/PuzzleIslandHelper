using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs
{
    [CustomProgram("Hologlobe")]
    public class HologramProgram : TerminalProgram
    {
        public enum States
        {
            Off,
            Idle,
            Selecting,
            ReadyToLaunch,
            Launched,
        }
        public bool Launched;
        public States State;
        public int SelectedBox;
        public HologramProgram(FakeTerminal terminal) : base(terminal)
        {
            AddCommand("on", null, TurnOn, "turnon", "turnon", "start", "activate");
            AddCommand("off", null, TurnOff, "turnoff", "turnoff", "end", "deactivate");
            AddCommand("scan", null, Scan, "scanning", "scanning", "scanworld", "detect");
            AddCommand("launch", null, Launch, "execute");
            AddCommand("select", null, Select);
        }
        public IEnumerator Select(string input)
        {
            if (State is not States.Selecting or States.ReadyToLaunch)
            {
                Error("WSM cannot select a box before" +
                                   "{n}performing a scan of the area.");
                yield break;
            }
            string[] array = input.Split(' ');
            bool found = false;
            string errorMessage = null;
            if (array.Length < 2)
            {
                errorMessage = "ERROR: No number provided after \"select\".";
            }
            else
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (!found)
                    {
                        if (array[i].Equals("select"))
                        {
                            found = true;
                        }
                        continue;
                    }
                    if (int.TryParse(array[i], out int result))
                    {
                        if (result < 1 || result > 6)
                        {
                            errorMessage = "ERROR: Selected index is out of bounds.";
                        }
                        else
                        {
                            errorMessage = null;
                            State = States.ReadyToLaunch;
                            SelectedBox = result;
                            yield return TextConfirm("Area data at index " + result + " selected. " +
                                                    "{n}Ready to launch!", Color.Green);
                        }
                        break;
                    }
                    else
                    {
                        errorMessage = "ERROR: Non-numerical entry after string \"select\"." +
                                        "{n}Please provide a number.";
                        break;
                    }
                }
            }
            if (!string.IsNullOrEmpty(errorMessage))
            {
                Error(errorMessage);
            }
        }
        public IEnumerator TurnOn(string input)
        {
            if (State != States.Off)
            {
                Error("ERROR: WSM is already active.");
                yield break;
            }
            WorldShiftMachine machine = Scene.Tracker.GetEntity<WorldShiftMachine>();
            if (machine != null)
            {
                yield return machine.Activate();
                yield return AddText("WSM successfully turned on.");
                State = States.Idle;
            }
            else
            {
                Error("Unable to find nearby WSM.");
            }
            yield return null;
        }
        public IEnumerator TurnOff(string input)
        {
            if (State == States.Off)
            {
                Error("ERROR: WSM is already inactive.");
                yield break;
            }
            WorldShiftMachine machine = Scene.Tracker.GetEntity<WorldShiftMachine>();
            if (machine != null)
            {
                yield return machine.Deactivate();
                yield return AddText("WSM successfully turned off.");
                State = States.Off;
            }
            else
            {
                Error("Unable to find nearby WSM.");
            }
            yield return null;
        }
        public IEnumerator Scan(string input)
        {
            if (State == States.Off)
            {
                EarlyActionError();
                yield break;
            }
            if (State is States.Launched)
            {
                Error("ERROR: Cooldown in effect. Estimated time remaining: 2m14d");
                yield break;
            }
            yield return Loading("Scanning", null, 0.3f, 0.7f, true);
            yield return AddText("Scan complete!", Color.LimeGreen);
            yield return AddText("Please select an area box below to modify.", Color.LimeGreen);
            yield return AddText("\t 1     2     3    4    5    6", Color.White);
            yield return AddText("\t[  ]  [  ]  [  ]  [*]  [  ]  [  ]", Color.White);
            State = States.Selecting;
            yield return null;
        }
        public override IEnumerator Help(string input)
        {
            yield return TextConfirm("--Welcome to the WSM help menu!--", Color.Orange);
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
        public override bool Continue() => State != States.Launched;
        public IEnumerator Launch(string input)
        {
            if (State == States.Off)
            {
                EarlyActionError();
                yield break;
            }
            if (Launched)
            {
                yield return TextConfirm("ERROR: Cooldown in effect. Estimated time remaining - 2m14d");
                yield break;
            }
            else if (State != States.ReadyToLaunch)
            {
                Error("ERROR: Cannot launch until a valid{n}scanned box has been selected.");
                yield break;
            }
            yield return Loading("Validating box data", null, 0.76f, 0.5f, false);

            if (SelectedBox != 4)
            {
                Error("ERROR: Selected box is empty." +
                    "{n}Please select another box.");
                State = States.Selecting;
                yield break;
            }
            Coroutine load = new Coroutine(Loading("Preparing to launch", "Preparations complete!", 0.76f, 0.5f, true));
            Add(load);
            Hologlobe hologlobe = Scene.Tracker.GetEntity<Hologlobe>();
            if (hologlobe is null) yield break;
            for (int i = 0; i < 2; i++)
            {
                hologlobe.Glitchy = true;
                yield return Calc.Random.Range(1, 5) * Engine.DeltaTime;
                hologlobe.Glitchy = false;
                yield return Calc.Random.Range(3, 9) * Engine.DeltaTime;
            }
            hologlobe.Glitchy = true;
            while (!load.Finished)
            {
                yield return null;
            }
            SceneAs<Level>().Session.SetFlag("templeAccess");
            hologlobe.Glitchy = false;
            State = States.Launched;
            yield return null;
        }
        private void EarlyActionError()
        {
            Error("ERROR: WSM cannot execute actions until{n}activated by the command prompt.");
        }
        public override void OnClose()
        {

        }
    }

}