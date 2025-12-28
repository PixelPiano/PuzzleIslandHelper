using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    public class ImageShine : Image
    {
        public float Alpha = 0;
        public float ScaleIncrement = 0.5f;
        public float AlphaIncrement = -0.3f;
        public float PulseDelay = 3f;
        public int Layers = 2;
        private float scaleMult;
        private float alphaMult;
        public float PrePulseFrameOffset;
        public Action OnPrePulse;
        public Action OnPulse;
        public float AlphaFrom = 1;
        public float AlphaTo = 0;
        public float ScaleFrom = 0;
        public float ScaleTo = 1;
        public float SpeedMult = 1;
        public ImageShine(MTexture texture, float startAlpha = 1) : base(texture, true)
        {
            Alpha = startAlpha;
            JustifyOrigin(0.5f, 0.5f);
        }
        public override void Update()
        {
            base.Update();
            if (OnPrePulse != null && PrePulseFrameOffset != 0 && Scene.OnInterval(PulseDelay, PrePulseFrameOffset * Engine.DeltaTime))
            {
                OnPrePulse.Invoke();
            }
            if (Scene.OnInterval(PulseDelay))
            {
                if (OnPulse != null)
                {
                    OnPulse.Invoke();
                }
                scaleMult = ScaleFrom;
                alphaMult = ScaleTo;
            }
            float speed = 1f - (float)Math.Pow(0.0099999997764825821, Engine.DeltaTime * SpeedMult);
            scaleMult = scaleMult + (ScaleTo - scaleMult) * speed;
            alphaMult = alphaMult + (AlphaTo - alphaMult) * speed;
        }
        public override void Render()
        {
            if (Alpha > 0)
            {
                Color color = Color;
                Vector2 scale = Scale;
                float scaleFactor, alphaFactor;
                for (int i = Layers; i > 0; i--)
                {
                    scaleFactor = 1 + i * ScaleIncrement;
                    alphaFactor = 1 + i * AlphaIncrement;
                    Scale = scale * scaleFactor * (1 + scaleMult);
                    Color = color * alphaFactor * alphaMult;
                    Scale.X = (float)Math.Round(Scale.X, 1);
                    Scale.Y = (float)Math.Round(Scale.Y, 1);
                    base.Render();
                }
                Scale = scale;
                Color = color;
            }
        }
    }
}
