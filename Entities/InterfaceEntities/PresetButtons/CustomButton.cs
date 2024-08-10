using Microsoft.Xna.Framework;
using Monocle;
using System;

using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{

    [TrackedAs(typeof(Button))]
    public class CustomButton : Button
    {
        public CustomButton(Window window, string text, float textSize, Vector2 textOffset,  Action OnClicked = null, IEnumerator Routine = null) : base(window, null, OnClicked, Routine)
        {
            AutoPosition = true;
            Text = text;
            TextSize = textSize;
            TextOffset = textOffset;
        }
    }
}