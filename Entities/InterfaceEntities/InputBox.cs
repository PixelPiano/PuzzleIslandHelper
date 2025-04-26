using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using TAS.EverestInterop;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [Tracked]
    public class InputBox : WindowComponent
    {
        public InputBoxText Helper;
        public Color BoxColor
        {
            get
            {
                if (Interface.NightMode)
                {
                    return Color.SlateBlue;
                }
                else
                {
                    return Color.LightGray;
                }
            }
        }
        public Collider Collider;
        public int Width = 120;
        public int Height = 14;
        public bool Selected;
        private bool consumedButton;
        private Func<string, bool> onSubmit;
        public Vector2 ScreenSpacePosition
        {
            get
            {
                if (Scene is not Level level)
                {
                    return Vector2.Zero;
                }
                Vector2 cam = level.Camera.Position;
                Vector2 box = RenderPosition;
                return (box - cam) * 6;
            }
        }
        public InputBox(Window window, Func<string, bool> onSubmit = null, int width = 120) : base(window)
        {
            this.onSubmit = onSubmit;
            Width = width;
        }
        public InputBox(Window window, int maxChars, Func<string, bool> onSubmit = null) : base(window)
        {
            this.onSubmit = onSubmit;
            string measure = "";
            for (int i = 0; i < maxChars; i++)
            {
                measure += "_";
            }
            Width = (int)(ActiveFont.Measure(measure).X / 6);
        }
        public override void OnOpened(Scene scene)
        {
            base.OnOpened(scene);
            Position = new Vector2(Window.CaseWidth / 2, Window.CaseHeight / 2) - Collider.HalfSize;
        }
        public void ClearText()
        {
            Helper.Text = "";
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
                MInput.Disabled = consumedButton;
            }
            consumedButton = false;
            base.Update();
            if (Collider is null) return;
            Collider.Position = RenderPosition.Floor();
            Collider.Width = Width;
            Collider.Height = Height;

            bool collidingWithMouse = Interface.MouseOver(Collider);
            if (Interface.LeftPressed)
            {
                Selected = collidingWithMouse;
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
            Scene.Add(Helper = new InputBoxText(Interface, this));
            if (!AddedToOnInput)
            {
                TextInput.OnInput += OnTextInput;
                AddedToOnInput = true;
            }
            Collider = new Hitbox(Width, Height, RenderPosition.X, RenderPosition.Y);
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
                        if (string.IsNullOrEmpty(Helper.Text))
                        {
                            return;
                        }
                        if (!onSubmit.Invoke(Helper.Text))
                        {
                            ClearText();
                        }
                    };
                    break;
                case '\b':
                    if (Helper.Text.Length > 0)
                    {
                        Helper.Text = Helper.Text[..^1];
                    }
                    else if (Input.MenuCancel.Pressed)
                    {
                        Deselect();
                    }
                    break;
                case ' ':
                    if (Helper.Text.Length > 0 && ActiveFont.Measure(Helper.Text + c + ActiveFont.Measure("_").X).X < Width * 6)
                    {
                        Helper.Text += c;
                    }
                    break;
                default:
                    {
                        if (!char.IsControl(c) && ActiveFont.FontSize.Characters.ContainsKey(c) && ActiveFont.Measure(Helper.Text + c + ActiveFont.Measure("_").X).X < Width * 6)
                        {
                            Helper.Text += c;
                        }
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
            Draw.Rect(Collider, (Selected && !Interface.Buffering) ? Color.Blue : BoxColor);
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
        public class InputBoxText : TextHelper
        {
            public enum PositionModes
            {
                Left,
                Center,
                Right
            }
            public PositionModes PositionMode = PositionModes.Center;
            public InputBox Track;
            private float timer;
            private bool showUnderscore;
            private float underWidth;
            public Interface Interface;
            public InputBoxText(Interface inter, InputBox track) : base(Color.White)
            {
                Interface = inter;
                Track = track;
            }
            public override void Update()
            {
                base.Update();
                underWidth = ActiveFont.Measure("_").X;
                Visible = Track.Visible;
                if (Scene is not Level level)
                {
                    return;
                }
                if (Track.Selected)
                {
                    if (timer < 0.5f)
                    {
                        timer += Engine.DeltaTime;
                    }
                    else
                    {
                        timer = 0;
                        showUnderscore = !showUnderscore;
                    }
                }
                else
                {
                    timer = 0;
                    showUnderscore = false;
                }
            }
            public override void Render()
            {
                base.Render();
                float x = ActiveFont.Measure(Text).X;

                if (Interface.Buffering)
                {
                    showUnderscore = false;
                }
                Vector2 sc = Track.ScreenSpacePosition;
                Vector2 offset = PositionMode switch
                {
                    PositionModes.Left => Vector2.UnitY * Track.Height * 3,
                    PositionModes.Center => new Vector2(Track.Width * 3 - x / 2, Track.Height * 3),
                    PositionModes.Right => new Vector2(Track.Width * 6 - x, Track.Height * 3)
                };
                Vector2 pos = sc + offset;
                DrawOutline(pos, Text);
                if (showUnderscore)
                {
                    if (pos.X + x >= sc.X + Track.Width * 6 - underWidth && Text.Length >= 2)
                    {
                        DrawOutline(new Vector2(pos.X + x - ActiveFont.Measure(Text[^1]).X, pos.Y), "_");
                    }
                    else
                    {
                        DrawOutline(new Vector2(pos.X + x, pos.Y), "_");
                    }

                }
            }
        }
    }
}