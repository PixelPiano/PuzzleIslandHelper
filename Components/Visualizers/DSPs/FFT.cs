using FMOD;
using Monocle;
using System;
using System.Runtime.InteropServices;

namespace Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs
{
    public class FFT : AEDSP
    {
        public const int MinWindowSize = 128;
        public const int MaxWindowSize = 8192;
        public int WindowSize;
        public Windows Window;
        public DSP_PARAMETER_FFT Parameter;
        public float[][] Spectrum;
        public enum Windows
        {
            Rect,
            Triangle,
            Hamming,
            Hanning,
            Blackman,
            Blackmanharris
        }
        public float DominantFreq
        {
            get
            {
                if (!Valid) return 1;
                Dsp.getParameterFloat((int)DSP_FFT.DOMINANT_FREQ, out float value);
                return value;

            }
        }
        public FFT(int windowSize = 128, Windows window = Windows.Rect) : base(DSP_TYPE.FFT)
        {
            WindowSize = Calc.Clamp(windowSize, MinWindowSize, MaxWindowSize);
            Window = window;
        }
        public override void SetParams()
        {
            base.SetParams();
            Dsp.getParameterData((int)DSP_FFT.SPECTRUMDATA, out IntPtr unmanagedData, out uint length);
            Parameter = (DSP_PARAMETER_FFT)Marshal.PtrToStructure(unmanagedData, typeof(DSP_PARAMETER_FFT));
            Spectrum = Parameter.spectrum;
            Dsp.setParameterInt((int)DSP_FFT.WINDOWTYPE, (int)Window);
            Dsp.setParameterInt((int)DSP_FFT.WINDOWSIZE, WindowSize);
        }

    }

}
