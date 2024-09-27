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
        public static bool Selected;
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
        public InputBox(Window window, Func<string, bool> onSubmit = null) : base(window)
        {
            this.onSubmit = onSubmit;

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
            if (Collider is null)
            {
                return;
            }
            Collider.Position = RenderPosition.Floor();
            Collider.Width = Width;
            Collider.Height = Height;

            bool collidingWithMouse = Interface.MouseOver(Collider);


            if (collidingWithMouse && Interface.LeftPressed)
            {
                Selected = true;
            }
            else if (Interface.LeftPressed)
            {
                Selected = false;
            }
        }
        public void DoSomething()
        {

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
                        Helper.Text = Helper.Text.Substring(0, Helper.Text.Length - 1);
                    }
                    else if (Input.MenuCancel.Pressed)
                    {
                        Deselect();
                    }
                    break;
                case ' ':
                    if (Helper.Text.Length > 0 && ActiveFont.Measure(Helper.Text + c + "_").X < Width * 6)
                    {
                        Helper.Text += c;
                    }
                    break;
                default:
                    {
                        if (!char.IsControl(c) && ActiveFont.FontSize.Characters.ContainsKey(c) && ActiveFont.Measure(Helper.Text + c + "_").X < Width * 6)
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
            Selected = false;
            Helper.RemoveSelf();

            if (AddedToOnInput)
            {
                TextInput.OnInput -= OnTextInput;
                AddedToOnInput = false;
            }
            MInput.Disabled = false;
        }
        [OnLoad]
        public static void Load()
        {
            Selected = false;
            IL.Monocle.MInput.Update += MInput_Update;
        }
        [OnUnload]
        public static void Unload()
        {
            Selected = false;
            IL.Monocle.MInput.Update -= MInput_Update;
        }

        private static void MInput_Update(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(
                    MoveType.After,
                    instr => instr.MatchCall<Engine>("get_Commands"),
                    instr => instr.MatchLdfld<Monocle.Commands>("Open")
                    ))
            {
                cursor.EmitDelegate(DisableHotkeys);
                cursor.Emit(OpCodes.Or);
            }
        }
        private static bool DisableHotkeys()
        {
            return Selected;
        }

        [Tracked]
        public class InputBoxText : TextHelper
        {
            public InputBox Track;
            private float timer;
            private bool showUnderscore;
            public Interface Interface;
            public InputBoxText(Interface inter, InputBox track) : base(Color.White)
            {
                Interface = inter;
                Track = track;
            }
            public override void Update()
            {
                base.Update();

                Visible = Track.Visible;
                if (Scene is not Level level)
                {
                    return;
                }
                if (Selected)
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
                DrawOutline(Track.ScreenSpacePosition + new Vector2(Track.Width * 3 - x / 2, Track.Height * 3), Text + (showUnderscore ? "_" : ""));
            }
        }
    }
}