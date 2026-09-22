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
        public float Timer;
        public float Duration;
        public float Percent;
        public float Eased;
        public int Index;
        public bool Static;
        public bool Fades;
        public Ease.Easer Easer;
        public Color this[int index]
        {
            get => OriginalColors[index];
            set => OriginalColors[index] = value;
        }
        public Color this[int index, float lerp]
        {
            get
            {
                if (!Fades) return this[index];
                return Color.Lerp(this[index], this[(index + 1) % OriginalColors.Count], Math.Min(1, lerp));
            }
        }
        public Color Current => Colors[Index];
        public ColorShifter(params Color[] colors) : this(1, Ease.Linear, colors)
        {
        }
        public ColorShifter(float duration, params Color[] colors) : this(duration, Ease.Linear, colors) { }
        public ColorShifter(float duration, Ease.Easer ease, params Color[] colors) : base(true, true)
        {
            Easer = ease ?? Ease.Linear;
            Colors = [.. colors];
            OriginalColors = [.. colors];
            Timer = Duration = duration;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            OriginalRate = Rate;
        }
        public void NextColor()
        {
            if (OriginalColors.Count == 0) return;
            Index = (Index + 1) % Colors.Count;
            Timer = 0;
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
            Timer = 0;
            Active = true;
        }
        public void Cancel()
        {
            Index = 0;
            Timer = 0;
            Active = false;
        }
        public override void Update()
        {
            base.Update();
            Advance();
        }
        public Color GetOGColor(float progress)
        {
            if ((int)progress == progress)
            {
                return OriginalColors[(int)progress % OriginalColors.Count];
            }
            int indexA = (int)(progress % 1) % OriginalColors.Count;
            int indexB = (indexA + 1) % OriginalColors.Count;
            return Color.Lerp(OriginalColors[indexA], OriginalColors[indexB], progress - (int)progress);
        }
        public Color GetColor(float progress)
        {
            if ((int)progress == progress)
            {
                return Colors[(int)progress % Colors.Count];
            }
            int indexA = (int)(progress % 1) % Colors.Count;
            int indexB = (indexA + 1) % Colors.Count;
            return Color.Lerp(Colors[indexA], Colors[indexB], progress - (int)progress);
        }
        public void Advance()
        {
            if (OriginalColors.Count == 0 || Rate <= 0) return;
            Timer += Engine.DeltaTime * Rate;
            if (Timer > Duration)
            {
                NextColor();
            }
            Percent = Fades ? Timer / Duration : 0;
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
