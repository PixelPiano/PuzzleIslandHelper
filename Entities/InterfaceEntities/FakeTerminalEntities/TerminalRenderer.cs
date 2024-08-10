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
        public Vector2 TextPosition;
        public bool FromTxt;
        public string Text;
        public readonly int LinesAvailable;
        public int StartIndex
        {
            get
            {
                if (Groups.Count < LinesAvailable) return 0;
                return Calc.Clamp(startIndex, 0, (int)Calc.Max(0, Groups.Count - LinesAvailable));
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
                return Calc.Clamp(lineIndex, StartIndex, Groups.Count - 1);
            }
            set
            {
                lineIndex = value;
            }
        }
        private int lineIndex;
        private int startIndex;
        public List<Group> Groups = new();

        public TerminalRenderer(FakeTerminal terminal)
        {
            Tag |= TagsExt.SubHUD | Tags.TransitionUpdate;
            Terminal = terminal;
            LinesAvailable = (int)(terminal.Height / Group.LINEHEIGHT);
            TextPosition = Terminal.TopLeft + new Vector2(3, 0);
        }
        public void SetGroupAlphas(float value)
        {
            foreach(Group g in Groups)
            {
                g.Alpha = value;
            }
        }
        public IEnumerator FadeGroups(float from, float to, float duration)
        {
            for(float i = 0; i<1; i += Engine.DeltaTime / duration)
            {
                float alpha = Calc.LerpClamp(from, to, i);
                foreach(Group g in Groups)
                {
                    g.Alpha = alpha;
                }
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
                addGroup(new EmptyLine(Terminal, Groups.Count + 1));
            }
        }
        public bool UserInputSelectedPreviously;
        public override void Update()
        {
            base.Update();
            if(Groups is null || lineIndex >= Groups.Count) return;
            UserInput.CurrentlySelected = Groups[LineIndex] is UserInput;
            if(UserInput.CurrentlySelected != UserInput.PreviouslySelected)
            {
                UserInput.RefreshBlockedBindings();
            }
            UserInput.PreviouslySelected = UserInput.CurrentlySelected;
        }
        public TextLine[] AddText(string input, params Color[] lineColors)
        {
            string[] array = input.Split(new string[] { "{n}" }, StringSplitOptions.RemoveEmptyEntries);
            TextLine[] lines = new TextLine[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                Color c = lineColors.Length > i ? lineColors[i] : Color.White;
                if (lineColors.Length > i) c = lineColors[i];
                TextLine line = new TextLine(Terminal, array[i], Groups.Count + 1, c);
                lines[i] = line;
                addGroup(line);
            }
            return lines;
        }
        public TextLine[] AddText(string input, Color color)
        {
            string[] array = input.Split(new string[] { "{n}" }, StringSplitOptions.RemoveEmptyEntries);
            TextLine[] lines = new TextLine[array.Length];
            for (int i = 0; i < array.Length; i++)
            {

                TextLine line = new TextLine(Terminal, array[i], Groups.Count + 1, color);
                lines[i] = line;
                addGroup(line);
            }
            return lines;
        }
        public UserInput AddUserInput(Color color, Func<string, bool> onSubmit = null)
        {
            UserInput input = new UserInput(Terminal, Groups.Count + 1, color, onSubmit);
            addGroup(input);
            return input;
        }
        private void addGroup(Group group)
        {
            Groups.Add(group);
            Scene.Add(group);
            if (Groups.Count > LinesAvailable)
            {
                if ((Groups.Count - StartIndex) * Group.LINEHEIGHT > Terminal.Height)
                {
                    StartIndex++;
                }
            }
            LineIndex = Groups.Count;
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
            for (int i = StartIndex; i < Groups.Count && i < StartIndex + LinesAvailable; i++)
            {
                Groups[i].IsCurrentIndex = i == LineIndex;
                Groups[i].TerminalRender(level, TextPosition + Vector2.UnitY * offset);
                offset += Group.LINEHEIGHT;
            }
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