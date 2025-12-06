using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [Tracked]
    public class ColorShifter : Component, IEnumerable<Color>, IEnumerable
    {
        public List<Color> Colors = [];
        private List<Color> OriginalColors = [];
        public bool HasBeenAdded;
        public float Rate = 1;
        public float OriginalRate = 1;
        public float TimeLeft;
        public float Duration;
        public float Percent;
        public float Eased;
        public int Index;
        public bool Static;
        public bool Fades;
        public Ease.Easer Easer;
        public Color this[int i]
        {
            get => OriginalColors[i];
            set => OriginalColors[i] = value;
        }
        public Color this[int i, float j]
        {
            get
            {
                return Fades ? Color.Lerp(this[i], this[(i + 1) % OriginalColors.Count], j) : this[i];
            }
        }
        public Color Current => Colors[Index];
        public ColorShifter(params Color[] colors) : this(1, Ease.Linear, colors)
        {
        }
        public ColorShifter(float duration, params Color[] colors) : this(duration, Ease.Linear, colors) { }
        public ColorShifter(float duration, Ease.Easer ease, params Color[] colors) : base(true, true)
        {
            Easer = ease;
            Easer ??= Ease.Linear;
            Colors = [.. colors];
            OriginalColors = [.. colors];
            TimeLeft = Duration = duration;

        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            OriginalRate = Rate;
        }
        public void AdvanceColors()
        {
            if (OriginalColors.Count == 0) return;
            Index = (Index + 1) % Colors.Count;
            TimeLeft = 0;
        }
        public void SetColors(float percent)
        {
            if (OriginalColors.Count == 0) return;
            for (int i = 0; i < Colors.Count; i++)
            {
                Colors[i] = this[i, percent];
            }
        }
        public void Pause()
        {
            Active = false;
        }
        public void Resume()
        {
            Active = true;
        }
        public void Start()
        {
            Index = 0;
            TimeLeft = 0;
            Active = true;
        }
        public void Cancel()
        {
            Index = 0;
            TimeLeft = 0;
            Active = false;
        }
        public override void Update()
        {
            base.Update();
            if (OriginalColors.Count == 0) return;
            TimeLeft += Engine.DeltaTime * Rate;
            if (TimeLeft > Duration)
            {
                AdvanceColors();
            }
            Percent = Fades ? TimeLeft / Duration : 0;
            Eased = Easer(Percent);
            SetColors(Eased);
        }
        public void Insert(int index, Color color)
        {
            if (index > OriginalColors.Count)
            {
                Add(color);
            }
            else if (index <= 0)
            {
                Index++;
                Colors.Insert(0, color);
                OriginalColors.Insert(0, color);
            }
            else
            {
                if (Index > index)
                {
                    Index++;
                }
                Colors.Insert(index, color);
                OriginalColors.Insert(index, color);
            }
        }

        public void DebugDraw(Vector2 pos)
        {
            for (int i = 0; i < OriginalColors.Count; i++)
            {
                Draw.Rect(pos + i * Vector2.UnitX * 8, 8, 8, OriginalColors[i]);
            }
            for (int i = 0; i < Colors.Count; i++)
            {
                Draw.Rect(pos + new Vector2(i * 8, 8), 8, 8, Colors[i]);
            }
            Draw.HollowRect(pos, 24, 16, Color.Red * Eased);
        }

        public void Add(Color color)
        {
            OriginalColors.Add(color);
            Colors.Add(color);
        }
        public void Remove(Color color)
        {
            if (OriginalColors.Count == 0) return;
            int index = OriginalColors.IndexOf(color);
            if (index != -1)
            {
                OriginalColors.RemoveAt(index);
                Colors.RemoveAt(index);
                if (Index >= index)
                {
                    Index = Math.Max(Index - 1, 0);
                }

            }
        }
        IEnumerator<Color> IEnumerable<Color>.GetEnumerator()
        {
            return ((IEnumerable<Color>)Colors).GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return Colors.GetEnumerator();
        }
    }
}
