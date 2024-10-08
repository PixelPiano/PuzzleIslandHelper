
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs
{
    public class FreqGetter : AudioEffect
    {
        public float[] TargetParams = new float[6];
        public float[] Params = new float[6];
        public FreqGetter() : base(true)
        {

        }
        public override void Update()
        {
            base.Update();

            for (int i = 0; i < 6; i++)
            {
                Params[i] = Calc.Approach(Params[i], TargetParams[i], 30f * Engine.DeltaTime);
                Param("" + (i + 1), Params[i]);
            }


            /*
             * 1: Offset inside a level?
             * 2: X position YAmount?
             * 3: Y position YAmount?
             * 4: Level has void area?
             * 5: Level has Dash Code?
             * 6: 
             */

        }
        public void Start()
        {
            PlayEvent("event:/PianoBoy/Soundwaves/static_noise");
        }
        public void Stop()
        {
            StopEvent();
        }
    }
}