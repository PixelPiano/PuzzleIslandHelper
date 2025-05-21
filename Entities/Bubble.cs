//PuzzleIslandHelper.CustomWater
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    public class BubbleParticleSystem : ParticleSystem
    {
        public BubbleParticleSystem(int depth, int maxParticles) : base(depth, maxParticles)
        {
            Tag = Tags.Persistent | Tags.TransitionUpdate | Tags.Global;
        }
    }
    [CustomEntity("PuzzleIslandHelper/Bubble")]
    [Tracked]
    public class Bubble : Actor
    {
        private bool reactsToPipes;
        public enum BubbleType
        {
            FloatDown,
            Straight,
            FullControl
        }
        private bool InHold;
        private float HoldScale = 1;
        private ParticleType P_Trail = new ParticleType
        {
            Source = GFX.Game["objects/PuzzleIslandHelper/particles/bubbleSmall"],
            Size = 0.8f,
            SizeRange = 0.3f,
            SpeedMin = 20,
            SpeedMax = 40,
            Direction = 90f.ToRad(),
            DirectionRange = 15f.ToRad(),
            LifeMin = 1f,
            LifeMax = 2f,
            SpeedMultiplier = 0.6f,
            FadeMode = ParticleType.FadeModes.Late,
            Friction = 0.1f,
            Acceleration = new Vector2(2, -10),
            ScaleOut = true

        };
        private ParticleType P_Jump = new ParticleType
        {
            Source = GFX.Game["objects/PuzzleIslandHelper/particles/bubbleSmall"],
            Size = 1f,
            SpeedMin = 100,
            SpeedMax = 100,
            LifeMin = 0.5f,
            LifeMax = 1f,
            SpeedMultiplier = 1.4f,
            FadeMode = ParticleType.FadeModes.Late,
            Friction = 0.1f,
            ScaleOut = true
        };
        private ParticleType P_Pop = new ParticleType
        {
            Source = GFX.Game["objects/PuzzleIslandHelper/particles/bubbleSmall"],
            Size = 1f,
            SizeRange = 0.1f,
            SpeedMin = 10,
            SpeedMax = 20,
            LifeMin = 1f,
            LifeMax = 3f,
            FadeMode = ParticleType.FadeModes.None,
            Friction = 0.1f
        };
        private ParticleType P_Respawn = new ParticleType
        {
            Source = GFX.Game["objects/PuzzleIslandHelper/particles/bubbleSmall"],
            SpinMin = 0.3f,
            SpinMax = 1,
            Size = 0.4f,
            SpeedMin = 50,
            SpeedMax = 50,
            LifeMin = 0.6f,
            LifeMax = 0.6f,
            FadeMode = ParticleType.FadeModes.Late,
        };
        private const float HoldTime = 4;
        private Color BubbleColor = Color.White;
        public BubbleType Type;
        public int Layers = 1;
        private Player player;
        public static readonly float NormalSpeed = 80f;
        public static readonly float DashSpeed = 120f;
        public static readonly Vector2 playerOffset = new Vector2(0, 2);
        private Sprite Sprite;
        private Image Flash;
        private float FlashMult;
        public bool InBubble => Moving || InHold;
        public float InBubbleTimer;
        public bool Moving;
        public Action OnRemoved;
        private Vector2 AimDir = Vector2.Zero;
        public Vector2 Speed;
        public float BaseSpeed => baseSpeed - speedDecrease;
        private float fullControlXMult => AimDir.X == 0 ? 1.3f : 1;
        private float fullControlYMult => AimDir.Y == 0 ? 1.4f : 1;
        public Vector2 AimSpeed => BaseSpeed * AimDir;
        public Vector2 VectorSpeed
        {
            get
            {
                return Type switch
                {
                    BubbleType.FloatDown => AimSpeed * new Vector2(speedLerp + xOffset, speedLerp + yOffset),
                    BubbleType.FullControl => AimSpeed * new Vector2(fullControlXMult, fullControlYMult),
                    _ => AimSpeed,
                } + Speed;
            }
        }
        private float baseSpeed;
        private bool popping;
        private bool CanHold;
        private bool popped;
        private float xOffset = 0;
        private float yOffset = 0;
        private float speedLerp = 1;
        private bool PassThru;
        private float AimXMult;
        private float AimYMult;
        public const float JumpHeight = 150f;
        private int Jumps = 3;
        private float jumpTimer = jumpDelay;
        private const float jumpDelay = 0.1f;
        private SineWave floatDown;
        private bool Respawns;
        private Vector2 _pos;
        private int _layers;
        private bool Respawning;
        private Wiggler wiggler;
        private float scaleMult = 1;
        private float shrinkMult = 1;
        private float jumpScale = 0;
        private bool InWater
        {
            get
            {
                return CollideCheck<Water>();
            }
        }
        private float speedDecrease = 0f;
        private const float Slowed = 50f;
        public Bubble(EntityData data, Vector2 offset)
        : this(data.Position + offset - Vector2.One * 4, false, data.Int("layers", 1),
               data.Enum<BubbleType>("bubbleType"), data.Bool("noCollision"), data.Bool("respawns"))
        {
            reactsToPipes = data.Bool("onlyOnPipesBroken");
            Add(new Coroutine(IdleFloat()));
        }
        public Bubble(Vector2 position, bool immediate, int layers, BubbleType type, bool passThroughSolids = false, bool respawns = false) : base(position)
        {
            _pos = position;
            Moving = immediate;
            Respawns = respawns;
            Layers = _layers = layers;
            Type = type;
            PassThru = passThroughSolids;

            Depth = -9999;
            Collider = new Hitbox(16, 16, 8, 8);

            BubbleColor = type switch
            {
                BubbleType.Straight => Color.White,
                BubbleType.FullControl => Color.OrangeRed,
                BubbleType.FloatDown => Color.DeepPink,
                _ => Color.White
            };
            AimXMult = type switch
            {
                BubbleType.FullControl => 0.8f,
                BubbleType.FloatDown => 0.7f,
                _ => 1
            };
            AimYMult = type switch
            {
                BubbleType.FullControl => 0.8f,
                BubbleType.FloatDown => 1f,
                _ => 1
            };
            CreateSprites();
            Add(new VertexLight(Vector2.One * 8 + Collider.HalfSize, Color.White, 1f, 16, 32));
            Add(new BloomPoint(Vector2.One * 8 + Collider.HalfSize, 0.1f, 16f));
            Add(wiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                scaleMult = 1f + f * 0.25f;
            }));
            Add(new DashListener(OnPlayerDashed));
            Add(new MirrorReflection());

            Add(new PlayerCollider(OnPlayer));
            Add(new Coroutine(HoldUpdate()));
        }

        public void OnPlayerDashed(Vector2 direction)
        {
            if (InBubble)
            {
                if (InWater)
                {
                    StartMove(player, true);
                }
                else
                {
                    Pop();
                }
            }
        }
        public void OnPlayer(Player player)
        {
            if (!InBubble && !Respawning)
            {
                StartMoving(player);
            }
            if (InBubble && player.CollideAll<Bubble>().Count > 1)
            {
                Pop();
            }
        }
        private IEnumerator HoldUpdate()
        {

            while (true)
            {
                if (!InBubble || !CanHold)
                {
                    yield return null;
                    continue;
                }
                for (float i = 0; i < HoldTime; i += Engine.DeltaTime)
                {
                    if (!InHold)
                    {
                        i = 0;
                    }
                    HoldScale = Calc.LerpClamp(1, 0.7f, Ease.SineIn(i / HoldTime));
                    yield return null;
                }
                Pop();
                yield return null;
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (reactsToPipes && PianoModule.Session.GetPipeState() < 2)
            {
                RemoveSelf();
            }
            if (PianoUtils.SeekController<BubbleParticleSystem>(scene) == null)
            {
                scene.Add(PianoModule.Session.BubbleSystem = new BubbleParticleSystem(0, 500));
            }
            player = scene.Tracker.GetEntity<Player>();
            if (Moving && player != null)
            {
                StartMoving(player, true);
            }
        }
        private void StartMoving(Player player, bool dashing)
        {
            Add(new Coroutine(MoveRoutine(player, dashing)));
        }
        private void StartMoving(Player player)
        {
            bool dashing = player.StateMachine.State == Player.StDash || player.StartedDashing || player.DashAttacking;
            Add(new Coroutine(MoveRoutine(player, dashing)));
        }
        public IEnumerator MoveRoutine(Player player, bool dashing)
        {
            player.MuffleLanding = true;
            if (dashing)
            {
                StartMove(player, true);
            }
            else if (CanHold)
            {
                StartHold(player);
            }
            else
            {
                StartMove(player, false);
            }
            while (InBubble)
            {
                Tag = Tags.Persistent | Tags.TransitionUpdate;
                yield return null;
                while (SceneAs<Level>().Transitioning)
                {
                    Respawns = false;
                    Position = player.Position + new Vector2(-16, -23);
                    yield return null;
                }
                if (Moving)
                {
                    Speed.Y = Calc.Approach(Speed.Y, 0, 450f * Engine.DeltaTime);
                    Speed.X = Calc.Approach(Speed.X, 0, 300f * Engine.DeltaTime);
                    jumpTimer += Engine.DeltaTime;

                    if (Type != BubbleType.Straight)
                    {
                        AimDir.X = Calc.Approach(AimDir.X, Input.MoveX.Value * AimXMult, Engine.DeltaTime);
                        AimDir.Y = Calc.Approach(AimDir.Y, Input.MoveY.Value * AimYMult, Engine.DeltaTime);
                    }
                }
                if (player.Dead || CollideCheck<Spikes>() || CollideCheck<PipeSpout>() || CollideCheck<CrystalStaticSpinner>())
                {
                    if (!popping)
                    {
                        Sprite.Scale.Y = 1;
                        Sprite.Play("pop");
                        Pop();
                        yield return null;
                        continue;
                    }
                }
                if (!popping)
                {
                    if (!popped)
                    {
                        if (Scene.OnInterval(10f / 60f) && Moving && VectorSpeed.Length() > 20f)
                        {
                            TrailParticles();
                        }

                        if (jumpTimer > jumpDelay && Input.Jump.Pressed)
                        {
                            jumpTimer = 0;
                            Jump();
                        }
                    }
                    if (!InHold || !CanHold)
                    {
                        Vector2 speed = VectorSpeed * Engine.DeltaTime;
                        MoveH(speed.X, OnCollideH);
                        MoveV(speed.Y, OnCollideV);
                    }
                }
                if (PassThru)
                {
                    NoCollisionUpdate(player);
                }
                if (player is not null)
                {
                    PlayerUpdate(player);
                }
            }
            Tag = 0;
        }

        public override void Update()
        {
            jumpScale = Calc.Approach(jumpScale, 0, Engine.DeltaTime * 1f);
            Flash.Scale = Sprite.Scale = (Vector2.One * scaleMult - Vector2.UnitY * jumpScale) * HoldScale * Ease.SineIn(shrinkMult);
            FlashMult = Calc.Approach(FlashMult, 0, Engine.DeltaTime * 2);
            Flash.Color = BubbleColor * FlashMult;
            speedDecrease = Calc.Approach(speedDecrease, InWater ? Slowed : 0, 2);

            base.Update();
            if (InBubble && !popping && !popped)
            {
                InBubbleTimer += Engine.DeltaTime;
                /*                if (InBubbleTimer > shrinkThreshold)
                                {
                                    shrinkMult = Calc.Approach(shrinkMult, 0, Engine.DeltaTime * 0.5f);
                                }*/
                if (player is not null && !player.Dead)
                {
                    player.Sprite.Scale = Vector2.One;
                    player.DummyAutoAnimate = false;
                    player.DummyGravity = false;
                }
            }
            else
            {
                InBubbleTimer = 0;
            }
        }
        private void Respawn()
        {
            shrinkMult = 1;
            InBubbleTimer = 0;
            Respawning = false;
            Flash.Scale.Y = Sprite.Scale.Y = 1;
            FlashMult = 1;
            Flash.Color = BubbleColor * FlashMult;
            Speed = Vector2.Zero;
            jumpTimer = 0;
            speedLerp = 1;
            xOffset = 0;
            yOffset = 0;
            Sprite.Visible = true;
            Sprite.Play("idle");
            Flash.Visible = true;
            Collidable = true;
            Position = _pos;
            Layers = _layers;
            Jumps = 3;
            Moving = false;
            InHold = false;
            popping = false;
            popped = false;
            wiggler.Start();
            for (float j = 0; j <= 360; j += 45)
            {
                PianoModule.Session.BubbleSystem.Emit(P_Respawn, 1, Center, Vector2.Zero, BubbleColor, j.ToRad());
            }
        }
        private IEnumerator RespawnRoutine()
        {
            yield return 0.1f;
            if (!Respawning)
            {
                Respawning = true;
                Sprite.Visible = false;
                Flash.Visible = false;
                Collidable = false;
                Moving = false;
                InHold = false;
                yield return 2.5f;
                Respawn();
            }
        }
        private void CreateSprites()
        {
            Add(Sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/bubble/"));
            Sprite.AddLoop("idle", "bubble", 0.1f);
            Sprite.Add("pop", "bubblePop", 0.1f);
            Sprite.CenterOrigin();
            Sprite.Position += new Vector2(Sprite.Width / 2 + 4, Sprite.Height / 2 + 4);
            Sprite.Play("idle");
            Sprite.Color = BubbleColor;

            Sprite.OnFinish = (s) =>
                {
                    if (s == "pop")
                    {
                        popped = true;
                        if (Respawns)
                        {
                            Add(new Coroutine(RespawnRoutine()));
                        }
                        else
                        {
                            RemoveSelf();
                        }

                    }
                };

            Add(Flash = new Image(GFX.Game["objects/PuzzleIslandHelper/bubble/solid"]));
            Flash.Color = BubbleColor * 0;
            Flash.CenterOrigin();
            Flash.Position += new Vector2(Flash.Width / 2 + 4, Flash.Height / 2 + 4);

        }
        private void StartMove(Player player, bool dashing)
        {
            InHold = false;
            player.DummyAutoAnimate = false;
            player.DummyGravity = false;
            AimDir = dashing ? player.DashDir : Vector2.Zero;
            baseSpeed = dashing ? DashSpeed : NormalSpeed;
            if (Type == BubbleType.FullControl)
            {
                baseSpeed = DashSpeed;
            }
            if (Type == BubbleType.FloatDown)
            {
                Add(new Coroutine(SpeedRoutine()));
            }
            wiggler.Start();

            Moving = true;
        }
        private void StartHold(Player player)
        {
            Moving = false;
            player.DummyAutoAnimate = false;
            player.DummyGravity = false;
            AimDir = Vector2.Zero;
            baseSpeed = 0;
            wiggler.Start();
            InHold = true;

        }
        private void TrailParticles()
        {
            PianoModule.Session.BubbleSystem.Emit(P_Trail, 1, Center - (VectorSpeed * Engine.DeltaTime) / 2, Vector2.One, BubbleColor * 0.2f);
        }
        private void JumpParticles()
        {

            PianoModule.Session.BubbleSystem.Emit(P_Jump, 1, BottomRight, Vector2.Zero, BubbleColor * 0.7f, 45f.ToRad());
            PianoModule.Session.BubbleSystem.Emit(P_Jump, 1, BottomCenter, Vector2.Zero, BubbleColor * 0.7f, 90f.ToRad());
            PianoModule.Session.BubbleSystem.Emit(P_Jump, 1, BottomLeft, Vector2.Zero, BubbleColor * 0.7f, 135f.ToRad());

        }
        private void PopParticles()
        {

            for (float j = 0; j <= 360; j += 45)
            {
                PianoModule.Session.BubbleSystem.Emit(P_Pop, 1, PianoUtils.RotatePoint(Center + Vector2.One * 6, Center, j), Vector2.One * 0.5f, BubbleColor, j.ToRad());
            }

        }

        private IEnumerator IdleFloat()
        {
            yield break;
            /*int YAmount = 4;
            while (true)
            {
                idleFloat = 0;
                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    if (Respawning || Moving)
                    {
                        idleFloat = 0;
                        break;
                    }
                    idleFloat = Calc.LerpClamp(-YAmount, YAmount, Ease.SineInOut(i));
                    yield return null;
                }
                if (!(Respawning || Moving))
                {
                    idleFloat = YAmount;
                }

                for (float i = 0; i < 1; i += Engine.DeltaTime)
                {
                    if (Respawning || Moving)
                    {
                        idleFloat = 0;
                        break;
                    }
                    idleFloat = Calc.LerpClamp(YAmount, -YAmount, Ease.SineInOut(i));
                    yield return null;
                }
                yield return null;
            }*/
        }
        public void NoCollisionUpdate(Player player)
        {
            if (InBubble)
            {
                player.DummyAutoAnimate = false;
                player.DummyGravity = false;
                player.Collidable = false;
            }
            else if (popping || popped)
            {
                player.Collidable = true;
                if (!player.Dead && player.CollideCheck<Solid>())
                {
                    player.Die(AimDir);
                }
            }
        }
        private void Jump()
        {
            if (Jumps > 0)
            {
                jumpScale = 0.4f;
                Jumps--;
                Speed.Y -= JumpHeight;
                FlashMult = 1;
                JumpParticles();
                if (Type == BubbleType.Straight && AimDir.Y == 1)
                {
                    AimDir.Y = 0;
                }
            }
            else
            {
                Pop();
            }
        }
        private void PlayerUpdate(Player player)
        {
            if (!popping && !popped)
            {
                player.DummyAutoAnimate = false;
                player.DummyGravity = false;
                Vector2 vector2 = new Vector2((int)Center.X - (int)player.Collider.Center.X + (int)playerOffset.X, (int)Center.Y - (int)player.Collider.Center.Y + (int)playerOffset.Y);
                if (!player.DashAttacking)
                {
                    player.RefillDash();
                }
                player.RefillStamina();

                float speed = CanHold && InHold ? int.MaxValue : BaseSpeed;
                player.Speed = Vector2.Zero;
                player.MoveTowardsX((int)Math.Round(vector2.X), speed);
                player.MoveTowardsY((int)Math.Round(vector2.Y), speed);
            }
            else if (!popped)
            {
                popped = true;
                if (player is not null && !player.Dead)
                {
                    player.DummyAutoAnimate = true;
                    if (!player.DashAttacking)
                    {
                        player.Bounce(player.Center.Y);
                    }
                }
                Sprite.Play("pop");
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Player player = scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                player.StateMachine.State = Player.StNormal;
            }
            OnRemoved?.Invoke();
        }
        private void OnCollideH(CollisionData data)
        {
            if (data.Hit is Solid && !PassThru)
            {
                BreakLayer();
            }
        }
        private void OnCollideV(CollisionData data)
        {
            if (data.Hit is Solid && !PassThru)
            {
                BreakLayer();
            }
        }
        private void Pop()
        {
            popping = true;
            Player player = Scene.Tracker.GetEntity<Player>();
            player.MuffleLanding = false;
            if (player != null)
            {
                player.MuffleLanding = false;
                if (!player.DashAttacking)
                {
                    SceneAs<Level>().Displacement.AddBurst(Center, 0.2f, 8f, 32f, 0.4f, Ease.QuadOut, Ease.QuadOut);
                }
            }
            P_Pop.Acceleration = AimSpeed * Engine.DeltaTime;
            PopParticles();
            Tag = 0;
        }
        private void BreakLayer()
        {
            Layers--;
            if (Layers <= 0)
            {
                Moving = false;
                InHold = false;
                Pop();
            }
        }
        private IEnumerator FloatDown()
        {
            yOffset = 30f;
            floatDown = new SineWave(1);
            Add(floatDown);
            while (true)
            {
                xOffset = (int)Math.Round(floatDown.Value * 44);
                yield return null;
            }
        }
        private IEnumerator SpeedRoutine()
        {
            float duration = 3;
            bool added = false;
            for (float i = 0; i < duration; i += Engine.DeltaTime)
            {
                speedLerp = 1 - Ease.SineOut(i / duration);
                yield return null;
                if (i > duration - duration / 3f && !added)
                {
                    added = true;
                    Add(new Coroutine(FloatDown()));
                }
            }
        }
    }
}
