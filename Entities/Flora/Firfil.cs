using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.WARP;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [CustomEntity("PuzzleIslandHelper/Firfil")]
    [Tracked]
    public class Firfil : Actor
    {
        [Command("add_firfil", "Creates x firfils and makes them follow the player")]
        public static void CreateFirfils(int x)
        {
            if (Engine.Scene is not Level level)
            {
                Engine.Commands.Log("Current Scene is currently not a level.");
                return;
            }
            if (level.GetPlayer() is not Player player)
            {
                Engine.Commands.Log("Current Scene does not contain a player.");
                return;
            }
            for (int i = 0; i < x; i++)
            {
                Firfil f = new Firfil(player.Position, new(Guid.NewGuid().ToString(), 0));
                level.Add(f);
            }
        }

        public struct Flicker
        {
            public static readonly Flicker Danger = new Flicker(Color.Red, Color.Yellow, 0.08f);
            public static readonly Flicker Nest = new Flicker(Color.Lime, Color.Yellow);
            public static readonly Flicker Pollinate = new Flicker(Color.DarkGray, Color.White);
            public static readonly Flicker Follow = new Flicker(Color.Cyan, Color.MediumVioletRed);
            public static readonly Flicker Default = new Flicker(Color.Magenta, Color.Yellow);
            public Color ColorA;
            public Color ColorB;
            public float Interval;
            public float AfterImageTimer;
            public Flicker(Color a, Color b, float interval = 0.15f, float afterimageTimer = 0.15f)
            {
                ColorA = a;
                ColorB = b;
                Interval = interval;
                AfterImageTimer = afterimageTimer;
            }
            public override readonly bool Equals([NotNullWhen(true)] object obj)
            {
                if (obj is Flicker flicker)
                {
                    return !(flicker.ColorA != ColorA || flicker.ColorB != ColorB || flicker.Interval != Interval || flicker.AfterImageTimer != AfterImageTimer);
                }
                return base.Equals(obj);
            }
            public static bool operator !=(Flicker left, Flicker right)
            {
                return !(left == right);
            }
            public static bool operator ==(Flicker left, Flicker right)
            {
                return left.Equals(right);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
        public Flicker CurrentFlicker
        {
            get => currentFlicker;
            set
            {
                currentFlicker = value;
                colorTween.Duration = value.Interval * 2;
            }
        }
        private Flicker currentFlicker = Flicker.Default;
        public static int FirfilsFollowing
        {
            get
            {
                if (Engine.Scene is not null)
                {
                    return Engine.Scene.Tracker.GetEntities<Firfil>().Where(item => (item as Firfil).FollowingPlayer).Count();
                }
                return 0;
            }
        }
        public bool Stored => FirfilStorage.Stored.Contains(ID);
        public bool FollowingPlayer => Follower != null && Follower.HasLeader && Follower.Leader.Entity is Player;
        public bool FollowingEnabled => CanFollowFlag;
        public bool AtNest;
        public Follower Follower;
        public Vector2 Offset;
        public Vector2 OffsetTarget;
        public const float FlySpeed = 20f;
        public const int MaxFollowing = 100;
        public float SpeedMult = 1;
        public EntityID ID;
        private float offsetTimer;
        private float colorLerp;
        public Vector2 PulsePosition;
        public bool InView;
        private float afterImageTimer = 0.1f;
        public float Alpha = 1;
        public bool FollowImmediately;
        public bool Idle = true;
        public float FervorMult = 1;
        public bool Distracted;
        public PlayerCollider PlayerCollider;
        public WarpCapsule.OnWarpComponent OnWarp;
        public bool Fleeing;
        public Leader LastLeader;
        public bool LastLeaderWasPlayer;
        public Circle AvoidCollider;
        public Collider NormalCollider;
        public FlagList CanFollowFlag;
        public Vector2 AvoidOffset;
        public const float FleeDelayInterval = 0.05f;
        public Color ColorA => CurrentFlicker.ColorA;
        public Color ColorB => CurrentFlicker.ColorB;
        public Color Color => Color.Lerp(ColorA, ColorB, colorLerp);
        public Hitbox StatidCollider;
        public Statid Pollinating;
        public Statid Colliding;
        public bool IsPollinating => Pollinating != null;
        public float StatidColliderTimer = 2;
        public Vector2 AvoidOrig;
        public float FleeDelay;
        public Vector2 Speed;
        private Tween colorTween;
        private Coroutine dashFlickerCoroutine;
        private static Color prevDashColor;
        private Player dashingPlayer;
        public Firfil(EntityData data, Vector2 offset, EntityID id) : this(data.Position + offset, id)
        {
            CanFollowFlag = data.FlagList("canFollowFlag");
        }
        public Firfil(Vector2 position, EntityID id) : base(position)
        {
            ID = id;
            Collider = new Hitbox(9, 9, -4, -4);
            NormalCollider = new Hitbox(9, 9, -4, -4);
            AvoidCollider = new Circle(40);
            StatidCollider = new Hitbox(16, 16, -8, -8);
            Add(Follower = new Follower(OnGainLeader, OnLoseLeader));
            Follower.FollowDelay = Engine.DeltaTime;
            Add(PlayerCollider = new PlayerCollider(OnPlayer));
            colorTween = Tween.Set(this, Tween.TweenMode.Looping, 0.3f, Ease.SineInOut, t => colorLerp = t.Eased);
            Tag |= Tags.TransitionUpdate;
            OnWarp = new WarpCapsule.OnWarpComponent((p, c) => Flee(false));
            Add(OnWarp);
            TransitionListener transitionListener = new();
            transitionListener.OnOutBegin = () =>
            {
                if (Distracted)
                {
                    if (Pollinating != null)
                    {
                        Pollinating.Firfils.Remove(this);
                        if (Scene.GetPlayer() is Player player)
                        {
                            Pollinating = null;
                            player.Leader.GainFollower(Follower);
                            Distracted = false;
                        }
                    }
                }
            };
            Add(transitionListener);
            //DashListener dashListener = new DashListener(OnDash);
            //Add(dashListener);
            Add(dashFlickerCoroutine = new Coroutine(false));
/*            Add(new PostUpdateHook(() =>
            {
                if (startDashFlickerCoroutine)
                {
                    //dashFlickerCoroutine.Replace(dashFlicker(prevDashColor, dashingPlayer.Hair.Color));
                    dashingPlayer = null;
                    startDashFlickerCoroutine = false;
                }
            }));*/
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Collider c = Collider;
            Collider = AvoidCollider;
            Collider.Render(camera);
            Collider = c;
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (FollowingEnabled && FollowImmediately)
            {
                Player player = scene.GetPlayer();
                if (player != null)
                {
                    StartFollowing(player.Leader, false);
                }
            }
            if (PianoModule.Session.CollectedFirfilIDs.Contains(ID) && !AtNest)
            {
                RemoveSelf();
            }
            if (FirfilStorage.Stored.Contains(ID))
            {
                RemoveSelf();
            }
            afterImageTimer += Calc.Random.Range(0f, 0.3f);
        }
        private float mult;

        public override void Update()
        {
            if (!Stored)
            {
                if (Scene.GetPlayer() is Player player)
                {
                    if (!FollowingEnabled)
                    {
                        Collider = AvoidCollider;
                        if (CollideCheck(player))
                        {
                            float radius = AvoidCollider.Radius;
                            Vector2 offset = player.Center - Position;;
                            float distanceSquared = Vector2.DistanceSquared(Position, player.Center);
                            float normalMult = radius - distanceSquared;
                            float speedMult = 300 + (200 * normalMult);
                            Vector2 target = Vector2.Normalize(offset) * speedMult;
                            float approachMult = 300;
                            if(distanceSquared < (radius * radius) / 2)
                            {
                                approachMult += 200f;
                            }
                            Speed = Calc.Approach(Speed, target, approachMult * Engine.DeltaTime);
                        }
                    }
                    Collider = NormalCollider;
                    PollinateUpdate(player);
                }

                if (Speed.X != 0)
                {
                    Speed.X = Calc.Approach(Speed.X, 0f, 100f * Engine.DeltaTime);
                }
                if (Speed.Y != 0)
                {
                    Speed.Y = Calc.Approach(Speed.Y, 0f, 100f * Engine.DeltaTime);
                }
                Position += Speed * Engine.DeltaTime;
                base.Update();
                InView = SceneAs<Level>().Camera.GetBounds().Colliding(Collider.Bounds, 8);
                if (InView && afterImageTimer > 0)
                {
                    afterImageTimer -= Engine.DeltaTime;
                    if (afterImageTimer <= 0)
                    {
                        afterImageTimer = CurrentFlicker.AfterImageTimer;
                        AfterImage.Create(Position + Offset, DrawWithAlpha, 1, 0.5f);
                    }
                }
                if (offsetTimer >= 0)
                {
                    offsetTimer -= Engine.DeltaTime;
                    if (offsetTimer <= 0 && Idle)
                    {
                        SpeedMult = Calc.Random.Range(0.6f, 1f);
                        offsetTimer = Calc.Random.Range(0.5f, 1.2f);
                        OffsetTarget = Calc.AngleToVector(Calc.Random.NextAngle(), Calc.Random.Range(1f, 8));
                    }
                }
                Offset += (OffsetTarget - Offset) * (1f - (float)Math.Pow(0.0099999997764825821, Engine.DeltaTime)) * SpeedMult * FervorMult;
            }
        }
        public override void Render()
        {
            DrawWithAlpha(Position + Offset, 1);
        }
        public void DrawWithAlpha(Vector2 position, float alphaMult)
        {
            if (InView)
            {
                base.Render();
                Draw.Point(position, Color * (Alpha * alphaMult));
            }
        }
        public void PollinateUpdate(Player player)
        {
            if (Pollinating == null)
            {
                Statid statid = null;
                if (FollowingPlayer)
                {
                    if (Math.Abs(player.PreviousPosition.X - player.X) < 2 && Math.Abs(player.PreviousPosition.Y - player.Y) < 2)
                    {
                        statid = player.CollideFirst<Statid>();
                    }
                }
                if (statid != null && !statid.Dead)
                {
                    CurrentFlicker = Flicker.Pollinate;
                    if (Colliding != statid)
                    {
                        Colliding = statid;
                        StatidColliderTimer = 2;
                    }
                    if (StatidColliderTimer > 0)
                    {
                        StatidColliderTimer -= Engine.DeltaTime;
                        if (StatidColliderTimer <= 0)
                        {
                            Pollinating = statid;
                            Colliding = null;
                        }
                    }
                }
                else
                {
                    StatidColliderTimer = 2;
                    Colliding = null;
                    if (FollowingPlayer && CurrentFlicker != Flicker.Follow)
                    {
                        CurrentFlicker = Flicker.Follow;
                    }
                }
            }
            if (Pollinating != null)
            {
                Colliding = null;
                if (FollowingPlayer)
                {
                    Distracted = true;
                    player.Leader.LoseFollower(Follower);
                }
                if (!Pollinating.Firfils.Contains(this))
                {
                    Pollinating.Firfils.Add(this);
                    if (Pollinating.Firfils.Count > 9)
                    {
                        Pollinating.HasSap = true;
                    }
                }

                if (Distracted)
                {
                    if (Vector2.Distance(player.Center, Center) > 60)
                    {
                        if (!FollowingPlayer)
                        {
                            player.Leader.GainFollower(Follower);
                        }
                        Pollinating.Firfils.Remove(this);
                        Pollinating = null;
                        Distracted = false;
                    }
                    else
                    {
                        Position = Calc.Approach(Position, Pollinating.TopCenter - Vector2.UnitY * 16, 20f * Engine.DeltaTime);
                    }
                }
                else
                {
                    if (!FollowingPlayer)
                    {
                        player.Leader.GainFollower(Follower);
                    }
                    Pollinating.Firfils.Remove(this);
                    Pollinating = null;
                    Distracted = false;
                }
            }
        }
        public void OnPlayer(Player player)
        {
            if (!Distracted && !Stored && !AtNest && !FollowingPlayer && FollowingEnabled && FirfilsFollowing < MaxFollowing)
            {
                StartFollowing(player.Leader, true);
            }
        }
        public void OnDash(Vector2 dir)
        {
            if (FollowingPlayer)
            {
                Player player = Follower.Leader.Entity as Player;
                if (player != null)
                {
                    startDashFlickerCoroutine = true;
                    dashingPlayer = player;
                }
            }
        }
        private bool startDashFlickerCoroutine;
        [OnLoad]
        public static void Load()
        {
            On.Celeste.Player.StartDash += Player_StartDash;
        }
        private static int Player_StartDash(On.Celeste.Player.orig_StartDash orig, Player self)
        {
            prevDashColor = self.Hair.Color;
            return orig(self);
        }
        [OnUnload]
        public static void Unload()
        {
            On.Celeste.Player.StartDash -= Player_StartDash;
        }
        private IEnumerator dashFlicker(Color colorA, Color colorB)
        {
            Flicker flicker = new Flicker(colorA, colorB, 0.07f, Engine.DeltaTime);
            Flicker prevFlicker = CurrentFlicker;
            CurrentFlicker = flicker;
            for (float i = 0; i < 0.85f; i += Engine.DeltaTime)
            {
                if (CurrentFlicker != flicker)
                {
                    yield break;
                }
                yield return null;
            }
            CurrentFlicker = prevFlicker;
        }
        public void OnGainLeader()
        {
            SceneAs<Level>().Session.DoNotLoad.Add(ID);
            Tag |= Tags.Persistent;
            LastLeader = Follower.Leader;
            LastLeaderWasPlayer = Follower.Leader.Entity is Player;
            CurrentFlicker = Flicker.Follow;
            FleeDelay = 0;
            foreach (var f in LastLeader.GetFollowers<Firfil>())
            {
                if (f.Entity is Firfil firfil && !firfil.Fleeing)
                {
                    FleeDelay += FleeDelayInterval;
                }
            }
        }
        public void OnLoseLeader()
        {
            SceneAs<Level>().Session.DoNotLoad.Remove(ID);
            Tag &= ~Tags.Persistent;
            if (LastLeader != null)
            {
                if (LastLeaderWasPlayer && (Scene.GetPlayer() is not Player player || player.Dead) && !Fleeing)
                {
                    Flee(true);
                }
            }
            else
            {
                CurrentFlicker = Flicker.Default;
            }
            LastLeader = null;
        }
        public void StartFollowing(Leader leader, bool pulse = true)
        {
            if (pulse) PulseEntity.Circle(Position + Offset, Depth, Pulse.Fade.Linear, Pulse.Mode.Oneshot, 0, 8, 0.8f, true, Color, Color.White, Ease.CubeIn, Ease.CubeIn);
            Follower.PersistentFollow = true;
            leader.GainFollower(Follower);
        }
        public void StopFollowing(Leader leader, bool pulse = true)
        {
            if (pulse) PulseEntity.Circle(Position + Offset, Depth, Pulse.Fade.Linear, Pulse.Mode.Oneshot, 8, 0, 0.8f, true, Color, Color.White, Ease.CubeIn, Ease.CubeIn);
            leader?.LoseFollower(Follower);
        }
        public void Flee(bool frozenUpdate)
        {
            Collidable = false;
            if (frozenUpdate)
            {
                Tag |= Tags.FrozenUpdate;
            }
            Fleeing = true;
            CurrentFlicker = Flicker.Danger;
            Add(new Coroutine(FleeRandomDirection()));
        }
        public void FadeIn(float duration = 0.8f)
        {
            Alpha = 0;
            FadeTo(1, duration);
        }
        public void FadeTo(float to, float time)
        {
            float a = Alpha;
            Tween.Set(this, Tween.TweenMode.Oneshot, time, Ease.SineIn, t => Alpha = Calc.LerpClamp(a, to, t.Eased));
        }
        public void SetTarget(Vector2 position)
        {
            OffsetTarget = position - Position;
        }
        public void OnArriveAtNest()
        {
            Active = true;
            Visible = true;
            Collidable = false;
            AtNest = true;
            CurrentFlicker = Flicker.Nest;
            if (Follower != null)
            {
                Follower.Leader?.LoseFollower(Follower);
            }
        }
        public IEnumerator FleeRandomDirection()
        {
            yield return FleeDelay;
            float speedAngle = Calc.Random.NextAngle();
            Vector2 dir = Calc.AngleToVector(speedAngle, 1);
            Idle = false;
            float rand = Calc.Random.Range(0f, 32f);
            float speed = 3f + Calc.Random.Range(2f, 5f);
            Camera camera = SceneAs<Level>().Camera;
            float sinSpeed = 3f + Calc.Random.Range(0f, 5f);

            float sinAngle = speedAngle + 45f.ToRad();
            float angleOffset = -90f.ToRad();
            float startAngle = sinAngle;
            Tween.Set(this, Tween.TweenMode.YoyoLooping, 1f, Ease.SineInOut, t =>
            {
                sinAngle = startAngle + angleOffset * t.Eased;
            });
            int sinMult = Calc.Random.Sign();
            Vector2 prev = Position;
            while (this.OnScreen(8))
            {
                Position = prev + dir * speed;
                prev = Position;
                Position += Calc.AngleToVector(sinAngle, sinSpeed);
                OffsetTarget = Calc.Approach(OffsetTarget, Vector2.UnitX * (float)Math.Sin(Scene.TimeActive + rand * 8) * sinMult, sinSpeed * Engine.DeltaTime);
                speed = Calc.Approach(speed, 3f, 40f * Engine.DeltaTime);
                sinSpeed = Calc.Approach(sinSpeed, 30f, 5f * Engine.DeltaTime);
                yield return null;
            }
            Fleeing = false;
            RemoveSelf();
        }
    }
    [CustomEntity("PuzzleIslandHelper/FirfilNest")]
    [Tracked]
    public class FirfilNest : Entity
    {
        public static HashSet<EntityID> CollectedFirfilIDs => PianoModule.Session.CollectedFirfilIDs;

        public static MTexture Texture => GFX.Game["objects/PuzzleIslandHelper/firfil/nest"];
        public HashSet<Firfil> Firfils = [];
        public Image Nest;
        public float CircleRotation;
        public TalkComponent Talk;
        public int RequiredForKey;
        public string KeyID;
        public string FullKeyID => "FirfilNestKeyCollected:" + KeyID;
        private float rotateRate = 1;
        public bool CanSpawnKey => Firfils.Count >= RequiredForKey && !KeyCollected && !KeySpawned;
        public bool KeyCollected
        {
            get => KeyID.GetFlag();
            set => KeyID.SetFlag(value);
        }
        public bool KeySpawned
        {
            get => FullKeyID.GetFlag();
            set => FullKeyID.SetFlag();
        }
        public bool InCutscene;
        public bool SnapPositions;
        public int PositionLoops = 1;
        public FirfilNest(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = 1;
            Add(Nest = new Image(Texture));
            Collider = new Hitbox(Nest.Width, Nest.Height);
            Rectangle r = new Rectangle(0, 0, (int)Width, (int)Height);
            Add(Talk = new TalkComponent(r, Vector2.UnitX * Width / 2, Interact));
            KeyID = data.Attr("keyID");
            Tag |= Tags.TransitionUpdate;
            RequiredForKey = data.Int("requiredFirfils", 10);
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.Point(TopCenter - Vector2.UnitY * 32, Color.Magenta);
        }
        public override void Update()
        {
            base.Update();
            HandlePositions(SnapPositions);
            CircleRotation = (CircleRotation + rotateRate) % 360;
            if (InCutscene)
            {
                return;
            }
            if (Scene.GetPlayer() is Player player)
            {
                Talk.Enabled = player.Leader.Followers.Find(item => item.Entity is Firfil) != null;
            }
        }
        public class FirfilKeyCutscene : CutsceneEntity
        {
            public Player Player;
            public FirfilNest Nest;
            private Coroutine zoomRoutine, moveRoutine;
            private Vector2 cameraOrig;
            public FirfilKeyCutscene(Player player, FirfilNest nest) : base()
            {
                Player = player;
                Nest = nest;
            }
            public override void OnBegin(Level level)
            {
                Player.DisableMovement();
                Add(new Coroutine(Sequence()));
            }
            public IEnumerator Sequence()
            {
                cameraOrig = Level.Camera.Position;
                Vector2 target = Nest.TopCenter - Vector2.UnitY * 32;

                Add(moveRoutine = new Coroutine(CameraTo(target - new Vector2(160, 90), 2, Ease.SineInOut)));
                Add(zoomRoutine = new Coroutine(Level.ZoomTo(new Vector2(160, 90), 2, 1.5f)));

                yield return 0.8f;
                float from = Nest.rotateRate;
                for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
                {
                    Nest.rotateRate = Calc.LerpClamp(from, 0, Ease.CubeIn(i));
                    yield return null;
                }
                while (!zoomRoutine.Finished) yield return null;
                Nest.PositionLoops = 2;
                float speed = 0;
                float speedTimer = 0;
                while (speedTimer < 1.7f)
                {
                    Nest.rotateRate -= speed;
                    speed = Calc.Approach(speed, 200f, 20f * Engine.DeltaTime);
                    if (speed > 10)
                    {
                        speedTimer += Engine.DeltaTime;
                    }
                    yield return null;
                }
                Nest.SpawnKey(false);
                from = Nest.rotateRate;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    Nest.rotateRate = Calc.LerpClamp(from, 1, Ease.SineInOut(i));
                    yield return null;
                }
                Add(zoomRoutine = new(Level.ZoomBack(1)));
                Add(moveRoutine = new(CameraTo(cameraOrig, 1, Ease.SineInOut)));
                yield return 1;
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                zoomRoutine.RemoveSelf();
                moveRoutine.RemoveSelf();
                Level.ResetZoom();
                Level.Camera.Position = cameraOrig;
                if (!Nest.KeySpawned)
                {
                    Nest.SpawnKey(true);
                }
                Nest.PositionLoops = 1;
                Nest.rotateRate = 1;
                Nest.InCutscene = false;
                Player.EnableMovement();
            }
        }
        public void HandlePositions(bool instant)
        {
            int count = 0;
            foreach (Firfil f in Firfils)
            {
                SetFirfilPosition(count, f, instant);
                count++;
            }
        }
        public void SetFirfilPosition(int index, Firfil f, bool instant)
        {
            Vector2 position = TopCenter - Vector2.UnitY * 32;
            Vector2 offset;
            switch (index)
            {
                case < 11:
                    float space = 36;
                    float num = index * space;
                    float angle = (num + CircleRotation).ToRad();
                    offset = Calc.AngleToVector(angle, 10);
                    break;
                default:
                    offset = Vector2.Zero;
                    break;
            }

            Vector2 vector2 = position + offset;
            if (instant)
            {
                f.Position = vector2;
            }
            else
            {
                for (int i = 0; i < PositionLoops; i++)
                {
                    f.Position += (vector2 - f.Position) * (1f - (float)Math.Pow(0.0099999997764825821, Engine.DeltaTime));
                }
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (scene.GetPlayer() is Player player)
            {
                Talk.Enabled = player.Leader.Followers.Find(item => item.Entity is Firfil) != null;
            }
            HashSet<EntityID> inLevel = [];
            foreach (Firfil f in scene.Tracker.GetEntities<Firfil>())
            {
                //Auto collect any firfils in the level that should be at the nest
                if (CollectedFirfilIDs.Contains(f.ID))
                {
                    CollectFirfil(f);
                    inLevel.Add(f.ID);
                }
            }
            //re-add any firfils that aren't in the level that should be at the nest
            foreach (EntityID id in CollectedFirfilIDs)
            {
                if (!inLevel.Contains(id))
                {
                    CollectFirfil(CreateNewFirfil(id));
                }
            }
            HandlePositions(true);

            if (KeySpawned && !KeyCollected)
            {
                SpawnKey(true);
            }

        }
        public void SpawnKey(bool instant)
        {
            CutsceneHeart key = new(TopCenter - Vector2.UnitY * 32 - Vector2.One * 16, new EntityID(Guid.NewGuid().ToString(), 0), "", false, "", "", KeyID, Color.White, !instant);
            Scene.Add(key);
            KeySpawned = true;
        }
        public Firfil CreateNewFirfil(EntityID id)
        {
            Firfil newFirfil = new(TopCenter - Vector2.UnitY * 32, id);
            Scene.Add(newFirfil);
            return newFirfil;
        }
        public void CollectFirfil(Firfil firfil)
        {
            if (Firfils.Add(firfil))
            {
                CollectedFirfilIDs.Add(firfil.ID);
                firfil.OnArriveAtNest();
            }
        }
        public void Interact(Player player)
        {
            Input.Dash.ConsumePress();
            //todo: add ui
            foreach (Firfil f in player.Leader.GetFollowerEntities<Firfil>())
            {
                CollectFirfil(f);
            }
            if (CanSpawnKey)
            {
                Scene.Add(new FirfilKeyCutscene(player, this));
                InCutscene = true;
            }
        }
    }

    public static class FirfilStorage
    {
        public static HashSet<EntityID> Stored = [];
        public static void Store(Firfil firfil)
        {
            if (Stored.Add(firfil.ID))
            {
                firfil.RemoveSelf();
            }
        }
        public static void Take()
        {
            if (Stored.Count > 0)
            {
                if (Engine.Scene is not null && Engine.Scene.GetPlayer() is Player player)
                {
                    Firfil firfil = new Firfil(player.Position, Stored.First());
                    Stored.Remove(Stored.First());
                    Engine.Scene.Add(firfil);
                    firfil.FollowImmediately = true;
                }
            }
        }
        public static void ReleaseAll()
        {
            if (Engine.Scene is Level level)
            {
                foreach (EntityID id in Stored)
                {
                    level.Session.DoNotLoad.Remove(id);
                }
                Stored.Clear();

            }
        }
    }
}
