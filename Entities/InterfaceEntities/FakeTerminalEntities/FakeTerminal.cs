using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using static Celeste.Mod.PuzzleIslandHelper.Entities.ArtifactSlot;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities
{
    [Tracked]
    public class FakeTerminal : Entity
    {
        public UserInput UserInput => Renderer.Input;
        public bool Waiting;
        public const int LINEHEIGHT = 6;
        public Color Color;
        public Color BorderColor;
        public int BorderSize = 1;
        public TerminalRenderer Renderer;
        private float keyBufferTimer;
        public float TransitionAmount;
        public Color DebugColor = Color.White;
        public Group SelectedGroup => Renderer.SelectedGroup;
        private float hideSquareTimer;
        public bool HideSquare => hideSquareTimer > 0;
        public FakeTerminal(Vector2 position, float width, float height) : base(position)
        {
            Depth = -100000;
            Collider = new Hitbox(width, height);
            Color = Color.Black;
            BorderColor = Color.LightGray;
        }
        public void Open()
        {
            TransitionAmount = 0;
            Add(new Coroutine(transitionOpen()));
        }
        private IEnumerator transitionOpen()
        {
            TransitionAmount = 0;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.8f)
            {
                TransitionAmount = Calc.LerpClamp(0, 1, Ease.Follow(Ease.ElasticIn, Ease.BounceOut)(i));
                yield return null;
            }
            TransitionAmount = 1;
            yield return Renderer.FadeGroups(0, 1, 1);
        }
        public void SetGroupAlphas(float value)
        {
            Renderer.SetGroupAlphas(value);
        }
        private IEnumerator transitionClose()
        {
            yield return Renderer.FadeGroups(1, 0, 1);
            TransitionAmount = 1;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.5f)
            {
                TransitionAmount = Calc.LerpClamp(1, 0, Ease.Follow(Ease.ElasticIn, Ease.BounceOut)(i));
                yield return null;
            }
            TransitionAmount = 0;
            RemoveSelf();

        }
        public void Close()
        {
            UserInput.BlockHotkeys = false;
            TransitionAmount = 1;
            Add(new Coroutine(transitionClose()));
        }
        public bool KeyCheck(params Keys[] keys)
        {
            KeyboardState state = MInput.Keyboard.CurrentState;
            foreach (Keys key in keys)
            {
                if (state[key] == KeyState.Down)
                {
                    return true;
                }
            }
            return false;
        }
        public override void Update()
        {
            base.Update();
            if (Renderer.Groups.Count == 0) return;

            if (HideSquare)
            {
                hideSquareTimer -= Engine.DeltaTime;
            }
            if (keyBufferTimer <= 0)
            {
                int lines;
                if (KeyCheck(Keys.LeftShift, Keys.RightShift))
                {
                    lines = 50;
                }
                else if (KeyCheck(Keys.LeftControl, Keys.RightControl))
                {
                    lines = 10;
                }
                else
                {
                    lines = 1;
                }
                if (MInput.Keyboard.Check(Keys.Down))
                {
                    Shift(lines);
                    keyBufferTimer = Engine.DeltaTime * 5;
                }
                else if (MInput.Keyboard.Check(Keys.Up))
                {
                    Shift(-lines);
                    keyBufferTimer = Engine.DeltaTime * 5;
                }
                if (MInput.Keyboard.Check(Keys.Left))
                {
                    SelectedGroup?.OnLeft();
                }
                if (MInput.Keyboard.Check(Keys.Right))
                {
                    SelectedGroup?.OnRight();
                }
            }
            else
            {
                keyBufferTimer -= Engine.DeltaTime;
            }
        }
        public void AddGroup(Group group)
        {
            Renderer.AddGroup(group);
        }
        public TextLine[] AddText(string text, params Color[] lineColors)
        {
            return Renderer.AddText(text, lineColors);
        }
        public TextLine[] AddText(string text, Color color)
        {
            return Renderer.AddText(text, color);
        }
        public void AddSpace(int spaces)
        {
            Renderer.AddSpace(spaces);
        }
        public void Shift(int lines)
        {
            int a = Math.Abs(lines);
            int d = Math.Sign(lines);
            for (int i = 0; i < a; i++)
            {
                string current = SelectedGroup.ID;
                int prev = Renderer.LineIndex;
                Renderer.LineIndex += d;
                int newLine = Renderer.LineIndex;
                int start = Renderer.StartIndex;
                if (newLine == prev || newLine < start || newLine > start + Renderer.LinesAvailable - 1)
                {
                    Renderer.StartIndex += d;
                }
                if (SelectedGroup is EmptyLine line && line.ID != current)
                {
                    Shift(d);
                }
            }


        }
        public void Clear()
        {
            Renderer.Clear();
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(Renderer = new TerminalRenderer(this));
            MInput.Disabled = true;
            Open();
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Renderer.RemoveSelf();
            MInput.Disabled = false;
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            MInput.Disabled = false;
        }
        /*        public override void DebugRender(Camera camera)
                {
                    base.DebugRender(camera);
                    Draw.Rect(camera.Position + new Vector2(160, 0), 160, 90, DebugColor);
                }*/
        public override void Render()
        {
            base.Render();
            if (TransitionAmount <= 0) return;
            Vector2 pos = (Position + Collider.HalfSize * (1 - TransitionAmount)).Floor();
            float width = Width * TransitionAmount;
            float height = Height * TransitionAmount;
            Draw.Rect(pos, width, height, Color.Lerp(Color.White, Color, TransitionAmount));
            Draw.HollowRect(pos.X - BorderSize, pos.Y - BorderSize, width + BorderSize * 2, height + BorderSize * 2, Color.Lerp(Color.White, BorderColor, TransitionAmount));
        }
    }
}