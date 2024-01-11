using Microsoft.Xna.Framework;
using Monocle;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities
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
}