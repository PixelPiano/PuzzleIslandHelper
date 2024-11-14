using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
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
        public bool GetFlag(string flag)
        {
            return Level.Session.GetFlag(flag);
        }
        public void SetFlag(string flag, bool value = true)
        {
            Level.Session.SetFlag(flag, value);
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