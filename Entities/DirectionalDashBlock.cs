using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DirectionalDashBlock")]
    [TrackedAs(typeof(DashBlock))]
    public class DirectionalDashBlock : DashBlock
    {
        public enum Directions
        {
            Left,
            Right,
            Up,
            Down
        }
        public Directions Direction;
        public FlagList FlagOnBreak;
        public DirectionalDashBlock(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
        {
            OnDashCollide = NewOnDashed;
            Direction = data.Enum<Directions>("direction");
            FlagOnBreak = data.FlagList("flagOnBreak");
        }
        public DashCollisionResults NewOnDashed(Player player, Vector2 direction)
        { 
            if (!canDash && player.StateMachine.State != 5 && player.StateMachine.State != 10)
            {
                return DashCollisionResults.NormalCollision;
            }
            else
            {
                bool validDir = Direction switch
                {
                    Directions.Left => direction.X < 0,
                    Directions.Right => direction.X > 0,
                    Directions.Up => direction.Y < 0,
                    Directions.Down => direction.Y > 0,
                    _ => false
                };
                if (!validDir)
                {
                    return DashCollisionResults.NormalCollision;
                }
            }
            FlagOnBreak.State = true;
            Break(player.Center, direction, true, true);
            return DashCollisionResults.Rebound;
        }

    }
}