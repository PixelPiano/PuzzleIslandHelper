using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using FMOD;
using FMOD.Studio;
using Monocle;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
        public DSP Capture;
        public DSP_READCALLBACK ReadCallback;
        public GCHandle ObjHandle;
        public float[] DataBuffer;
        public uint BufferLength;
        public bool UseCapture;
        public int Channels = 0;
        public float DspVolume = 1;
        public virtual void OnEject() //Called once after the dsps have been removed from the event instance
        {

        }
        public virtual void OnInject() //Called once after the dsps have been added to the event instance
        {

        }

        public AudioEffect(params AEDSP[] aedsps) : base()
        {
            UseCapture = false;
            foreach (AEDSP dsp in aedsps)
            {
                Add(dsp);
            }
        }
        public AudioEffect(bool useCapture) : base()
        {
            UseCapture = useCapture;
        }
        public void SetVolume(float volume)
        {
            if (Playing)
            {
                instance.setVolume(volume);
            }
        }
        public static RESULT CaptureDSPReadCallback(ref DSP_STATE dsp_state, IntPtr inbuffer, IntPtr outbuffer, uint length, int inchannels, ref int outchannels)
        {
            DSP_STATE_FUNCTIONS functions = (DSP_STATE_FUNCTIONS)Marshal.PtrToStructure(dsp_state.functions, typeof(FMOD.DSP_STATE_FUNCTIONS));

            IntPtr userData;
            functions.getuserdata(ref dsp_state, out userData);

            GCHandle objHandle = GCHandle.FromIntPtr(userData);
            AudioEffect obj = objHandle.Target as AudioEffect;
            if (obj is not null)
            {
                // Save the channel count out for the update function
                obj.Channels = inchannels;

                // Copy the incoming heeheeBuffer to process later
                int lengthElements = (int)length * inchannels;
                Marshal.Copy(inbuffer, obj.DataBuffer, 0, lengthElements);

                // Copy the inbuffer to the outbuffer so we can still hear it
                Marshal.Copy(obj.DataBuffer, 0, outbuffer, lengthElements);
            }
            return RESULT.OK;
        }
        public void CreateCaptureDSP(ChannelGroup group)
        {
            uint bufferLength;
            int numBuffers;
            if (Audio.System.getLowLevelSystem(out FMOD.System system) == RESULT.OK)
            {
                if (system.getDSPBufferSize(out bufferLength, out numBuffers) == RESULT.OK)
                {
                    DataBuffer = new float[bufferLength * 8];
                    BufferLength = bufferLength;
                    ObjHandle = GCHandle.Alloc(this);
                    if (ObjHandle != null)
                    {
                        ReadCallback = CaptureDSPReadCallback;
                        DSP_DESCRIPTION desc = new DSP_DESCRIPTION()
                        {
                            numinputbuffers = 1,
                            numoutputbuffers = 1,
                            read = ReadCallback,
                            userdata = GCHandle.ToIntPtr(ObjHandle)
                        };

                        if (system.createDSP(ref desc, out Capture) == RESULT.OK)
                        {
                            if (group.addDSP(0, Capture) != RESULT.OK)
                            {
                                Logger.Log("AudioEffect", "Unable to add Capture to the channel group");
                            }
                        }
                    }
                }

            }
        }
        public void DestroyCaptureDSP(ChannelGroup group)
        {
            if (group != null)
            {
                if (Audio.System.getLowLevelSystem(out var system) == RESULT.OK)
                {
                    // Remove the capture DSP from the channel group
                    group.removeDSP(Capture);
                    // Release the DSP and free the object handle
                    Capture.release();
                }
            }
            ObjHandle.Free();
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
                Dsps[i].SetVolume(DspVolume);
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
            if (Audio.System.getLowLevelSystem(out FMOD.System system) == RESULT.OK)
            {
                foreach (AEDSP aedsp in Dsps)
                {
                    if (!aedsp.Injected)
                    {
                        aedsp.Initialize(system, aedsp.Type);
                        aedsp.ActiveGroup = ActiveGroup;
                        aedsp.Inject(instance);
                    }
                }
                if (UseCapture)
                {
                    CreateCaptureDSP(ActiveGroup);
                }
                SetAllParams();
                OnInject();
                injecting = false;
                DSPsInjected = true;
            }
        }
        public void EjectDsp(AEDSP dsp)
        {
            if (Dsps is null || dsp is null || !Dsps.Contains(dsp)) return;
            dsp.Eject();
        }
        private void FreeDSPs()
        {
            if (Dsps is null) return;
            for (int i = 0; i < Dsps.Count; i++)
            {
                if (Dsps[i] is null || Dsps[i].Dsp is null) continue;
                Dsps[i].Dsp.release();
            }
        }
        private void EjectDSPs()
        {
            if (ActiveGroup is null) return;
            if (Dsps is not null && Dsps.Count > 0)
            {
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
            }
            if (UseCapture)
            {
                DestroyCaptureDSP(ActiveGroup);
            }
            OnEject();
            ActiveGroup = null;
            DSPsInjected = false;

        }
        public override void Removed(Entity entity)
        {
            base.Removed(entity);
            EjectDSPs();
            FreeDSPs();
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
            if (instance is null) return null;
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
