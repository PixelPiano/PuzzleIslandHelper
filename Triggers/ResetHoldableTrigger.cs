using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/ResetHoldableTrigger")]
    [Tracked]
    public class ResetHoldableTrigger : Trigger
    {
        public ResetHoldableTrigger(EntityData data, Vector2 offset)
            : base(data, offset)
        {
        }
        public override void OnEnter(Player player)
        {
            
        }

        public override void OnLeave(Player player)
        {
        }
      
    }
}
