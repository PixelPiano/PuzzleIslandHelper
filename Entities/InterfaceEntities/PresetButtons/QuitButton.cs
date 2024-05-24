using Microsoft.Xna.Framework;
using Monocle;
using System;

using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{

    [TrackedAs(typeof(BetterButton))]
    public class QuitButton : BetterButton
    {
        public QuitButton(BetterWindow window, Action OnClicked = null, IEnumerator Routine = null) : base(window,null, OnClicked, Routine)
        {
            AutoPosition = true;
            Text = "Quit";
            TextSize = 35f;
            TextOffset = Vector2.UnitX * 6;

        }
        public override void Update()
        {
            base.Update();

        }
    }
}