using Celeste.Mod.PuzzleIslandHelper.Entities.Flora;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.CustomCalidusEntities
{
    [Tracked]
    public class CalidusFollowerTarget : Entity
    {
        public class CustomLeader : Leader
        {
            public float SnapTimer;
            public void Snap(float duration)
            {
                Vector2 position = Entity.Position + Position;
                for (int i = 0; i < PastPoints.Count; i++)
                {
                    PastPoints[i] = position;
                }
                SnapTimer = duration;
            }
            public override void Update()
            {
                base.Update();
                Vector2 vector = Entity.Position + Position;
                if (SnapTimer > 0 || (Scene.OnInterval(0.02f) && (PastPoints.Count == 0 || (vector - PastPoints[0]).Length() >= 3f)))
                {
                    PastPoints.Insert(0, vector);
                    if (PastPoints.Count > 350)
                    {
                        PastPoints.RemoveAt(PastPoints.Count - 1);
                    }
                }

                int num = 5;
                foreach (Follower follower in Followers)
                {
                    if (num >= PastPoints.Count)
                    {
                        break;
                    }

                    Vector2 vector2 = PastPoints[num];
                    if (follower.DelayTimer <= 0f && follower.MoveTowardsLeader)
                    {
                        if (SnapTimer > 0)
                        {
                            follower.Entity.Position = vector;
                        }
                        else
                        {
                            follower.Entity.Position = follower.Entity.Position + (vector2 - follower.Entity.Position) * (1f - (float)Math.Pow(0.0099999997764825821, Engine.DeltaTime));
                        }
                    }

                    num += 5;
                }
                if (SnapTimer > 0)
                {
                    SnapTimer -= Engine.DeltaTime;
                }
            }
        }
        public float XOffset;
        public Vector2 Offset;
        public Vector2 AdditionalOffset;
        public CustomLeader Leader;
        public Entity Follow;
        public CalidusFollowerTarget() : base()
        {
            Tag |= Tags.TransitionUpdate | Tags.Persistent | Tags.Global;
        }
        public void Initialize(Entity entity)
        {
            if (Follow == null && entity is Player)
            {
                Offset.X = -(int)(entity as Player).Facing * 10;
            }
            Follow = entity;
            Offset.Y = -20f;
            if (Leader == null)
            {
                Leader = new CustomLeader();
                Add(Leader);
            }
        }
        public static void SetOffset(Vector2 offset)
        {
            if (Engine.Scene is Level level)
            {
                var target = level.Tracker.GetEntity<CalidusFollowerTarget>();
                if (target != null)
                {
                    target.AdditionalOffset = offset;
                }
            }
        }
        public static void OffsetBy(Vector2 offset)
        {
            if (Engine.Scene is Level level)
            {
                var target = level.Tracker.GetEntity<CalidusFollowerTarget>();
                if (target != null)
                {
                    target.AdditionalOffset += offset;
                }
            }
        }
        public void Resume()
        {
            Active = true;
        }
        public void Pause()
        {
            Active = false;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Color color = Active ? Color.Magenta : Color.Gray;
            Draw.Point(Position - Vector2.UnitY, color);
            Draw.Point(Position + Vector2.UnitY, color);
            Draw.Point(Position - Vector2.UnitX, color);
            Draw.Point(Position + Vector2.UnitX, color);
        }
        public override void Update()
        {
            base.Update();
            if (Scene is not Level level || level.GetPlayer() is not Player player) return;
            if (Leader == null)
            {
                Initialize(player);
            }
            else
            {
                Follow = player;
                if (Math.Abs(player.Speed.X) > 0)
                {
                    Offset.X = Calc.Approach(Offset.X, -(int)player.Facing * 10, Engine.DeltaTime * 70f);
                }
                Position = new Vector2(Follow.CenterX, Follow.Top) + Offset + AdditionalOffset;
            }
        }
        [OnLoad]
        public static void Load()
        {
            Everest.Events.Player.OnDie += Player_OnDie;
            Everest.Events.Player.OnSpawn += Player_OnSpawn;
        }

        private static bool spawnCalidusOnPlayerSpawn;
        private static void Player_OnSpawn(Player obj)
        {
            if (spawnCalidusOnPlayerSpawn)
            {
                AddCalidusFollower();
                spawnCalidusOnPlayerSpawn = false;
            }
        }

        private static void Player_OnDie(Player obj)
        {
            Calidus calidus = obj.Scene.Tracker.GetEntity<Calidus>();
            if (calidus != null && calidus.Following && calidus.FollowTarget != null && calidus.FollowTarget.Follow == obj)
            {
                spawnCalidusOnPlayerSpawn = true;
            }
        }

        [OnUnload]
        public static void Unload()
        {
            Everest.Events.Player.OnDie -= Player_OnDie;
            Everest.Events.Player.OnSpawn -= Player_OnSpawn;
        }
        [Command("add_calidus_follower", "adds a good boy to follow you and look at you")]
        public static void AddCalidusFollower()
        {
            if (Engine.Scene is not Level level) return;
            if (level.Tracker.GetEntity<CalidusFollowerTarget>() is CalidusFollowerTarget target)
            {
                AddCalidusFollower(target);
            }
            else if (level.GetPlayer() is Player player)
            {
                AddCalidusFollower(player);
            }
        }
        public static void AddCalidusFollower(Player player)
        {
            Calidus.CreateAndFollow(player);
        }
        public static void AddCalidusFollower(CalidusFollowerTarget target)
        {
            Calidus c = Calidus.Create();
            c.FollowTarget = target;
            c.CreateFollower();
            c.Position = target.Position;
            c.StartFollowing(Calidus.Looking.Player);
            target.Leader.Snap(0.5f);
        }
    }
}
