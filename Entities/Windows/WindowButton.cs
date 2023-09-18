using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Windows
{
    [CustomEntity("PuzzleIslandHelper/WindowButton")]
    [Tracked]

    public class WindowButton : Entity
    {
        private static readonly Dictionary<string, List<string>> fontPaths;
        private Level l;
        private Sprite sprite;
        static WindowButton()
        {
            // Fonts.paths is private static and never instantiated besides in the static constructor, so we only need to get the reference to it once.
            fontPaths = (Dictionary<string, List<string>>)typeof(Fonts).GetField("paths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }
        public static float ButtonWidth = 30;
        public static float ButtonHeight = 30;
        private static string fontName = "Tahoma Regular font";
        public ButtonType Type = ButtonType.Quit;
        public ButtonText BT;
        public bool Drawing = false;
        public static float Size = 40f;
        public bool Waiting = false;
        public Vector2 WindowPosition;
        public static List<ButtonType> Buttons = new();
        public static List<WindowButton> CustomButtons = new();
        public Action OnClicked;
        public Vector2 Scale;
        public enum ButtonType
        {
            Start,
            Quit,
            Ok,
            Custom
        }
        public void Clicked()
        {
            if (Scene == null)
            {
                return;
            }
            if (OnClicked != null)
            {
                OnClicked();
            }
        }
        public WindowButton(Vector2 position, string Text, Action OnClicked, Vector2 Scale)
            : this(ButtonType.Custom, position, Scale, Text)
        {
            this.OnClicked = OnClicked;
        }
        public WindowButton(ButtonType type, Vector2 position, Vector2 Scale, string Text = null)
        {
            if (Scale != Vector2.Zero)
            {
                this.Scale = Scale;
            }
            else
            {
                this.Scale = Vector2.One;
            }

            Type = type;
            Depth = Interface.BaseDepth - 2;
            if (Scale.X > 1 && Scale.Y > 1)
            {
                BT = new ButtonText(Text, Scale);
            }
            else
            {
                BT = new ButtonText(type, Scale, Text);
            }
            Position = ToInt(position);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(BT);
            Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/interface/icons/"));
            sprite.AddLoop("idle", "button", 1f);
            sprite.AddLoop("pressed", "buttonPressed", 1f);
            sprite.Play("idle");
            sprite.Scale = Scale;
            Collider = new Hitbox(sprite.Width * Scale.X, sprite.Height * Scale.Y);
            WindowPosition = new Vector2(Window.CaseWidth / 2, Window.CaseHeight);
        }
        public override void Update()
        {
            base.Update();
            //CloseWindow = true;
            Position = ToInt(Position);
            if (CollideCheck<Interface>() && Interface.LeftClicked)
            {
                Waiting = true;
                sprite.Play("pressed");
            }
            else
            {
                sprite.Play("idle");
            }
        }
        private Vector2 ToInt(Vector2 vector)
        {
            return new Vector2((int)vector.X, (int)vector.Y);
        }
        public Vector2 ButtonDrawPosition()
        {
            return ToInt(ToInt(Window.DrawPosition) + WindowPosition - new Vector2(ButtonWidth / 3, ButtonHeight / 4) - Vector2.One);
        }

        public class ButtonText : Entity
        {
            private Level l;
            public string Text;
            public Vector2 Scale;
            public bool IsDestruct;
            public ButtonText(string text, Vector2 Scale)
            : this(ButtonType.Custom, Scale, text)
            {
                IsDestruct = true;
            }
            public ButtonText(ButtonType type, Vector2 Scale, string Text = null)
            {
                Tag = TagsExt.SubHUD;
                if (string.IsNullOrEmpty(Text))
                {
                    this.Text = type.ToString();
                }
                else
                {
                    this.Text = Text;
                }

                this.Scale = Scale;
            }
            public override void Render()
            {
                base.Render();
                if (Scene as Level is null || !Window.Drawing)
                {
                    return;
                }
                l = Scene as Level;
                if (Window.Drawing)
                {
                    if (IsDestruct)
                    {
                        for (int i = 0; i < Window.ButtonsUsed.Count; i++)
                        {
                            Fonts.Get(fontName).Draw(Size, Text, AbsoluteDrawPosition(i), Vector2.Zero, Scale, Color.Black);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Window.ButtonsUsed.Count; i++)
                        {
                            Fonts.Get(fontName).Draw(Size, Window.ButtonsUsed[i].Type.ToString(), AbsoluteDrawPosition(i), Vector2.Zero, Scale, Color.Black);
                        }
                    }
                }
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                ensureCustomFontIsLoaded();
            }
            private Vector2 ToInt(Vector2 vector)
            {
                return new Vector2((int)vector.X, (int)vector.Y);
            }
            private void ensureCustomFontIsLoaded()
            {
                if (Fonts.Get(fontName) == null)
                {
                    // this is a font we need to load for the cutscene specifically!
                    if (!fontPaths.ContainsKey(fontName))
                    {
                        // the font isn't in the list... so we need to list fonts again first.
                        Logger.Log(LogLevel.Warn, "PuzzleIslandHelper/EscapeTimer", $"We need to list fonts again, {fontName} does not exist!");
                        Fonts.Prepare();
                    }

                    Fonts.Load(fontName);
                    Engine.Scene.Add(new FontHolderEntity());
                }
            }
            public Vector2 AbsoluteDrawPosition(int i, bool destruct = false)
            {
                Vector2 vec1 = ToInt(l.Camera.CameraToScreen(Window.ButtonsUsed[i].Position)) * 6;
                if (destruct)
                {
                    vec1 = ToInt(l.Camera.CameraToScreen(Destruct.Buttons[i].Position)) * 6;
                }
                Vector2 adjust = ToInt(new Vector2(ButtonWidth / 2, 6));
                return vec1 + adjust;
            }
        }

        // a small entity that just ensures the font loaded by the timer unloads upon leaving the map.
        private class FontHolderEntity : Entity
        {
            public FontHolderEntity()
            {
                Tag = Tags.Global;
            }

            public override void SceneEnd(Scene scene)
            {
                base.SceneEnd(scene);
                Fonts.Unload(fontName);
            }
        }

    }
}