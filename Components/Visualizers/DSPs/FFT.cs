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
    public class FFT : AEDSP
    {
        public const int MinWindowSize = 128;
        public const int MaxWindowSize = 8192;
        public int WindowSize;
        public Windows Window;
        public enum Windows
        {
            Rect,
            Triangle,
            Hamming,
            Hanning,
            Blackman,
            Blackmanharris
        }
        public DSP_PARAMETER_FFT Parameter;
        public float DominantFreq
        {
            get
            {
                if (!Valid) return 1;
                Dsp.getParameterFloat((int)DSP_FFT.DOMINANT_FREQ, out float value);
                return value;

            }
        }
        public float[][] Spectrum
        {
            get
            {
                return Parameter.spectrum;
            }
        }
        public FFT() : this(128, Windows.Rect)
        {
        }
        public FFT(int size, Windows window) : this(size)
        {

            Window = window;
        }
        public FFT(Windows window) : this(128)
        {
            Window = window;
        }
        public FFT(int windowSize) : base(DSP_TYPE.FFT)
        {
            WindowSize = Calc.Clamp(windowSize, MinWindowSize, MaxWindowSize);
        }
        public override void SetParams()
        {
            base.SetParams();
            Dsp.getParameterData((int)DSP_FFT.SPECTRUMDATA, out IntPtr unmanagedData, out uint length);
            Parameter = (DSP_PARAMETER_FFT)Marshal.PtrToStructure(unmanagedData, typeof(DSP_PARAMETER_FFT));
            Dsp.setParameterInt((int)DSP_FFT.WINDOWTYPE, (int)Window);
            Dsp.setParameterInt((int)DSP_FFT.WINDOWSIZE, WindowSize);
        }

    }

}
