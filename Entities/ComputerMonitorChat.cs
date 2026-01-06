using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities.Programs;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class ComputerMonitorChat : CutsceneEntity
    {
        public float MaxHeight;
        public float MaxWidth;
        private const string fontName = "Undead Pixel 8 Regular";
        private static readonly Dictionary<string, List<string>> fontPaths;
        static ComputerMonitorChat()
        {
            fontPaths = (Dictionary<string, List<string>>)typeof(Fonts).GetField("paths", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }

        private string FirstID;
        public bool InCutscene = true;
        public class Line
        {
            public ComputerMonitorChat Chat;
            public Vector2 Position;
            public string ID;
            public bool Raw;
            public ExtraFancyText.Text Text;
            public int End = 0;
            public int Index;
            public Vector2 Scale = Vector2.One;
            public Line(ComputerMonitorChat chat, int index, PixelFont font, float height, Vector2 scale, string id, bool raw, int maxLineWidth, int maxLines, Vector2 offset)
            {
                Index = index;
                Chat = chat;
                ID = id;
                Raw = raw;
                Scale = scale;
                Text = ExtraFancyText.Parse(raw ? id : Dialog.Get(id), maxLineWidth, maxLines, offset);
                Text.Font = font;
                Text.BaseSize = height;
            }
            public override string ToString()
            {
                return string.IsNullOrEmpty(ID) ? Text.ToString() : Raw ? ID : Dialog.Get(ID);
            }
            public List<Line> AdvanceToNext(ExtraFancyText.Advance advance)
            {
                int w = (int)Chat.MaxWidth;
                return Chat.AddChunk(advance.ID, w, 1, Vector2.UnitX * w);
            }
            public IEnumerator Scroll()
            {
                for (int i = 0; i < Text.Nodes.Count; i++)
                {
                    End = i + 1;
                    ExtraFancyText.Node node = Text.Nodes[i];
                    if (node is ExtraFancyText.Wait wait)
                    {
                        yield return wait.Duration;
                    }
                    if (node is ExtraFancyText.Char c)
                    {
                        yield return c.Delay * (Chat.SpeedUp ? 0.5f : 1.5f);
                    }
                    if (node is ExtraFancyText.Confirm confirm)
                    {
                        Chat.InControl = true;
                        while (!Input.DashPressed) yield return null;
                        Chat.InControl = false;
                    }
                    if (node is ExtraFancyText.ConsoleChoice choice)
                    {
                        float keyBuffer = 0.2f;
                        float timer = keyBuffer;
                        int index = 0;
                        ExtraFancyText.Advance advance2 = null;

                        while (advance2 == null)
                        {
                            Chat.CurrentChoice = choice;
                            if (timer <= 0)
                            {
                                if (Input.MenuRight.Pressed)
                                {
                                    index = Math.Min(choice.Options.Count - 1, index + 1);
                                    timer = keyBuffer;
                                }
                                if (Input.MenuLeft.Pressed)
                                {
                                    index = Math.Max(0, index - 1);
                                    timer = keyBuffer;
                                }
                                if (Input.MenuConfirm.Pressed)
                                {
                                    advance2 = choice.Options[index].Advance;
                                    break;
                                }
                            }
                            else timer -= Engine.DeltaTime;
                            for (int j = 0; j < choice.Options.Count; j++)
                            {
                                choice.Options[j].Selected = index == j;
                            }
                            yield return null;
                        }
                        List<Line> next = AdvanceToNext(advance2);
                        Chat.CurrentChoice = null;
                        yield return 0.2f;
                        yield return ScrollThroughLines(next);
                    }
                    if (node is ExtraFancyText.Advance advance)
                    {
                        List<Line> next = AdvanceToNext(advance);
                        yield return ScrollThroughLines(next);
                    }
                }
            }
            public void Render(Vector2 position, float alpha, Color color)
            {
                Text.Draw(position, Vector2.Zero, Scale, alpha, color, 0, End);
                Draw.Rect(position.X - 20, position.Y, 16, 16, Color.Red);
            }
        }
        public static List<Line> CreateLines(ComputerMonitorChat chat, PixelFont font, float height, Vector2 scale, string dialogID, int maxLineWidth, int maxLinesPerPage, Vector2 offset)
        {
            List<Line> list = [];
            var lines = ExtraFancyText.ParseSplit(Dialog.Get(dialogID), maxLineWidth, maxLinesPerPage, offset);
            foreach (var text in lines)
            {
                list.Add(new Line(chat, chat.Lines.Count + list.Count, font, height, scale, text, true, int.MaxValue, maxLinesPerPage, offset));
            }
            return list;
        }
        public static IEnumerator ScrollThroughLines(List<Line> lines)
        {
            foreach (Line line in lines)
            {
                if (line.Index > line.Chat.LineIndex + line.Chat.LinesPerPage - 1)
                {
                    line.Chat.ShiftToLine(line.Index - line.Chat.LinesPerPage + 1);
                }
                yield return line.Scroll();
            }
        }
        private readonly List<Line> Lines = [];
        public ExtraFancyText.ConsoleChoice CurrentChoice;
        public int LinesPerPage;
        public float LineHeight;
        public bool InControl;
        public bool SpeedUp;
        private bool upPressed;
        private bool upWasPressed;
        private bool downPressed;
        private bool downWasPressed;
        public int LineIndex;
        public ComputerMonitorChat(Vector2 screenPosition, string dialogID, float width, float height, int linesPerPage)
        {
            MaxWidth = width;
            MaxHeight = height;
            Position = screenPosition;
            Tag |= TagsExt.SubHUD;
            Depth = -1000001;
            FirstID = dialogID;
            LinesPerPage = linesPerPage;
            LineHeight = (int)(height / LinesPerPage);
        }
        private List<Line> AddChunk(string id, int maxLineWidth, int linesPerPage, Vector2 offset)
        {
            ensureCustomFontIsLoaded();
            PixelFont font = Fonts.Get(fontName);
            List<Line> chunk = CreateLines(this, font, LineHeight, Vector2.One, id, maxLineWidth, linesPerPage, offset);
            AddChunk(chunk);
            return chunk;
        }
        public void AddChunk(List<Line> chunk)
        {
            Lines.AddRange(chunk);
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
                    Logger.Log(LogLevel.Warn, "PuzzleIslandHelper/ComputerMonitorChat", $"We need to list fonts again, {fontName} does not exist!");
                    Fonts.Prepare();
                }

                Fonts.Load(fontName);
                Engine.Scene.Add(new FontHolderEntity());
            }
        }

        private IEnumerator Cutscene(List<Line> chunk)
        {
            yield return ScrollThroughLines(chunk);
            EndCutscene(Level);
        }
        public void ShiftLines(int lines)
        {
            LineIndex = Math.Clamp(LineIndex + lines, 0, Math.Max(0, Lines.Count - 1 - LinesPerPage));
        }
        public void ShiftToLine(int lineIndex)
        {
            LineIndex = Math.Clamp(lineIndex, 0, Math.Max(0, Lines.Count - LinesPerPage));
        }
        public override void Update()
        {
            base.Update();
            SpeedUp = Input.Dash.Pressed || Input.Jump.Pressed;
            upWasPressed = upPressed;
            downWasPressed = downPressed;
            upPressed = Input.MenuUp.Pressed;
            downPressed = Input.MenuDown.Pressed;
            if (InControl)
            {
                if (upPressed && !upWasPressed)
                {
                    ShiftLines(-1);
                }
                else if (downPressed && !downWasPressed)
                {
                    ShiftLines(1);
                }
            }
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (Line line in Lines)
            {
                line.Text = null;
            }
            Lines.Clear();
        }
        public override void OnBegin(Level level)
        {
            LineIndex = 0;
            Lines.Clear();
            ensureCustomFontIsLoaded();
            List<Line> firstChunk = AddChunk(FirstID, (int)MaxWidth, 1, Vector2.Zero);
            level.GetPlayer()?.DisableMovement();
            Add(new Coroutine(Cutscene(firstChunk)));
        }

        public override void OnEnd(Level level)
        {
            level.GetPlayer()?.EnableMovement();
            InCutscene = false;
        }
        #region Rendering
        public override void Render()
        {
            Vector2 p = Position;
            Vector2 scale = Vector2.One;
            for (int i = LineIndex; i < Lines.Count && i < LineIndex + LinesPerPage; i++)
            {
                Vector2 position = Position + Vector2.UnitY * (i - LineIndex) * LineHeight;
                Lines[i].Render(position, 1, Color.White);
            }
            if (CurrentChoice != null)
            {
                float y = Calc.Min(Y, Y + MaxHeight);
                CurrentChoice.Render(new Vector2(X, y), scale, new Vector2(10, LineHeight), (int)MaxWidth);
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