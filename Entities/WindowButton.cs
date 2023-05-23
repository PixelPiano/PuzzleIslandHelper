using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
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
        public static List<ButtonType> Buttons = new List<ButtonType>();
        public enum ButtonType
        {
            Start,
            Quit,
            Ok
        }
        public WindowButton(ButtonType type, Vector2 position)
        {
            Type = type;
            Depth = Interface.BaseDepth - 2;
            BT = new ButtonText(type);
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
            Collider = new Hitbox(sprite.Width, sprite.Height);
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
            public ButtonText(ButtonType type)
            {
                Tag = TagsExt.SubHUD;
                Text = type.ToString();
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
                    for (int i = 0; i < Window.ButtonsUsed.Count; i++)
                    {
                        Fonts.Get(fontName).Draw(Size, Window.ButtonsUsed[i].Type.ToString(), AbsoluteDrawPosition(i), Vector2.Zero, Vector2.One * 1f, Color.Black);
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
            public Vector2 AbsoluteDrawPosition(int i)
            {
                Vector2 vec1 = ToInt(l.Camera.CameraToScreen(Window.ButtonsUsed[i].Position)) * 6;
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