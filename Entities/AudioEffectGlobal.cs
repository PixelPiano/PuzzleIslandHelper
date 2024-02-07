using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using FMOD;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using FMOD.Studio;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class AudioEffectGlobal : Entity
    {
        public TransitionListener Listener;
        public static List<AEDSP> StaticDsps = new();
        private static List<AEDSP> toRemove = new();
        public static void AddEffect(AEDSP dsp, EventInstance instance)
        {
            if (StaticDsps.Contains(dsp))
            {
                Console.WriteLine("already contains dsp");
                return;
            }
            if (!dsp.InjectSelf(instance)) return;
            Console.WriteLine("added dsp");
            dsp.SetParams();
            StaticDsps.Add(dsp);
        }
        public static void RemoveEffect(AEDSP dsp)
        {
            dsp.Eject();
        }
        public static void RemoveID(string id = "")
        {
            if (StaticDsps is null || StaticDsps.Count == 0)
            {
                Console.WriteLine("static dsps is null or empty");
                return;
            }
            for (int i = 0; i < StaticDsps.Count; i++)
            {
                if (StaticDsps[i].ID == id || string.IsNullOrEmpty(id))
                {
                    toRemove.Add(StaticDsps[i]);
                }
            }
        }
        public static void RemoveAll()
        {
            for(int i = 0; i<StaticDsps.Count; i++)
            {
                RemoveEffect(StaticDsps[i]);
            }
            StaticDsps.Clear();
            RemoveIDImmediately();
        }
        public static void RemoveIDImmediately(string id = "")
        {
            List<AEDSP> toRemove = new();
            for (int i = 0; i < StaticDsps.Count; i++)
            {
                if (StaticDsps[i].ID == id || string.IsNullOrEmpty(id))
                {
                    toRemove.Add(StaticDsps[i]);
                }
            }
            foreach (AEDSP dsp in toRemove)
            {
                RemoveEffect(dsp);
                StaticDsps.Remove(dsp);
                Console.WriteLine("removed dsp");
            }
            toRemove.Clear();
        }

        public void PostUpdatehook()
        {
            foreach (AEDSP dsp in toRemove)
            {
                RemoveEffect(dsp);
                StaticDsps.Remove(dsp);
                Console.WriteLine("removed dsp");
            }
            toRemove.Clear();
        }
        public static void Unload()
        {
            RemoveAll();
            On.Celeste.LevelLoader.ctor -= LevelLoader_ctor;
        }
        public static void Load()
        {
            RemoveAll();
            StaticDsps = new();
            On.Celeste.LevelLoader.ctor += LevelLoader_ctor;
        }

        private static void LevelLoader_ctor(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition)
        {
            orig(self, session, startPosition);
            self.Level.Add(new AudioEffectGlobal());
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
                    case DSP_TYPE.OSCILLATOR:
                        (StaticDsps[i] as Osc).SetParams();
                        break;
                    case DSP_TYPE.ECHO:
                        (StaticDsps[i] as Echo).SetParams();
                        break;
                    case DSP_TYPE.FLANGE:
                        (StaticDsps[i] as Flange).SetParams();
                        break;
                    case DSP_TYPE.DISTORTION:
                        (StaticDsps[i] as Distortion).SetParams();
                        break;
                    case DSP_TYPE.NORMALIZE:
                        (StaticDsps[i] as Normalize).SetParams();
                        break;
                    case DSP_TYPE.PITCHSHIFT:
                        (StaticDsps[i] as PitchShift).SetParams();
                        break;
                    case DSP_TYPE.CHORUS:
                        (StaticDsps[i] as Chorus).SetParams();
                        break;
                    case DSP_TYPE.TREMOLO:
                        (StaticDsps[i] as Tremolo).SetParams();
                        break;
                }
            }
        }
        public static void SetParam(string id)
        {

        }
        public AudioEffectGlobal() : base(Vector2.Zero)
        {
            Tag |= Tags.Global | Tags.Persistent | Tags.TransitionUpdate | Tags.FrozenUpdate | Tags.PauseUpdate;
            Add(new PostUpdateHook(PostUpdatehook));
        }

        public override void Update()
        {
            base.Update();
            SetAllParams();
        }
    }
}