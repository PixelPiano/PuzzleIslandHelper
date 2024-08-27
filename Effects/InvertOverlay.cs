using Celeste.Mod.Backdrops;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomBackdrop("PuzzleIslandHelper/InvertOverlay")]
    public class InvertOverlay : Backdrop
    {
        private string flag;
        private float savedTimerate;
        public static float WaitTime;
        private bool previousState;
        public static bool State;
        private static float OnTime;
        public EventInstance invertAudio;
        private bool Transitioning;
        private bool WasTransitioning;
        public static bool HoldState;
        private static PropertyInfo engineDeltaTimeProp = typeof(Engine).GetProperty("DeltaTime");
        private static float baseTimeRate = 1f;

        public static float playerTimeRate = 1f;
        private Player player;
        public static bool ForcedState;
        public static bool EnforceState;

        private float holdTimer;
        public static bool UseNormalTimeRate;
        public bool wasReleased;
        public PianoModuleSettings.InvertActivationModes Mode => PianoModule.Settings.ToggleInvert;
        public InvertOverlay(BinaryPacker.Element data) : this(data.Attr("colorgradeFlag"), data.AttrFloat("timeMod")) { }
        public ButtonBinding Binding => PianoModule.Settings.InvertAbilityBinding;
        public InvertOverlay(string flag, float timeMod)
        {
            this.flag = flag;

            OnTime = timeMod;

        }

        private bool CheckScene(Scene scene)
        {
            if (scene is not Level level || level.GetPlayer() is not Player player)
            {
                Reset(scene);
                return false;
            }
            if(level.Wipe != null)
            {
                return true;
            }
            Transitioning = (scene as Level).Transitioning;
            if (!Transitioning)
            {
                if (player.Dead || player.StateMachine.State == 11)
                {
                    if (!HoldState)
                    {
                        Reset(scene);
                    }
                    return false;
                }
            }
            if (Transitioning)
            {
                if (!WasTransitioning)
                {
                    savedTimerate = Engine.TimeRate;
                }
                WasTransitioning = true;
                Engine.TimeRate = 1;
            }
            else if (WasTransitioning)
            {
                WasTransitioning = false;
                Engine.TimeRate = savedTimerate;
            }
            return true;
        }
        public override void Update(Scene scene)
        {
            base.Update(scene);
            Level level = scene as Level;
            if(level.GetPlayer() is PlayerCalidus)
            {
                State = false;
                return;
            }
            if (PianoModule.Settings.InvertAbility)
            {
                level.Session.SetFlag("invertOverlay");
            }
            if (!level.Session.GetFlag("invertOverlay"))
            {
                return;
            }
            previousState = State;
            if (!HoldState)
            {
                switch (Mode)
                {
                    case PianoModuleSettings.InvertActivationModes.Toggle:
                        if (Binding.Pressed) State = !State;
                        break;
                    case PianoModuleSettings.InvertActivationModes.Hold:
                        bool check = Binding.Check;
                        holdTimer = check ? holdTimer + Engine.DeltaTime : 0;
                        State = check && holdTimer >= WaitTime;
                        break;
                }
            }
            if (EnforceState)
            {
                State = ForcedState;
            }
            if (!State) Engine.TimeRate = 1;
            level.Session.SetFlag(flag, State);
            if (!CheckScene(scene))
            {
                return;
            }

            if (IsVisible(level))
            {
                if (invertAudio != null)
                {
                    invertAudio.getPaused(out bool Paused);
                    if (!Paused && !State)
                    {
                        invertAudio.setPaused(true);
                    }
                }
                if (previousState != State)
                {
                    level.NextColorGrade(State ? "PianoBoy/Inverted" + (PianoModule.Settings.InvertEffectIntensity * 20) : "none", 10f);
                    Engine.TimeRate = State ? OnTime : Engine.TimeRate;
                    if (State)
                    {
                        Audio.CurrentMusicEventInstance?.setPaused(true);
                        invertAudio = Audio.Play("event:/PianoBoy/invertAmbience");
                    }
                    else
                    {
                        invertAudio?.setPaused(true);
                        Audio.CurrentMusicEventInstance?.setPaused(false);
                    }
                }
            }
        }

        public override void Ended(Scene scene)
        {
            base.Ended(scene);
            if (invertAudio is not null)
            {
                invertAudio.stop(STOP_MODE.IMMEDIATE);
            }
            EnforceState = false;
        }
        internal static void Load()
        {
            WaitTime = 1.5f;
            EnforceState = false;
            ForcedState = false;
            Everest.Events.Level.OnTransitionTo += Transition;
            On.Celeste.Player.Update += PlayerUpdate;
        }
        internal static void Unload()
        {
            Everest.Events.Level.OnTransitionTo -= Transition;
            On.Celeste.Player.Update -= PlayerUpdate;
        }
        private static void Transition(Level level, LevelData data, Vector2 dir)
        {
            Engine.TimeRate = State ? OnTime : 1;
        }
        private static void PlayerUpdate(On.Celeste.Player.orig_Update orig, Player self)
        {
            float deltaTime = Engine.DeltaTime;
            if ((!UseNormalTimeRate && State) || (EnforceState && ForcedState))
            {
                engineDeltaTimeProp.SetValue(null, Engine.RawDeltaTime * Engine.TimeRateB * baseTimeRate * playerTimeRate, null);
            }
            orig.Invoke(self);
            if (State)
            {
                engineDeltaTimeProp.SetValue(null, deltaTime, null);

            }
        }
        private void Reset(Scene scene)
        {
            if (invertAudio != null)
            {
                invertAudio.stop(STOP_MODE.IMMEDIATE);
            }
            (scene as Level).SnapColorGrade("none");
            (scene as Level).Session.SetFlag(flag, false);
            Audio.CurrentMusicEventInstance?.setPaused(false);
            State = false;
            previousState = false;
            Engine.TimeRate = 1;
        }
        public static void ForceState(bool state)
        {
            ForcedState = state;
            EnforceState = true;
        }
        public static void ResetState()
        {
            ForcedState = false;
            EnforceState = false;
        }
    }
}

