using Celeste.Mod.Core;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public class LevelShaker
    {
        internal class Shaker : Entity
        {
            public Shaker() : base()
            {
                Tag |= Tags.TransitionUpdate | Tags.Global;
            }
            public override void Update()
            {
                base.Update();
                Level level = SceneAs<Level>();
                if (Intensity > 0 && level != null)
                {
                    level.shakeDirection = Vector2.Zero;
                    level.shakeTimer = Engine.DeltaTime;
                }
            }

        }
        internal static Shaker ShakeHelper;
        public static float Intensity = 0;
        [OnLoad]
        public static void Load()
        {
            Intensity = 0;
            On.Celeste.Level.BeforeRender += Level_BeforeRender;
            Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
        }

        private static void LevelLoader_OnLoadingThread(Level level)
        {
            level.Add(ShakeHelper = new Shaker());
        }

        [OnUnload]
        public static void Unload()
        {
            Intensity = 0;
            On.Celeste.Level.BeforeRender -= Level_BeforeRender;
            Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
        }
        private static void Level_BeforeRender(On.Celeste.Level.orig_BeforeRender orig, Level self)
        {
            if (Intensity > 0)
            {
                Vector2 prev = self.ShakeVector;
                self.ShakeVector *= Intensity;
                orig(self);
                self.ShakeVector = prev;
            }
            else
            {
                orig(self);
            }
        }
    }
}