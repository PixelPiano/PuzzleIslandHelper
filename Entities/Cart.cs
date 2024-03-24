using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Minecart")]
    [Tracked]
    public class Cart : Solid
    {
        private Image cart;
        private Sprite wheels;
        public bool Moving;
        public Collider WheelCollider;
        public Solid L;
        public Solid R;
        public int Dir;
        public Rail Rail;
        private int WheelExtend = 4;
        public bool SpeedingUp;
        public const float RideSpeed = 180f;
        public const float DashBoost = 35f;
        private int SideWidth = 3;
        private float TargetSpeedX;
        private float collideTimer;
        private ParticleType Sparks = new ParticleType()
        {
            Size = 1,
            SizeRange = 1,
            Color = Color.Orange,
            Color2 = Color.OrangeRed,
            DirectionRange = 1f.ToRad(),
            SpeedMin = 30,
            SpeedMax = 50,

            FadeMode = ParticleType.FadeModes.Linear,
            SpeedMultiplier = 1.1f,
            ColorMode = ParticleType.ColorModes.Choose,
            LifeMin = 1f,
            LifeMax = 3f,
            Acceleration = -Vector2.UnitY * 10,
        };
        private ParticleSystem SparkSystem;
        public Cart(Vector2 position) : base(position, 18, 2, false)
        {
            Depth = -1;
            cart = new Image(GFX.Game["objects/PuzzleIslandHelper/minecart/cart00"]);

            WheelCollider = new Hitbox(Width + 2, 8, 0, 6);
            L = new Solid(Position, SideWidth, 10, false);
            R = new Solid(Position + Vector2.UnitX * (Width - SideWidth + 2), SideWidth, 10, false);
            Collider = new Hitbox(Width + 2, SideWidth, 0, 7);
            wheels = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/minecart/wheels");
            wheels.AddLoop("spinning", "", 0.1f);
            wheels.AddLoop("idle", "", 0.1f, 0);
            wheels.Play("idle");
            wheels.Position = WheelCollider.Position;
            wheels.Visible = false;
            Add(cart, wheels);
            Add(new DashListener(OnDash));
        }
        public void OnDash(Vector2 dir)
        {
            Player player = SceneAs<Level>().GetPlayer();
            if (player is null)
            {
                return;
            }
            if (HasPlayerOnTop())
            {
                Dir = dir.X != 0 && dir.Y >= 0 ? Math.Sign(dir.X) : 0;
                if (!Moving)
                {
                    if (Dir != 0)
                    {
                        TargetSpeedX = RideSpeed;
                        StartRide(player);
                    }
                }
                else if (SpeedingUp)
                {
                    Speed.X += dir.X * (45f + DashBoost);
                    TargetSpeedX = Calc.Min(TargetSpeedX + 45f, RideSpeed * 2);
                }
            }
            else
            {
                TargetSpeedX = 0;
            }
        }

        private void EmitSparks(int dir)
        {
            if (dir == 0)
            {
                return;
            }
            Vector2 pos = dir < 0 ? WheelCollider.BottomRight - Vector2.UnitX * (WheelExtend + 4) : WheelCollider.BottomLeft + Vector2.UnitX * (WheelExtend + 4);
            SparkSystem.Emit(Sparks, pos, dir > 0 ? 45f.ToRad() : 135f.ToRad());
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            SparkSystem = new ParticleSystem(Depth - 1, 1000);
            scene.Add(SparkSystem);
            Rail = (scene as Level).Tracker.GetEntity<Rail>();
            scene.Add(L, R);
            if (Rail is null)
            {
                RemoveSelf();
            }
        }
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Remove(SparkSystem);
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(WheelCollider, Color.Yellow);
        }
        public override void Render()
        {
            cart.DrawSimpleOutline();
            base.Render();
            wheels.DrawSimpleOutline();
            wheels.Render();
        }
        public override void Update()
        {
            base.Update();
            L.Position = Position;
            R.Position = Position + Vector2.UnitX * (Width - SideWidth);
            wheels.Rate = Speed.X / (RideSpeed / 2);
            Position = Position.ToInt();
            WheelCollider.Position = Position + Vector2.UnitY * 6;
            collideTimer = Calc.Approach(collideTimer, 0, Engine.DeltaTime);
            if (collideTimer <= 0)
            {
                L.Collidable = R.Collidable = Collidable = true;
            }
            if (Moving && Math.Abs(Speed.X) > RideSpeed)
            {
                EmitSparks(Dir);
            }
            if (Moving && HasPlayerOnTop())
            {
                float adjust = Dir == -1 ? -1 : 1;
                Player player = (Scene as Level).GetPlayer();
                if (player is null || player.Dead)
                {
                    return;
                }
                player.MoveToX((int)(CenterX + adjust));
            }
        }
        public IEnumerator MovingShakeRoutine()
        {
            while (Moving)
            {
                yield return ShakeCart(Vector2.UnitY, 2, 0.1f, 1);
            }
            yield return null;
        }
        public IEnumerator ImpactShake()
        {
            yield return ShakeCart(Vector2.UnitX, 4, 0.1f, 2);
        }
        public IEnumerator ShakeCart(Vector2 direction, int loops, float delay, float distance)
        {
            for (int i = 0; i < loops; i++)
            {
                if (i == 0 || i == loops - 1)
                {
                    cart.Position += direction * (distance / 2);
                    wheels.Position += direction * (distance / 2);
                }
                else
                {
                    cart.Position += direction * distance;
                    wheels.Position += direction * distance;
                }
                direction = -direction;
                yield return delay;
            }
            yield return null;
        }
        public bool RailFound(int Direction)
        {
            if (Direction != 0)
            {
                float x = Direction > 0 ? WheelCollider.AbsoluteRight + WheelExtend :
                    Direction < 0 ? WheelCollider.AbsoluteLeft - WheelExtend : 0;
                if (x == 0)
                {
                    return false;
                }
                return Collide.CheckPoint(Rail, new Vector2(x, WheelCollider.Bottom));
            }
            return false;
        }
        private IEnumerator RideRoutine(Player player)
        {
            Level level = SceneAs<Level>();
            Moving = true;
            wheels.Play("spinning");
            SpeedingUp = true;
            Add(new Coroutine(MovingShakeRoutine()));
            Add(new Coroutine(PanCamera()));
            float playerLerp = 0;
            while (RailFound(Dir))
            {
                if (Math.Abs(Speed.X) > TargetSpeedX)
                {
                    SpeedingUp = false;
                }
                if (HasPlayerOnTop())
                {
                    Speed.X = Calc.Approach(Speed.X, Dir * TargetSpeedX, 120f * Engine.DeltaTime);
                }
                else
                {
                    Speed.X -= Dir * 5f;
                }
                if (HasPlayerOnTop())
                {
                    //float adjust = Dir == -1 ? -2 : 1;
                    //player.MoveTowardsX((int)(CenterX + adjust), (int)Speed.X);
                    //player.CenterX = Calc.LerpClamp(player.CenterX, CenterX, playerLerp);
                    //player.Speed = Speed;
                }
                if (Dir == 1 && Speed.X <= 0 || Dir == -1 && Speed.X >= 0)
                {
                    StopRide();
                    yield break;
                }
                playerLerp += Engine.DeltaTime;
                yield return null;
            }
            SpeedingUp = false;
            if (Math.Abs(Speed.X) > Math.Abs(RideSpeed))
            {
                TossPlayer(Dir, player);
                Add(new Coroutine(ImpactShake()));
            }
            Speed.X = -Speed.X * 0.2f;
            yield return 0.1f;
            Speed.X = -Speed.X;
            yield return 0.1f;
            StopRide();
            yield return null;
        }
        public IEnumerator PanCamera(Ease.Easer ease = null)
        {
            if (ease == null)
            {
                ease = Ease.CubeInOut;
            }
            Level level = SceneAs<Level>();
            int offset = 80;
            Vector2 prev = level.CameraOffset;
            float lerp = 0;
            while (Moving)
            {
                yield return null;
                level.CameraOffset = Dir * (Vector2.UnitX * offset) * ease(Math.Min(lerp, 1f));
                lerp += Engine.DeltaTime / 2;
            }
            for (float i = 0; i < 1; i += Engine.DeltaTime / 2)
            {
                level.CameraOffset = Vector2.Lerp(level.CameraOffset, prev, ease(i));
            }
            level.CameraOffset = prev;
        }
        public void TossPlayer(int dir, Player player)
        {
            if (player is null || !HasPlayerOnTop())
            {
                return;
            }
            if (dir > 0)
            {
                R.Collidable = false;
                collideTimer = 0.2f;
            }
            else if (dir < 0)
            {
                L.Collidable = false;
                collideTimer = 0.2f;
            }

            player.Speed.X = dir * 340f;
            player.Speed.Y = -130f;
            player.varJumpSpeed = Speed.Y;
            player.varJumpTimer = 0.15f;
            player.AutoJump = true;
            player.AutoJumpTimer = 0f;
            player.dashAttackTimer = 0f;
            player.gliderBoostTimer = 0f;
            player.wallSlideTimer = 1.2f;
            player.wallBoostTimer = 0f;
            player.launched = true;
            player.lowFrictionStopTimer = 0.15f;
            player.forceMoveXTimer = 0f;
            player.StateMachine.State = 0;
        }
        public void StartRide(Player player)
        {
            Moving = true;
            wheels.Rate = 0;
            Add(new Coroutine(RideRoutine(player)));
        }
        public void StopRide()
        {
            Moving = false;
            SpeedingUp = false;
            wheels.Play("idle");
            wheels.Rate = 1;
            Speed = Vector2.Zero;
        }
        public Cart(EntityData data, Vector2 offset) : this(data.Position + offset)
        {

        }
    }
}
