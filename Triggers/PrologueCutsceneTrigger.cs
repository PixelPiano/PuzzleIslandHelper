using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes.Prologue;
using Microsoft.Xna.Framework;
using Monocle;

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


            //EjectSelf();
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


    }
}
