using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Ascwiit;
using static Celeste.Mod.PuzzleIslandHelper.PianoModuleSession;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{

    [CustomEntity("PuzzleIslandHelper/Ascwiit")]
    [Tracked]
    public class Ascwiit : Actor
    {
        public class Flicker : Component
        {
            public float Amount;
            public float Interval;
            public float Max;
            public float Rate;
            private float target;
            private float lerp;
            private float from;

            public Flicker(float interval, float max, float rate) : base(true, true)
            {
                Interval = interval;
                Max = max;
                Rate = rate;
            }
            public override void Update()
            {
                base.Update();
                if (Scene.OnInterval(Interval))
                {
                    target = Calc.Random.Range(0, Max);
                    from = Amount;
                    lerp = 0;
                }
                Amount = Calc.LerpClamp(from, target, lerp);
                lerp = Calc.Min(lerp + Engine.DeltaTime * Rate, 1);
            }
        }

        [Tracked]
        [CustomEntity("PuzzleIslandHelper/AscwiitPath")]
        public class Path : Entity
        {
            public class Node : Entity
            {
                public float SpeedMult = 1;
                public bool Targeted;
                public Node(Vector2 position, float radius, float speedMult = 1) : base(position)
                {
                    SpeedMult = speedMult;
                    Collider = new Circle(radius);
                }
                public override void DebugRender(Camera camera)
                {
                    (Collider as Circle).Render(camera, Targeted ? Color.Red : Color.Yellow);
                }
            }
            public Node[] Nodes;
            public int TotalNodes;
            public bool NaiveFly;
            public string ID;
            public bool RemoveBirdAtEnd;
            public FlagList FlagOnBirdRemoved;
            public Path(EntityData data, Vector2 offset) : base(data.Position + offset + Vector2.One * 4)
            {
                Collider = new Hitbox(8, 8, -4, -4);
                float radius = data.Float("nodeRadius", 24);
                float speedMult = data.Float("speedMult", 1);
                ID = data.Attr("pathID");
                NaiveFly = data.Bool("naiveFly");
                RemoveBirdAtEnd = data.Bool("removeBirdAtEnd");
                Nodes = [.. data.NodesWithPosition(offset).Select(item => new Node(item + Vector2.One * 4, radius, speedMult))];
                FlagOnBirdRemoved = data.FlagList("flagOnBirdRemoved");
                TotalNodes = Nodes.Length;

            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                Scene.Add(Nodes);
            }
            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                scene.Remove(Nodes);
            }
            public override void DebugRender(Camera camera)
            {
                base.DebugRender(camera);
                for (int i = 1; i < Nodes.Length; i++)
                {
                    Draw.Line(Nodes[i - 1].Position, Nodes[i].Position, Color.Lerp(Color.Cyan, Color.Orange, i / (float)(Nodes.Length - 1)));
                }
                if (!RemoveBirdAtEnd)
                {
                    Draw.Line(Nodes[^1].Position, Nodes[0].Position, Color.Cyan);
                }
            }
        }

        [Tracked]
        [CustomEntity("PuzzleIslandHelper/AscwiitNoHopZone")]
        public class NoHopZone : Trigger
        {
            public NoHopZone(EntityData data, Vector2 offset) : base(data, offset)
            {
            }
        }

        [Tracked]
        [CustomEntity("PuzzleIslandHelper/AscwiitController")]
        public class Controller : Entity
        {
            [CustomEntity("PuzzleIslandHelper/AscwiitSequence")]
            [Tracked]
            public class Sequence : Entity
            {
                public class Data
                {
                    public string Group;
                    public int[] Steps;
                    public Vector2 PositionInRoom;
                    public string Room;
                }
                public Vector2? Node;
                public string Group;
                public int[] Steps;
                public int Step => SceneAs<Level>().Session.GetCounter("AscwiitSequence:" + Group);
                public bool LastDirectionRight = true;
                public Sequence(EntityData data, Vector2 offset) : base(data.Position + offset)
                {
                    Collider = new Hitbox(32, 32);
                    Group = data.Attr("groupID");
                    string[] steps = data.Attr("steps").Replace(" ", "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    List<int> stepsList = [];
                    foreach (string s in steps)
                    {
                        if (int.TryParse(s, out int result))
                        {
                            stepsList.Add(result);
                        }
                    }
                    Steps = stepsList.ToArray();
                    Vector2[] nodes = data.NodesOffset(offset);
                    Node = nodes.Length > 0 ? nodes[0] : null;
                    LastDirectionRight = data.Bool("lastDirectionRight");
                }
            }
            public bool RemoveAscwiitsIfWrongWay;
            public bool RemoveAscwiitsIfDisabled;
            public FlagList DisableFlag;
            public Sequence.Data Data;
            public int Direction;
            public string Group;
            public static bool Incorrect;
            public string Flag => "AscwiitSequence:" + Group;
            public int Step => SceneAs<Level>().Session.GetCounter(Flag);
            public bool Finished => SceneAs<Level>().Session.GetFlag("AscwiitSequenceFinished:" + Group);
            public bool Started => SceneAs<Level>().Session.GetFlag("AscwiitSequenceStarted:" + Group);
            public Controller(EntityData data, Vector2 offset) : base(data.Position + offset)
            {
                Group = data.Attr("groupID");
                Collider = new Hitbox(32, 32);
                DisableFlag = data.FlagList("disableFlag");
                DisableFlag.Inverted = true;
                Tag |= Tags.TransitionUpdate;
            }
            public override void Added(Scene scene)
            {
                base.Added(scene);
                foreach (var d in PianoModule.Session.AscwiitSequences)
                {
                    if (d.Group == Group)
                    {
                        Data = d;
                        break;
                    }
                }
                if (Data == null) RemoveSelf();
            }
            public override void Awake(Scene scene)
            {
                base.Awake(scene);

                if (Finished || (!Started && scene.Tracker.GetEntity<Sequence>() == null) || (Incorrect && RemoveAscwiitsIfWrongWay))
                {
                    RemoveAllNotFleeing(scene);
                }
                Incorrect = false;
                if (!DisableFlag)
                {
                    Decide();
                }
                else
                {
                    if (RemoveAscwiitsIfDisabled)
                    {
                        RemoveAllNotFleeing(scene);
                    }
                    RemoveSelf();
                }
            }
            public void RemoveAllNotFleeing(Scene scene)
            {
                foreach (Ascwiit a in scene.Tracker.GetEntities<Ascwiit>())
                {
                    if (!a.Fleeing)
                    {
                        a.RemoveSelf();
                    }
                }
            }
            public void Decide()
            {
                Level level = SceneAs<Level>();
                Session session = level.Session;
                if (session.GetFlag(Flag))
                {
                    if (level.Tracker.GetEntity<Sequence>() is var entity)
                    {
                        Direction = entity.LastDirectionRight ? 1 : -1;
                    }
                    else
                    {
                        Vector2 roomPosition = level.LevelOffset;
                        if (session.MapData.Get(Data.Room) is var data)
                        {
                            Direction = data.Position.X > roomPosition.X ? 1 : -1;
                        }
                    }
                }
                else
                {
                    int counter = session.GetCounter(Flag);
                    if (Data.Steps.Length > counter)
                    {
                        Direction = Data.Steps[counter];
                    }
                    else
                    {
                        RemoveSelf();
                    }
                }
            }
            private static void Level_OnTransitionTo(Level level, LevelData next, Vector2 direction)
            {
                foreach (Controller c in level.Tracker.GetEntities<Controller>())
                {
                    c.OnTransition(level, Math.Sign(direction.X));
                }
            }
            public void OnTransition(Level level, int xDir)
            {
                Session s = level.Session;
                if (xDir != 0 && xDir == Direction)
                {
                    s.IncrementCounter(Flag);
                    int count = s.GetCounter(Flag);
                    if (count >= Data.Steps.Length)
                    {
                        s.SetFlag(Flag);
                    }
                }
                else
                {
                    if (Started)
                    {
                        Incorrect = true;
                        s.SetFlag("AscwiitSequenceStarted:" + Group, false);
                    }
                    s.SetCounter(Flag, 0);
                }
            }
            [OnLoad]
            public static void Load()
            {
                Everest.Events.Level.OnTransitionTo += Level_OnTransitionTo;
            }
            [OnUnload]
            public static void Unload()
            {
                Everest.Events.Level.OnTransitionTo -= Level_OnTransitionTo;
            }
        }

        [Command("spawn_ascwiit", "")]
        public static void SpawnAscwiit(int state, string marker)
        {
            if (Engine.Scene is Level level)
            {
                Ascwiit bird = new Ascwiit(Vector2.Zero, state);
                if (!string.IsNullOrEmpty(marker))
                {
                    string markername = marker.Trim();
                    if (Marker.TryFind(markername, out Vector2 pos))
                    {
                        bird.Position = pos;
                    }
                }

                if (level.GetPlayer() is Player player)
                {
                    if (bird.Position == Vector2.Zero)
                    {
                        bird.Position = player.Center;
                    }
                }
                level.Add(bird);
            }
        }
        public StateMachine StateMachine;
        public int StartingState = 0;
        public const int StIdle = 0;
        public const int StFlee = 1;
        public const int StPath = 2;
        public const int StFlyTo = 3;
        public const int StDummy = 4;
        public Tween colorTween;
        public float colorLerp;
        public static readonly Vector2[] WingPoints = new Vector2[] { new(0, 0), new(0, 2), new(2, 0) };
        public static readonly Vector2[] BodyPoints = new Vector2[] { new(0, 0), new(1, 0), new(2, 0), new(3, 0), new(1, 1), new(2, 1), new(3, 1), new(4, 1) };
        public static readonly int[] WingIndices = new int[] { 0, 1, 2 };
        public static readonly int[] BodyIndices = new int[] { 0, 1, 4, 4, 1, 5, 1, 2, 5, 2, 3, 6, 5, 2, 6, 6, 3, 7 };

        public VertexPositionColor[] WingVertices;
        public VertexPositionColor[] BodyVertices;
        public List<float> BodyAlphas = new();
        public List<Color> BodyColors = new();
        public bool IdleHops = true;
        public Flicker[] Flickers = new Flicker[6];
        public readonly Vector2 BodyOffset = Vector2.Zero;
        public int BirdHeight => (int)(4 * Scale);
        public int BirdWidth => (int)(8 * Scale);
        public float Scale = 1;
        public const float NormalGravity = 900f;
        public bool DisableGravity;
        public bool DisableFriction;
        public bool AutoFlap;
        public float Gravity
        {
            get
            {
                if (DisableGravity || (StateMachine.State == StDummy && !DummyGravity))
                {
                    return 0;
                }
                return gravity;
            }
            set => gravity = value;
        }
        private float gravity = NormalGravity;
        public Facings Facing = Facings.Right;
        public Vector2 WingOffset;
        public Vector2 Speed;
        public bool Persistent;
        public bool OnlyCheckFlagOnAdded;
        public const float HopSpeedX = 110f;
        public const float HopSpeedY = -80f;
        public enum FleeFacings
        {
            Default,
            Unchanged,
            Random,
            Left,
            Right
        }
        private enum WingStates
        {
            Up = -1,
            AtRest = 0,
            Down = 1
        }
        public bool UseSequenceDirection;
        public FleeFacings FleeFacing;
        public FlagList Flag;
        private float peckTimer;
        private float flapTimer;
        private float chirpTimer;
        private bool idleFlapping;
        public float MinX;
        public float MaxX;
        public float MinY;
        public float FleeSpeedX;
        public float FleeSpeedY;
        public bool OnScreen;
        public bool FirfilEated;
        public float ScaredXOffset;
        private WingStates WingState;
        private Hitbox DefaultHitbox, DetectHitbox;
        public bool Fleeing => State == StFlee;
        public bool Idle => State == StIdle;
        public int State => StateMachine.State;
        private const float DetectXRange = 20f;
        private const float DetectYRange = 16f;

        public SoundSource tweetingSfx;
        private float hopTimer;
        public Vector2 Friction
        {
            get
            {
                if (DisableFriction || (StateMachine.State == StDummy && !DummyFriction))
                {
                    return Vector2.Zero;
                }
                return _friction;
            }
            set => _friction = value;
        }
        private Vector2 _friction = Vector2.One;
        public const float FlyingFrictionMult = 0.1f;
        private float fleeSpeedLerp = 0;
        private float fleeXAmount;

        public bool IgnoreSolids;
        public bool Naive => IgnoreSolids || (Fleeing && !onGround) || (State == StPath && FollowPath != null && FollowPath.NaiveFly) || State == StFlee;
        private bool onGround;
        private float ColorLerp;
        private float firstPeckTimer;
        public EntityID id;
        private float whiteLerp, whiteLerpTarget, whiteLerpSpeed;
        public bool FleesFromPlayer = true;
        public FlagList PathFlag;
        public FlagList FirfilFlag;
        public string PathID
        {
            get => _pathID;
            set
            {
                _pathID = value;
                Path prevPath = FollowPath;
                RefreshPath();
                if (FollowPath != prevPath)
                {
                    PathIndex = 0;
                    PathNode = null;
                }
            }
        }
        private string _pathID;
        public Path FollowPath;
        public Path.Node PathNode;
        public Vector2 PathNodeOffset;
        private float randSinOffset;
        private CritterLight CritterLight;
        private VertexLight Light;
        public bool AtFlyToTarget;
        public bool Scared
        {
            get => _scared && !FirfilEated;
            set => _scared = value;
        }
        private bool _scared;
        private float scaredTimer;
        public Wiggler ScaredWiggler;
        public bool AvoidNoHopZones;
        public bool StartDummy;
        public bool DummyPeck;
        public bool DummyChirp;
        public bool DummyFacePlayer;
        public bool CanPeck => !(!canPeck || (StateMachine.State == StDummy && !DummyPeck));
        public bool CanChirp => !(!canChirp || (StateMachine.State == StDummy && !DummyChirp));
        private bool canPeck = true;
        private bool canChirp = true;
        private bool canHop = true;
        public FlagList FlagOnEatSap;
        private Vector2 flyTo;
        private Action<Ascwiit> onFlyToEnd;
        private float flapSpeed;
        private Coroutine squawkCoroutine;
        private Player fleeingFrom;
        public float OffscreenFleeTimer;
        public int PathIndex;
        public Vector2 DebugOffset;
        public float MaxFlySpeedY = 50f;
        private float sinSpeed;
        public float NoFlapTimer;
        public bool ReturnToPreviousStateAtArrival;
        private int? stateBeforeFlyTo;
        public bool DummyGravity;
        public bool DummyFlap;
        public bool DummyFriction;
        public bool Hopping;
        public bool SnapToGround;
        public float FlyingStability = 0;
        public Firfil.Flicker FirfilFlicker = Firfil.Flicker.Default;
        private List<HopData> hopDatas = [];
        public enum HopResult
        {
            Success,
            OutOfBounds,
            HitWall,
            HitNoHopZone
        }
        private static string[] stateNames = ["Idle", "Flee", "Path", "FlyTo", "Dummy"];
        private static int getStateIndex(string name)
        {
            if (stateNames.Contains(name))
            {
                return Array.IndexOf(stateNames, name);
            }
            return StIdle;
        }
        public Ascwiit(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, getStateIndex(data.Attr("startingState", "Idle")), data.Float("scale", 1))
        {
            FlyingStability = data.Float("flyingStability");
            SnapToGround = data.Bool("snapToGround", true);
            Flag = data.FlagList("flag");
            FirfilFlag = data.FlagList("firfilFlag");
            FlagOnEatSap = data.FlagList("sapFlag");
            PathFlag = data.FlagList("pathFlag");
            _pathID = data.Attr("pathID");
            FleeFacing = data.Enum<FleeFacings>("flyFacing");
            FirfilEated = data.Bool("eatedFirfil");
            canPeck = data.Bool("peck", true);
            canChirp = data.Bool("chirp", true);
            canHop = data.Bool("hop", true);
            DummyFacePlayer = data.Bool("dummyFacePlayer");
            FleesFromPlayer = data.Bool("fleesFromPlayer", true);
            UseSequenceDirection = data.Bool("useSequenceDirection");
            Persistent = data.Bool("persistent");
            OnlyCheckFlagOnAdded = data.Bool("onlyFlagOnAdded");
            _scared = data.Bool("scared");
            AvoidNoHopZones = data.Bool("avoidNoHopZones");
            this.id = id;
        }
        public Ascwiit(Vector2 position) : this(position, StIdle) { }
        public Ascwiit(Vector2 position, int state, float scale = 1) : base(position)
        {
            colorTween = Tween.Set(this, Tween.TweenMode.Looping, Firfil.Flicker.Default.Interval * 2, Ease.SineInOut, t => colorLerp = t.Eased);
            Scale = scale;
            Depth = 1;
            WingVertices = PianoUtils.Initialize((VertexPositionColor)default, WingPoints.Length);
            BodyVertices = PianoUtils.Initialize((VertexPositionColor)default, BodyPoints.Length);
            DefaultHitbox = new Hitbox(BirdWidth, BirdHeight);
            DetectHitbox = new Hitbox(DetectXRange * 2, DetectYRange * 2, -DetectXRange + BirdWidth / 2, -DetectYRange + BirdHeight / 2);
            Collider = DefaultHitbox;
            WingOffset = new Vector2(BirdWidth / 3f, Height / 2f);
            for (int i = 0; i < 6; i++)
            {
                Flickers[i] = new Flicker(0.5f, 0.6f, 3);
            }
            IgnoreJumpThrus = true;
            Add(Flickers);
            AddTag(Tags.TransitionUpdate);
            Add(CritterLight = new CritterLight(32, null, FirfilEated));
            Add(ScaredWiggler = Wiggler.Create(0.4f, 7, (f) => { ScaredXOffset = f * 2f; }));
            ScaredWiggler.StartZero = true;
            StateMachine = new StateMachine();
            StateMachine.SetCallbacks(StIdle, IdleUpdate, null, IdleBegin, IdleEnd);
            StateMachine.SetCallbacks(StFlee, FleeingUpdate, null, FleeBegin, FleeEnd);
            StateMachine.SetCallbacks(StPath, PathUpdate, null, PathBegin);
            StateMachine.SetCallbacks(StFlyTo, FlyToUpdate, null, flyToBegin);
            StateMachine.SetCallbacks(StDummy, DummyUpdate, null, DummyBegin, DummyEnd);
            Add(StateMachine);
            Tag |= Tags.TransitionUpdate;
            StartingState = state;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!Flag)
            {
                RemoveSelf();
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (PianoModule.Session.AscwiitsWithFirfils.Contains(id))
            {
                FirfilEated = true;
            }
            hopTimer = Calc.Random.Range(0.7f, 4);
            randSinOffset = Calc.Random.Range(0f, 10f);
            scaredTimer = Calc.Random.Range(1f, 3f);
            for (int i = 0; i < BodyPoints.Length + 3; i += 3)
            {
                BodyAlphas.Add(Calc.Random.Range(0.4f, 0.8f));
                BodyColors.Add(Calc.Random.Choose(Color.LightGreen, Color.ForestGreen));
            }
            firstPeckTimer = Calc.Random.Range(0, 10f);
            fleeXAmount = Calc.Random.Range(0.3f, 1f);
            FleeSpeedX = Calc.Random.Range(8f, 30f);
            FleeSpeedY = Calc.Random.Range(-6f, -2f);
            if (SnapToGround)
            {
                while (!OnGround())
                {
                    Position.Y++;
                    if (Position.Y > (scene as Level).Bounds.Bottom)
                    {
                        RemoveSelf();
                    }
                }
            }
            if (!string.IsNullOrEmpty(PathID))
            {
                RefreshPath();
            }
            StateMachine.State = StartingState;
        }
        public void RefreshPath()
        {
            foreach (Path path in Scene.Tracker.GetEntities<Path>())
            {
                if (path.ID == PathID)
                {
                    FollowPath = path;
                    return;
                }
            }
            FollowPath = null;
        }
        public override void Update()
        {
            if (!OnlyCheckFlagOnAdded && !Flag)
            {
                return;
            }
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            if (FollowPath == null && !string.IsNullOrEmpty(PathID))
            {
                RefreshPath();
            }
            CritterLight.Enabled = FirfilEated;
            if (Scene.OnInterval(0.3f))
            {
                Collider = DetectHitbox;
                OnScreen = this.OnScreen(16);
                Collider = DefaultHitbox;
                if (PathNode != null)
                {
                    PathNodeOffset = Calc.AngleToVector(Calc.Random.NextAngle(), PathNode.Width * 0.4f);
                }
            }
            if (whiteLerp != whiteLerpTarget)
            {
                whiteLerp = Calc.Approach(whiteLerp, whiteLerpTarget, whiteLerpSpeed * Engine.DeltaTime);
            }
            Speed = SimulateSpeedChange(Position, Speed, Friction, Gravity, out onGround, out bool skipWingUpdate);
            if (skipWingUpdate)
            {
                WingState = WingStates.Up;
            }
            if (Scared)
            {
                if (onGround && scaredTimer > 0)
                {
                    scaredTimer -= Engine.DeltaTime;
                    if (scaredTimer <= 0)
                    {
                        ScaredWiggler.Start();
                        scaredTimer = Calc.Random.Range(1f, 3f);
                    }
                }
            }
            else if (ScaredWiggler.Active)
            {
                ScaredWiggler.StopAndClear();
            }
            base.Update();
            if (Naive)
            {
                Position.X += Speed.X * Engine.DeltaTime;
                Position.Y += (Speed.Y + flapSpeed + sinSpeed * (1 - FlyingStability)) * Engine.DeltaTime;
            }
            else
            {
                MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
                MoveV((Speed.Y + flapSpeed + sinSpeed * (1 - FlyingStability)) * Engine.DeltaTime, OnCollideV);
            }

            if (!skipWingUpdate)
            {
                WingUpdate();
            }
            UpdateVertices();
            if (FirfilEated && Scene.OnInterval(Engine.DeltaTime * 4))
            {
                AfterImage image = AfterImage.Create(this, RenderZero, 2f, 0.3f, null, Depth + 1);
                image.ScaleOut = true;
            }
        }
        public void UpdateVertices()
        {
            Vector2 wingOffset = WingOffset;
            Vector2 scale = new Vector2(Width / 4f, Height).Floor();
            scale *= Calc.LerpClamp(1, Depth > 0 ? 0.3f : 2, ColorLerp);

            Vector2 beakPoint = BodyPoints[0];
            if (peckTimer > 0)
            {
                BodyPoints[0].Y += 1;
            }
            Color to = Depth > 0 ? Color.DarkGreen : Color.LightGreen;
            for (int i = 0; i < BodyPoints.Length; i += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (BodyPoints.Length <= i + j) break;
                    Color color = Color.Lerp(BodyColors[i / 3], Color.Green, BodyAlphas[i / 3]);
                    if (Flickers.Length > i / 3)
                    {
                        BodyVertices[i + j].Color = Color.Lerp(Color.Lerp(color, to, ColorLerp), Color.Black, Flickers[i / 3].Amount / 1.2f);
                    }
                    else
                    {
                        BodyVertices[i + j].Color = Color.Lerp(color, to, ColorLerp);
                    }
                    Vector2 point = BodyPoints[i + j] * scale;
                    if (Facing is Facings.Right)
                    {
                        point.X = (point.X * -1) + Width;
                    }
                    point.X += ScaredXOffset;
                    BodyVertices[i + j].Position = new Vector3(Position + point, 0);
                    if (whiteLerp > 0)
                    {
                        BodyVertices[i + j].Color = Color.Lerp(BodyVertices[i + j].Color, Color.White, whiteLerp);
                    }
                }
            }
            BodyPoints[0] = beakPoint;
            scale.Y *= WingState == 0 ? 0.3f : (int)WingState;

            for (int i = 0; i < WingPoints.Length; i++)
            {
                if (whiteLerp > 0)
                {
                    WingVertices[i].Color = Color.Lerp(Color.Lerp(Color.LightGreen, to, ColorLerp), Color.White, whiteLerp);
                }
                else
                {
                    WingVertices[i].Color = Color.Lerp(Color.LightGreen, to, ColorLerp);
                }
                Vector2 point = wingOffset + (WingPoints[i] * scale);
                if (Facing is Facings.Right)
                {
                    point.X = (point.X * -1) + Width;
                }
                point.X += ScaredXOffset;
                WingVertices[i].Position = new Vector3(Position + point, 0);
            }
        }
        public override void Render()
        {
            base.Render();
            if (Scene is not Level level || (!OnlyCheckFlagOnAdded && !Flag) || (!OnScreen && !Fleeing)) return;
            Draw.SpriteBatch.End();
            GFX.DrawIndexedVertices(level.Camera.Matrix, BodyVertices, BodyVertices.Length, BodyIndices, 6);
            GFX.DrawIndexedVertices(level.Camera.Matrix, WingVertices, WingVertices.Length, WingIndices, 1);
            GameplayRenderer.Begin();
            if (FirfilEated)
            {
                Draw.Point(Center, Color.Lerp(FirfilFlicker.ColorA, FirfilFlicker.ColorB, colorLerp));
            }
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);

            if (DebugOffset != Vector2.Zero)
            {
                Draw.Line(Position, Position + DebugOffset, Color.White);
            }
            if (PathNode != null)
            {
                //PathNode.Collider.Render(camera, Color.Red);
                Draw.Point(PathNode.Position + PathNodeOffset, Color.White);
            }
            foreach (HopData data in hopDatas)
            {
                for (int i = 1; i < data.Points.Count; i++)
                {
                    Draw.Line(data.Points[i - 1].Item1, data.Points[i].Item1, data.Points[i - 1].Item2 * 0.5f);
                }
                for (int i = 0; i < data.Points.Count; i++)
                {
                    Draw.Point(data.Points[i].Item1, data.Points[i].Item2);
                }
            }
        }
        public void RenderZero()
        {
            Draw.SpriteBatch.End();
            OffsetVertices(-Position);
            GFX.DrawIndexedVertices(Matrix.Identity, BodyVertices, BodyVertices.Length, BodyIndices, 6);
            GFX.DrawIndexedVertices(Matrix.Identity, WingVertices, WingVertices.Length, WingIndices, 1);
            OffsetVertices(Position);
            Draw.SpriteBatch.Begin();
            if (FirfilEated)
            {
                Draw.Point(Collider.HalfSize, Color.Lerp(FirfilFlicker.ColorA, FirfilFlicker.ColorB, colorLerp));
            }
        }

        public void OffsetVertices(Vector2 offset)
        {
            for (int i = 0; i < BodyPoints.Length; i++)
            {
                BodyVertices[i].Position += new Vector3(offset, 0);
            }
            for (int i = 0; i < WingPoints.Length; i++)
            {
                WingVertices[i].Position += new Vector3(offset, 0);
            }
        }
        public bool FlyTo(Vector2 position, bool revertStateAtEnd = false, Action<Ascwiit> onEnd = null)
        {
            ReturnToPreviousStateAtArrival = revertStateAtEnd;
            if (!revertStateAtEnd)
            {
                stateBeforeFlyTo = null;
            }
            else
            {
                stateBeforeFlyTo = State;
            }
            onFlyToEnd = onEnd;
            if (StateMachine.State == StFlyTo && flyTo == position)
            {
                return false;
            }
            AtFlyToTarget = false;
            flyTo = position;
            StateMachine.State = StFlyTo;
            return true;
        }
        public bool FlyTo(Vector2 position, int? stateAtEnd, Action<Ascwiit> onEnd = null)
        {
            onFlyToEnd = onEnd;
            if (StateMachine.State == StFlyTo && flyTo == position && stateBeforeFlyTo == stateAtEnd)
            {
                return false;
            }
            onFlyToEnd = onEnd;
            AtFlyToTarget = false;
            stateBeforeFlyTo = stateAtEnd;
            flyTo = position;
            StateMachine.State = StFlyTo;
            return true;
        }
        public IEnumerator FlyToRoutine(Vector2 position, bool revertStateAtEnd = false, Action<Ascwiit> onEnd = null)
        {

            if (FlyTo(position, revertStateAtEnd, onEnd))
            {
                while (!AtFlyToTarget)
                {
                    yield return null;
                }
            }
        }
        public IEnumerator FlyToRoutine(Vector2 position, int? stateAtEnd, Action<Ascwiit> onEnd = null)
        {
            if (FlyTo(position, stateAtEnd, onEnd))
            {
                while (!AtFlyToTarget)
                {
                    yield return null;
                }
            }
        }
        public IEnumerator EatSap(Statid statid)
        {
            Depth = -100000;
            Vector2 position = Position;
            IgnoreSolids = true;
            yield return FlyToRoutine(statid.Center);
            Gravity = 0;
            Speed.Y = 0;
            Speed.X = 0;
            yield return 1f;
            Scared = false;
            statid.HasSap = false;
            statid.IsSapped = true;
            FirfilEated = true;
            PianoModule.Session.AscwiitsWithFirfils.Add(id);
            Gravity = NormalGravity;
        }
        public Vector2 SimulateSpeedChange(Vector2 position, Vector2 inputSpeed, Vector2 friction, float gravity, out bool onGround, out bool floating)
        {
            floating = false;
            float yMoveMult = (Math.Abs(inputSpeed.Y) < 40f ? 0.5f : 1f) * friction.Y;
            onGround = OnGround(position);
            if (!onGround)
            {
                if (gravity != 0 && yMoveMult != 0)
                {
                    if (inputSpeed.Y > 100f && !CollideCheck<Solid>(position + Vector2.UnitY * 20))
                    {
                        if (State == StIdle)
                        {
                            gravity *= 0.5f;
                            floating = true;
                        }
                    }
                    inputSpeed.Y = Calc.Approach(inputSpeed.Y, 160f, gravity * yMoveMult * Engine.DeltaTime);
                }
            }
            if (State != StIdle)
            {
                if (Math.Abs(inputSpeed.X) > 90f)
                {
                    inputSpeed.X = Calc.Approach(inputSpeed.X, 90f * Math.Sign(inputSpeed.X), 2500f * Engine.DeltaTime);
                }
            }
            if (friction.X != 0)
            {
                float frictionX = 1000f;
                if (!onGround)
                {
                    frictionX *= 0.5f;
                }
                inputSpeed.X = Calc.Approach(inputSpeed.X, 0f, (frictionX * Engine.DeltaTime) * friction.X);
            }
            return inputSpeed;
        }
        public void WingUpdate()
        {
            if (Speed.Y < 0)
            {
                if (WingState == WingStates.AtRest)
                {
                    WingState = WingStates.Down;
                    flapTimer = 0;
                }
                flapTimer -= Engine.DeltaTime;
                if (flapTimer < 0)
                {
                    flapTimer = 0.21f - (fleeSpeedLerp * 0.15f);
                    fleeSpeedLerp = Calc.Min(fleeSpeedLerp + Engine.DeltaTime * 2, 1);
                    WingState = (WingStates)(-(int)WingState);
                }
            }
            else if (!OnGround())
            {
                flapTimer -= Engine.DeltaTime;
                if (flapTimer < 0)
                {
                    if (StateMachine.State != StDummy || DummyGravity)
                    {
                        if (Speed.Y > 0)
                        {
                            WingState = WingStates.Up;
                            return;
                        }
                        if (WingState == WingStates.Up)
                        {
                            flapSpeed -= 60f * Engine.DeltaTime;
                        }
                    }
                    flapTimer = 0.1f;
                    WingState = (WingStates)(-(int)WingState);
                }
            }
            else if (!idleFlapping || (StateMachine.State == StDummy && !DummyFlap))
            {
                WingState = WingStates.AtRest;
                flapTimer = 0;
            }
        }
        public void OnCollideV(CollisionData data)
        {
            Speed.Y = 0;
            if (Hopping)
            {
                if (data.Direction.Y == 1)
                {
                    _friction.X = 1;
                }
            }
        }
        public void IdleEnd()
        {
            Hopping = false;
        }
        public void OnCollideH(CollisionData data)
        {
            if (data.Hit is Solid)
            {
                if (Math.Abs(Speed.X) > 20f)
                {
                    Facing = (Facings)(-(int)Facing);
                    Speed.X *= -1;
                }
                else
                {
                    Speed.X = 0;
                }
            }
        }
        public void Chirp()
        {
            chirpTimer = 0.4f;
            //play chirp sound
        }
        public void Squawk()
        {
            squawkCoroutine?.Cancel();
            Add(squawkCoroutine = new Coroutine(squawkRoutine()));
            Pulse.Diamond(this, Pulse.Fade.Late, Pulse.Mode.Oneshot, Collider.HalfSize, Collider.Height / 2, 16, 1, true, Color.White, Color.Transparent, Ease.Linear, Ease.SineOut);
        }
        private IEnumerator squawkRoutine()
        {
            Audio.Play("event:/game/general/bird_squawk", Position);
            whiteLerpTarget = 1;
            whiteLerpSpeed = 15f;
            yield return 0.7f;
            //squawkImage.Visible = true;
            whiteLerpTarget = 0;
            whiteLerpSpeed = 3f;
            yield return 1f;
            //squawkImage.Visible = false;
        }
        public void Peck()
        {
            peckTimer = 0.1f;
        }
        public void FleeFromPlayer(Player player)
        {
            Flee(player);
        }
        public void Flee(Player player = null)
        {
            AddTag(Tags.Persistent);
            fleeingFrom = player;
            StateMachine.State = StFlee;
            Collider = DetectHitbox;
            foreach (Ascwiit bird in Scene.Tracker.GetEntities<Ascwiit>())
            {
                if (bird != this && !bird.Fleeing)
                {
                    if (bird.CollideCheck(this))
                    {
                        bird.WaitThenFlee(player);
                    }
                }
            }
            Collider = DefaultHitbox;

        }
        public void WaitThenFlee(Player player = null)
        {
            Alarm.Set(this, Calc.Random.Range(0.1f, 0.2f), () => Flee(player));
        }
        public bool Hop()
        {
            hopTimer = Calc.Random.Range(0.6f, 4);
            float hopSpeedXMult = Calc.Random.Range(0.7f, 1f);
            float hopSpeedYMult = Calc.Random.Range(0.7f, 1f);
            int dir = Calc.Random.Choose(-1, 1);
            Rectangle bounds = SceneAs<Level>().Bounds;
            Vector2 hopFriction = new Vector2(0.5f, 0.95f);
            float origSpeedX, origSpeedY;
            hopDatas.Clear();
            //ensure that the ascwiit's hop won't take it out of bounds or into a pit of some kind.
            //calculate 6 different paths the hop could take the ascwiit, then compare the positions and choose the best one

            for (int j = 0; j < 2; j++)
            {
                origSpeedX = (HopSpeedX + HopSpeedX * 0.5f * j) * dir * hopSpeedXMult;
                for (int i = 0; i < 3; i++)
                {
                    origSpeedY = HopSpeedY * (i + 1) * hopSpeedYMult;
                    Vector2 origSpeed = new Vector2(origSpeedX, origSpeedY);
                    TryFindHopTarget(Position, origSpeed, hopFriction, Gravity, bounds, out HopData data);
                    if (data.Result == HopResult.Success)
                    {
                        data.Facing = (Facings)dir;
                        hopDatas.Add(data);
                    }
                }
            }

            if (hopDatas.Count == 0) return false;
            HopData best = hopDatas[0];
            foreach (var d in hopDatas)
            {
                if (d.Collision.Y < Y)
                {
                    best = d;
                    break;
                }
            }
            AdoptHop(best);
            return true;
        }
        public void AdoptHop(HopData data)
        {
            Speed = data.FinalSpeed;
            Friction = data.FinalFriction;
            Facing = data.Facing;
        }
        public struct HopData
        {
            public HopResult Result;
            public Vector2 FinalSpeed;
            public Vector2 FinalFriction;
            public Facings Facing;
            public Vector2 Collision;
            public List<(Vector2, Color)> Points = [];
            public HopData() { }
        }
        public bool TryFindHopTarget(Vector2 from, Vector2 speed, Vector2 friction, float gravity, Rectangle bounds, out HopData hopData)
        {
            bool onNewGround = false;
            Vector2 origSpeed = speed;
            hopData = new();
            hopData.Points.Add((from, Color.White));
            bool hitWall = false;
            while (true)
            {
                speed = SimulateSpeedChange(Position, speed, friction, gravity, out _, out _);
                MoveH(speed.X * Engine.DeltaTime, (CollisionData data) =>
                {
                    hitWall = true;
                });
                if (hitWall)
                {
                    hopData.Collision = Position;
                    hopData.Result = HopResult.HitWall;
                    hopData.Points.Add((Position, Color.Red));
                    Position = from;
                    return false;
                }
                MoveV(speed.Y * Engine.DeltaTime, (CollisionData data) =>
                {
                    if (data.Direction.Y > 0)
                    {
                        onNewGround = true;
                    }
                });
                if (Top > bounds.Bottom || Right < bounds.Left || Left > bounds.Right)
                {
                    hopData.Points.Add((Position, Color.Red));
                    hopData.Result = HopResult.OutOfBounds;
                    Position = from;
                    return false;
                }
                if (CollideCheck<NoHopZone>())
                {
                    hopData.Points.Add((Position, Color.Red));
                    hopData.Collision = Position;
                    hopData.Result = HopResult.HitNoHopZone;
                    Position = from;
                    return false;
                }
                if (onNewGround)
                {
                    hopData.Points.Add((Position, Color.Lime));
                    hopData.Collision = Position;
                    hopData.FinalSpeed = origSpeed;
                    hopData.FinalFriction = friction;
                    Position = from;
                    return true;
                }
                hopData.Points.Add((Position, Color.White));
            }
        }
        public int FleeingUpdate()
        {
            sinSpeed = 0;
            if (NoFlapTimer > 0)
            {
                NoFlapTimer -= Engine.DeltaTime;
                return StFlee;
            }
            if (fleeSpeedLerp > 0.5f)
            {
                ColorLerp += Engine.DeltaTime;
            }
            fleeSpeedLerp = Calc.Min(fleeSpeedLerp + Engine.DeltaTime, 1);

            Friction = Vector2.One * FlyingFrictionMult;
            Speed.Y += FleeSpeedY * fleeSpeedLerp;
            Speed.X += FleeSpeedX * (int)Facing * fleeSpeedLerp * fleeXAmount;

            Collider c = Collider;
            Collider = DetectHitbox;
            if (!this.OnScreen(8))
            {
                OffscreenFleeTimer += Engine.DeltaTime;
            }
            else
            {
                OffscreenFleeTimer = 0;
            }
            Collider = c;
            if (OffscreenFleeTimer > 1.8f)
            {
                if (Persistent)
                {
                    SceneAs<Level>().Session.DoNotLoad.Add(id);
                }
                RemoveSelf();
            }
            return StFlee;

        }
        public void IdleFlap()
        {
            Add(new Coroutine(IdleFlapRoutine()));
        }
        private IEnumerator IdleFlapRoutine()
        {
            if (idleFlapping) yield break;
            idleFlapping = true;
            WingState = WingStates.Up;
            for (int i = 0; i < Calc.Random.Choose(4, 6); i++)
            {
                WingState = (WingStates)(-(int)WingState);
                yield return 0.05f;
            }
            if (!Fleeing)
            {
                WingState = WingStates.AtRest;
            }
            idleFlapping = false;
        }
        protected void flyToBegin()
        {
            AtFlyToTarget = false;
            PathNodeOffset = Vector2.Zero;
            PathNode = null;
            idleFlapping = false;
            Friction = Vector2.One * FlyingFrictionMult;
            if (OnGround())
            {
                SceneAs<Level>().ParticlesFG.Emit(Calc.Random.Choose(ParticleTypes.Dust), BottomCenter, -(float)Math.PI / 2f);
                WingState = WingStates.Up;
            }
        }
        public int FlyToUpdate()
        {
            if (NoFlapTimer > 0)
            {
                NoFlapTimer -= Engine.DeltaTime;
                return StFlyTo;
            }
            sinSpeed = Calc.Approach(sinSpeed, (float)Math.Sin(Scene.TimeActive + randSinOffset) * 20f, 40f * Engine.DeltaTime);
            if (FlyApproach(flyTo, MaxFlySpeedY, 2f))
            {
                Facing = (Facings)Math.Sign(flyTo.X - CenterX);
            }
            else
            {
                AtFlyToTarget = true;
                onFlyToEnd?.Invoke(this);
                if (ReturnToPreviousStateAtArrival)
                {
                    if (stateBeforeFlyTo.HasValue && stateBeforeFlyTo.Value != StateMachine.State)
                    {
                        return stateBeforeFlyTo.Value;
                    }
                }
            }
            return StFlyTo;

        }
        public void FleeBegin()
        {
            switch (FleeFacing)
            {
                case FleeFacings.Default:
                    if (fleeingFrom != null)
                    {
                        Facing = (Facings)(-Math.Sign(fleeingFrom.X - CenterX));
                    }
                    break;
                case FleeFacings.Random:
                    Facing = Calc.Random.Choose(Facings.Left, Facings.Right);
                    break;
                case FleeFacings.Left:
                    Facing = Facings.Left;
                    break;
                case FleeFacings.Right:
                    Facing = Facings.Right;
                    break;
            }
            if (UseSequenceDirection)
            {
                Controller controller = Scene.Tracker.GetEntity<Controller>();
                if (controller != null && controller.Direction != 0)
                {
                    Facing = (Facings)controller.Direction;
                }
                Controller.Sequence sequence = Scene.Tracker.GetEntity<Controller.Sequence>();
                if (sequence != null)
                {
                    SceneAs<Level>().Session.SetFlag("AscwiitSequenceStarted:" + sequence.Group);
                }
            }
            SceneAs<Level>().ParticlesFG.Emit(Calc.Random.Choose(ParticleTypes.Dust), BottomCenter, -(float)Math.PI / 2f);
            idleFlapping = false;
            fleeSpeedLerp = 0;
            Speed.Y -= 70f;
            NoFlapTimer = Calc.Random.Range(0.2f, 0.67f);
            Speed.X += -(int)Facing * 40f;
            Friction = Vector2.One * FlyingFrictionMult;
            WingState = WingStates.Up;
            Alarm flapAlarm = Alarm.Set(this, 0.1f, () =>
            {
                WingState = WingStates.Down;
                Alarm.Set(this, 0.1f, () =>
                {
                    WingState = WingStates.Up;
                });
            });
            if (Calc.Random.Chance(0.5f))
            {
                Depth = -10001;
            }
        }
        public void FleeEnd()
        {
            ColorLerp = 0;
            fleeSpeedLerp = 0;
            fleeXAmount = 0;
            OffscreenFleeTimer = 0;
            sinSpeed = 0;
        }
        public void IdleBegin()
        {
            Friction = Vector2.One;
            IdleHops = true;
        }
        public int IdleUpdate()
        {
            sinSpeed = 0;
            firstPeckTimer = Calc.Max(firstPeckTimer - Engine.DeltaTime, 0);

            if (FleesFromPlayer && Scene.GetPlayer() is Player player)
            {
                Collider = DetectHitbox;
                bool collided = CollideCheck(player);
                Collider = DefaultHitbox;
                Vector2 start = Facing == Facings.Right ? TopRight : TopLeft;
                if ((Math.Abs(player.Speed.X) >= 90f || Math.Abs(player.Speed.Y) > 50f) && collided)
                {
                    bool abort = false;
                    foreach (Solid solid in Scene.Tracker.GetEntities<Solid>())
                    {
                        if (solid.CollideLine(start, player.Center))
                        {
                            abort = true;
                            break;
                        }
                    }

                    if (!abort)
                    {
                        return StFlee;
                    }
                }
            }

            if (FollowPath != null && PathFlag)
            {
                return StPath;
            }
            if (onGround)
            {
                if (Calc.Random.Chance(0.008f))
                {
                    IdleFlap();
                }
                if (chirpTimer > 0)
                {
                    chirpTimer -= Engine.DeltaTime;
                }
                else if (CanChirp && Calc.Random.Chance(0.005f))
                {
                    Chirp();
                }
                if (peckTimer > 0)
                {
                    peckTimer -= Engine.DeltaTime;
                }
                else if (CanPeck && Calc.Random.Chance(0.01f))
                {
                    Peck();
                }
                if (Scared || peckTimer > 0 || firstPeckTimer > 0)
                {
                    return StIdle;
                }

                if (hopTimer > 0)
                {
                    hopTimer -= Engine.DeltaTime;
                    if (hopTimer <= 0)
                    {
                        if (canHop && IdleHops && !Hop())
                        {
                            return StIdle;
                        }
                    }
                }
            }
            else
            {
                peckTimer = 0;
            }
            return StIdle;
        }
        public void PathBegin()
        {
            PathNodeOffset = Vector2.Zero;
            PathNode = null;
            PathIndex = 0;
            idleFlapping = false;
            Friction = Vector2.One * FlyingFrictionMult;
            if (OnGround())
            {
                SceneAs<Level>().ParticlesFG.Emit(Calc.Random.Choose(ParticleTypes.Dust), BottomCenter, -(float)Math.PI / 2f);
                Speed.Y -= 70f;
                NoFlapTimer = Calc.Random.Range(0.2f, 0.67f);
                Speed.X += -(int)Facing * 40f;
                WingState = WingStates.Up;
            }
        }
        public int PathUpdate()
        {
            if (NoFlapTimer > 0)
            {
                NoFlapTimer -= Engine.DeltaTime;
                return StPath;
            }
            PathNode = null;
            if (FollowPath != null && PathFlag)
            {
                if (PathIndex >= FollowPath.Nodes.Length)
                {
                    if (!FollowPath.RemoveBirdAtEnd) PathIndex = 0;
                    else
                    {
                        FollowPath.FlagOnBirdRemoved.State = true;
                        if (Persistent)
                        {
                            SceneAs<Level>().Session.DoNotLoad.Add(id);
                        }
                        RemoveSelf();
                        return StPath;
                    }
                }
                sinSpeed = Calc.Approach(sinSpeed, (float)Math.Sin(Scene.TimeActive + randSinOffset) * 20f, 40f * Engine.DeltaTime);
                foreach (Path.Node node in FollowPath.Nodes)
                {
                    node.Targeted = false;
                }
                PathNode = FollowPath.Nodes[PathIndex];
                PathNode.Targeted = true;
                Vector2 target = PathNode.Position + PathNodeOffset;

                if (!CollideCheck(PathNode))
                {
                    FlyApproach(target, MaxFlySpeedY);
                    Facing = (Facings)Math.Sign(PathNode.CenterX - CenterX);
                }
                else
                {
                    PathIndex++;
                }
            }
            else
            {
                return StIdle;
            }
            return StPath;
        }
        public void DummyBegin()
        {

            idleFlapping = false;
            DummyGravity = true;
            DummyFlap = true;
            DummyFriction = true;
            Friction = Vector2.One;
        }
        public int DummyUpdate()
        {
            if (DummyFacePlayer)
            {
                if (Scene.GetPlayer() is Player player)
                {
                    Facing = (Facings)Math.Sign(player.CenterX - CenterX);
                }
            }
            return StDummy;
        }
        public void DummyEnd()
        {
            DummyGravity = false;
            DummyFlap = false;
            DummyFriction = false;
            DummyPeck = false;
            DummyChirp = false;
        }
        public bool FlyApproach(Vector2 target, float maxYSpeed, float escapeDistance = -1)
        {
            float dist = Vector2.DistanceSquared(Center, target);
            if (escapeDistance >= 0 && dist <= escapeDistance * escapeDistance)
            {
                return false;
            }
            float angle = (target - Center).Angle();
            Vector2 offset = Calc.AngleToVector(angle, Math.Max(dist, 25));
            DebugOffset = offset * Engine.DeltaTime;
            float stabilityBuff = 500f * FlyingStability;
            Speed = Calc.Approach(Speed, offset, 500f * Engine.DeltaTime);
            if (Speed.Y > maxYSpeed)
            {
                Speed.Y = Calc.Approach(Speed.Y, maxYSpeed, 250f * Engine.DeltaTime);
                WingState = WingStates.Up;
            }
            else if (Speed.Y < -maxYSpeed)
            {
                Speed.Y = Calc.Approach(Speed.Y, -maxYSpeed, 250f * Engine.DeltaTime);
            }

            Facing = (Facings)Math.Sign(target.X - CenterX);
            return true;
        }
    }
}
