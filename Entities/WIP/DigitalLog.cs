using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

// PuzzleIslandHelper.DigitalLog
namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/DigitalLog")]
    public class DigitalLog : Entity
    {
        private Sprite sprite;
        private string DialogID;
        public DigitalLog(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Depth = 1;
            DialogID = data.Attr("dialogID", "TestDialogue");
            Collider = new Hitbox(16, 16, 0, 0);
            Add(new TalkComponent(new Rectangle(0, 0, (int)Collider.Width, (int)Collider.Height), Vector2.Zero, Interact));

        }
        private void Interact(Player player)
        {
            player.StateMachine.State = 11;
            SceneAs<Level>().Add(new LogDisplay(DialogID));
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);

        }
    }
    public class LogDisplay : Entity
    {
        private string Dialogue;
        private Vector2 DrawPosition;
        private Vector2 Area;
        private Sprite Page;
        private Sprite Background;
        private float Opacity;
        private float PageOpacity;
        private const string fontName = "alarm clock";
        private static readonly Dictionary<string, List<string>> fontPaths;
        private FancyText.Text activeText;
        private bool Closing;
        private Color PageColor;
        private bool Fading;
        private float[] randomScale = new float[4];
        private BackgroundText MovingText;
        static LogDisplay()
        {
            // Fonts.paths is private static and never instantiated besides in the static constructor, so we only need to get the reference to it once.
            fontPaths = (Dictionary<string, List<string>>)typeof(Fonts).GetField("paths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }
        public LogDisplay(string DialogID)
        {
            Tag |= TagsExt.SubHUD;
            PageColor = Color.Lerp(Color.Green, Color.LightGreen, 0.4f);
            Page = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/logDisplay/");
            Background = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/logDisplay/");
            Page.AddLoop("idle", "page", 1f);
            Page.Color = PageColor * 0;
            Background.AddLoop("idle", "blackBackground", 1f);
            Add(Background);
            Add(Page);
            Background.Color = Color.White * 0;
            Background.Play("idle");
            Page.Play("idle");
            Dialogue = DialogID;
            Area = new Vector2(738, 936);
            DrawPosition = new Vector2(630, 1080 / 20 * 2.5f);
            Opacity = 0;
        }
        private IEnumerator Fade(bool state)
        {
            Fading = true;
            for (int i = 0; i < 2; i++)
            {
                PageOpacity = state ? 0.5f : 0;
                yield return 0.04f;
                PageOpacity = state ? 0 : 0.5f;
                yield return 0.04f;
            }
            PageOpacity = state ? 1 : 0;
            Fading = false;
            if (!state)
            {
                RemoveSelf();
            }
        }
        public override void Render()
        {
            MovingText.RenderText();
            base.Render();
            float scale = 0.8f;
            Color textColor = Color.White;
            activeText = FancyText.Parse(Dialog.Get(Dialogue), (int)(Area.X * 2 - Area.X * scale), 14, 1, textColor);
            activeText.Font = ActiveFont.Font;
            activeText.Draw(DrawPosition, Vector2.Zero, Vector2.One * scale, PageOpacity);
        }
        public override void Update()
        {
            base.Update();
            BackgroundText.Opacity = Opacity * 0.4f;
            if (Opacity == 1 && Input.MenuConfirm.Pressed)
            {
                Closing = true;
            }
            if (Opacity == 0 && Closing && !Fading)
            {
                SceneAs<Level>().Tracker.GetEntity<Player>().StateMachine.State = 0;
                Add(new Coroutine(Fade(false)));
            }
            if (!Closing)
            {
                Opacity = Opacity < 1 ? Opacity + Engine.DeltaTime * 2 : 1;
            }
            else
            {
                Opacity = Opacity > 0 ? Opacity - Engine.DeltaTime * 2 : 0;
            }
            Page.Color = PageColor * PageOpacity;
            Background.Color = Color.White * (Opacity / 2);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            for (int i = 0; i < randomScale.Length; i++)
            {
                randomScale[i] = Calc.Random.Range(0.5f, 1f);
            }
            scene.Add(MovingText = new BackgroundText(Dialogue, 1, Color.Green));
            Add(new Coroutine(Fade(true)));
            ensureCustomFontIsLoaded();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(MovingText);
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
        private class BackgroundText : Entity
        {
            private FancyText.Text[] Texts = new FancyText.Text[5];
            private Vector2[] TextPositions = new Vector2[5];
            public static float Opacity;
            private float Scale;
            private float TextWidth;
            public BackgroundText(string Dialogue, float scale, Color Color)
            {

                Tag |= TagsExt.SubHUD;
                Scale = scale;
                TextWidth = 1920 / (Texts.Length - 1);
                for (int i = 0; i < Texts.Length; i++)
                {
                    Texts[i] = FancyText.Parse(Dialog.Get(Dialogue), (int)TextWidth, 20, 1, Color);
                    TextPositions[i] = new Vector2((i - 1) * (TextWidth * Scale), 0);
                }
            }
            public void RenderText()
            {
                base.Render();

                int subtract = Scale == 1 ? 0 : 2;
                for (int i = 0; i < Texts.Length - subtract; i++)
                {
                    Texts[i].Draw(TextPositions[i], Vector2.Zero, Vector2.One * Scale, Opacity);
                }
            }
            public override void Update()
            {
                base.Update();

                for (int i = 0; i < TextPositions.Length; i++)
                {
                    TextPositions[i].X += 5;
                    if (TextPositions[i].X >= 1920)
                    {
                        TextPositions[i].X = -(TextWidth * Scale);
                    }

                    Opacity = Opacity < 1 ? Opacity + Engine.DeltaTime : 1;
                }

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
    }
}