using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.Prologue
{
    [Tracked]
    public class OnScreenText : Entity
    {
        private const int MaxLineWidth = 1920 / 2;
        private const string fontName = "pixelary";
        private float TextOpacity = 1;

        private static readonly Dictionary<string, List<string>> fontPaths;
        static OnScreenText()
        {
            fontPaths = (Dictionary<string, List<string>>)typeof(Fonts).GetField("paths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }
        private string TextID;
        private float LineSpace;
        private float _timer;
        private const float _timerLimit = 0.6f;
        private bool Waiting;
        public bool InCutscene = true;
        private bool _visible;
        private bool _forceHide;
        private Vector2 _position;
        private FancyTextExt.Text FText;
        private int CurrentNode;
        private Vector2 TextPosition;
        private Color TextColor;
        public bool RemoveAfterScrolling;
        public bool FadeOutOnEnd;
        public OnScreenText(Vector2 position, Color color, string textID, bool fadeOutOnEnd = true, bool removeAfterScrolling = true) : base(Vector2.Zero)
        {
            Tag |= TagsExt.SubHUD;
            Depth = -1000001;
            TextID = textID;
            TextPosition = position;
            TextColor = color;
            RemoveAfterScrolling = removeAfterScrolling;
            FadeOutOnEnd = fadeOutOnEnd;
        }
        private void LoadText(int maxLineWidth, int linesPerPage, Vector2 offset)
        {
            FText = FancyTextExt.Parse(Dialog.Get(TextID), maxLineWidth, linesPerPage, offset);

        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            _forceHide = true;
            LoadText(MaxLineWidth, 16, Vector2.UnitX * MaxLineWidth);
            LineSpace = FText.BaseSize;
            Add(new Coroutine(ScrollThroughText()));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            //ensureCustomFontIsLoaded();
        }
        private void ensureCustomFontIsLoaded()
        {
            if (Fonts.Get(fontName) == null)
            {
                // this is a font we need to load for the cutscene specifically!
                if (!fontPaths.ContainsKey(fontName))
                {
                    // the font isn't in the list... so we need to list fonts again first.
                    Logger.Log(LogLevel.Warn, "PuzzleIslandHelper/MemoryTextscene", $"We need to list fonts again, {fontName} does not exist!");
                    Fonts.Prepare();
                }

                Fonts.Load(fontName);
                Engine.Scene.Add(new FontHolderEntity());
            }
        }

        private IEnumerator ScrollThroughText()
        {
            _forceHide = false;
            _timer = 0;
            _visible = true;
            for (int i = 0; i < FText.Nodes.Count; i++)
            {
                float _ypos = LineSpace - FText.BaseSize / 8;

                FancyTextExt.Node Node = FText.Nodes[i];
                CurrentNode = i + 1;
                if (Node is FancyTextExt.Char c)
                {
                    if (c.Character != ' ')
                    {
                        PixelFontSize size = FText.Font.Get(FText.BaseSize);
                        PixelFontCharacter ch = size.Get(c.Character);
                        _position.X = c.Position + c.Offset.X + ch.XOffset + ch.XAdvance;
                        _position.Y = _ypos + c.Offset.Y;
                    }
                    yield return c.Delay * 1.5f;
                }
            }
            if (FadeOutOnEnd)
            {
                for (float i = 0; i < 1f; i += Engine.DeltaTime)
                {
                    TextOpacity = Calc.LerpClamp(1, 0, i);
                    yield return null;
                }
            }
            if (RemoveAfterScrolling)
            {
                RemoveSelf();
            }
        }
        public override void Update()
        {
            base.Update();
            if (Waiting)
            {
                if (_timer >= _timerLimit)
                {
                    _visible = !_visible;
                    _timer = 0;
                }
                _timer += Engine.DeltaTime;
            }
            else
            {
                _visible = true;
            }

        }

        #region Rendering
        private void DrawUnderscore(PixelFont font, float baseSize, Vector2 position, Vector2 scale, float alpha, Color color)
        {
            Vector2 vector = scale;
            PixelFontSize pixelFontSize = font.Get(baseSize * Math.Max(vector.X, vector.Y));
            PixelFontCharacter pixelFontCharacter = pixelFontSize.Get('_');
            vector *= baseSize / pixelFontSize.Size;
            position += TextPosition;
            Vector2 zero = Vector2.Zero;
            zero.X += pixelFontCharacter.XOffset;
            pixelFontCharacter.Texture.Draw(position + zero * vector, Vector2.Zero, color * alpha, vector);
        }
        public override void Render()
        {
            if (Scene is Level)
            {
                FText.Draw(TextPosition, Vector2.Zero, Vector2.One * SceneAs<Level>().Zoom, 1, TextColor * TextOpacity, 0, CurrentNode);
                if (_visible && !_forceHide)
                {
                    DrawUnderscore(FText.Font, FText.BaseSize, _position, Vector2.One, TextOpacity, TextColor * TextOpacity);
                }
            }
            base.Render();
        }
        #endregion
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

