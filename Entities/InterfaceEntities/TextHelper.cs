using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    public class TextHelper : Entity
    {
        public string Text;
        public Vector2 Scale = Vector2.One;
        public float Alpha = 1;
        public Color Color;
        public bool UseWorldCoords;
        public TextHelper(Color color) : base(Vector2.Zero)
        {
            Color = color;
            Tag |= TagsExt.SubHUD;
        }
        public void DrawOutline()
        {
            ActiveFont.DrawOutline(Text, Position * 6, new Vector2(0f, 0.5f), Scale, Color * Alpha, 2f, Color.Black * (Alpha * Alpha * Alpha));
        }
        public void DrawOutline(Vector2 position, string text)
        {
            ActiveFont.DrawOutline(text, position, new Vector2(0f, 0.5f), Scale, Color * Alpha, 2f, Color.Black * (Alpha * Alpha * Alpha));
        }
        public void Draw()
        {
            ActiveFont.Draw(Text, Position * 6, Color * Alpha);
        }
    }
    [Tracked]
    public class TextComponent : GraphicsComponent
    {
        public string Text;
        public float Alpha = 1;
        public bool UseWorldCoords;
        private Vector2 textPosition;
        
        public TextComponent(Color color, bool useWorldCoords) : base(true)
        {
            UseWorldCoords = useWorldCoords;
            Color = color;
        }
        public override void Update()
        {
            base.Update();
            if (UseWorldCoords && Scene is Level level)
            {
                textPosition = level.ScreenToWorld(RenderPosition) * 6;
            }
            else
            {
                textPosition = Position * 6;
            }
        }
        public void DrawOutline()
        {
            ActiveFont.DrawOutline(Text, textPosition, new Vector2(0f, 0.5f), Scale, Color * Alpha, 2f, Color.Black * (Alpha * Alpha * Alpha));
        }
        public void DrawOutline(Vector2 position, string text)
        {
            ActiveFont.DrawOutline(text, position, new Vector2(0f, 0.5f), Scale, Color * Alpha, 2f, Color.Black * (Alpha * Alpha * Alpha));
        }
        public void Draw()
        {
            ActiveFont.Draw(Text, textPosition, Color * Alpha);
        }
    }
}