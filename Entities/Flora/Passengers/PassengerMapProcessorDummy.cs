using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/PassengerMapProcessorDummy")]
    [Tracked]
    internal class PMPDummy : Entity
    {
        public PMPDummy(EntityData data, Vector2 offset) : base(data.Position + offset)
        {

        }
    }
}