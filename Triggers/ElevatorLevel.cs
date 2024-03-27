using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/ElevatorLevel")]
    [Tracked]
    public class ElevatorLevel : Trigger
    {
        public int Floor;
        public string ElevatorID;
        public ElevatorLevel(EntityData data, Vector2 offset)
    : base(data, offset)
        {
            Floor = data.Int("floorNumber");
            ElevatorID = data.Attr("elevatorID");
        }
    }
}
