using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    public class ConstantTimeBurst : DisplacementRenderer.Burst
    {
        public Helper helper;
        public ConstantTimeBurst(MTexture texture, Vector2 position, Vector2 origin, float duration) : base(texture, position, origin, duration)
        {

        }
        public static ConstantTimeBurst AddBurst(Vector2 position, float duration, float radiusFrom, float radiusTo, float alpha = 1f, Ease.Easer alphaEaser = null, Ease.Easer radiusEaser = null)
        {
            if(Engine.Scene is not Level level || level.Displacement is null) return null;
            MTexture mTexture = GFX.Game["util/displacementcircle"];
            ConstantTimeBurst burst = new ConstantTimeBurst(mTexture, position, mTexture.Center, duration);
            burst.ScaleFrom = radiusFrom / (float)(mTexture.Width / 2);
            burst.ScaleTo = radiusTo / (float)(mTexture.Width / 2);
            burst.AlphaFrom = alpha;
            burst.AlphaTo = 0f;
            burst.AlphaEaser = alphaEaser;
            return level.Displacement.Add(burst) as ConstantTimeBurst;
        }
        public static void Load()
        {
            On.Celeste.DisplacementRenderer.Add += DisplacementRenderer_Add;
            On.Celeste.DisplacementRenderer.Remove += DisplacementRenderer_Remove;
        }
        public static void Unload()
        {
            On.Celeste.DisplacementRenderer.Add -= DisplacementRenderer_Add;
            On.Celeste.DisplacementRenderer.Remove -= DisplacementRenderer_Remove;
        }
        private static DisplacementRenderer.Burst DisplacementRenderer_Remove(On.Celeste.DisplacementRenderer.orig_Remove orig, DisplacementRenderer self, DisplacementRenderer.Burst point)
        {
            if (point is ConstantTimeBurst burst && burst.helper != null && Engine.Scene is Level level)
            {
                level.Remove(burst.helper);
            }
            return orig(self, point);
        }

        private static DisplacementRenderer.Burst DisplacementRenderer_Add(On.Celeste.DisplacementRenderer.orig_Add orig, DisplacementRenderer self, DisplacementRenderer.Burst point)
        {
            if (point is ConstantTimeBurst burst && burst.helper == null && Engine.Scene is Level level)
            {
                level.Add(burst.helper = new Helper(burst));
            }
            return orig(self, point);
        }

        [Tracked]
        public class Helper : Entity
        {
            public ConstantTimeBurst Burst;
            public Helper(ConstantTimeBurst burst) : base()
            {
                Burst = burst;
                Add(new Coroutine(ReplacementUpdate()) { UseRawDeltaTime = true });
            }
            public IEnumerator ReplacementUpdate()
            {
                while (true)
                {
                    Burst.Percent += Engine.RawDeltaTime / Burst.Duration;
                    yield return null;
                }
            }
        }
    }

}