using System;
using System.Collections.Generic;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.FrequencyEntities;
using Celeste.Mod.PuzzleIslandHelper.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.PuzzleIslandHelper.Helpers.BitrailHelper;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [Tracked]
    public class FrequencyTeleportComponent : GlobalFrequencyReceiver
    {
        public FrequencyTeleportComponent(params float[] rates) : base(rates)
        {
        }
        protected override void FullPower()
        {
            base.FullPower();
            if(Scene.GetPlayer() is Player player)
            {

            }
        }
    }
}
