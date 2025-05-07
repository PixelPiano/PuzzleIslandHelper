using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    public class OnGroundAlarm : Component
    {
        private float delay;
        private float delayTimer;
        private float duration;
        private float timer;
        public Action InAir;
        public Action OnGround;
        public OnGroundAlarm(float delay, float durationOnGround, Action inAir, Action onComplete) : base(true, false)
        {
            this.delay = delayTimer = delay;
            this.duration = timer = durationOnGround;
            InAir = inAir;
            OnGround = onComplete;
        }
        public override void Update()
        {
            base.Update();
            if (Entity != null)
            {
                if (delayTimer > 0)
                {
                    delayTimer -= Engine.DeltaTime;
                    return;
                }
                if (Entity.OnGround())
                {
                    if(timer > 0)
                    {
                        timer -= Engine.DeltaTime; 
                    }
                    if(timer <= 0)
                    {
                        OnGround?.Invoke();
                        Active = false;
                        RemoveSelf();
                    }
                }
                else
                {
                    InAir?.Invoke();
                }
            }
        }
    }
}
