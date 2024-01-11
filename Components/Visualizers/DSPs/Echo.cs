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
    public class Echo : AEDSP
    {
        public float Delay;
        public float Feedback;
        public float DryLevel;
        public float WetLevel;
        public Echo(float delay, float feedback, float drylevel, float wetlevel) : base(DSP_TYPE.ECHO)
        {
            Delay = delay;
            Feedback = feedback;
            DryLevel = drylevel;
            WetLevel = wetlevel;
        }
        public Echo() : this(500, 50, 0, 0) { }
        public override void SetParams()
        {
            base.SetParams();
            Dsp.setParameterFloat((int)DSP_ECHO.DELAY, Calc.Clamp(Delay, 10, 5000));
            Dsp.setParameterFloat((int)DSP_ECHO.FEEDBACK, Calc.Clamp(Feedback, 0, 100f));
            Dsp.setParameterFloat((int)DSP_ECHO.DRYLEVEL, Calc.Clamp(DryLevel, -80, 10));
            Dsp.setParameterFloat((int)DSP_ECHO.WETLEVEL, Calc.Clamp(WetLevel, -80, 10));
        }

    }

}
