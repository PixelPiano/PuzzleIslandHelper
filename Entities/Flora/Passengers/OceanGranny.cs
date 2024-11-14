using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/Passengers/OceanGranny")]
    [Tracked]
    public class OceanGranny : OldPassenger
    {
        public OceanGranny(EntityData data, Vector2 offset) : base(data, offset)
        {
        }
    }
}
