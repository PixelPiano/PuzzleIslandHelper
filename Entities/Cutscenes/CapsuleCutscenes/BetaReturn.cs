using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using Celeste.Mod.PuzzleIslandHelper.Entities.WARP;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.CapsuleCutscenes
{
    [Tracked]
    [CustomEvent("PuzzleIslandHelper/Capsule/BetaReturn")]
    public class BetaReturn : WarpCapsuleCutscene
    {
        public BetaReturn(EventTrigger trigger, Player player, string eventID) : base(trigger, player, eventID)
        {

        }
        public override void OnBegin(Level level)
        {
            base.OnBegin(level);
            if (!Intro)
            {
                Add(new Coroutine(Outro()));
            }
        }
        public IEnumerator Outro()
        {
            yield return Textbox.Say("CalidusLeftBehindB");
            EndCutscene(Level);
        }
        public override void OnEnd(Level level)
        {
            base.OnEnd(level);
            if (!Intro)
            {
                CalCut.SecondTalkAboutWarp.Register();
            }
        }
    }
}