using Celeste.Mod.Entities;
using Celeste.Mod.LuaCutscenes;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes
{
    [CustomEvent("PuzzleIslandHelper/RandyDisappear")]
    [Tracked]
    public class RandyDisappearCutscene : CutsceneEntity
    {
        public FormativeRival Jaques;
        public Player Player;
        public Calidus Calidus;
        //public FestivalTrailer Trailer;
        //public Ghost Ghost;
        public override void OnBegin(Level level)
        {
            Jaques = level.Tracker.GetEntity<FormativeRival>();
            Player = level.GetPlayer();
            Calidus = level.Tracker.GetEntity<Calidus>();
            Player.StateMachine.State = Player.StDummy;
            Add(new Coroutine(cutscene()));
        }
        private IEnumerator cutscene()
        {
            yield return Textbox.Say("FESTIVAL_RIVALS_09", jaquesRunToTrailer, calidusStern);
            yield return null;
        }
        private IEnumerator jaquesRunToTrailer()
        {
            yield return null;
        }
        private IEnumerator calidusStern()
        {
            Calidus?.Emotion(Calidus.Mood.Stern);
            yield return null;
        }
        public override void OnEnd(Level level)
        {
            Player.StateMachine.State = Player.StNormal;
        }
    }
}
