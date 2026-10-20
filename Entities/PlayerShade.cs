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
    public class PlayerShade : Component
    {
        public float Amount;
        private float shadeTo;
        private float shadeFrom;
        private float lerpTime;
        private Ease.Easer lerpEase;
        private bool removeSelfOnLerpFinish;
        private float lerpTimeRemaining;
        public bool UseRawDeltaTime;
        public PlayerShade(float shade) : base(false, true)
        {
            Amount = shade;
        }
        public void StopLerp()
        {
            Active = false;
        }
        public void Lerp(float shadeTo, float time, Ease.Easer ease = null, bool removeSelfOnFinish = false)
        {
            shadeFrom = Amount;
            this.shadeTo = shadeTo;
            lerpTime = time;
            lerpTimeRemaining = time;
            lerpEase = ease ?? Ease.Linear;
            removeSelfOnLerpFinish = removeSelfOnFinish;
            Active = true;
        }
        public override void Update()
        {
            base.Update();
            if (lerpTimeRemaining > 0)
            {
                lerpTimeRemaining -= UseRawDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime;
                if (lerpTimeRemaining <= 0)
                {
                    lerpTimeRemaining = 0;
                    Amount = shadeTo;
                    if (removeSelfOnLerpFinish) RemoveSelf();
                    else Active = false;
                }
                else
                {
                    Amount = Calc.LerpClamp(shadeFrom, shadeTo, lerpEase(1 - lerpTimeRemaining / lerpTime));
                }
            }
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
                foreach (PlayerShade shade in self.Scene.Tracker.GetComponents<PlayerShade>())
                {
                    if (shade.Visible) lerp += shade.Amount;
                }
                if (lerp != 0) self.Color = Color.Lerp(prev, Color.Black, lerp);
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
                foreach (PlayerShade shade in self.Scene.Tracker.GetComponents<PlayerShade>())
                {
                    if (shade.Visible) lerp += shade.Amount;
                }
                if (lerp != 0) self.Color = Color.Lerp(prev, Color.Black, lerp);
            }
            orig(self);
            self.Color = prev;
        }
    }
}
