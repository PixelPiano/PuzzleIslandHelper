using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
