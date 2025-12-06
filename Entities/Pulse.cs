using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Runtime.CompilerServices;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Pulse;
using static Celeste.MoonGlitchBackgroundTrigger;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{

    public class PulseEntity : Entity
    {
        public PulseEntity()
        {
        }
        private static PulseEntity create(Vector2 position, int depth, Shapes shape, Fade fadeMode, Mode pulseMode, Vector2 to, float duration,
    float widthFrom, float widthTo, float heightFrom, float heightTo,
    bool start, Color colorA = default, Color colorB = default, Ease.Easer colorEase = null, Ease.Easer sizeEase = null)
        {
            Pulse pulse = new()
            {
                Shape = shape,
                FadeMode = fadeMode,
                to = to,
                TimeLeft = duration,
                Duration = duration,
                StartWidth = widthFrom,
                DestWidth = widthTo,
                StartHeight = heightFrom,
                DestHeight = heightTo,
                StartColor = colorA,
                Color = colorA,
                ColorB = colorB,
                ColorEase = colorEase ?? Ease.Linear,
                SizeEase = sizeEase ?? Ease.Linear,
                PulseMode = pulseMode
            };
            PulseEntity entity = new PulseEntity();
            entity.Add(pulse);
            entity.Position = position;
            entity.Depth = depth;
            Engine.Scene?.Add(entity);
            if (start)
            {
                pulse.Start();
            }
            return entity;
        }
        public static PulseEntity Line(Vector2 from, Vector2 to, int depth, Fade fadeMode, Mode pulseMode, float duration = 1, bool start = true, Color colorA = default, Color colorB = default, Ease.Easer colorEase = null, Ease.Easer sizeEase = null)
        {
            return create(from, depth, Shapes.Line, fadeMode, pulseMode, to - from, duration, 0, 0, 0, 0, start, colorA, colorB, colorEase, sizeEase);
        }
        public static PulseEntity Circle(Vector2 position, int depth, Fade fadeMode, Mode pulseMode, float radiusFrom, float radiusTo, float duration = 1, bool start = true, Color colorA = default, Color colorB = default, Ease.Easer colorEase = null, Ease.Easer sizeEase = null)
        {
            return create(position, depth, Shapes.Circle, fadeMode, pulseMode, Vector2.Zero, duration, radiusFrom, radiusTo, 0, 0, start, colorA, colorB, colorEase, sizeEase);
        }
        public static PulseEntity Rect(Vector2 position, int depth, Fade fadeMode, Mode pulseMode, float widthFrom, float widthTo, float heightFrom, float heightTo, float duration = 1, bool start = true, Color colorA = default, Color colorB = default, Ease.Easer colorEase = null, Ease.Easer sizeEase = null)
        {
            return create(position, depth, Shapes.Line, fadeMode, pulseMode, Vector2.Zero, duration, widthFrom, widthTo, heightFrom, heightTo, start, colorA, colorB, colorEase, sizeEase);
        }
        public static PulseEntity Diamond(Vector2 position, int depth, Fade fadeMode, Mode pulseMode, float radiusFrom, float radiusTo, float duration = 1, bool start = true, Color colorA = default, Color colorB = default, Ease.Easer colorEase = null, Ease.Easer sizeEase = null)
        {
            return create(position, depth, Shapes.Diamond, fadeMode, pulseMode, Vector2.Zero, duration, radiusFrom, radiusTo, 0, 0, start, colorA, colorB, colorEase, sizeEase);
        }
    }
    [Tracked]
    public class Pulse : GraphicsComponent
    {
        public enum Shapes
        {
            Rectangle,
            Circle,
            Diamond,
            Line
        }
        public float TimeLeft;
        public float Duration;
        public Shapes Shape;
        public Color ColorB;
        public Color StartColor;
        public float Alpha = 1;
        public Ease.Easer ColorEase;
        public Ease.Easer SizeEase;
        public float StartWidth;
        public float StartHeight;
        public float DestWidth;
        public float DestHeight;
        public float width, height;
        public float Percent;
        public float SizePercent => SizeEase(Percent);
        public float ColorPercent => ColorEase(Percent);
        public float Thickness = 1;
        private bool startInstant;
        public Vector2 to;
        public enum Fade
        {
            None,
            Linear,
            Late,
            InAndOut
        }
        public enum Mode
        {
            Persist,
            Oneshot,
            Looping,
            YoyoOneshot,
            YoyoLooping
        }
        public Fade FadeMode;
        public Mode PulseMode;
        private static Pulse create(Entity follow, Shapes shape, Fade fadeMode, Mode pulseMode, Vector2 position, Vector2 to, float duration,
            float widthFrom, float widthTo, float heightFrom, float heightTo,
            bool start, Color colorA = default, Color colorB = default, Ease.Easer colorEase = null, Ease.Easer sizeEase = null, float thickness = 1)
        {
            Pulse pulse = new()
            {
                Shape = shape,
                FadeMode = fadeMode,
                Position = position,
                to = to,
                TimeLeft = duration,
                Duration = duration,
                StartWidth = widthFrom,
                DestWidth = widthTo,
                StartHeight = heightFrom,
                DestHeight = heightTo,
                StartColor = colorA,
                Color = colorA,
                ColorB = colorB,
                ColorEase = colorEase ?? Ease.Linear,
                SizeEase = sizeEase ?? Ease.Linear,
                PulseMode = pulseMode,
                Thickness = thickness
            };
            if (follow != null) follow.Add(pulse);
            if (start)
            {
                pulse.Start();
            }
            return pulse;
        }
        public static Pulse Line(Entity follow, Fade fadeMode, Mode pulseMode, Vector2 from, Vector2 to, float duration = 1, bool start = true, Color colorA = default, Color colorB = default, Ease.Easer colorEase = null, Ease.Easer sizeEase = null)
        {
            return create(follow, Shapes.Line, fadeMode, pulseMode, from, to, duration, 0, 0, 0, 0, start, colorA, colorB, colorEase, sizeEase);
        }
        public static Pulse Circle(Entity follow, Fade fadeMode, Mode pulseMode, Vector2 position, float radiusFrom, float radiusTo, float duration = 1, bool start = true, Color colorA = default, Color colorB = default, Ease.Easer colorEase = null, Ease.Easer sizeEase = null)
        {
            return create(follow, Shapes.Circle, fadeMode, pulseMode, position, Vector2.Zero, duration, radiusFrom, radiusTo, 0, 0, start, colorA, colorB, colorEase, sizeEase);
        }
        public static Pulse Rect(Entity follow, Fade fadeMode, Mode pulseMode, Vector2 position, float widthFrom, float widthTo, float heightFrom, float heightTo, float duration = 1, bool start = true, Color colorA = default, Color colorB = default, Ease.Easer colorEase = null, Ease.Easer sizeEase = null)
        {
            return create(follow, Shapes.Rectangle, fadeMode, pulseMode, position, Vector2.Zero, duration, widthFrom, widthTo, heightFrom, heightTo, start, colorA, colorB, colorEase, sizeEase);
        }
        public static Pulse Diamond(Entity follow, Fade fadeMode, Mode pulseMode, Vector2 position, float radiusFrom, float radiusTo, float duration = 1, bool start = true, Color colorA = default, Color colorB = default, Ease.Easer colorEase = null, Ease.Easer sizeEase = null)
        {
            return create(follow, Shapes.Diamond, fadeMode, pulseMode, position, Vector2.Zero, duration, radiusFrom, radiusTo, 0, 0, start, colorA, colorB, colorEase, sizeEase);
        }
        public static Pulse Line(Entity follow, Vector2 from, Vector2 to, Color colorA = default, Color colorB = default, float duration = 1, bool start = true, Ease.Easer colorEase = null, Ease.Easer sizeEase = null)
        {
            return create(follow, Shapes.Line, Fade.Linear, Mode.Oneshot, from, to, duration, 0, 0, 0, 0, start, colorA, colorB, colorEase, sizeEase);
        }
        public static Pulse Circle(Entity follow, float radius, Color colorA = default, Color colorB = default, float duration = 1, bool start = true, Ease.Easer colorEase = null, Ease.Easer sizeEase = null)
        {
            return create(follow, Shapes.Circle, Fade.Linear, Mode.Oneshot, Vector2.Zero, Vector2.Zero, duration, 0, radius, 0, radius, start, colorA, colorB, colorEase, sizeEase);
        }
        public static Pulse Rect(Entity follow, float width, float height, Color colorA = default, Color colorB = default, float duration = 1, bool start = true, Ease.Easer colorEase = null, Ease.Easer sizeEase = null)
        {
            return create(follow, Shapes.Rectangle, Fade.Linear, Mode.Oneshot, Vector2.Zero, Vector2.Zero, duration, 0, width, 0, height, start, colorA, colorB, colorEase, sizeEase);
        }
        public static Pulse Diamond(Entity follow, float radius, Color colorA = default, Color colorB = default, float duration = 1, bool start = true, Ease.Easer colorEase = null, Ease.Easer sizeEase = null)
        {
            return create(follow, Shapes.Diamond, Fade.Linear, Mode.Oneshot, Vector2.Zero, Vector2.Zero, duration, 0, radius, 0, radius, start, colorA, colorB, colorEase, sizeEase);
        }
        public Pulse() : base(false)
        {
        }
        public void Set(float percent)
        {
            TimeLeft = percent * Duration;
        }
        public void Stop()
        {
            Active = false;
        }
        public void Resume()
        {
            Active = true;
        }
        public bool Reverse;
        private bool startedReversed;
        public override void Update()
        {
            TimeLeft -= Engine.DeltaTime;
            Percent = Math.Max(0f, TimeLeft) / Duration;
            if (Reverse)
            {
                Percent = 1f - Percent;
            }
            float num3 = FadeMode switch
            {
                Fade.Linear => Percent,
                Fade.Late => Math.Min(1f, Percent / 0.25f),
                Fade.InAndOut => (Percent > 0.75f) ? (1f - (Percent - 0.75f) / 0.25f) : ((!(Percent < 0.25f)) ? 1f : (Percent / 0.25f)),
                _ => 1f
            };
            Color = num3 == 0 ? Color.Transparent : Color.Lerp(ColorB, StartColor, ColorEase(Percent)) * Math.Min(num3, 1);
            width = Calc.LerpClamp(DestWidth, StartWidth, SizeEase(Percent));
            height = Calc.LerpClamp(DestHeight, StartHeight, SizeEase(Percent));

            if (!(TimeLeft <= 0f))
            {
                return;
            }
            TimeLeft = 0f;

            switch (PulseMode)
            {
                case Mode.Persist:
                    Active = false;
                    break;
                case Mode.Oneshot:
                    Active = false;
                    RemoveSelf();
                    break;
                case Mode.Looping:
                    Start(Reverse);
                    break;
                case Mode.YoyoOneshot:
                    if (Reverse == startedReversed)
                    {
                        Start(!Reverse);
                        startedReversed = !Reverse;
                    }
                    else
                    {
                        Active = false;
                        RemoveSelf();
                    }
                    break;
                case Mode.YoyoLooping:
                    Start(!Reverse);
                    break;
            }

        }
        public void Start()
        {
            Start(reverse: false);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Start(bool reverse)
        {
            bool flag2 = (Reverse = reverse);
            startedReversed = flag2;
            TimeLeft = Duration;
            float eased = (Percent = (Reverse ? 1 : 0));
            Active = true;
        }

        public void Start(float duration, bool reverse = false)
        {
            Duration = duration;
            Start(reverse);
        }
        public void Reset()
        {
            TimeLeft = Duration;
        }
        public override void Render()
        {
            base.Render();
            Vector2 size = new Vector2(width, height);
            switch (Shape)
            {
                case Shapes.Rectangle:
                    PianoUtils.HollowRect(RenderPosition - size / 2, size.X, size.Y, Color * Alpha, (int)Thickness);
                    break;
                case Shapes.Line:
                    Draw.Line(RenderPosition, to, Color, Thickness);
                    break;
                case Shapes.Circle:
                    Draw.Circle(RenderPosition, Math.Max(size.X, size.Y), Color * Alpha, Thickness, 20);
                    break;
                default:
                    Draw.Circle(RenderPosition, Math.Max(size.X, size.Y), Color * Alpha, Thickness, 1);
                    break;
            }
        }
    }
}