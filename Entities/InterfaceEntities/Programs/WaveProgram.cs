
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using ExtendedVariants.Entities.ForMappers;
using ExtendedVariants.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Programs
{

    [TrackedAs(typeof(WindowContent))]
    [CustomProgram("Wave")]
    public class WaveProgram : WindowContent
    {
        public AudioEffect Sound;
        public List<Vector2> Points = new();
        public Vector2[] Signal;
        public Vector2 MaxPoint;
        public List<Vector2> Output = new();
        public float XOffset;
        public List<FreqPlayback> Buttons = new();
        public BetterButton UpButton;
        public BetterButton DownButton;
        public Vector2 Padding = Vector2.One * 4;
        private float totalHeight;
        private float scrollOffset;
        private float waveWidth => Width / 2;
        private float waveHeight => Height / 8;
        public Vector2 WavePosition => Position + Collider.HalfSize - Vector2.UnitX;
        public static MTexture Wavetable = GFX.Game["objects/PuzzleIslandHelper/interface/wavetable"];
        public float Volume = 1;
        public float ScrollSpeed = 30f * Engine.DeltaTime;

        public Slider Slider;
        public WaveProgram(BetterWindow window) : base(window)
        {
            Name = "Wave";
            Sound = new AudioEffect(true);
            Add(Sound);
        }
        private string grassShiftText()
        {
            if (Scene is not Level level) return "";
            TimeSpan t = TimeSpan.FromSeconds(level.TimeActive);

            string answer = string.Format("{0:D2}:{1:D2}:{2:D2}",
                            t.Hours,
                            t.Minutes,
                            t.Seconds);
            return answer;
        }
        public override void OnOpened(BetterWindow window)
        {
            base.OnOpened(window);
            DownButton.Position.X = UpButton.Position.X = Buttons[0].Position.X + Buttons[0].Width + Padding.X;
            DownButton.Position.Y = window.CaseHeight - DownButton.Height - Padding.Y;
            UpButton.Position.Y += 4;
            DownButton.ImageOffset = DownButton.HalfArea;
            Slider.Position = DownButton.Position + Vector2.UnitX * 40;

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            string[] events = new string[] { "maze", "digi_test_1", "digi_test_2", "digi_test_3", "GrassShift", "inversion_analysis", "replication_test", "world_shift_analysis", "maze_to_void", "a", "a", "aaaa", "aaaaaaaaaa" };

            for (int i = 0; i < events.Length; i++)
            {
                Func<string> text = events[i] == "GrassShift" ? grassShiftText : null;
                string theEvent = "event:/PianoBoy/Soundwaves/Program/" + events[i];
                FreqPlayback button = new FreqPlayback(this, theEvent, text);
                Buttons.Add(button);
                ProgramComponents.Add(button);
            }
            float x = Padding.X, y = Padding.Y;
            for (int i = 0; i < Buttons.Count; i++)
            {
                Buttons[i].Position.Y = y;
                Buttons[i].Position.X = x;

                Buttons[i].origPosition = Buttons[i].Position;

                y += Buttons[i].Height + Padding.Y;
                totalHeight = y;
            }
            UpButton = new BetterButton(Window, "arrow");
            DownButton = new BetterButton(Window, "arrow");
            DownButton.Outline = UpButton.Outline = true;
            DownButton.CenterOrigin();
            DownButton.Rotation = 180f.ToRad();
            Slider = new Slider(Window, 80, 0, 2, 1);
            Slider.BoxIn = true;
            Slider.BoxColor = Color.DarkBlue;
            ProgramComponents.Add(UpButton);
            ProgramComponents.Add(DownButton);
            ProgramComponents.Add(Slider);

        }
        public void PlaySound(FreqPlayback from)
        {
            Add(new Coroutine(SoundRoutine(from.EventName)));
        }
        public void StopSound()
        {
            Sound.StopEvent();
        }
        private IEnumerator SoundRoutine(string path)
        {
            Sound.StopEvent();
            yield return null;
            Sound.PlayEvent(path);
        }

        public override void OnClosed(BetterWindow window)
        {
            base.OnClosed(window);
            Sound.StopEvent();
        }
        public override void Update()
        {
            scrollOffset += (UpButton.Pressing ? -1 : DownButton.Pressing ? 1 : 0) * ScrollSpeed;
            scrollOffset = Calc.Clamp(scrollOffset, 0, Math.Max(totalHeight - Window.CaseHeight, 0));
            foreach (FreqPlayback button in Buttons)
            {
                button.Y = button.origPosition.Y - scrollOffset;
            }
            Volume = Slider.Value;
            base.Update();
            if (Sound.Playing)
            {
                Sound.SetVolume(Volume);
                if (Scene.OnInterval(3f / 60))
                {
                    GetBuffer(WavePosition, 1, waveHeight, waveWidth, new Vector2(waveWidth / Sound.BufferLength, 0.7f));
                }
            }

        }
        public void GetBuffer(Vector2 position, float spacing, float height, float width, Vector2 scale)
        {
            height *= 2;
            Points.Clear();
            scale.Y *= Volume;
            int perPixel = (int)Math.Max(Math.Floor(Sound.BufferLength / width), 1);
            if (Sound is null || Sound.BufferLength <= 0) return;
            Vector2 start = Vector2.Zero;
            float loudest = 0;
            for (int j = 0; j < Sound.BufferLength; j += perPixel)
            {
                start.X = j * spacing;
                start.Y = 0;

                for (int i = 0; i < Sound.Channels; i++)
                {
                    start.Y += Sound.DataBuffer[(j * Sound.Channels) + i];
                }
                start.Y /= Sound.Channels;
                loudest = Math.Max(loudest, Math.Abs(start.Y));
                Points.Add(start);
            }
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] = new Vector2(Points[i].X, Points[i].Y / loudest * height) * scale;
                Points[i] += position;
            }
        }
        public override void WindowRender()
        {
            base.WindowRender();
            if (Sound.Playing)
            {
                DrawWave(Color.Black, 3, Vector2.UnitY * 2);
                DrawWave(Interface.NightMode ? Color.Green : Color.Red, 2);
            }

        }
        private void DrawWave(Color color, int thickness, Vector2 offset = default)
        {
            for (int i = 1; i < Points.Count; i++)
            {
                Draw.Line((Points[i - 1] + offset).Floor(), (Points[i] + offset).Floor(), color, thickness);
            }
        }
    }
}