
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [Tracked]
    public class MiniLoader : Component
    {
        public MiniLoaderText Renderer;
        public Vector2 offset;
        private string[] dialogs;
        private float textSize;
        private float maxLength;
        private int maxLines;
        private float maxBarLength;
        public bool Finished;
        public BetterWindow Window;
        public MiniLoader(BetterWindow window, Vector2 textposition, int maxLines, string[] dialogs, float textSize, float maxLineLength, float maxBarLength) : base(true, true)
        {
            Window = window;
            offset = textposition;
            this.maxLines = maxLines;
            this.dialogs = dialogs;
            maxLength = maxLineLength;
            this.maxBarLength = maxBarLength;
            this.textSize = textSize;
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level)
            {
                return;
            }
            Renderer.Position = level.WorldToScreen(Entity.Position + offset);
            Finished = Renderer.Finished;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            Renderer = new MiniLoaderText(Window, entity.Position + offset, dialogs, textSize, (int)maxLength, maxBarLength, maxLines);
            entity.Scene.Add(Renderer);
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Dispose(scene);
        }
        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            Dispose(entity.Scene);
        }
        private void Dispose(Scene scene)
        {
            if (Renderer is not null)
            {
                scene.Remove(Renderer);
            }
            Renderer = null;
        }
        public class MiniLoaderText : Entity
        {
            public bool Finished;
            private int CurrentLine = 1;
            private int CurrentNode;
            private int CurrentID;
            private int MaxLineWidth;
            private int StartNode;

            private float TextOpacity = 1;
            private float LineSpace;

            private List<string> DialogIDs = new();

            private FancyTextExt.Text FText;
            public Vector2 Scale;
            public float Size;
            public int MaxLines;
            public float LineYOffset;
            public const float BarHeight = 24;
            public float BarWidth;
            public BetterWindow Window;

            public MiniLoaderText(BetterWindow window, Vector2 RenderPosition, string[] dialogs, float textSize, int maxLineWidth, float maxBarWidth, int maxLines)
            {
                Window = window;
                Position = RenderPosition;
                Size = textSize;
                Tag = TagsExt.SubHUD;
                MaxLineWidth = maxLineWidth;
                MaxLines = maxLines;
                BarWidth = maxBarWidth;
                DialogIDs.AddRange(dialogs);

            }
            private void LoadText(int maxLineWidth, int linesPerPage, Vector2 offset)
            {
                FText = FancyTextExt.Parse(Dialog.Get(DialogIDs[CurrentID]), maxLineWidth, linesPerPage, offset);
                LineSpace = FText.BaseSize * Size;
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                LoadText(MaxLineWidth, MaxLines, Vector2.UnitX * MaxLineWidth);
                FText.BaseSize = Size;
                Add(new Coroutine(Cutscene()));
            }

            public override void Update()
            {
                base.Update();
                LineYOffset = CurrentLine * LineSpace * 6;
            }
            #region Routines
            private void Advance(int next)
            {
                StartNode = 0;
                CurrentNode = 0;
                CurrentLine = 0;
                FText = FancyTextExt.Parse(Dialog.Get(DialogIDs[next]), MaxLineWidth, 16, Vector2.UnitX * MaxLineWidth);
            }
            private IEnumerator Cutscene()
            {
                float delayScalar = 1f;
                int charSkip = 5;
                //start scrolling text
                for (int k = 0; k < DialogIDs.Count; k++)
                {
                    CurrentLine = 0;
                    while (CurrentNode < FText.Nodes.Count)
                    {
                        int charSkipCount = 0;
                        bool hitChar = false;
                        for (int i = 0; i < FText.Nodes.Count; i++)
                        {
                            FancyTextExt.Node Node = FText.Nodes[i];

                            CurrentNode = i + 1;

                            if (Node is FancyTextExt.Char c)
                            {

                                if (charSkipCount < charSkip)
                                {
                                    charSkipCount++;
                                }
                                else
                                {
                                    hitChar = true;
                                    yield return c.Delay * 1.5f * delayScalar;
                                    charSkipCount = 0;
                                }
                            }
                            if (Node is FancyTextExt.NewLine)
                            {
                                CurrentLine++;
                            }
                        }
                        if (!hitChar)
                        {
                            yield return null;
                        }
                    }

                    if (k + 1 < DialogIDs.Count)
                    {
                        Advance(k + 1);
                    }
                }
                yield return 0.3f;
                Finished = true;
                //close
            }


            #endregion

            #region Rendering
            public override void Render()
            {
                if (!Window.Drawing)
                {
                    base.Render();
                    return;
                }
                FText.Draw(Position - Vector2.UnitY * ((CurrentLine * LineSpace) + BarHeight + 1), Vector2.Zero, Size * Vector2.One, 1, Color.White * TextOpacity, StartNode, CurrentNode);
                Draw.Rect(Position, BarWidth, BarHeight, Color.Black);
                float lerp = (float)CurrentNode / FText.Nodes.Count;
                Draw.Rect(Position + Vector2.One, Calc.LerpClamp(0, BarWidth - 2, lerp), BarHeight - 2, Color.Green);
                base.Render();
            }
            #endregion
        }
    }
}