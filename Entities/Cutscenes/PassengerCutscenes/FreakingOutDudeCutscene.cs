
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes.PassengerCutscenes
{
    [CustomPassengerCutscene("FreakingOutDude")]
    [Tracked]
    public class FreakingOutDudeCutscene : PassengerCutscene
    {
        public FreakingOutDudeCutscene(Passenger passenger, Player player) : base(passenger, player)
        {
           
        }

        public override void OnBegin(Level level)
        {
            Player.DisableMovement();
            Add(new Coroutine(cutscene()));
        }
        private IEnumerator cutscene()
        {
            yield return Textbox.Say("FreakingOutDude", Wait1, Wait2, Wait3);
            EndCutscene(Level);
        }
        public override void OnEnd(Level level)
        {
            Level.ResetZoom();
            Player.EnableMovement();
          
        }
    }
}
