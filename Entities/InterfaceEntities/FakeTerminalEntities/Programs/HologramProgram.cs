using Microsoft.Xna.Framework;
using Monocle;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs
{
    [TerminalProgram("Hologlobe")]
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
        public bool ProgramFinished;
        public bool Closed;
        public States State;
        public static readonly Dictionary<string, List<string>> CommandStrings = new()
        {
            {"on",new(){"turnon","turnon", "on", "start", "activate"} },
            {"off",new(){"turnoff", "turnoff", "off", "end", "deactivate"} },
            {"scan",new(){"scan", "scanning", "scanarea", "scanworld", "detect"} },
            {"launch",new(){"launch","execute"} },
            {"help",new(){"help","info","assist","skibidi","rizz","gyat","ohio","fanum","rizzler","rizzlord","rizzer","stopit"} },
            {"exit", new(){"exit","forceclose","letmeout"} },
            {"clear",new(){"clear","erase"} },
            {"color",new(){"color"}}
        };
        public static readonly List<string> WSMCommands = new()
        {
            "off","scan","launch","select"
        };
        public Color UserColor = Color.Cyan;
        public int SelectedBox;
        public HologramProgram(FakeTerminal terminal) : base(terminal)
        {
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Start();
        }
        public override void Start()
        {
            Add(new Coroutine(Routine()));
        }
        public string GetCommandType(string input)
        {
            if (input.ToLower().Contains("select")) return "select";
            if (input.ToLower().Contains("color")) return "color";
            foreach (KeyValuePair<string, List<string>> pair in CommandStrings)
            {
                foreach (string s in pair.Value)
                {
                    if (s.Equals(input, StringComparison.OrdinalIgnoreCase))
                    {
                        return pair.Key;
                    }
                }
            }
            return null;
        }
        private IEnumerator menu()
        {
            AddText("Welcome. To exit, enter the \"exit\" command.");
            while (State != States.Launched)
            {
                AddText("--Awaiting command--", Color.Yellow);
                UserInput.RefreshBlockedBindings();
                UserInput input = AddUserInput(UserColor);
                yield return input.WaitForSubmit();
                string output = GetCommandType(input.Text.Replace(" ", ""));
                //if command is valid
                if (!string.IsNullOrEmpty(output))
                {
                    if (output == "exit")
                    {
                        Closed = true;
                        yield break;
                    }

                    if (State == States.Off && WSMCommands.Contains(output))
                    {
                        yield return Error("ERROR: WSM cannot execute actions until{n}activated by the command prompt.");
                    }
                    else
                    {
                        switch (output)
                        {
                            case "on":
                                yield return TurnOn();
                                break;
                            case "off":
                                yield return TurnOff();
                                break;
                            case "scan":
                                yield return Scan();
                                break;
                            case "launch":
                                yield return Launch();
                                break;
                            case "help":
                                yield return Help();
                                break;
                            case "select":
                                yield return Select(input.Text);
                                break;
                            case "color":
                                ChangeUserColor(input.Text);
                                break;
                            case "clear":
                                Clear();
                                break;
                        }
                    }
                }
                else
                {
                    yield return Error("ERROR: Unknown command.");
                }
            }
        }
        public static Color? GetColor(string input)
        {
            PropertyInfo[] names = typeof(Color).GetProperties();
            foreach (PropertyInfo info in names)
            {
                if (info != null)
                {
                    var prop = (Color)info.GetValue(null, null);
                    if (prop != null)
                    {
                        string name = info.Name.ToLower();
                        if (name.Equals(input, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return prop;
                        }

                    }
                }
            }
            return null;
        }
        private void ChangeUserColor(string input)
        {
            string text = input.Replace(" ", "").Replace("color", "");
            Color? c = GetColor(text);
            if (c.HasValue)
            {
                UserColor = c.Value;
            }
            else
            {
                UserColor = Calc.HexToColor(text);
            }
        }
        private IEnumerator Error(string message)
        {
            yield return AddText(message, Color.Red);
        }
        public IEnumerator Select(string input)
        {
            if (State is not States.Selecting or States.ReadyToLaunch)
            {
                yield return Error("WSM cannot select a box before" +
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
                yield return Error(errorMessage);
            }
        }
        public IEnumerator Routine()
        {
            yield return menu();
            if (!Closed)
            {
                AddText("End of program");
            }
            yield return Loading("Closing program", null, 3, 0.7f, false);
            Terminal.Close();
            ProgramFinished = true;
            RemoveSelf();
        }
        public IEnumerator TurnOn()
        {
            if (State != States.Off)
            {
                yield return Error("ERROR: WSM is already active.");
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
                yield return Error("Unable to find nearby WSM.");
            }
            yield return null;
        }
        public IEnumerator TurnOff()
        {
            if (State == States.Off)
            {
                yield return Error("ERROR: WSM is already inactive.");
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
                yield return Error("Unable to find nearby WSM.");
            }
            yield return null;
        }
        public IEnumerator Scan()
        {
            if (State is States.Launched)
            {
                yield return Error("ERROR: Cooldown in effect. Estimated time remaining: 2m14d");
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
        public IEnumerator Help()
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
                                    "{n}contact one of the Mandelle brothers.", Color.Yellow);
            AddSpace();
            yield return TextConfirm("Note: WSM cannot procede until area scan" +
                                    "{n}procedure has been initiated and" +
                                    "{n}completed safely.", Color.Orange);
            yield return null;
        }
        public bool Launched;

        public IEnumerator Launch()
        {
            if (Launched)
            {
                yield return TextConfirm("ERROR: Cooldown in effect. Estimated time remaining - 2m14d");
                yield break;
            }
            else if (State != States.ReadyToLaunch)
            {
                yield return Error("ERROR: Cannot launch until a valid{n}scanned box has been selected.");
                yield break;
            }
            yield return Loading("Validating box data", null, 0.76f, 0.5f, false);

            if (SelectedBox != 4)
            {
                yield return Error("ERROR: Selected box is empty." +
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
        private IEnumerator LaunchRoutine()
        {
            yield return null;
        }
    }

}