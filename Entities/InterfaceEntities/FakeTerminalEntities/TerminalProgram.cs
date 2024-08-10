using Microsoft.Xna.Framework;
using Monocle;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using System.Security.Policy;
using System.Collections.Generic;
using System;
using VivHelper.Triggers;
using YamlDotNet.Core.Tokens;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities
{
    [Tracked]
    public class TerminalProgram : Entity
    {
        public FakeTerminal Terminal;
        public string Name;
        public bool HitEnter;
        private bool _hitEnter;
        public TerminalProgram(FakeTerminal terminal) : base()
        {
            Terminal = terminal;
        }
        public void Clear()
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
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            TextInput.OnInput -= TextInput_OnInput;
        }
        private void TextInput_OnInput(char obj)
        {
            if (Scene is not Level level || level.Paused) return;
            if (obj == '\r')
            {
                _hitEnter = true;
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
        public virtual void Start()
        {
        }
    }

}