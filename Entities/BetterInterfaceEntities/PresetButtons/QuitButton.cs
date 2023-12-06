
using Microsoft.Xna.Framework;
using Monocle;
using System;

using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities
{

    [TrackedAs(typeof(BetterWindowButton))]
    public class QuitButton : BetterWindowButton
    {
        public QuitButton(Interface entityInterface, Action OnClicked = null, IEnumerator Routine = null) : base(entityInterface,null, OnClicked, Routine)
        {
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