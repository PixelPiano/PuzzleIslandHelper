﻿using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class DiamondPulseLines : Component
    {
        public static readonly Vector2[] LineCache =
        {
            new(-1, 0), new(0, -1), new(1, 0), new(0,1)
        };
        public Vector2[] Lines;
        public Vector2 Scale = Vector2.One;
        public Vector2 Position;
        public float LineScaleAddition;
        public float Alpha = 1;
        public int Size;
        public float Timer;

        public float PulseTime;
        public float Expand;
        public float FadeTime;
        public Vector2 RenderPosition
        {
            get => (Entity?.Position ?? Vector2.Zero) + Position;
            set => Position = value - (Entity?.Position ?? Vector2.Zero);
        }
        public bool On;
        public DiamondPulseLines(int size, float expand, float time, float fadeTime, bool start = true) : base(true, true)
        {
            Size = size;
            Lines = new Vector2[LineCache.Length];
            LineCache.CopyTo(Lines, 0);
            if (start)
            {
                Start(time, expand, fadeTime);
            }
        }
        public DiamondPulseLines(int size) : this(size, 0, 0, 0, false) { }

        public void Start(float time, float expand, float fadeTime)
        {
            On = true;
            Expand = expand;
            PulseTime = time;
            Timer = 0;
            FadeTime = fadeTime;
        }
        public override void Update()
        {
            base.Update();
            Visible = On;
            if (On)
            {
                bool timeUp = Timer >= PulseTime;
                float amount = Timer / PulseTime;

                Timer = timeUp ? PulseTime : Timer + Engine.DeltaTime;
                LineScaleAddition = timeUp ? Expand : Calc.LerpClamp(0, Expand, amount);

                if(Timer >= PulseTime - FadeTime)
                {
                    Alpha = Calc.Clamp(Alpha - (Engine.DeltaTime / FadeTime), 0, 1);
                }
                Vector2 scaleOffset = Scale * ((Size / 2) + LineScaleAddition);
                for (int i = 0; i < LineCache.Length; i++)
                {
                    Lines[i] = (LineCache[i] * scaleOffset) + RenderPosition;
                };
            }
        }
        public override void Render()
        {
            base.Render();
            DrawPulseLines();
        }

        public void DrawPulseLines()
        {
            Vector2 start = Lines[^1];//what
            for (int i = 0; i < LineCache.Length; i++)
            {
                Vector2 end = Lines[i];
                Draw.Line(start, end, Color.White * Alpha);
                start = end;
            }
        }
    }
}
