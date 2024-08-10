using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using static Celeste.Overworld;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities
{
    [TrackedAs(typeof(Group))]
    public class TextLine : Group
    {
        public FancyText.Text activeText;
        public string Text;
        public float LineHeight;
        private bool squareOn;
        public Color Color = Color.White;
        public bool DrawsSquare = true;
        public TextLine(FakeTerminal terminal, string text, int index, Color color) : base(terminal, index)
        {
            Color = color;
            Text = text;
            Alarm alarm = Alarm.Create(Alarm.AlarmMode.Looping, delegate { squareOn = !squareOn; }, 1, true);
            Add(alarm);
        }
        public override void TerminalRender(Level level, Vector2 renderAt)
        {
            string text = "> ";
            float scale = 0.7f;
            if (!string.IsNullOrEmpty(Text)) text += Text;
            Vector2 pos = level.Camera.CameraToScreen(renderAt).Floor() * 6;
            ActiveFont.Draw(text, pos, Vector2.Zero, Vector2.One * scale, Color * Alpha);
            if (DrawsSquare && IsCurrentIndex && !Halt)
            {
                Vector2 measure = ActiveFont.Measure(text) * scale;
                float width = ActiveFont.BaseSize * 0.6f * scale;
                float height = ActiveFont.BaseSize * 0.8f * scale;
                Vector2 position = pos + new Vector2(measure.X + 2, measure.Y / 2f - height / 2f);
                Draw.Rect(position, width, height, Color * Alpha);
            }
        }
    }
}