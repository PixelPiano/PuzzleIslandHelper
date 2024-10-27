using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [Tracked]
    public abstract class PassengerCutscene : CutsceneEntity
    {
        public Passenger Passenger;
        public Player Player;
        public PassengerCutscene(Passenger passenger, Player player) : base()
        {
            Passenger = passenger;
            Player = player;
        }
    }

    [CustomPassengerCutscene("TestCutscene")]
    public class Testtt : PassengerCutscene
    {
        public Testtt(Passenger passenger, Player player) : base(passenger, player)
        {
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(cutscene()));
        }
        private IEnumerator cutscene()
        {
            yield return Textbox.Say("cutscene test");
            EndCutscene(Level);
        }
        public override void OnEnd(Level level)
        {
            
        }
    }
}