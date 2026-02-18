using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using YamlDotNet.Core.Tokens;
using static Celeste.MoonGlitchBackgroundTrigger;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{
    [Tracked]
    public class Envelope : Component
    {
        public enum States
        {
            Attack,
            Sustain,
            Release,
            Delay
        }
        public enum Modes
        {
            Oneshot,
            Persist,
            Looping,
            YoyoOneshot,
            YoyoLooping
        }
        public Modes Mode;
        public float this[States state]
        {
            get
            {
                return state switch
                {
                    States.Attack => Attack,
                    States.Sustain => Sustain,
                    States.Release => Release,
                    States.Delay => Delay
                };
            }
        }
        /// <summary>
        /// How long it takes for <see cref="Power"/> to go from <see cref="float"/> 0 to <see cref="float"/> 1. <br/> 
        /// Used after <see cref="Delay"/> is completed. <br/>
        /// Uses the ease function <see cref="AttackEase"/> (defaults to <see cref="Ease.SineIn"/>)
        /// </summary>
        public float Attack;
        /// <summary>
        /// How long <see cref="Power"/> will linger at <see cref="float"/> 1. <br/> 
        /// Used after <see cref="Attack"/> is completed.
        /// </summary>
        public float Sustain;
        /// <summary>
        /// How long it takes for <see cref="Power"/> to go from <see cref="float"/> 1 to <see cref="float"/> 0. <br/> 
        /// Used after <see cref="Sustain"/> is completed.
        /// </summary>
        public float Release;
        /// <summary>
        /// How long <see cref="Power"/> will linger at <see cref="float"/> 0. <br/> 
        /// Used after <see cref="Release"/> is completed. <br/> 
        /// Uses the ease function <see cref="ReleaseEase"/> (defaults to <see cref="Ease.SineOut"/>)
        /// </summary>
        public float Delay;

        public float Power;
        public Ease.Easer AttackEase, ReleaseEase;
        public Ease.Easer CurrentEaser
        {
            get
            {
                return State switch
                {
                    States.Attack => AttackEase,
                    States.Release => ReleaseEase,
                    _ => Ease.Linear
                };
            }
        }
        public float Eased => CurrentEaser(Power);
        public float TimerPercent => 1 - (timer / timerDuration);
        private float timer;
        private float timerDuration;
        private bool start;
        public States State, EndState;
        private States startState;
        /// <summary>
        /// Invoked at the end of Update.
        /// </summary>
        public Action<Envelope> OnUpdate;
        /// <summary>
        /// Invoked during <see cref="SetStates(States,bool)"/> if <see cref="State"/> will be changed.
        /// </summary>
        public Action<Envelope> OnStateChanged;
        public Action<Envelope> OnComplete;
        private bool startReversed;
        public bool SkipEmptyStates = true;
        public Envelope(float attack, float sustain, float release, float delay, bool reversed, bool start = false, Modes mode = Modes.Oneshot, Ease.Easer attackEase = null, Ease.Easer releaseEase = null, States startingState = States.Attack, States endState = States.Attack) : base(false, false)
        {
            Attack = attack;
            Sustain = sustain;
            Release = release;
            Delay = delay;
            AttackEase ??= Ease.SineIn;
            ReleaseEase ??= Ease.SineOut;
            this.start = start;
            PrevState = startState = State = startingState;
            this.EndState = endState;
            Mode = mode;
            startReversed = reversed;
        }
        public override void Added(Entity entity)
        {
            base.Added(entity);
            if (start)
            {
                Start(false, startReversed);
            }
        }
        public void Stop()
        {
            Active = false;
            timer = 0;
            timerDuration = 0;
        }
        public void Resume()
        {
            Active = !(Attack == 0 && Sustain == 0 && Release == 0 && Delay == 0);
        }
        public void Start(bool replaceStartState = false, bool reversed = false) => Start(State, replaceStartState, reversed);
        public void Start(States state, bool replaceStartState = false, bool reversed = false)
        {
            if (Attack == 0 && Sustain == 0 && Release == 0 && Delay == 0)
            {
                Active = false;
                return;
            }
            starting = true;
            SetState(state, reversed);
            justStarted = true;
            starting = false;
            if (replaceStartState)
            {
                startState = state;
            }
            Active = true;
        }
        public void Start(States state, States endState, bool replaceStartState = false, bool reversed = false)
        {
            if (Attack == 0 && Sustain == 0 && Release == 0 && Delay == 0)
            {
                Active = false;
                return;
            }
            starting = true;
            Start(state, replaceStartState, reversed);
            justStarted = true;
            starting = false;
            this.EndState = endState;
        }
        public States PrevState;
        private bool yoyoHit;
        private bool justStarted;
        private bool starting;
        public bool TryFindNextUsableState(States from, out States next)
        {
            States orig = from;
            next = from;
            if (!SkipEmptyStates || this[from] > 0)
            {
                return true;
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    if (this[from] > 0)
                    {
                        next = from;
                        return true;
                    }
                    from = NextStateFrom(from, dir);
                }
                next = orig;
                return false;
            }
        }
        public void SetState(States state)
        {
            if (!TryFindNextUsableState(state, out States next))
            {
                Stop();
                return;
            }
            justStarted = false;
            float nextTimer = this[next];

            Power = next switch
            {
                States.Attack => dir == -1 ? 1 : 0,
                States.Sustain => 1,
                States.Release => dir == -1 ? 0 : 1,
                States.Delay => 0,
                _ => 0
            };
            PrevState = State;
            State = next;
            if (!starting)
            {
                OnStateChanged?.Invoke(this);
            }
            timer = timerDuration = nextTimer;
        }
        public void SetState(States state, bool reversed)
        {
            dir = reversed ? -1 : 1;
            SetState(state);
        }
        private int dir = 1;
        public void AdvanceState()
        {
            SetState(NextState(dir));
        }
        public States NextState(int direction)
        {
            int s = (int)State + dir;
            if (s < 0) s = 3;
            if (s > 3) s = 0;
            return (States)s;
        }
        public States NextStateFrom(States from, int direction)
        {
            int s = (int)from + dir;
            if (s < 0) s = 3;
            if (s > 3) s = 0;
            return (States)s;
        }
        public bool Reversed => dir < 0;
        public override void Update()
        {
            base.Update();
            float prevPower = Power;
            States prev = State;
            timer -= Engine.DeltaTime;
            Power = State switch
            {
                States.Attack => Reversed ? 1 - TimerPercent : TimerPercent,
                States.Sustain => 1,
                States.Release => Reversed ? TimerPercent : 1 - TimerPercent,
                States.Delay => 0,
            };
            if (timer < 0)
            {
                timer = 0;
                if (!justStarted && NextState(dir) == EndState)
                {
                    switch (Mode)
                    {
                        case Modes.Oneshot:
                            OnComplete?.Invoke(this);
                            Stop();
                            RemoveSelf();
                            break;
                        case Modes.Persist:
                            OnComplete?.Invoke(this);
                            Stop();
                            break;
                        case Modes.Looping:
                            SetState(startState, startReversed);
                            break;
                        case Modes.YoyoOneshot:
                            if (Reversed == startReversed)
                            {
                                SetState(State, dir == 1);
                                yoyoHit = true;
                            }
                            else
                            {
                                OnComplete?.Invoke(this);
                                Stop();
                                RemoveSelf();
                            }
                            break;
                        case Modes.YoyoLooping:
                            SetState(State, dir == 1);
                            break;
                    }
                }
                else
                {
                    AdvanceState();
                }
            }
            OnUpdate?.Invoke(this);
        }
    }
}
