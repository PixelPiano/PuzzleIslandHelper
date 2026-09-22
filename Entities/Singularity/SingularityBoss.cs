using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using YamlDotNet.Core.Tokens;
using static Celeste.Mod.DecalRegistry;
using static Celeste.Mod.PuzzleIslandHelper.Boss.ActionRegistry;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Singularity.SingularityBoss;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Singularity
{
    //[CustomEntity("PuzzleIslandHelper/Singularity")]
    [Tracked]
    public class SingularityBoss : Actor
    {
        public enum Phases
        {
            Easy,
            Medium,
            Hard,
            Final
        }
        public string PreviousAttackType = "";
        public const double RubberbandFactor = 0.0099999997764825821;

        public static ActionRegistryHandler TestAction;
        public static Coroutine TestActionCoroutine;
        [OnLoad]
        public static void Load()
        {
            TestAction = null;
            TestActionCoroutine = null;
        }
        [Command("bossset", "")]
        public static void BossSet(string action)
        {
            if (RegisteredActions.TryGetValue(action, out ActionInfo value))
            {
                TestAction = value.Handler;
                Engine.Commands.Log("TestAction set to " + action, Color.Lime);
            }
            else
            {
                Engine.Commands.Log('"' + action + "\" not found in RegisteredActions", Color.Red);
            }
        }
        [Command("set_value", "")]
        public static void SetValue(string name, string value)
        {
            TestAction?.SetValue(name, value);
        }
        [Command("play", "")]
        public static void Play()
        {
            if (TestAction == null)
            {
                Engine.Commands.Log("TestAction is null.");
                return;
            }
            SingularityBoss s = Engine.Scene.Tracker.GetEntity<SingularityBoss>();
            TestActionCoroutine?.RemoveSelf();
            s.Add(TestActionCoroutine = new Coroutine(TestAction.Routine(s)));
        }
        public static bool RubberbandApproach(Entity entity, Vector2 position, int exitDistance = 2, double factor = 0.0099999997764825821)
        {
            if (Vector2.DistanceSquared(entity.Position, position) > exitDistance * exitDistance)
            {
                entity.Position += (position - entity.Position) * (1f - (float)Math.Pow(factor, Engine.DeltaTime));
                return false;
            }
            else
            {
                entity.Position = position;
                return true;
            }
        }
        public static float RubberbandApproach(float input, float target, int exitDistance = 2, double factor = 0.0099999997764825821)
        {
            if (MathHelper.Distance(input, target) > exitDistance)
            {
                return input += (target - input) * (1f - (float)Math.Pow(factor, Engine.DeltaTime));
            }
            else
            {
                return target;
            }
        }
        public static IEnumerator RubberbandTo(Entity entity, Vector2 position, int exitDistance = 2, double factor = 0.0099999997764825821, Action onUpdate = null)
        {
            while (Vector2.DistanceSquared(entity.Position, position) > exitDistance * exitDistance)
            {
                entity.Position += (position - entity.Position) * (1f - (float)Math.Pow(factor, Engine.DeltaTime));
                onUpdate?.Invoke();
                yield return null;
            }
        }

        public static IEnumerator WindUp(Entity entity, float angle, float dist, float time, Action<float> onUpdate = null) => WindUp(entity, Calc.AngleToVector(angle, 1), dist, time, onUpdate);
        public static IEnumerator WindUp(Entity entity, Vector2 direction, float dist, float time, Action<float> onUpdate = null)
        {
            Vector2 windUpDirection = direction;
            Vector2 p = entity.Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                entity.Position = p + windUpDirection * Ease.SineIn(i) * dist;
                onUpdate?.Invoke(Ease.SineIn(i));
                yield return null;
            }
        }
        [Command("test_phase", "test an attack from Singularity.cs")]
        public static void TestAttack(string phaseName)
        {
            SingularityBoss s = Engine.Scene.Tracker.GetEntity<SingularityBoss>();
            if (Enum.TryParse(phaseName, out Phases result))
            {
                s.SwitchPhase(result);
            }
            else
            {
                Engine.Commands.Log("Invalid Phase name", Color.Yellow);
            }
        }
        [Command("test_pattern", "test a pattern from SingularityMoveset.xml")]
        public static void TestPattern(string patternName)
        {
            Engine.Scene.Tracker.GetEntity<SingularityBoss>()?.StartPattern(patternName);
        }
        public void StartPattern(string patternName)
        {
            Add(new Coroutine(Pattern(patternName)));
        }
        public void SwitchPhase(Phases newPhase)
        {
            CurrentPhase = newPhase;
            BossfightCoroutine.Replace(Coreography(newPhase));
        }
        #region Variables
        public Phases CurrentPhase;
        public bool Tri;
        public float Rotation;
        internal Orb R, G, B;
        public Circle Circle;
        public Hitbox Collision;
        public float Radius;
        public float RadiusOffset;
        public float SizeOffset;
        public float Distance = 8;
        public float RotationSpeed = 1f;
        public Vector2 Speed;
        public PlayerCollider PlayerCollider;
        public FlagList Flag;
        public Vector2 RenderOffset;
        public CutsceneEntity Cutscene;
        internal List<Orb> Orbs = [];
        private bool cutscenePlayed;
        public bool Deadly;
        public int Health;
        public const int StIdle = 0;
        public const int StSlam = 1;
        public const int StScatter = 2;
        public const int StSpikeSlam = 3;
        public const int StRoll = 4;
        public const int StDummy = 5;
        public const int StDart = 6;
        public StateMachine StateMachine;
        public event Action<Player> OnPlayerCollide;
        public float RotationSpeedMult = 1;
        public int State
        {
            get => StateMachine.State;
            set => StateMachine.ForceState(value);
        }
        public Collision StateCollideH, StateCollideV;
        public SingularityBoss(EntityData data, Vector2 offset) : this(data.Position + offset)
        {
            Flag = data.FlagList("flag");
        }
        public Vector2 OrigPosition;
        public Coroutine BossfightCoroutine;
        public Func<Player, bool> StatePlayerCollide;
        #endregion
        public PatternInfo? CurrentPattern;
        public Vector2 PrevPosition;
        public bool UpdateCircle = true;
        public bool UpdateHitbox = true;
        public event Action OnTakeDamage;
        public Vector2 Shake;
        public BetterShaker Shaker;

        public SingularityBoss(Vector2 position) : base(position)
        {
            KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.O, Play);
            OrigPosition = position;
            Collider = Collision = new Hitbox(0, 0);
            Circle = new Circle(0);
            Radius = 8;
            Add(PlayerCollider = new PlayerCollider(OnPlayer, Circle));
            Tag |= Tags.TransitionUpdate;
            StateMachine = new StateMachine();
            Add(StateMachine);
            StateMachine.SetCallbacks(StIdle, IdleUpdate, null, IdleBegin, IdleEnd);
            StateMachine.SetCallbacks(StSlam, SlamUpdate, SlamRoutine, SlamBegin, SlamEnd);
            StateMachine.SetCallbacks(StScatter, ScatterUpdate, ScatterRoutine, ScatterBegin, ScatterEnd);
            StateMachine.SetCallbacks(StSpikeSlam, SpikeSlamUpdate, SpikeSlamRoutine, SpikeSlamBegin, SpikeSlamEnd);
            StateMachine.SetCallbacks(StRoll, RollUpdate, RollRoutine, RollBegin, RollEnd);
            StateMachine.SetCallbacks(StDummy, DummyUpdate, null, DummyBegin, DummyEnd);
            Add(BossfightCoroutine = new Coroutine(false));
            Add(Shaker = new BetterShaker(OnShake));
            //Add(new Coroutine(originTest()));
        }
        private IEnumerator originTest()
        {
            while (true)
            {
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    JustifyOrbs(i, 0);
                    yield return null;
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    JustifyOrbs(1, i);
                    yield return null;
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    JustifyOrbs(1 - i, 1);
                    yield return null;
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    JustifyOrbs(0, 1 - i);
                    yield return null;
                }
            }
        }
        public void StartShaking(float time = -1)
        {
            Shaker.ShakeFor(time);
        }
        public void StopShaking()
        {
            Shaker.StopShaking();
        }
        public void OnShake(Vector2 shake)
        {
            Shake += shake;
        }
        public IEnumerator Pattern(string patternName)
        {
            if (RegisteredPatterns.TryGetValue(patternName, out PatternInfo pattern))
            {
                while (true)
                {
                    Coroutine patternRoutine = new Coroutine(pattern.Routine(this));
                    Add(patternRoutine);
                    while (!patternRoutine.Finished && patternRoutine.Active)
                    {
                        yield return null;
                    }
                    if (string.IsNullOrEmpty(pattern.Next) || !RegisteredPatterns.TryGetValue(pattern.Next, out pattern))
                    {
                        break;
                    }
                }
            }
            else
            {
                Engine.Commands.Log("failed");
            }
            yield return null;
        }
        [Command("test_spike_slam", "")]
        public static void TestSpikeSlam()
        {
            SingularityBoss s = Engine.Scene.Tracker.GetEntity<SingularityBoss>();
            s.SpikeSlam(3, 3);
        }
        [Command("print_test_values", "")]
        public static void PrintTestValues()
        {
            TestAction?.PrintValues();
        }
        public IEnumerator Coreography(Phases startPhase)
        {
            CurrentPhase = startPhase;
            while (true)
            {
                Coroutine patternRoutine = new Coroutine(Pattern("easy"));
                Add(patternRoutine);
                int health = 10;
                while (health > 0)
                {
                    yield return null;
                }
                patternRoutine.Cancel();
                patternRoutine.RemoveSelf();
                if (CurrentPhase is Phases.Final)
                {
                    break;
                }
                else
                {
                    CurrentPhase = (Phases)((int)CurrentPhase + 1);
                }
            }
            RemoveSelf();
        }
        public void Continue()
        {
            Idle();
        }

        private float prevRotationSpeed;
        private float prevRotationMult;
        #region Dummy
        public void DummyBegin()
        {

        }
        public int DummyUpdate()
        {
            return StDummy;
        }
        public void DummyEnd()
        {

        }
        #endregion

        //attack finished, be open to polish and adjustments
        #region SpikeSlam
        //attack functionable but not completely polished. orb variant has not been tested or implemented.
        private Vector2 spikeSlamTarget;
        private int spikeSlamLoops;
        private bool spikeSlamImpact;
        private int spikeLength;
        private float spkslmIntensity = 1;
        private string[] spikeSlamMarkers;
        private bool spikeSlamTargetPlayer;
        private bool spikeSlamBlockWalls;
        public float spikeWindUp = 32;
        public float spikeWindUpTime = 1f;
        public float spikeStartSpeed = 75;
        public float spikeTargetSpeed = 900;
        public float spikeMaxMove = 600;
        public float spikeRotationSpeedMult = 70;
        public float spikeDuration;
        public void SpikeSlam(int loops, int length, float speedMult = 1, float spikeDuration = 0.6f, bool targetPlayer = false, bool blockWalls = false)
        {
            spikeSlamLoops = loops;
            spikeLength = length;
            spkslmIntensity = speedMult;
            spikeSlamTargetPlayer = targetPlayer;
            spikeSlamBlockWalls = blockWalls;
            this.spikeDuration = spikeDuration;
            setSpikeSlamPosition();
            StateMachine.ForceState(StSpikeSlam);
        }
        public void SpikeSlam(string[] markers, int loops, int length, float speedMult = 1, float spikeDuration = 0.6f, bool targetPlayer = false, bool blockWalls = false)
        {
            spikeSlamMarkers = markers;
            SpikeSlam(loops, length, speedMult, spikeDuration, targetPlayer, blockWalls);
        }
        public int SpikeSlamUpdate()
        {
            return StSpikeSlam;
        }


        private IEnumerator prepareSpikeSlam(float mult)
        {
            if (spikeSlamTargetPlayer && Scene.GetPlayer() is Player player)
            {
                EaseToAngle(Math.Sign(player.CenterX - X), MathHelper.TwoPi * 3, 1);
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    RubberbandApproach(this, player.TopCenter - Vector2.UnitY * (40 + spikeWindUp), 5, RubberbandFactor / Math.Max(1, mult * 0.52f));
                    yield return null;
                }
                JustifyOrbs(0.5f, 1);
            }
            else
            {
                setSpikeSlamPosition();
                EaseToAngle(Math.Sign(spikeSlamTarget.X - X), MathHelper.TwoPi * 3, 1);
                yield return RubberbandTo(this, spikeSlamTarget, 5, RubberbandFactor / Math.Max(1, mult * 0.52f));
                JustifyOrbs(0.5f, 1);
                yield return WindUp(this, -MathHelper.PiOver2, spikeWindUp, spikeWindUpTime * (float)(1 - Math.Min(0.6f, 0.2 * (mult - 1))));

            }
        }
        private IEnumerator doSpikeSlam(float mult, float startSpeed, float targetSpeed, int slamNum)
        {
            Speed.Y = startSpeed;
            spikeSlamImpact = false;
            SquishTo(-0.3f, 0.4f, false); //falling deform
            while (!spikeSlamImpact)
            {
                Speed.Y = Calc.Approach(Speed.Y, targetSpeed, spikeMaxMove * mult * Engine.DeltaTime);
                RotationSpeedMult = 1 + Speed.Y / targetSpeed * spikeRotationSpeedMult * (mult / 3f);
                yield return null;
            }

            RotationSpeedMult = 0;
            SquishTo(0.5f, 0.16f, true); //impact deform
            Speed.Y = 0;

            if (slamNum < spikeSlamLoops - 1)
            {
                yield return 0.4f;
                Unsquish(0.2f, true);
                RotationSpeedMult = 1;
                yield return new SwapImmediately(prepareSpikeSlam(mult + 1));
            }
            else
            {
                yield return 0.4f;
                Unsquish(0.2f, true);
                yield return 1.5f;
            }
        }

        public IEnumerator SpikeSlamRoutine()
        {
            float mult = Math.Max(1, spkslmIntensity);
            float startSpeed = spikeStartSpeed * Math.Max(1, spkslmIntensity * 0.75f);
            float targetSpeed = spikeTargetSpeed * Math.Max(1, spkslmIntensity * 0.55f);
            //speedMult
            //spikeTime
            UpdateHitbox = true;
            if (spikeSlamBlockWalls)
            {
                EaseToAngle(1, MathHelper.TwoPi, 1);
                yield return RubberbandTo(this, OrigPosition, 5, RubberbandFactor / Math.Max(1, mult * 0.52f));
                JustifyOrbs(0.5f, 0);
                yield return WindUp(this, MathHelper.PiOver2, spikeWindUp, spikeWindUpTime * (float)(1 - Math.Min(0.6f, 0.2 * (mult - 1))));
                //windUpTime decreases by .2 for every whole value in spikeSlamMult after 2 until the difference is 0.6

                Speed.Y = -startSpeed;
                spikeSlamImpact = false;
                SquishTo(-0.3f, 0.4f, false);
                while (!spikeSlamImpact)
                {
                    Speed.Y = Calc.Approach(Speed.Y, -targetSpeed, spikeMaxMove * mult * Engine.DeltaTime);
                    RotationSpeedMult = -(1 + Speed.Y / targetSpeed * spikeRotationSpeedMult * (mult / 3f));
                    yield return null;
                }
                RotationSpeedMult = 0;
                SquishTo(0.5f, 0.3f, true);
                Speed.Y = 0;
                yield return 2;
                Unsquish(0.5f, true);
                yield return RubberbandTo(this, OrigPosition, 5, RubberbandFactor / Math.Max(1, mult * 0.52f));
            }
            yield return new SwapImmediately(prepareSpikeSlam(mult));
            for (int i = 0; i < spikeSlamLoops; i++)
            {
                yield return new SwapImmediately(doSpikeSlam(mult, startSpeed, targetSpeed, i));
            }
            yield return RubberbandTo(this, OrigPosition);
            Continue();
        }
        private void setSpikeSlamPosition()
        {
            if (spikeSlamTargetPlayer) return;
            spikeSlamTarget = OrigPosition;
            if (spikeSlamMarkers != null && spikeSlamMarkers.Length > 0)
            {
                if (spikeSlamMarkers.Length > 1)
                {
                    int markerIndex = Calc.Random.Range(0, spikeSlamMarkers.Length - 1);
                    if (Marker.TryFind(spikeSlamMarkers[markerIndex], out Vector2 a))
                    {
                        spikeSlamTarget = a;
                        if (Marker.TryFind(spikeSlamMarkers[markerIndex + 1], out Vector2 b))
                        {
                            spikeSlamTarget = Vector2.Lerp(a, b, Calc.Random.Range(0f, 1f));
                        }
                    }
                }
                else if (Marker.TryFind(spikeSlamMarkers[0], out Vector2 a))
                {
                    spikeSlamTarget = a;
                }
            }
            spikeSlamTarget.X -= spikeSlamTarget.X % 8;
        }
        public void SpikeSlamBegin()
        {
            prevRotationSpeed = RotationSpeed;
            prevRotationMult = RotationSpeedMult;
            Collider.Height = Radius;
            UpdateHitbox = false;
            setSpikeSlamPosition();
            StateCollideV = spikeSlamCollideV;
        }
        public void SpikeSlamEnd()
        {
            RotationSpeed = prevRotationSpeed;
            RotationSpeedMult = 0;
            foreach (Orb orb in Orbs)
            {
                orb.Sprite.JustifyOrigin(0.5f, 0.5f);
                orb.Sprite.Y = 0;
            }
            UpdateHitbox = true;
            UpdateColliders(Radius);
            StateCollideV = null;
            spikeSlamMarkers = null;
            foreach (waveSpike waveSpike in Scene.Tracker.GetEntities<waveSpike>())
            {
                if (!waveSpike.StartedExit && !waveSpike.Auto)
                {
                    waveSpike.Exit(0.5f);
                }
            }
        }
        private void spikeSlamCollideV(CollisionData data)
        {
            if (!spikeSlamImpact)
            {
                int dir = Math.Sign(Speed.Y);
                if (dir == 0)
                {
                    dir = Math.Sign(PrevPosition.Y - Position.Y);
                    PushOutOfFloors(dir);
                    return;
                }
                PushOutOfFloors(dir);
                Spikes.Directions d = dir == 1 ? Spikes.Directions.Up : Spikes.Directions.Down;
                spikeSlamImpact = true;
                int xOffset = 8 * spikeLength;
                Vector2 p = Center + dir * Vector2.UnitY * Height / 2;
                if (dir != 1) p = p.Floor();
                float duration = dir == -1 ? 0.3f : spikeDuration;//0.6f - (0.2f * (1 - spkslmIntensity));
                bool isAuto = dir == 1;
                Scene.Add(new waveSpike(p, 1, xOffset, xOffset, d, "default", duration, isAuto, !isAuto));
                Scene.Add(new waveSpike(p - Vector2.UnitX * xOffset, -1, xOffset, xOffset, d, "default", duration, isAuto, !isAuto));
                SceneAs<Level>().Shake(0.3f);
            }
        }
        #endregion
        //bug test
        #region Scatter
        private int scatterBounces;
        private float scatterSpeed;
        private Orb[] scatterOrbs;
        public void Scatter(int bounces, float speed, params Orb[] orbs)
        {
            if (orbs == null || orbs.Length == 0) scatterOrbs = [.. Orbs];
            else scatterOrbs = orbs;
            Speed = Vector2.Zero;
            scatterBounces = bounces;
            scatterSpeed = speed;
            StateMachine.ForceState(StScatter);
        }
        public void ScatterBegin()
        {
            Engine.Commands.Log("Singularity- ScatterBegin", Color.Lime);
            Speed = Vector2.Zero;
        }
        public void ScatterEnd()
        {
            scatterOrbs = null;
            Collidable = true;
        }
        public int ScatterUpdate()
        {
            //Position += (scatterPosition - Position) * (1f - (float)Math.Pow(0.0099999997764825821, Engine.DeltaTime));
            return StScatter;
        }
        public IEnumerator ScatterRoutine()
        {
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                RotationSpeedMult = 1 - i;
                yield return null;
            }
            RotationSpeedMult = 0;
            yield return 0.5f;
            bool combo = scatterOrbs.Length != Orbs.Count;
            foreach (Orb orb in scatterOrbs)
            {
                orb.Bounce(scatterBounces, scatterSpeed, 1);
            }
            Orb track = scatterOrbs[0];
            while (!track.Deadly) yield return null;
            Collidable = false;
            if (!combo)
            {
                bool atLeastOneActive = true;
                while (atLeastOneActive)
                {
                    atLeastOneActive = false;
                    foreach (Orb orb in scatterOrbs)
                    {
                        if (orb.bouncesRemaining > 0)
                        {
                            atLeastOneActive = true;
                            break;
                        }
                    }
                    yield return null;
                }
                foreach (Orb orb in scatterOrbs)
                {
                    orb.Auto();
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    RotationSpeedMult = i;
                    yield return null;
                }
                foreach (Orb orb in scatterOrbs)
                {
                    while (orb.StateMachine.State != Orb.StAuto)
                    {
                        yield return null;
                    }
                }
                Collidable = true;
            }
            Continue();
        }
        #endregion
        //FINISHED
        #region Idle
        private float idleSpeedMult = 1;
        private Vector2? idleTarget = null;
        private string[] idleMarkers;
        private float durationAtTarget;
        private float atTargetTimer;
        private int idlePositionsIndex;
        private Vector2[] idlePositions;
        private bool routineMove;
        public void Idle()
        {
            durationAtTarget = atTargetTimer = -1;
            idlePositionsIndex = 0;
            idleMarkers = null;
            idleSpeedMult = 1;
            idleTarget = null;
            StateMachine.ForceState(StIdle);
        }
        public void Idle(Vector2 start, float speedMult = 1, bool routineMove = false)
        {
            this.routineMove = routineMove;
            idleTarget = start;
            idleSpeedMult = speedMult;
            StateMachine.ForceState(StIdle);
        }
        public void Idle(float timeSpentAtEachTarget = -1, float speedMult = 1, params string[] markers)
        {
            durationAtTarget = atTargetTimer = timeSpentAtEachTarget;
            idleMarkers = markers;
            idleSpeedMult = speedMult;
            idlePositions = new Vector2[idleMarkers.Length];
            for (int i = 0; i < idleMarkers.Length; i++)
            {
                if (Marker.TryFind(idleMarkers[i], out Vector2 p))
                {
                    idlePositions[i] = p;
                }
            }
            idleTarget = idlePositions[0];
            StateMachine.ForceState(StIdle);
        }
        public void IdleBegin()
        {
            Engine.Commands.Log("Singularity - IdleBegin", Color.Gray);
            idlePositionsIndex = 0;
        }
        public void IdleEnd()
        {
            routineMove = false;
            idleTarget = null;
        }
        public int IdleUpdate()
        {
            if (RotationSpeedMult != 1)
            {
                RotationSpeedMult = Calc.Approach(RotationSpeedMult, 1, Engine.DeltaTime * 5f);
            }
            bool atTarget = false;
            if (idleTarget.HasValue)
            {
                atTarget = RubberbandApproach(this, idleTarget.Value, 2, RubberbandFactor * idleSpeedMult);
            }
            if (atTarget && idlePositions != null)
            {
                if (atTargetTimer > 0)
                {
                    atTargetTimer -= Engine.DeltaTime;
                }
                if (atTargetTimer <= 0)
                {
                    idlePositionsIndex = (idlePositionsIndex + 1) % idlePositions.Length;
                    idleTarget = idlePositions[idlePositionsIndex];
                    atTargetTimer = durationAtTarget;
                }
            }
            return StIdle;
        }
        #endregion //FINISHED
        //wip draft
        #region Roll
        //concept phase
        private float maxRollSpeed;
        private float rollAcceleration;
        private int maxRolls;
        private bool rollFollowPlayer;
        private RollData[] rollData;
        private float rollDebrisY;
        private float rollDebrisX;
        private float rollDebrisWidth;
        public struct RollData
        {
            public Vector2 Direction;
            public int DebrisCount;
            public float DebrisDelay;
            public bool UsesDebris;
            public string DebrisSpawnerID;
        }
        public void Roll(RollData rollData, float maxSpeed, float acceleration, int rolls)
        {
            this.rollData = [rollData];
            maxRollSpeed = maxSpeed;
            rollAcceleration = acceleration;
            maxRolls = rolls;
            rollFollowPlayer = true;
            StateMachine.ForceState(StRoll);
        }
        public void Roll(RollData[] rollData, float maxSpeed, float acceleration, int rolls)
        {
            this.rollData = rollData;
            maxRollSpeed = maxSpeed;
            rollAcceleration = acceleration;
            maxRolls = rolls;
            rollFollowPlayer = false;
            StateMachine.ForceState(StRoll);
        }
        [CustomEntity("PuzzleIslandHelper/BossDebrisSpawner")]
        public class BossDebrisSpawner : Entity
        {
            public string ID;
            public BossDebrisSpawner(EntityData data, Vector2 offset) : base(data.Position + offset)
            {
                Collider = new Hitbox(data.Width, data.Height);
                ID = data.Attr("spawnerID");
            }
        }
        public void RollBegin()
        {
            UpdateHitbox = true;
            CallOrbs();
            StateCollideH = rollCollideH;
            StateCollideV = rollCollideV;
            StatePlayerCollide = rollCollidePlayer;
            rollCollide = false;
        }
        public void RollEnd()
        {
            UpdateHitbox = false;
            prevRollDirection = Vector2.Zero;
            rollRotateDirection = 0;
        }
        private int rollRotateDirection;
        public int RollUpdate()
        {
            if (rollRotateDirection != 0)
            {
                RotationSpeedMult = RubberbandApproach(RotationSpeedMult, rollRotateDirection * 4, 0, RubberbandFactor * 5);
            }
            return StRoll;
        }
        private Tween rollRotTween;
        private bool rollCollide;
        private Vector2 prevRollDirection;
        private IEnumerator RollRoutine()
        {
            yield return RubberbandTo(this, OrigPosition);
            RotationSpeedMult = 1;
            rollRotTween?.RemoveSelf();
            rollRotTween = Tween.Set(this, Tween.TweenMode.Oneshot, 2, Ease.SineIn, t =>
            {
                RotationSpeedMult += Engine.DeltaTime * 4;
            });
            yield return 1.5f;
            yield return WindUp(this, -Vector2.UnitY, 16, 0.4f);
            for (int i = 0; i < maxRolls; i++)
            {
                yield return new SwapImmediately(doRoll(rollData[i % rollData.Length], 400));
            }
            Continue();
        }
        private IEnumerator doRoll(RollData data, float speed)
        {
            if (prevRollDirection != Vector2.Zero)
            {
                float angleA = data.Direction.Angle();
                float angleB = prevRollDirection.Angle();
                rollRotateDirection = Math.Sign(angleA - angleB);
                yield return 1;
            }
            Speed = speed * data.Direction;
            rollCollide = false;
            while (!rollCollide) yield return null;
            SceneAs<Level>().Shake();
            if (data.UsesDebris)
            {
                Add(new Coroutine(spawnDebris(0.6f, data)));
            }
            prevRollDirection = data.Direction;
        }
        private IEnumerator spawnDebris(float initialDelay, RollData data)
        {
            Entity spawner = Scene.Tracker.GetEntities<BossDebrisSpawner>().Find(item => (item as BossDebrisSpawner).ID == data.DebrisSpawnerID);
            if (spawner != null)
            {
                yield return initialDelay;
                List<FallingDebris> debris = [];
                float space = (spawner.Width - 16) / data.DebrisCount;
                Vector2 start = spawner.TopCenter - Vector2.UnitX * (space * (data.DebrisCount / 2));
                for (int i = 0; i < data.DebrisCount; i++)
                {
                    debris.Add(new FallingDebris(start + Vector2.UnitX * space * i, 16, 16));
                }
                while (debris.Count > 0)
                {
                    FallingDebris removed = debris.Random();
                    debris.Remove(removed);
                    Scene.Add(removed);
                    yield return data.DebrisDelay;
                }
            }
        }
        private class FallingDebris : Actor
        {
            private char tiletype = '3';
            private TileGrid tilegrid;
            private float collidableY;
            private float speedY = 100f;
            public float Delay;
            private bool canFall;
            public FallingDebris(Vector2 position, float width, float height) : base(position)
            {
                Collider = new Hitbox(width, height);
                Add(new PlayerCollider((p) =>
                {
                    p.Die(Vector2.Zero);
                }));
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                tilegrid = GFX.FGAutotiler.GenerateBox(tiletype, (int)Width, (int)Height).TileGrid;
                Add(tilegrid);
                if (Delay == 0)
                {
                    canFall = true;
                }
                else
                {
                    Alarm.Set(this, Delay, () => canFall = true);
                }
            }
            public override void Update()
            {
                base.Update();
                if (canFall)
                {
                    speedY = Calc.Approach(speedY, 600f, 900f * Engine.DeltaTime);
                    if (Y < collidableY)
                    {
                        Y += speedY * Engine.DeltaTime;
                    }
                    else
                    {
                        MoveV(speedY * Engine.DeltaTime, (d) =>
                        {
                            RemoveSelf();
                        });
                    }
                }
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                Vector2 position = Position;
                while (CollideCheck<Solid>())
                {
                    Y++;
                }
                collidableY = Y;
                Position = position;
            }
        }
        private void rollCollideV(CollisionData data)
        {
            int dir = Math.Sign(Speed.Y);
            if (PushOutOfFloors(-dir))
            {
                rollCollide = true;
                Speed.Y = 0;
            }
        }
        private void rollCollideH(CollisionData data)
        {
            int dir = Math.Sign(Speed.X);
            if (PushOutOfWalls(-dir))
            {
                rollCollide = true;
                Speed.X = 0;
            }
        }
        private bool rollCollidePlayer(Player player)
        {
            return true;
        }
        #endregion
        //bug test
        #region Slam
        private Vector2 slamDirection;
        private float slamSpeed;
        private int slamBounces;
        private bool slamming;
        private bool slamDelayed;
        private float slamDelay;
        private float nextSpeedMult;
        public void Slam(Vector2 direction, float speed, int bounces)
        {
            slamDirection = direction;
            slamSpeed = speed;
            slamBounces = bounces;
            slamming = false;
            StateMachine.ForceState(StSlam);
        }
        public void Slam(float speed, int bounces)
        {
            if (Scene.GetPlayer() is Player player)
            {
                slamDirection = Vector2.Zero;
                slamDirection.X = Math.Sign(player.CenterX - X);
                slamDirection.Y = Math.Sign(player.CenterY - Y);
                slamSpeed = speed;
                slamBounces = bounces;
                slamming = false;
                StateMachine.ForceState(StSlam);
            }
        }
        public int SlamUpdate()
        {
            if (slamDelay > 0)
            {
                slamDelay -= Engine.DeltaTime;
            }
            if (slamBounces > 0 && slamming)
            {
                Speed = slamDirection * slamSpeed;
            }
            return StSlam;
        }
        public IEnumerator SlamRoutine()
        {
            Player player = Scene.GetPlayer();
            Vector2 p = Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.8f)
            {
                Position = p + -slamDirection * Ease.SineIn(i) * 8;
                RotationSpeedMult = 1 - i;
                yield return null;
            }
            Speed = slamDirection * slamSpeed;
            slamming = true;
            Deadly = true;
            RotationSpeedMult = 10;
            RadiusOffset = 8;
            while (slamming)
            {
                RotationSpeedMult = Calc.Approach(RotationSpeedMult, Math.Sign(RotationSpeedMult) * 6f, Engine.DeltaTime);
                yield return null;
            }
            Deadly = false;
            RadiusOffset = 0;
            Vector2 from = Position;
            //return to original position
            float rsmFrom = RotationSpeedMult;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 1.3f)
            {
                RotationSpeedMult = Calc.LerpClamp(rsmFrom, 1, Ease.SineInOut(i));
                Position = Vector2.Lerp(from, p, Ease.SineInOut(i));
                yield return null;
            }
            Continue();
        }
        public void SlamBegin()
        {
            Engine.Commands.Log("Singularity - SlamBegin", Color.Gray);
            slamming = false;
            StatePlayerCollide = slamPlayerCollide;
            StateCollideH = slamCollideH;
            StateCollideV = slamCollideV;
        }
        public void SlamEnd()
        {
            slamming = false;
            Speed = Vector2.Zero;
            RotationSpeedMult = 1;
            RadiusOffset = 0;
            StatePlayerCollide = null;
            StateCollideH = null;
            StateCollideV = null;
            JustifyOrbs(0.5f, 0.5f);
        }
        private bool slamPlayerCollide(Player player)
        {
            return !player.DashAttacking;
        }
        private void slamCollideH(CollisionData data)
        {
            if (Speed.X != 0)
            {
                slamDirection.X *= -1;
                slamBounces--;
                if (slamBounces > 1)
                {
                    JustifyOrbs(Math.Sign(Speed.X), 0.5f);
                    YoyoSquishTo(-0.5f, 0.1f, true);
                    RotationSpeedMult = -Math.Sign(RotationSpeedMult) * 10;
                }
                else
                {
                    slamBounces = 0;
                    slamming = false;
                    Speed = Vector2.Zero;
                }
            }
        }
        private void slamCollideV(CollisionData data)
        {
            if (Speed.Y != 0)
            {
                slamDirection.Y *= -1;
                slamBounces--;
                if (slamBounces > 1)
                {
                    JustifyOrbs(0.5f, Math.Sign(Speed.Y));
                    YoyoSquishTo(0.5f, 0.1f, true);
                    RotationSpeedMult = -Math.Sign(RotationSpeedMult) * 10;
                }
                else
                {
                    slamBounces = 0;
                    slamming = false;
                    Speed = Vector2.Zero;
                }
            }
        }
        #endregion
        public Vector2 OrbOrigin;
        private Tween angleEaseTween;
        public override void Update()
        {
            MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
            AfterMoveH();
            MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
            AfterMoveV();
            Rotation = (Rotation + RotationSpeed * RotationSpeedMult * Engine.DeltaTime) % MathHelper.TwoPi;
            UpdateOrbs();
            UpdateColliders(Radius, UpdateCircle, UpdateHitbox);
            foreach (Orb orb in Orbs)
            {
                orb.Circle.Radius = Radius;
                Vector2 o = orb.Sprite.Origin;
                OrbOrigin = o;
            }
            base.Update();
            PrevPosition = Position;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!Flag)
            {
                RemoveSelf();
            }
            else
            {
                Boss.ActionRegistry.Load();
                R = new Orb(this, Vector2.Zero, Radius, Radius / 2, OrbColors.RFill, OrbColors.REdge, OrbColors.RCenter);
                G = new Orb(this, Vector2.Zero, Radius, Radius / 2, OrbColors.GFill, OrbColors.GEdge, OrbColors.GCenter);
                B = new Orb(this, Vector2.Zero, Radius, Radius / 2, OrbColors.BFill, OrbColors.BEdge, OrbColors.BCenter);
                scene.Add(R, G, B);
                Orbs = [G, B];
                R.Visible = R.Collidable = Tri;
                if (Tri)
                {
                    Orbs.Add(R);
                }
                UpdateOrbs();
                UpdateColliders(Radius);
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(R, G, B);
        }
        public void UpdateColliders(float radius, bool circle = true, bool hitbox = true)
        {
            if (circle)
            {
                Circle.Radius = radius + RadiusOffset;
            }
            if (hitbox)
            {
                Collision.Width = radius * 2 + SizeOffset;
                Collision.Height = radius * 2 + SizeOffset;
                Collision.Position = -Collision.HalfSize;
            }
        }
        public void OnCollideH(CollisionData data)
        {
            StateCollideH?.Invoke(data);
        }
        public void OnCollideV(CollisionData data)
        {
            StateCollideV?.Invoke(data);
        }
        public void AfterMoveH()
        {

        }
        public void AfterMoveV()
        {

        }
        public void OnPlayer(Player player)
        {
            bool kill = StatePlayerCollide != null && StatePlayerCollide.Invoke(player) || Deadly;
            if (kill)
            {
                player.Die(Vector2.Zero);
            }
            else if (player.DashAttacking)
            {
                Health--;
                player.ReflectBounce(Vector2.Normalize(player.Center - Position));
                OnTakeDamage?.Invoke();
            }
        }

        public void UpdateOrbs()
        {
            float inc = MathHelper.TwoPi / Orbs.Count;
            int count = 0;
            foreach (var orb in Orbs)
            {
                orb.RotationOffset = count * inc;
                orb.Rotation = Rotation;
                if (orb.StateMachine.State == Orb.StAuto)
                {
                    orb.Position = orb.AutoPosition;
                }
                count++;
            }
        }
        public void CallOrbs()
        {
            foreach (Orb orb in Orbs)
            {
                orb.Auto();
            }
        }
        public void EaseToAngle(int dir, float angle, float duration = 1)
        {
            Rotation %= MathHelper.TwoPi;
            float next2 = dir * angle;
            RotationSpeedMult = 0;
            RotationSpeed = 0;
            float from2 = Rotation;
            angleEaseTween?.RemoveSelf();
            angleEaseTween = Tween.Set(this, Tween.TweenMode.Oneshot, duration, Ease.SineOut, t =>
            {
                Rotation = Calc.LerpClamp(from2, next2, t.Eased);
            });
        }
        public void SquishTo(float amount, float time, bool wiggleOnEnd)
        {
            foreach (Orb orb in Orbs)
            {
                orb.SquishTo(amount, time, wiggleOnEnd);
            }
        }
        public void Unsquish(float time, bool wiggleOnEnd)
        {
            foreach (Orb orb in Orbs)
            {
                orb.Unsquish(time, wiggleOnEnd);
            }
        }
        public void YoyoSquishTo(float amount, float time, bool wiggleOnEnd)
        {
            foreach (Orb orb in Orbs)
            {
                orb.YoyoSquishTo(amount, time, wiggleOnEnd);
            }
        }
        public void JustifyOrbs(float x, float y)
        {
            foreach (Orb orb in Orbs)
            {
                OriginOrb(orb, orb.Sprite.Radius * 2 * x, orb.Sprite.Radius * 2 * y);
            }
        }
        public void OriginOrb(Orb orb, float x, float y)
        {
            Vector2 prev = orb.Sprite.Origin;
            orb.Sprite.Origin = new Vector2(x, y);
            orb.Sprite.Position += orb.Sprite.Origin - prev;
        }
        public bool PushOutOfFloors(int dir)
        {
            if (dir != 0)
            {
                bool collided = false;
                while (CollideCheck<Solid>())
                {
                    collided = true;
                    Y += dir;
                }
                return collided;
            }
            return false;
        }
        public bool PushOutOfWalls(int dir)
        {
            if (dir != 0)
            {
                bool collided = false;
                while (CollideCheck<Solid>())
                {
                    collided = true;
                    X += dir;
                }
                return collided;
            }
            return false;
        }
        [Tracked]
        private class waveSpike : Spikes
        {
            public bool StartedEnter;
            public bool StartedExit;
            public bool EndedEnter;
            public bool EndedExit;
            public bool TriedToSpawn;
            public bool FailedToSpawn;
            public int SpreadDirection;
            public Vector2 From, To;
            private string type;
            private float duration;
            public bool Auto;
            private int maxLength;
            private Coroutine coroutine;
            private int length;
            public waveSpike(Vector2 position, int spreadDirection, int size, int maxSize, Directions direction, string type, float duration, bool auto = true, bool speedUp = false, int length = 8) : base(position, size, direction, type)
            {
                /*                KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.Up, () =>
                                {
                                    SpawnNext();
                                    state++;
                                });
                                KeyComponent.ForKey(this, Microsoft.Xna.Framework.Input.Keys.Enter, () =>
                                {
                                    state = 0;
                                });*/
                this.length = length;
                maxLength = maxSize;
                Auto = auto;
                SpeedUp = speedUp;
                this.duration = duration;
                this.type = type;
                SpreadDirection = spreadDirection;
                Add(coroutine = new Coroutine(false));
            }
            public Vector2 Dir()
            {
                return Direction switch
                {
                    Directions.Up => -Vector2.UnitY,
                    Directions.Down => Vector2.UnitY,
                    Directions.Left => -Vector2.UnitX,
                    Directions.Right => Vector2.UnitX
                };
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                To = Position;
                Vector2 scale = Collider.Size;
                From = Position + -Dir() * scale;
            }
            private IEnumerator routine()
            {
                StartedEnter = true;
                for (float i = 0; i < 1; i += Engine.DeltaTime / duration)
                {
                    Position = Vector2.Lerp(From, To, Ease.SineOut(i)).Round();
                    yield return null;
                }
                Position = To;
                EndedEnter = true;
                SpawnNext();
                if (Auto)
                {
                    yield return new SwapImmediately(exitRoutine(duration));
                }
            }
            public void Exit(float time = -1)
            {
                coroutine.Replace(exitRoutine(time < 0 ? duration : time));
            }
            private IEnumerator exitRoutine(float time)
            {
                StartedExit = true;
                for (float i = 0; i < 1; i += Engine.DeltaTime / time)
                {
                    Position = Vector2.Lerp(To, From, Ease.SineInOut(i));
                    yield return null;
                }
                EndedExit = true;
                RemoveSelf();
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                //Position = To;
                Position = From;
                coroutine.Replace(routine());
            }
            public Vector2? debugPosition;
            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                Draw.HollowRect(debugRect, Color.Magenta);
            }
            private Rectangle debugRect;
            public void SpawnNext()
            {
                if (Spawned) return;
                Rectangle bounds = new Rectangle();
                switch (Direction)
                {
                    case Directions.Up:
                    case Directions.Down:
                        bounds.X = (int)(SpreadDirection < 0 ? Left : Right);
                        bounds.Y = (int)Top;
                        bounds.Width = 0;
                        bounds.Height = (int)Height;
                        break;
                    case Directions.Left:
                    case Directions.Right:
                        bounds.X = (int)Left;
                        bounds.Y = (int)(SpreadDirection < 0 ? Top : Bottom);
                        bounds.Width = (int)Width;
                        bounds.Height = 0;
                        break;

                }
                debugRect = bounds;
                int loops = 0;
                Rectangle next = Direction is Directions.Up or Directions.Down ? bounds.ExtendHorizontally(8, SpreadDirection) : bounds.ExtendVertically(8, SpreadDirection);

                while (!Scene.CollideCheck<Solid>(next) && Math.Max(bounds.Width, bounds.Height) < size)
                {
                    debugRect = bounds;
                    bounds = next;
                    switch (Direction)
                    {
                        case Directions.Up: case Directions.Down: next = next.ExtendHorizontally(8, SpreadDirection); break;
                        case Directions.Left: case Directions.Right: next = next.ExtendVertically(8, SpreadDirection); break;
                    }
                    loops++;
                }
                int nextSize = Math.Max(bounds.Width, bounds.Height);
                if (Direction == Directions.Left)
                {
                    bounds.X += bounds.Width;
                }
                if (Direction == Directions.Up)
                {
                    bounds.Y += bounds.Height;
                }
                float duration = SpeedUp ? Math.Max(this.duration * 0.8f, 0.1f) : this.duration;
                if (nextSize >= 8)
                {
                    Scene.Add(new waveSpike(bounds.Location.ToVector2(), SpreadDirection, nextSize, maxLength, Direction, type, duration, Auto, SpeedUp));
                    Spawned = true;
                }
                else if (Direction == Directions.Down)
                {
                    Scene.Add(new waveSpike(bounds.Location.ToVector2(), 1, maxLength, maxLength, SpreadDirection > 0 ? Directions.Left : Directions.Right, type, duration, false, SpeedUp));
                    Spawned = true;
                }
            }
            public bool SpeedUp;
            public bool Spawned;
            private class debugComponent : GraphicsComponent
            {
                public debugComponent(Vector2 position) : base(true)
                {
                    Position = position;
                }
                public override void Render()
                {
                    base.Render();
                    Vector2 start = RenderPosition;
                    Draw.HollowRect(start.X, start.Y, 8, 8, Color.Red);
                    Draw.Rect(start.X + 1, start.Y + 1, 6, 6, Color.DarkRed);

                }
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
            }
        }
        [Tracked]
        public class Orb : Actor
        {
            public event Action<Orb> OnAttackEnd = (o) => { };
            public bool QueuedForAttack;
            public StateMachine StateMachine;
            public SingularityBoss Parent;
            public Circle Circle;
            public VertexOrb Sprite;
            public Hitbox Hitbox;
            public float Radius
            {
                get => radius;
                set
                {
                    radius = value;
                    Sprite.Radius = value;
                    UpdateColliders(value, UpdateCircle, UpdateHitbox);
                }
            }
            private float radius;
            public float RadiusOffset, SizeOffset;
            public float Distance;
            public float Rotation;
            public float RotationOffset;
            public Color Color
            {
                get => Sprite.Color;
                set => Sprite.Color = value;
            }
            private Color origColor;
            public Vector2 PrevPosition;
            private ColorShifter shifter;
            public delegate Color GetColor(Orb orb);
            public GetColor ColorFunction;
            public Vector2 Speed;
            public bool Deadly;
            public bool CollideSolids;
            public Orb Rival;
            public bool UpdateHitbox = true;
            public bool UpdateCircle = true;
            public bool Attacking;
            public int State => StateMachine.State;
            public bool Available => !Attacking || StateMachine.State == StAuto;
            public Collision StateCollideH, StateCollideV;
            private Vector2? preAttackPosition = null;
            public BetterShaker Shaker;
            public Vector2 Shake;
            public Orb(SingularityBoss parent, Vector2 offset, float radius, float distance, Color fill, Color edge, Color center) : base(parent.Position + offset)
            {
                Parent = parent;
                Distance = distance;
                Sprite = new VertexOrb(offset, radius, Tower.Portal.MaxOrbCorners, fill, edge, 2);
                Sprite.CenterColor = center;
                Add(Sprite);
                origColor = Color = fill;
                ColorFunction = (o) => origColor;
                Circle = new Circle(radius);
                Collider = Hitbox = new Hitbox(radius * 2, radius * 2, -radius, -radius);
                Radius = radius;
                Add(new PlayerCollider(OnPlayer, Circle));
                Add(StateMachine = new StateMachine(20));
                StateMachine.SetCallbacks(StAuto, AutoUpdate);
                StateMachine.SetCallbacks(StBounce, BounceUpdate, BounceRoutine, BeginBounce, EndBounce);
                StateMachine.SetCallbacks(StTargetPlayer, TargetPlayerUpdate, TargetPlayerRoutine, TargetPlayerBegin, TargetPlayerEnd);
                StateMachine.SetCallbacks(StShockwave, ShockwaveUpdate, ShockwaveRoutine);
                StateMachine.SetCallbacks(StSingleSpikeSlam, SingleSpikeSlamUpdate, SingleSpikeSlamRoutine, SingleSpikeSlamBegin, SingleSpikeSlamEnd);
                StateMachine.SetCallbacks(StSlide, SlideUpdate, SlideRoutine, SlideBegin, SlideEnd);
                StateMachine.SetCallbacks(StFall, FallUpdate, null, FallBegin, FallEnd);
                Add(Shaker = new BetterShaker(OnShake));
            }
            public void OnShake(Vector2 amount)
            {
                Shake += amount;
                Sprite.RenderOffset += amount;
            }
            public void StartShaking(float time = -1)
            {
                Shaker.ShakeFor(time);
            }
            public void StartShaking(GetColor function, float time = -1)
            {
                Shaker.ShakeFor(time);
                ColorFunction = function;
            }
            public void StopShaking()
            {
                Shaker.StopShaking();
                ColorFunction = (o) => origColor;
            }
            public const int StAuto = 0;
            public const int StBounce = 1;
            public const int StFight = 2;
            public const int StTargetPlayer = 3;
            public const int StShockwave = 4;
            public const int StSingleSpikeSlam = 5;
            public const int StSlide = 6;
            public const int StFall = 7;
            public const int StDart = 8;
            public const int StBarrier = 9;
            public const int StRevolve = 10;

            #region Auto
            private bool autoRubberband;
            private double autoRubberbandFactor;
            public void Auto(double rubberbandFactor = 0.0099999997764825821)
            {
                autoRubberbandFactor = rubberbandFactor;
                StateMachine.ForceState(StAuto);
            }
            public void BeginAuto()
            {
                Engine.Commands.Log("Orb- AutoBegin", Color.Lime);
                autoRubberband = Vector2.DistanceSquared(Position, AutoPosition) > 4;
                Speed = Vector2.Zero;
                CollideSolids = false;
                Deadly = false;
                Attacking = false;
            }
            public int AutoUpdate()
            {
                if (autoRubberband)
                {
                    autoRubberband = RubberbandApproach(this, AutoPosition, 2, autoRubberbandFactor);
                }
                else
                {
                    Position = AutoPosition;
                }
                return StAuto;
            }
            public Vector2 AutoPosition => Parent.Position + Calc.AngleToVector(Rotation + RotationOffset, Distance);
            #endregion
            #region Bounce
            public int bouncesRemaining;
            private int maxBounces;
            private bool canLaunchBounce;
            private float bounceWindUpTimeMult = 1;
            public float MaxBounceSpeed = 120f;
            private Vector2 bounceDirection;
            private bool bounceReturnBySelf;
            private bool bounceFinished;
            private float bounceDelay;
            private Vector2 bounceSpeedCache;
            public event Action<Orb> OnBounceEnd = (o) => { };
            public void Bounce(int bounces, float speed, float windUpMult, bool returnBySelf = false, Vector2? startPosition = null)
            {
                preAttackPosition = startPosition;
                bounceReturnBySelf = returnBySelf;
                bouncesRemaining = maxBounces = bounces;
                MaxBounceSpeed = speed;
                bounceWindUpTimeMult = windUpMult;
                StateMachine.ForceState(StBounce);
            }
            private void BeginBounce()
            {
                Engine.Commands.Log("Orb- BounceBegin", Color.Lime);
                Attacking = true;
                StateCollideH = bounceCollideH;
                StateCollideV = bounceCollideV;
                Deadly = false;
                canLaunchBounce = false;
                bounceFinished = false;
            }
            private int BounceUpdate()
            {
                if (canLaunchBounce)
                {
                    if (bouncesRemaining > 0)
                    {
                        Speed = bounceDirection * MaxBounceSpeed;
                    }
                    else
                    {
                        if (bounceReturnBySelf)
                        {
                            bounceFinished = true;
                        }
                        else
                        {
                            Speed = Calc.Approach(Speed, Vector2.Zero, 10f * Engine.DeltaTime);
                        }
                    }
                }
                return StBounce;
            }
            private IEnumerator BounceRoutine()
            {
                if (preAttackPosition.HasValue)
                {
                    yield return new SwapImmediately(RubberbandTo(this, preAttackPosition.Value));
                }
                Vector2 windUpDirection = Vector2.Normalize(Position - Parent.Position);
                yield return new SwapImmediately(WindUp(this, -windUpDirection, 6, 0.5f));
                Deadly = true;
                canLaunchBounce = true;
                bounceDirection = windUpDirection;
                bounceFinished = false;
                while (!bounceFinished)
                {
                    if (bounceDelay > 0)
                    {
                        yield return bounceDelay;
                        bounceDelay = 0;
                        Speed = bounceSpeedCache;
                        canLaunchBounce = true;
                    }
                    yield return null;
                }
                Deadly = false;
                if (bounceReturnBySelf)
                {
                    Auto();
                }
            }
            private void EndBounce()
            {
                OnAttackEnd?.Invoke(this);
                OnBounceEnd?.Invoke(this);
                OnBounceEnd = null;
                gravity = 0;
                frictionX = 0;
                Speed = Vector2.Zero;
                Deadly = false;
                bouncesRemaining = 0;
                canLaunchBounce = false;
                bounceFinished = false;
                bounceReturnBySelf = false;
                StateCollideH = StateCollideV = null;
            }
            private void bounceCollideH(CollisionData data)
            {
                if (Speed.X != 0)
                {
                    if (bouncesRemaining > 0)
                    {
                        bounceDirection.X *= -1;
                        bouncesRemaining--;
                        if (bouncesRemaining == 0) Speed = Vector2.Zero;
                        if (frictionX == 0)
                        {
                            canLaunchBounce = false;
                            Speed = Vector2.Zero;
                            int dir = Math.Sign(Speed.X);
                            Sprite.JustifyOrigin(0.5f + dir * .5f, 0.5f);
                            Sprite.X = -dir * Radius;
                            SquishTo(0.5f, 0.3f, false, () =>
                            {
                                Unsquish(0.3f, true, () =>
                                {
                                    Sprite.JustifyOrigin(0.5f, 0.5f);
                                    Sprite.X = 0;
                                    canLaunchBounce = true;
                                });
                            });
                        }
                    }
                    if (frictionX != 0)
                    {
                        Speed.X *= -0.8f;
                        YoyoSquishTo(-0.1f, 0.1f, true);
                    }
                }
            }
            private void bounceCollideV(CollisionData data)
            {
                if (Speed.Y != 0)
                {
                    if (bouncesRemaining > 0)
                    {
                        bounceDirection.Y *= -1;
                        bouncesRemaining--;
                        if (bouncesRemaining == 0) Speed = Vector2.Zero;
                        if (gravity == 0)
                        {
                            canLaunchBounce = false;
                            Speed = Vector2.Zero;
                            int dir = Math.Sign(Speed.Y);
                            Sprite.JustifyOrigin(0.5f, 0.5f + dir * .5f);
                            Sprite.Y = -dir * Sprite.Radius;
                            SquishTo(0.5f, 0.6f, false, () =>
                            {
                                Unsquish(0.3f, true, () =>
                                {
                                    Sprite.JustifyOrigin(0.5f, 0.5f);
                                    Sprite.Y = 0;
                                    canLaunchBounce = true;
                                });
                            });
                        }
                    }
                    if (gravity != 0)
                    {
                        Speed.Y *= -0.8f;
                    }
                }
            }
            #endregion
            #region Fight
            public event Action<Orb> OnFightEnd = (o) => { };
            #endregion
            #region TargetPlayer
            private float windUpTime;
            private float chargeSpeed;
            private Player targetPlayer;
            private int targetPlayerTimes;
            private bool targetPlayerContinue;
            private float targetPlayerAirFriction;
            public event Action<Orb> OnTargetPlayerEnd = (s) => { };
            public void TargetPlayer(Player player, float windUpTime, float speed, int times = 1, float airFriction = 0.5f)
            {
                targetPlayerAirFriction = airFriction;
                targetPlayerTimes = times;
                this.windUpTime = windUpTime;
                chargeSpeed = speed;
                targetPlayer = player;
                StateMachine.ForceState(StTargetPlayer);
            }
            public IEnumerator TargetPlayerRoutine()
            {
                for (int i = 0; i < targetPlayerTimes; i++)
                {
                    Deadly = false;
                    frictionX = 0;
                    frictionY = 0;
                    yield return new SwapImmediately(RubberbandTo(this, preAttackPosition.Value));
                    Vector2 dir = Vector2.Normalize(targetPlayer.Center - Position);
                    yield return new SwapImmediately(WindUp(this, -dir, 4, windUpTime));
                    targetPlayerContinue = false;
                    Speed = dir * chargeSpeed;
                    Deadly = true;
                    while (!targetPlayerContinue)
                    {
                        yield return null;
                    }
                    if (i + 1 < targetPlayerTimes)
                    {
                        recalculateTargetPlayerPosition();
                        yield return 0.5f;
                    }
                }
                Auto();
            }
            public void TargetPlayerBegin()
            {
                Engine.Commands.Log("Orb- TargetBegin", Color.Lime);
                Attacking = true;
                Speed = Vector2.Zero;
                frictionY = 0;
                frictionX = 0;
                StateCollideH = StateCollideV = targetPlayerCollide;
                recalculateTargetPlayerPosition();
            }
            private void targetPlayerCollide(CollisionData data)
            {
                targetPlayerContinue = true;
                Speed = Vector2.Zero;
            }
            private void recalculateTargetPlayerPosition()
            {
                preAttackPosition ??= Position;
                int i;
                for (i = 0; i < 20; i++)
                {
                    preAttackPosition = targetPlayer.Center + Calc.AngleToVector(Calc.Random.NextAngle(), 50);
                    if (!CollideCheck<Solid>(preAttackPosition.Value))
                    {
                        break;
                    }
                    preAttackPosition = null;
                }
            }
            public void TargetPlayerEnd()
            {
                OnAttackEnd?.Invoke(this);
                OnTargetPlayerEnd?.Invoke(this);
                OnTargetPlayerEnd = null;
                Speed = Vector2.Zero;
                preAttackPosition = null;
                targetPlayer = null;
                StateCollideH = StateCollideV = null;
            }
            public int TargetPlayerUpdate()
            {
                if (frictionX == frictionY && frictionX == targetPlayerAirFriction)
                {
                    if (Speed.LengthSquared() < 0.5f)
                    {
                        targetPlayerContinue = true;
                    }
                }
                return StTargetPlayer;
            }
            #endregion
            #region Shockwave
            private Vector2 shockwavePosition;
            private float shockwaveDelay;
            private float shockwaveWaveSpeed;
            private float shockwaveSegmentLength;
            private int shockwaveNextState;
            private shockwave.Data[] shockwaveData;
            public event Action<Orb> OnShockwaveEnd = (s) => { };
            public void Shockwave(Vector2 position, float delay, float waveSpeed, float segmentLength, int nextState, params shockwave.Data[] data)
            {
                shockwavePosition = position;
                shockwaveDelay = delay;
                shockwaveWaveSpeed = waveSpeed;
                shockwaveSegmentLength = segmentLength;
                shockwaveData = data;
                shockwaveNextState = nextState;
                StateMachine.ForceState(StShockwave);
            }
            public int ShockwaveUpdate()
            {
                return StShockwave;
            }
            public IEnumerator ShockwaveRoutine()
            {
                for (int i = 0; i < shockwaveData.Length; i++)
                {
                    if (shockwaveData[i].Position.HasValue)
                    {
                        yield return new SwapImmediately(RubberbandTo(this, shockwaveData[i].Position.Value));
                    }
                    shockwave wave = new shockwave(Position, shockwaveData[i]);
                    yield return shockwaveData[i].Delay;
                }
                Auto();
            }
            public void GenerateShockwave(Vector2 center, shockwave.Data data)
            {
                shockwave wave = new shockwave(center, data);
            }
            public class shockwave
            {
                public struct Data
                {
                    public Vector2? Position;
                    public bool[] SegmentStates;
                    public float Speed;
                    public float Delay;
                }
                public Vector2 Center;
                private Data data;
                public shockwave(Vector2 center, Data data)
                {
                    Center = center;
                    this.data = data;
                }
            }
            #endregion
            #region SpikeSlam
            private Vector2 spikeSlamTarget;
            private int spikeSlamLoops;
            private bool spikeSlamImpact;
            private int spikeLength;
            private float spikeSlamMult = 1;
            private string spikeSlamMarkerA, spikeSlamMarkerB;
            public event Action<Orb> OnSpikeSlamEnd = (s) => { };
            public void SingleSpikeSlam(int loops, int length, float mult = 1)
            {
                spikeSlamMarkerA = spikeSlamMarkerB = null;
                spikeSlamMult = mult;
                spikeLength = length;
                spikeSlamLoops = loops;
                spikeSlamTarget = Position;
                StateMachine.ForceState(StSpikeSlam);
            }
            public void SingleSpikeSlam(Vector2? position, string markerA, string markerB, int loops, int length, float mult = 1)
            {
                spikeSlamMarkerA = markerA;
                spikeSlamMarkerB = markerB;
                if (position.HasValue)
                {
                    spikeSlamTarget = position.Value;
                }
                else
                {
                    randomizeSpikeSlamPosition();
                }
                spikeSlamLoops = loops;
                spikeLength = length;
                spikeSlamMult = mult;
                StateMachine.ForceState(StSpikeSlam);
            }
            public int SingleSpikeSlamUpdate()
            {
                return StSpikeSlam;
            }
            public IEnumerator SingleSpikeSlamRoutine()
            {
                for (int i = 0; i < spikeSlamLoops; i++)
                {
                    yield return new SwapImmediately(RubberbandTo(this, spikeSlamTarget));
                    yield return new SwapImmediately(WindUp(this, -Vector2.UnitY, 4, 0.5f));
                    Speed.Y = 0;
                    spikeSlamImpact = false;
                    while (!spikeSlamImpact)
                    {
                        Speed.Y = Calc.Approach(Speed.Y, 900f, 300f * Engine.DeltaTime);
                        yield return null;
                    }
                    if (i < spikeSlamLoops - 1)
                    {
                        Speed.Y = -10f;
                        yield return 0.8f;
                        randomizeSpikeSlamPosition();
                    }
                }
                Auto();
            }
            private void randomizeSpikeSlamPosition()
            {
                if (Marker.TryFind(spikeSlamMarkerA, out Vector2 a) && Marker.TryFind(spikeSlamMarkerB, out Vector2 b))
                {
                    spikeSlamTarget = Vector2.Lerp(a, b, Calc.Random.Range(0, 1));
                    spikeSlamTarget.X -= spikeSlamTarget.X % 8;
                    spikeSlamTarget.X += 4;
                }
                else
                {
                    spikeSlamTarget = Position;
                }
            }
            public void SingleSpikeSlamBegin()
            {
                Engine.Commands.Log("Orb- SpikeBegin", Color.Lime);
                Collider.Height = Radius;
                UpdateHitbox = false;
                StateCollideH = null;
                StateCollideV = singleSpikeSlamCollideV;
            }
            public void SingleSpikeSlamEnd()
            {
                OnAttackEnd?.Invoke(this);
                OnSpikeSlamEnd?.Invoke(this);
                OnSpikeSlamEnd = null;
                UpdateHitbox = true;
                UpdateColliders(Radius);
                StateCollideH = StateCollideV = null;
                spikeSlamMarkerA = spikeSlamMarkerB = null;
            }
            private void singleSpikeSlamCollideV(CollisionData data)
            {
                if (Speed.Y > 0 && !spikeSlamImpact)
                {
                    while (CollideCheck<Solid>())
                    {
                        Y--;
                    }
                    Speed.Y = -100f;
                    frictionY = 20f;
                    spikeSlamImpact = true;
                    int xOffset = 8 * spikeLength;
                    Scene.Add(new waveSpike(BottomCenter + new Vector2(4 + xOffset, 0), 1, spikeLength, spikeLength, Spikes.Directions.Up, "default", 0.6f - 0.2f * (1 - spikeSlamMult), true));
                    Scene.Add(new waveSpike(BottomCenter + new Vector2(-4 - xOffset, 0), 1, spikeLength, spikeLength, Spikes.Directions.Up, "default", 0.6f - 0.2f * (1 - spikeSlamMult), true));
                }
            }
            #endregion
            #region Slide
            private Vector2 slideFrom, slideDirection;
            private float slideMaxSpeed;
            private Action onSlideEnd;
            private bool slideCollideSignal;
            private Ease.Easer slideEase;
            private float slideEaseTime, slideEaseTimer;
            private bool startSlideEase;
            public event Action<Orb> OnSlideEnd = (s) => { };
            public void Slide(Vector2 from, Vector2 direction, float maxSpeed, float easeTime = -1, Ease.Easer ease = null, Action onSlideEnd = null)
            {
                slideEaseTimer = easeTime;
                slideEaseTime = easeTime;
                slideEase = ease ?? Ease.Linear;
                slideFrom = from;
                slideDirection = direction;
                slideMaxSpeed = maxSpeed;
                this.onSlideEnd = onSlideEnd;
                StateMachine.ForceState(StSlide);
            }
            public int SlideUpdate()
            {
                if (startSlideEase)
                {
                    Vector2 speed = slideMaxSpeed * slideDirection;
                    if (slideEaseTime > 0 && slideEaseTimer > 0)
                    {
                        speed *= slideEase(1 - slideEaseTimer / slideEaseTime);
                        slideEaseTimer = Math.Max(0, slideEaseTimer - Engine.DeltaTime);
                    }
                    Speed = speed;
                }
                return StSlide;
            }
            public void SlideBegin()
            {
                Engine.Commands.Log("Orb- SlideBegin", Color.Lime);
                Attacking = true;
                startSlideEase = false;
                UpdateHitbox = true;
                Speed = Vector2.Zero;
                frictionX = frictionY = 0;
                StateCollideH = slideCollideH;
                StateCollideV = slideCollideV;
            }
            public void SlideEnd()
            {
                OnAttackEnd?.Invoke(this);
                OnSlideEnd?.Invoke(this);
                OnSlideEnd = null;
                onSlideEnd = null;
                StateCollideH = StateCollideV = null;
                UpdateHitbox = false;
            }
            public IEnumerator SlideRoutine()
            {
                yield return new SwapImmediately(RubberbandTo(this, slideFrom));
                Speed = Vector2.UnitY * 70f;
                slideCollideSignal = false;
                while (!slideCollideSignal) yield return null;
                slideCollideSignal = false;
                Speed = Vector2.Zero;
                SceneAs<Level>().Shake();
                yield return 0.6f;
                startSlideEase = true;
                slideCollideSignal = false;
                while (!slideCollideSignal) yield return null;
                yield return 0.7f;
                onSlideEnd?.Invoke();
                Auto();
            }
            private void slideCollideH(CollisionData data)
            {
                slideCollideSignal = true;
                if (Speed.X != 0)
                {
                    while (CollideCheck<Solid>())
                    {
                        X += -Math.Sign(Speed.X);
                    }
                    Speed.X = 0;
                }
            }
            private void slideCollideV(CollisionData data)
            {
                slideCollideSignal = true;
                if (Speed.Y != 0)
                {
                    while (CollideCheck<Solid>())
                    {
                        Y += -Math.Sign(Speed.Y);
                    }
                    Speed.Y = 0;
                }
            }
            #endregion
            #region Fall
            private float fallGroundTime;
            public void Fall(Vector2 speed, float groundTime)
            {
                Speed = speed;
                gravity = Player.Gravity;
                frictionX = 200f;
                fallGroundTime = groundTime;
            }
            public int FallUpdate()
            {
                bool onGround = CollideCheck<Solid>(Position + Vector2.UnitY);
                if (onGround)
                {
                    if (fallGroundTime > 0)
                    {
                        fallGroundTime -= Engine.DeltaTime;
                        if (fallGroundTime <= 0)
                        {
                            fallGroundTime = 0;
                            Auto();

                        }
                    }
                    else if (Math.Abs(Speed.X) < 2f)
                    {
                        Auto();
                    }
                }

                return StFall;
            }
            public void FallBegin()
            {
                StateCollideH = fallCollideH;
                StateCollideV = fallCollideV;
            }
            public void FallEnd()
            {
                StateCollideH = StateCollideV = null;
            }
            private void fallCollideV(CollisionData data)
            {
                Speed.Y *= -0.8f;
            }
            private void fallCollideH(CollisionData data)
            {
                Speed.X *= -0.8f;
            }
            #endregion
            #region Collision
            public void OnCollideH(CollisionData data)
            {
                StateCollideH?.Invoke(data);
            }
            public void OnCollideV(CollisionData data)
            {
                StateCollideV?.Invoke(data);
            }
            private float playerImmuneTimer;
            private float gravity;
            private float frictionX;
            private float frictionY;
            private float invincibleTimer;
            public void OnPlayer(Player player)
            {
                if (!player.Dead)
                {
                    if (Deadly)
                    {
                        if (StateMachine.State == StBounce)
                        {
                            if (player.DashAttacking && invincibleTimer <= 0 && bouncesRemaining > 0)
                            {
                                if (player.DashDir.X != 0)
                                {
                                    bounceDirection.X = player.DashDir.X;
                                }
                                if (player.DashDir.Y != 0)
                                {
                                    bounceDirection.Y = player.DashDir.Y;
                                }
                                player.ReflectBounce(-player.DashDir);
                                bouncesRemaining--;
                                playerImmuneTimer = 0.5f;
                                invincibleTimer = 0.6f;
                                if (bouncesRemaining == 0)
                                {
                                    Fall(bounceDirection * 75f, 1);
                                }
                                return;
                            }
                            if (bouncesRemaining > 0 && playerImmuneTimer <= 0)
                            {
                                player.Die(Vector2.Zero);
                                return;
                            }
                        }
                        if (playerImmuneTimer <= 0)
                        {
                            player.Die(Vector2.Zero);
                        }
                    }
                }

            }
            #endregion
            #region Overrides
            public override void Added(Scene scene)
            {
                base.Added(scene);
                Add(squishRoutine = new Coroutine(false));
                Add(ScaleWiggler = Wiggler.Create(0.5f, 4f, (f) =>
                {
                    WiggleVector = Vector2.One * (f * 0.25f);
                }));
                Add(WigglerX = Wiggler.Create(0.5f, 4f, (f) =>
                {
                    WiggleVector.X = f * 0.25f;
                }));
                Add(WigglerY = Wiggler.Create(0.5f, 4f, (f) =>
                {
                    WiggleVector.Y = f * 0.25f;
                }));
            }
            public float Squish, Stretch;
            public Vector2 Scale = Vector2.One;
            public Vector2 WiggleVector;
            private Coroutine squishRoutine;
            public IEnumerator SquishToRoutine(float amount, float time, bool wiggleOnEnd, Action onEnd = null)
            {
                float from = Squish;
                for (float i = 0; i < 1; i += Engine.DeltaTime / time)
                {
                    Squish = Calc.LerpClamp(from, amount, i);
                    yield return null;
                }
                Squish = amount;
                if (wiggleOnEnd)
                {
                    if (Squish > 0)
                    {
                        WigglerY.StopAndClear();
                        WigglerY.Start();
                    }
                    else
                    {
                        WigglerX.StopAndClear();
                        WigglerX.Start();
                    }
                }
                onEnd?.Invoke();
            }
            private float prevSquishAmount, prevSquishTime;
            public void SquishTo(float amount, float time, bool wiggleOnEnd, Action onEnd = null)
            {
                if (prevSquishAmount == amount && prevSquishTime == time) return;
                prevSquishAmount = amount;
                prevSquishTime = time;
                squishRoutine.Replace(SquishToRoutine(amount, time, wiggleOnEnd, onEnd));
            }
            public void YoyoSquishTo(float amount, float time, bool wiggleOnEnd)
            {
                squishRoutine.Replace(SquishToRoutine(amount, time, false, () =>
                {
                    Unsquish(time, wiggleOnEnd);
                }));
            }
            public void Unsquish(float time, bool wiggleOnEnd, Action onEnd = null) => SquishTo(0, time, wiggleOnEnd, onEnd);
            public Wiggler WigglerX;
            public Wiggler WigglerY;
            public Wiggler ScaleWiggler;
            public override void Update()
            {
                base.Update();
                Vector2 scale = Scale + WiggleVector + new Vector2(Squish, -Squish);
                Sprite.Scale = scale;
                if (playerImmuneTimer > 0)
                {
                    playerImmuneTimer -= Engine.DeltaTime;
                }
                if (invincibleTimer > 0)
                {
                    invincibleTimer -= Engine.DeltaTime;
                }
                MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
                AfterMoveH();
                MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
                AfterMoveV();
                if (frictionX > 0) Speed.X = Calc.Approach(Speed.X, 0, frictionX * Engine.DeltaTime);
                if (frictionY > 0) Speed.Y = Calc.Approach(Speed.Y, 0, frictionY * Engine.DeltaTime);
                if (gravity != 0) Speed.Y = Calc.Approach(Speed.Y, 200 * Math.Sign(gravity), Math.Abs(gravity) * Engine.DeltaTime);
                Sprite.Color = ColorFunction?.Invoke(this) ?? origColor;
            }
            public void AfterMoveH()
            {
                if (StateMachine.State == StBounce)
                {
                    if (bouncesRemaining > 0 && bouncesRemaining < maxBounces)
                    {
                        if (CollideFirst<Orb>() is Orb orb && orb.Deadly)
                        {
                            bounceDirection.X *= -1;
                            bouncesRemaining--;
                        }
                    }
                }
            }
            public void AfterMoveV()
            {
                if (StateMachine.State == StBounce)
                {
                    if (bouncesRemaining > 0 && bouncesRemaining < maxBounces - 1)
                    {
                        if (CollideCheck<Orb>())
                        {
                            bounceDirection.Y *= -1;
                            bouncesRemaining--;
                        }
                    }
                }
            }
            public override void Render()
            {
                base.Render();
                //Ellipse(Position, Width * scaleX, Height * scaleY, Color, (int)(Math.Max(Width * scaleX, Height * scaleY) * 2));
                //Draw.Circle(Position + Parent.Shake + Shake, Radius, Color, (int)Radius * 2);
            }
            public void Ellipse(Vector2 position, float width, float height, Color color, int resolution)
            {
                Vector2 scale = new Vector2(width, height);
                Vector2 prevAngle = Vector2.UnitX * scale;
                for (int i = 1; i <= resolution; i++)
                {
                    Vector2 angle = Calc.AngleToVector(i * MathHelper.TwoPi / resolution, 1) * scale;
                    Draw.Line(position + prevAngle, position + angle, color);
                    prevAngle = angle;
                }
            }
            #endregion
            #region Utils
            public IEnumerator MoveTo(Vector2 position, float time, Ease.Easer ease = null)
            {
                ease ??= Ease.Linear;
                Vector2 p = Position;
                for (float i = 0; i < 1; i += Engine.DeltaTime / time)
                {
                    Position = Vector2.Lerp(p, position, ease(i));
                    yield return null;
                }
                Position = position;
            }
            public void UpdateColliders(float radius, bool updateCircle = true, bool updateHitbox = true)
            {
                if (updateCircle)
                {
                    Circle.Radius = radius + RadiusOffset;
                }
                if (updateHitbox)
                {
                    Hitbox.Width = radius * 2 + SizeOffset;
                    Hitbox.Height = radius * 2 + SizeOffset;
                    Hitbox.Position = -Hitbox.HalfSize;
                }
            }
            #endregion
        }
        public override void Render()
        {
            base.Render();
            //GFX.Game["objects/PuzzleIslandHelper/dvd"].DrawCentered(Center, Color.White);
            GameplayRenderer.End();
            PianoUtils.DrawUserPrimitives<VertexPositionColor>(SceneAs<Level>().Camera.Matrix, ForEachPass);
            GameplayRenderer.Begin();
        }
        public void ForEachPass(EffectPass pass)
        {
            if (R.Sprite.Visible && R.Visible) R.Sprite.DirectRenderVertices();
            if (G.Sprite.Visible && G.Visible) G.Sprite.DirectRenderVertices();
            if (B.Sprite.Visible && B.Visible) B.Sprite.DirectRenderVertices();
        }

        //might not use
        [Tracked]
        public class ColorMod : Component
        {
            public delegate void Modify(Color color, float timePassed);
            public Modify function;
            private Action onEnd;
            private float interval;
            private Func<Color> getBaseColor;
            private float startTime;
            private float duration;
            public bool RemoveOnEnd = true;
            public ColorMod(Modify modify, Color orig, bool start = true, float duration = -1, float interval = -1) : base(false, false)
            {
                getBaseColor = () => orig;
                this.interval = interval;
                function = modify;
                this.duration = duration;
                if (start)
                {
                    Start(duration);
                }
            }
            public ColorMod(Modify modify, Color orig, Action onEnd, bool start = true, float duration = -1, float interval = -1) : base(true, false)
            {
                getBaseColor = () => orig;
                this.interval = interval;
                function = modify;
                this.onEnd = onEnd;
                this.duration = duration;
                if (start) Start(duration);
            }
            public ColorMod(Modify modify, Func<Color> getBaseColor, bool start = true, float duration = -1, float interval = -1) : base(true, false)
            {
                this.getBaseColor = getBaseColor;
                this.interval = interval;
                function = modify;
                this.duration = duration;
                if (start) Start(duration);
            }
            public ColorMod(Modify modify, Func<Color> getBaseColor, Action onEnd, bool start = true, float duration = -1, float interval = -1) : base(true, false)
            {
                this.getBaseColor = getBaseColor;
                this.interval = interval;
                function = modify;
                this.onEnd = onEnd;
                this.duration = duration;
                if (start) Start(duration);
            }
            public void Start(float duration)
            {
                Active = true;
                this.duration = duration;
                startTime = Scene.TimeActive;
            }
            public void Start(float duration, Color baseColor)
            {
                getBaseColor = () => baseColor;
                Start(duration);
            }
            public void Start(float duration, Func<Color> getBaseColor)
            {
                this.getBaseColor = getBaseColor;
                Start(duration);
            }
            public void Stop()
            {
                onEnd?.Invoke();
                Active = false;
                function.Invoke(getBaseColor.Invoke(), Scene.TimeActive - startTime);
            }
            public override void Update()
            {
                base.Update();
                if (interval <= 0 || Scene.OnInterval(interval))
                {
                    function.Invoke(getBaseColor.Invoke(), Scene.TimeActive - startTime);
                }
                if (duration > 0)
                {
                    duration -= Engine.DeltaTime;
                    if (duration <= 0)
                    {
                        duration = 0;
                        if (RemoveOnEnd) RemoveSelf();
                        else Stop();
                    }
                }
            }
        }
    }

}