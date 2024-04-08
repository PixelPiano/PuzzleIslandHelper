using System;
using System.Reflection;
using BepInEx.AssemblyPublicizer;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class CustomShaker : Shaker
    {
        public Vector2 MaxShake;
        private bool easeShakeAmount;
        private float timeLimit;
        private int sign = 1;
        public CustomShaker(bool on = true, Action<Vector2> onShake = null)
            : base(on, onShake)
        {
            this.on = on;
            OnShake = onShake;
        }

        public CustomShaker ShakeFor(Vector2 maxShake, float seconds, bool easeShakeAmount, bool removeOnFinish)
        {
            MaxShake = maxShake;
            on = true;
            Timer = seconds;
            timeLimit = seconds;
            RemoveOnFinish = removeOnFinish;
            this.easeShakeAmount = easeShakeAmount;
            return this;
        }
        public CustomShaker(float time, bool removeOnFinish, Action<Vector2> onShake = null)
            : base(on: true, onShake)
        {
            Timer = time;
            timeLimit = time;
            RemoveOnFinish = removeOnFinish;
        }
        public override void Update()
        {
            if (on && Timer > 0f)
            {
                Timer -= Engine.DeltaTime;
                if (Timer <= 0f)
                {
                    on = false;
                    Value = Vector2.Zero;
                    if (OnShake != null)
                    {
                        OnShake(Vector2.Zero);
                    }

                    if (RemoveOnFinish)
                    {
                        RemoveSelf();
                    }

                    return;
                }
            }

            if (on && Scene.OnInterval(Interval))
            {
                float lerp = easeShakeAmount ? 1 - (Timer / timeLimit) : 1;
                Value = Vector2.Lerp(Vector2.Zero, sign * MaxShake, lerp);
                sign = -sign;
                if (OnShake != null)
                {
                    OnShake(Value);
                }
            }
        }
    }
}
