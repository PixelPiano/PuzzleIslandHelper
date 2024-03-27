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
