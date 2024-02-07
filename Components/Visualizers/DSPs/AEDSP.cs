﻿using Celeste.Mod.PuzzleIslandHelper.Entities;
using FMOD;
using FMOD.Studio;
using FrostHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs
{
    public class AEDSP
    {
        public bool Injected;
        public DSP Dsp;
        public DSP_TYPE Type;
        public bool EjectOnSceneEnd;
        public ChannelGroup ActiveGroup;
        public EventInstance Instance;
        public string ID;
        public bool Initialized;
        public bool Valid => Dsp is not null && Injected;
        public AEDSP(DSP_TYPE type)
        {
            Type = type;
        }
        public AEDSP(DSP_TYPE type, string id) : this(type)
        {
            ID = id;
        }
        public virtual void SetParams()
        {

        }
        public void Initialize(FMOD.System system, DSP_TYPE type)
        {
            CheckResult(system.createDSPByType(type, out Dsp), "Initialize");
            Initialized = true;
        }
        public bool InitializeSelf()
        {
            CheckResult(Audio.System.getLowLevelSystem(out FMOD.System system), "InitializeSelfGetLowLevelSystem");
            CheckResult(system.createDSPByType(Type, out Dsp), "InitializeSelfCreateDsp");
            Initialized = true;
            return true;
        }

        public void CheckResult(RESULT result, string function)
        {
            if (result != RESULT.OK) throw new Exception(Error.String(result) + "Function: " + function);
        }
        public bool Inject(EventInstance instance)
        {
            if (Dsp is null || !Initialized) return false;
            instance.getChannelGroup(out ChannelGroup group);
            if (group is null) return false;
            group.addDSP(CHANNELCONTROL_DSP_INDEX.HEAD, Dsp);
            Dsp.setWetDryMix(0.5f, 0.5f, 0);
            ActiveGroup = group;
            Injected = true;
            return true;
        }
        public void Eject(EventInstance instance = null)
        {
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
    }

}
