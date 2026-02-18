using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [Tracked]
    public class BetterShaker : Component
    {
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
        public BetterShaker(Action<Vector2> onShake) : base(true, false)
        {
            OnShake = onShake;
        }
        public void StartShaking(float time = -1f)
        {
            Shaking = true;
            shakeTimer = time;
        }
        private event Action onStop = () => { };
        public void StartShaking(Action onStop, float time = -1f)
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
            if (UseRawDeltaTime ? Scene.OnRawInterval(0.04f) : Scene.OnInterval(0.04f))
            {
                Vector2 vector = shakeAmount;
                shakeAmount = Calc.Random.ShakeVector();
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
