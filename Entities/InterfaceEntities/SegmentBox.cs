using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using TAS.EverestInterop;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [Tracked]
    public class SegmentBox : WindowComponent
    {
        public struct Box
        {
            public int Levels;
            public int LevelHeight;
            public int Width;
            public Vector2 Offset;
            public int CurrentLevel;
            public Color Color;
            private const float flashTimer = 0.3f;
            private float timer = 0;

            public Box(Vector2 offset, int levels, int levelHeight, int width, Color color)
            {
                Offset = offset;
                Levels = levels;
                LevelHeight = levelHeight;
                Width = width;
                Color = color;
            }
            public void Flash()
            {
                timer = flashTimer;
            }
            public void Update()
            {
                if (timer > 0)
                {
                    timer -= Engine.DeltaTime;
                }
                else
                {
                    timer = 0;
                }
            }
            public void Add(int value)
            {
                Set(CurrentLevel + value);
            }
            public void Set(int value)
            {
                CurrentLevel = Calc.Clamp(value, 0, Levels);
            }
            public void Render(Vector2 position, bool selected)
            {
                int totalHeight = LevelHeight * Levels;
                Draw.Rect(position + Offset - Vector2.One, Width + 2, totalHeight + 2, Color.Lerp(Color.Black, Color.Red, timer / flashTimer));
                if (selected)
                {
                    Draw.HollowRect(position + Offset - Vector2.One * 2, Width + 4, totalHeight + 4, Color.White);
                }
                float offset = totalHeight - LevelHeight;
                for (int i = 0; i < Levels; i++)
                {
                    Color c = i < CurrentLevel ? Color : Color.Gray;
                    Vector2 p = position + Vector2.UnitY * offset;
                    Draw.Rect(p, Width, LevelHeight - 1, c);
                    offset -= LevelHeight + 1;
                }
            }
        }
        public List<Box> Boxes = [];
        public Color BoxColor => Interface.NightMode ? Color.SlateBlue : Color.LightGray;
        public Vector2 ScreenSpacePosition => Scene is not Level level ? Vector2.Zero : (RenderPosition - level.Camera.Position) * 6;
        public int Pad;
        public bool Selected;
        private float arrowTimer;
        private bool consumedButton;
        private string defaultText;
        public Func<char, bool> IsValidCharacter;
        public Action ValidAction, InvalidAction;
        public Rectangle Bounds;
        public int MaxLevels;
        public int BoxCount;
        public int[] Code;
        public int Width;
        public int Height;
        public int SelectedBox;
        public int BoxWidth;
        public SegmentBox(Window window, int[] code, int pad, int boxWidth, int levelHeight, Action ifValid = null, Action ifInvalid = null)
            : base(window)
        {
            BoxWidth = boxWidth;
            ValidAction = ifValid;
            InvalidAction = ifInvalid;
            Pad = pad;
            MaxLevels = code.Max();
            BoxCount = code.Length;
            Code = code;
            Width = boxWidth * code.Length;
            Height = levelHeight * MaxLevels;
            for (int i = 0; i < code.Length; i++)
            {
                Boxes.Add(new Box(Vector2.UnitX * (boxWidth + pad) * i, MaxLevels, levelHeight, boxWidth, Color.White));
            }
        }
        public override void OnOpened(Scene scene)
        {
            base.OnOpened(scene);
            Position = new Vector2(Window.CaseWidth / 2, Window.CaseHeight / 2) - new Vector2(Width / 2, Height / 2);
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
            if (Selected)
            {
                MInput.Disabled = false;
                MInput.Update();
                if (MInput.Keyboard.Pressed(Keys.Delete))
                {
                    consumedButton = true;
                    MInput.UpdateNull();
                }
                else if (arrowTimer <= 0)
                {
                    bool detected = false;
                    if (MInput.Keyboard.Check(Keys.Left) || Input.MoveX.Value < 0)
                    {
                        SelectedBox = Math.Max(0, SelectedBox - 1);
                        detected = true;
                    }
                    else if (MInput.Keyboard.Check(Keys.Right) || Input.MoveX.Value > 0)
                    {
                        SelectedBox = Math.Min(SelectedBox + 1, BoxCount - 1);
                        detected = true;
                    }
                    else if (MInput.Keyboard.Check(Keys.Up) || Input.MoveY.Value < 0)
                    {
                        detected = true;
                        Boxes[SelectedBox].Add(1);
                    }
                    else if (MInput.Keyboard.Check(Keys.Down) || Input.MoveY.Value > 0)
                    {
                        detected = true;
                        Boxes[SelectedBox].Add(-1);
                    }
                    if (detected)
                    {
                        arrowTimer = 0.2f;
                        consumedButton = true;
                        MInput.UpdateNull();
                    }
                }
                MInput.Disabled = consumedButton;
            }
            consumedButton = false;
            base.Update();
            foreach(Box b in Boxes)
            {
                b.Update();
            }
            Vector2 p = RenderPosition;
            if (Interface.LeftPressed)
            {
                Selected = false;
                for (int i = 0; i < BoxCount; i++)
                {
                    if (Interface.MouseOver(p + (Vector2.UnitX * BoxWidth * i), BoxWidth, Height))
                    {
                        Selected = true;
                        SelectedBox = i;
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
            switch (c)
            {
                case '\r':
                    Engine.Scene.OnEndOfFrame += delegate
                    {
                        for (int i = 0; i < Code.Length; i++)
                        {
                            if (Boxes[i].CurrentLevel != Code[i])
                            {
                                InvalidAction?.Invoke();
                                break;
                            }
                        }
                        ValidAction?.Invoke();
                        foreach (Box b in Boxes)
                        {
                            b.Flash();
                        }
                    };
                    break;
                case '\b':
                    Boxes[SelectedBox].Set(0);
                    SelectedBox = Math.Max(SelectedBox - 1, 0);
                    break;
                case ' ':
                    SelectedBox = Math.Min(SelectedBox + 1, BoxCount - 1);
                    break;
                default:
                    {
                        break;
                    }
            }
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
            Vector2 p = RenderPosition;
            for (int i = 0; i < Boxes.Count; i++)
            {
                Boxes[i].Render(p, i == SelectedBox);
            }
        }
        public override void Removed(Entity entity)
        {
            base.Removed(entity);

            if (AddedToOnInput)
            {
                TextInput.OnInput -= OnTextInput;
                AddedToOnInput = false;
            }
            MInput.Disabled = false;
        }
    }
}