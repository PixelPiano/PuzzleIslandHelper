using FMOD;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs
{
    public class PitchShift : AEDSP
    {
        public float Pitch;
        public float FFTSize;
        public float MaxChannels;
        public PitchShift(float pitch, float fftSize) : base(DSP_TYPE.PITCHSHIFT)
        {
            Pitch = pitch;
            FFTSize = fftSize;
            MaxChannels = 0;
        }
        public PitchShift() : this(1,1024) { }
        public override void SetParams()
        {
            base.SetParams();
            Dsp.setParameterFloat((int)DSP_PITCHSHIFT.PITCH, Calc.Clamp(Pitch, 0.5f, 2));
            Dsp.setParameterFloat((int)DSP_PITCHSHIFT.FFTSIZE, FFTSize);
            Dsp.setParameterFloat((int)DSP_PITCHSHIFT.MAXCHANNELS, Calc.Clamp(MaxChannels, 0, 16));
        }

    }

}
