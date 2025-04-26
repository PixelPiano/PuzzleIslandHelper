using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities
{
    [Tracked]
    public class TerminalRenderer : Entity
    {
        public FakeTerminal Terminal;
        public List<TextLine> Lines = new();
        public UserInput Input;
        public Vector2 TextPosition;
        public bool FromTxt;
        public string Text;
        public readonly int LinesAvailable;
        public Color DebugColor = Color.White;
        public int StartIndex
        {
            get
            {
                return Math.Clamp(startIndex, 0, Math.Max(0, Groups.Count - LinesAvailable));
                //return Groups.Count < LinesAvailable ? 0 : Math.Clamp(startIndex, 0, Math.Max(0, Groups.Count - LinesAvailable));
            }
            set
            {
                startIndex = value;
            }
        }
        public int LineIndex
        {
            get
            {
                return Groups is null || Groups.Count < 1 ? 0 : Calc.Clamp(lineIndex, StartIndex, Groups.Count - 1);
            }
            set
            {
                lineIndex = value;
            }
        }
        private int lineIndex;
        private int startIndex;
        public List<Group> Groups = new();
        public Group SelectedGroup
        {
            get
            {
                int index = LineIndex;
                return Groups is null || Groups.Count == 0 || index < 0 || index >= Groups.Count ? null : Groups[index];
            }

        }
        public bool UserInputSelectedPreviously;
        private Alarm squareAlarm;
        public TerminalRenderer(FakeTerminal terminal)
        {
            Tag |= TagsExt.SubHUD | Tags.TransitionUpdate;
            Terminal = terminal;
            LinesAvailable = (int)(terminal.Height / Group.LINEHEIGHT) - 1;
            TextPosition = Terminal.TopLeft + new Vector2(3, 0);
            squareAlarm = Alarm.Create(Alarm.AlarmMode.Persist, delegate { TextLine.UniversalHideSquare = false; }, 5 * Engine.DeltaTime, false);
            Add(squareAlarm);
            Input = new UserInput(Terminal, Color.Cyan);
            Input.Alpha = 0;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Scene.Add(Input);
            Groups.Add(Input);
        }
        public void MoveInputToFront()
        {
            Groups.Remove(Input);
            Groups.Add(Input);
        }
        public void SetGroupAlphas(float value)
        {
            foreach (Group g in Groups)
            {
                g.Alpha = value;
            }
            //Input.Alpha = value;
        }
        public IEnumerator FadeGroups(float from, float to, float duration)
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
            {
                SetGroupAlphas(Calc.LerpClamp(from, to, i));
                yield return null;
            }
        }
        public void AddGroup(Group group)
        {
            addGroup(group);
        }
        public void AddSpace(int spaces)
        {
            for (int i = 0; i < spaces; i++)
            {
                addGroup(new EmptyLine(Terminal));
            }
        }
        public override void Update()
        {
            base.Update();
            LineIndex = Calc.Clamp(lineIndex, StartIndex, Groups.Count - 1);
            UpdateSelected();
        }
        public void UpdateSelected()
        {
            UserInput.RefreshBlockedBindings();
        }
        public TextLine[] AddText(string input, string prefix, params Color[] lineColors)
        {
            string[] array = input.Split(["{n}"], StringSplitOptions.RemoveEmptyEntries);
            TextLine[] lines = new TextLine[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                Color c = lineColors.Length > i ? lineColors[i] : Color.White;
                if (lineColors.Length > i) c = lineColors[i];
                TextLine line = new TextLine(Terminal, array[i], c)
                {
                    Prefix = prefix,
                };
                lines[i] = line;
                addGroup(line);
            }

            return lines;
        }
        public TextLine[] AddText(string input, string prefix, Color color)
        {
            string[] array = input.Split(["{n}"], StringSplitOptions.RemoveEmptyEntries);
            TextLine[] lines = new TextLine[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                TextLine line = new TextLine(Terminal, array[i], color)
                {
                    Prefix = prefix
                };
                lines[i] = line;
                addGroup(line);
            }
            return lines;
        }
        private void addGroup(Group group)
        {
            squareAlarm.Stop();
            squareAlarm.Start();
            TextLine.UniversalHideSquare = true;
            Scene.Add(group);
            Groups.Add(group);
            MoveInputToFront();
            int count = Groups.Count + 1;
            if (count > LinesAvailable && (count - StartIndex) * Group.LINEHEIGHT > Terminal.Height)
            {
                StartIndex++;
            }
            LineIndex = Groups.Count;
            UpdateSelected();
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (Group g in Groups)
            {
                g.RemoveSelf();
            }
        }

        public override void Render()
        {
            if (Scene is not Level level) return;
            float offset = 0;
            PixelFont font = ActiveFont.Font;
            for (int i = StartIndex; i < Groups.Count && i < StartIndex + LinesAvailable; i++)
            {
                Groups[i].IsCurrentIndex = i == LineIndex;
                Groups[i].TerminalRender(level, TextPosition + Vector2.UnitY * offset, font);
                offset += Group.LINEHEIGHT;
            }
            //Input.TerminalRender(level, TextPosition + Vector2.UnitY * (Group.LINEHEIGHT * (LinesAvailable)));

        }
        public void Clear()
        {
            foreach (Group group in Scene.Tracker.GetEntities<Group>())
            {
                Scene.Remove(group);
            }
        }
    }

}