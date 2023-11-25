using Celeste.Mod.PuzzleIslandHelper.Entities.Windows;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities
{

    [TrackedAs(typeof(Component))]
    public class StartButton : BetterWindowButton
    {
        public StartButton(Action OnClicked = null, IEnumerator Routine = null) : base(OnClicked, Routine)
        {
            Text = "Start";
        }
        public override void Update()
        {
            base.Update();
            
        }
    }
}