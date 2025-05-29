using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;

// PuzzleIslandHelper.DecalEffects
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class PlayerShade : Entity
    {
        public float Alpha;
        private Tween tween;
        public PlayerShade(float alpha) : base()
        {
            Alpha = alpha;
        }
        public void Fade(float to, float time, Ease.Easer ease, bool removeSelfOnFinish = false)
        {
            tween?.RemoveSelf();
            float from = Alpha;
            tween = Tween.Set(this, Tween.TweenMode.Oneshot, time, ease, t => Alpha = Calc.LerpClamp(from, to, t.Eased), (t) =>
            {
                if (removeSelfOnFinish)
                {
                    RemoveSelf();
                }
            });
        }
        [OnLoad]
        public static void Load()
        {
            On.Celeste.PlayerSprite.Render += PlayerSprite_Render;
            On.Celeste.PlayerHair.Render += PlayerHair_Render;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.PlayerSprite.Render -= PlayerSprite_Render;
            On.Celeste.PlayerHair.Render -= PlayerHair_Render;
        }
        private static void PlayerHair_Render(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self)
        {
            Color prev = self.Color;
            if (self.Scene != null)
            {
                float lerp = 0;
                foreach (PlayerShade shade in self.Scene.Tracker.GetEntities<PlayerShade>())
                {
                    lerp += shade.Alpha;
                }
                self.Color = Color.Lerp(prev, Color.Black, lerp);
            }
            orig(self);
            self.Color = prev;
        }

        private static void PlayerSprite_Render(On.Celeste.PlayerSprite.orig_Render orig, PlayerSprite self)
        {
            Color prev = self.Color;
            if (self.Scene != null)
            {
                float lerp = 0;
                foreach (PlayerShade shade in self.Scene.Tracker.GetEntities<PlayerShade>())
                {
                    lerp += shade.Alpha;
                }
                self.Color = Color.Lerp(prev, Color.Black, lerp);
            }
            orig(self);
            self.Color = prev;
        }
    }
}
