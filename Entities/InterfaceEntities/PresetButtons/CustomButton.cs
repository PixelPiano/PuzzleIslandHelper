using Microsoft.Xna.Framework;
using Monocle;
using System;

using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{

    [TrackedAs(typeof(BetterButton))]
    public class CustomButton : BetterButton
    {
        public CustomButton(BetterWindow window, string text, float textSize, Vector2 textOffset,  Action OnClicked = null, IEnumerator Routine = null) : base(window, null, OnClicked, Routine)
        {
            AutoPosition = true;
            Text = text;
            TextSize = textSize;
            TextOffset = textOffset;
        }
    }
}