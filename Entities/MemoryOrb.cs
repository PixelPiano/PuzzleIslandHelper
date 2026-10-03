using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Celeste.Mod.PuzzleIslandHelper.Entities.Tower;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

// PuzzleIslandHelper.LabDoor
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    public static class OrbFlags
    {
        public static bool PortalDead
        {
            get => deadFlag;
            set => deadFlag.State = value;
        }
        public static bool GreenCollected
        {
            get => greenOrbCollected;
            set => greenOrbCollected.State = value;
        }
        public static bool BlueCollected
        {
            get => blueOrbCollected;
            set => blueOrbCollected.State = value;
        }
        public static bool RedCollected
        {
            get => redOrbCollected;
            set => redOrbCollected.State = value;
        }
        private static FlagList deadFlag = new("PortalIsDead");
        private static FlagList greenOrbCollected = new("GreenOrbCollected");
        private static FlagList blueOrbCollected = new("BlueOrbCollected");
        private static FlagList redOrbCollected = new("RedOrbCollected");
    }
    public static class OrbColors
    {
        public static readonly Color OffFill = Color.Black;
        public static readonly Color OffEdge = Color.LightGray;
        public static readonly Color OffCenter = Color.Gray;
        public static readonly Color BEdge = Color.DarkBlue;
        public static readonly Color BCenter = Color.LightBlue;
        public static readonly Color BFill = Color.Blue;
        public static readonly Color GEdge = Color.LimeGreen;
        public static readonly Color GCenter = Color.DarkGreen;
        public static readonly Color GFill = Color.Lime;
        public static readonly Color REdge = Color.DarkRed;
        public static readonly Color RCenter = new Color(1, 0.5f, 0.5f);
        public static readonly Color RFill = Color.Red;
    }

    [TrackedAs(typeof(MemoryOrb))]
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/BlueMemoryOrb")]
    public class BlueMemoryOrb : MemoryOrb
    {
        public BlueMemoryOrb(Vector2 position) : base(position, OrbColors.BFill, OrbColors.BCenter, OrbColors.BCenter)
        {
            OrbType = OrbTypes.Blue;
        }
    }
    [TrackedAs(typeof(MemoryOrb))]
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/GreenMemoryOrb")]
    public class GreenMemoryOrb : MemoryOrb
    {
        public GreenMemoryOrb(Vector2 position) : base(position, OrbColors.GFill, OrbColors.GCenter, OrbColors.GCenter)
        {
            OrbType = OrbTypes.Green;
        }
    }

    [TrackedAs(typeof(MemoryOrb))]
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/RedMemoryOrb")]
    public class RedMemoryOrb : MemoryOrb
    {
        public const float HiddenAlpha = 0.04f;
        public const float PortalAlpha = 0.06f;

        public bool AutoFade;
        public float TargetAlpha;
        public float RedAlpha = 0;
        public bool Reflection;
        public bool Follows;
        public bool Global;
        public bool Hidden;
        public bool RemoveIfAlreadyCollected;
        private VirtualRenderTarget target;
        private FlagList flag;
        private MirrorReflection reflection;

        public enum Presets
        {
            MainFollower,
            Reflection,
            Dummy
        }
        public Presets Preset;
        public RedMemoryOrb() : this(Vector2.Zero, Presets.MainFollower)
        {
        }
        public RedMemoryOrb(EntityData data, Vector2 offset) : this(data.Position + offset, data.Enum<Presets>("preset"), data.FlagList("flag"))
        {
        }
        public RedMemoryOrb(Vector2 position, Presets type, FlagList flag = default) : base(position, OrbColors.RFill, OrbColors.RCenter, OrbColors.REdge)
        {

            this.flag = flag;
            target = VirtualContent.CreateRenderTarget("RedMemoryOrbReflectionTarget", 32, 32);
            switch (type)
            {
                case Presets.MainFollower:
                    Hidden = true;
                    Follows = true;
                    Global = true;
                    RemoveIfAlreadyCollected = true;
                    break;
                case Presets.Reflection:
                    Reflection = true;
                    Follows = true;
                    RemoveIfAlreadyCollected = true;
                    break;
                case Presets.Dummy:
                    break;
            }
            Preset = type;
            SpiralApproachPortal = false;
            HoverMult = 0;
            Tag |= Tags.TransitionUpdate;
            if (Global)
            {
                Tag |= Tags.Global | Tags.Persistent;
            }
            OrbType = OrbTypes.Red;
            if (Reflection)
            {
                Add(reflection = new MirrorReflection());
            }
            else
            {
                AutoFade = true;
            }
            Add(new BeforeRenderHook(() =>
            {
                if (Reflection)
                {
                    target.SetAsTarget(true);
                    Vector2 offset = -Position + Vector2.One * (8 + Orb.Radius);
                    Orb.OffsetVertices(offset);
                    PianoUtils.DrawUserPrimitives<VertexPositionColor>(Matrix.Identity, ForEachPass);
                    Orb.OffsetVertices(-offset);
                }
            }));
        }
        public override void Render()
        {
            if (Reflection)
            {
                if (reflection.IsRendering)
                {
                    Components.Render();
                    Draw.SpriteBatch.Draw(target, Position - target.HalfSize(), Color.White);
                }
            }
            else
            {
                base.Render();
            }
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!flag)
            {
                RemoveSelf();
                return;
            }
            if (Hidden) RedAlpha = 0;
            if (Follows) State = StFollowPlayer;
        }
        public override void Update()
        {
            if (AutoFade && RedAlpha != TargetAlpha)
            {
                RedAlpha = PianoUtils.RubberbandApproach(RedAlpha, TargetAlpha, 0.05f);
            }
            base.Update();
            if (Reflection)
            {
                target.Width = (int)Orb.Radius * 2 + 16;
                target.Height = target.Width;
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            target?.Dispose();
        }
        public override void FollowPlayerBegin()
        {
            base.FollowPlayerBegin();
            if (Reflection)
            {
                Position = PlayerFollowing.Center;
                AtPlayer = true;
            }
            AfterImagesEnabled = false;
            if (Hidden)
            {
                TargetAlpha = HiddenAlpha;
                if (PlayerFollowing != null && PlayerFollowing.JustRespawned)
                {
                    RedAlpha = 0;
                }
            }
            else
            {
                TargetAlpha = 1;
                RedAlpha = 1;
            }
        }
        public override int FollowPlayerUpdate()
        {
            //if player gets too close to portal, red orb flies out to portal
            if (PlayerFollowing != null && !Reflection && SceneAs<Level>().Tracker.GetEntity<Portal>() is Portal portal && portal.AttractsRedOrb)
            {
                float dist = Vector2.Distance(PlayerFollowing.Center, portal.Center);
                if (dist < Portal.RedOrbDetectRadius)
                {
                    Portal = portal;
                    return StMoveToPortal;
                }
            }
            return base.FollowPlayerUpdate();
        }
        public override int MoveToPortalUpdate()
        {
            //if player gets too far away from portal, orb refollows player
            if (PlayerFollowing != null && !Reflection)
            {
                float dist = Vector2.Distance(PlayerFollowing.Center, Portal.Center);
                if (dist > Portal.RedOrbDetectRadius)
                {
                    return StFollowPlayer;
                }
            }
            return base.MoveToPortalUpdate();
        }
        public override int PortalUpdate()
        {
            if (!Reflection)
            {
                if (PlayerFollowing != null)
                {
                    float dist = Vector2.Distance(PlayerFollowing.Center, Portal.Center);
                    if (dist > Portal.RedOrbDetectRadius)
                    {
                        TargetAlpha = HiddenAlpha;
                        return StFollowPlayer;
                    }
                }
                TargetAlpha = PortalAlpha;
            }
            return base.PortalUpdate();
        }
        public override float GetAlpha()
        {
            return base.GetAlpha() * RedAlpha;
        }
        private static void Level_OnAfterUpdate1(Level obj)
        {
            foreach (RedMemoryOrb orb in obj.Tracker.GetEntities<RedMemoryOrb>())
            {
                orb.Level_OnAfterUpdate(obj);
            }
        }
        private static void Player_OnSpawn1(Player obj)
        {
            foreach (RedMemoryOrb orb in obj.Scene.Tracker.GetEntities<RedMemoryOrb>())
            {
                orb.Player_OnSpawn(obj);
            }
        }
        private static void Player_OnDie1(Player obj)
        {
            foreach (RedMemoryOrb orb in obj.Scene.Tracker.GetEntities<RedMemoryOrb>())
            {
                orb.Player_OnDie(obj);
            }
        }
        private void Player_OnSpawn(Player obj)
        {
            if (Follows && Global)
            {
                RemoveTag(Tags.FrozenUpdate);
                Position = obj.Center;
                PlayerFollowing = obj;
                AtPlayer = false;
                State = StFollowPlayer;
                TargetAlpha = HiddenAlpha;
            }
        }
        private void Player_OnDie(Player obj)
        {
            if (Follows && Global)
            {
                AddTag(Tags.FrozenUpdate);
                TargetAlpha = 0;
            }
        }
        private void Level_OnAfterUpdate(Level obj)
        {
            if (Follows && State == StFollowPlayer && AtPlayer && PlayerFollowing != null)
            {
                AtPlayer = true;
                Position = PlayerFollowing.Center;
                Orb.UpdateVertices(PlayerFollowing.Center + Orb.Shake * Orb.ShakeMult, Orb.Rotation, Orb.Radius);
            }
        }
        [OnLoad]
        public static void Load()
        {
            Everest.Events.Player.OnDie += Player_OnDie1;
            Everest.Events.Player.OnSpawn += Player_OnSpawn1;
            Everest.Events.Level.OnAfterUpdate += Level_OnAfterUpdate1;
            Everest.Events.Level.OnLoadLevel += Level_OnLoadLevel;
            Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
        }

        private static void LevelLoader_OnLoadingThread(Level level)
        {
            if (PianoModule.IsFromPuzzleIsland(level) && !level.Tracker.GetEntities<RedMemoryOrb>().Exists(item => (item as RedMemoryOrb).Preset == Presets.MainFollower) && !OrbFlags.RedCollected)
            {
                level.Add(new RedMemoryOrb());
            }
        }
        private static void Level_OnLoadLevel(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            bool collected = OrbFlags.RedCollected;
            //if follows and player is not null, set to snap at the end of the frame
            foreach (RedMemoryOrb orb in level.Tracker.GetEntities<RedMemoryOrb>())
            {
                if (orb.Follows && orb.PlayerFollowing != null)
                {
                    orb.AtPlayer = true;
                }
                if (collected && orb.RemoveIfAlreadyCollected)
                {
                    orb.RemoveSelf();
                }

            }
        }

        [OnUnload]
        public static void Unload()
        {
            Everest.Events.Player.OnDie -= Player_OnDie1;
            Everest.Events.Player.OnSpawn -= Player_OnSpawn1;
            Everest.Events.Level.OnAfterUpdate -= Level_OnAfterUpdate1;
            Everest.Events.Level.OnLoadLevel -= Level_OnLoadLevel;
            Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
        }

    }
    [Tracked]
    public abstract class MemoryOrb : Actor
    {
        public Vector2 PreviousPosition;
        public Vector2 Friction;
        public bool UseRawDeltaTime
        {
            get => Orb.UseRawDeltaTime;
            set => Orb.UseRawDeltaTime = value;
        }

        public bool RubberbandShake = false;
        public Portal.PortalHookComponent PortalHook;
        public bool SpiralApproachPortal = true;
        public Rectangle ActivateBounds;
        public Color Color
        {
            get => Orb.Color;
            set => Orb.Color = value;
        }
        public Color CenterColor
        {
            get => Orb.CenterColor.Value;
            set => Orb.CenterColor = value;
        }
        public Color EdgeColor
        {
            get => Orb.EdgeColor;
            set => Orb.EdgeColor = value;
        }
        public Vector2 Speed;
        public float SpeedMult = 1;
        private float maxSpeed;
        public BetterShaker Shaker;
        public BetterShaker CombineShakerA, CombineShakerB;
        public float CombineShakeA, CombineShakeB;
        public bool Combining;
        public Vector2 CombineShakeCenter;
        public float CombineIntensity = 1;

        public Vector2 ShakeVector;
        public Vector2 ShakeMult = Vector2.One;
        public Vector2 CombineShakeTarget
        {
            get
            {
                float angle = Calc.Angle(Position, CombineShakeCenter);
                Vector2 a = Calc.AngleToVector(angle, CombineShakeA) * CombineIntensity;
                Vector2 b = Calc.AngleToVector(angle + MathHelper.PiOver2, CombineShakeB) * CombineIntensity;
                return a + b;
            }
        }
        public Vector2 CurrentCombineShake;
        public Vector2 CurrentShakeVector;

        public Vector2 ShakeOffset
        {
            get
            {
                Vector2 offset = CurrentShakeVector;
                if (Combining)
                {
                    offset += CurrentCombineShake;
                }

                return offset;
            }
        }
        public float SineDist = 4;
        public float HoverMult = 1;
        public float Alpha
        {
            get => GetAlpha();
            set => _alpha = value;
        }
        private float _alpha = 1;
        public float AlphaMult = 1;
        public float InPortalAlpha = 1;
        public float ToPortalAlpha = 1;
        public float FollowPlayerAlpha = 1;
        private Tween combineIntensityTween;
        public float Delta => Orb.Delta;
        public void CombineIntensityTo(float to, float time, Ease.Easer ease = null) => CombineIntensityTo(CombineIntensity, to, time, ease);
        public void CombineIntensityTo(float from, float to, float time, Ease.Easer ease = null)
        {
            combineIntensityTween?.RemoveSelf();
            combineIntensityTween = Tween.Set(this, Tween.TweenMode.Oneshot, time, ease ?? Ease.SineOut, t =>
            {
                CombineIntensity = Calc.LerpClamp(from, to, t.Eased);
            }, t => CombineIntensity = to);
        }
        [CustomEntity("PuzzleIslandHelper/MemoryOrbPath")]
        [Tracked]
        public class OrbPath : Entity
        {
            public int Points => positions.Length;
            private Vector2[] positions;
            public float[] Lengths;
            public float TotalLength
            {
                get
                {
                    float f = 0;
                    foreach (float f2 in Lengths)
                    {
                        f += f2;
                    }
                    return f;
                }
            }
            public float ExitRadius;
            public string ID;
            public bool RemoveEntityOnEnd;
            public FlagList FlagsOnEnd;
            public string NextPathID;
            public int MaxLoops;
            public OrbPath(EntityData data, Vector2 offset) : this(offset, data.Nodes)
            {
                ID = data.Attr("pathID");
                ExitRadius = data.Float("nodeRadius");
                MaxLoops = data.Int("loops");
                NextPathID = data.Attr("nextPathID");
                RemoveEntityOnEnd = data.Bool("removeEntityOnEnd");
                FlagsOnEnd = data.FlagList("flagsOnEnd");
            }
            public OrbPath(Vector2 position, params Vector2[] positions) : base(position)
            {
                Vector2[] modified = [.. positions.Select(p => p + Vector2.One * 4)];
                SetPoints(modified);
            }
            public void SetPoints(params Vector2[] points)
            {
                positions = new Vector2[points.Length];
                Lengths = new float[points.Length - 1];
                Vector2 prev = default;
                for (int i = 0; i < points.Length; i++)
                {
                    Vector2 current = points[i];
                    positions[i] = current;
                    if (i > 0)
                    {
                        Lengths[i - 1] = (current - prev).Length();
                    }
                    prev = current;
                }
            }
            public Vector2 GetPoint(float progress, bool loop = false)
            {
                if (progress == 0) return positions[0];
                if (progress < 0)
                {
                    if (!loop) return positions[0];
                    else
                    {
                        progress += TotalLength;
                    }
                }
                float currentLength = progress;
                while (true)
                {
                    for (int i = 0; i < Lengths.Length; i++)
                    {
                        float l = Lengths[i];
                        if (currentLength >= l)
                        {
                            currentLength -= l;
                        }
                        else
                        {
                            return Calc.Approach(positions[i], positions[i + 1], currentLength);
                        }
                    }
                    if (!loop) break;
                }
                return positions[^1];
            }
            public Vector2 GetPoint(Vector2 prevPoint)
            {
                for (int i = 1; i < positions.Length; i++)
                {
                    if (positions[i - 1] == prevPoint) return positions[i];
                }
                return positions[0];
            }
            public Vector2 GetPoint(int index) => positions[index];
        }
        public string PathID;
        public OrbPath Path;
        public StateMachine StateMachine;
        public const int StDummy = 0;
        public const int StPath = 1;
        public const int StPortal = 2;
        public const int StMoveToPortal = 3;
        public const int StMoveToOrigin = 4;
        public const int StFight = 5;
        public const int StSummon = 6;
        public const int StFollowPlayer = 7;
        public int State
        {
            get => StateMachine.State;
            set
            {
                StateMachine.State = value;
            }
        }
        public int PathIndex { get; private set; }
        private SineWave sineFloat;
        private Vector2 sineOffset;
        public Vector2 Origin;
        public VertexOrb Orb;
        public Circle Circle;
        public float Radius
        {
            get => Orb.Radius;
            set
            {
                Orb.Radius = value;
                Circle.Radius = value;
            }
        }
        public List<VertexOrb.AfterImage> AfterImages = [];
        public void OffsetRender(Vector2 offset)
        {
            Orb.OffsetVertices(offset);
            foreach (var a in AfterImages)
            {
                a.OffsetVertices(offset);
            }
        }
        public float ShiverMult = 1;
        public bool InvertColors;
        public string CutsceneID;
        public bool AfterImagesEnabled;
        public float AfterImageInterval = 0.2f;
        public MemoryOrb Opponent;
        public bool AutoHandlePathSpeed = true;
        private float pathSpeedMult;
        private int pathLoops;
        public Portal Portal;
        public bool DummyHover;
        public bool DummyAfterImage;
        public float Yaw;
        public float Roll;
        public float Pitch;
        public Matrix YawRollPitch;
        public float YawRate, RollRate, PitchRate;
        public enum OrbTypes
        {
            Red,
            Green,
            Blue,
        }
        public OrbTypes OrbType = OrbTypes.Blue;
        public virtual float GetAlpha()
        {
            return _alpha * AlphaMult;
        }
        public MemoryOrb(Vector2 position, Color color, Color centerColor, Color edgeColor, float maxPathSpeed = 0, string pathId = null) : base(position)
        {
            Depth = 4;
            Add(Orb = new VertexOrb(Vector2.Zero, Portal.MaxOrbRadius, Portal.MaxOrbCorners, color, edgeColor) { CenterColor = centerColor });
            Collider = Circle = new Circle(Portal.MaxOrbRadius);
            PathID = pathId;
            maxSpeed = maxPathSpeed;
            Add(sineFloat = new SineWave(0.5f) { OnUpdate = (f) => sineOffset = Vector2.UnitY * f * SineDist });
            Orb.Visible = true;
            StateMachine = new StateMachine(10);
            Add(StateMachine);
            StateMachine.SetCallbacks(StDummy, null, null, DummyBegin, DummyEnd);
            StateMachine.SetCallbacks(StPath, PathUpdate, null, PathBegin);
            StateMachine.SetCallbacks(StPortal, PortalUpdate, null, PortalBegin, PortalEnd);
            StateMachine.SetCallbacks(StMoveToPortal, MoveToPortalUpdate, MoveToPortalRoutine, MoveToPortalBegin, MoveToPortalEnd);
            StateMachine.SetCallbacks(StFight, FightingUpdate, null, FightingBegin);
            StateMachine.SetCallbacks(StFollowPlayer, FollowPlayerUpdate, null, FollowPlayerBegin, FollowPlayerEnd);
            Origin = Position;

            Add(Shaker = new BetterShaker(v => ShakeVector += v));
            Add(CombineShakerA = new BetterShaker(v => CombineShakeA += v.X));
            Add(CombineShakerB = new BetterShaker(v => CombineShakeB += v.X));

            Add(PortalHook = new Portal.PortalHookComponent(OnPortalUpdateVertices, OnPortalRender, OnPortalRenderAfterImages));

        }
        public void CombineShake(float time = -1)
        {
            Combining = true;
            CombineShakerA.ShakeFor(time);
            CombineShakerB.ShakeFor(time);
        }
        public void StopCombineShake()
        {
            Combining = false;
            CombineShakerA.StopShaking();
            CombineShakerB.StopShaking();
        }
        public virtual void OnPortalUpdateVertices(Portal portal)
        {
            if (State == StPortal && Portal == portal)
            {
                VertexOrb p = Portal.Capsules[(int)OrbType];
                Position = p.RenderPosition;
                Orb.UpdateVertices(Position, p.Rotation, Orb.Radius);
            }
        }
        public virtual void OnPortalRenderAfterImages(Portal portal)
        {
            if (AfterImagesEnabled && State == StPortal && Portal == portal && AfterImages != null)
            {
                foreach (var a in AfterImages)
                {
                    a.DirectRenderVertices();
                }
            }
        }
        public virtual void OnPortalRender(Portal portal)
        {
            if (Visible && Orb.Visible && State == StPortal && Portal == portal)
            {
                Orb.DirectRenderVertices();
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
        }
        public MemoryOrb(EntityData data, Vector2 offset) : this(data.Position + offset, Calc.HexToColorWithAlpha(data.Attr("color")), Calc.HexToColorWithAlpha(data.Attr("centerColor")), Calc.HexToColorWithAlpha(data.Attr("edgeColor")), data.Float("maxPathSpeed"), data.Attr("pathID"))
        {
        }
        public bool AtPlayer;
        public Player PlayerFollowing;
        public virtual void FollowPlayerBegin()
        {
            PlayerFollowing = Scene.GetPlayer();
            AtPlayer = false;
            if (PlayerFollowing != null)
            {
                AtPlayer = PlayerFollowing.Center == Position;
            }
        }
        public virtual int FollowPlayerUpdate()
        {
            if (!AtPlayer)
            {
                PlayerFollowing ??= Scene.GetPlayer();
                if (PlayerFollowing != null)
                {
                    AtPlayer = this.TryRubberbandApproach(PlayerFollowing.Center, 1);
                }
            }
            return StFollowPlayer;
        }
        public virtual void FollowPlayerEnd()
        {
            AtPlayer = false;
            PlayerFollowing = null;
        }
        public void ShakeFor(float time = -1)
        {
            Shaker.ShakeFor(time);
        }
        private Tween shakeMultTween;
        public void ShakeMultTo(Vector2 from, Vector2 to, float time, Ease.Easer ease = null, Action onEnd = null) => ShakeMultTo(from.X, from.Y, to.X, to.Y, time, ease, onEnd);
        public void ShakeMultTo(float from, float to, float time, Ease.Easer ease = null, Action onEnd = null) => ShakeMultTo(from, from, to, to, time, ease, onEnd);
        public void ShakeMultTo(Vector2 to, float time, Ease.Easer ease = null, Action onEnd = null) => ShakeMultTo(ShakeMult.X, ShakeMult.Y, to.X, to.Y, time, ease, onEnd);
        public void ShakeMultTo(float to, float time, Ease.Easer ease = null, Action onEnd = null) => ShakeMultTo(ShakeMult.X, ShakeMult.Y, to, to, time, ease, onEnd);
        public void ShakeMultTo(float fromX, float fromY, float toX, float toY, float time, Ease.Easer ease = null, Action onEnd = null)
        {
            shakeMultTween?.RemoveSelf();
            Vector2 from = new Vector2(fromX, fromY);
            Vector2 to = new Vector2(toX, toY);
            shakeMultTween = Tween.Set(this, Tween.TweenMode.Oneshot, time, ease ?? Ease.Linear, t =>
            {
                ShakeMult = Vector2.Lerp(from, to, t.Eased);
            }, t =>
            {
                ShakeMult = to;
                onEnd?.Invoke();
            });
        }

        public void StopShaking()
        {
            Shaker.StopShaking();
        }
        public void DummyBegin()
        {
            DummyHover = true;
            DummyAfterImage = true;
        }
        public void DummyEnd()
        {
            DummyHover = false;
            DummyAfterImage = false;
        }
        /*
         * orb stays moving on path for a bit before circling around in front of the player.
         * player backs up a bit until the centerX of the screen is between them and the orb.
         * player slowly approaches orb and reaches out to touch it.
         * the orb shakes before growing to cover the whole screen with the player being the only other thing visible.
         * heavenly text sequence
         * portal opens
         * before rotation starts, do moving to portal sequence
         */
        public virtual void MoveToPortal(Portal portal)
        {
            Portal = portal;
            State = StMoveToPortal;
        }
        public virtual void AttachToPortal(Portal portal)
        {
            Portal = portal;
            State = StPortal;
        }
        public void StartFighting(MemoryOrb opponent)
        {
            Opponent = opponent;
            State = StFight;
        }

        public bool MoveTowards(Vector2 target, float speed, float exitDistance, float speedMult)
        {
            if (Vector2.DistanceSquared(Position, target) < exitDistance * exitDistance)
            {
                return true;
            }
            if (AutoHandlePathSpeed)
            {
                Vector2 angleVector = Vector2.Normalize(target - Position) * speed * Delta;
                Speed = Calc.Approach(Speed, angleVector, speedMult * 200f * Delta);
            }
            return false;
        }
        public void PathBegin()
        {
            PathIndex = 0;
            pathSpeedMult = 0f;
            SpeedMult = 1;
            AutoHandlePathSpeed = true;
            pathLoops = 0;
        }
        public int PathUpdate()
        {
            if (Path != null)
            {
                if (MoveTowards(Path.GetPoint(PathIndex), maxSpeed, Path.ExitRadius, pathSpeedMult))
                {
                    if (PathIndex + 1 >= Path.Points) //if at end of path
                    {
                        if (Path.MaxLoops < 0) //if loop forever...
                        {
                            PathIndex = 0; //reset the index
                        }
                        else //else if loop only a set number of times...
                        {
                            if (pathLoops < Path.MaxLoops) //if current loop count is below...
                            {
                                //reset the index and increment total loops
                                PathIndex = 0;
                                pathLoops++;
                            }
                            else
                            {
                                //else, we've reached the max loops for this path.
                                if (Path.RemoveEntityOnEnd)
                                {
                                    RemoveSelf();
                                }
                                else
                                {
                                    foreach (OrbPath p in Scene.Tracker.GetEntities<OrbPath>())
                                    {
                                        if (p.ID == Path.NextPathID)
                                        {
                                            //next path found, reset with the new path targeted
                                            Path = p;
                                            pathLoops = 0;
                                            PathIndex = 0;
                                            return StPath;
                                        }
                                    }
                                    return StDummy;
                                }
                            }
                        }
                    }
                    else PathIndex++;
                }
                else
                {
                    pathSpeedMult = Calc.Approach(pathSpeedMult, 1, Delta);
                }
            }
            else
            {
                Speed = Calc.Approach(Speed, Vector2.Zero, (1 - pathSpeedMult) * 200f * Delta);
                pathSpeedMult = Calc.Approach(pathSpeedMult, 0, Delta);
            }
            return StPath;
        }
        public virtual void PortalBegin()
        {
            Portal ??= Scene.Tracker.GetEntity<Portal>();
            if (Portal == null) throw new Exception("Cannot put MemoryOrb into State 'Portal' without an active portal reference.");
            AfterImagesEnabled = false;
        }
        public virtual int PortalUpdate()
        {
            Speed = Vector2.Zero;
            return StPortal;
        }
        public virtual void PortalEnd()
        {
            AfterImagesEnabled = false;
            Portal = null;
        }
        public virtual void MoveToPortalEnd()
        {

        }
        public virtual void MoveToPortalBegin()
        {
            Portal ??= Scene.Tracker.GetEntity<Portal>();
            if (Portal == null) throw new NullReferenceException("No Portal entity discovered in Scene");
            AfterImagesEnabled = true;
            AfterImageInterval = MoveToPortalAfterImagesInterval;

        }
        public virtual IEnumerator MoveToPortalRoutine()
        {
            VertexOrb trackedCircle = Portal.Capsules[(int)OrbType];
            if (SpiralApproachPortal)
            {
                float maxRotation = 2 * MathHelper.TwoPi;
                float prevPortalRotation = Portal.Rotation;
                float startDist = Vector2.Distance(Position, Portal.Position);
                float startAngle = (Position - Portal.Position).Angle();
                float endDist = Vector2.Distance(trackedCircle.RenderPosition, Portal.Position);
                float endAngle = (trackedCircle.RenderPosition - Portal.Position).Angle();
                Ease.Easer easer = Ease.Follow(Ease.SineIn, Ease.CubeOut);
                for (float i = 0; i < 1; i += Delta / 2)
                {
                    float eased = easer(i);
                    float dist = Calc.LerpClamp(startDist, endDist, eased);
                    float angle = Calc.LerpClamp(startAngle, endAngle, eased);
                    Position = Portal.Position + Calc.AngleToVector(angle + maxRotation * eased, dist);
                    yield return null;
                    endAngle += Portal.Rotation - prevPortalRotation;
                    startAngle += Portal.Rotation - prevPortalRotation;
                    prevPortalRotation = Portal.Rotation;
                }

            }
            else
            {
                Vector2 from = Position;
                for (float i = 0; i < 1; i += Delta / 0.8f)
                {
                    Position = Vector2.Lerp(from, trackedCircle.RenderPosition, Ease.CubeOut(i));
                    yield return null;
                }
            }
            State = StPortal;
        }
        public virtual int MoveToPortalUpdate()
        {
            return StMoveToPortal;
        }
        public virtual void MovingToPortalEnd()
        {
            AfterImagesEnabled = false;
            AfterImageInterval = 0.2f;
        }

        public virtual void FightingBegin()
        {

        }
        public virtual int FightingUpdate()
        {
            return StFight;
        }
        public virtual void FightingEnd()
        {
            Opponent = null;
        }
        private Ease.Easer expoOutIn = Ease.Follow(Ease.ExpoOut, Ease.ExpoIn);
        public Vector2 GetShiverOffset(VertexOrb.Shiver shiver, VertexOrb.VertexType type, int index, float angle)
        {
            double diff = (angle - shiver.CurrentAngle + MathHelper.Pi) % MathHelper.TwoPi - MathHelper.Pi;
            if (diff < -MathHelper.Pi) diff += MathHelper.TwoPi;
            diff = Math.Abs(diff);
            float lerp = 0;
            float indexLerp = Math.Abs(index % 2 * 0.5f);
            if (diff < shiver.Area / 2)
            {
                Ease.Easer ease = shiver.VertexEaser ?? Ease.Linear;
                lerp = 1 - expoOutIn((float)diff / (shiver.Area / 2));
            }
            return ShiverMult * lerp * Calc.AngleToVector(angle, Calc.LerpClamp(shiver.CurrentMinIntensity, shiver.CurrentMaxIntensity, indexLerp));
        }
/*        public Vector2 GetVertexOffset(VertexOrb.VertexType type, int index, float angle)
        {
            Vector2 offset = Vector2.Zero;
            if (type != VertexOrb.VertexType.Center)
            {
                if (ShiverMult > 0)
                {
                    foreach (VertexOrb.Shiver shiver in Orb.Shivers)
                    {
                        offset += GetShiverOffset(shiver, type, index, angle);
                    }
                }
            }
            return offset;
        }
*/
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (OrbPath p in scene.Tracker.GetEntities<OrbPath>())
            {
                if (p.ID == PathID)
                {
                    Path = p;
                    break;
                }
            }
            Orb.ShiverMult = 1;
            //Orb.VertexOffset = GetVertexOffset;
        }
        public override void Render()
        {
            base.Render();
            if (State != StPortal || Portal == null)
            {
                Draw.SpriteBatch.End();
                PianoUtils.DrawUserPrimitives<VertexPositionColor>(SceneAs<Level>().Camera.Matrix, ForEachPass);
                GameplayRenderer.Begin();
            }
        }
        public Vector2 RenderOffset;
        public void ForEachPass(EffectPass pass)
        {
            foreach (var a in AfterImages)
            {
                a.DirectRenderVertices();
            }
            if (Orb.Visible)
            {
                Orb.DirectRenderVertices();
            }
        }
        public const float MoveToPortalAfterImagesInterval = 0.1f;
        public virtual void CreateAfterImage()
        {
            VertexOrb.AfterImage afterImage = new(Orb, 1, 1, 0, AfterImages);
            afterImage.FillAlpha *= 0.2f;
            afterImage.EdgeAlpha *= 0.8f;
            Add(afterImage);
        }
        public override void Update()
        {
            Vector2 hoverOffset = sineOffset * HoverMult;
            if ((State == StDummy && !DummyHover) || State == StPortal) hoverOffset *= 0;
            if (State == StMoveToPortal) hoverOffset *= 0.7f;

            CurrentShakeVector = RubberbandShake ? CurrentShakeVector.RubberbandApproach(ShakeVector * ShakeMult, 0.05f, 0.000000007764825821) : ShakeVector * ShakeMult;
            CurrentCombineShake = CurrentCombineShake.RubberbandApproach(CombineShakeTarget, 0.05f, 0.000000007764825821);
            Orb.RenderOffset = hoverOffset + ShakeOffset + RenderOffset;
            Orb.Alpha = Alpha * AlphaMult;
            base.Update();
            float delta = Delta;
            if (AfterImagesEnabled && (State != StDummy || DummyAfterImage))
            {
                if (UseRawDeltaTime)
                {
                    if (Scene.OnRawInterval(AfterImageInterval)) CreateAfterImage();
                }
                else if (Scene.OnInterval(AfterImageInterval)) CreateAfterImage();
            }
            PreviousPosition = Position;
            if (Speed.X * SpeedMult != 0) MoveH(Speed.X * SpeedMult * delta);
            if (Speed.Y * SpeedMult != 0) MoveV(Speed.Y * SpeedMult * delta);
            if (Friction.X != 0)
            {
                if (Speed.X != 0) Speed.X = Calc.Approach(Speed.X, 0, Math.Abs(Friction.X) * delta);
            }
            if (Friction.Y != 0)
            {
                if (Speed.Y != 0) Speed.Y = Calc.Approach(Speed.Y, 0, Math.Abs(Friction.Y) * delta);
            }
        }
        #region Visual Manipulation
        public void RemoveAllInverts() => Orb.RemoveAllInverts();
        public void RemoveAllColorMods() => Orb.RemoveAllColorMods();
        public VertexOrb.InvertMod Invert(float duration, int loops, bool fade, float invertAdjust = 0) => Orb.Invert(duration, loops, fade, invertAdjust);

        public VertexOrb.ColorMod FlashColorMod(Color to, float flashDuration, float loopDelay, int flashes) => Orb.FlashColorMod(to, flashDuration, loopDelay, flashes);

        public VertexOrb.ColorMod FlashColorMod(Color centerTo, Color fillTo, Color edgeTo, float flashDuration, float loopDelay, int flashes) => Orb.FlashColorMod(centerTo, fillTo, edgeTo, flashDuration, loopDelay, flashes);

        public VertexOrb.ColorMod FlashColorMod(float flashDuration, float loopDelay, int flashes, VertexOrb.ColorMod.GetColor mod) => Orb.FlashColorMod(flashDuration, loopDelay, flashes, mod);

        public VertexOrb.ColorMod FlashColorMod(float flashDuration, float loopDelay, int flashes, VertexOrb.ColorMod.GetColor centerMod, VertexOrb.ColorMod.GetColor fillMod, VertexOrb.ColorMod.GetColor edgeMod) => Orb.FlashColorMod(flashDuration, loopDelay, flashes, centerMod, fillMod, edgeMod);

        public VertexOrb.ColorMod PulseColorMod(float inDuration, float outDuration, Ease.Easer inEase, Ease.Easer outEase, int loops, VertexOrb.ColorMod.GetColor mod) => Orb.PulseColorMod(inDuration, outDuration, inEase, outEase, loops, mod);

        public VertexOrb.ColorMod PulseColorMod(float inDuration, float outDuration, Ease.Easer inEase, Ease.Easer outEase, int loops, VertexOrb.ColorMod.GetColor centerMod, VertexOrb.ColorMod.GetColor fillMod, VertexOrb.ColorMod.GetColor edgeMod) => Orb.PulseColorMod(inDuration, outDuration, inEase, outEase, loops, centerMod, fillMod, edgeMod);
        [Command("add_shiver", "")]
        public static void AddShiver()
        {
            foreach (RedMemoryOrb orb in Engine.Scene.Tracker.GetEntities<RedMemoryOrb>())
            {
                orb.AddShiver(1, 0, 8, 2, MathHelper.Pi, Ease.Linear, Tween.TweenMode.Oneshot);
            }
        }
        public VertexOrb.Shiver AddShiver(float duration, float startAngle, float maxIntensity, float minIntensity, float angleArea, Ease.Easer ease, Tween.TweenMode tweenMode, float intensityLerpDuration = 0, Ease.Easer vertexEase = null, Ease.Easer intensityLerpEase = null, Action<VertexOrb.Shiver> onEnd = null, Ease.Easer afterLoopEase = null) => Orb.AddShiver(duration, startAngle, maxIntensity, minIntensity, angleArea, ease, tweenMode, intensityLerpDuration, vertexEase, intensityLerpEase, onEnd, afterLoopEase);
        
        public void AddShiver(VertexOrb.Shiver shiver) => Orb.AddShiver(shiver);
        public void RemoveShivers() => Orb.RemoveShivers();
        
        public void FadeShiver(VertexOrb.Shiver shiver, float duration, bool removeOnComplete = true) => Orb.FadeShiver(shiver, duration, removeOnComplete);
        public void FadeAllShivers(float duration, bool removeOnComplete = true) => Orb.FadeAllShivers(duration, removeOnComplete);
        #endregion
    }
}
