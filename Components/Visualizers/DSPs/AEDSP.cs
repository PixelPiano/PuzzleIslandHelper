using FMOD;
using FMOD.Studio;
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
        public string ID;
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
            system.createDSPByType(type, out Dsp);
        }
        public void Inject()
        {
            if (Dsp is null || ActiveGroup is null) return;
            ActiveGroup.addDSP(CHANNELCONTROL_DSP_INDEX.HEAD, Dsp);
            Injected = true;
        }
        public void Eject()
        {
            Injected = false;
            if (!Valid || ActiveGroup is null) return;
            ActiveGroup.removeDSP(Dsp);
        }
    }

}
