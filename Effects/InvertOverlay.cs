using Celeste.Mod.Backdrops;
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
        private int LastTimelinePosition;
        public EventInstance invertAudio;
        private bool MusicGlitchCutscene;
        private bool Transitioning;
        private bool WasTransitioning;
        private int LoopCount;
        private int Timer;
        private int Last;
        private bool Wait = false;
        public static bool HoldState;
        private int Loops;
        private int LastSaved;
        private static PropertyInfo engineDeltaTimeProp = typeof(Engine).GetProperty("DeltaTime");
        private static float baseTimeRate = 1f;

        public static float playerTimeRate = 1f;
        private float SavedVolume;
        private float Pitch;
        private Player player;
        public static bool ForcedState;
        public static bool EnforceState;
        private string prevColorGrade;

        private float holdTimer;
        private VirtualButton button;
        public static bool UseNormalTimeRate;
        public InvertOverlay(BinaryPacker.Element data) : this(data.Attr("colorgradeFlag"), data.AttrFloat("timeMod")) { }
        public InvertOverlay(string flag, float timeMod)
        {
            this.flag = flag;

            OnTime = timeMod;
            button = (VirtualButton)typeof(Input).GetField("Dash").GetValue(null);
        }

        private bool CheckScene(Scene scene)
        {
            player = (scene as Level).Tracker.GetEntity<Player>();
            if (player is null)
            {
                Reset(scene);
                return false;
            }
            if (player.Dead || player.StateMachine.State == 11)
            {
                if (!HoldState)
                {
                    Reset(scene);
                }
                return false;
            }
            Transitioning = (scene as Level).Transitioning;
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
            if (PianoModule.Session.HasInvert)
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
                if (button.Check)
                {
                    holdTimer += Engine.DeltaTime;
                }
                else
                {
                    holdTimer = 0f;
                    Engine.TimeRate = 1;
                }
                State = ((button.Check && holdTimer >= WaitTime) || (EnforceState && ForcedState));
                level.Session.SetFlag(flag, State);
            }
            if (!CheckScene(scene))
            {
                return;
            }


            /*            #region GlitchMusic
                        if (MusicGlitchCutscene && !Laser)
                        {
                            Event.CurrentMusicEventInstance?.setPitch(Pitch - 0.1f);
                            Event.MusicVolume = 0.5f;
                            MusicGlitch(Loops);
                            return;
                        }
                        #endregion*/

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

                    level.SnapColorGrade(State || (EnforceState && ForcedState) ? "PianoBoy/Inverted" : "none");

                    if (State)
                    {
                        Audio.CurrentMusicEventInstance?.getTimelinePosition(out LastTimelinePosition);
                    }

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

                    if (!State)
                    {
                        Audio.CurrentMusicEventInstance?.setPaused(false);
                        if (Audio.CurrentMusicEventInstance is not null)
                        {
                            Audio.CurrentMusicEventInstance.getParameterValue("PitchShift", out float value, out float finalvalue);
                            if (finalvalue == 0)
                            {
                                Audio.SetMusicParam("PitchShift", 1);
                            }
                            else
                            {
                                Audio.SetMusicParam("PitchShift", 0);
                            }
                        }
                    }
                }
            }
            else
            {
                /*                if(Event.CurrentMusicEventInstance is not null)
                                {
                                    Event.CurrentMusicEventInstance.setPaused(false);
                                    Event.CurrentMusicEventInstance.setParameterValue("PitchShift", 0);
                                }*/
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
        private void MusicGlitch(int loops)
        {
            int random = 0;
            if (Audio.CurrentMusicEventInstance is null)
            {
                return;
            }
            Audio.CurrentMusicEventInstance.getDescription(out EventDescription desc);
            desc.getLength(out int limit);
            if (LoopCount == 0)
            {
                Last = LastTimelinePosition;
                LastSaved = Last;
            }
            if (LoopCount < loops)
            {
                if (!Wait)
                {
                    random = Calc.Random.Range(0, limit);
                    Audio.CurrentMusicEventInstance.setTimelinePosition(random);
                    Wait = true;
                    Timer = 4;
                }
                if (Timer != 0)
                {
                    Timer--;
                    Audio.CurrentMusicEventInstance.setTimelinePosition(random);
                    return;
                }
                Wait = false;
                LoopCount++;
            }
            if (LoopCount == loops)
            {
                LoopCount = 0;
                Wait = false;
                MusicGlitchCutscene = false;
                Audio.MusicVolume = SavedVolume;
                Audio.CurrentMusicEventInstance.setPitch(Pitch);
                Audio.CurrentMusicEventInstance.setTimelinePosition(LastTimelinePosition);
                Audio.CurrentMusicEventInstance.setPaused(false);
                State = false;
                previousState = false;
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

        /* public class MusicGlitch : Entity
    {
        private Coroutine routine;
        private bool Activated;
        private readonly int loops;
        private IEnumerator DistortMusic(int loops)
        {
            int random;
            Event.CurrentMusicEventInstance.getPitch(out float Pitch, out float FinalPitch);
            Event.CurrentMusicEventInstance.setPitch(Pitch - 0.1f);
            Event.MusicVolume = 0.5f;
            InvertOverlay.WaitForGlitch = true;
            Event.CurrentMusicEventInstance.getTimelinePosition(out int position);
            for (int i = 0; i < loops; i++)
            {
                Event.CurrentMusicEventInstance.getDescription(out EventDescription desc);
                desc.getLength(out int length);
                random = Calc.Random.Range(0, length);
                for (float j = 0; j < 5; j += Engine.DeltaTime)
                {
                    Event.CurrentMusicEventInstance.setTimelinePosition(random);
                    yield return null;
                }
            }
            Event.CurrentMusicEventInstance.setTimelinePosition(position);
            yield return null;
        }
        public override void BeenAdded(Scene scene)
        {
            base.BeenAdded(scene);
            routine = new Coroutine(DistortMusic(loops));
            Add(routine);
            Activated = true;
        }
        public MusicGlitch()
        : base(Vector2.Zero)
        {
            loops = Calc.Random.Range(3, 9);
        }
        public override void Update()
        {
            base.Update();
            if (Activated && !routine.Active)
            {
                InvertOverlay.WaitForGlitch = false;
                EjectSelf();
            }
        }
    }*/
    }
}

