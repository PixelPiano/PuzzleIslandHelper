using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/PrologueCutsceneTrigger")]
    [Tracked]
    public class PrologueCutsceneTrigger : Trigger
    {

        public PrologueCutsceneTrigger(EntityData data, Vector2 offset)
    : base(data, offset)
        {
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Level level = Scene as Level;
            if (!level.Session.GetFlag("startFalling") || !level.Session.GetFlag("startFalling2"))
            {
                return;
            }
            PrologueBird bird = level.Tracker.GetEntity<PrologueBird>();
            if(bird is not null)
            {
                bird.Enabled = true;
            }
            RemoveSelf();
        }
    }
}
