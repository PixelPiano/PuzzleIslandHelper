using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;
using System;
using System.Collections;
using static FrostHelper.CustomZipMover;
using System.Runtime.InteropServices;
using System.IO;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [TrackedAs(typeof(CustomTalkComponent))]
    public class DotX3 : CustomTalkComponent
    {
        public DotX3(float x, float y, float width, float height, Vector2 drawAt, Action<Player> onTalk) : base(x, y, width, height, drawAt, onTalk, SpecialType.DotDotDot)
        {
        }
    }


}