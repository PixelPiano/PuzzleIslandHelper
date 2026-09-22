using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [Tracked]
    public class BetterShaker : Component
    {
        public Random CustomRandom;
        public bool Shaking
        {
            protected set => _shaking = value;
            get => _shaking;
        }
        private bool _shaking;
        private float shakeTimer;
        private Vector2 shakeAmount;
        public Action<Vector2> OnShake;
        public bool UseRawDeltaTime;
        public float Interval = 0.04f;
        public BetterShaker(Action<Vector2> onShake) : base(true, false)
        {
            OnShake = onShake;
        }
        public void ShakeFor(float time = -1f)
        {
            Shaking = true;
            shakeTimer = time;
        }
        private event Action onStop = () => { };
        public void ShakeFor(Action onStop, float time = -1f)
        {
            Shaking = true;
            shakeTimer = time;
            this.onStop += onStop;
        }
        public void StopShaking()
        {
            Shaking = false;
            if (shakeAmount != Vector2.Zero)
            {
                OnShake.Invoke(-shakeAmount);
                shakeAmount = Vector2.Zero;
            }
            onStop?.Invoke();
            onStop = () => { };
        }

        public override void Update()
        {
            base.Update();
            if (!Shaking)
            {
                return;
            }
            if (UseRawDeltaTime ? Engine.Scene.OnRawInterval(Interval) : Engine.Scene.OnInterval(Interval))
            {
                Vector2 vector = shakeAmount;
                shakeAmount = CustomRandom?.ShakeVector() ?? Calc.Random.ShakeVector();
                OnShake?.Invoke(shakeAmount - vector);
            }
            if (shakeTimer > 0f)
            {
                shakeTimer -= (UseRawDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime);
                if (shakeTimer <= 0f)
                {
                    Shaking = false;
                    StopShaking();
                }
            }
        }
    }
}
