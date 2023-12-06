using Microsoft.Xna.Framework;
using Monocle;
using System;

using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities
{

    [TrackedAs(typeof(BetterWindowButton))]
    public class StartButton : BetterWindowButton
    {
        public StartButton(Interface entityInterface, Action OnClicked = null, IEnumerator Routine = null) : base(entityInterface,null, OnClicked, Routine)
        {
            Text = "Start";
            TextSize = 35f;
            TextOffset = Vector2.UnitX;
        }
        public override void Update()
        {
            base.Update();
            
        }
    }
}