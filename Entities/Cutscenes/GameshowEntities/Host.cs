using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Cutscenes.GameshowEntities
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
