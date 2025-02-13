using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class Rect : GraphicsComponent
    {
        public float Width;
        public float Height;
        public bool UseEntityCollider;
        public Color Fill;
        public Color Border;
        public bool UseBorder = false;
        public bool UseFill = true;
        public int BorderThickness;
        public Rect(Vector2 position, float width, float height, Color color) : base(true)
        {
            Fill = Border = color;
            Position = position;
            Width = width;
            Height = height;
        }
        public override void Render()
        {
            base.Render();
            float w = Width, h = Height;
            Vector2 pos;
            if (UseEntityCollider && Entity != null && Entity.Collider != null)
            {
                w = Entity.Collider.Width;
                h = Entity.Collider.Height;
                pos = Entity.Collider.AbsolutePosition + Position;
            }
            else
            {
                pos = RenderPosition;
            }
            if (UseBorder)
            {
                Draw.HollowRect(pos, w, h, Border);
                if (UseFill)
                {
                    Draw.Rect(pos + Vector2.One, w - 2, h - 2, Fill);
                }
            }
            else if (UseFill)
            {
                Draw.Rect(pos, w, h, Fill);
            }
        }
    }
}
