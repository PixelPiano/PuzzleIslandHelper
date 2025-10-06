using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [Tracked]
    public class ShakeComponent : Component
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
        public ShakeComponent(Action<Vector2> onShake) : base(true, false)
        {
            OnShake = onShake;
        }
        public void StartShaking(float time = -1f)
        {
            Shaking = true;
            shakeTimer = time;
        }
        public void StopShaking()
        {
            Shaking = false;
            if (shakeAmount != Vector2.Zero)
            {
                OnShake.Invoke(-shakeAmount);
                shakeAmount = Vector2.Zero;
            }
        }

        public override void Update()
        {
            base.Update();
            if (!Shaking)
            {
                return;
            }
            if (Scene.OnInterval(0.04f))
            {
                Vector2 vector = shakeAmount;
                shakeAmount = Calc.Random.ShakeVector();
                OnShake?.Invoke(shakeAmount - vector);
            }
            if (shakeTimer > 0f)
            {
                shakeTimer -= Engine.DeltaTime;
                if (shakeTimer <= 0f)
                {
                    Shaking = false;
                    StopShaking();
                }
            }
        }
    }
}
