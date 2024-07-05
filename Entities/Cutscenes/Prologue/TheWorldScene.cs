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
    public class TheWorldScene : Entity
    {
        private const int XOffset = 1920 / 16;
        private const int MaxLineWidth = 1920 / 2 - XOffset;
        private const string fontName = "pixelary";
        private int isThatXOffset;
        private float TextOpacity = 1;

        private static readonly Dictionary<string, List<string>> fontPaths;
        static TheWorldScene()
        {
            fontPaths = (Dictionary<string, List<string>>)typeof(Fonts).GetField("paths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }

        private int CurrentIsThatNode;
        private int CurrentWorldNode;
        private int CurrentID;
        private float SolidOpacity;
        private float IsThatTextOpacity = 1;
        private float WorldTextOpacity = 1;
        private float LineSpace;
        private float _timer;
        private const float _timerLimit = 0.6f;


        private string[] DialogIDs;

        private bool Waiting;
        public bool InCutscene = true;
        private bool _visible;
        private bool _forceHide;
        private Vector2 _position;
        private FancyTextExt.Text FText;
        private FancyTextExt.Text WorldText;
        private FancyTextExt.Text IsThatText;
        private bool worldDrawing;
        private bool isThatDrawing;
        private bool scatterDrawing;
        private float WorldOffset = 100;
        private int CurrentNode;
        private bool preDrawing;
        private float WorldColorLerp;
        private float FadeOutLerp;
        public TheWorldScene(params string[] additionalIds) : base(Vector2.Zero)
        {
            Tag |= TagsExt.SubHUD;
            Depth = -1000001;
            DialogIDs = additionalIds;
        }
        private void LoadText(int maxLineWidth, int linesPerPage, Vector2 offset)
        {
            FText = FancyTextExt.Parse(Dialog.Get(DialogIDs[CurrentID]), maxLineWidth, linesPerPage, offset);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            _forceHide = true;
            LoadText(MaxLineWidth, 16, Vector2.UnitX * MaxLineWidth);
            IsThatText = FancyTextExt.Parse(Dialog.Get("pTF"), MaxLineWidth, 16, Vector2.UnitX * MaxLineWidth);
            WorldText = FancyTextExt.Parse(Dialog.Get("pTWorld"), MaxLineWidth, 16, Vector2.UnitX * MaxLineWidth);
            LineSpace = FText.BaseSize;
            Add(new Coroutine(Cutscene()));
        }

        private IEnumerator TypeFirstTexts()
        {
            bool startOfNewSegment = false;
            preDrawing = true;
            for (int k = 0; k < DialogIDs.Length; k++)
            {
                while (CurrentNode < FText.Nodes.Count)
                {
                    _forceHide = false;
                    _visible = true;
                    for (int i = 0; i < FText.Nodes.Count; i++)
                    {
                        if (startOfNewSegment)
                        {
                            startOfNewSegment = false;
                            _forceHide = false;
                        }
                        float _ypos = LineSpace + XOffset - FText.BaseSize / 8;

                        FancyTextExt.Node Node = FText.Nodes[i];
                        CurrentNode = i + 1;
                        if (Node is FancyTextExt.Char c)
                        {
                            if (c.Character != ' ')
                            {
                                PixelFontSize size = FText.Font.Get(FText.BaseSize);
                                PixelFontCharacter ch = size.Get(c.Character);
                                _position.X = c.Position + XOffset + c.Offset.X + ch.XOffset + ch.XAdvance;
                                _position.Y = _ypos + c.Offset.Y;
                            }
                            yield return c.Delay * 1.5f;
                        }
                        if (Node is FancyTextExt.Wait)
                        {
                            _forceHide = true;
                            yield return (Node as FancyTextExt.Wait).Duration;
                            _forceHide = false;
                        }
                    }
                }

                if (k < DialogIDs.Length - 1)
                {
                    yield return Reset(k + 1); //advance to next dialog ID
                }
                else
                {
                    yield return WaitForButton();
                }
            }
            for (float i = 0; i < 1f; i += Engine.DeltaTime)
            {
                TextOpacity = Calc.LerpClamp(1, 0, i);
                yield return null;
            }
            preDrawing = false;
        }
        private IEnumerator Reset(int next)
        {
            yield return WaitForButton();
            for (float i = 0; i < 1f; i += Engine.DeltaTime)
            {
                TextOpacity = Calc.LerpClamp(1, 0, i);
                yield return null;
            }
            CurrentNode = 0;
            FText = FancyTextExt.Parse(Dialog.Get(DialogIDs[next]), MaxLineWidth, 16, Vector2.UnitX * MaxLineWidth);
            yield return null;
            for (float i = 0; i < 1f; i += Engine.DeltaTime)
            {
                TextOpacity = Calc.LerpClamp(0, 1, i);
                yield return null;
            }
            _forceHide = true;
            yield return null;
        }
        #region Routines
        private IEnumerator TypeIsThat()
        {
            isThatDrawing = true;
            worldDrawing = false;
            CurrentIsThatNode = 0;
            CurrentWorldNode = 0;
            _forceHide = false;
            _visible = true;
            for (int i = 0; i < IsThatText.Nodes.Count; i++)
            {
                float _ypos = LineSpace + XOffset - IsThatText.BaseSize / 8;

                FancyTextExt.Node Node = IsThatText.Nodes[i];
                CurrentIsThatNode = i + 1;
                if (Node is FancyTextExt.Char c)
                {
                    if (c.Character != ' ')
                    {
                        PixelFontSize size = IsThatText.Font.Get(FText.BaseSize);
                        PixelFontCharacter ch = size.Get(c.Character);
                        _position.X = c.Position + XOffset + c.Offset.X + ch.XOffset + ch.XAdvance;
                        _position.Y = _ypos + c.Offset.Y;
                        isThatXOffset = (int)_position.X;
                    }
                    yield return c.Delay * 1.5f;
                }
                if (Node is FancyTextExt.Wait)
                {
                    _forceHide = true;
                    yield return (Node as FancyTextExt.Wait).Duration;
                    _forceHide = false;
                }
            }
        }
        private IEnumerator TypeWorld()
        {
            isThatDrawing = worldDrawing = true;
            CurrentWorldNode = 0;
            CurrentIsThatNode = IsThatText.Nodes.Count;
            _forceHide = false;
            _visible = true;
            for (int i = 0; i < WorldText.Nodes.Count; i++)
            {
                float _ypos = LineSpace + XOffset - WorldText.BaseSize / 8;

                FancyTextExt.Node Node = WorldText.Nodes[i];
                CurrentWorldNode = i + 1;
                if (Node is FancyTextExt.Char c)
                {
                    if (c.Character != ' ')
                    {
                        PixelFontSize size = WorldText.Font.Get(FText.BaseSize);
                        PixelFontCharacter ch = size.Get(c.Character);
                        _position.X = c.Position + XOffset + c.Offset.X + ch.XOffset + ch.XAdvance;
                        _position.Y = _ypos + c.Offset.Y;
                    }
                    yield return c.Delay * 1.5f;
                }
                if (Node is FancyTextExt.Wait)
                {
                    _forceHide = true;
                    yield return (Node as FancyTextExt.Wait).Duration;
                    _forceHide = false;
                }
            }
        }
        private IEnumerator Cutscene()
        {
            _visible = false;
            Level level = SceneAs<Level>();
            Player player = level.Tracker.GetEntity<Player>();
            if (player is not null)
            {
                player.StateMachine.State = Player.StDummy;
            }

            yield return 1;
            for (float i = 0; i < 1f; i += Engine.DeltaTime)
            {
                SolidOpacity = Calc.LerpClamp(0, 0.4f, i);
                yield return null;
            }
            _timer = 0;
            //start scrolling text
            yield return TypeFirstTexts();
            yield return TypeIsThat();
            yield return TypeWorld();

            yield return 1;
            for (float i = 0; i < 1f; i += Engine.DeltaTime)
            {
                IsThatTextOpacity = Calc.LerpClamp(1, 0, i);
                WorldColorLerp = i;
                yield return null;
            }
            WorldColorLerp = 1;
            IsThatTextOpacity = 0;
            yield return ScatterText();
            yield return 2;
            SolidOpacity = 1;
            preDrawing = isThatDrawing = scatterDrawing = worldDrawing = false;
            //slowly fade in eerie nothing sound atmosphere noises
            yield return 4;
            InCutscene = false;
        }

        private IEnumerator ScatterText()
        {
            scatterDrawing = true;
            bool zooming = false;
            List<OnScreenText> onScreenTexts = new();
            float buffer = 1;
            while (true)
            {
                if (!zooming && onScreenTexts.Count > 40)
                {
                    zooming = true;
                    Add(new Coroutine(WorldZoom()));
                }
                if (FadeOutLerp >= 1) break;
                OnScreenText newText = CreateRandomizedText();
                onScreenTexts.Add(newText);
                Scene.Add(newText);
                yield return buffer;
                buffer /= 1.5f;
            }
            foreach (OnScreenText text in onScreenTexts)
            {
                Scene.Remove(text);
            }
        }

        private IEnumerator WorldZoom()
        {
            float duration = 4;
            Add(new Coroutine(SceneAs<Level>().ZoomTo(new Vector2(160, 90), 1.5f, duration)));
            for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
            {
                FadeOutLerp = Ease.SineIn(i);
                yield return null;
            }
            FadeOutLerp = 1;

        }
        private OnScreenText CreateRandomizedText()
        {
            Vector2 position = PianoUtils.Random(0, Engine.ViewWidth * 2, 0, Engine.ViewHeight * 2);
            float rand = Calc.Random.Range(0.4f, 1f);
            Color color = new Color(1, 1, 1, rand);

            string text = DialogIDs[Calc.Random.Range(0, DialogIDs.Length)] + "a";
            return new OnScreenText(position, color, text, false, false);
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
        private IEnumerator WaitForButton()
        {
            Waiting = true;

            while (Waiting)
            {
                if (Input.DashPressed)
                {
                    break;
                }
                yield return null;
            }
            Waiting = false;
            yield return null;
            _forceHide = true;
        }
        #endregion

        #region Rendering
        private void DrawUnderscore(PixelFont font, float baseSize, Vector2 position, Vector2 scale, float alpha, Color color)
        {
            Vector2 vector = scale;
            PixelFontSize pixelFontSize = font.Get(baseSize * Math.Max(vector.X, vector.Y));
            PixelFontCharacter pixelFontCharacter = pixelFontSize.Get('_');
            vector *= baseSize / pixelFontSize.Size;
            position.X = _position.X;
            if (worldDrawing)
            {
                position.X += WorldOffset;
            }
            Vector2 zero = Vector2.Zero;
            zero.X += pixelFontCharacter.XOffset;
            pixelFontCharacter.Texture.Draw(position + zero * vector, Vector2.Zero, color * alpha, vector);
        }
        public override void Render()
        {
            if (Scene is Level)
            {
                Draw.Rect(0, 0, 1920, 1080, Color.Black * SolidOpacity);

                if (worldDrawing) DrawText(WorldText, Vector2.UnitX * WorldOffset, Color.Lerp(Color.White, Color.Red, WorldColorLerp), WorldTextOpacity, CurrentWorldNode);
                if (scatterDrawing)
                {
                    if (FadeOutLerp > 0) Draw.Rect(0, 0, 1920, 1080, Color.White * FadeOutLerp);
                    return;
                }
                if (preDrawing) DrawText(FText, Vector2.Zero, Color.White, TextOpacity, CurrentNode);
                if (isThatDrawing) DrawText(IsThatText, Vector2.Zero, Color.White, IsThatTextOpacity, CurrentIsThatNode);
                if (_visible && !_forceHide) DrawUnderscore(WorldText.Font, WorldText.BaseSize, _position, Vector2.One, IsThatTextOpacity, Color.White);

            }
            base.Render();
        }
        public void DrawText(FancyTextExt.Text text, Vector2 offset, Color color, float opacity, int node)
        {
            text.Draw(offset + Vector2.One * XOffset, Vector2.Zero, Vector2.One * SceneAs<Level>().Zoom, opacity, color, 0, node);
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

