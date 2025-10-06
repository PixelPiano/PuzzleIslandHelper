using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Cutscenes;
using Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities;
using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [ConstantEntity("PuzzleIslandHelper/CalidusFollowingUpdater")]
    public class CalidusFollowUpdater : Entity
    {
        public CalidusFollowUpdater() : base()
        {
            Tag |= Tags.Global | Tags.TransitionUpdate;
        }
        public override void Update()
        {
            base.Update();
            foreach (Calidus c in SceneAs<Level>().Tracker.GetEntities<Calidus>())
            {
                if (c.Following)
                {
                    SceneAs<Level>().Session.SetFlag("CalidusFollowing");
                    return;
                }
            }
            SceneAs<Level>().Session.SetFlag("CalidusFollowing", false);
        }
    }

    [CustomEntity("PuzzleIslandHelper/Calidus")]
    [Tracked]
    public class Calidus : Actor
    {
        public class DialogCutsceneEntity : CutsceneEntity
        {
            public Calidus Calidus;
            public Player Player;
            public DialogCutsceneEntity(Calidus calidus, Player player) : base()
            {
                Calidus = calidus;
                Player = player;
            }
            public override void OnBegin(Level level)
            {
                Calidus.OnCutsceneStart();
                Player.StateMachine.State = Player.StDummy;
                Add(new Coroutine(text()));
            }
            private IEnumerator text()
            {
                yield return Textbox.Say(Calidus.CutsceneID);
                EndCutscene(Level);
            }
            public override void OnEnd(Level level)
            {
                level.ResetZoom();
                Calidus.OnCutsceneEnd();
                Player.StateMachine.State = Player.StNormal;
            }
        }
        private int calidi
        {
            get
            {
                return SceneAs<Level>().Tracker.GetEntities<Calidus>().Count;
            }
        }
        public Arguments StartArgs;
        public Arguments EndArgs;
        public Arguments AwakeArgs;
        public string RawStartArgs;
        public string RawEndArgs;
        public bool Persistent
        {
            get => TagCheck(Tags.Persistent);
            set
            {
                if (value)
                {
                    AddTag(Tags.Persistent);
                    if (Engine.Scene is Level level)
                    {
                        level.Session.DoNotLoad.Add(ID);
                    }
                }
                else
                {
                    RemoveTag(Tags.Persistent);
                    if (Engine.Scene is Level level)
                    {
                        level.Session.DoNotLoad.Remove(ID);
                    }
                }
            }
        }
        public class Argument()
        {
            public string Name;
            public string Content = "";
            public override string ToString()
            {
                return $"{{{Name}:{Content}}}";
            }
        }
        public class Arguments
        {
            public List<Argument> Args;
            public Arguments(params List<Argument> args)
            {
                Args = args;
            }
            public override string ToString()
            {
                string output = "";
                foreach (Argument arg in Args)
                {
                    output += '\n' + arg.ToString();
                }
                return output;
            }
        }
        public void OnCutsceneStart()
        {
            ExecuteArgs(StartArgs);
        }
        public void OnCutsceneEnd()
        {
            //FlagOnCutsceneEnd.State = true;
            ExecuteArgs(EndArgs);
        }
        public void ExecuteArgs(Arguments args)
        {
            if (args != null)
            {
                foreach (Argument s in args.Args)
                {
                    switch (s.Name)
                    {
                        case "follow":
                            if (!string.IsNullOrEmpty(s.Content) && Enum.TryParse<Looking>(s.Content, out Looking looking))
                            {
                                StartFollowing(looking);
                            }
                            else
                            {
                                StartFollowing();
                            }
                            break;
                        case "xoff" or "yoff" or "woff" or "hoff":
                            if (!string.IsNullOrEmpty(s.Content) && Single.TryParse(s.Content, out Single extend))
                            {
                                switch (s.Name[0])
                                {
                                    case 'x':
                                        Talk.Bounds.X += (int)extend;
                                        break;
                                    case 'y':
                                        Talk.Bounds.Y += (int)extend;
                                        break;
                                    case 'w':
                                        Talk.Bounds.Width += (int)extend;
                                        break;
                                    case 'h':
                                        Talk.Bounds.Height += (int)extend;
                                        break;
                                }
                            }
                            break;
                        case "mood":
                            if (!string.IsNullOrEmpty(s.Content) && Enum.TryParse<Mood>(s.Content, out Mood result))
                            {
                                Emotion(result);
                            }
                            break;
                        case "stopfollowing":
                            StopFollowing();
                            break;
                        case "flag":
                            string[] array = s.Content.Split(',');
                            if (array != null && array.Length > 0)
                            {
                                for (int i = 0; i < array.Length; i++)
                                {
                                    string flag = array[i];
                                    if (!string.IsNullOrEmpty(flag))
                                    {
                                        flag = flag.Replace(" ", "");
                                        bool inverted = false;
                                        if (flag.Length > 1 && flag[0] == '!')
                                        {
                                            inverted = true;
                                            flag = flag[1..];
                                        }
                                        SceneAs<Level>().Session.SetFlag(flag, !inverted);
                                    }
                                }
                            }
                            break;

                    }
                }
            }
        }

        public enum Mood
        {
            Happy,
            Stern,
            Normal,
            RollEye,
            Laughing,
            Shakers,
            Nodders,
            Closed,
            Angry,
            Surprised,
            Wink,
            Eugh,
            Dizzy,
            None
        }
        public enum Looking
        {
            None,
            Left,
            Right,
            Up,
            Down,
            UpRight,
            UpLeft,
            DownRight,
            DownLeft,
            Center,
            Player,
            Dynamic,
            Target,
        }
        public enum EyeDepths
        {
            Front,
            Behind
        }
        public EyeDepths EyeDepth;
        public Looking LookDir
        {
            get
            {
                if (LookOverride != Looking.None)
                {
                    return LookOverride;
                }
                if (followLook != Looking.None)
                {
                    return followLook;
                }
                return look;
            }
            set
            {
                look = value;
            }
        }
        public Looking LookOverride;
        private Looking followLook;
        private Looking look = Looking.Center;
        public Mood CurrentMood
        {
            get => mood;
            set
            {
                prevMood = mood;
                mood = value;
                if (value != prevMood)
                {
                    Emotion(value);
                }
            }
        }
        private Mood mood = Mood.Normal;
        private Mood prevMood;
        public Vector2 dizzyOffset;
        private Vector2 dizzyTarget;
        private float dizzyTimer;
        public Entity LookEntity;
        private int ColorTweenLoops = 2;
        private int ArmLoopCount;
        private int ArmBuffer = 60;
        private int heeheeBuffer = 2;
        private (float, float) ArmOffsets;
        public float FloatHeight = 6;
        public float LookSpeed = 1;
        public List<CalCut> RequiredWatched = [];
        public List<CalCut> ExcludedWatched = [];
        public bool Dizzy;
        public bool AddBackToPlayerOnSpawn;
        public Part BrokenParts;
        public Part OrbSprite;
        public Part EyeSprite;
        public Sprite Symbols;
        public Part[] Arms = new Part[2];
        private ParticleSystem system;
        public VertexLight Light;
        private Tween nudgeTween;
        public Vector2 OrigPosition;
        public Vector2 NudgeOffset;
        public Vector2 Scale = Vector2.One;
        public Vector2 EyeScale = Vector2.One;
        public Vector2 ApproachScale = Vector2.One;
        public Vector2 LookTarget;
        public Vector2 EyeOffset;
        private Vector2 StarPos;
        public Vector2 Offset => NudgeOffset + shakeVector + dizzyOffset + Vector2.UnitY * sineY;
        private EntityID ID;
        private Vector2 shakeVector;
        private float sineY;
        private float sineYTarget;
        private float StarRotation;
        private float OutlineOpacity;
        public float FloatTarget;
        public float FloatAmount
        {
            get => rawFloatAmount * floatMult;
            set => rawFloatAmount = value;
        }
        private float rawFloatAmount;
        private float floatMult = 1;
        public bool Continue;
        public bool EyeInstant;
        public bool CanFloat = true;
        public bool AutoSetEyeOffset;
        private bool RenderStar;
        public bool Broken;
        public bool Talkable;
        public bool Following;
        public bool ForceBlink;
        public bool Blinking;
        public bool FallenApart;
        public bool ForceShake;
        public bool Shaking => ForceShake || shakeTimer > 0;
        public bool StartFollowingImmediately;
        private float shakeTimer;
        public string[] CutsceneArgs;
        public Follower Follower;

        public Collider SpriteBox;
        public List<Part> Parts = new();
        private MTexture Star;
        public TalkComponent Talk;
        private Coroutine burstRoutine;
        public TextboxListener EmotionListener;
        public float Alpha = 1;
        public FlagList SpawnData;
        public FlagList TalkFlag;
        public string CutsceneID;
        public bool RequireSpawnFlag;
        public static HashSet<string> BannedCutsceneIDs = [];
        public bool CutsceneIsDialog;
        private ParticleType HeeHee = new ParticleType
        {
            Source = GFX.Game["objects/PuzzleIslandHelper/particles/heehee00"],
            Size = 1f,
            Color = Color.Gray,
            Color2 = Color.White,
            ColorMode = ParticleType.ColorModes.Choose,
            LifeMin = 1f,
            LifeMax = 1f,
            SpeedMin = 12,
            SpeedMax = 12,
            Direction = Calc.Up,
            DirectionRange = 45f.ToRad(),
            FadeMode = ParticleType.FadeModes.Linear,
        };
        public class Part : Sprite
        {
            public Calidus Parent;
            public Vector2 OrigPosition;
            public float ReturnDelay;
            public bool Assembled = true;
            public float FallSpeed;
            public float RotationRate;
            public Vector2 Offset;
            public Vector2 VisualOffset;
            public bool VisualOffsetApproachZero = true;
            public float OutlineOpacity = 1;
            public bool OnGround;
            public float FallOffset;
            public new Vector2 RenderPosition
            {
                get
                {
                    return ((base.Entity == null) ? Vector2.Zero : base.Entity.Position + Parent.Offset) + Position;
                }
                set
                {
                    Position = value - ((base.Entity == null) ? Vector2.Zero : base.Entity.Position + Parent.Offset);
                }
            }
            public float Alpha = 1;
            public Part(Atlas atlas, string path) : base(atlas, path)
            {
            }
            public override void Added(Entity entity)
            {
                base.Added(entity);
                Parent = entity as Calidus;
                CenterOrigin();
                Position += new Vector2(Width / 2, Height / 2).Floor();
            }
            public override void Render()
            {
                RenderAt(RenderPosition + VisualOffset);
            }
            public override void Update()
            {
                base.Update();
                if (VisualOffsetApproachZero && VisualOffset != Vector2.Zero)
                {
                    Vector2 prev = VisualOffset;
                    VisualOffset -= VisualOffset * (1f - (float)Math.Pow(0.0099999997764825821, Engine.DeltaTime));
                    if (Vector2.DistanceSquared(prev, VisualOffset) < 0.00001f)
                    {
                        VisualOffset = Vector2.Zero;
                    }
                }
            }
            public void Return(float speedMult = 1)
            {
                Entity?.Add(new Coroutine(ReturnRoutine(speedMult)));
            }
            public void Reset()
            {
                OnGround = false;
                Assembled = true;
                Rotation = 0;
                Position = OrigPosition;
            }
            public void Fall(float distanceToFloor, bool instant = false)
            {
                Assembled = false;
                if (!instant)
                {
                    Entity?.Add(new Coroutine(FallRoutine(distanceToFloor)));
                }
                else
                {
                    Ground(distanceToFloor);
                }
            }
            public void DrawOutline()
            {
                DrawOutline(Color.Black * OutlineOpacity, 1);
            }
            public void DrawOutline(Vector2 at)
            {
                Vector2 from = Position;
                Position = at + Offset;
                DrawOutline();
                Position = from;
            }
            public void RenderAt(Vector2 at)
            {
                if (Texture != null)
                {
                    Draw.SpriteBatch.Draw(Texture.Texture.Texture_Safe, at + Offset, null, Color * Parent.Alpha, Rotation, Origin, Scale, Effects, 0);
                }
            }
            public void Ground(float distanceToFloor)
            {
                distanceToFloor += FallOffset;
                Assembled = false;
                OnGround = false;
                float orig = Y + Height;
                float ySpeed = FallSpeed;
                float rot = RotationRate.ToRad();
                OrigPosition = Position;
                int sign = 1;
                int bouncesLeft = 2;
                while (true)
                {
                    Y += ySpeed * Engine.DeltaTime;
                    float num = (Math.Abs(ySpeed) < 40f ? 0.5f : 1f);
                    ySpeed = Calc.Approach(ySpeed, 160f, 450f * num * Engine.DeltaTime);
                    Rotation += rot * num * sign;
                    if (Y >= orig + distanceToFloor)
                    {
                        if (bouncesLeft > 0)
                        {
                            ySpeed = -32 * (bouncesLeft / 2f);
                            bouncesLeft--;
                        }
                        else break;
                    }
                }
                Y = orig + distanceToFloor;
                OnGround = true;
            }
            private IEnumerator FallRoutine(float distanceToFloor)
            {
                distanceToFloor += FallOffset;
                Assembled = false;
                OnGround = false;
                float orig = Y + Height;
                float ySpeed = FallSpeed;
                float rot = RotationRate.ToRad();
                OrigPosition = Position;
                int sign = 1;
                int bouncesLeft = 2;
                while (true)
                {
                    Y += ySpeed * Engine.DeltaTime;
                    float num = (Math.Abs(ySpeed) < 40f ? 0.5f : 1f);
                    ySpeed = Calc.Approach(ySpeed, 160f, 450f * num * Engine.DeltaTime);
                    Rotation += rot * num * sign;
                    if (Y >= orig + distanceToFloor)
                    {
                        if (bouncesLeft > 0)
                        {
                            ySpeed = -32 * (bouncesLeft / 2f);
                            bouncesLeft--;
                        }
                        else break;
                    }
                    yield return null;
                }
                Y = orig + distanceToFloor;
                OnGround = true;
            }
            public IEnumerator ReturnRoutine(float speedMult = 1)
            {
                OnGround = true;
                Vector2 p = Position;
                int increment = 1;
                for (int i = 0; i < 8; i++)
                {
                    Position.X += increment;
                    increment = -increment;
                    yield return Engine.DeltaTime * 2 / speedMult;
                }
                Position = p;
                yield return ReturnDelay / speedMult;
                Vector2 from = Position;
                float rotFrom = Rotation;
                OnGround = false;
                for (float i = 0; i < 1; i += Engine.DeltaTime * speedMult)
                {
                    Position = Vector2.Lerp(from, OrigPosition, Ease.Follow(Ease.CubeIn, Ease.ElasticOut)(i));
                    Rotation = Calc.LerpClamp(rotFrom, 0, Ease.CubeIn(i + 0.1f));
                    yield return null;
                }
                Assembled = true;
                Reset();
            }
        }
        [OnUnload]
        public static void Unload()
        {
            BannedCutsceneIDs.Clear();
        }
        public static Calidus Create()
        {
            return Create(Vector2.Zero, false, true, Looking.Center, Mood.Normal);
        }
        public static Calidus CreateAndFollow(Player player)
        {
            return Create(player.Position, false, true, Looking.Player, Mood.Normal, true);
        }
        public static Calidus Create(Vector2 position, bool broken = false, bool startFloating = true, Looking look = Looking.Center, Mood mood = Mood.Normal, bool startFollowing = false)
        {
            Calidus c = new Calidus(position, new EntityID(Guid.NewGuid().ToString(), 0), broken, startFloating, look, mood, startFollowing);
            Engine.Scene.Add(c);
            return c;
        }
        public static string LastRegisteredRoom;
        public static Vector2 LastRegisteredOffset;
        public void RegisterLastPosition()
        {
            if (Scene is Level level)
            {
                LastRegisteredRoom = level.Session.Level;
                LastRegisteredOffset = Position - level.LevelOffset;
            }
        }
        public Calidus(EntityData data, Vector2 offset, EntityID id)
            : this(data.Position + offset, id, data.Bool("broken"), data.Bool("startFloating", true), data.Enum("looking", Looking.Center),
                  data.Enum("mood", Mood.Normal), data.Bool("followPlayer"), data.Attr("requiredCutscenes"), data.Attr("excludedCutscenes"))
        {
            RequireSpawnFlag = data.Bool("requireSpawnFlag");
            SpawnData = new FlagList(data.Attr("spawnFlag"));
            TalkFlag = new FlagList(data.Attr("cutsceneFlag"));
            CutsceneID = data.Attr("cutscene");
            CutsceneIsDialog = data.Attr("cutsceneType") == "Dialog";
            RawStartArgs = data.Attr("startCutsceneArgs");
            RawEndArgs = data.Attr("endCutsceneArgs");
            StartArgs = ParseArgs(RawStartArgs);
            EndArgs = ParseArgs(RawEndArgs);
            AwakeArgs = ParseArgs(data.Attr("awakeArgs"));
        }
        public static Arguments ParseArgs(string args)
        {
            if (!string.IsNullOrEmpty(args))
            {
                List<Argument> finalArgs = [];
                string[] array = args.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (string s in array)
                {
                    Argument argument = new Argument();
                    if (s.Contains(':'))
                    {
                        string[] array2 = s.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        if (array2.Length > 0)
                        {
                            argument.Name = array2[0];
                            if (array2.Length > 1)
                            {
                                argument.Content = array2[1];
                            }
                        }
                    }
                    else
                    {
                        argument.Name = s;
                    }
                    finalArgs.Add(argument);
                }
                return new(finalArgs);
            }
            return null;
        }
        public Calidus(Vector2 position, bool broken = false, bool startFloating = true, Looking looking = Looking.Center, Mood mood = Mood.Normal, bool startFollowingImmediately = false)
            : this(position, new EntityID(Guid.NewGuid().ToString(), 0), broken, startFloating, looking, mood, startFollowingImmediately) { }
        public Calidus(Vector2 position, EntityID id, bool broken = false, bool startFloating = true, Looking look = Looking.Center, Mood mood = Mood.Normal, bool startFollowingImmediately = false, string requiredWatched = "", string excludeWatched = "") : base(position)
        {
            Add(idleCoroutine = new Coroutine(false));
            Add(idleEndRoutine = new Coroutine(false));
            foreach (string s in requiredWatched.Replace(" ", "").Split(','))
            {
                if (Enum.TryParse(s, true, out CalCut result))
                {
                    RequiredWatched.Add(result);
                }
            }
            foreach (string s in excludeWatched.Replace(" ", "").Split(','))
            {
                if (Enum.TryParse(s, true, out CalCut result))
                {
                    ExcludedWatched.Add(result);
                }
            }
            Light = new VertexLight(Color.White, 1, 32, 64);
            Add(Light);
            if (broken)
            {
                Light.Alpha = 0;
            }
            ID = id;
            LookDir = look;
            CanFloat = startFloating;
            Broken = broken;
            OutlineOpacity =/* broken ? 0 :*/ 1;
            string path = "characters/PuzzleIslandHelper/Calidus/";
            EyeSprite = new Part(GFX.Game, path); //{ Visible = !Broken };
            Symbols = new Sprite(GFX.Game, path);// { Visible = !Broken };
            OrbSprite = new Part(GFX.Game, path)
            {
                FallSpeed = -48,
                RotationRate = 8,
                //Visible = !Broken,
                ReturnDelay = 0.3f
            };
            Arms[0] = new Part(GFX.Game, path)
            {
                RotationRate = -5,
                FallSpeed = -32,
                //Visible = !Broken,
                ReturnDelay = 0,
                FallOffset = 8
            };
            Arms[1] = new Part(GFX.Game, path)
            {
                RotationRate = 6,
                FallSpeed = -32,
                //Visible = !Broken,
                ReturnDelay = 0.6f,
                FallOffset = 8
            };
            BrokenParts = new Part(GFX.Game, path)
            {
                Color = Color.Lerp(Color.Black, Color.White, 0.5f),
                //Visible = Broken
            };
            Star = GFX.Game[path + "star00"];
            Symbols.AddLoop("exclamation", "surprisedSymbol", 0.1f);
            Symbols.AddLoop("anger", "anger", 0.1f);
            Symbols.AddLoop("eughIdle", "eughSymbol", 0.1f, 3);
            Symbols.Add("eugh", "eughSymbol", 0.2f);
            OrbSprite.AddLoop("idle", "orbIdle", 0.1f);
            EyeSprite.AddLoop("neutral", "eyeFront", 0.1f);
            EyeSprite.AddLoop("happy", "eyeHappy", 0.1f);
            EyeSprite.AddLoop("stern", "eyeStern", 0.1f);
            EyeSprite.AddLoop("closed", "eyeClosed", 0.1f);
            EyeSprite.AddLoop("surprised", "eyeSurprised", 0.1f);
            EyeSprite.AddLoop("wink", "eyeWink", 0.1f);
            EyeSprite.AddLoop("dizzy", "eyeDizzy", 0.1f);
            for (int i = 0; i < 2; i++)
            {
                Arms[i].AddLoop("idle", "armSpinH", 0.1f, 0);
                Arms[i].Add("spinH", "armSpinH", 0.07f, "idle");
                Arms[i].Add("spinV", "armSpinV", 0.1f, "idle");
                Arms[i].FlipX = i == 1;
            }
            Symbols.Visible = false;
            BrokenParts.AddLoop("broken", "formation", 0.1f, 0);
            BrokenParts.AddLoop("jitter", "jitter", 0.05f);
            BrokenParts.Add("assemble", "formation", 0.12f);

            Add(BrokenParts, OrbSprite, Symbols, Arms[0], Arms[1], EyeSprite);
            CurrentMood = mood;
            Emotion(mood);

            Add(EmotionListener = new("Calidus", null, null, TextboxEmotion));
            StartFollowingImmediately = startFollowingImmediately;
        }
        public void TextboxEmotion(FancyText.Portrait portrait, string anim)
        {
            if (Enum.TryParse(anim, true, out Mood mood))
            {
                Emotion(mood);
            }
        }
        public void Nudge(Vector2 offset, float duration, Ease.Easer ease, bool yoyo)
        {
            nudgeTween?.RemoveSelf();
            NudgeOffset = Vector2.Zero;
            nudgeTween = Tween.Set(this, yoyo ? Tween.TweenMode.YoyoOneshot : Tween.TweenMode.Oneshot, duration, ease, t =>
            {
                NudgeOffset = t.Eased * offset;
            },
            t =>
            {
                if (yoyo) NudgeOffset = Vector2.Zero;
            });
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (RequireSpawnFlag && !SpawnData)
            {
                RemoveSelf();
                return;
            }
            /*            if (!Broken)
                        {
                            FixSelf();
                        }
                        else
                        {
                            BrokenParts.Play("broken");
                            Vector2 offset = new Vector2(BrokenParts.Width / 2, BrokenParts.Height / 2);
                            Position -= offset;
                            SpriteBox = new Hitbox(offset.X, offset.Y);
                        }*/
            FixSelf();

            OrigPosition = Position;
            scene.Add(system = new ParticleSystem(Depth + 1, 500));
            FollowTarget = PianoUtils.SeekController<CalidusFollowerTarget>(scene);
            if (FollowTarget is null)
            {
                scene.Add(FollowTarget = new CalidusFollowerTarget());
            }
        }
        public CalidusFollowerTarget FollowTarget;
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            RefreshIdleAnimationTimer();
            Collider collider = Collider ?? SpriteBox;
            Rectangle talkRect = new Rectangle((int)collider.Position.X, 0, (int)Width, (int)Height + 24);
            Vector2 drawAt = OrbSprite.Center.X * Vector2.UnitX;
            Add(Talk = new TalkComponent(talkRect, drawAt, Interact)
            {
                Enabled = CanTalk
            });

            foreach (Part part in Components.GetAll<Part>())
            {
                Parts.Add(part);
            }
            Parts.Remove(EyeSprite);
            if (scene.GetPlayer() is Player player)
            {
                FollowTarget.Initialize(player);
            }
            CreateFollower();
            if (StartFollowingImmediately)
            {
                StartFollowing(Looking.Dynamic);
            }
            foreach (CalCut c in RequiredWatched)
            {
                if (!c.GetCutsceneFlag())
                {
                    RemoveSelf();
                    return;
                }
            }
            foreach (CalCut c in ExcludedWatched)
            {
                if (c.GetCutsceneFlag())
                {
                    RemoveSelf();
                    return;
                }
            }
            if (Broken)
            {
                FallApart(true);
            }
            ExecuteArgs(AwakeArgs);
        }
        public void CreateFollower()
        {
            if (Follower == null)
            {
                Add(Follower = new Follower()
                {
                    FollowDelay = 0.2f,
                    MoveTowardsLeader = false,
                    PersistentFollow = false
                });
                if (FollowTarget.Leader == null)
                {
                    FollowTarget.Add(FollowTarget.Leader = new CalidusFollowerTarget.CustomLeader());
                }
            }
        }
        public void SnapToLeader()
        {
            Position = FollowTarget.Position;
        }

        public override void Removed(Scene scene)
        {
            // CatHelper.CatState(false);
            StopFollowing();
            base.Removed(scene);
            if (FollowTarget != null)
            {
                if (FollowTarget.Leader != null && Follower != null)
                {
                    FollowTarget.Leader.LoseFollower(Follower);
                }
                if (scene.Tracker.GetEntities<Calidus>().Count < 2)
                {
                    FollowTarget.RemoveSelf();
                }
            }
        }
        private float dynamicLookMultX;
        private float dynamicLookMultY;
        private float dynamicLookHoldTimer;
        public Vector2 IdleRoutineOffset;
        private Coroutine idleCoroutine;
        private Coroutine idleEndRoutine;
        private int IdleIndex;
        private Mood lastMood;
        private IEnumerator idleRoutines()
        {
            lastMood = CurrentMood;
            IdleIndex = 3;//Calc.Random.Range(0, 4);
            switch (IdleIndex)
            {
                case 0:
                    LookOverride = Looking.Left;
                    yield return 1;
                    LookOverride = Looking.Right;
                    yield return 3;
                    break;
                case 1:
                    LookOverride = Looking.Right;
                    yield return 1;
                    LookOverride = Looking.Left;
                    yield return 3;
                    break;
                case 2:
                    LookOverride = Looking.Player;
                    yield return 3;
                    if (CurrentMood == Mood.Normal && Calc.Random.Chance(0.3f))
                    {
                        CurrentMood = Mood.Happy;
                        yield return 1f;
                    }
                    break;
                case 3:
                    float time = Scene.TimeActive;
                    float xmult = 0;
                    float ymult = 0;
                    for (float i = 0; i < 1; i += Engine.DeltaTime / 1)
                    {
                        floatMult = Ease.SineInOut(1 - i);
                        yield return null;
                    }
                    floatMult = 0;
                    while (!CollideCheck<Solid>(Position + Vector2.UnitY * IdleRoutineOffset))
                    {
                        IdleRoutineOffset.X = (float)Math.Sin((Scene.TimeActive - time) * 3) * 8 * xmult;
                        IdleRoutineOffset.Y += 10 * Engine.DeltaTime * ymult;
                        ymult = Calc.Approach(ymult, 1, Engine.DeltaTime * 0.35f);
                        xmult = Calc.Approach(xmult, 1, Engine.DeltaTime * 0.6f);
                        yield return null;
                    }
                    yield return 1.4f;
                    if (EyeOffset.X > 0)
                    {
                        LookOverride = Looking.DownRight;
                    }
                    else if (EyeOffset.X < 0)
                    {
                        LookOverride = Looking.DownLeft;
                    }
                    else
                    {
                        LookOverride = Looking.Down;
                    }
                    CurrentMood = Mood.Closed;
                    yield return 1.3f;
                    EyeSprite.VisualOffsetApproachZero = false;
                    while (true)
                    {
                        for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
                        {
                            EyeSprite.VisualOffset.Y = Ease.SineIn(i) * -3f;
                            yield return null;
                        }
                        yield return 0.5f;
                        for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
                        {
                            EyeSprite.VisualOffset.Y = -3 + Ease.SineOut(i) * 3;
                            yield return null;
                        }
                    }
            }
            StopIdleAnimation();
        }

        private IEnumerator idleAnimationEnd()
        {
            switch (IdleIndex)
            {
                case 3:
                    Tween.Set(this, Tween.TweenMode.Oneshot, 0.4f, Ease.SineIn, t => floatMult = t.Eased);
                    CurrentMood = Mood.Surprised;
                    yield return 0.3f;
                    EyeSprite.VisualOffsetApproachZero = true;

                    break;
            }
            CurrentMood = lastMood;
            LookOverride = Looking.None;
            RefreshIdleAnimationTimer();
            yield return null;
        }
        public void StopIdleAnimation()
        {
            if (idleCoroutine.Active)
            {
                idleEndRoutine.Replace(idleAnimationEnd());
            }
            idleCoroutine.Cancel();
        }
        public void DoRandomIdleAnimation()
        {
            if (idleCoroutine.Active) return;
            if (idleEndRoutine.Active)
            {
                idleEndRoutine.Cancel();
            }
            idleCoroutine.Replace(idleRoutines());
        }
        public void RefreshIdleAnimationTimer()
        {
            dynamicLookHoldTimer = Calc.Random.Range(8, 15);
        }
        public bool CanTalk => TalkFlag.State && !string.IsNullOrEmpty(CutsceneID) && !BannedCutsceneIDs.Contains(CutsceneID);
        public override void Update()
        {
            if (Following && Follower.Leader.Entity is CalidusFollowerTarget)
            {
                if (Scene.GetPlayer() is Player player)
                {
                    if (player.Speed.Y < 0 || player.Speed.X != 0 || !(player.StateMachine.State is Player.StNormal or Player.StDummy))
                    {
                        if (idleCoroutine.Active)
                        {
                            StopIdleAnimation();
                        }
                        RefreshIdleAnimationTimer();
                    }
                    if (player.StateMachine.State is not Player.StDummy)
                    {
                        Vector2 input = new Vector2(Input.MoveX, Input.MoveY);
                        dynamicLookMultX = Calc.Approach(dynamicLookMultX, input.X == 0 ? 0f : 1, Engine.DeltaTime * 3);
                        dynamicLookMultY = Calc.Approach(dynamicLookMultY, input.Y == 0 ? 0f : 1, Engine.DeltaTime * 3);
                        if (player.DashAttacking)
                        {
                            dynamicLookMultX = dynamicLookMultY = 1;
                            input = player.DashDir;
                        }
                        if (input != Vector2.Zero)
                        {
                            if (idleCoroutine.Active)
                            {
                                StopIdleAnimation();
                            }
                            //downwards movement isn't used enough to warrent Calidus looking that direction during normal gameplay - plus it makes wavedashing look weird
                            float newY = Math.Min(0, input.Y);
                            dynamicLookOffset.X = Calc.Approach(dynamicLookOffset.X, input.X * 4 * dynamicLookMultX, 100 * Engine.DeltaTime);
                            dynamicLookOffset.Y = Calc.Approach(dynamicLookOffset.Y, newY * 4 * dynamicLookMultY, 100 * Engine.DeltaTime);

                            RefreshIdleAnimationTimer();
                        }
                        else
                        {
                            if (dynamicLookHoldTimer > 0)
                            {
                                dynamicLookHoldTimer -= Engine.DeltaTime;
                            }
                            else if (!idleCoroutine.Active && !idleEndRoutine.Active)
                            {
                                DoRandomIdleAnimation();
                            }
                        }
                    }

                }

            }
            if (!idleCoroutine.Active && IdleRoutineOffset != Vector2.Zero)
            {
                Vector2 prevIdleOffset = IdleRoutineOffset;
                IdleRoutineOffset -= IdleRoutineOffset * (1f - (float)Math.Pow(0.0099999997764825821, Engine.DeltaTime));
                if (Vector2.DistanceSquared(prevIdleOffset, IdleRoutineOffset) < 0.00001f)
                {
                    IdleRoutineOffset = Vector2.Zero;
                }
            }
            Talk.Enabled = CanTalk;
            if (Dizzy)
            {
                dizzyTimer -= Engine.DeltaTime;
                if (dizzyTimer <= 0)
                {
                    dizzyTimer = Calc.Random.Range(0.4f, 1.3f);
                    dizzyTarget = Calc.AngleToVector(Calc.Random.NextAngle(), Calc.Random.Range(2, 10));
                }
                dizzyOffset = Calc.Approach(dizzyOffset, dizzyTarget, Engine.DeltaTime * 10);
                sineYTarget = (float)Math.Sin(Scene.TimeActive * 7) * 16;
            }
            else
            {
                sineYTarget = Calc.Approach(sineYTarget, 0, Engine.DeltaTime);
                dizzyOffset = Calc.Approach(dizzyOffset, Vector2.Zero, Engine.DeltaTime);
            }
            if (ApproachScale != Vector2.One)
            {
                ApproachScale = Calc.Approach(ApproachScale, Vector2.One, Engine.DeltaTime * 2);
            }
            sineY = Calc.Approach(sineY, sineYTarget, Engine.DeltaTime * 6);
            if (Shaking)
            {
                shakeTimer -= Engine.DeltaTime;
                shakeVector = Calc.Random.ShakeVector();
            }
            else
            {
                shakeVector = Vector2.Zero;
            }
            FloatAmount = Calc.Approach(FloatAmount, CanFloat ? FloatTarget : 0, Engine.DeltaTime * 2);
            foreach (Part part in Parts)
            {
                part.Offset.Y = -FloatAmount;
            }
            EyeSprite.Offset.Y = -FloatAmount;
            Arms[0].Offset.X = ArmOffsets.Item1;
            Arms[1].Offset.X = ArmOffsets.Item2;
            //BrokenParts.Visible = Broken;
            //BrokenParts.OutlineOpacity = OutlineOpacity;
            base.Update();

            StarRotation = (RenderStar ? 0 : StarRotation + Engine.DeltaTime * 3) % 360;
            Arms[0].Scale = Arms[1].Scale = OrbSprite.Scale = Scale;
            EyeSprite.Scale = EyeScale;
            Vector2 centerOffset = GetLookOffset();
            if(LookDir is Looking.Down or Looking.DownRight or Looking.DownLeft && CurrentMood == Mood.Closed)
            {
                centerOffset.Y -= 2;
            }
            if (!AutoSetEyeOffset)
            {
                EyeOffset = !EyeInstant ? Calc.Approach(EyeOffset, centerOffset, LookSpeed) : centerOffset;
            }
            EyeSprite.Color = Blinking || ForceBlink ? Color.Lerp(Color.White, Color.Black, 0.3f) : Color.White;
            EyeSprite.RenderPosition = Calc.Approach(EyeSprite.RenderPosition, OrbSprite.RenderPosition + EyeOffset, LookSpeed);

            if (Collider != null)
            {
                foreach (CalidusCollider c in Scene.Tracker.GetComponents<CalidusCollider>())
                {
                    if (c.Check(this)) break;
                }
            }
        }
        public void AddAfterImage(float time = 1, float alpha = 0.8f)
        {
            Vector2 padding = Vector2.One * 4;
            Collider prev = Collider;
            Collider = SpriteBox;
            Action render = delegate { RenderAt((-SpriteBox.Position + padding).Floor()); };
            AfterImage image = new AfterImage(Collider.AbsolutePosition - padding, Width + padding.X * 2, Height + padding.Y * 2, render, time, alpha);
            Collider = prev;
            Scene.Add(image);
        }
        public void RenderAt(Vector2 position)
        {
            Vector2 prev = Position;
            Position = position;
            Render();
            Position = prev;
        }
        public override void Render()
        {
            Position += IdleRoutineOffset;
            foreach (Part p in Parts)
            {
                if (p.Visible)
                {
                    p.DrawOutline();
                }
            }
            if (Symbols.Visible)
            {
                Symbols.DrawSimpleOutline();
            }
            bool prev = EyeSprite.Visible;
            if (EyeSprite.Visible)
            {
                EyeSprite.DrawOutline();
            }
            if (EyeDepth == EyeDepths.Behind)
            {
                EyeSprite.Render();
                EyeSprite.Visible = false;
            }
            base.Render();
            EyeSprite.Visible = prev;
            /*            if (RenderStar)
                        {
                            Star.Draw(StarPos, Star.Center, Color.Yellow, Vector2.One, StarRotation);
                        }*/
            Position -= IdleRoutineOffset;
        }
        public CalCut DialogCutscene = CalCut.TalkAboutNote;
        private CutsceneEntity cutscene;
        public void Interact(Player player)
        {
            if (CutsceneIsDialog)
            {
                Scene.Add(new DialogCutsceneEntity(this, player));
            }
            if (EventTrigger.CutsceneLoaders.TryGetValue(CutsceneID, out EventTrigger.CutsceneLoader value))
            {
                Entity entity = value(null, player, CutsceneID);
                if (entity is CutsceneEntity)
                {
                    cutscene = entity as CutsceneEntity;
                }
                Scene.Add(entity);
            }
        }
        private Vector2 GetLookOffset()
        {
            if (LookEntity != null)
            {
                return RotatePoint(OrbSprite.Center.XComp(), Vector2.Zero, Calc.Angle(Center, LookEntity.Center).ToDeg());
            }
            Vector2 p = Center;
            if (Scene.GetPlayer() is Player player)
            {
                p = player.Center;
            }
            return LookDir switch
            {
                Looking.Left => -Vector2.UnitX * 4,
                Looking.Right => Vector2.UnitX * 4,
                Looking.Up => -Vector2.UnitY * 4,
                Looking.Down => Vector2.UnitY * 4,
                Looking.UpLeft => Vector2.One * -4,
                Looking.UpRight => new Vector2(4, -4),
                Looking.DownLeft => new Vector2(-4, 4),
                Looking.DownRight => Vector2.One * 4,
                Looking.Player => RotatePoint(OrbSprite.Center.XComp(), Vector2.Zero, Calc.Angle(Center, p).ToDeg()),
                Looking.Dynamic => dynamicLookOffset,
                Looking.Target => RotatePoint(OrbSprite.Center.XComp(), Vector2.Zero, Calc.Angle(Center, LookTarget).ToDeg()),
                Looking.Center => Vector2.Zero,
                _ => Vector2.Zero
            };
        }
        private Vector2 dynamicLookOffset;
        private Tween lightTween;
        public void FadeLight(float to, float time = 1)
        {
            lightTween?.Stop();
            lightTween?.RemoveSelf();
            float from = Light.Alpha;
            lightTween = Tween.Set(this, Tween.TweenMode.Oneshot, time, Ease.SineInOut, t => Light.Alpha = Calc.LerpClamp(from, to, t.Eased), t => Light.Alpha = to);

        }
        public void FixSelf()
        {
            bool fromBroken = BrokenParts.Visible;
            BrokenParts.Visible = false;
            OrbSprite.Play("idle");
            EyeSprite.Play("neutral");
            Arms[0].Play("idle");
            Arms[1].Play("idle");
            OrbSprite.Visible = EyeSprite.Visible = Arms[0].Visible = Arms[1].Visible = true;
            Arms[0].OnLastFrame = (string s) =>
            {
                if (!CanFloat) return;
                if (s == "idle")
                {
                    ArmLoopCount++;
                }
                else if (s == "spinH" || s == "spinV")
                {
                    Arms[1].Play(Calc.Random.Chance(0.5f) ? "spinH" : "spinV");
                }
                if (ArmLoopCount == ArmBuffer)
                {
                    Arms[0].Play(Calc.Random.Chance(0.5f) ? "spinH" : "spinV");
                    ArmLoopCount = 0;
                }
            };
            EyeSprite.Position += OrbSprite.Center + GetLookOffset();
            Arms[0].Position.X += -(Arms[0].Width + 1);
            Arms[1].Position.X += OrbSprite.Width;
            SpriteBox = new Hitbox(Arms[0].Width * 2 + 3 + OrbSprite.Width, OrbSprite.Height, Arms[0].X - 5);
            Collider = new Hitbox(OrbSprite.Width, OrbSprite.Height);
            Position -= new Vector2(6, 4);
            Add(new Coroutine(AddRoutines(fromBroken)));
        }
        private void PlayerFace(Player player)
        {
            player.Facing = Position.X > player.Position.X ? Facings.Right : Facings.Left;
        }
        public void FallApart(bool instant = false)
        {
            CanFloat = false;
            foreach (Part p in Parts)
            {
                p.Fall(16 + FloatTarget - Height, instant);
            }
            EyeSprite.Fall(16 + FloatTarget - Height, instant);
        }
        public void Reassemble()
        {
            Add(new Coroutine(ReassembleRoutine()));
        }
        public void ReturnParts(float speedMult = 1)
        {
            foreach (Part part in Parts)
            {
                part.Return(speedMult);
            }
        }
        public void ResetParts()
        {
            foreach (Part part in Parts)
            {
                part.Reset();
            }
        }
        public void LerpOutline(bool instant = false)
        {
            if (instant)
            {
                BrokenParts.Color = Color.White;
                OutlineOpacity = 1;
            }
            else
            {
                Tween Outline = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, 1);
                Outline.OnUpdate = (Tween t) =>
                {
                    BrokenParts.Color = Color.Lerp(Color.Black, Color.White, 0.5f + (t.Eased / 2));
                    OutlineOpacity = t.Eased;
                };
                Outline.OnComplete = delegate { BrokenParts.Color = Color.White; };
                Add(Outline);
                Outline.Start();
            }
        }
        public void FixSequence(bool instant = false)
        {
            if (instant)
            {
                FixSelf();
                Position += new Vector2(BrokenParts.Width / 2, BrokenParts.Height / 2);
                Broken = false;
                LerpOutline(true);
            }
            else
            {
                BrokenParts.Play("broken");
                Part Glint = new Part(GFX.Game, "characters/PuzzleIslandHelper/Calidus/");
                Glint.Add("shine", "brokenShine", 0.05f);
                Glint.Add("gleam", "brokenGleam", 0.1f, "shine");
                Add(Glint);

                Glint.Play("gleam");
                Glint.OnLastFrame = (string s) =>
                {
                    if (s == "shine")
                    {
                        BrokenParts.Play("assemble");
                        LerpOutline();
                        BrokenParts.OnLastFrame = (string s) =>
                        {
                            if (s == "assemble")
                            {
                                FixSelf();
                                Position += new Vector2(BrokenParts.Width / 2, BrokenParts.Height / 2);
                                Broken = false;
                            }
                        };
                        Glint.Stop();
                        Remove(Glint);

                    }
                };
            }
        }
        public void Eugh()
        {
            CurrentMood = Mood.Eugh;
            Symbols.Position = OrbSprite.Position + Vector2.One;
            Add(new Coroutine(EughRoutine()));
        }
        public void Wink()
        {
            CurrentMood = Mood.Wink;
            Add(new Coroutine(WinkRoutine()));
        }
        public void Surprised(bool useSymbol = true)
        {
            CurrentMood = Mood.Surprised;
            Symbols.Position = OrbSprite.Position + new Vector2(4, -11);
            Add(new Coroutine(SurprisedRoutine(useSymbol)));
        }
        public void Angry()
        {
            CurrentMood = Mood.Angry;
            Symbols.Position -= Vector2.One * 2;
            Add(new Coroutine(AngryRoutine()));
        }
        public void Happy()
        {
            if (EyeSprite.CurrentAnimationID != "happy") EyeSprite.Play("happy");
            Symbols.Visible = false;
            CurrentMood = Mood.Happy;
        }
        public void Stern()
        {
            if (EyeSprite.CurrentAnimationID != "stern") EyeSprite.Play("stern");
            Symbols.Visible = false;
            CurrentMood = Mood.Stern;

        }
        public void Normal()
        {
            if (EyeSprite.CurrentAnimationID != "neutral") EyeSprite.Play("neutral");
            Symbols.Visible = false;
            CurrentMood = Mood.Normal;
        }
        public void ShakeHead()
        {
            CurrentMood = Mood.Shakers;
            Add(new Coroutine(ShakeHeadRoutine(false)));
        }
        public void NodHead()
        {
            CurrentMood = Mood.Nodders;
            Add(new Coroutine(ShakeHeadRoutine(true)));
        }
        public void CloseEye()
        {
            if (EyeSprite.CurrentAnimationID != "closed") EyeSprite.Play("closed");
            CurrentMood = Mood.Closed;
        }
        public void RollEye()
        {
            CurrentMood = Mood.RollEye;
            Add(new Coroutine(RollEyeRoutine()));
        }
        public void Laugh()
        {
            CurrentMood = Mood.Laughing;
            Add(new Coroutine(LaughRoutine()));
        }
        public void DizzyEye()
        {
            CurrentMood = Mood.Dizzy;
            EyeSprite.Play("dizzy");
        }
        public void ShakeFor(float time)
        {
            shakeTimer = time;
        }
        public void StartShaking()
        {
            ForceShake = true;
        }
        public void StopShaking()
        {
            ForceShake = false;
            shakeTimer = 0;
        }
        private void HeeHeeParticles()
        {
            if (heeheeBuffer <= 0)
            {
                system.Emit(HeeHee, 1, TopCenter - Vector2.UnitY * 8, Vector2.UnitX * 4);
                heeheeBuffer = 1;
            }
            else
            {
                heeheeBuffer--;
            }
        }
        public void Emotion(int mood)
        {
            Action Action = mood switch
            {
                (int)Mood.Happy => Happy,
                (int)Mood.Stern => Stern,
                (int)Mood.Normal => Normal,
                (int)Mood.RollEye => RollEye,
                (int)Mood.Laughing => Laugh,
                (int)Mood.Shakers => ShakeHead,
                (int)Mood.Nodders => NodHead,
                (int)Mood.Closed => CloseEye,
                (int)Mood.Angry => Angry,
                (int)Mood.Surprised => delegate { Surprised(false); }
                ,
                (int)Mood.Wink => Wink,
                (int)Mood.Eugh => Eugh,
                _ => null
            };
            Action?.Invoke();
        }
        private bool ignoreRegularMoodSetters;
        public void Emotion(Mood mood)
        {
            Emotion((int)mood);
        }
        public IEnumerator DelayedEmotion(Mood emotion, float delay)
        {
            yield return delay;
            Emotion(emotion);
        }
        public void Emotion(string mood)
        {
            foreach (Mood @enum in Enum.GetValues(typeof(Mood)).Cast<Mood>())
            {
                string enumName = @enum.ToString();
                if (mood.Equals(enumName, StringComparison.OrdinalIgnoreCase))
                {
                    Emotion(@enum);
                    break;
                }
            }
        }
        public void Look(Looking dir)
        {
            LookDir = dir;
            LookEntity = null;
        }
        public void Look(Facings facing)
        {
            if (facing == Facings.Left) Look(Looking.Left);
            else Look(Looking.Right);
        }
        public void LookOpposite()
        {
            Look(GetOpposite(look));
        }
        public static Looking GetOpposite(Looking from)
        {
            return from switch
            {
                Looking.Left => Looking.Right,
                Looking.Right => Looking.Left,
                Looking.Up => Looking.Down,
                Looking.Down => Looking.Up,
                Looking.UpRight => Looking.DownLeft,
                Looking.UpLeft => Looking.DownRight,
                Looking.DownRight => Looking.UpLeft,
                Looking.DownLeft => Looking.UpRight,
                _ => from
            };
        }
        public void Look(string direction)
        {
            foreach (Looking @enum in Enum.GetValues(typeof(Looking)).Cast<Looking>())
            {
                string enumName = @enum.ToString();
                if (direction.Equals(enumName, StringComparison.OrdinalIgnoreCase))
                {
                    Look(@enum);
                    break;
                }
            }
        }
        public void LookAt(Entity entity)
        {
            LookEntity = entity;
        }
        public void LookAt(Vector2 position)
        {
            Look(Looking.Target);
            LookTarget = position;
        }
        public void StartFollowing(Looking look)
        {
            StopFollowing();
            if (!Following)
            {
                FollowTarget.Leader.GainFollower(Follower);
                FollowTarget.Leader.Snap(0.2f);
                Following = true;
                Follower.MoveTowardsLeader = true;
                followLook = look;
            }
            Persistent = true;
        }
        public void StartFollowing()
        {
            StartFollowing(Looking.Dynamic);
        }
        public void StopFollowing(bool keepPersistent = false)
        {
            if (Following)
            {
                Following = false;
                FollowTarget.Leader.LoseFollower(Follower);
                followLook = Looking.None;
            }
            if (!keepPersistent)
            {
                Persistent = false;
            }
        }
        public void MoveTo(Vector2 position)
        {
            Position = position;
        }
        public void FloatToLerp(Vector2 position, float time, Ease.Easer ease = null)
        {
            Add(new Coroutine(FloatToLerp(position, time, ease, true, true)));
        }
        public IEnumerator RotateAround(Vector2 center, float radius, float startingAngle, float endingAngle, float time, Ease.Easer ease = null)
        {
            ease ??= Ease.Linear;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                float angle = Calc.AngleLerp(startingAngle, endingAngle, ease(i));
                Vector2 offset = Calc.AngleToVector(angle, radius);
                Position = center + offset;
                yield return null;
            }
            Position = center + Calc.AngleToVector(endingAngle, radius);
        }
        public IEnumerator RotateAroundApproach(Vector2 center, float radius, float startingAngle, float endingAngle, float time, float speedMult = 1, Ease.Easer ease = null)
        {
            ease ??= Ease.Linear;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                float angle = Calc.AngleLerp(startingAngle, endingAngle, ease(i));
                Vector2 offset = Calc.AngleToVector(angle, radius);
                Position.X = Calc.Approach(Position.X, (center + offset).X, 90f * speedMult * Engine.DeltaTime);
                Position.Y = Calc.Approach(Position.Y, (center + offset).Y, 90f * speedMult * Engine.DeltaTime);
                yield return null;
            }
        }
        public IEnumerator RotateAroundOffset(Vector2 center, float radius, float angleOffset, float time, float speedMult = 1, Ease.Easer ease = null)
        {
            float startingAngle = (center - Position).Angle();
            float endingAngle = startingAngle + angleOffset;
            yield return new SwapImmediately(RotateAround(center, radius, startingAngle, endingAngle, time, ease));
        }
        public IEnumerator RotateAroundApproachOffset(Vector2 center, float radius, float angleOffset, float time, float speedMult = 1, Ease.Easer ease = null)
        {
            float startingAngle = (center - Position).Angle();
            float endingAngle = startingAngle + angleOffset;
            yield return new SwapImmediately(RotateAroundApproach(center, radius, startingAngle, endingAngle, time, speedMult, ease));
        }
        public IEnumerator FloatTo(Vector2 position, float speedmult = 1)
        {
            while (Math.Abs(X - position.X) > 2 || Math.Abs(Y - position.Y) > 2)
            {
                Position.X = Calc.Approach(Position.X, position.X, 90f * speedmult * Engine.DeltaTime);
                Position.Y = Calc.Approach(Position.Y, position.Y, 90f * speedmult * Engine.DeltaTime);
                yield return null;
            }
            Position = position;
        }
        public IEnumerator FloatTo(float x, float y, float speedmult = 1)
        {
            yield return new SwapImmediately(FloatTo(new Vector2(x, y), speedmult));
        }
        public IEnumerator FloatToX(float x, float speedmult = 1)
        {
            while (Math.Abs(X - x) > 2)
            {
                Position.X = Calc.Approach(Position.X, x, 90f * speedmult * Engine.DeltaTime);
                yield return null;
            }
            Position.X = x;
        }
        public IEnumerator FloatToY(float y, float speedmult = 1)
        {
            while (Math.Abs(Y - y) > 2)
            {
                Position.Y = Calc.Approach(Position.Y, y, 90f * speedmult * Engine.DeltaTime);
                yield return null;
            }
            Position.Y = y;
        }
        public IEnumerator FloatToXNaive(float x, float speedmult = 1)
        {
            yield return new SwapImmediately(FloatToX(X + x, speedmult));
        }
        public IEnumerator FloatToYNaive(float y, float speedmult = 1)
        {
            yield return new SwapImmediately(FloatToY(Y + y, speedmult));
        }
        public IEnumerator FloatToNaive(Vector2 position, float speedmult = 1)
        {
            yield return new SwapImmediately(FloatTo(Position + position, speedmult));
        }
        public IEnumerator FloatToNaive(float h, float v, float speedmult = 1)
        {
            yield return new SwapImmediately(FloatTo(Position + new Vector2(h, v), speedmult));
        }
        public IEnumerator FloatToXLerp(float x, float time, Ease.Easer ease)
        {
            yield return FloatToLerp(Vector2.UnitX * x, time, ease, true, false);
        }
        public IEnumerator FloatToYLerp(float y, float time, Ease.Easer ease)
        {
            yield return FloatToLerp(Vector2.UnitY * y, time, ease, false, true);
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (Persistent)
            {
                Draw.Circle(Position, 3, Color.White, 10);
            }
        }
        public bool AllPartsFallen()
        {
            foreach (Part p in Parts)
            {
                if (!p.OnGround) return false;
            }
            return true;
        }
        public bool AllPartsAssembled()
        {

            foreach (Part p in Parts)
            {
                if (!p.Assembled) return false;
            }
            return true;
        }
        static Vector2 RotatePoint(Vector2 pointToRotate, Vector2 centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Vector2
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }
        private IEnumerator AddRoutines(bool fromBroken)
        {
            if (fromBroken)
            {
                float y = Position.Y;
                for (float i = 0; i < 1; i += Engine.DeltaTime * 2)
                {
                    MoveTowardsY(y - 8, 8 * Engine.DeltaTime * 2);
                    yield return null;
                }
            }
            Alarm alarm = Alarm.Create(Alarm.AlarmMode.Looping,
                delegate { if (CanFloat) Add(new Coroutine(ColorFlash())); }, 2.5f, true);
            Add(alarm);
            Add(new Coroutine(FloatLoop()));
            Add(new Coroutine(ArmLoop()));
            Add(new Coroutine(Blink()));
            /*            if (!IsPlayer)
                        {
                            Add(new Coroutine(DuckBounceCheck()));
                        }*/
        }
        public IEnumerator FallApartRoutine()
        {
            FallApart();
            Add(new Coroutine(BlinkInterval(3, Engine.DeltaTime * 2, true)));
            while (!AllPartsFallen())
            {
                yield return null;
            }
            FallenApart = true;
            yield return null;
        }
        private IEnumerator BlinkInterval(int times, float interval, bool endState)
        {
            for (int i = 0; i < times; i++)
            {
                ForceBlink = !ForceBlink;
                yield return interval;
            }
            ForceBlink = endState;
        }
        private IEnumerator BlinkIntervalRandom(int times, bool endState)
        {
            for (int i = 0; i < times; i++)
            {
                ForceBlink = !ForceBlink;
                yield return Calc.Random.Range(Engine.DeltaTime, Engine.DeltaTime * 8);
            }
            ForceBlink = endState;
        }
        private IEnumerator SpriteBlink(Part sprite, int times, float interval, bool endState)
        {
            Color prev = sprite.Color;
            bool off = false;
            for (int i = 0; i < times; i++)
            {
                sprite.Color = Color.Lerp(prev, Color.Black, off ? 0.3f : 0);
                yield return interval;
            }
            sprite.Color = Color.Lerp(prev, Color.Black, endState ? 0.3f : 0);
        }
        public IEnumerator ReassembleRoutine()
        {
            Look(Looking.Center);
            ReturnParts();
            while (!AllPartsAssembled())
            {
                yield return null;
            }
            FallenApart = false;
            CanFloat = true;
            AutoSetEyeOffset = false;
            ForceBlink = false;
        }
        public IEnumerator WaitForReassemble()
        {
            while (FallenApart)
            {
                yield return null;
            }
        }
        public IEnumerator WaitForFallenApart()
        {
            while (!FallenApart)
            {
                yield return null;
            }
        }
        private IEnumerator EughRoutine()
        {

            EyeSprite.Play("stern");
            yield return 0.1f;
            Symbols.Visible = true;
            Symbols.Play("eugh");
            while (CurrentMood == Mood.Eugh || !Continue)
            {
                Symbols.Position = OrbSprite.Position + Vector2.One;
                yield return null;
            }
            Symbols.Visible = false;
            yield return null;
        }
        private IEnumerator WinkRoutine()
        {
            EyeSprite.Play("wink");

            RenderStar = true;
            int x = 10;
            int y = 8;
            StarPos = EyeSprite.Position + Position + Vector2.UnitX * 8;

            Vector2 start = StarPos;
            float speed = 4;
            for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
            {
                StarPos.X = Calc.LerpClamp(start.X, start.X + x / 2, i);
                StarPos.Y = Calc.LerpClamp(start.Y, start.Y - y / 2, Ease.SineInOut(i));
                yield return null;
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
            {
                StarPos.X = Calc.LerpClamp(start.X + x / 2, start.X + x, i);
                StarPos.Y = Calc.LerpClamp(start.Y - y / 2, start.Y + y, Ease.SineInOut(i));
                yield return null;
            }
            RenderStar = false;
            yield return null;
        }
        private IEnumerator SurprisedRoutine(bool useSymbol)
        {
            if (useSymbol)
            {
                Symbols.Visible = true;
                Symbols.Play("exclamation");
            }
            else
            {
                Symbols.Stop();
            }

            EyeSprite.Play("surprised");
            yield return 2f;
            Symbols.Stop();
            Symbols.Visible = false;
            while (CurrentMood == Mood.Surprised)
            {
                yield return null;
            }
            yield return null;
        }
        private IEnumerator AngryRoutine()
        {
            EyeInstant = true;
            Symbols.Visible = true;
            Symbols.Play("anger");
            float delay = 0.05f;
            while (CurrentMood == Mood.Angry && !AutoSetEyeOffset)
            {
                yield return 3;
                if (!(CurrentMood == Mood.Angry && !AutoSetEyeOffset)) break;
                for (int i = 0; i < 2; i++)
                {
                    EyeOffset.X = -1;
                    if (!(CurrentMood == Mood.Angry && !AutoSetEyeOffset))
                    {
                        EyeOffset.X = 0;
                        break;
                    }
                    yield return delay;
                    EyeOffset.X = 1;
                    yield return delay;
                }
                EyeOffset.X = 0;

            }
            EyeInstant = false;
            Symbols.Visible = false;
            yield return null;
        }
        private IEnumerator ShakeHeadRoutine(bool Nod)
        {
            float speed = 7;
            float amount = 1.5f;
            Looking prev = LookDir;
            AutoSetEyeOffset = false;
            Look(Looking.None);
            for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
            {
                if (Nod)
                {
                    EyeOffset.Y = Calc.LerpClamp(0, -amount, i);
                }
                else
                {
                    EyeOffset.X = Calc.LerpClamp(0, -amount, i);
                }
                yield return null;
            }
            for (int j = 0; j < 2; j++)
            {
                for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
                {
                    if (Nod)
                    {
                        EyeOffset.Y = Calc.LerpClamp(-amount, amount, i);
                    }
                    else
                    {
                        EyeOffset.X = Calc.LerpClamp(-amount, amount, i);
                    }
                    yield return null;
                }
                for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
                {
                    if (Nod)
                    {
                        EyeOffset.Y = Calc.LerpClamp(amount, -amount, i);
                    }
                    else
                    {
                        EyeOffset.X = Calc.LerpClamp(amount, -amount, i);
                    }
                    yield return null;
                }
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime * speed)
            {
                if (Nod)
                {
                    EyeOffset.Y = Calc.LerpClamp(EyeOffset.Y, 0, i);
                }
                else
                {
                    EyeOffset.X = Calc.LerpClamp(EyeOffset.X, 0, i);
                }
                yield return null;
            }
            while (Nod ? CurrentMood == Mood.Nodders : CurrentMood == Mood.Shakers)
            {
                yield return null;
            }
            Look(prev);
            yield return null;
        }
        private IEnumerator LaughRoutine()
        {
            EyeSprite.Play("happy");
            float delay = 0.1f;
            while (CurrentMood == Mood.Laughing && !AutoSetEyeOffset)
            {
                HeeHeeParticles();
                EyeOffset.Y = -2;
                yield return delay;
                EyeOffset.Y = 0;
                yield return delay;
            }
            yield return null;
        }
        public IEnumerator RollEyeRoutine()
        {
            AutoSetEyeOffset = true;
            for (float i = 0; i < 1; i += Engine.DeltaTime)
            {
                Vector2 Rotated = RotatePoint(Vector2.Zero, EyeSprite.Center, Ease.SineOut(i) * 90);
                EyeSprite.Position = Calc.Approach(EyeSprite.Position, Rotated, LookSpeed);
                yield return null;
            }
            while (CurrentMood == Mood.RollEye)
            {
                yield return null;
            }
            AutoSetEyeOffset = false;
            yield return null;
        }
        private IEnumerator ColorFlash()
        {

            float delay = Engine.DeltaTime * 2;
            float[] values = new float[] { 0.2f, 0.7f, 0.2f, 0 };
            for (int j = 0; j < ColorTweenLoops; j++)
            {
                foreach (float f in values)
                {
                    Arms[0].Color = Color.Lerp(Color.White, Color.Green, f);
                    Arms[1].Color = Color.Lerp(Color.White, Color.Green, f);
                    yield return delay;
                }
            }
        }
        private IEnumerator Blink()
        {
            while (true)
            {
                yield return Calc.Random.Range(1, 10f);
                Blinking = true;
                EyeSprite.Color = Color.Lerp(Color.White, Color.Black, 0.3f);

                yield return 0.1f;
                Blinking = false;
                EyeSprite.Color = Color.White;
            }
        }
        private IEnumerator DuckDuck()
        {
            int offset = 5;
            float Lookspeed = LookSpeed;
            LookSpeed = offset;
            if (CurrentMood == Mood.Angry)
            {
                Symbols.Visible = false;
            }
            for (int i = 0; i < 15; i++)
            {

                Scale = new Vector2(1, 0.6f);

                OrbSprite.RenderPosition += Vector2.UnitY * offset;
                EyeOffset.Y = 4;
                Arms[0].RenderPosition += Vector2.UnitY * offset;
                Arms[1].RenderPosition += Vector2.UnitY * offset;
                yield return 0.1f;
                Scale = Vector2.One;
                OrbSprite.RenderPosition -= Vector2.UnitY * offset;
                EyeOffset.Y = 0;
                Arms[0].RenderPosition -= Vector2.UnitY * offset;
                Arms[1].RenderPosition -= Vector2.UnitY * offset;
                yield return 0.1f;
            }
            if (CurrentMood == Mood.Angry)
            {
                Symbols.Visible = true;
            }
            LookSpeed = Lookspeed;
            //Emotion(m);
            yield return null;
        }
        private IEnumerator DuckBounceCheck()
        {
            while (true)
            {
                if (Scene.GetPlayer() is not Player player)
                {
                    yield return null;
                    continue;
                }
                int count = 0;
                bool wasUnducked = false;

                for (int i = 0; i < 300; i++)
                {
                    if (wasUnducked && player.Ducking)
                    {
                        count++;
                    }
                    wasUnducked = !player.Ducking;
                    if (count > 8)
                    {
                        yield return DuckDuck();
                        break;
                    }
                    yield return null;
                }
            }
        }
        public IEnumerator MoveToPlayerX(Player player, float offset = 0, bool updateTarget = true)
        {
            float target = player.Position.X + OrbSprite.Position.X + offset;

            while (Position.X != target)
            {
                if (updateTarget) target = player.Position.X + OrbSprite.Position.X + offset;
                Position.X = Calc.Approach(Position.X, target, 1);
                yield return null;
            }
        }
        public IEnumerator MoveToPlayerY(Player player, float offset = 0, bool updateTarget = true)
        {
            float target = player.Position.Y + offset;
            while (Position.Y != target)
            {
                if (updateTarget) target = player.Position.Y + offset;
                Position.Y = Calc.Approach(Position.Y, target, 1);
                yield return null;
            }
        }
        private IEnumerator FloatLoop()
        {
            float percent = 0.5f;
            float time = 1;
            float to = FloatHeight;
            while (true)
            {
                for (float i = percent; i < 1; i += Engine.DeltaTime / time)
                {
                    while (!CanFloat)
                    {
                        yield return null;
                    }
                    FloatTarget = Calc.LerpClamp(-to, to, Ease.SineInOut(i));
                    yield return null;
                }
                to = -to;
                percent = 0;
                time = 2;
            }
        }
        public IEnumerator FloatToLerp(Vector2 position, float time, Ease.Easer ease = null, bool x = true, bool y = true)
        {
            Vector2 from = Position;
            ease ??= Ease.Linear;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                Vector2 pos = Vector2.Lerp(from, position, ease(i));
                if (x) Position.X = pos.X;
                if (y) Position.Y = pos.Y;
                yield return null;
            }
            if (x) Position.X = position.X;
            if (y) Position.Y = position.Y;
        }
        private IEnumerator ArmLoop()
        {
            ArmOffsets = (0, 0);
            while (true)
            {
                if (CanFloat)
                {
                    Color target = Color.Lerp(Color.White, Color.Black, 0.2f);
                    float armPosX = ArmOffsets.Item1;
                    float armRightPosX = ArmOffsets.Item2;
                    float delay = Engine.DeltaTime / 2;
                    int armDistance = 1;
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        ArmOffsets.Item1 = (int)Math.Round(Calc.LerpClamp(armPosX, armPosX - armDistance, Ease.SineInOut(i)));
                        ArmOffsets.Item2 = (int)Math.Round(Calc.LerpClamp(armRightPosX, armRightPosX + armDistance, Ease.SineInOut(i)));
                        OrbSprite.Color = Color.Lerp(target, Color.White, Ease.SineInOut(i));
                        yield return delay;
                    }
                    armPosX = ArmOffsets.Item1;
                    armRightPosX = ArmOffsets.Item2;
                    for (float i = 0; i < 1; i += Engine.DeltaTime)
                    {
                        ArmOffsets.Item1 = (int)Math.Round(Calc.LerpClamp(armPosX, armPosX + armDistance, Ease.SineInOut(i)));
                        ArmOffsets.Item2 = (int)Math.Round(Calc.LerpClamp(armRightPosX, armRightPosX - armDistance, Ease.SineInOut(i)));
                        OrbSprite.Color = Color.Lerp(Color.White, target, Ease.SineInOut(i));
                        yield return delay;
                    }
                }
                else
                {
                    ArmOffsets = (0, 0);
                }
                yield return null;
            }
        }
        public IEnumerator FloatHeightTo(float to, float time)
        {
            float from = FloatHeight;
            for (float i = 0; i < 1; i += Engine.DeltaTime / time)
            {
                FloatHeight = Calc.LerpClamp(from, to, i);
                yield return null;
            }
            FloatHeight = to;
        }
        public void Hide(bool instant = false)
        {
            if (instant)
            {
                Visible = false;
                return;
            }
            DisplacementRenderer.Burst burst = SceneAs<Level>().Displacement.AddBurst(Center, 0.5f, 20, 0, 1, null, Ease.CubeOut);
            burstRoutine.Replace(ScaleToBurst(burst, true));
        }
        public void Reveal(bool instant = false)
        {
            if (instant)
            {
                Visible = true;
                return;
            }
            DisplacementRenderer.Burst burst = SceneAs<Level>().Displacement.AddBurst(Center, 0.5f, 20, 0, 1, null, Ease.CubeOut);
            burstRoutine.Replace(ScaleToBurst(burst, false));
        }
        public void RevealAfterTransition()
        {
            Add(new Coroutine(waitForTransitionThenReveal()));
        }
        private IEnumerator waitForTransitionThenReveal()
        {
            while ((Scene as Level).Transitioning)
            {
                yield return null;
            }
            yield return 0.2f;
            Reveal();
        }
        private IEnumerator ScaleToBurst(DisplacementRenderer.Burst burst, bool hiding)
        {
            Ease.Easer ease = burst.ScaleEaser ?? Ease.Linear;
            if (!hiding)
            {
                Visible = true;
            }
            UpdateScale(hiding ? Vector2.One : Vector2.Zero);
            for (float i = 0; i < 1; i += Engine.DeltaTime / burst.Duration)
            {
                burst.Position = Center;
                UpdateScale(Vector2.One * Calc.LerpClamp(0, 1, ease(hiding ? 1 - i : i)));
                yield return null;
            }
            UpdateScale(hiding ? Vector2.Zero : Vector2.One);
            if (hiding)
            {
                Visible = false;
            }
        }
        public void UpdateScale(Vector2 scale)
        {
            Scale = scale;
            foreach (Part p in Parts)
            {
                p.Scale = scale;
            }
        }
    }
}
