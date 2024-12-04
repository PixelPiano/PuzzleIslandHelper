using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [Tracked]
    public class ColorShifter : Component
    {
        public Color[] Colors;
        private Color[] OriginalColors;
        public bool HasBeenAdded;
        public float Rate = 1;
        public float TimeLeft;
        public float Duration;
        public float Percent;
        public float Eased;
        public int NextIndex;
        public Ease.Easer Easer;
        public Color this[int i]
        {
            get
            {
                return Colors[i];
            }
        }
        public Color Value => Colors[0];
        public ColorShifter(params Color[] colors) : this(1, Ease.Linear, colors)
        {
        }
        public ColorShifter(float duration, Ease.Easer ease, params Color[] colors) : base(true, true)
        {
            if (colors != null && colors.Length == 0)
            {
                return;
            }
            Easer = ease;
            Easer ??= Ease.Linear;
            Colors = new Color[colors.Length];
            OriginalColors = new Color[colors.Length];
            for (int i = 0; i < Colors.Length; i++)
            {
                Colors[i] = colors[i];
                OriginalColors[i] = colors[i];
            }
            TimeLeft = Duration = duration;

        }
        public void AdvanceColors()
        {
            NextIndex = (NextIndex + 1) % Colors.Length;
            TimeLeft = 0;
        }
        public void SetColors(float percent)
        {
            int next = NextIndex;
            int start = NextIndex - 1;
            if (start < 0) start = Colors.Length - 1;
            for (int i = 0; i < Colors.Length; i++)
            {
                Colors[i] = Color.Lerp(OriginalColors[start], OriginalColors[next], percent);
                start = next;
                next++;
                next %= Colors.Length;
            }
        }
        public override void Update()
        {
            base.Update();
            TimeLeft += Engine.DeltaTime * Rate;
            if (TimeLeft > Duration)
            {
                AdvanceColors();
            }
            Percent = TimeLeft / Duration;
            Eased = Easer(Percent);
            SetColors(Eased);
        }
        public void DebugDraw(Vector2 pos)
        {
            for (int i = 0; i < OriginalColors.Length; i++)
            {
                Draw.Rect(pos + i * Vector2.UnitX * 8, 8, 8, OriginalColors[i]);
            }
            for (int i = 0; i < Colors.Length; i++)
            {
                Draw.Rect(pos + new Vector2(i * 8, 8), 8, 8, Colors[i]);
            }
            Draw.HollowRect(pos, 24, 16, Color.Red * Eased);
        }
    }
}
