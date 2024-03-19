using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using FMOD;
using FMOD.Studio;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Components.Visualizers
{
    [Tracked]
    public class AudioEffect : SoundSource
    {
        public List<AEDSP> Dsps = new(); //"Audio Effect Digital Signal Processor"s that manage and add DSPs to the event instance
        public bool DSPsInjected;        //if successfully added the dsps to the event instance
        public bool injecting;           //if currently attempting to add the dsps to the event instance (sometimes takes multiple frames)
        public ChannelGroup ActiveGroup; //the channel group the event instance is playing from
        public float TimePlaying;        //how long the event instance has been playing for
        public virtual void OnEject() //Called once after the dsps have been removed from the event instance
        {

        }
        public virtual void OnInject() //Called once after the dsps have been added to the event instance
        {

        }

        public AudioEffect(params DSP_TYPE[] types) : base()
        {
            foreach (DSP_TYPE type in types)
            {
                AddDSP(type);
            }
        }
        public void AddDSP(DSP_TYPE type)
        {
            //there's probably a better way to do this but i have a feeling it would be less optimal
            if (Dsps is null) return;
            switch (type)
            {
                case DSP_TYPE.FFT: Add(new FFT()); break;
                case DSP_TYPE.OSCILLATOR: Add(new Osc()); break;
                case DSP_TYPE.CHORUS: Add(new Chorus()); break;
                case DSP_TYPE.NORMALIZE: Add(new Normalize()); break;
                case DSP_TYPE.PITCHSHIFT: Add(new PitchShift()); break;
                case DSP_TYPE.DISTORTION: Add(new Distortion()); break;
                case DSP_TYPE.ECHO: Add(new Echo()); break;
                case DSP_TYPE.FLANGE: Add(new Flange()); break;
            }
        }
        public void SetAllParams()
        {
            if (Dsps is null || Dsps.Count == 0) return; 
            for (int i = 0; i < Dsps.Count; i++)
            {
                if (Dsps[i] is null || !Dsps[i].Injected) continue;
                switch (Dsps[i].Type)
                {
                    case DSP_TYPE.FFT: (Dsps[i] as FFT).SetParams(); break;
                    case DSP_TYPE.OSCILLATOR: (Dsps[i] as Osc).SetParams(); break;
                    case DSP_TYPE.CHORUS: (Dsps[i] as Chorus).SetParams(); break;
                    case DSP_TYPE.NORMALIZE: (Dsps[i] as Normalize).SetParams(); break;
                    case DSP_TYPE.PITCHSHIFT: (Dsps[i] as PitchShift).SetParams(); break;
                    case DSP_TYPE.DISTORTION: (Dsps[i] as Distortion).SetParams(); break;
                    case DSP_TYPE.ECHO: (Dsps[i] as Echo).SetParams(); break;
                    case DSP_TYPE.FLANGE: (Dsps[i] as Flange).SetParams(); break;
                }
            }
        }
        public void Add(AEDSP dsp)
        {
            Dsps.Add(dsp);
        }

        public override void Update()
        {
            if (injecting && !DSPsInjected)                     //if trying to add dsps but haven't yet
            {
                ActiveGroup = GetActiveChannelGroup(instance);  //try to get the channel the instance is playing from
                if (ActiveGroup is not null)                    //if the channel exists...
                {
                    InjectDSPs();                               //add the dsps to it
                }
            }
            SetAllParams();                                     //update the parameters for each dsp in the list
            base.Update();

            TimePlaying = Playing ? TimePlaying + Engine.DeltaTime : 0;
            if (!injecting && DSPsInjected && !InstancePlaying) //if dsps are injected and the instance is not longer playing, make sure the dsps are removed.
                                                                //(if this doesn't happen, some instances may not get fully removed when they should)
            {
                EjectDSPs();
            }
        }
        private void InjectDSPs()
        {
            Audio.System.getLowLevelSystem(out FMOD.System system);
            if (system is null) throw new Exception("Error: Low Level System is null!");
            foreach (AEDSP aedsp in Dsps)
            {
                if (!aedsp.Injected)
                {
                    aedsp.Initialize(system, aedsp.Type);
                    aedsp.ActiveGroup = ActiveGroup;
                    aedsp.Inject(instance);
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
        public SoundSource PlayEvent(string path, string param = null, float value = 0)
        {
            injecting = true;
            return base.Play(path, param, value);
        }
        public SoundSource StopEvent(bool allowFadeOut = true)
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
