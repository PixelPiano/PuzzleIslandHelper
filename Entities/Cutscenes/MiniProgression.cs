using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes
{
    [CustomPassengerCutscene("MiniProgression")]
    [Tracked]
    public class MiniProgression : PassengerCutscene
    {
        public MiniProgression(Passenger passenger, Player player, params string[] args) : base(passenger, player, args)
        {
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(cutscene()));
        }
        private IEnumerator cutscene()
        {
            if(Args != null && Args.Length > 0)
            {
                if(int.TryParse(Args[0],out int result))
                {
                    switch (result)
                    {

                    }
                }
            }
            yield return null;
            EndCutscene(Level);
        }
        public override void OnEnd(Level level)
        {
        }
    }
}
