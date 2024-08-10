
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Reflection;
using static Celeste.Mod.PuzzleIslandHelper.Entities.FancyTextExt;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    public class TextWindow : Entity
    {
        private static readonly Dictionary<string, List<string>> fontPaths;
        public FancyTextExt.Text activeText;
        private static string fontName = "alarm clock";
        public bool Drawing = false;
        public string CurrentID = "";
        private string LastUsedID;
        static TextWindow()
        {
            // Fonts.paths is private static and never instantiated besides in the static constructor, so we only need to get the reference to it once.
            fontPaths = (Dictionary<string, List<string>>)typeof(Fonts).GetField("paths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }
        public List<Node> Nodes => activeText.Nodes;

        public Vector2 TextPosition;
        public int TextWidth = 0;
        public float textScale = 0.7f;
        public Interface Interface;
        public Window Window;
        public TextWindow(Interface inter, string dialog)
        {
            Tag |= TagsExt.SubHUD | Tags.TransitionUpdate;
            Interface = inter;
            Window = inter.Window;
            Depth = Interface.BaseDepth - 5;
        }
        public void Initialize(string dialog)
        {
            ChangeCurrentID(dialog, true, true);
        }
        public void ChangeCurrentID(string text, bool dialog = true, bool forceChange = false)
        {
            if (LastUsedID == text && !forceChange)
            {
                return;
            }
            activeText = Parse(dialog ? Dialog.Get(text) : text, (int)Window.CaseWidth * 8, 15, Vector2.Zero, 1);
            LastUsedID = text;
            CurrentID = text;
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level || Interface.ForceHide)
            {
                return;
            }

            //if the text is being drawn
            if (Drawing && !string.IsNullOrEmpty(CurrentID) && activeText != null)
            {
                activeText.Font ??= ActiveFont.Font;
                activeText?.Draw((level.Camera.CameraToScreen(TextPosition)).Floor() * 6, Vector2.Zero, Vector2.One * textScale,1f, Interface.NightMode ? Color.White * Window.Alpha : Color.Black * Window.Alpha);

            }
        }
        public override void Update()
        {

            base.Update();
            if (!Window.Drawing)
            {
                Drawing = false;
                return;
            }
            if (LastUsedID != CurrentID)
            {
                ChangeCurrentID(CurrentID);
            }
        }
        #region Finished
        private Vector2 ToInt(Vector2 vector)
        {
            return new Vector2((int)vector.X, (int)vector.Y);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Depth = Interface.BaseDepth - 5;
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

        // a small entity that just ensures the font loaded by the scaleTimer unloads upon leaving the map.
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
        #endregion
    }

}
