using Celeste.Mod.PuzzleIslandHelper.Entities.WIP;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using static Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities.PlayerCalidus;
using Looking = Celeste.Mod.PuzzleIslandHelper.Entities.Calidus.Looking;
using Mood = Celeste.Mod.PuzzleIslandHelper.Entities.Calidus.Mood;
using Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WARP;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.CapsuleCutscenes
{
    [Tracked]
    [CustomEvent("PuzzleIslandHelper/Capsule/BetaPull")]
    public class BetaPull : WarpCapsuleCutscene
    {
        public BetaPull(EventTrigger trigger, Player player, string eventID) : base(trigger, player, eventID)
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
            IEnumerator waithalfsecond() { yield return 0.5f; }
            IEnumerator wait1() { yield return 1; }
            yield return Textbox.Say("Calidus1a", waithalfsecond, wait1);
            yield return Level.ZoomBack(1);
            Audio.PauseMusic = false;
            EndCutscene(Level);
        }
        public override void OnEnd(Level level)
        {
            base.OnEnd(level);
            Level.ResetZoom();
            Audio.PauseMusic = false;
        }
    }
}