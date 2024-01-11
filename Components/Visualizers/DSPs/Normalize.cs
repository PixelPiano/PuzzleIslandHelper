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
    public class Normalize : AEDSP
    {
        public float Fadetime;
        public float Threshold;
        public float MaxAmp;
        public Normalize(float fadetimeMS, float threshold, float maxAmp) : base(DSP_TYPE.NORMALIZE)
        {
            Fadetime = fadetimeMS;
            Threshold = threshold;
            MaxAmp = maxAmp;
        }
        public Normalize() : this(5000, 0.1f, 20) { }
        public override void SetParams()
        {
            base.SetParams();
            Dsp.setParameterFloat((int)DSP_NORMALIZE.FADETIME, Calc.Clamp(Fadetime, 0, 20000));
            Dsp.setParameterFloat((int)DSP_NORMALIZE.THRESHHOLD, Calc.Clamp(Threshold, 0, 1));
        }

    }

}
