using FMOD;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs
{
    public class Distortion : AEDSP
    {
        public float Level;
        public Distortion(float levelLinear) : base(DSP_TYPE.DISTORTION)
        {
            Level = levelLinear;
        }
        public Distortion() : this(0.5f) { }
        public override void SetParams()
        {
            base.SetParams();
            Dsp.setParameterFloat((int)DSP_DISTORTION.LEVEL, Calc.Clamp(Level, 0, 1));
        }

    }

}
