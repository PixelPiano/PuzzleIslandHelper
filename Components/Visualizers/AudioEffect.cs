using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using FMOD;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Components.Visualizers
{
    [Tracked]
    public class AudioEffect : SoundSource
    {
        public List<AEDSP> Dsps = new();
        public bool DSPsInjected;
        public bool injecting;
        public ChannelGroup ActiveGroup;
        public float TimePlaying;
        public bool Valid => Dsps is not null && DSPsInjected;

        public void SetAllParams()
        {
            if (Dsps is null || Dsps.Count == 0) return;
            for (int i = 0; i < Dsps.Count; i++)
            {
                if (Dsps[i].Dsp is null || !Dsps[i].Injected) continue;
                switch (Dsps[i].Type)
                {
                    case DSP_TYPE.FFT:
                        (Dsps[i] as FFT).SetParams();
                        break;
                }
            }
        }
        public virtual void OnEject()
        {

        }
        public virtual void OnInject()
        {

        }
        public AudioEffect(params DSP_TYPE[] types) : base()
        {
            Dsps = new();
            foreach (DSP_TYPE type in types)
            {
                AddDSP(type);
            }
        }
        public void AddDSP(DSP_TYPE type)
        {
            if (Dsps is null) return;
            switch (type)
            {
                case DSP_TYPE.FFT: AddFFT(new FFT()); break;
                case DSP_TYPE.OSCILLATOR: AddOsc(new Osc()); break;
                case DSP_TYPE.CHORUS: AddChorus(new Chorus()); break;
                case DSP_TYPE.NORMALIZE: AddNormalize(new Normalize()); break;
                case DSP_TYPE.PITCHSHIFT: AddPitchShift(new PitchShift()); break;
                case DSP_TYPE.DISTORTION: AddDistortion(new Distortion()); break;
                case DSP_TYPE.ECHO: AddEcho(new Echo()); break;
                case DSP_TYPE.FLANGE: AddFlange(new Flange()); break;
            }
        }

        public void AddFFT(FFT fft)
        {
            Dsps.Add(fft);
        }
        public void AddOsc(Osc osc)
        {
            Dsps.Add(osc);
        }
        public void AddChorus(Chorus chorus)
        {
            Dsps.Add(chorus);
        }
        public void AddDistortion(Distortion distortion)
        {
            Dsps.Add(distortion);
        }
        public void AddEcho(Echo echo)
        {
            Dsps.Add(echo);
        }
        public void AddFlange(Flange flange)
        {
            Dsps.Add(flange);
        }
        public void AddNormalize(Normalize normalize)
        {
            Dsps.Add(normalize);
        }
        public void AddPitchShift(PitchShift pitchshift)
        {
            Dsps.Add(pitchshift);
        }
        public void AddTremolo(Tremolo tremolo)
        {
            Dsps.Add(tremolo);
        }
        public override void Update()
        {
            if (Valid)
            {
                SetAllParams();
            }
            if (injecting && !DSPsInjected && instance != null)
            {
                InjectDSPs();
            }
            base.Update();

            TimePlaying = Playing ? TimePlaying + Engine.DeltaTime : 0;
            if (!injecting && DSPsInjected && !InstancePlaying)
            {
                EjectDSPs();
            }
        }
        private void InjectDSPs()
        {
            ActiveGroup = GetActiveChannelGroup(instance);
            if (ActiveGroup is null) return;
            Audio.System.getLowLevelSystem(out FMOD.System system);
            if (system is null) return;
            foreach (AEDSP aedsp in Dsps)
            {
                if (!aedsp.Injected)
                {
                    aedsp.Initialize(system, aedsp.Type);
                    aedsp.ActiveGroup = ActiveGroup;
                    aedsp.Inject();
                }
            }
            SetAllParams();
            OnInject();
            injecting = false;
            DSPsInjected = true;
        }
        public void EjectDsp(AEDSP dsp)
        {
            if (Dsps is null || dsp is null || !Dsps.Contains(dsp)) return;
            dsp.Eject();
            Dsps.Remove(dsp);
        }
        private void EjectDSPs()
        {
            if (ActiveGroup is null || Dsps is null || Dsps.Count == 0) return;
            List<AEDSP> toRemove = new();
            for (int i = 0; i < Dsps.Count; i++)
            {
                if (!Dsps[i].Injected || Dsps[i].Dsp == null) continue;
                toRemove.Add(Dsps[i]);
            }
            foreach (AEDSP aedsp in toRemove)
            {
                EjectDsp(aedsp);
            }
            OnEject();
            Dsps.Clear();
            ActiveGroup = null;
            DSPsInjected = false;

        }
        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            EjectDSPs();
        }
        public new SoundSource Play(string path, string param = null, float value = 0)
        {
            injecting = true;
            return base.Play(path, param, value);
        }
        public new SoundSource Stop(bool allowFadeOut = true)
        {
            injecting = false;
            return base.Stop(allowFadeOut);
        }
        public static Channel GetActiveChannel(EventInstance instance)
        {
            if (instance is null)
            {
                Logger.Log(LogLevel.Warn, "AudioEffectGlobal", "Provided instance is null");
                return null;
            }
            instance.getChannelGroup(out ChannelGroup group);
            if (group is null)
            {
                Logger.Log(LogLevel.Warn, "AudioEffectGlobal", "Could not find Channel Group for instance of event");
                return null;
            }

            group.getNumChannels(out int numChannels);
            group.getNumGroups(out int numGroups);
            if (numChannels == 0 && numGroups == 0) return null;
            if (numGroups > 0)
            {
                for (int i = 0; i < numGroups; i++)
                {
                    group.getGroup(i, out ChannelGroup subGroup);
                    if (subGroup is null) continue;

                    Channel result = GetActiveChannel(subGroup);
                    if (result is null) continue;
                    return result;
                }
            }

            Logger.Log(LogLevel.Debug, "AudioEffectGlobal", "Could not find Active Channel for instance");

            return null;
        }
        public static Channel GetActiveChannel(ChannelGroup group)
        {
            if (group is null) return null;
            group.getNumChannels(out int numchannels);
            for (int i = 0; i < numchannels; i++)
            {
                group.getChannel(i, out Channel channel);
                if (channel is null) continue;
                channel.getMute(out bool mute);
                channel.getFrequency(out float frequency);
                if (mute || frequency == 0) continue;
                return channel;
            }
            return null;
        }
        public static ChannelGroup GetActiveChannelGroup(EventInstance instance, bool volSearch = false)
        {
            if (!volSearch)
            {
                instance.getChannelGroup(out ChannelGroup searchgroup);
                return searchgroup;
            }
            Channel channel = GetActiveChannel(instance);
            if (channel is null)
            {
                Logger.Log(LogLevel.Warn, "AudioEffectGlobal", "Could not find Channel for instance of event");
                return null;
            }
            channel.getChannelGroup(out ChannelGroup group);
            return group;
        }
    }
}
