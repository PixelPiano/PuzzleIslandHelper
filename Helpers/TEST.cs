using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;

namespace Celeste.Mod.PuzzleIslandHelper.Helpers
{
    [CustomEntity("PuzzleIslandHelper/TEST")]
    [Tracked]
    public class TEST : Entity
    {
        public TEST(Vector2 position) : base(position)
        {

        }
        public TEST(EntityData data, Vector2 offset) : this(data.Position + offset)
        {

        }
    }
}
