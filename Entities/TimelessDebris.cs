using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Pooled]
    public class TimelessDebris : Debris
    {
        public TimelessDebris() : base()
        {
        }
        private void orig_update()
        {
            orig_orig_update();
            LiftSpeed = Vector2.Zero;
            if (liftSpeedTimer > 0f)
            {
                liftSpeedTimer -= Engine.RawDeltaTime;
                if (liftSpeedTimer <= 0f)
                {
                    lastLiftSpeed = Vector2.Zero;
                }
            }
        }
        private void orig_orig_update()
        {
            Components.Update();
        }
        public override void Update()
        {
            orig_update();
            image.Rotation += Math.Abs(speed.X) * (float)rotateSign * Engine.RawDeltaTime;
            if (fadeLerp < 1f)
            {
                fadeLerp = Calc.Approach(fadeLerp, 1f, 2f * Engine.RawDeltaTime);
            }
            MoveH(speed.X * Engine.RawDeltaTime, collideH);
            MoveV(speed.Y * Engine.RawDeltaTime, collideV);
            if (dreaming)
            {
                speed.X = Calc.Approach(speed.X, 0f, 50f * Engine.RawDeltaTime);
                speed.Y = Calc.Approach(speed.Y, 6f * dreamSine.Value, 100f * Engine.RawDeltaTime);
            }
            else
            {
                bool flag = OnGround();
                speed.X = Calc.Approach(speed.X, 0f, (flag ? 50f : 20f) * Engine.RawDeltaTime);
                if (!flag)
                {
                    speed.Y = Calc.Approach(speed.Y, 100f, 400f * Engine.RawDeltaTime);
                }
            }

            if (lifeTimer > 0f)
            {
                lifeTimer -= Engine.RawDeltaTime;
            }
            else if (alpha > 0f)
            {
                alpha -= 4f * Engine.RawDeltaTime;
                if (alpha <= 0f)
                {
                    RemoveSelf();
                }
            }

            image.Color = Color.Lerp(Color.White, Color.Gray, fadeLerp) * alpha;
        }
    }
}