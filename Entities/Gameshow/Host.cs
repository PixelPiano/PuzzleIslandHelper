using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Gameshow
{
    [CustomEntity("PuzzleIslandHelper/Host")]
    [Tracked]
    public class Host : Actor
    {
        public Host(Vector2 position) : base(position)
        {

        }
        public Host(EntityData data, Vector2 offset) : this(data.Position + offset)
        {

        }
    }
}
