using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class Pulse : Entity
    {
        public enum Shapes
        {
            Rectangle,
            Circle,
            Diamond,
            Line
        }
        private float life;
        private float startLife;
        public Shapes Shape;
        public Color Color;
        public Color ColorB;
        public Color StartColor;
        public Ease.Easer ColorEase;
        public Ease.Easer SizeEase;
        public float DestWidth;
        private float width, height;
        public float DestHeight;
        public float StartWidth;
        public float StartHeight;
        public float Percent;
        private Vector2 to;
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            startLife = life;
        }
        public static Pulse Line(Vector2 from, Vector2 to, FadeModes fadeMode,int depth = 0, float duration = 1, Color colorA = default, Color colorB = default, Ease.Easer colorEase = null, Ease.Easer sizeEase = null)
        {
            if (Engine.Scene is Level level)
            {
                Pulse pulse = new()
                {
                    Depth = depth,
                    FadeMode = fadeMode,
                    life = duration,
                    Position = from,
                    to = to,
                    StartColor = colorA,
                    Color = colorA,
                    ColorB = colorB,
                    ColorEase = colorEase ?? Ease.Linear,
                    SizeEase = sizeEase ?? Ease.Linear,
                    Shape = Shapes.Circle
                };
                level.Add(pulse);
                return pulse;
            }
            return null;
        }
        public static Pulse Circle(Vector2 position, float radiusFrom, float radiusTo, FadeModes fadeMode, int depth = 0, float duration = 1, Color colorA = default, Color colorB = default, Ease.Easer colorEase = null, Ease.Easer sizeEase = null)
        {
            if (Engine.Scene is Level level)
            {
                Pulse pulse = new()
                {
                    Depth = depth,
                    FadeMode = fadeMode,
                    life = duration,
                    Position = position,
                    StartWidth = radiusFrom,
                    DestWidth = radiusTo,
                    StartColor = colorA,
                    Color = colorA,
                    ColorB = colorB,
                    ColorEase = colorEase ?? Ease.Linear,
                    SizeEase = sizeEase ?? Ease.Linear,
                    Shape = Shapes.Circle
                };
                level.Add(pulse);
                return pulse;
            }
            return null;
        }
        public static Pulse Rect(Vector2 position, float widthFrom, float widthTo, float heightFrom, float heightTo, FadeModes fadeMode, int depth = 0, float duration = 1, Color colorA = default, Color colorB = default, Ease.Easer colorEase = null, Ease.Easer sizeEase = null)
        {
            if (Engine.Scene is Level level)
            {
                Pulse pulse = new()
                {
                    Depth = depth,
                    FadeMode = fadeMode,
                    life = duration,
                    Position = position,
                    StartWidth = widthFrom,
                    DestWidth = widthTo,
                    StartHeight = heightFrom,
                    DestHeight = heightTo,
                    StartColor = colorA,
                    Color = colorA,
                    ColorB = colorB,
                    ColorEase = colorEase ?? Ease.Linear,
                    SizeEase = sizeEase ?? Ease.Linear,
                    Shape = Shapes.Rectangle
                };
                level.Add(pulse);
                return pulse;
            }
            return null;
        }
        public static Pulse Diamond(Vector2 position, float radiusFrom, float radiusTo, FadeModes fadeMode, int depth = 0, float duration = 1, Color colorA = default, Color colorB = default, Ease.Easer colorEase = null, Ease.Easer sizeEase = null)
        {
            if (Engine.Scene is Level level)
            {
                Pulse pulse = new()
                {
                    Depth = depth,
                    FadeMode = fadeMode,
                    life = duration,
                    Position = position,
                    StartWidth = radiusFrom,
                    DestWidth = radiusTo,
                    StartColor = colorA,
                    Color = colorA,
                    ColorB = colorB,
                    ColorEase = colorEase ?? Ease.Linear,
                    SizeEase = sizeEase ?? Ease.Linear,
                    Shape = Shapes.Diamond
                };
                level.Add(pulse);
                return pulse;
            }
            return null;
        }
        public enum FadeModes
        {
            None,
            Linear,
            Late,
            InAndOut
        }
        public FadeModes FadeMode;
        public override void Update()
        {
            base.Update();
            life -= Engine.DeltaTime;
            if (life <= 0)
            {
                RemoveSelf();
                return;
            }
            Percent = life / startLife;
            float num3 = FadeMode switch
            {
                FadeModes.Linear => Percent,
                FadeModes.Late => Math.Min(1f, Percent / 0.25f),
                FadeModes.InAndOut => (Percent > 0.75f) ? (1f - (Percent - 0.75f) / 0.25f) : ((!(Percent < 0.25f)) ? 1f : (Percent / 0.25f)),
                _ => 1f
            };
            Color = num3 == 0 ? Color.Transparent : Color.Lerp(ColorB, StartColor, ColorEase(Percent)) * Math.Min(num3, 1);
            width = Calc.LerpClamp(DestWidth, StartWidth, SizeEase(Percent));
            height = Calc.LerpClamp(DestHeight, StartHeight, SizeEase(Percent));
        }
        public override void Render()
        {
            base.Render();
            Vector2 size = new Vector2(width, height);
            switch (Shape)
            {
                case Shapes.Rectangle:
                    Draw.Rect(Position - size / 2, size.X, size.Y, Color);
                    break;
                case Shapes.Line:
                    Draw.Line(Position, to, Color);
                    break;
                default:
                    Draw.Circle(Position, Math.Max(size.X, size.Y), Color, Shape == Shapes.Circle ? 20 : 1);
                    break;
            }
        }

    }
}