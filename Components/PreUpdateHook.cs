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
            Everest.Events.Level.OnBeforeUpdate += Level_OnBeforeUpdate;
        }

        private static void Level_OnBeforeUpdate(Level obj)
        {
            foreach (PreUpdateHook hook in obj.Tracker.GetComponents<PreUpdateHook>())
            {
                if (hook.Active && hook.Entity.Active)
                {
                    hook.Callback?.Invoke();
                }
            }
        }

        [OnUnload]
        public static void Unload()
        {
            Everest.Events.Level.OnBeforeUpdate -= Level_OnBeforeUpdate;
        }
    }
}
