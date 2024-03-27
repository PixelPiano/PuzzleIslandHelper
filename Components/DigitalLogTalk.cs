using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [TrackedAs(typeof(CustomTalkComponent))]
    public class DigitalLogTalk : CustomTalkComponent
    {
        public DigitalLogTalk(float x, float y, float width, float height, Vector2 drawAt, Action<Player> onTalk) : base(x, y, width, height, drawAt, onTalk, SpecialType.DigitalLog)
        {
        }
    }


}