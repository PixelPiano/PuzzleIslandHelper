
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components.Visualizers.DSPs;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{
    public enum Easings
    {
        Linear,
        SineIn,
        SineOut,
        SineInOut,
        CubeIn,
        CubeOut,
        CubeInOut,
        QuintIn,
        QuintOut,
        QuintInOut,
        QuadIn,
        QuadOut,
        QuadInOut,
        BounceIn,
        BounceOut,
        BounceInOut,
        ElasticIn,
        ElasticOut,
        ElasticInOut,
        BackIn,
        BackOut,
        BackInOut,
        BigBackIn,
        BigBackOut,
        BigBackInOut,
        ExpoIn,
        ExpoOut,
        ExpoInOut,
    }
    public static class EasingsExt
    {
        public static Ease.Easer ToEase(this Easings easing)
        {
            return easing switch
            {
                Easings.Linear => Ease.Linear,
                Easings.SineIn => Ease.SineIn,
                Easings.SineOut => Ease.SineOut,
                Easings.SineInOut => Ease.SineInOut,
                Easings.CubeIn => Ease.CubeIn,
                Easings.CubeOut => Ease.CubeOut,
                Easings.CubeInOut => Ease.CubeInOut,
                Easings.QuintIn => Ease.QuintIn,
                Easings.QuintOut => Ease.QuintOut,
                Easings.QuintInOut => Ease.QuintInOut,
                Easings.QuadIn => Ease.QuadIn,
                Easings.QuadOut => Ease.QuadOut,
                Easings.QuadInOut => Ease.QuadInOut,
                Easings.BounceIn => Ease.BounceIn,
                Easings.BounceOut => Ease.BounceOut,
                Easings.BounceInOut => Ease.BounceInOut,
                Easings.ElasticIn => Ease.ElasticIn,
                Easings.ElasticOut => Ease.ElasticOut,
                Easings.ElasticInOut => Ease.ElasticInOut,
                Easings.BackIn => Ease.BackIn,
                Easings.BackOut => Ease.BackOut,
                Easings.BackInOut => Ease.BackInOut,
                Easings.BigBackIn => Ease.BigBackIn,
                Easings.BigBackOut => Ease.BigBackOut,
                Easings.BigBackInOut => Ease.BigBackInOut,
                Easings.ExpoIn => Ease.ExpoIn,
                Easings.ExpoOut => Ease.ExpoOut,
                Easings.ExpoInOut => Ease.ExpoInOut,
                _ => Ease.Linear
            };
        }
    }
    [CustomEntity("PuzzleIslandHelper/AdjustEffectParamTrigger")]
    [Tracked]
    public class AdjustEffectParamTrigger : Trigger
    {
        public string flag;
        public bool inverted;
        public Coroutine Routine;
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

        public enum Mode
        {
            OnEnter,
            OnLeave,
            OnLevelStart,
            OnLevelEnd,
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
        public enum Params
        {
            None = 0,
            ChorusMix,
            ChorusRate,
            ChorusDepth,
            DistortLevel,
            EchoDelay,
            EchoFeedback,
            EchoDryLevel,
            EchoWetLevel,
            FlangeMix,
            FlangeDepth,
            FlangeRate,
            NormFadeTime,
            NormThresh,
            NormMaxAmp,
            PitchPitch,
            PitchSize,
            TremFreq,
            TremDepth,
            TremShape,
            TremSkew,
            TremDuty,
            TremFlatness,
            TremPhase,
            TremSpread,
            OscWave,
            OscRate
        }
        public Params Param;
        public Effects Effect;
        public Mode TriggerMode;

        public float Value;
        private float time;
        private float delay;
        private bool allowFlagInterrupt;
        private Easings Easing;
        private Ease.Easer Easer;
        private bool onlyOnce;
        private bool ran;
        private bool snapToValueIfOff;
        private float To;
        private bool persistUntilFinished;
        private bool inRoutine;
        public AdjustEffectParamTrigger(EntityData data, Vector2 offset)
    : base(data, offset)
        {
            TriggerMode = data.Enum<Mode>("mode");
            Effect = data.Enum<Effects>("effect");
            Easing = data.Enum<Easings>("easing");
            Param = data.Enum<Params>("parameter");
            time = data.Float("acceleration");
            delay = data.Float("delay");
            Value = data.Float("value");
            To = Value;
            ID = data.Attr("effectID");
            flag = data.Attr("flag");
            Easer = Easing.ToEase();
            onlyOnce = data.Bool("onlyOnce");
            persistUntilFinished = data.Bool("persistUntilComplete");
            allowFlagInterrupt = data.Bool("allowFlagCancel");
            snapToValueIfOff = data.Bool("snapValueIfInterrupted");
            Add(Routine = new Coroutine(false));
        }
        private IEnumerator Sequence()
        {
            bool addedTags = false;
            if (persistUntilFinished)
            {
                AddTag(Tags.Global);
                AddTag(Tags.Persistent);
                addedTags = true;
            }
            inRoutine = true;
            yield return delay;
            float from = GetParam();
            Value = from;
            bool broke = false;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                if (allowFlagInterrupt && !FlagState)
                {
                    broke = true;
                    break;
                }
                Value = Calc.LerpClamp(from, To, Easer(i));
                SetParam();
                yield return null;
            }
            if (!broke || (broke && snapToValueIfOff))
            {
                Value = To;
                SetParam();
            }
            ran = true;
            //addedTags exists just in case the entity is given a global but not persistent tag (or vice versa) in the middle of the routine.
            if (persistUntilFinished && addedTags)
            {
                RemoveTag(Tags.Global);
                RemoveTag(Tags.Persistent);
            }
            inRoutine = false;
            yield return null;
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            if (TriggerMode == Mode.OnEnter) Run();
        }
        public override void OnLeave(Player player)
        {
            base.OnLeave(player);
            if (TriggerMode == Mode.OnLeave) Run();

        }
        public override void SceneBegin(Scene scene)
        {
            base.SceneBegin(scene);
            if (TriggerMode == Mode.OnLevelStart) Run();

        }
        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            if (TriggerMode == Mode.OnLevelEnd) Run();
        }
        public void Run()
        {
            if (!FlagState || (onlyOnce && ran) || inRoutine) return;
            CancelSimilarAndReplace();
        }
        public void CancelSimilarAndReplace()
        {
            foreach (AdjustEffectParamTrigger trigger in SceneAs<Level>().Tracker.GetEntities<AdjustEffectParamTrigger>())
            {
                if (trigger == this) continue;
                if (!trigger.ran || !trigger.inRoutine || trigger.Effect != Effect || trigger.Param != Param || trigger.ID != ID) continue;
                trigger.Cancel();
            }
            Routine.Replace(Sequence());
        }
        public void Cancel()
        {
            inRoutine = false;
            Routine.Cancel();
        }
        private void SetParam()
        {
            for (int i = 0; i < AudioEffectGlobal.StaticDsps.Count; i++)
            {
                if (AudioEffectGlobal.StaticDsps[i] == null || !AudioEffectGlobal.StaticDsps[i].Injected || AudioEffectGlobal.StaticDsps[i].ID != ID) continue;
                switch (Effect)
                {
                    case Effects.Chorus:
                        switch (Param)
                        {
                            case Params.ChorusDepth: (AudioEffectGlobal.StaticDsps[i] as Chorus).Depth = Value; break;
                            case Params.ChorusMix: (AudioEffectGlobal.StaticDsps[i] as Chorus).Mix = Value; break;
                            case Params.ChorusRate: (AudioEffectGlobal.StaticDsps[i] as Chorus).Rate = Value; break;
                        }
                        break;
                    case Effects.Distortion:
                        switch (Param)
                        {
                            case Params.DistortLevel: (AudioEffectGlobal.StaticDsps[i] as Distortion).Level = Value; break;
                        }
                        break;
                    case Effects.Echo:
                        switch (Param)
                        {
                            case Params.EchoDelay: (AudioEffectGlobal.StaticDsps[i] as Echo).Delay = Value; break;
                            case Params.EchoFeedback: (AudioEffectGlobal.StaticDsps[i] as Echo).Feedback = Value; break;
                            case Params.EchoDryLevel:
                                (AudioEffectGlobal.StaticDsps[i] as Echo).DryLevel = Value;
                                break;
                            case Params.EchoWetLevel:
                                (AudioEffectGlobal.StaticDsps[i] as Echo).WetLevel = Value;
                                break;
                        }
                        break;
                    case Effects.Flange:
                        switch (Param)
                        {
                            case Params.FlangeDepth:
                                (AudioEffectGlobal.StaticDsps[i] as Flange).Depth = Value;
                                break;
                            case Params.FlangeMix:
                                (AudioEffectGlobal.StaticDsps[i] as Flange).Mix = Value;
                                break;
                            case Params.FlangeRate:
                                (AudioEffectGlobal.StaticDsps[i] as Flange).Rate = Value;
                                break;
                        }
                        break;
                    case Effects.Normalize:
                        switch (Param)
                        {
                            case Params.NormFadeTime:
                                (AudioEffectGlobal.StaticDsps[i] as Normalize).Fadetime = Value;
                                break;
                            case Params.NormMaxAmp:
                                (AudioEffectGlobal.StaticDsps[i] as Normalize).MaxAmp = Value;
                                break;
                            case Params.NormThresh:
                                (AudioEffectGlobal.StaticDsps[i] as Normalize).Threshold = Value;
                                break;
                        }
                        break;
                    case Effects.Oscillator:
                        switch (Param)
                        {
                            case Params.OscWave:
                                (AudioEffectGlobal.StaticDsps[i] as Osc).WaveType = (Osc.Wave)(int)Value;
                                break;
                            case Params.OscRate:
                                (AudioEffectGlobal.StaticDsps[i] as Osc).Rate = Value;
                                break;
                        }
                        break;
                    case Effects.PitchShift:
                        switch (Param)
                        {
                            case Params.PitchPitch:
                                (AudioEffectGlobal.StaticDsps[i] as PitchShift).Pitch = Value;
                                break;
                            case Params.PitchSize:
                                (AudioEffectGlobal.StaticDsps[i] as PitchShift).FFTSize = Value;
                                break;
                        }
                        break;
                    case Effects.Tremolo:
                        switch (Param)
                        {
                            case Params.TremDepth:
                                (AudioEffectGlobal.StaticDsps[i] as Tremolo).Depth = Value;
                                break;
                            case Params.TremDuty:
                                (AudioEffectGlobal.StaticDsps[i] as Tremolo).Duty = Value;
                                break;
                            case Params.TremFreq:
                                (AudioEffectGlobal.StaticDsps[i] as Tremolo).Freq = Value;
                                break;
                            case Params.TremFlatness:
                                (AudioEffectGlobal.StaticDsps[i] as Tremolo).Flatness = Value;
                                break;
                            case Params.TremSkew:
                                (AudioEffectGlobal.StaticDsps[i] as Tremolo).Skew = Value;
                                break;
                            case Params.TremPhase:
                                (AudioEffectGlobal.StaticDsps[i] as Tremolo).Phase = Value;
                                break;
                            case Params.TremShape:
                                (AudioEffectGlobal.StaticDsps[i] as Tremolo).Shape = Value;
                                break;
                            case Params.TremSpread:
                                (AudioEffectGlobal.StaticDsps[i] as Tremolo).Spread = Value;
                                break;
                        }
                        break;
                }
            }
        }
        private float GetParam()
        {
            for (int i = 0; i < AudioEffectGlobal.StaticDsps.Count; i++)
            {
                if (AudioEffectGlobal.StaticDsps[i] == null || !AudioEffectGlobal.StaticDsps[i].Injected || AudioEffectGlobal.StaticDsps[i].ID != ID) continue;
                switch (Effect)
                {
                    case Effects.Chorus:
                        switch (Param)
                        {
                            case Params.ChorusDepth: return (AudioEffectGlobal.StaticDsps[i] as Chorus).Depth;
                            case Params.ChorusMix: return (AudioEffectGlobal.StaticDsps[i] as Chorus).Mix;
                            case Params.ChorusRate: return (AudioEffectGlobal.StaticDsps[i] as Chorus).Rate;
                        }
                        break;
                    case Effects.Distortion:
                        switch (Param)
                        {
                            case Params.DistortLevel: return (AudioEffectGlobal.StaticDsps[i] as Distortion).Level;
                        }
                        break;
                    case Effects.Echo:
                        switch (Param)
                        {
                            case Params.EchoDelay: return (AudioEffectGlobal.StaticDsps[i] as Echo).Delay;
                            case Params.EchoFeedback: return (AudioEffectGlobal.StaticDsps[i] as Echo).Feedback;
                            case Params.EchoDryLevel: return (AudioEffectGlobal.StaticDsps[i] as Echo).DryLevel;
                            case Params.EchoWetLevel: return (AudioEffectGlobal.StaticDsps[i] as Echo).WetLevel;
                        }
                        break;
                    case Effects.Flange:
                        switch (Param)
                        {
                            case Params.FlangeDepth:
                                return (AudioEffectGlobal.StaticDsps[i] as Flange).Depth;
                            case Params.FlangeMix:
                                return (AudioEffectGlobal.StaticDsps[i] as Flange).Mix;
                            case Params.FlangeRate:
                                return (AudioEffectGlobal.StaticDsps[i] as Flange).Rate;
                        }
                        break;
                    case Effects.Normalize:
                        switch (Param)
                        {
                            case Params.NormFadeTime:
                                return (AudioEffectGlobal.StaticDsps[i] as Normalize).Fadetime;
                            case Params.NormMaxAmp:
                                return (AudioEffectGlobal.StaticDsps[i] as Normalize).MaxAmp;
                            case Params.NormThresh:
                                return (AudioEffectGlobal.StaticDsps[i] as Normalize).Threshold;
                        }
                        break;
                    case Effects.Oscillator:
                        switch (Param)
                        {
                            case Params.OscWave:
                                return (float)(AudioEffectGlobal.StaticDsps[i] as Osc).WaveType;
                            case Params.OscRate:
                                return (AudioEffectGlobal.StaticDsps[i] as Osc).Rate;
                        }
                        break;
                    case Effects.PitchShift:
                        switch (Param)
                        {
                            case Params.PitchPitch:
                                return (AudioEffectGlobal.StaticDsps[i] as PitchShift).Pitch;
                            case Params.PitchSize:
                                return (AudioEffectGlobal.StaticDsps[i] as PitchShift).FFTSize;
                        }
                        break;
                    case Effects.Tremolo:
                        switch (Param)
                        {
                            case Params.TremDepth:
                                return (AudioEffectGlobal.StaticDsps[i] as Tremolo).Depth;
                            case Params.TremDuty:
                                return (AudioEffectGlobal.StaticDsps[i] as Tremolo).Duty;
                            case Params.TremFreq:
                                return (AudioEffectGlobal.StaticDsps[i] as Tremolo).Freq;
                            case Params.TremFlatness:
                                return (AudioEffectGlobal.StaticDsps[i] as Tremolo).Flatness;
                            case Params.TremSkew:
                                return (AudioEffectGlobal.StaticDsps[i] as Tremolo).Skew;
                            case Params.TremPhase:
                                return (AudioEffectGlobal.StaticDsps[i] as Tremolo).Phase;
                            case Params.TremShape:
                                return (AudioEffectGlobal.StaticDsps[i] as Tremolo).Shape;
                            case Params.TremSpread:
                                return (AudioEffectGlobal.StaticDsps[i] as Tremolo).Spread;
                        }
                        break;
                }
            }
            return 0;
        }
        public bool Inverted(string flag)
        {
            return flag[0] == '!';
        }
    }
}
