using Microsoft.Xna.Framework;
using Monocle;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using System.Security.Policy;
using System.Collections.Generic;
using System;
using VivHelper.Triggers;
using YamlDotNet.Core.Tokens;
using System.Reflection;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities
{
    public class TerminalCommand
    {
        public readonly List<string> Identifiers = new();
        public readonly string ID;
        private string idLower;
        public Action<string> ActionOnCommand;
        public Func<string, IEnumerator> RoutineOnCommand;
        public TerminalCommand(string id, Action<string> action, Func<string, IEnumerator> routine, params string[] identifiers)
        {
            ActionOnCommand = action;
            RoutineOnCommand = routine;
            ID = id.ToLower();
            Identifiers.Add(ID);
            foreach (string s in identifiers)
            {
                Identifiers.Add(s.ToLower());
            }
        }
        public static TerminalCommand Create(string id, Action<string> actionOnCommand = null, Func<string, IEnumerator> routineOnCommand = null, params string[] identifiers)
        {
            return new TerminalCommand(id, actionOnCommand, routineOnCommand, identifiers);
        }
        public bool Check(string input)
        {
            foreach (string s in Identifiers)
            {
                if (s.Equals(input))
                {
                    return true;
                }
            }
            return false;
        }
    }
    [Tracked]
    public abstract class TerminalProgram : Entity
    {
        public FakeTerminal Terminal;
        public static bool ShowCommandDebug = false;
        public string Name;
        public bool HitEnter;
        private bool _hitEnter;
        public bool ProgramFinished;
        public bool Closed;
        public string Welcome;
        public Color UserColor = Color.Cyan;
        private bool forcedClosed;
        public List<TerminalCommand> Commands = new();
        public List<TerminalCommand> DefaultCommands = new();
        public void AddCommand(string id, Action<string> action = null, Func<string, IEnumerator> routine = null, params string[] identifiers)
        {
            Commands.Add(TerminalCommand.Create(id, action, routine, identifiers));
        }
        public TerminalCommand CreateCommand(string id, Action<string> action = null, Func<string, IEnumerator> routine = null, params string[] identifiers)
        {
            TerminalCommand command;
            command = TerminalCommand.Create(id, action, routine, identifiers);
            return command;
        }
        public bool TryGetCommand(List<TerminalCommand> list, string input, out TerminalCommand command)
        {
            foreach (TerminalCommand c in list)
            {
                if (c.Check(input))
                {
                    command = c;
                    return true;
                }
            }
            command = null;
            return false;
        }
        public TerminalProgram(FakeTerminal terminal, string welcomeMessage = "") : base()
        {
            Terminal = terminal;
            Welcome = welcomeMessage;
            DefaultCommands = new()
            {
                CreateCommand("help", null, Help),
                CreateCommand("exit",CloseMenu),
                CreateCommand("clear", Clear),
                CreateCommand("color",ChangeUserColor)
            };
        }
        public void Clear(string input)
        {
            Terminal.Clear();
        }
        public override void Update()
        {
            base.Update();
            HitEnter = _hitEnter;
            if (_hitEnter) _hitEnter = false;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            TextInput.OnInput += TextInput_OnInput;
            BeforeBegin();
            Begin();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            TextInput.OnInput -= TextInput_OnInput;
        }
        public void SplitInput(string text, out string first, out string second)
        {
            first = text;
            second = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == ' ')
                {
                    first = text.Substring(0, i);
                    if (i + 1 < text.Length)
                    {
                        second = text.Substring(i + 1);
                    }
                    return;
                }
            }
        }
        public IEnumerator CommandEnter()
        {
            while (Continue())
            {
                AddText("--Awaiting command--", Color.Yellow);
                UserInput.RefreshBlockedBindings();
                UserInput input = AddUserInput(UserColor);
                yield return input.WaitForSubmit();
                SplitInput(input.Text, out string first, out string second);
                first = first.ToLower();
                if (TryGetCommand(DefaultCommands, first, out TerminalCommand defaultCommand))
                {
                    defaultCommand.ActionOnCommand?.Invoke(second);
                    if (defaultCommand.RoutineOnCommand != null) yield return defaultCommand.RoutineOnCommand.Invoke(second);

                }
                else if (TryGetCommand(Commands, first, out TerminalCommand customCommand))
                {
                    customCommand.ActionOnCommand?.Invoke(second);
                    if (customCommand.RoutineOnCommand != null) yield return customCommand.RoutineOnCommand.Invoke(second);
                }
                else
                {
                    yield return Error("ERROR: Unknown command.");
                    if (ShowCommandDebug)
                    {
                        AddText("First: " + first + ", Second: " + second);
                    }
                }
            }
        }
        public IEnumerator Routine()
        {
            AddText(Welcome);
            yield return CommandEnter();
            OnClose();
            yield return Loading("Closing program", null, 3, 0.7f, false);
            Terminal.Close();
            ProgramFinished = true;
            RemoveSelf();
        }
        public void CloseMenu(string input)
        {
            Closed = true;
        }
        private void TextInput_OnInput(char obj)
        {
            if (Scene is not Level level || level.Paused) return;
            if (obj == '\r')
            {
                _hitEnter = true;
            }
        }
        public IEnumerator Error(string message)
        {
            yield return AddText(message, Color.Red);
        }
        public void ChangeUserColor(string input)
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
        public IEnumerator Loading(string fillerString, string completeString, float rate, float interval, bool showProgress)
        {
            fillerString ??= "Loading";
            string dots = "...";
            int dotCount = 3;
            float percentage = 0;

            TextLine[] lines = showProgress ?
                AddText(fillerString + dots + "{n}/100", Color.LightGray, Color.Magenta) :
                AddText(fillerString + dots, Color.LightGray);

            float endMult;
            while (percentage < 100)
            {

                endMult = 0.01f + 0.96f * (Calc.Clamp(100 - percentage, 0, 30) / 40f) * 0.95f;

                if (showProgress)
                {
                    lines[1].Text = Calc.Clamp(percentage, 0, 100).ToString("F2") + "/100";
                }
                if (Scene.OnInterval(interval))
                {
                    dotCount++;
                    dotCount %= 4;
                    dots = "";
                    for (int i = 0; i < dotCount; i++)
                    {
                        dots += ".";
                    }
                    lines[0].Text = fillerString + dots;
                }
                percentage += rate * endMult;

                yield return null;
            }
            if (showProgress)
            {
                lines[1].Text = "100/100";
            }
            lines[0].Text = fillerString + "...";
            if (!string.IsNullOrEmpty(completeString))
            {
                yield return TextConfirm(completeString);
            }
        }
        public IEnumerator TextConfirm(string text, Color color = default)
        {
            AddText(text, color);
            while (!HitEnter && !Input.DashPressed)
            {
                yield return null;
            }
        }
        public void AddSpace(int spaces = 1)
        {
            Terminal.AddSpace(spaces);
        }
        public void AddGroup(Group group)
        {
            Terminal.AddGroup(group);
        }
        public TextLine[] AddText(string text, params Color[] lineColors)
        {
            return Terminal.AddText(text, lineColors);
        }
        public TextLine[] AddText(string text, Color color)
        {
            return Terminal.AddText(text, color);
        }
        public UserInput AddUserInput(Color color, Func<string, bool> onSubmit = null)
        {
            return Terminal.AddUserInput(color, onSubmit);
        }

        public virtual void BeforeBegin() { }
        public void Begin()
        {
            Add(new Coroutine(Routine()));
        }
        public abstract bool Continue();
        public abstract IEnumerator Help(string input);
        public abstract void OnClose();
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
    }

}