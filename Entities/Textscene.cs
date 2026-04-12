using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class Textscene : CutsceneEntity
    {
        private const int Offset = 120;
        private const int MaxLineWidth = 1920 / 2 - Offset;
        public Color BackgroundColor = Color.Transparent;
        public int CurrentLine = 1;
        public int CurrentSegment;
        public int CurrentNode;
        public int CurrentID;
        public float SolidOpacity;
        public float TextOpacity = 1;
        public float LineSpace;
        public float SegmentSpace;
        public float _timer;
        private const float _timerLimit = 0.6f;
        public string[] DialogIDs;
        public bool Waiting;
        public bool InCutscene = true;
        public bool _visible;
        public bool _forceHide;
        public bool DisableUnderscore;
        public Vector2 _offset;
        public ExtraFancyText.Text FText;
        public float segmentHeight;
        public float ScrollOffset => View.Y;
        public bool Reading;
        public bool SpeedUp;
        private Color _color = Color.White;
        private Color ArrowColor = Color.White;
        private float upArrowAlpha = 0;
        private float downArrowAlpha = 0;
        private float arrowOffset;
        public Rectangle View;
        public Rectangle RevealedTextBounds;
        public MTexture UpArrow => GFX.Game["objects/PuzzleIslandHelper/textsceneUp"];
        public MTexture DownArrow => GFX.Game["objects/PuzzleIslandHelper/textsceneDown"];
        public List<Func<string[], IEnumerator>> CuedRoutines = [];
        public Textscene(string dialogID, float segmentSpace = -1, float lineSpace = -1) : this(segmentSpace, lineSpace, dialogID) { }
        public Textscene(float segmentSpace, float lineSpace, params string[] dialogIDs) : this()
        {
            DialogIDs = dialogIDs;
            SegmentSpace = segmentSpace;
            LineSpace = lineSpace;
        }
        public Textscene() : base()
        {
            Tag |= TagsExt.SubHUD;
            Depth = -1000001;
            Collider = new Hitbox(4, 90);
            View = new Rectangle(0, 0, 1920, 1080);
            RevealedTextBounds = new Rectangle(Offset, Offset, 1920 - Offset * 2, 64);
        }
        public virtual void OnCue(string[] args)
        {

        }
        public void LoadText(int maxLineWidth, int linesPerPage, Vector2 offset)
        {
            FText = ExtraFancyText.Parse(Dialog.Get(DialogIDs[CurrentID]), maxLineWidth, linesPerPage, offset);
            FText.ReplaceAll<ExtraFancyText.NewPage, ExtraFancyText.NewLine>(() => new ExtraFancyText.NewLine());
            _offset = Vector2.UnitX * Offset;
            View.Y = 0;
            RevealedTextBounds.Y = Offset;
            RevealedTextBounds.Height = FText.Font.Get(FText.BaseSize).LineHeight;
        }

        public override void OnBegin(Level level)
        {
            _forceHide = true;
            LoadText(MaxLineWidth, 16, Vector2.UnitX * MaxLineWidth);
            if (SegmentSpace == -1)
            {
                SegmentSpace = FText.BaseSize;
            }
            if (LineSpace == -1)
            {
                LineSpace = FText.BaseSize;
            }
            Player player = level.GetPlayer();
            if (player is not null)
            {
                player.StateMachine.State = Player.StDummy;
            }
            Add(new Coroutine(Cutscene()));
        }
        public override void OnEnd(Level level)
        {
            Reading = false;
            SolidOpacity = 0;
            TextOpacity = 0;
            if (!EndingChapterAfter && level.GetPlayer() is Player player)
            {
                player.StateMachine.State = Player.StNormal;
                Input.MenuConfirm.ConsumePress();
            }
            InCutscene = false;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            SolidOpacity = 1;
        }
        public override void Update()
        {
            base.Update();
            arrowOffset = ((float)(Math.Sin(Scene.TimeActive * 0.4f) + 1) / 2f);
            ArrowColor = Color.Lerp(Color.White, Color.DarkOrange, arrowOffset);
            SpeedUp = Input.MenuConfirm.Pressed;
            Position = Level.Camera.Position;
            if (Waiting)
            {
                int size = FText.Font.Get(FText.BaseSize).LineHeight;
                if (Input.MenuUp.Pressed)
                {
                    if (RevealedTextBounds.Bottom > View.Bottom - Offset)
                    {
                        View.Y += size;
                    }
                }
                else if (Input.MenuDown.Pressed)
                {
                    if (View.Y > 0)
                    {
                        View.Y -= size;
                    }
                }
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

        #region Routines
        private IEnumerator Cutscene()
        {
            _visible = false;
            bool startOfNewSegment = false;

            yield return 1;
            _forceHide = false;
            _timer = 0;
            _visible = true;
            Reading = true;

            //Start scrolling text
            for (int k = 0; k < DialogIDs.Length; k++)
            {
                PixelFontSize size = FText.Font.Get(FText.BaseSize);
                //loop through each node in the selected dialog
                while (CurrentNode < FText.Nodes.Count)
                {
                    for (int i = 0; i < FText.Nodes.Count; i++)
                    {
                        if (startOfNewSegment)
                        {
                            startOfNewSegment = false;
                            _forceHide = false;
                        }
                        float nodeYOffset = (CurrentLine * LineSpace) + (CurrentSegment * SegmentSpace);
                        ExtraFancyText.Node Node = FText.Nodes[i];
                        CurrentNode = i + 1;
                        if (Node is ExtraFancyText.Cue)
                        {
                            yield return WaitForButton();
                        }
                        if (Node is ExtraFancyText.Char c)
                        {
                            Waiting = false;
                            if (c.Character != ' ')
                            {
                                PixelFontCharacter ch = size.Get(c.Character);
                                _offset.X = Offset + c.Position + c.Offset.X + ch.XOffset + ch.XAdvance;
                                _offset.Y = nodeYOffset + c.Offset.Y - FText.BaseSize / 8;
                                _color = c.Color;
                            }
                            yield return c.Delay * (SpeedUp ? 0.1f : 1.5f);
                        }
                        if (Node is ExtraFancyText.NewLine)
                        {
                            CurrentLine++;
                            RevealedTextBounds.Height += size.LineHeight;
                            if (RevealedTextBounds.Bottom + Offset > View.Bottom)
                            {
                                View.Y = ((RevealedTextBounds.Bottom + Offset) - View.Height);
                            }
                        }
                        if (Node is ExtraFancyText.NewSegment ns)
                        {
                            CurrentSegment++;
                            CurrentNode += (int)Calc.Max(ns.Lines - 1, 0);
                            startOfNewSegment = true;
                            yield return WaitForButton();
                        }
                        if (Node is ExtraFancyText.Wait && !SpeedUp)
                        {
                            _forceHide = true;
                            yield return (Node as ExtraFancyText.Wait).Duration;
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

            //close
            EndCutscene(Level);
        }
        public IEnumerator WaitForButton()
        {
            Waiting = true;
            while (Waiting && !Input.MenuConfirm)
            {
                yield return null;
            }
            Waiting = false;
            yield return null;
        }
        public IEnumerator Reset(int next, bool waitForButton = true)
        {
            if (waitForButton)
            {
                yield return WaitForButton();
            }
            for (float i = 0; i < 1f; i += Engine.DeltaTime)
            {
                TextOpacity = Calc.LerpClamp(1, 0, i);
                yield return null;
            }
            CurrentNode = 0;
            CurrentLine = 1;
            CurrentSegment = 0;
            CurrentID = next;
            LoadText(MaxLineWidth, 200, Vector2.UnitX * MaxLineWidth);
            yield return null;
            for (float i = 0; i < 1f; i += Engine.DeltaTime)
            {
                TextOpacity = Calc.LerpClamp(0, 1, i);
                yield return null;
            }
            _forceHide = false;
            yield return null;
        }
        #endregion

        #region Rendering
        private void DrawUnderscore(PixelFont font, float baseSize, Vector2 position, Vector2 scale, float alpha, Color color)
        {
            Vector2 vector = scale;
            PixelFontSize pixelFontSize = font.Get(baseSize * Math.Max(vector.X, vector.Y));
            PixelFontCharacter pixelFontCharacter = pixelFontSize.Get('_');
            vector *= baseSize / pixelFontSize.Size;
            position.X = _offset.X;
            Vector2 zero = Vector2.Zero;
            zero.X += pixelFontCharacter.XOffset;
            //zero.Y += (float)pixelFontCharacter.YOffset;
            color = new Color(color.R * _color.R, color.G * _color.G, color.B * _color.B, color.A * _color.A);
            pixelFontCharacter.Texture.Draw(position + zero * vector, Vector2.Zero, color * alpha, vector);
        }
        public override void Render()
        {
            Draw.Rect(0, 0, 1920, 1080, BackgroundColor * SolidOpacity);
            FText.DrawScrollOutline(Vector2.One * Offset, -View.Y, Vector2.Zero, Vector2.One, 1, Color.White * TextOpacity, 0, CurrentNode, 4);
            if (_visible && !_forceHide && !DisableUnderscore)
            {
                Vector2 p = new Vector2(RevealedTextBounds.X + _offset.X, RevealedTextBounds.Y + _offset.Y - View.Y);
                DrawUnderscore(FText.Font, FText.BaseSize, p, Vector2.One, TextOpacity, Color.White * TextOpacity);
            }
            base.Render();
        }
        #endregion
    }

}

