using FMOD;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs
{
    public class Chorus : AEDSP
    {
        public float Mix;
        public float Rate;
        public float Depth;
        public Chorus(float mix, float rate, float depth) : base(DSP_TYPE.CHORUS)
        {
            Mix = mix;
            Rate = rate;
            Depth = depth;
        }
        public Chorus() : this(50, 0.8f, 3) { }
        public override void SetParams()
        {
            base.SetParams();
            Dsp.setParameterFloat((int)DSP_CHORUS.MIX, Calc.Clamp(Mix, 0, 100));
            Dsp.setParameterFloat((int)DSP_CHORUS.RATE, Calc.Clamp(Rate, 0, 20));
            Dsp.setParameterFloat((int)DSP_CHORUS.DEPTH, Calc.Clamp(Depth, 0, 100));
        }

    }

}
