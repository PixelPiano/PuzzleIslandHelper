using Microsoft.Xna.Framework;
using System;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class PreUpdateHook : Component
    {
        public Action Callback;
        public PreUpdateHook(Action callback) : base(true, false)
        {
            Callback = callback;
        }
        [OnLoad]
        public static void Load()
        {
            On.Celeste.Level.Update += Level_Update;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.Level.Update -= Level_Update;
        }
        private static void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
        {
            foreach (PreUpdateHook hook in self.Tracker.GetComponents<PreUpdateHook>())
            {
                if (hook.Active && hook.Entity.Active)
                {
                    hook.Callback?.Invoke();
                }
            }
            orig(self);
        }
    }
}
