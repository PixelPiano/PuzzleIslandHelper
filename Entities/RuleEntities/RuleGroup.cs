using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.RuleEntities
{
    [CustomEntity("PuzzleIslandHelper/RuleGroup")]
    [Tracked]
    public class RuleGroup : Entity
    {
        
        public RuleGroup(Vector2 position) : base(position)
        {

        }
        public RuleGroup(EntityData data, Vector2 offset) : this(data.Position + offset)
        {

        }
    }
}
