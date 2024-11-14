using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.FakeTerminalEntities
{
    [TrackedAs(typeof(Group))]
    public class TextLine : Group
    {
        public FancyText.Text activeText;
        public string Text;
        public string FullText { get; private set; }
        public float LineHeight;

        public Vector2 Scale = Vector2.One * 0.7f;
        public Color Color = Color.White;
        public static Color DebugColor = Color.White;
        public bool DrawsSquare = true;
        public static bool HideSquare;
        public bool ForceSquare;
        public Vector2 RenderPosition { get; private set; }
        public TextLine(FakeTerminal terminal, string text, Color color) : base(terminal)
        {
            Color = color;
            Text = text;
        }
        public virtual string GetText()
        {
            return Text;
        }
        public override void Update()
        {
            base.Update();
            string text = "> ";
            string full = GetText();
            if (!string.IsNullOrEmpty(full))
            {
                text += full;
            }
            FullText = text;
        }
        public override void TerminalRender(Level level, Vector2 renderAt)
        {
            RenderPosition = level.Camera.CameraToScreen(renderAt).Floor() * 6;
            ActiveFont.Draw(FullText, RenderPosition, Vector2.Zero, Scale, Color * Alpha);
            if (ForceSquare || (DrawsSquare && IsCurrentIndex && !Halt && !HideSquare))
            {
                Vector2 measure = ActiveFont.Measure(FullText) * Scale;
                float width = ActiveFont.BaseSize * 0.6f * Scale.X;
                float height = ActiveFont.BaseSize * 0.8f * Scale.Y;
                Vector2 position = RenderPosition + new Vector2(measure.X + 2, measure.Y / 2f - height / 2f);
                Draw.Rect(position, width, height, Color * Alpha); ;
            }
        }
    }
}