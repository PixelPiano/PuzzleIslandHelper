using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Cutscenes;
using Celeste.Mod.PuzzleIslandHelper.Entities.Tower;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Core.Tokens;
using static Celeste.Session;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Singularity
{
    [CustomEntity("PuzzleIslandHelper/Singularity")]
    [Tracked]
    public class SingularityActor : Actor
    {
        public const double RubberbandFactor = 0.0099999997764825821;
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

        public const int StIdle = 0;
        public const int StDummy = 1;
        public const int StPath = 2;
        public const int StSlam = 3;
        public const int StLaunch = 4;
        public const int StEndingAFlyUp = 6;
        public const int StEndingAMinigame = 7;
        private int startState;
        public float AutoOrbitSpeedMult = 0.5f;
        public bool UseAutoOrbitSpeed;
        public float AutoOrbitSpeed;
        private Vector2[] launchNodes;
        public float LaunchSpeed;
        public int LaunchNode;
        public bool Launched;

        #region Variables
        public Circle Circle;
        public Hitbox Collision;
        public float Radius;
        public float RadiusOffset;
        public float SizeOffset;
        public float Distance = 8;
        private Vector2 speed;
        public Vector2 Speed { get; set; }
        public PlayerCollider PlayerCollider;
        public FlagList Flag;
        public Vector2 RenderOffset;
        public SingularitySprite Sprite;
        public StateMachine StateMachine;
        public float Orbit
        {
            get => Sprite.Orbit;
            set => Sprite.Orbit = value;
        }
        public float OrbitRate
        {
            get => Sprite.OrbitRate;
            set => Sprite.OrbitRate = value;
        }
        public float OrbitRateMult
        {
            get => Sprite.OrbitRateMult;
            set => Sprite.OrbitRateMult = value;
        }
        public int State
        {
            get => StateMachine.State;
            set => StateMachine.ForceState(value);
        }
        public Collision StateCollideH, StateCollideV;
        public Vector2 OrigPosition;
        public Vector2 PrevPosition;
        public bool UpdateCircle = true;
        public bool UpdateHitbox = true;
        public Vector2 Shake;
        public BetterShaker Shaker;
        public bool Naive;
        public TimeRateModifier TimeMod;
        public bool CanLaunch;
        public bool AtLaunchNode = true;
        public List<VertexOrb.AfterImage> AfterImages = [];
        public float AfterImageInterval = 0.05f;
        public bool SpawnAfterImages;
        private float afterImagesTimer;
        public bool AnchorAfterImages = true;
        public Vector2 AfterImageSpeed;
        #endregion
        public SingularityActor(EntityData data, Vector2 offset) : this(data.Position + offset)
        {
            launchNodes = data.NodesWithPosition(offset);
            Add(Shaker = new BetterShaker((v) =>
            {
                Shake += v;
            }));
            Flag = data.FlagList("flag");
            startState = data.Attr("startState") switch
            {
                "Idle" or "Launch" => StIdle,
                "Dummy" => StDummy,
                "Path" => StPath,
                "Slam" => StSlam,
                _ => StIdle
            };
            if (data.Attr("startState") == "Launch")
            {
                CanLaunch = true;
            }
            string counter = data.Attr("counterIndex");
            if (int.TryParse(counter, out int result))
            {
                pathCounter = result;
            }
            PathBreakObjects = data.Bool("breakObjects", true);
            PathFlag = data.FlagList("pathFlag");
            pathRemoveIfPathFlagFalse = data.Bool("removeIfPathFlagFalse");
            pathRemoveIfPathDoesNotExist = data.Bool("removeIfPathDoesNotExist");
            pathRemoveIfOutOfLevel = data.Bool("removeIfOutOfLevel", true);
            pathSetEndPathFlagsIfRemovedEarly = data.Bool("setEndFlagsIfRemoved");
            pathStardust = data.Bool("spawnStardust");
            easeIntoPath = data.Bool("easeIn");
            PathNaive = data.Bool("naive", true);
            PathID = data.Attr("pathID");
        }
        public SingularityActor(Vector2 position) : base(position)
        {
            Tag |= Tags.TransitionUpdate;
            Add(TimeMod = new TimeRateModifier(1));
            OrigPosition = position;
            Collider = Collision = new Hitbox(10, 10, -5, -5);
            Circle = new Circle(8);
            Radius = 8;
            Add(PlayerCollider = new PlayerCollider(OnPlayer, Circle));
            Add(StateMachine = new StateMachine());
            StateMachine.SetCallbacks(StIdle, IdleUpdate, null, IdleBegin, IdleEnd);
            StateMachine.SetCallbacks(StDummy, DummyUpdate, null, DummyBegin, DummyEnd);
            StateMachine.SetCallbacks(StPath, PathUpdate, null, PathBegin, PathEnd);
            StateMachine.SetCallbacks(StSlam, SlamUpdate, null, SlamBegin, SlamEnd);
            StateMachine.SetCallbacks(StLaunch, LaunchUpdate, LaunchRoutine, LaunchBegin, LaunchEnd);
            StateMachine.SetCallbacks(StEndingAFlyUp, FlyUpUpdate, null, FlyUpBegin, null);
        }
        public virtual void CreateAfterImage(float fillMult, float edgeMult, float scaleSpeed = 0, Vector2 speed = default)
        {
            Sprite.CreateAfterImage(fillMult, edgeMult, scaleSpeed, speed);
        }
        public void Wiggle() => Wiggle(0.5f);
        public void Wiggle(float duration) => Wiggle(duration, 4);
        public void Wiggle(float duration, float frequency)
        {
            foreach (var v in Sprite.ActiveOrbs)
            {
                v.Wiggle(duration, frequency);
            }
        }
        #region Idle
        private float idleSpeedMult = 1;
        private Vector2? idleTarget = null;
        private string[] idleMarkers;
        private float durationAtTarget;
        private float atTargetTimer;
        private int idlePositionsIndex;
        private Vector2[] idlePositions;
        public void Idle()
        {
            durationAtTarget = atTargetTimer = -1;
            idlePositionsIndex = 0;
            idleMarkers = null;
            idleSpeedMult = 1;
            idleTarget = null;
            StateMachine.ForceState(StIdle);
        }
        public void Idle(Vector2 start, float speedMult = 1)
        {
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
            idlePositionsIndex = 0;
        }
        public void IdleEnd()
        {
            idleTarget = null;
        }
        public int IdleUpdate()
        {
            if (OrbitRateMult != 1)
            {
                OrbitRateMult = Calc.Approach(OrbitRateMult, 1, Engine.DeltaTime * 5f);
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
            UseAutoOrbitSpeed = false;
        }
        #endregion
        #region Slam
        private Vector2 slamDirection = Vector2.UnitX;
        private Vector2 slamTarget;
        private Vector2 slamOrig;
        private Session.Slider windUpSlider;
        private Session.Slider chargeSlider;
        private Session.Slider recoilSlider;
        private Counter slamStateCounter;
        public void SlamBegin()
        {
            UseAutoOrbitSpeed = true;
            AutoOrbitSpeedMult = 1;
            slamOrig = Position;
            Level level = SceneAs<Level>();
            while (!CollideCheck<Solid>(Position))
            {
                Position += slamDirection;
                if (!level.IsInBounds(this))
                {
                    Position = slamOrig;
                    State = StDummy;
                    return;
                }
            }
            slamTarget = Position;
            Position = slamOrig;
        }
        public int SlamUpdate()
        {
            switch (slamStateCounter.Value)
            {
                case 0: //wait
                    StopShaking();
                    Position = slamOrig;
                    break;
                case 1: //windUp
                    StopShaking();
                    Position = Vector2.Lerp(slamOrig, slamOrig - slamDirection * 6, windUpSlider.Value);
                    break;
                case 2: //charge
                    StopShaking();
                    Position = Vector2.Lerp(slamOrig - slamDirection * 6, slamTarget, chargeSlider.Value);
                    break;
                case 3: //impact
                    StartShaking(-1);
                    Position = slamTarget;
                    break;
                case 4: //recoil delay
                    StopShaking();
                    Position = slamTarget;
                    break;
                case 5: //recoil
                    StopShaking();
                    Position = Vector2.Lerp(slamTarget, slamOrig, recoilSlider.Value);
                    break;
            }
            return StSlam;
        }
        public void SlamEnd()
        {
            StopShaking();
            AutoOrbitSpeed = 0.5f;
        }
        [CustomEvent("PuzzleIslandHelper/SingularitySlam")]
        private class slamCutscene : CutsceneEntity
        {
            private Coroutine zoomCoroutine;
            private SingularityActor singularity;
            private Solid solid;
            private Player player;
            public slamCutscene(EventTrigger trigger, Player player, string eventID) : base(true)
            {
                this.player = player;
            }

            public override void OnBegin(Level level)
            {
                Tag |= Tags.FrozenUpdate;
                singularity = level.Tracker.GetEntity<SingularityActor>();

                Vector2 from = singularity.Position;
                Vector2 dir = singularity.slamDirection;
                Vector2 to = Calc.Clamp(singularity.Position + new Vector2(dir.X * level.Bounds.Width, dir.Y * level.Bounds.Height), level.Bounds.Left, level.Bounds.Top, level.Bounds.Right, level.Bounds.Bottom);
                List<Solid> all = Scene.CollideAll<Solid>(from, to);
                foreach (Solid solid in all)
                {
                    if (solid is not SolidTiles)
                    {
                        this.solid = solid;
                        break;
                    }
                }
                if (solid != null)
                {
                    Add(new Coroutine(routine()));
                }
                else
                {
                    RemoveSelf();
                }
            }
            private IEnumerator routine()
            {
                //wait for player to land on ground.
                player.DisableMovement();
                player.ForceCameraUpdate = true;
                while (!player.OnGround())
                {
                    yield return null;
                }
                yield return 0.5f;
                player.ForceCameraUpdate = false;
                Session session = Level.Session;
                Session.Slider windUp = session.GetSliderObject("SingularitySlam:WindUp");
                Session.Slider charge = session.GetSliderObject("SingularitySlam:Charge");
                Session.Slider shake = session.GetSliderObject("SingularitySlam:Shake");
                Counter state = session.GetCounterObject("SingularitySlam:State");
                ShakeController.PersistentShake controller = Level.Tracker.GetEntity<ShakeController.PersistentShake>();
                //find the 2d point to zoom towards
                Marker zoomMarker = Marker.Find("SingularitySlam:Zoom");
                if (zoomMarker != null)
                {
                    float zoom = 1.5f;
                    float duration = 1;
                    if (zoomMarker.Args.TryGetValue("zoom", out string value) && float.TryParse(value, out float result))
                    {
                        zoom = result;
                    }
                    if (zoomMarker.Args.TryGetValue("duration", out string value2) && float.TryParse(value, out float result2))
                    {
                        duration = result2;
                    }
                    Add(zoomCoroutine = new Coroutine(Level.ZoomToWorld(zoomMarker.Position, zoom, duration)));
                }
                //wait for the PersistentShake entity to restart, then remove it
                if (controller != null)
                {
                    while (state.Value != 0)
                    {
                        yield return null;
                    }
                    controller.RemoveSelf();
                }
                state.Value = 0;
                //make sure all effects from auto slam are squashed
                ShakeController.ClearSliderValues(Scene);
                //ensure PersistentShake never activates automatically again this session
                ShakeController.SingularitySlamFlag.Set(true);
                //we define our own delay in place of SingularitySlam:DelayTime
                yield return 1;

                //wind up
                state.Value = 1;
                yield return PianoUtils.Lerp(Ease.SineInOut, 1.2f, f => windUp.Value = f, true);
                yield return 0.5f;
                singularity.State = StDummy;
                singularity.SpawnStardust = true;
                singularity.speed = singularity.slamDirection * 400f;
                singularity.Naive = true;
                while (Level.IsInBounds(singularity.Position, 16))
                {
                    if (singularity.CollideFirst<Solid>() is Solid solid && solid is not SolidTiles)
                    {
                        solid.Break('1', singularity.Position, singularity.slamDirection, true, true);
                    }
                    yield return null;
                }
                yield return 1;
                yield return Level.ZoomBack(1);
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                solid.RemoveSelf();
                if (solid.SourceId.Key != default(EntityID).Key)
                {
                    level.Session.DoNotLoad.Add(solid.SourceId);
                }
                level.Session.SetFlag("SingularitySlam");
                zoomCoroutine?.RemoveSelf();
                if (level.Tracker.GetEntity<SingularityActor>() is SingularityActor s)
                {
                    if (level.CollideFirst<Solid>(s.Center, s.Center + Vector2.UnitX * 40) is Solid solid)
                    {
                        if (solid is not SolidTiles)
                        {
                            solid.RemoveSelf();
                        }
                    }
                    s.RemoveSelf();
                }
                level.EnableMovement();
                level.ResetZoom();
            }
        }
        #endregion
        #region Flee
        public class StardustGroup : Entity
        {
            public List<Stardust> Stardust;
            public Rectangle Bounds;
            public bool UpdateQueued;
            public StardustGroup(params Stardust[] stardust) : base()
            {
                Stardust = [.. stardust];
                Depth = stardust[0].Depth;
                foreach (var s in stardust)
                {
                    s.Visible = false;
                    s.Group = this;
                }
                Bounds = new Rectangle();
                Add(new PostUpdateHook(() =>
                {
                    if (UpdateQueued)
                    {
                        UpdateBounds();
                    }
                    Camera c = SceneAs<Level>().Camera;
                    Visible = Bounds.Left < c.Right && Bounds.Right > c.Left && Bounds.Top < c.Bottom && Bounds.Bottom > c.Top;
                    UpdateQueued = false;
                }));
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                UpdateBounds();
                Camera c = (scene as Level).Camera;
                Visible = Bounds.Left < c.Right && Bounds.Right > c.Left && Bounds.Top < c.Bottom && Bounds.Bottom > c.Top;
            }
            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                Draw.HollowRect(Bounds, Color.Orange);
            }
            public override void Render()
            {
                base.Render();
                foreach (var s in Stardust)
                {
                    s.Render();
                }
            }
            public override void Update()
            {
                base.Update();
                if (Stardust.Count == 0)
                {
                    RemoveSelf();
                }
            }
            public void UpdateBounds()
            {
                Bounds = default;
                int left = int.MaxValue, top = int.MaxValue;
                int right = int.MinValue, bottom = int.MinValue;
                foreach (var s in Stardust)
                {
                    left = Math.Min(left, (int)s.X);
                    right = Math.Max(right, (int)s.X);
                    top = Math.Min(top, (int)s.Y);
                    bottom = Math.Max(bottom, (int)s.Y);
                }
                Bounds.X = left;
                Bounds.Y = top;
                Bounds.Width = right - left;
                Bounds.Height = bottom - top;
            }

        }
        [Tracked]
        public class Stardust : Actor
        {
            public StardustGroup Group;
            public const float LerpDelay = 2;
            public const float LerpTime = 1;
            public const float RemoveDelay = 1;
            public const float RemoveLerpTime = 1.5f;
            public Color Color;
            private float xOffset;
            public float Frequency = 4;
            private float dist = 4;
            public float Timer;
            private float gravity = 90f;
            private float speedY = 20f;
            public bool landed;
            private float initialX;
            private float shade = 0.2f;
            private float timerOffset;
            public Stardust(Vector2 position, Color color) : base(position)
            {
                initialX = position.X;
                Color = color;
                Tag |= Tags.TransitionUpdate;
                Collider = new Hitbox(1, 1);
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                Timer += (timerOffset = Calc.Random.NextFloat());
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                if (CollideCheck<Solid>()) RemoveSelf();
            }
            public bool InWater;
            public Water Water;
            public Color FinalColor;
            public bool FadeOut = true;
            public void Advance(Level level)
            {
                xOffset = (float)Math.Sin(Timer * Frequency) * dist;
                Timer += Engine.DeltaTime;
                speedY = Calc.Approach(speedY, gravity, 30f * Engine.DeltaTime);
                Vector2 prev = Position;
                MoveV(speedY * Engine.DeltaTime, OnCollideV);
                MoveToX(initialX + xOffset);
                if (CollideFirst<Water>() is Water water)
                {
                    Water = water;
                    if (!InWater)
                    {
                        water.TopSurface.DoRipple(Position, 0.2f);
                    }
                    InWater = true;
                    landed = true;
                    if (FadeOut)
                    {
                        Add(new Coroutine(routine()));
                    }
                }
            }
            private IEnumerator routine()
            {
                yield return LerpDelay;
                Color from = Color;
                yield return PianoUtils.Lerp(Ease.SineInOut, LerpTime, f => Color = Color.Lerp(from, Color.Gray, f), true);
                yield return RemoveDelay;
                yield return PianoUtils.Lerp(Ease.SineInOut, RemoveLerpTime, f => Color = Color.Lerp(Color.Gray, Color.Transparent, f), true);
                RemoveSelf();
            }
            public override void Update()
            {
                base.Update();
                Vector2 prevPosition = Position;
                if (Engine.Scene.OnInterval(0.15f, timerOffset))
                {
                    shade = -shade;
                }
                //try to minimize how much processing each particle takes since we might be using a lot of them
                if (!landed)
                {
                    Level level = Engine.Scene as Level;
                    Advance(level);
                    if (Y > level.Bounds.Bottom)
                    {
                        RemoveSelf();
                        return;
                    }
                }
                if (Water != null)
                {
                    Vector2 vector = Water.TopSurface.Position + Water.TopSurface.Outwards.Perpendicular() * (-Water.TopSurface.Width / 2 + (Position.X - Water.X)) + Water.TopSurface.Outwards * Water.TopSurface.GetSurfaceHeight(Position.X - Water.X);
                    Position.Y = vector.Y - 2;
                }
                if (shade < 0)
                {
                    FinalColor = Color.Lerp(Color, Color.Black, Math.Abs(shade));
                }
                else if (shade > 0)
                {
                    FinalColor = Color.Lerp(Color, Color.White, shade);
                }
                else
                {
                    FinalColor = Color;
                }
                if (Group != null && prevPosition != Position)
                {
                    Group.UpdateQueued = true;
                }
            }
            private void OnCollideV(CollisionData data)
            {
                speedY = 0;
                landed = true;
                if (FadeOut)
                {
                    Add(new Coroutine(routine()));
                }
            }
            public override void Render()
            {
                Draw.Point(Position, FinalColor);
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                if (Group != null)
                {
                    Group.Stardust.Remove(this);
                    Group = null;
                }
            }
        }

        public bool SpawnStardust;
        public static void SpawnParticle(Scene scene, Vector2 center, int maxRange, Color color)
        {
            scene.Add(new Stardust(center + Vector2.One * Calc.Random.Range(-maxRange, maxRange), color));
        }
        public static Stardust[] SpawnStardustGroup(Scene scene, Vector2 center, int maxRange, Color color)
        {
            Stardust[] array = new Stardust[4];
            scene.Add(array[0] = new Stardust(center - Vector2.One * Calc.Random.Range(1, maxRange), color));
            scene.Add(array[1] = new Stardust(center + Vector2.One * Calc.Random.Range(1, maxRange), color));
            scene.Add(array[2] = new Stardust(center + new Vector2(-1, 1) * Calc.Random.Range(1, maxRange), color));
            scene.Add(array[3] = new Stardust(center + new Vector2(1, -1) * Calc.Random.Range(1, maxRange), color));
            scene.Add(new StardustGroup(array));
            return array;
        }
        #endregion
        #region Path
        private float pathSpeedMult = 1;
        private Tween angleEaseTween;
        private int PathNodeIndex;
        private float pathApproachSpeedMult;
        private int currentPathLoop;
        public string PathID;
        public SingularityPath Path;
        public bool PathExists => Path != null;
        public float PathPauseTimer;
        private bool pathStarted;

        public bool PathNaive;
        private int? pathCounter;
        public bool PathBreakObjects;
        private bool prevNaive;
        public FlagList PathFlag;
        private bool pathRemoveIfPathFlagFalse;
        private bool pathRemoveIfPathDoesNotExist;
        private bool pathRemoveIfOutOfLevel;
        private bool pathSetEndPathFlagsIfRemovedEarly;
        private bool pathStardust;
        private bool easeIntoPath;

        private float pathMaxSpeed;
        public void PathBegin()
        {
            pathSpeedMult = 1;
            Path = null;
            SpawnStardust = false;
            if (!PathFlag)
            {
                if (pathRemoveIfPathFlagFalse)
                {
                    RemoveSelf();
                }
                else
                {
                    State = StDummy;
                }
                return;
            }
            if (!string.IsNullOrEmpty(PathID) &&
                (!pathCounter.HasValue || SceneAs<Level>().Session.GetCounter(PathID) < pathCounter.Value))
            {
                foreach (SingularityPath path in Scene.Tracker.GetEntities<SingularityPath>())
                {
                    if (pathCounter.HasValue)
                    {
                        if (path.ID == PathID + ':' + pathCounter.Value)
                        {
                            Path = path;
                            break;
                        }
                    }
                    else if (path.ID == PathID)
                    {
                        Path = path;
                        break;
                    }
                }
            }

            if (Path == null)
            {
                if (pathRemoveIfPathDoesNotExist)
                {
                    RemoveSelf();
                }
                else
                {
                    State = StDummy;
                }
                return;
            }
            else
            {
                if (pathCounter.HasValue)
                {
                    Counter c = SceneAs<Level>().Session.GetCounterObject(PathID);
                    c.Value = Math.Max(c.Value, pathCounter.Value);
                }
                pathMaxSpeed = Path.Speed;
                pathStarted = Path.PathStartMode switch
                {
                    SingularityPath.PathStartModes.EntityOnScreen => SceneAs<Level>().IsInCamera(Position, Radius * 2),
                    SingularityPath.PathStartModes.Awake => true,
                    SingularityPath.PathStartModes.Triggered => Path.Triggered
                };
                if (pathStarted)
                {
                    Path.Start();
                    SpawnStardust = pathStardust;
                }
                PathNodeIndex = 0;
                pathApproachSpeedMult = easeIntoPath ? 0 : 1;
                currentPathLoop = 0;
                prevNaive = Naive;
                Naive = PathNaive;
                UseAutoOrbitSpeed = true;
                StateCollideH = null;
                StateCollideV = null;
            }
        }
        public Vector2 PathSpeed;
        public void PathEnd()
        {
            UpdatePathCounterIfValid(Scene);
            Path?.Stop();
            Naive = prevNaive;
            UseAutoOrbitSpeed = false;
            SpawnStardust = false;
        }
        public int PathUpdate()
        {
            Level level = SceneAs<Level>();
            bool skipIntermediateUpdate = false;
            if (!PathFlag)
            {
                Path?.Stop();
                pathStarted = false;
                if (pathRemoveIfPathFlagFalse)
                {
                    RemoveSelf();
                }
            }
            else if (PathExists)
            {
                if (!pathStarted)
                {
                    pathStarted = Path.PathStartMode switch
                    {
                        SingularityPath.PathStartModes.EntityOnScreen => level.IsInCamera(Position, Radius * 2),
                        SingularityPath.PathStartModes.Awake => Path.Active && Path.Scene != null,
                        SingularityPath.PathStartModes.Triggered => Path.Triggered,
                    };
                    if (pathStarted)
                    {
                        Path.Start();
                        SpawnStardust = pathStardust;
                        skipIntermediateUpdate = true;
                    }
                }
                if (pathStarted && !skipIntermediateUpdate)
                {
                    if (pathRemoveIfOutOfLevel && !level.IsInBounds(Position, Radius * 1.5f))
                    {
                        RemoveSelf();
                    }
                    else if (PathBreakObjects)
                    {
                        if (CollideFirst<DashBlock>() is DashBlock block)
                        {
                            block.Break(Position, Vector2.Normalize(Position - block.Center), true);
                        }
                        if (CollideFirst<LabDoor>() is LabDoor door)
                        {
                            door.RemoveAndFlagAsGone();
                        }
                    }
                    if (PathPauseTimer > 0)
                    {
                        pathApproachZero();
                    }
                    else
                    {
                        if (TryMoveTowards(Path.GetPoint(PathNodeIndex), Path.Speed, Path.ExitRadius, Path.ApproachSpeed * pathApproachSpeedMult, out _, out _))
                        {
                            if (!advanceInPath())
                            {
                                Path.FlagsOnEnd.State = true;
                                if (Path.RemoveEntityOnEnd)
                                {
                                    RemoveSelf();
                                }
                            }
                        }
                        else
                        {
                            pathApproachSpeedMult = Calc.Approach(pathApproachSpeedMult, 1, Engine.DeltaTime);
                        }
                    }
                }
            }
            else
            {
                pathApproachZero();
            }
            return StPath;
        }
        private bool advanceInPath()
        {
            if (PathNodeIndex + 1 >= Path.Points) //if at end of path
            {
                if (Path.MaxLoops < 0) //if loop forever...
                {
                    PathNodeIndex = 0; //reset the index
                }
                else //else if loop only a set number of times...
                {
                    if (currentPathLoop < Path.MaxLoops) //if current loop count is below...
                    {
                        //reset the index and increment total loops
                        PathNodeIndex = 0;
                        currentPathLoop++;
                    }
                    else return false;
                    //else, we've reached the max # of loops for this path.
                }
            }
            else PathNodeIndex++;
            return true;
        }
        private void pathApproachZero(float friction = 1)
        {
            PathSpeed = Calc.Approach(PathSpeed, Vector2.Zero, (1 - pathApproachSpeedMult) * pathMaxSpeed * Engine.DeltaTime);
            pathApproachSpeedMult = Calc.Approach(pathApproachSpeedMult, 0, Engine.DeltaTime * friction);
        }
        #endregion
        #region Launch
        private Player launchPlayer;
        private Tween multTween;
        public void LaunchBegin()
        {
            CanLaunch = false;
            Sprite.OrbitRateMult = 0;
            multTween?.Stop();
        }
        public void LaunchEnd()
        {
            if (CanLaunch)
            {
                Sprite.OrbitRateMult = 1;
            }
            CanLaunch = true;
        }
        public int LaunchUpdate()
        {
            return StLaunch;
        }
        public IEnumerator LaunchRoutine()
        {
            LaunchNode++;
            Collidable = false;
            bool finalLaunch = LaunchNode >= launchNodes.Length;
            Level level = Scene as Level;
            bool hasGolden = false;
            if (!finalLaunch)
            {
                foreach (Follower follower in launchPlayer.Leader.Followers)
                {
                    if (follower.Entity is Strawberry { Golden: not false })
                    {
                        hasGolden = true;
                        break;
                    }
                }
            }
            bool transitionToEndingA = finalLaunch && !hasGolden;
            if (!finalLaunch)
            {
                Audio.Play("event:/char/badeline/booster_begin", Position);
            }
            else
            {
                Audio.Play("event:/char/badeline/booster_final", Position);
            }

            if (launchPlayer.Holding != null)
            {
                launchPlayer.Drop();
            }
            launchPlayer.StateMachine.State = Player.StDummy;
            launchPlayer.DummyAutoAnimate = false;
            launchPlayer.DummyGravity = false;
            launchPlayer.Speed = Vector2.Zero;
            launchPlayer.Sprite.Play(PlayerSprite.Fall);
            if (launchPlayer.Inventory.Dashes > 1)
            {
                launchPlayer.Dashes = 1;
            }
            else
            {
                launchPlayer.RefillDash();
            }

            launchPlayer.RefillStamina();
            launchPlayer.Speed = Vector2.Zero;
            Vector2 playerFrom = launchPlayer.Position;
            Vector2 playerTo = Position;
            float d;
            if (Orbit > MathHelper.Pi)
            {
                d = MathHelper.TwoPi - Orbit;
            }
            else
            {
                d = MathHelper.Pi - Orbit;
            }
            OrbitRateMult = 0;
            float orbitFrom = Orbit;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / 0.2f)
            {
                Orbit = Calc.LerpClamp(orbitFrom, orbitFrom + d, Ease.SineOut(p));
                Vector2 vector = Vector2.Lerp(playerFrom, playerTo, p);
                if (launchPlayer.Scene != null)
                {
                    launchPlayer.MoveToX(vector.X);
                }
                if (launchPlayer.Scene != null)
                {
                    launchPlayer.MoveToY(vector.Y);
                }
                yield return null;
            }
            if (finalLaunch)
            {
                Vector2 screenSpaceFocusPoint = new Vector2(Calc.Clamp(launchPlayer.X - level.Camera.X, 120f, 200f), Calc.Clamp(launchPlayer.Y - level.Camera.Y, 60f, 120f));
                Add(new Coroutine(level.ZoomTo(screenSpaceFocusPoint, 1.5f, 0.18f)));
                TimeMod.Multiplier = 0.5f;
            }
            else
            {
                Audio.Play("event:/char/badeline/booster_throw", Position);
            }
            //badeline.Sprite.Play("boost");
            yield return 0.1f;
            if (!launchPlayer.Dead)
            {
                launchPlayer.MoveV(5f);
            }

            yield return 0.1f;

            Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () =>
            {
                if (launchPlayer.Dashes < launchPlayer.Inventory.Dashes)
                {
                    launchPlayer.Dashes = launchPlayer.Inventory.Dashes;
                }
                (Scene as Level).Displacement.AddBurst(Position, 0.25f, 8f, 32f, 0.5f);
            }, 0.15f, start: true));
            (Scene as Level).Shake();
            if (!finalLaunch)
            {
                launchPlayer.SummitLaunch(X);
                launchPlayer.Speed *= 0.8f;
                Vector2 from = Position;
                Vector2 to = launchNodes[LaunchNode];
                float val = Vector2.Distance(from, to) / 320f;
                val = Math.Min(2f, val);
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineInOut, val, start: true);
                float dist = Sprite.Distance;
                SpawnAfterImages = true;
                tween.OnUpdate = (Tween t) =>
                {
                    float e = t.Percent;
                    Sprite.Distance = e switch
                    {
                        < 0.2f => Calc.LerpClamp(dist, 20, Ease.SineIn(e / 0.2f)),
                        < 0.5f => Calc.LerpClamp(20, -20, Ease.Linear((e - 0.2f) / 0.3f)),
                        < 0.8f => Calc.LerpClamp(-20, 20, Ease.Linear((e - 0.5f) / 0.3f)),
                        _ => Calc.LerpClamp(20, dist, Ease.SineOut((e - 0.8f) / 0.2f))
                    };
                    Position = Vector2.Lerp(from, to, t.Eased);
                    if (t.Eased < 0.9f && Scene.OnInterval(0.03f))
                    {
                        TrailManager.Add(launchPlayer, Player.TwoDashesHairColor, 0.5f, frozenUpdate: false, useRawDeltaTime: false);
                        level.ParticlesFG.Emit(BadelineBoost.P_Move, 1, Center, Vector2.One * 4f);
                    }
                };
                tween.OnComplete = (Tween t) =>
                {
                    State = StIdle;
                    Collidable = true;
                    Sprite.Distance = dist;
                    SpawnAfterImages = false;
                    multTween = Tween.Set(this, Tween.TweenMode.Oneshot, 0.4f, Ease.SineInOut, t =>
                    {
                        OrbitRateMult = t.Eased;
                    }, t =>
                    {
                        OrbitRateMult = 1;
                    });
                };
                Add(tween);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                level.DirectionalShake(-Vector2.UnitY);
                level.Displacement.AddBurst(Center, 0.4f, 8f, 32f, 0.5f);
            }
            else
            {
                /*                if (finalCh9Boost)
                                {
                                    Ch9FinalBoostSfx = Audio.Play("event:/new_content/char/badeline/booster_finalfinal_part2", Position);
                                }*/

                Engine.FreezeTimer = 0.1f;
                yield return null;
                if (transitionToEndingA)
                {
                    level.TimerHidden = true;
                }

                Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
                level.Flash(Color.White * 0.5f, drawPlayerOver: true);
                level.DirectionalShake(-Vector2.UnitY, 0.6f);
                level.Displacement.AddBurst(Center, 0.6f, 8f, 64f, 0.5f);
                level.ResetZoom();
                launchPlayer.SummitLaunch(X);
                TimeMod.Multiplier = 1f;
                Scene.Add(new EndingA(launchPlayer, this));
                SceneAs<Level>().Displacement.AddBurst(base.Center, 0.5f, 24f, 96f, 0.4f);
                SceneAs<Level>().Particles.Emit(BadelineOldsite.P_Vanish, 12, base.Center, Vector2.One * 6f);
                SceneAs<Level>().CameraLockMode = Level.CameraLockModes.None;
                SceneAs<Level>().CameraOffset = new Vector2(0f, -16f);
            }
        }
        #endregion
        #region FlyUp
        public float FlyUpSpiralRadius = 32f;
        private float flyUpSpeedMult = 1.2f;
        private Vector2 spiralStart;
        public void FlyUpBegin()
        {
            FlyUpSpiralRadius = 32f;
            speed.X = 0;
            speed.Y = 0;
            spiralStart = Position;
            flyUpSpeedMult = 1.2f;
            Tween.Set(this, Tween.TweenMode.Oneshot, 0.75f, Ease.SineOut, t =>
            {
                flyUpSpeedMult = Calc.LerpClamp(1.2f, 1, t.Eased);
            });
        }
        public int FlyUpUpdate()
        {
            float speed = 0;
            Player player = Scene.GetPlayer();
            if (player != null)
            {
                speed = player.Speed.Y * flyUpSpeedMult;
            }
            MoveV(speed * Engine.DeltaTime);
            float y = spiralStart.Y - Y;
            MoveToX(spiralStart.X +
                (float)(double)(FlyUpSpiralRadius * Math.Sin(y * Math.PI / 40f)));
            return StEndingAFlyUp;
        }
        #endregion

        #region Ending A Minigame
        #endregion
        public void StartShaking(float time = -1) => Sprite?.ShakeFor(time);
        public void StopShaking() => Sprite?.StopShaking();
        public override void Update()
        {

            Vector2 speedMod = default;
            if (StateMachine.State == StPath)
            {
                if (PathPauseTimer > 0)
                {
                    speedMod = -speed;
                    PathPauseTimer -= Engine.DeltaTime;
                }
            }
            else
            {
                pathSpeedMult = Calc.Approach(pathSpeedMult, 0, Engine.DeltaTime * 5);
            }
            PrevPosition = Position;
            Speed = speed + speedMod + PathSpeed * pathSpeedMult;
            if (Naive)
            {
                NaiveMove(Speed * Engine.DeltaTime);
            }
            else
            {
                MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
            }
            float orbitSpeed;
            if (UseAutoOrbitSpeed)
            {
                orbitSpeed = AutoOrbitSpeed = (Position - PrevPosition).Length() / Engine.DeltaTime * AutoOrbitSpeedMult;
            }
            else
            {
                orbitSpeed = OrbitRate;
            }
            Orbit = (Orbit + (orbitSpeed * OrbitRateMult) * Engine.DeltaTime) % MathHelper.TwoPi;
            Circle.Radius = Radius + RadiusOffset;
            Position += RenderOffset;
            base.Update();
            if ((afterImagesTimer > 0 || SpawnAfterImages) && Scene.OnInterval(AfterImageInterval))
            {
                CreateAfterImage(0.5f, 0.5f, 0, AfterImageSpeed);
            }
            if (afterImagesTimer > 0)
            {
                afterImagesTimer -= Engine.DeltaTime;
            }
            Position -= RenderOffset;
            if (SpawnStardust)
            {
                float offset = 0;
                int count = 0;
                foreach (var o in Sprite.ActiveOrbs)
                {
                    if (Scene.OnInterval(0.1f, offset))
                    {
                        SpawnStardustGroup(Scene, o.AutoPosition + Sprite.Position + Sprite.shake, 4, o.GetFillColor(Color.Lerp(o.Color, o.ColorB, o.FillColorLerp), 0));
                    }
                    count++;
                    offset += 0.05f;
                }
            }
            if (Collidable)
            {
                foreach (SingularityFlagTrigger trigger in CollideAll<SingularityFlagTrigger>())
                {
                    trigger.Triggered = true;
                    trigger.Flag.State = true;
                }
            }
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            Session session = (scene as Level).Session;
            windUpSlider = session.GetSliderObject("SingularitySlam:WindUp");
            chargeSlider = session.GetSliderObject("SingularitySlam:Charge");
            recoilSlider = session.GetSliderObject("SingularitySlam:Recoil");
            slamStateCounter = session.GetCounterObject("SingularitySlam:State");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!Flag)
            {
                RemoveSelf();
            }
            else
            {
                Add(Sprite = new SingularitySprite(Radius, PianoModule.Session.RescuedRedOrb));
                Sprite.Update();
                State = startState;
            }
        }
        public void UpdatePathCounterIfValid(Scene scene)
        {
            Level level = scene as Level;
            if (pathCounter.HasValue && !string.IsNullOrEmpty(PathID))
            {
                if (level.Session.GetCounter(PathID) < pathCounter.Value)
                {
                    level.Session.SetCounter(PathID, pathCounter.Value);
                }
            }
        }
        public override void Removed(Scene scene)
        {
            if (State == StPath)
            {
                UpdatePathCounterIfValid(scene);
                if (Path != null && pathSetEndPathFlagsIfRemovedEarly)
                {
                    Path.FlagsOnEnd.State = true;
                }
            }
            base.Removed(scene);
        }
        public virtual void OnCollideH(CollisionData data)
        {
            StateCollideH?.Invoke(data);
        }
        public virtual void OnCollideV(CollisionData data)
        {
            StateCollideV?.Invoke(data);
        }
        public virtual void OnPlayer(Player player)
        {
            if (CanLaunch)
            {
                if (State == StLaunch) State = StIdle;
                launchPlayer = player;
                State = StLaunch;
            }
        }
        public void EaseToAngle(int dir, float angle, float duration = 1)
        {
            Orbit %= MathHelper.TwoPi;
            float next2 = dir * angle;
            OrbitRateMult = 0;
            OrbitRate = 0;
            float from2 = Orbit;
            angleEaseTween?.RemoveSelf();
            angleEaseTween = Tween.Set(this, Tween.TweenMode.Oneshot, duration, Ease.SineOut, t =>
            {
                Orbit = Calc.LerpClamp(from2, next2, t.Eased);
            });
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
        public bool TryMoveTowards(Vector2 target, float maxSpeed, float exitDistance, float speedApproach, out float distTo, out Vector2 speedAngle)
        {
            speedAngle = default;
            distTo = Vector2.DistanceSquared(Position, target);
            if (distTo < exitDistance * exitDistance)
            {
                return true;
            }
            speedAngle = Vector2.Normalize(target - Position) * maxSpeed;
            PathSpeed = Calc.Approach(PathSpeed, speedAngle, speedApproach * Engine.DeltaTime);
            return false;
        }
    }
}