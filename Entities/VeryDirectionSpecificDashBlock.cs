using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/VeryDirectionSpecificDashBlock")]
    [TrackedAs(typeof(DashBlock))]
    public class VeryDirectionSpecificDashBlock : DashBlock
    {
        private readonly Dictionary<Vector2, FlagList> Flags = [];
        private FlagList FlagOnBreak;
        public VeryDirectionSpecificDashBlock(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
        {
            FlagOnBreak = data.FlagList("flagOnBreak");
            Flags.Add(Vector2.UnitY, data.FlagList("downFlag"));
            Flags.Add(Vector2.UnitX, data.FlagList("rightFlag"));
            Flags.Add(-Vector2.UnitY, data.FlagList("upFlag"));
            Flags.Add(-Vector2.UnitX, data.FlagList("leftFlag"));
            Flags.Add(Vector2.One, data.FlagList("downRightFlag"));
            Flags.Add(-Vector2.One, data.FlagList("upLeftFlag"));
            Flags.Add(new(1, -1), data.FlagList("upRightFlag"));
            Flags.Add(new(-1, 1), data.FlagList("downLeftFlag"));
            OnDashCollide = (p, d) =>
            {
                if ((Flags.ContainsKey(d) && !Flags[d]) || (!canDash && p.StateMachine.State != 5 && p.StateMachine.State != 10))
                {
                    return DashCollisionResults.NormalCollision;
                }
                Break(p.Center, d, true, true);
                FlagOnBreak.State = true;
                return DashCollisionResults.Rebound;
            };
        }
    }
}