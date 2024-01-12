
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using FMOD;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/AudioEffectTrigger")]
    [Tracked]
    public class AudioEffectTrigger : Trigger
    {
        public string flag;
        public bool inverted;
        public bool FlagState
        {
            get
            {
                if (string.IsNullOrEmpty(flag))
                {
                    return true;
                }
                bool flagState = SceneAs<Level>().Session.GetFlag(flag);
                return inverted ? !flagState : flagState;
            }
        }
        public string ID;
        public enum Method
        {
            Add,
            Remove,
            Nothing
        }
        public enum Events
        {
            Music,
            AltMusic,
            Ambience
        }
        public enum Effects
        {
            Chorus,
            Distortion,
            Echo,
            Flange,
            Normalize,
            Oscillator,
            PitchShift,
            Tremolo
        }
        public Effects Effect;
        public Method OnLevelStartMethod;
        public Method OnLevelEndMethod;
        public Method OnLeaveMethod;
        public Method OnEnterMethod;
        public Events Event;
        public AEDSP Dsp;

        private float chorusMix, chorusRate, chorusDepth;
        private float distortionLevel;
        private float echoDelay, echoFeedback, echoDryLvl, echoWetLvl;
        private float flangeMix, flangeDepth, flangeRate;
        private float normFadeTime, normThresh, normMaxAmp;
        private float pitchPitch;
        private int pitchSize;
        private float tremFreq, tremDepth, tremShape, tremSkew, tremDuty, tremFlatness, tremPhase, tremSpread;

        private Osc.Wave oscWave;
        private float oscRate;
        public AudioEffectTrigger(EntityData data, Vector2 offset)
    : base(data, offset)
        {
            OnLevelStartMethod = data.Enum<Method>("onLevelStart");
            OnLevelEndMethod = data.Enum<Method>("onLevelEnd");
            OnEnterMethod = data.Enum<Method>("onEnter");
            OnLeaveMethod = data.Enum<Method>("onLeave");
            Event = data.Enum<Events>("audio");
            Effect = data.Enum<Effects>("effect");
            inverted = data.Bool("inverted");
            ID = data.Attr("effectID");
            Dsp.ID = ID;
            flag = data.Attr("flag");
            
            switch (Effect)
            {
                case Effects.Chorus:
                    chorusMix = data.Float("chorusMix");
                    chorusRate = data.Float("chorusRate");
                    chorusDepth = data.Float("chorusDepth");
                    Dsp = new Chorus(chorusMix, chorusRate, chorusDepth);
                    break;
                case Effects.Distortion:
                    distortionLevel = data.Float("distorationLevel");
                    Dsp = new Distortion(distortionLevel);
                    break;
                case Effects.Echo:
                    echoDelay = data.Float("echoDelay");
                    echoFeedback = data.Float("echoFeedback");
                    echoDryLvl = data.Float("dryLevel");
                    echoWetLvl = data.Float("wetLevel");
                    Dsp = new Echo(echoDelay, echoFeedback, echoDryLvl, echoWetLvl);
                    break;
                case Effects.Flange:
                    flangeMix = data.Float("flangeMix");
                    flangeDepth = data.Float("flangeDepth");
                    flangeRate = data.Float("flangeRate");
                    Dsp = new Flange(flangeMix, flangeDepth, flangeRate);
                    break;
                case Effects.Normalize:
                    normFadeTime = data.Float("fadeTime");
                    normThresh = data.Float("threshold");
                    normMaxAmp = data.Float("maxAmp");
                    Dsp = new Normalize(normFadeTime, normThresh, normMaxAmp);
                    break;
                case Effects.Oscillator:
                    oscWave = data.Enum<Osc.Wave>("wave");
                    oscRate = data.Float("oscRate");
                    Dsp = new Osc(oscWave, oscRate);
                    break;
                case Effects.PitchShift:
                    pitchPitch = data.Float("pitch");
                    pitchSize = data.Int("fftSize");
                    Dsp = new PitchShift(pitchPitch, pitchSize);
                    break;
                case Effects.Tremolo:
                    tremFreq = data.Float("tremoloFreq");
                    tremDepth = data.Float("tremoloDepth");
                    tremShape = data.Float("tremoloShape");
                    tremSkew = data.Float("tremoloSkew");
                    tremDuty = data.Float("tremoloDuty");
                    tremFlatness = data.Float("tremoloFlatness");
                    tremPhase = data.Float("tremoloPhase");
                    tremSpread = data.Float("tremSpread");
                    Dsp = new Tremolo(tremFreq, tremDepth, tremShape, tremSkew, tremDuty, tremFlatness, tremPhase, tremSpread);
                    break;
            }
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Run(OnEnterMethod);
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            Run(OnLeaveMethod);

        }
        public override void SceneBegin(Scene scene)
        {
            base.SceneBegin(scene);
            Run(OnLevelStartMethod);

        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Run(OnLevelEndMethod);
        }
        public void Run(Method state)
        {
            if (!FlagState || Dsp is null || Dsp.Injected) return;
            switch (state)
            {
                case Method.Add:
                    AudioEffectGlobal.AddEffect(Dsp, EnumToInstance());
                    break;
                case Method.Remove:
                    for (int i = 0; i < AudioEffectGlobal.StaticDsps.Count; i++)
                    {
                        if (AudioEffectGlobal.StaticDsps[i].ID == ID)
                        {
                            AudioEffectGlobal.RemoveEffect(AudioEffectGlobal.StaticDsps[i]);
                        }
                    }
                    break;
                case Method.Nothing:
                    break;
                default: break;
            }
        }
        public EventInstance EnumToInstance()
        {
            return Event switch
            {
                Events.Music => Audio.currentMusicEvent,
                Events.AltMusic => Audio.currentAltMusicEvent,
                Events.Ambience => Audio.currentAmbientEvent,
                _ => null
            };
        }
        public bool Inverted(string flag)
        {
            return flag[0] == '!';
        }
    }
}