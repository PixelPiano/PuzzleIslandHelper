using Microsoft.Xna.Framework;
using Monocle;
using System;

using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{

    [TrackedAs(typeof(BetterButton))]
    public class StartButton : BetterButton
    {
        public StartButton(BetterWindow window, Action OnClicked = null, IEnumerator Routine = null) : base(window, null, OnClicked, Routine)
        {
            AutoPosition = true;
            Text = "start";
            TextSize = 35f;
            TextOffset = Vector2.UnitX;
        }
        public override void Update()
        {
            base.Update();

        }
    }
}