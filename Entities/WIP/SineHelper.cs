using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using static Celeste.Mod.PuzzleIslandHelper.Entities.WIP.DigiFolliage;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    public class SineHelper : Entity
    {
        public static float Value;
        public static float Percent;
        public SineHelper() : base()
        {
            Tag |= Tags.Global | Tags.TransitionUpdate | Tags.Persistent;
        }
        public static void Load()
        {
            Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
        }
        public static void Unload()
        {
            Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
        }
        private static void LevelLoader_OnLoadingThread(Level level)
        {
            level.Add(new SineHelper());
        }
        public override void Update()
        {
            base.Update();
            Value = (float)Math.Sin(Scene.TimeActive);
            Percent = (Value + 1) / 2f;
        }
    }


}