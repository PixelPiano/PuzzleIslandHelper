using FMOD;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs
{
    public class Flange : AEDSP
    {
        public float Mix;
        public float Depth;
        public float Rate;
        public Flange(float mixPercent, float depthPercent, float rateHz) : base(DSP_TYPE.FLANGE)
        {
            Mix = mixPercent;
            Depth = depthPercent;
            Rate = rateHz;
        }
        public Flange() : this(50, 1, 0.1f) { }
        public override void SetParams()
        {
            base.SetParams();
            Dsp.setParameterFloat((int)DSP_FLANGE.MIX, Calc.Clamp(Mix, 0, 100));
            Dsp.setParameterFloat((int)DSP_FLANGE.DEPTH, Calc.Clamp(Depth, 0.01f, 1));
            Dsp.setParameterFloat((int)DSP_FLANGE.RATE, Calc.Clamp(Rate, 0, 20));
        }

    }

}
