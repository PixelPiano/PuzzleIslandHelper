using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Windows.WindowButton;
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
        public float Size = 40f;
        public bool Waiting = false;
        public Vector2 WindowPosition;

        public Action OnClicked;
        public Vector2 Scale;
        public string Text;

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
        public WindowButton(Vector2 position, string Text, Action OnClicked, Vector2 Scale, float textSize)
            : this(ButtonType.Custom, position, Scale, textSize, Text)
        {
            this.OnClicked = OnClicked;
        }

        public WindowButton(ButtonType type, Vector2 position, Vector2 Scale, float textSize = -1, string Text = null)
        {
            this.Scale = Scale != Vector2.Zero ? Scale : Vector2.One;

            Type = type;
            if (textSize != -1)
            {
                Size = textSize;
            }
            Depth = Interface.BaseDepth - 2;
            BT = new ButtonText(this, type, Size, Scale, Text);
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
            bool collidedWithMouse = CollideCheck<Interface>();
            if (collidedWithMouse && Interface.LeftClicked)
            {
                Waiting = true;
                sprite.Play("pressed");
            }
            else
            {
                if (Waiting && collidedWithMouse)
                {
                    OnClicked.Invoke();
                }
                Waiting = false;
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
            public string Text;
            public Vector2 Scale;
            public WindowButton Button;
            public float Size;
            public ButtonText(WindowButton button, string text, float textSize, Vector2 Scale)
            : this(button, ButtonType.Custom, textSize, Scale, text)
            {
            }
            public ButtonText(WindowButton button, ButtonType type, float textSize, Vector2 Scale, string Text = null)
            {
                Size = textSize;
                Button = button;
                Tag = TagsExt.SubHUD;
                this.Text = string.IsNullOrEmpty(Text) ? type.ToString() : Text;
                this.Scale = Scale;
            }
            public override void Render()
            {
                base.Render();
                if (Scene is not Level level || !Window.Drawing)
                {
                    return;
                }

                Center = (level.Camera.CameraToScreen(Button.Center) - new Vector2(Button.Width / 2, Button.Height / 2) + new Vector2(2, 1)) * 6;
                Fonts.Get(fontName).Draw(Size, Text, Position, Vector2.Zero, Scale, Color.Black);

            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                ensureCustomFontIsLoaded();
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
            public override void Update()
            {
                base.Update();
            }
            public Vector2 AbsoluteDrawPosition(Level level)
            {
                Vector2 vec1 = level.Camera.CameraToScreen(Button.Position) * 6;
                return (vec1 + Vector2.One * 6).ToInt();
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