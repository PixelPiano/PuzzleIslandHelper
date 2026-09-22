using System;
using Celeste.Mod.CommunalHelper;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    public class Flicker : Component
    {
        public int StartIndex;
        public int Index;
        public float Timer;
        public float Interval;
        public Color[] Colors;
        public int Increment = 1;
        public Color Color => Colors[Index];
        public Flicker(float interval, int startIndex = 0, bool start = true, params Color[] colors) : base(false, false)
        {
            if (colors == null || colors.Length == 0)
            {
                throw new ArgumentException("colors array must not be null and must have at least one entry!");
            }
            Colors = colors;
            Interval = interval;
            Index = startIndex % colors.Length;
            if (start)
            {
                Start();
            }
        }
        public Color GetColor(int index)
        {
            return Colors[index % Colors.Length];
        }
        public override void Update()
        {
            base.Update();
            Timer -= Engine.DeltaTime;
            if (Timer < 0)
            {
                Index += Increment;
                switch (Increment)
                {
                    case > 0:
                        if (Index >= Colors.Length) Index = Index % Colors.Length;
                        break;
                    default:
                        if (Index < 0) Index = Colors.Length - (Math.Abs(Index) % Colors.Length);
                        break;
                }
                Timer = Interval;
            }
        }
        public void Start()
        {
            Active = true;
            Timer = Interval;
            Index = StartIndex;
        }
        public void Stop()
        {
            Active = false;
        }
    }
}
