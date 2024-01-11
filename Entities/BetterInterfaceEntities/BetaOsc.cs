using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Microsoft.Xna.Framework;
using Monocle;
using System;


namespace Celeste.Mod.PuzzleIslandHelper.Entities.BetterInterfaceEntities
{
    [CustomEntity("PuzzleIslandHelper/Osc")]
    [Tracked]
    public class BetaOsc : Entity
    {
        public Components.Visualizers.AudioEffect Sound;
        public FFT FFT;
        public Chorus chorus;
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
        private bool Valid
        {
            get
            {
                if (FFT is null) return false;
                return FFT.Valid;
            }
        }
        private bool FlagState
        {
            get
            {
                if (string.IsNullOrEmpty(flag)) return true;
                if (Scene is not Level level) return false;
                return level.Session.GetFlag(flag);
            }
        }
        public bool SpectrumExists
        {
            get
            {
                return Valid && FFT.Spectrum != null && FFT.Spectrum.Length > 0 && FFT.Spectrum[0].Length >= 0;
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
        public BetaOsc(Vector2 position, string eventName, string flag, float width, float height, Vector2 scale, bool dangerous, bool twoSided, int thickness, Color baseColor, Color peakColor, int depth, int windowSize, FFT.Windows windowType, Types drawType) : base(position)
        {
            Add(Sound = new Components.Visualizers.AudioEffect());
            Sound.AddFFT(FFT = new FFT(windowSize, windowType));
            Sound.AddChorus(chorus = new Chorus(100, 40, 30));
            Sound.AddEcho(new Echo());
            Sound.AddFlange(new Flange());
            EventName = eventName;
            this.flag = flag;
            Type = drawType;
            Collider = new Hitbox(width, height);
            Scale = scale;
            Dangerous = dangerous;
            Thickness = thickness;
            BaseColor = baseColor;
            PeakColor = peakColor;
            Depth = depth;
            TwoSided = twoSided;
        }

        public BetaOsc(EntityData data, Vector2 offset) : this(data.Position + offset, data.Attr("event"), data.Attr("flag"), data.Width, data.Height, new Vector2(data.Float("scaleX"), data.Float("scaleY")), data.Bool("dangerous"), data.Bool("twoSided"), data.Int("thickness"), data.HexColor("baseColor"), data.HexColor("peakColor"), data.Int("depth"), data.Int("fftWindowSize"), data.Enum<FFT.Windows>("windowType"), data.Enum<Types>("drawType"))
        {
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (FlagState)
            {
                Sound.Play(EventName);
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
        public void CheckForPlayer(Vector2 position, Vector2[] signal, float width, float height, Vector2 scale)
        {
            Vector2 s = new Vector2(width / signal.Length, -height) * scale;
            Vector2 offset = position + Vector2.UnitY * (height - 1);
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            for (int i = 1; i < signal.Length; i++)
            {
                if (player.Dead) break;

                Vector2 signalA = signal[i - 1].Abs();
                Vector2 signalB = signal[i].Abs();
                Vector2 start = offset + signalA * s;
                Vector2 end = offset + signalB * s;

                if (player.CollideLine(start, end, player.BottomCenter))
                {
                    player.Die(Vector2.UnitY * scale.Y);
                }
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (Signal is null) return;
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            Vector2 s = new Vector2(Width / Signal.Length, -Height);
            Vector2 offset = Position + Vector2.UnitY * (Height - 1);
            for (int i = 1; i < Signal.Length; i++)
            {
                if (player.Dead) break;

                Vector2 signalA = Signal[i - 1].Abs();
                Vector2 signalB = Signal[i].Abs();
                Vector2 start = offset + signalA * s;
                Vector2 end = offset + signalB * s;

                if (player.CollideLine(start, end, player.BottomCenter))
                {
                    Draw.Circle(player.Center, 8, Color.Magenta, 20);
                }
            }
        }
        public void DrawAt(Vector2 position, float width, float height, Vector2 scale, Color color, bool twoSided = false, int thickness = 1)
        {
            DrawSignal(position, Signal, width, height, scale, color, twoSided, thickness);
        }
        public override void Update()
        {
            base.Update();
            if (!SpectrumExists) return;
            if (Scene is not Level level) return;
            bool flagState = FlagState;
            if (flagState && !Sound.Playing)
            {
                Sound.Play(EventName);
            }
            if (!flagState && Sound.Playing)
            {
                Sound.Stop();
            }
            if (!flagState) return;
            Freq = FFT.DominantFreq;
            Signal = CreateSignal(FFT.Spectrum[0], Freq, out MaxVol);
            if (Dangerous)
            {
                CheckForPlayer(Position, Signal, Width, Height, Scale);
            }
        }
    }
}
