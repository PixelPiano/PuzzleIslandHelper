using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using FMOD;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using FMOD.Studio;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class AudioEffectGlobal : Entity
    {
        public static List<AEDSP> StaticDsps = new();
        public static void AddEffect(AEDSP dsp, EventInstance instance)
        {
            ChannelGroup group = AudioEffect.GetActiveChannelGroup(instance);
            
            if (group is null)
            {
                Logger.Log(LogLevel.Warn, "AudioEffectGlobal", "Could not find Channel Group for instance of event");
                return;
            }
            Audio.System.getLowLevelSystem(out FMOD.System system);
            if (system is null)
            {
                Logger.Log(LogLevel.Warn, "AudioEffectGlobal", "Could not find Low Level FMOD.System for instance of event");
                return;
            }
            dsp.Initialize(system, dsp.Type);
            dsp.ActiveGroup = group;
            dsp.Inject();
            dsp.SetParams();
            StaticDsps.Add(dsp);
        }
        public static void Load()
        {
            StaticDsps = new();
            On.Celeste.LevelLoader.ctor += LevelLoader_ctor;
            //Everest.Events.AssetReload
           
        }
        private static void LevelLoader_ctor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition)
        {
            orig(self, session, startPosition);
            self.Level.Add(new AudioEffectGlobal());
        }
        public static void Unload()
        {
            if (StaticDsps is not null && StaticDsps.Count > 0)
            {
                foreach (AEDSP dsp in StaticDsps)
                {
                    if (!dsp.Valid) return;
                    RemoveEffect(dsp);
                }
                StaticDsps.Clear();
            }
        }
        public static void SetAllParams()
        {
            if (StaticDsps is null || StaticDsps.Count == 0) return;
            for (int i = 0; i < StaticDsps.Count; i++)
            {
                if (StaticDsps[i].Dsp is null || !StaticDsps[i].Injected) continue;
                switch (StaticDsps[i].Type)
                {
                    case DSP_TYPE.FFT:
                        (StaticDsps[i] as FFT).SetParams();
                        break;
                }
            }
        }
        public static void SetParam(string id)
        {

        }
        public static void RemoveEffect(AEDSP dsp)
        {
            dsp.Eject();
        }
        public AudioEffectGlobal() : base(Vector2.Zero)
        {
            Tag |= Tags.Global | Tags.Persistent | Tags.TransitionUpdate | Tags.FrozenUpdate | Tags.PauseUpdate;
        }
        public override void Update()
        {
            base.Update();
            SetAllParams();
        }
    }
}