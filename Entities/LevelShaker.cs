using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class LevelShakeModifier : BetterShaker
    {
        public bool SkipIfInactive;
        public float Mult = 1;
        public Vector2 DirectionalMult = Vector2.One;
        public Vector2 ShakeVector;
        public static Vector2 totalAdditionalShake;
        public enum Types
        {
            AddToValue,
            MultiplyFinalModifierValue,
            MultiplyFinalGlobalValue
        }
        public Types Type;
        public LevelShakeModifier(float mult = 1) : base(null)
        {
            Mult = mult;
            OnShake = onShake;
        }
        private void onShake(Vector2 amount)
        {
            ShakeVector += amount;
        }
        [OnLoad]
        public static void Load()
        {
            On.Celeste.Level.BeforeRender += Level_BeforeRender;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.Level.BeforeRender -= Level_BeforeRender;
        }

        private static void Level_BeforeRender(On.Celeste.Level.orig_BeforeRender orig, Level self)
        {
            Vector2 vector = default;
            Vector2 finalMult = Vector2.One;
            Vector2 finalGlobalMult = Vector2.One;
            foreach (LevelShakeModifier modifier in self.Tracker.GetComponents<LevelShakeModifier>())
            {
                if (modifier.Active || !modifier.SkipIfInactive)
                {
                    switch (modifier.Type)
                    {
                        case Types.AddToValue:
                            vector += modifier.ShakeVector * (modifier.DirectionalMult * modifier.Mult);
                            break;
                        case Types.MultiplyFinalModifierValue:
                            finalMult *= modifier.DirectionalMult * modifier.Mult;
                            break;
                        case Types.MultiplyFinalGlobalValue:
                            finalGlobalMult *= modifier.DirectionalMult * modifier.Mult;
                            break;
                    }
                }
            }
            if (Settings.Instance.ScreenShake == ScreenshakeAmount.Half)
            {
                finalMult *= 0.5f;
            }
            Vector2 prev = self.ShakeVector;
            vector *= finalMult;
            self.ShakeVector += vector;
            self.ShakeVector *= finalGlobalMult;
            orig(self);
            self.ShakeVector = prev;
            totalAdditionalShake = vector;
        }
    }
    [Obsolete("Replaced by LevelShakeModifier.cs")]
    public class LevelShaker
    {
        internal class Shaker : Entity
        {
            public Shaker() : base()
            {
                Tag |= Tags.TransitionUpdate | Tags.Global;
            }
        }
        internal static Shaker ShakeHelper;
        public static float Intensity = 0;
        [OnLoad]
        public static void Load()
        {
            Intensity = 0;
            //On.Celeste.Level.BeforeRender += Level_BeforeRender;
            //Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
        }
        private static void LevelLoader_OnLoadingThread(Level level)
        {
            //level.Add(ShakeHelper = new Shaker());
        }

        [OnUnload]
        public static void Unload()
        {
            Intensity = 0;
            //On.Celeste.Level.BeforeRender -= Level_BeforeRender;
            //Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
        }
        private static void Level_BeforeRender(On.Celeste.Level.orig_BeforeRender orig, Level self)
        {
            Vector2 prev = self.ShakeVector;
            self.ShakeVector *= (1 + Intensity);
            orig(self);
            self.ShakeVector = prev;
        }
    }
}