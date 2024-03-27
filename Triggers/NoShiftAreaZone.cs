using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.GameplayEntities.GearEntities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/NoShiftAreaZone")]
    [Tracked]
    public class NoShiftAreaZone : Trigger
    {
        public NoShiftAreaZone(EntityData data, Vector2 offset) : base(data, offset)
        {

        }
    }
}
