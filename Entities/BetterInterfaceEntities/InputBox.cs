using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.IO;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities
{
    [Tracked]
    public class InputBox : Component
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
        public Vector2 Position;
        public Collider Collider;
        public int Width = 120;
        public int Height = 14;
        public bool Selected;
        private bool consumedButton;
        private Func<string, bool> onSubmit;
        public Interface Interface;
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
        public Vector2 RenderPosition
        {
            get
            {
                if (Entity is null)
                {
                    return Vector2.Zero;
                }
                return Entity.Position + Position - new Vector2(Width / 2, Height / 2);
            }
        }
        public InputBox(Interface inter, float x, float y, Func<string, bool> onSubmit = null) : base(true, true)
        {
            Interface = inter;
            this.onSubmit = onSubmit;
            Position = new Vector2(x, y);

        }
        public void ClearText()
        {
            Helper.Text = "";
        }
        public override void Update()
        {
            Visible = BetterWindow.Drawing;
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
            Collider.Position = RenderPosition.ToInt();
            Collider.Width = Width;
            Collider.Height = Height;

            bool collidingWithMouse = Interface.MouseOver(Collider);


            if (collidingWithMouse && Interface.LeftClicked)
            {
                Selected = true;
            }
            else if (Interface.LeftClicked)
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
                    goto IL_0157;
                case '\b':
                    if (Helper.Text.Length > 0)
                    {
                        Helper.Text = Helper.Text.Substring(0, Helper.Text.Length - 1);
                    }
                    else
                    {
                        if (!Input.MenuCancel.Pressed)
                        {
                            break;
                        }

                        Deselect();
                    }
                    goto IL_0157;
                case ' ':
                    if (Helper.Text.Length > 0 && ActiveFont.Measure(Helper.Text + c + "_").X < Width * 6)
                    {
                        Helper.Text += c;
                    }
                    goto IL_0157;
                default:
                    {
                        if (!char.IsControl(c))
                        {
                            if (ActiveFont.FontSize.Characters.ContainsKey(c))
                            {
                                if (ActiveFont.Measure(Helper.Text + c + "_").X < Width * 6)
                                {
                                    Helper.Text += c;
                                }
                                goto IL_0157;
                            }
                            break;
                        }
                        break;
                    }
                IL_0157:
                    consumedButton = true;
                    MInput.Disabled = true;
                    MInput.UpdateNull();
                    break;
            }
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
                DrawOutline(Track.ScreenSpacePosition + new Vector2(Track.Width * 3 - x / 2, Track.Height * 3), Text + (showUnderscore ? "_" : ""));
            }
        }
    }
}