using System;
using Celeste.Mod.CommunalHelper;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [TrackedAs(typeof(FancyLine))]
    public class FancyScreenLine : FancyLine
    {
        public bool Horizontal;
        public float PositionOne;
        public FancyScreenLine(float position, bool horizontal, Color color, float thickness = 1) : base(Vector2.Zero, Vector2.Zero, color, thickness)
        {
            PositionOne = position;
        }
        public FancyScreenLine(float position, bool horizontal, Color color, float thickness, Color color2, ColorModes colorMode, float interval) : this(position, horizontal, color, thickness)
        {
            ColorMode = colorMode;
            ColorInterval = interval;
            Color2 = color2;
        }
        public override void Render()
        {
            Camera cam = SceneAs<Level>().Camera;
            Rectangle bounds = cam.GetBounds();
            Vector2 start, end;
            if (Horizontal && (int)PositionOne > bounds.Top && (int)PositionOne < bounds.Bottom)
            {
                start = new Vector2(bounds.Left, PositionOne);
                end = new Vector2(bounds.Right, PositionOne);
            }
            else if (!Horizontal && (int)PositionOne > bounds.Left && (int)PositionOne < bounds.Right)
            {
                start = new Vector2(PositionOne, bounds.Top);
                end = new Vector2(PositionOne, bounds.Bottom);
            }
            else return;
            Render(start, end);
        }
    }
}
