using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    //[CustomEntity("PuzzleIslandHelper/MemoryTextscene")]
    [Tracked]
    public class MemoryTextscene : CutsceneEntity
    {
        private const int XOffset = 1920 / 16;
        private const int MaxLineWidth = 1920 / 2 - XOffset;
        private bool uses_;
        

        private const string fontName = "pixelary";
        private static readonly Dictionary<string, List<string>> fontPaths;
        static MemoryTextscene()
        {
            fontPaths = (Dictionary<string, List<string>>)typeof(Fonts).GetField("paths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }

        private int CurrentLine = 1;
        private int CurrentSegment;
        private int CurrentNode;
        private int CurrentID;


        private float SolidOpacity;
        private float TextOpacity = 1;
        private float LineSpace;
        private float SegmentSpace;
        private float _timer;
        private const float _timerLimit = 0.6f;

        public virtual void OnCue(string[] args)
        {

        }
        private string[] DialogIDs;
        private bool Waiting;
        public bool InCutscene = true;
        private bool _visible;
        private bool _forceHide;
        private Vector2 _position;
        private ExtraFancyText.Text FText;

        public MemoryTextscene(string dialogID, float segmentSpace = -1, float lineSpace = -1) : this(segmentSpace, lineSpace, dialogID) { }
        public MemoryTextscene(float segmentSpace, float lineSpace, params string[] dialogIDs) : base()
        {
            Tag |= TagsExt.SubHUD;
            Depth = -1000001;
            DialogIDs = dialogIDs;
            SegmentSpace = segmentSpace;
            LineSpace = lineSpace;
        }
        private void LoadText(int maxLineWidth, int linesPerPage, Vector2 offset)
        {
            FText = ExtraFancyText.Parse(Dialog.Get(DialogIDs[CurrentID]), maxLineWidth, linesPerPage, offset);
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
        
        #region Routines
        private IEnumerator Cutscene()
        {
            _visible = false;
            bool startOfNewSegment = false;
            for (float i = 0; i < 1f; i += Engine.DeltaTime)
            {
                SolidOpacity = Calc.LerpClamp(0, 0.4f, i);
                yield return null;
            }
            yield return 1;
            _forceHide = false;
            _timer = 0;
            _visible = true;


            //Start scrolling text
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
                        float _ypos = CurrentLine * LineSpace + CurrentSegment * SegmentSpace + XOffset - FText.BaseSize / 8;

                        ExtraFancyText.Node Node = FText.Nodes[i];
                        CurrentNode = i + 1;
                        if(Node is ExtraFancyText.Cue cue)
                        {
                            OnCue(cue.args);
                        }
                        if (Node is ExtraFancyText.Char c)
                        {
                            if (c.Character != ' ')
                            {
                                PixelFontSize size = FText.Font.Get(FText.BaseSize);
                                PixelFontCharacter ch = size.Get(c.Character);
                                _position.X = c.Position + XOffset + c.Offset.X + ch.XOffset + ch.XAdvance;
                                _position.Y = _ypos + c.Offset.Y;
                            }
                            yield return c.Delay * (SpeedUp ? 0.1f : 1.5f);
                        }
                        if (Node is ExtraFancyText.Wait)
                        {
                            _forceHide = true;
                            yield return (Node as ExtraFancyText.Wait).Duration;
                            _forceHide = false;
                        }
                        if (Node is ExtraFancyText.NewLine)
                        {
                            CurrentLine++;
                        }
                        if (Node is ExtraFancyText.NewSegment ns)
                        {
                            CurrentSegment++;
                            CurrentNode += (int)Calc.Max(ns.Lines - 1, 0);
                            startOfNewSegment = true;
                            yield return SpeedUp ? null : 0.7f;
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
            for (float i = 0; i < 1f; i += Engine.DeltaTime)
            {
                SolidOpacity = Calc.LerpClamp(0.4f, 0, i);
                TextOpacity = Calc.LerpClamp(1, 0, i);
                yield return null;
            }
            EndCutscene(Level);
        }
        public bool SpeedUp;
        public override void Update()
        {
            base.Update();
            SpeedUp= Input.Dash.Pressed || Input.Jump.Pressed;
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
        private IEnumerator Reset(int next)
        {
            yield return WaitForButton();
            for (float i = 0; i < 1f; i += Engine.DeltaTime)
            {
                TextOpacity = Calc.LerpClamp(1, 0, i);
                yield return null;
            }
            CurrentNode = 0;
            CurrentLine = 1;
            CurrentSegment = 0;
            FText = ExtraFancyText.Parse(Dialog.Get(DialogIDs[next]), MaxLineWidth, 16, Vector2.UnitX * MaxLineWidth);
            yield return null;
            for (float i = 0; i < 1f; i += Engine.DeltaTime)
            {
                TextOpacity = Calc.LerpClamp(0, 1, i);
                yield return null;
            }
            _forceHide = true;
            yield return null;
        }
        #endregion

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
            Player player = level.Tracker.GetEntity<Player>();
            if (player is not null)
            {
                player.StateMachine.State = Player.StDummy;
            }
            Add(new Coroutine(Cutscene()));
        }

        public override void OnEnd(Level level)
        {
            SolidOpacity = 0;
            TextOpacity = 0;
            if (level.GetPlayer() is Player player)
            {
                player.StateMachine.State = Player.StNormal;
            }
            InCutscene = false;
        }
        #region Rendering
        private void DrawUnderscore(PixelFont font, float baseSize, Vector2 position, Vector2 scale, float alpha, Color color)
        {
            Vector2 vector = scale;
            PixelFontSize pixelFontSize = font.Get(baseSize * Math.Max(vector.X, vector.Y));
            PixelFontCharacter pixelFontCharacter = pixelFontSize.Get('_');
            vector *= baseSize / pixelFontSize.Size;
            position.X = _position.X;
            Vector2 zero = Vector2.Zero;
            zero.X += pixelFontCharacter.XOffset;
            //zero.Y += (float)pixelFontCharacter.YOffset;
            pixelFontCharacter.Texture.Draw(position + zero * vector, Vector2.Zero, color * alpha, vector);
        }
        public override void Render()
        {
            Draw.Rect(0, 0, 1920, 1080, Color.Black * SolidOpacity);

            FText.Draw(Vector2.One * XOffset, Vector2.Zero, Vector2.One, 1, Color.White * TextOpacity, 0, CurrentNode);
            if (_visible && !_forceHide)
            {
                DrawUnderscore(FText.Font, FText.BaseSize, _position, Vector2.One, TextOpacity, Color.White * TextOpacity);
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

