using Microsoft.Xna.Framework;
using Monocle;
using System.ComponentModel;
using static Celeste.Trigger;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    public class LightController : Backdrop
    {
        public float ActiveLight;
        public float InactiveLight;
        public bool UseInactive;
        private bool Instant;

        public LightController(float value, bool instant)
        {
            ActiveLight = value;
            Instant = instant;
        }

        public override void Update(Scene scene)
        {

            base.Update(scene);
            Level level = scene as Level;
            if (IsVisible(level))
            {
                if (Instant)
                {
                    level.Lighting.Alpha = ActiveLight;
                }
                else
                {
                    level.Lighting.Alpha = Calc.Approach(level.Lighting.Alpha, ActiveLight, 0.05f);
                }
            }
        }
    }
}

