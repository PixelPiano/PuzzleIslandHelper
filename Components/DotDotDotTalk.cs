using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [TrackedAs(typeof(CustomTalkComponent))]
    public class DotX3 : CustomTalkComponent
    {
        public DotX3(float x, float y, float width, float height, Vector2 drawAt, Action<Player> onTalk) : base(x, y, width, height, drawAt, onTalk, SpecialType.DotDotDot)
        {
        }
        public DotX3(Collider collider, Action<Player> onTalk) : base(collider.Position.X,collider.Position.Y,collider.Width,collider.Height, Vector2.UnitX * collider.Width/2, onTalk, SpecialType.DotDotDot)
        {

        }
    }


}