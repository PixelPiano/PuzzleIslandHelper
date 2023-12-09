using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities
{
    [CustomEntity("PuzzleIslandHelper/IconText")]

    public class IconText : Entity
    {
        private static readonly Dictionary<string, List<string>> fontPaths;
        static IconText()
        {
            // Fonts.paths is private static and never instantiated besides in the static constructor, so we only need to get the reference to it once.
            fontPaths = (Dictionary<string, List<string>>)typeof(Fonts).GetField("paths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }
        public static float ButtonWidth;
        public static float ButtonHeight;
        private static string fontName = "alarm clock";
        public Vector2 DrawPosition;
        public float IconWidth = 0;
        public FancyText.Text ActiveText;
        public static readonly float TextScale = 0.8f;
        public static ComputerIcon CurrentIcon;
        public bool WasDrawing;
        public List<FancyText.Node> Nodes => ActiveText.Nodes;
        public IconText()
        {
            Tag = TagsExt.SubHUD;
        }
        public override void Update()
        {
            WasDrawing = BetterWindow.Drawing;
            base.Update();
            if (!WasDrawing && BetterWindow.Drawing)
            {
                ReloadText();
            }
        }
        public void ReloadText()
        {
            ActiveText = FancyText.Parse(Dialog.Get(CurrentIcon.TabText), (int)BetterWindow.WindowWidth * 10, 20);
        }
        private Vector2 ToInt(Vector2 vector)
        {
            return new Vector2((int)vector.X, (int)vector.Y);
        }
        public float WidestLine()
        {
            return ActiveText.WidestLine() / 6 * TextScale;
        }
        public override void Render()
        {
            base.Render();
            if (!BetterWindow.Drawing)
            {
                return;
            }
            if (ActiveText is null)
            {
                ReloadText();
            }
            ActiveText.Font = ActiveFont.Font;
            ActiveText.Draw(TabTextPosition(), Vector2.Zero, Vector2.One * TextScale, 1);
        }
        public Vector2 TabTextPosition()
        {
            if (Scene is not Level level)
            {
                return Vector2.Zero;
            }
            return ToInt(level.Camera.CameraToScreen(BetterWindow.DrawPosition)) * 6 +
                    ToInt(new Vector2(1, -BetterWindow.tabHeight) * 6);
        }
        private void ensureCustomFontIsLoaded()
        {
            if (Fonts.Get(fontName) == null)
            {
                // this is a font we need to load for the cutscene specifically!
                if (!fontPaths.ContainsKey(fontName))
                {
                    // the font isn't in the list... so we need to list fonts again first.
                    Logger.Log(LogLevel.Warn, "PuzzleIslandHelper/IconText", $"We need to list fonts again, {fontName} does not exist!");
                    Fonts.Prepare();
                }

                Fonts.Load(fontName);
                Engine.Scene.Add(new FontHolderEntity());
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