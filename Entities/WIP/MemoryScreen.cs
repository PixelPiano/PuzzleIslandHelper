using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/MemoryScreen")]
    [Tracked]
    public class MemoryScreen : Actor
    {
        public enum Types
        {
            Patrol,
            Warp,
            Cutscene
        }
        public const int MaxLookDist = 70;
        public const float FloatDist = 2;
        public const string path = "objects/PuzzleIslandHelper/memoryScreen/";
        public static MTexture Frame = GFX.Game[path + "frame"];
        public Vector2 Offset;
        public Player Player;
        public const int StIdle = 0;
        public const int StPatrol = 1;
        public const int StRushing = 2;
        public const int StBreaking = 3;
        public const int StMalfunctioning = 4;
        public const int StDummy = 5;
        public Facings Facing;
        public StateMachine StateMachine;
        public Vector2 Speed;
        public Vector2 PatrolStart;
        public Vector2 PatrolEnd;
        private bool floating;
        public const float PatrolSpeed = 40f;
        public Vector2 NextPatrolPosition;
        public float Rotation;
        public Vector2 LineStart;
        public Vector2 LineEnd;
        public Vector2 RushTarget;
        public MemoryScreenSprite Sprite;
        public bool cyclePaused;
        private float height = Frame.Height - 4;
        public const float RushPauseTime = 0.2f;
        public const float RushRecoveryTime = 1;
        private Vector2 preRushPosition;
        public const float RushSpeed = 160f;
        private float patrolWaitTimer;
        private Vector2 prevPosition;
        public bool CycleComplete;
        public const int StSpinOut = 6;
        public int rotationMult = 1;
        public Vector2 hitNormal;
        public MemoryScreen(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Sprite = new MemoryScreenSprite();
            Add(Sprite);
            Collider = new Hitbox(3, Frame.Height - 4, Frame.Width / 2f, 2);

            StateMachine = new StateMachine(7);
            StateMachine.SetCallbacks(0, idleUpdate, null, idleBegin, idleEnd);
            StateMachine.SetCallbacks(1, patrolUpdate, patrolRoutine, patrolBegin, patrolEnd);
            StateMachine.SetCallbacks(2, rushUpdate, RushRoutine, rushBegin, rushEnd);
            //StateMachine.SetCallbacks(3, breakUpdate, null, breakBegin, breakEnd);
            //StateMachine.SetCallbacks(4, malfuncUpdate, null, malfuncBegin, malfuncEnd);
            StateMachine.SetCallbacks(5, dummyUpdate, null, dummyBegin, dummyEnd);
            StateMachine.SetCallbacks(6, spinUpdate, SpinOutRoutine, spinBegin, spinEnd);
            Add(StateMachine);
            StartCycle();
            NextPatrolPosition = PatrolStart = Position;
            prevPosition = Position - Vector2.UnitX;
            if (data.Nodes.Length > 0)
            {
                PatrolEnd = data.NodesOffset(offset)[0];
                NextPatrolPosition = PatrolEnd;
                StateMachine.State = StPatrol;
            }
            Tween floatTween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, 1, true);
            floatTween.OnUpdate = t =>
            {
                if (floating)
                {
                    Sprite.FloatOffset = Calc.Approach(Sprite.FloatOffset, Calc.LerpClamp(-FloatDist, FloatDist, t.Eased), 30f * Engine.DeltaTime);
                }
                else
                {
                    Sprite.FloatOffset = Calc.Approach(Sprite.FloatOffset, 0, Engine.DeltaTime);
                }
            };
            Add(floatTween);
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Player = scene.GetPlayer();
        }
        public override void Update()
        {
            base.Update();
            if (Collidable && Scene.GetPlayer() is Player player && !player.Dead && CollideLine(player))
            {
                player.Die(-Speed.Sign());
            }
            if (StateMachine.State != StRushing)
            {
                int sign = Math.Sign(Position.X - prevPosition.X);
                if (sign != 0) Facing = (Facings)sign;
            }
            Sprite.Effects = Facing == Facings.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            prevPosition = Position;

            Vector2 dif = Calc.AngleToVector(MathHelper.PiOver2 + Sprite.Rotation, height / 2);
            LineStart = Center + dif;
            LineEnd = Center - dif;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(PatrolEnd + Collider.AbsolutePosition - Position, Collider.Width, Collider.Height, Color.Blue);
            Draw.HollowRect(RushTarget, 8, 8, Color.Orange);
            if (StateMachine.State != StPatrol)
            {
                Draw.HollowRect(preRushPosition + Collider.AbsolutePosition - Position, Collider.Width, Collider.Height, Color.White * 0.7f);
            }
            Draw.Line(LineStart, LineEnd, Color.Magenta);
        }
        public void PauseCycle()
        {
            cyclePaused = true;
        }
        public void ResumeCycle()
        {
            cyclePaused = false;
        }
        public void StartCycle()
        {
            Add(new Coroutine(CycleRoutine()));
        }
        private void idleBegin()
        {
            floating = true;
        }
        private int idleUpdate()
        {
            if (Speed.X != 0)
            {
                Facing = (Facings)Math.Sign(Speed.X);
            }
            return 0;
        }
        private void idleEnd()
        {

        }
        private void patrolBegin()
        {
            floating = true;
            patrolWaitTimer = 1;
        }

        private IEnumerator patrolRoutine()
        {
            while (StateMachine.State == StPatrol)
            {
                while (Position != NextPatrolPosition)
                {
                    LineMoveTowardsX(NextPatrolPosition.X, PatrolSpeed * Engine.DeltaTime);
                    yield return null;
                }
                yield return 1;
                NextPatrolPosition = NextPatrolPosition == PatrolStart ? PatrolEnd : PatrolStart;
            }
        }
        private int patrolUpdate()
        {
            preRushPosition = Position;
            if (Player != null && !Player.Dead)
            {
                if ((Facing == Facings.Right && Player.Right > Right) || (Facing == Facings.Left && Player.Left < Left))
                {
                    if ((Player.Top <= Bottom && Player.Top >= Top) || (Player.Bottom >= Top && Player.Bottom <= Bottom))
                    {
                        if (Vector2.DistanceSquared(Center, Player.Center) < (MaxLookDist * MaxLookDist))
                        {
                            return StRushing;
                        }
                    }
                }
            }
            return 1;
        }
        private void patrolEnd()
        {
            patrolWaitTimer = 0;

        }
        private void rushBegin()
        {
            floating = false;
        }
        private void spinBegin() { }
        private void spinEnd() { }
        private int spinUpdate()
        {
            return StSpinOut;
        }
        private void OnRushCollideH(CollisionData hit)
        {
            hitNormal = -Vector2.UnitX * Math.Sign(Speed.X);
            StateMachine.ForceState(StSpinOut);
        }
        private void OnRushCollideV(CollisionData hit)
        {
            hitNormal = -Vector2.UnitY * Math.Sign(Speed.Y);
            StateMachine.ForceState(StSpinOut);
        }
        private IEnumerator SpinOutRoutine()
        {
            rotationMult *= -1;
            Speed = Vector2.Reflect(Speed.Sign(), hitNormal) * Calc.Abs(Speed) * 0.8f;
            Vector2 from = Speed;
            float dist = Vector2.Distance(from, Vector2.Zero);
            Vector2 prev = Position;
            while (Speed != Vector2.Zero)
            {
                float amount = (Vector2.Distance(Speed, Vector2.Zero) / dist);
                Speed = Calc.Approach(Speed, Vector2.Zero, 20f * Engine.DeltaTime);
                Sprite.Rotation += rotationMult * 0.1f * amount;
                LineMoveH(Speed.X * Engine.DeltaTime, OnRushCollideH);
                LineMoveV(Speed.Y * Engine.DeltaTime, OnRushCollideV);
                yield return null;
            }
            yield return 0.2f;
            Speed.X = 0;
            Speed.Y = 0;
            yield return ResetTo(preRushPosition, 2, StPatrol);
            yield return null;
        }
        public IEnumerator ResetTo(Vector2 position, float duration, int newState)
        {
            yield return null;
            Speed = Vector2.Zero;
            Collidable = false;
            if (duration <= 0)
            {
                Sprite.Rotation = 0;
                Position = position;
            }
            else
            {
                PauseCycle();
                while (!CycleComplete)
                {
                    yield return null;
                }
                float halfTime = duration / 2f;
                yield return Sprite.FadeOut(halfTime);
                Sprite.Rotation = 0;
                Position = position;
                yield return Sprite.FadeIn(halfTime);
            }
            Collidable = true;
            StateMachine.State = newState;
        }
        private IEnumerator RushRoutine()
        {
            Speed = Vector2.Zero;
            preRushPosition = Position;
            if (Scene.GetPlayer() is not Player player)
            {
                StateMachine.State = StIdle;
                yield break;
            }
            Vector2 target = Position;
            for (float i = 0; i < 1; i += Engine.DeltaTime / 0.6f)
            {
                RushTarget = target = player.Center - (Vector2.UnitY * Height / 2f);
                if (target.X > Position.X) Facing = Facings.Right;
                else Facing = Facings.Left;
                Vector2 vec = Facing == Facings.Left ? Center - target : target - Center;
                if (!player.Dead && Vector2.DistanceSquared(player.Center, Center) > 100)
                {
                    Sprite.Rotation = Calc.LerpClamp(Sprite.Rotation, vec.Angle(), i);
                }
                yield return null;
            }
            target.Y -= Height / 2;
            yield return RushPauseTime;
            Vector2 dir = Vector2.Normalize((target - Position));
            Speed = dir * RushSpeed / 4;
            Vector2 from = Position;
            float dist = Vector2.Distance(Position, target);
            float sq = dist * dist;
            while (Speed != Vector2.Zero)
            {
                if (Vector2.DistanceSquared(from, Position) < sq)
                {
                    Speed = Calc.Approach(Speed, dir * RushSpeed, 50f * Engine.DeltaTime);
                }
                else
                {
                    Speed = Calc.Approach(Speed, Vector2.Zero, 50f * Engine.DeltaTime);
                }
                LineMoveH(Speed.X * Engine.DeltaTime, OnRushCollideH);
                LineMoveV(Speed.Y * Engine.DeltaTime, OnRushCollideV);
                yield return null;
            }
            yield return RushRecoveryTime;
            yield return ResetTo(preRushPosition, 2, StPatrol);
            yield return null;
        }
        private int rushUpdate()
        {
            return StRushing;
        }
        private void rushEnd()
        {

        }
        private void dummyBegin()
        {
        }
        private int dummyUpdate()
        {
            return StDummy;
        }
        private void dummyEnd()
        {
        }
        public void AdvanceWindowFrame()
        {
            int frame = Sprite.nextWindowFrame;
            Sprite.nextWindowFrame++;
            Sprite.nextWindowFrame %= MemoryScreenSprite.Windows;
            Sprite.Window = GFX.Game[path + "scenes0" + frame];
            Sprite.NextWindow = GFX.Game[path + "scenes0" + Sprite.nextWindowFrame];
            Sprite.NextWindowAlpha = 0;
        }
        public IEnumerator CycleRoutine()
        {
            while (true)
            {
                while (cyclePaused)
                {
                    yield return null;
                }
                CycleComplete = false;
                for (float i = 0; i < 1; i += Engine.DeltaTime / 0.3f)
                {
                    Sprite.NextWindowAlpha = i;
                    yield return null;
                }
                AdvanceWindowFrame();
                CycleComplete = true;
            }

        }
        #region Move Overrides
        public bool LineMoveH(float moveH, Collision onCollide = null, Solid pusher = null)
        {
            movementCounter.X += moveH;
            int num = (int)Math.Round(movementCounter.X, MidpointRounding.ToEven);
            if (num != 0)
            {
                movementCounter.X -= num;
                return LineMoveHExact(num, onCollide, pusher);
            }

            return false;
        }

        public bool LineMoveV(float moveV, Collision onCollide = null, Solid pusher = null)
        {
            movementCounter.Y += moveV;
            int num = (int)Math.Round(movementCounter.Y, MidpointRounding.ToEven);
            if (num != 0)
            {
                movementCounter.Y -= num;
                return LineMoveVExact(num, onCollide, pusher);
            }

            return false;
        }

        public bool LineMoveHExact(int moveH, Collision onCollide = null, Solid pusher = null)
        {
            Vector2 targetPosition = Position + Vector2.UnitX * moveH;
            int num = Math.Sign(moveH);
            int num2 = 0;
            while (moveH != 0)
            {
                Solid solid = FirstLine<Solid>(Position + Vector2.UnitX * num);
                if (solid != null)
                {
                    movementCounter.X = 0f;
                    onCollide?.Invoke(new CollisionData
                    {
                        Direction = Vector2.UnitX * num,
                        Moved = Vector2.UnitX * num2,
                        TargetPosition = targetPosition,
                        Hit = solid,
                        Pusher = pusher
                    });
                    return true;
                }

                num2 += num;
                moveH -= num;
                base.X += num;
            }

            return false;
        }

        public bool LineMoveVExact(int moveV, Collision onCollide = null, Solid pusher = null)
        {
            Vector2 targetPosition = Position + Vector2.UnitY * moveV;
            int num = Math.Sign(moveV);
            int num2 = 0;
            while (moveV != 0)
            {
                Platform platform = FirstLine<Solid>(Position + Vector2.UnitY * num);
                CollisionData data;
                if (platform != null)
                {
                    movementCounter.Y = 0f;
                    if (onCollide != null)
                    {
                        data = new CollisionData
                        {
                            Direction = Vector2.UnitY * num,
                            Moved = Vector2.UnitY * num2,
                            TargetPosition = targetPosition,
                            Hit = platform,
                            Pusher = pusher
                        };
                        onCollide(data);
                    }

                    return true;
                }

                num2 += num;
                moveV -= num;
                Y += num;
            }

            return false;
        }

        public void LineMoveTowardsX(float targetX, float maxAmount, Collision onCollide = null)
        {
            float toX = Calc.Approach(ExactPosition.X, targetX, maxAmount);
            LineMoveToX(toX, onCollide);
        }

        public void LineMoveTowardsY(float targetY, float maxAmount, Collision onCollide = null)
        {
            float toY = Calc.Approach(ExactPosition.Y, targetY, maxAmount);
            LineMoveToY(toY, onCollide);
        }

        public void LineMoveToX(float toX, Collision onCollide = null)
        {
            LineMoveH((float)((double)toX - (double)Position.X - (double)movementCounter.X), onCollide);
        }

        public void LineMoveToY(float toY, Collision onCollide = null)
        {
            LineMoveV((float)((double)toY - (double)Position.Y - (double)movementCounter.Y), onCollide);
        }
        public T CollideFirstLine<T>() where T : Entity
        {
            return FirstLine(Scene.Tracker.Entities[typeof(T)]) as T;
        }
        public T FirstLine<T>(Vector2 at) where T : Entity
        {
            Vector2 from = Position;
            Position = at;
            foreach (Entity item in Scene.Tracker.Entities[typeof(T)])
            {
                if (CollideLine(item))
                {
                    Position = from;
                    return item as T;
                }
            }
            Position = from;
            return null;
        }
        public Entity FirstLine(IEnumerable<Entity> b)
        {
            foreach (Entity item in b)
            {
                if (CollideLine(item))
                {
                    return item;
                }
            }
            return null;
        }
        #endregion

        #region Collide Overrides
        public bool CollideLine(Entity entity)
        {
            if (!Collidable) return false;
            Vector2 dif = Calc.AngleToVector(MathHelper.PiOver2 + Sprite.Rotation, height / 2);
            LineStart = Center + dif;
            LineEnd = Center - dif;
            return Collide.CheckLine(entity, LineStart, LineEnd);
        }
        public bool CollideLineCheck<T>() where T : Entity
        {
            return CheckLine(Scene.Tracker.Entities[typeof(T)]);
        }
        public bool CheckLine(IEnumerable<Entity> b)
        {
            foreach (Entity item in b)
            {
                if (CollideLine(item))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
        public class MemoryScreenSprite : GraphicsComponent
        {
            public const int Windows = 8;
            public MTexture Window;
            public MTexture NextWindow;
            public int nextWindowFrame;
            public float NextWindowAlpha = 1;
            public float FloatOffset;
            public float FrameAlpha = 1;
            public float WindowAlpha = 1;
            public new Vector2 RenderPosition
            {
                get
                {
                    return ((base.Entity == null) ? Vector2.Zero : base.Entity.Position) + Position + Vector2.UnitY * FloatOffset;
                }
                set
                {
                    Position = value - ((base.Entity == null) ? Vector2.Zero : base.Entity.Position);
                }
            }

            public MemoryScreenSprite() : base(true)
            {
                Window = GFX.Game[path + "scenes00"];
                NextWindow = GFX.Game[path + "scenes01"];
                Origin = new Vector2(Frame.Width / 2, Frame.Height / 2);
            }
            public IEnumerator FadeOut(float duration)
            {
                float halfTime = duration / 2f;
                for (float i = 0; i < 1; i += Engine.DeltaTime / halfTime)
                {
                    WindowAlpha = 1 - i;
                    yield return null;
                }
                WindowAlpha = 0;
                for (float i = 0; i < 1; i += Engine.DeltaTime / halfTime)
                {
                    FrameAlpha = 1 - i;
                    yield return null;
                }
                FrameAlpha = 0;
            }
            public IEnumerator FadeIn(float duration)
            {
                float halfTime = duration / 2f;
                for (float i = 0; i < 1; i += Engine.DeltaTime / halfTime)
                {
                    FrameAlpha = i;
                    yield return null;
                }
                FrameAlpha = 1;
                for (float i = 0; i < 1; i += Engine.DeltaTime / halfTime)
                {
                    WindowAlpha = i;
                    yield return null;
                }
                WindowAlpha = 1;
            }
            public override void Render()
            {
                base.Render();
                Vector2 rP = RenderPosition + Origin;
                Draw.SpriteBatch.Draw(Frame.Texture.Texture_Safe, rP, null, Color * FrameAlpha, Rotation, Origin, Scale, Effects, 0);
                Draw.SpriteBatch.Draw(Window.Texture.Texture_Safe, rP, null, Color * WindowAlpha, Rotation, Origin, Scale, Effects, 0);
                Draw.SpriteBatch.Draw(NextWindow.Texture.Texture_Safe, rP, null, Color * NextWindowAlpha, Rotation, Origin, Scale, Effects, 0);
            }
        }
    }


}