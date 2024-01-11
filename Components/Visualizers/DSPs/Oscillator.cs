using FMOD;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs
{
    public class Osc : AEDSP
    {
        public enum Wave
        {
            Sine,
            Square,
            Sawup,
            Sawdown,
            Triangle,
            Noise
        }
        public float Rate
        {
            get
            {
                return rate;
            }
            set
            {
                rate = Calc.Clamp(value, 0, 22000);
            }
        }
        private float rate = 220;
        public Wave WaveType;
        public Osc(Wave wave, float rate) : base(DSP_TYPE.OSCILLATOR)
        {
            WaveType = wave;
            Rate = rate;
        }
        public Osc() : this(Wave.Sine, 220) { }
        public override void SetParams()
        {
            base.SetParams();
            Dsp.setParameterInt((int)DSP_OSCILLATOR.TYPE, (int)WaveType);
            Dsp.setParameterFloat((int)DSP_OSCILLATOR.RATE, Rate);
        }

    }

}
