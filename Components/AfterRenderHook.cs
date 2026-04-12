using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class AfterRenderHook : Component
    {
        public Action Callback;
        public AfterRenderHook(Action callback) : base(false, false)
        {
            Callback = callback;
        }
        [OnLoad]
        public static void Load()
        {
            On.Celeste.Level.AfterRender += Level_AfterRender;
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.Level.AfterRender -= Level_AfterRender;
        }
        private static void Level_AfterRender(On.Celeste.Level.orig_AfterRender orig, Level self)
        {
            foreach (AfterRenderHook hook in self.Tracker.GetComponents<AfterRenderHook>())
            {
                hook.Callback?.Invoke();
            }
            orig(self);
        }
    }
}
