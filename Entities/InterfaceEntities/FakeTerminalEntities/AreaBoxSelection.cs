using Microsoft.Xna.Framework;
using Monocle;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities
{
    [TrackedAs(typeof(TextLine))]
    public class AreaBoxSelection : TextLine
    {
        public bool Submitted;
        private const string areaContent = "   *    ";
        public int SelectedBox;
        private Keys lastPressed;
        private float keyBufferTimer;

        public AreaBoxSelection(FakeTerminal terminal, Color color) : base(terminal, "", color)
        {
            string t = "";
            for (int i = 0; i < areaContent.Length; i++)
            {
                t += $"[{areaContent[i]}] ";
            }
            Text = t.TrimEnd(' ');
        }
        private float GetLengthTo(int index)
        {
            return ActiveFont.Measure("[").X + ActiveFont.Measure("] [").X * index;
        }
        public override void TerminalRender(Level level, Vector2 renderAt)
        {
            base.TerminalRender(level, renderAt);
            float scale = 0.7f;
            ActiveFont.Draw('_', 
                level.Camera.CameraToScreen(renderAt + new Vector2(GetLengthTo(SelectedBox), ActiveFont.BaseSize) * scale).Floor() * 6, 
                Vector2.Zero, Vector2.One * scale, Color);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }
        public override void Update()
        {
            base.Update();
            if(Submitted) return;
            if (lastPressed != Keys.None && MInput.Keyboard.Released(lastPressed))
            {
                keyBufferTimer = 0;
                lastPressed = Keys.None;
            }
            if (keyBufferTimer <= 0)
            {
                if (MInput.Keyboard.Check(Keys.Left))
                {
                    lastPressed = Keys.Left;
                    shiftBoxIndex(-1);
                    keyBufferTimer = Engine.DeltaTime * 7;
                }
                else if (MInput.Keyboard.Check(Keys.Right))
                {
                    lastPressed = Keys.Right;
                    shiftBoxIndex(1);
                    keyBufferTimer = Engine.DeltaTime * 7;
                }
            }
            else
            {
                keyBufferTimer -= Engine.DeltaTime;
            }
        }
        private void shiftBoxIndex(int direction)
        {
            SelectedBox = Calc.Clamp(SelectedBox + Math.Sign(direction), 0, areaContent.Length);
        }
        public IEnumerator WaitForSubmit()
        {
            while (!Submitted)
            {
                yield return null;
            }
        }
    }
}