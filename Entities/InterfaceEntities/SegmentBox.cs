using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using TAS.EverestInterop;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [Tracked]
    public class SegmentBox : WindowComponent
    {
        public char DefaultChar = '#';
        private Func<string, bool> onSubmit;
        public SegmentBoxText Helper;
        public Color BoxColor => Interface.NightMode ? Color.SlateBlue : Color.LightGray;
        public Vector2 ScreenSpacePosition => Scene is not Level level ? Vector2.Zero : (RenderPosition - level.Camera.Position) * 6;
        public int Characters = 3;
        public int Height = 14;
        public int Width => CellWidth * Characters;
        public int CellWidth => (int)(ActiveFont.BaseSize / 6 + Pad);
        public int SelectedCell;
        public int Pad;
        public bool Selected;
        private float arrowTimer;
        private bool consumedButton;
        private string defaultText;
        public Func<char, bool> IsValidCharacter;
        public Rectangle Bounds;

        public SegmentBox(Window window, int maxChars, int pad, Func<string, bool> onSubmit = null, Func<char, bool> isValidCharacter = null) : base(window)
        {
            this.onSubmit = onSubmit;
            Characters = maxChars;
            Pad = pad;
            IsValidCharacter = isValidCharacter;
            for (int i = 0; i < Characters; i++)
            {
                defaultText += DefaultChar;
            }
            Helper = new SegmentBoxText(this);
        }
        public override void OnOpened(Scene scene)
        {
            base.OnOpened(scene);
            Position = new Vector2(Window.CaseWidth / 2, Window.CaseHeight / 2) - new Vector2(Width / 2, Height / 2);
        }
        public void ClearText()
        {
            Helper.Text = defaultText;
        }
        public void BlockKeyPress(Hotkeys.Hotkey hotkeys)
        {
            foreach (var key in hotkeys.Keys)
            {
                if (MInput.Keyboard.Pressed(key))
                {
                    MInput.UpdateNull();
                }
            }
            MInput.UpdateNull();
        }

        public override void Update()
        {
            arrowTimer = Math.Max(arrowTimer - Engine.DeltaTime, 0);
            Visible = Window.Drawing;
            Helper.Visible = Visible;
            if (Selected)
            {
                MInput.Disabled = false;
                MInput.Update();
                if (MInput.Keyboard.Pressed(Keys.Delete))
                {
                    if (Helper.Text.Length > 0)
                    {
                        ClearText();
                    }
                    consumedButton = true;
                    MInput.UpdateNull();
                }
                else if (arrowTimer <= 0)
                {
                    if (MInput.Keyboard.Check(Keys.Left) || Input.MoveX.Value < 0)
                    {
                        SelectedCell = Math.Max(0, SelectedCell - 1);
                        arrowTimer = 0.2f;
                        consumedButton = true;
                        MInput.UpdateNull();
                    }
                    else if (MInput.Keyboard.Check(Keys.Right) || Input.MoveX.Value > 0)
                    {
                        SelectedCell = Math.Min(SelectedCell + 1, Characters - 1);
                        arrowTimer = 0.2f;
                        consumedButton = true;
                        MInput.UpdateNull();
                    }
                }
                MInput.Disabled = consumedButton;
            }
            consumedButton = false;
            base.Update();
            Vector2 p = RenderPosition;
            if (Interface.LeftPressed)
            {
                Selected = false;
                for (int i = 0; i < Characters; i++)
                {
                    if (Interface.MouseOver(p + (Vector2.UnitX * CellWidth * i), CellWidth, Height))
                    {
                        Selected = true;
                        SelectedCell = i;
                        break;
                    }
                }
            }
            if (Selected)
            {
                Window.BlockHotkeysThisFrame();
            }
        }
        public static bool AddedToOnInput;
        public override void Added(Entity entity)
        {
            base.Added(entity);
            Scene.Add(Helper);
            Helper.Text = defaultText;
            if (!AddedToOnInput)
            {
                TextInput.OnInput += OnTextInput;
                AddedToOnInput = true;
            }
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            if (AddedToOnInput)
            {
                TextInput.OnInput -= OnTextInput;
                AddedToOnInput = false;
            }
            MInput.Disabled = false;
        }
        public void OnTextInput(char c)
        {
            if (!Selected || Scene is not Level level || level.Paused || Interface.Buffering)
            {
                return;
            }
            char[] array = Helper.Text.ToCharArray();
            char[] array2 = Helper.Text.ToCharArray();
            switch (c)
            {
                case '\r':
                    Engine.Scene.OnEndOfFrame += delegate
                    {
                        if (string.IsNullOrEmpty(Helper.Text))
                        {
                            return;
                        }
                        if (onSubmit != null && !onSubmit.Invoke(Helper.Text))
                        {
                            ClearText();
                        }
                    };
                    break;
                case '\b':

                    for (int i = SelectedCell; i < Characters; i++)
                    {
                        array[Math.Max(i - 1, 0)] = array2[i];
                    }
                    array[Characters - 1] = DefaultChar;
                    SelectedCell = Math.Max(SelectedCell - 1, 0);
                    break;
                case ' ':
                    if (SelectedCell < Characters - 1)
                    {
                        for (int i = Characters - 1; i > SelectedCell; i--)
                        {
                            array[i] = array2[i - 1];
                        }
                        array[SelectedCell] = DefaultChar;
                        SelectedCell++;
                    }
                    else
                    {
                        array[^1] = DefaultChar;
                    }
                    break;
                default:
                    {
                        if (!char.IsControl(c) && ActiveFont.FontSize.Characters.ContainsKey(c) && (IsValidCharacter == null || IsValidCharacter.Invoke(c)))
                        {
                            array[SelectedCell] = c;
                        }
                        break;
                    }
            }
            Helper.Text = new string(array);
            consumedButton = true;
            MInput.Disabled = true;
            MInput.UpdateNull();
        }
        public void Deselect()
        {
            Selected = false;
        }
        public override void Render()
        {
            base.Render();
            bool altColor = Selected && !Interface.Buffering;
            Color boxColor = altColor ? Color.Blue : BoxColor;
            Vector2 p = RenderPosition;
            for (int i = 0; i < Characters; i++)
            {
                Draw.Rect(p + (Vector2.UnitX * CellWidth * i), CellWidth, Height,
                    altColor && SelectedCell == i ? Color.Blue : BoxColor);
            }
            for (int i = 0; i < Characters; i++)
            {
                Draw.HollowRect(p + (Vector2.UnitX * CellWidth * i) - Vector2.One, CellWidth + 1, Height + 2, Color.Lerp(BoxColor, Color.White, 0.5f));
            }
            if (altColor)
            {
                Draw.HollowRect(p + Vector2.UnitX * CellWidth * SelectedCell - Vector2.One, CellWidth + 1, Height + 2, Color.White);
            }

        }
        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            Helper.RemoveSelf();

            if (AddedToOnInput)
            {
                TextInput.OnInput -= OnTextInput;
                AddedToOnInput = false;
            }
            MInput.Disabled = false;
        }
        [Tracked]
        public class SegmentBoxText : TextHelper
        {
            public SegmentBox Track;
            public SegmentBoxText(SegmentBox track) : base(Color.White)
            {
                Track = track;
            }
            public override void Update()
            {
                base.Update();
                Visible = Track.Visible;
            }
            public override void Render()
            {
                base.Render();
                Vector2 sc = Track.ScreenSpacePosition + new Vector2(Track.Pad + Track.CellWidth / 2, Track.Height / 2) * 6;
                foreach (char c in Text)
                {
                    string s = c.ToString();
                    DrawOutline(sc - ActiveFont.Measure(s).XComp(), s);
                    sc.X += Track.CellWidth * 6;
                }
                DrawSnippets(sc);
            }
        }
    }
}