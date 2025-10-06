using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class BloomModifier : Component
    {
        public float Multiplier;
        public BloomModifier(float mult = 1) : base(true, false)
        {
            Multiplier = mult;
        }
        [OnLoad]
        public static void Load()
        {
            //On.Celeste.BloomRenderer.Apply += BloomRenderer_Apply;
        }

        private static void BloomRenderer_Apply(On.Celeste.BloomRenderer.orig_Apply orig, BloomRenderer self, VirtualRenderTarget target, Scene scene)
        {
            float prev = self.Strength;
            float mult = 1;
            foreach(BloomModifier mod in scene.Tracker.GetComponents<BloomModifier>())
            {
                mult *= mod.Multiplier;
            }
            self.Strength *= mult;
            orig(self, target, scene);
            self.Strength = prev;
        }
    }
}
