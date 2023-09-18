using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Threading;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    public class InvertOverlay : Backdrop
    {
        private string flag;
        public static float WaitTime;
        private static bool previousState;
        public static bool State;
        private static float OnTime;
        private int LastTimelinePosition;
        public EventInstance invertAudio;
        private bool MusicGlitchCutscene;
        private int LoopCount;
        private int Timer;
        private int Last;
        private bool Wait = false;
        private int Loops;
        private int LastSaved;
        private static PropertyInfo engineDeltaTimeProp = typeof(Engine).GetProperty("DeltaTime");
        private static float baseTimeRate = 1f;

        private static float playerTimeRate = 1f;
        private float SavedVolume;
        private float Pitch;
        private Player player;
        public static bool ForcedState;
        public static bool EnforceState;


        private float holdTimer;
        private VirtualButton button;

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
                Reset(scene);
                return false;
            }
            if ((scene as Level).Transitioning)
            {
                Engine.TimeRate = 1;
            }
            else if (IsVisible(scene as Level) && (scene as Level).Session.GetFlag("invertOverlay"))
            {
                if (State)
                {
                    Engine.TimeRate = OnTime;
                }
                else
                {
                    Engine.TimeRate = 1;
                }
            }
            return true;
        }
        public override void Update(Scene scene)
        {
            base.Update(scene);
            if (PianoModule.SaveData.HasInvert)
            {
                (scene as Level).Session.SetFlag("invertOverlay");
            }
            if (!(scene as Level).Session.GetFlag("invertOverlay"))
            {
                return;
            }
            if (button.Check)
            {
                holdTimer += Engine.DeltaTime;
            }
            else
            {
                holdTimer = 0f;
            }
            previousState = State;
            State = (button.Check && holdTimer >= WaitTime) || (EnforceState && ForcedState);
            (scene as Level).Session.SetFlag(flag, State);
            if (!CheckScene(scene))
            {
                return;
            }


            /*            #region GlitchMusic
                        if (MusicGlitchCutscene && !State)
                        {
                            Audio.CurrentMusicEventInstance?.setPitch(Pitch - 0.1f);
                            Audio.MusicVolume = 0.5f;
                            MusicGlitch(Loops);
                            return;
                        }
                        #endregion*/

            if (IsVisible(scene as Level))
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

                    (scene as Level).SnapColorGrade(State || (EnforceState && ForcedState) ? "PianoBoy/Inverted" : "default");

                    if (State)
                    {
                        Audio.CurrentMusicEventInstance?.getTimelinePosition(out LastTimelinePosition);
                    }

                    Engine.TimeRate = State ? OnTime : 1;
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
                /*                if(Audio.CurrentMusicEventInstance is not null)
                                {
                                    Audio.CurrentMusicEventInstance.setPaused(false);
                                    Audio.CurrentMusicEventInstance.setParameterValue("PitchShift", 0);
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
            engineDeltaTimeProp.SetValue(null, Engine.RawDeltaTime * Engine.TimeRateB * baseTimeRate * playerTimeRate, null);
            orig.Invoke(self);
            engineDeltaTimeProp.SetValue(null, deltaTime, null);


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
            (scene as Level).SnapColorGrade("default");
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
            Audio.CurrentMusicEventInstance.getPitch(out float Pitch, out float FinalPitch);
            Audio.CurrentMusicEventInstance.setPitch(Pitch - 0.1f);
            Audio.MusicVolume = 0.5f;
            InvertOverlay.WaitForGlitch = true;
            Audio.CurrentMusicEventInstance.getTimelinePosition(out int position);
            for (int i = 0; i < loops; i++)
            {
                Audio.CurrentMusicEventInstance.getDescription(out EventDescription desc);
                desc.getLength(out int length);
                random = Calc.Random.Range(0, length);
                for (float j = 0; j < 5; j += Engine.DeltaTime)
                {
                    Audio.CurrentMusicEventInstance.setTimelinePosition(random);
                    yield return null;
                }
            }
            Audio.CurrentMusicEventInstance.setTimelinePosition(position);
            yield return null;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
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
                RemoveSelf();
            }
        }
    }*/
    }
}

