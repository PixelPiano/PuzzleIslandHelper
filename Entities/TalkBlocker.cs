using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    [ConstantEntity("PuzzleIslandHelper/TalkBlocker")]
    public class TalkBlocker : Entity
    {
        [Command("disable_talk", "Prevents Talk Components from being used for x time")]
        public static void CommandDisableTalk(float time)
        {
            DisableFor(time);
        }
        private static Hook hook_TalkComponentUI_get_Display;
        private delegate bool orig_TalkComponenUI_get_Display(TalkComponent.TalkComponentUI self);
        public static bool TalkDisabled => DisabledTimer > 0 || ForcedOff;
        public static float DisabledTimer;
        public static bool ForcedOff;
        public TalkBlocker() : base()
        {
            Tag |= Tags.Global;
        }
        public static void Reset()
        {
            ForcedOff = false;
            DisabledTimer = 0;
        }
        public static void DisableFor(float time)
        {
            DisabledTimer = time;
        }
        public static void Consume()
        {
            DisabledTimer = Engine.DeltaTime;
        }
        public override void Update()
        {
            base.Update();
            DisabledTimer = Math.Max(0, DisabledTimer - Engine.DeltaTime);
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Reset();
        }
        public override void SceneBegin(Scene scene)
        {
            base.SceneBegin(scene);
            Reset();
        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Reset();
        }
        [OnLoad]
        public static void Load()
        {
            hook_TalkComponentUI_get_Display = new Hook(
               typeof(TalkComponent.TalkComponentUI).GetProperty("Display").GetGetMethod(),
               typeof(TalkBlocker).GetMethod("TalkComponentUI_get_Display", BindingFlags.NonPublic | BindingFlags.Static)
           );
            On.Celeste.TalkComponent.Update += TalkComponent_Update;
        }
        private static void TalkComponent_Update(On.Celeste.TalkComponent.orig_Update orig, TalkComponent self)
        {
            bool prev = self.Enabled;
            if (TalkDisabled)
            {
                self.Enabled = false;
            }
            orig(self);
            self.Enabled = prev;
        }
        private static bool TalkComponentUI_get_Display(orig_TalkComponenUI_get_Display orig, TalkComponent.TalkComponentUI self)
        {
            return !TalkDisabled && orig(self);
        }

        [OnUnload]
        public static void Unload()
        {
            hook_TalkComponentUI_get_Display?.Dispose();
            hook_TalkComponentUI_get_Display = null;
            On.Celeste.TalkComponent.Update -= TalkComponent_Update;
        }
    }
}