using Celeste.Mod.PuzzleIslandHelper.Entities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities
{
    public class TextWindow : Entity
    {
        #region Variables
        private bool SwitchAccess
        {
            get
            {
                if(CurrentID is null)
                {
                    return false;
                }
                return CurrentID.ToUpper() == "ACCESS" || CurrentID.ToUpper() == "ACCESSDENIED";
            }
        }
        private static readonly Dictionary<string, List<string>> fontPaths;
        public FancyTextExt.Text activeText;
        private static string fontName = "alarm clock";
        public static bool Drawing = false;
        public static string CurrentID = "";
        private string LastUsedID;
        static TextWindow()
        {
            // Fonts.paths is private static and never instantiated besides in the static constructor, so we only need to get the reference to it once.
            fontPaths = (Dictionary<string, List<string>>)typeof(Fonts).GetField("paths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }

        private Level l;
        public List<FancyTextExt.Node> Nodes => activeText.Nodes;

        public static Vector2 TextPosition;
        public static int TextWidth = 0;
        public float textScale = 0.7f;
        #endregion
        public TextWindow(string dialog)
        {
            Tag |= TagsExt.SubHUD | Tags.TransitionUpdate;
            Depth = Interface.BaseDepth - 5;
            ChangeCurrentID(dialog);
        }
        public void ChangeCurrentID(string dialog)
        {
            if (LastUsedID == dialog)
            {
                return;
            }
            activeText = FancyTextExt.Parse(Dialog.Get(dialog), (int)BetterWindow.CaseWidth * 8, 15, Vector2.Zero, 1);
            LastUsedID = dialog;
            CurrentID = dialog;
        }
        public override void Render()
        {
            if (SwitchAccess)
            {
                CurrentID = !LoadSequence.HasArtifact && Interface.Loading ? "ACCESSDENIED" : "ACCESS";
            }
            base.Render();
            if (Scene as Level == null)
            {
                return;
            }
            l = Scene as Level;

            //if the text is being drawn
            if (Drawing)
            {
                if (SwitchAccess && LoadSequence.HasArtifact && Interface.Loading)
                {
                    return;
                }
                activeText.Font ??= ActiveFont.Font;
                activeText?.Draw(ToInt(l.Camera.CameraToScreen(TextPosition)) * 6, Vector2.Zero, Vector2.One * textScale, 1f, Interface.NightMode ? Color.White : Color.Black);

            }
        }
        public override void Update()
        {

            base.Update();
            if (!BetterWindow.Drawing)
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
        #endregion
    }

}
