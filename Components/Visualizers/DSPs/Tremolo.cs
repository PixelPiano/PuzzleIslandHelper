using FMOD;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs
{
    public class Tremolo : AEDSP
    {
        public float Freq;
        public float Depth;
        public float Shape;
        public float Skew;
        public float Duty;
        public float Flatness;
        public float Phase;
        public float Spread;
        public Tremolo(float lfoFreq, float depth, float lfoShape, float lfoSkew, float lfoDuty, float lfoFlatness, float lfoPhase, float spread) : base(DSP_TYPE.TREMOLO)
        {
            Freq = lfoFreq;
            Depth = depth;
            Shape = lfoShape;
            Skew = lfoSkew;
            Duty = lfoDuty;
            Flatness = lfoFlatness;
            Phase = lfoPhase;
            Spread = spread;
        }
        public Tremolo() : this(5, 1, 1, 0, 0.5f, 1, 0, 0) { }
        public override void SetParams()
        {
            base.SetParams();
            Dsp.setParameterFloat((int)DSP_TREMOLO.FREQUENCY, Calc.Clamp(Freq, 0.1f, 20));
            Dsp.setParameterFloat((int)DSP_TREMOLO.DEPTH, Calc.Clamp(Depth, 0, 1));
            Dsp.setParameterFloat((int)DSP_TREMOLO.SHAPE, Calc.Clamp(Shape, 0, 1));
            Dsp.setParameterFloat((int)DSP_TREMOLO.SKEW, Calc.Clamp(Skew, -1, 1));
            Dsp.setParameterFloat((int)DSP_TREMOLO.DUTY, Calc.Clamp(Duty, 0, 1));
            Dsp.setParameterFloat((int)DSP_TREMOLO.SQUARE, Calc.Clamp(Flatness, 0, 1));
            Dsp.setParameterFloat((int)DSP_TREMOLO.PHASE, Calc.Clamp(Phase, 0, 1));
            Dsp.setParameterFloat((int)DSP_TREMOLO.SPREAD, Calc.Clamp(Spread, -1, 1));

        }

    }

}
