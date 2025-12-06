using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Microsoft.Xna.Framework;
using Monocle;
using System;


namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Osc")]
    [Tracked]
    public class Osc : Entity
    {
        public AudioEffect Sound;
        public FFT FFT;
        public Vector2 Scale = new Vector2(1, 1);
        public float Freq;
        public float MaxVol;
        public int Interval = 5;
        public bool Dangerous;
        public Color BaseColor = Color.White;
        public Color PeakColor = Color.Red;
        public bool TwoSided;
        public int Thickness;
        public int WindowSize;
        public FFT.Windows WindowType;
        private string flag;
        public string EventName;

        private bool FlagState
        {
            get
            {
                if (string.IsNullOrEmpty(flag)) return true;
                if (Scene is not Level level) return false;
                return level.Session.GetFlag(flag);
            }
        }
        public Vector2[] Signal;
        public enum Types
        {
            Surface,
            Full,
            Dots,
            Bars
        }
        public Types Type = Types.Surface;

        //DEBUGGING VARIABLES
        private bool InstanceIsNull => Sound.instance is null;
        private bool FFTExists => FFT is not null; //true
        private bool DSPExists => FFTExists && FFT.Dsp is not null; //true
        private bool Injected => DSPExists && FFT.Injected; //false, FFT not injected

        private bool SignalExists => Signal is not null;
        public bool SpectrumExists => Injected && FFT.Spectrum is not null;
        public bool SpectrumIsPopulated => SpectrumExists && FFT.Spectrum.Length > 0 && FFT.Spectrum[0].Length >= 0;
        public Osc(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            WindowSize = data.Int("fftWindowSize");
            WindowType = data.Enum<FFT.Windows>("windowType");
            Type = data.Enum<Types>("drawType");
            EventName = data.Attr("event");
            flag = data.Attr("flag");
            Thickness = data.Int("thickness");
            Depth = data.Int("depth");
            Collider = new Hitbox(data.Width, data.Height);
            Scale = new Vector2(data.Float("scaleX"), data.Float("scaleY"));
            Dangerous = data.Bool("dangerous");
            TwoSided = data.Bool("twoSided");
            BaseColor = data.HexColor("baseColor");
            PeakColor = data.HexColor("peakColor");
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Add(Sound = new AudioEffect(FFT = new FFT(WindowSize, WindowType)));
            if (FlagState)
            {
                Sound.PlayEvent(EventName);
            }
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level) return;
            bool flagState = FlagState;
            if (flagState && !Sound.Playing)
            {
                Sound.PlayEvent(EventName);
            }

            if (!flagState)
            {
                if (Sound.Playing)
                {
                    Sound.StopEvent();
                }
                return;
            }
            if (!SpectrumExists || !SpectrumIsPopulated) return;
            Freq = FFT.DominantFreq;
            Signal = CreateSignal(FFT.Spectrum[0], Freq, out MaxVol);
            if (Dangerous && level.GetPlayer() is Player player && PlayerCollide(player, Signal))
            {
                player.Die(Vector2.UnitY * Scale.Y);
            }
        }
        private Vector2[] CreateSignal(float[] spectrum, float freq, out float maxVol)
        {
            maxVol = 0;
            Vector2[] signal = new Vector2[(int)Width];
            for (int i = 0; i < Width; i++)
            {
                float wave = (float)(spectrum[i] * Math.Cos(2 * Math.PI * freq * Sound.TimePlaying));
                signal[i] = new Vector2(i, wave);
                if (spectrum[i] > maxVol) maxVol = spectrum[i];
            }
            return signal;
        }

        private void DrawSignal(Vector2 position, Vector2[] signal, float width, float height, Vector2 scale, Color color, bool twoSided, int thickness)
        {
            Vector2 offset = position + Vector2.UnitY * (height - 1);
            Vector2 s = new Vector2(signal.Length / width, -height) * scale;
            for (int i = 1; i < signal.Length; i++)
            {
                Vector2 signalA = signal[i - 1].Abs();
                Vector2 signalB = signal[i].Abs();
                Vector2 start = offset + signalA * s;
                Vector2 end = offset + signalB * s;
                switch (Type)
                {
                    case Types.Surface: Draw.Line(start, end, color, thickness); break;
                    case Types.Dots: Draw.Point(end, color); break;
                    case Types.Full: Draw.Line(end, new Vector2(end.X, position.Y + height), color, thickness); break;
                }
                if (twoSided)
                {
                    start = offset + signalA * new Vector2(s.X, -s.Y);
                    end = offset + signalB * new Vector2(s.X, -s.Y);
                    switch (Type)
                    {
                        case Types.Surface: Draw.Line(start, end, color, thickness); break;
                        case Types.Dots: Draw.Point(end, color); break;
                        case Types.Full: Draw.Line(end, new Vector2(end.X, position.Y + height), color, thickness); break;
                    }
                }
            }
        }
        public override void Render()
        {
            base.Render();
            if (!SpectrumExists || Signal == null || !FlagState) return;

            Color color = Color.Lerp(BaseColor, PeakColor, MaxVol);
            DrawSignal(Position, Signal, Width, Height, Scale, color, TwoSided, Thickness);
        }
        public void CheckForPlayer()
        {
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            if (PlayerCollide(player, Signal))
            {
                player.Die(Vector2.UnitY * Scale.Y);
            }
        }
        public bool PlayerCollide(Player player, Vector2[] signal)
        {
            if (player is null || player.Dead || signal is null) return false;
            Vector2 s = new Vector2(Width / signal.Length, -Height) * Scale;
            Vector2 offset = Position + Vector2.UnitY * (Height - 1);
            for (int i = 1; i < signal.Length; i++)
            {
                Vector2 signalA = signal[i - 1].Abs();
                Vector2 signalB = signal[i].Abs();
                Vector2 start = offset + signalA * s;
                Vector2 end = offset + signalB * s;

                if (player.CollideLine(start, end, player.BottomCenter))
                {
                    return true;
                }
            }
            return false;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            if (PlayerCollide(player, Signal))
            {
                Draw.Circle(player.Center, 8, Color.Magenta, 20);
            }
        }
        public void DrawAt(Vector2 position, float width, float height, Vector2 scale, Color color, bool twoSided = false, int thickness = 1)
        {
            DrawSignal(position, Signal, width, height, scale, color, twoSided, thickness);
        }

    }
}
