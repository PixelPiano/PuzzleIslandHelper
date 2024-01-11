using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.Prologue;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/PrologueCutsceneTrigger")]
    [Tracked]
    public class PrologueCutsceneTrigger : Trigger
    {
        public bool SecondTry = false;
        public bool Activated = false;
        public PrologueCutsceneTrigger(EntityData data, Vector2 offset)
    : base(data, offset)
        {
            Tag |= Tags.TransitionUpdate;
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Level level = Scene as Level;
            if (!SecondTry)
            {
                PrologueBird bird = level.Tracker.GetEntity<PrologueBird>();
                if (bird is not null)
                {
                    bird.Enabled = true;
                }
            }
            else if (!Activated)
            {
                Add(new Coroutine(RoomGlitch(level)));
            }

            //RemoveSelf();
        }
        public override void Update()
        {
            base.Update();
            if (Activated)
            {
                Player player = Scene.GetPlayer();
                if(player is not null && !player.Dead && player.CollideCheck<PrologueGlitchBlock>())
                {
                    player.Die(Vector2.Zero);
                }
            }
        }
        public IEnumerator RoomGlitch(Level level)
        {
            Activated = true;
            foreach (PrologueGlitchBlock block in level.Tracker.GetEntities<PrologueGlitchBlock>().OrderByDescending(item => item.X))
            {
                if (PianoModule.SaveData.ActiveGlitchBlocks.Contains(block))
                {
                    yield return null;
                    continue;
                }
                yield return Calc.Random.Range(0.4f, 0.7f);
                block.Activate();

            }
        }

    }
}
