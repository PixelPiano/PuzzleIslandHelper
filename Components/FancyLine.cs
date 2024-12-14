using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class FancyLine : GraphicsComponent
    {
        public enum ColorModes
        {
            Static,
            Flicker,
            Choose,
            Fade
        }
        public Vector2 Offset;
        public Vector2 RenderStart
        {
            get
            {
                return (Entity == null ? Vector2.Zero : Entity.Position) + start + Offset;
            }
            set
            {
                start = value - Offset - (Entity == null ? Vector2.Zero : Entity.Position);
            }
        }
        public Vector2 RenderEnd
        {
            get
            {
                return (Entity == null ? Vector2.Zero : Entity.Position) + end + Offset;
            }
            set
            {
                end = value - Offset - (Entity == null ? Vector2.Zero : Entity.Position);
            }
        }
        public Vector2 start, end;
        public ColorModes ColorMode;
        public Color StartColor;
        public Color? Color2;
        public float ColorInterval = 0.2f;
        public float ColorTimer;
        public float Thickness;
        public bool RandomizeOnAdd = true;
        public FancyLine(Vector2 start, Vector2 end, Color color, float thickness = 1) : base(true)
        {
            this.start = start;
            this.end = end;
            Color = color;
            Thickness = thickness;
        }
        public FancyLine(Vector2 start, Vector2 end, Color color, float thickness, Color color2, ColorModes colorMode, float interval) : this(start, end, color, thickness)
        {
            this.ColorMode = colorMode;
            ColorInterval = interval;
            this.Color2 = color2;
        }
        public void MoveBy(Vector2 amount)
        {
            start += amount;
            end += amount;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            if (RandomizeOnAdd) ColorTimer = Calc.Random.NextFloat();
        }
        public override void Update()
        {
            base.Update();
            if (ColorInterval != 0)
            {
                switch (ColorMode)
                {
                    case ColorModes.Flicker:
                        ColorTimer += Engine.DeltaTime;
                        if (Scene.OnInterval(ColorInterval) && Color2.HasValue)
                        {
                            Color = Color.Lerp(StartColor, Color2.Value, Calc.Random.Range(0, 1f));
                        }
                        break;
                    case ColorModes.Choose:
                        ColorTimer += Engine.DeltaTime;
                        if (Scene.OnInterval(ColorInterval) && Color2.HasValue)
                        {
                            Color = Calc.Random.Choose(StartColor, Color2.Value);
                        }
                        break;
                    case ColorModes.Fade:
                        ColorTimer += Engine.DeltaTime / ColorInterval;
                        Color target = Color2 ?? Color.Transparent;
                        Color = Color.Lerp(StartColor, target, ((float)Math.Sin(ColorTimer) + 1) / 2f);
                        break;
                }
            }
        }
        public override void Render()
        {
            Draw.Line(RenderStart, RenderEnd, Color, Thickness);
        }
        public void Render(Vector2 start, Vector2 end)
        {
            Draw.Line(start, end, Color, Thickness);
        }

    }
}
