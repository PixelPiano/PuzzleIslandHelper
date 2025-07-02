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
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.CapsuleCutscenes
{
    [Tracked]
    [CustomEvent("PuzzleIslandHelper/Capsule/BetaFail")]
    public class BetaFail : WarpCapsuleCutscene
    {
        public BetaFail(EventTrigger trigger, Player player, string eventID) : base(trigger, player, eventID)
        {

        }
        public override void OnBegin(Level level)
        {
            base.OnBegin(level);
            if (Intro)
            {
                Add(new Coroutine(introRoutine()));
            }
            else if(level.GetPlayer() is Player player)
            {
                Add(new Coroutine(outroRoutine(player)));
            }
        }
        private IEnumerator outroRoutine(Player player)
        {
            yield return player.DummyWalkToExact((int)player.X - 32);
            IEnumerator lookRight()
            {
                player.Facing = Facings.Right;
                yield return 0.5f;
            }
            yield return Textbox.Say("CalidusLeftBehindA", lookRight);
            EndCutscene(Level);
        }
        private IEnumerator introRoutine()
        {
            if (Level.GetPlayer() is Player player)
            {
                yield return playerToCenter(player);
            }
            if (Level.Tracker.GetEntity<Calidus>() is Calidus c && c.Following)
            {
                yield return calidusToCenter(c);
            }
            EndCutscene(Level);
        }
        public override void OnEnd(Level level)
        {
            base.OnEnd(level);
            if (!Intro)
            {
                if (WasSkipped)
                {
                    if (Level.GetPlayer() is Player player)
                    {
                        snapPlayer(player);
                    }
                    if (Level.Tracker.GetEntity<Calidus>() is Calidus c && c.Following)
                    {
                        snapCalidus(c);
                    }
                }
                CalCut.SecondTryWarp.Register();
            }
        }
    }
}