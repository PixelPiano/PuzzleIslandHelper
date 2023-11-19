using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

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
