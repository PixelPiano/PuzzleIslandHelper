using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora
{
    [CustomEntity("PuzzleIslandHelper/Flungus")]
    [Tracked]
    public class Flungus : Entity
    {
        public class Head
        {

        }
        public Flungus(EntityData data, Vector2 offset) : base(data.Position + offset)
        {

        }
    }
}
