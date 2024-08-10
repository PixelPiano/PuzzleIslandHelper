using FMOD;
using FMOD.Studio;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs
{
    public class AEDSP
    {
        public bool Injected;
        public DSP Dsp;
        public DSP_TYPE Type;
        public ChannelGroup ActiveGroup;
        public EventInstance Instance;
        public string ID;
        public bool Initialized;
        public bool StayInjected;
        public bool Valid => Dsp is not null && Injected;
        public AEDSP(DSP_TYPE type)
        {
            Type = type;
        }
        public AEDSP(DSP_TYPE type, string id) : this(type)
        {
            ID = id;
        }
        public void SetVolume(float volume)
        {
            Dsp.setWetDryMix(1, volume, 0);
        }
        public virtual void SetParams()
        {

        }
        public void Initialize(FMOD.System system, DSP_TYPE type)
        {
            CheckResult(system.createDSPByType(type, out Dsp), "Reset");
            Initialized = true;
        }
        public bool InitializeSelf()
        {
            CheckResult(Audio.System.getLowLevelSystem(out FMOD.System system), "InitializeSelfGetLowLevelSystem");
            CheckResult(system.createDSPByType(Type, out Dsp), "InitializeSelfCreateDsp");
            Initialized = true;
            return true;
        }
        public bool Inject(EventInstance instance)
        {
            if (Dsp is null || !Initialized || Injected) return false;
            if (instance.getChannelGroup(out ChannelGroup group) == RESULT.OK)
            {
                RESULT result = group.addDSP(CHANNELCONTROL_DSP_INDEX.HEAD, Dsp);
                if (result == RESULT.OK)
                {

                    if (Dsp.setWetDryMix(1, 1, 0) == RESULT.OK)
                    {
                        Dsp.getChannelFormat(out CHANNELMASK mask, out int channels, out _);
                        Dsp.setChannelFormat(mask, channels, SPEAKERMODE.SURROUND);
                        ActiveGroup = group;
                        Injected = true;
                        SetParams();
                        return true;
                    }
                    else
                    {
                        throw new Exception("AEDSP: Unable to set DSP wet dry mix");
                    }
                }
                else
                {
                    throw new Exception("AEDSP: error adding dsp to group. Result = " + result.ToString());
                }
            }
            else
            {
                throw new Exception("AEDSP: error getting channel group from event instance");
            }
        }
        public void Eject(EventInstance instance = null)
        {
            if (StayInjected) return;
            if (ActiveGroup is not null)
            {
                ActiveGroup.removeDSP(Dsp);
                Injected = false;
                return;
            }
            if (instance is not null)
            {
                instance.getChannelGroup(out ChannelGroup group);
                if (group is not null)
                {
                    group.removeDSP(Dsp);
                    Injected = false;
                }
            }
        }
        public bool InjectSelf(EventInstance instance)
        {
            if (!InitializeSelf()) return false;
            return Inject(instance);
        }
        public static void CheckResult(RESULT result, string function)
        {
            if (result != RESULT.OK) throw new Exception(Error.String(result) + "Function: " + function);
        }
    }

}
