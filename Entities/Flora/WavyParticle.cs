using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [Tracked]
    public class WavyParticle : Entity
    {
        public Color Color;
        public Color Color2;
        public Vector2 Dir;
        public bool DownFirst;
        public float WaveTime = 1;
        public float Offset;
        private float Speed;
        public float MinSpeed;
        public float MaxSpeed;
        private float Life;
        public float MinLife;
        public float MaxLife;
        public Vector2 Acceleration;
        public float Friction;
        public float Size = 1;
        private Vector2 offsetDir;
        private float targetOffset;
        public float MinOffset;
        public float MaxOffset;
        public float SizeOffset;
        public float SizeRate;
        private float trueSize;
        private Color StartColor;
        public enum ColorModes
        {
            None,
            FadeFromMiddle,
            SwitchAtMiddle
        }
        public ColorModes ColorMode;
        public enum OffsetModes
        {
            Static,
            ChangeEveryWave,
            ChangeEveryHalfWave
        }
        public OffsetModes OffsetMode;
        public enum SizeModes
        {
            Static,
            GrowWithWave,
            Grow,
            Shrink
        }
        public SizeModes SizeMode;
        public enum FadeModes
        {
            None,
            Linear,
            InAndOut,
            Late,
        }
        public FadeModes FadeMode;
        private float StartLife;
        public WavyParticle(Vector2 position) : base(position)
        {

        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            targetOffset = Calc.Random.Range(MinOffset, MaxOffset);
            Life = Calc.Random.Range(MinLife, MaxLife);
            Speed = Calc.Random.Range(MinSpeed, MaxSpeed);
            StartLife = Life;
            StartColor = Color;
            Add(new Coroutine(positionRoutine()));
        }
        private IEnumerator positionRoutine()
        {
            yield return Ease.SineOut.Lerp(WaveTime / 2f, f => Offset = Calc.LerpClamp(0, DownFirst ? 1 : -1, f));
            float from = Offset, to = -Math.Sign(Offset);
            bool newWave = true;
            while (true)
            {
                if (OffsetMode == OffsetModes.ChangeEveryHalfWave || (newWave && OffsetMode == OffsetModes.ChangeEveryWave))
                {
                    to *= Calc.Random.Range(0.2f, 1f);
                }
                yield return Ease.SineInOut.Lerp(WaveTime, f => Offset = Calc.LerpClamp(from, to, f));
                from = to;
                to *= -1;
                newWave = !newWave;
            }
        }
        public override void Render()
        {
            base.Render();

            Vector2 position = Position + (Offset * offsetDir * targetOffset);
            Draw.SpriteBatch.Draw(Draw.Pixel.Texture.Texture_Safe, position, Draw.Pixel.ClipRect, Color, 0, Vector2.Zero, trueSize, SpriteEffects.None, 0f);
        }
        public override void Update()
        {
            base.Update();
            if (Life <= 0)
            {
                RemoveSelf();
                return;
            }
            float num2 = Life / StartLife;
            float num3 = ((FadeMode == FadeModes.Linear) ? num2 : ((FadeMode == FadeModes.Late) ? Math.Min(1f, num2 / 0.25f) : ((FadeMode != FadeModes.InAndOut) ? 1f : ((num2 > 0.75f) ? (1f - (num2 - 0.75f) / 0.25f) : ((!(num2 < 0.25f)) ? 1f : (num2 / 0.25f))))));

            if (num3 == 0)
            {
                Color = Color.Transparent;
            }
            else
            {
                if (ColorMode == ColorModes.None)
                {
                    Color = StartColor;
                }
                else if (ColorMode == ColorModes.FadeFromMiddle)
                {
                    Color = Color.Lerp(StartColor, Color2, Math.Max(0, (Math.Abs(Offset) - 0.2f) / 0.8f));
                }
                else if (ColorMode == ColorModes.SwitchAtMiddle)
                {
                    Color = Math.Sign(Offset) >= 0 ? StartColor : Color2;
                }
                if (num3 < 1)
                {
                    Color *= num3;
                }
            }
            Position += Dir * (Speed * Engine.DeltaTime) + Acceleration * Engine.DeltaTime;
            offsetDir = Dir.Rotate(-MathHelper.PiOver2);
            Speed = Calc.Approach(Speed, 0, Friction * Engine.DeltaTime);
            Life -= Engine.DeltaTime;
            trueSize = SizeMode switch
            {
                SizeModes.GrowWithWave => Size + Math.Abs(Offset) * SizeOffset,
                SizeModes.Grow => trueSize + SizeRate * Engine.DeltaTime,
                SizeModes.Shrink => Math.Max(0, trueSize - SizeRate * Engine.DeltaTime),
                _ => Size
            };

        }
    }
}
