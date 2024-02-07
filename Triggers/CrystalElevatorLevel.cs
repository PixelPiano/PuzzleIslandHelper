using Celeste.Mod.CommunalHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using ExtendedVariants.Variants;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/CrystalElevatorLevel")]
    [Tracked]
    public class CrystalElevatorLevel : Trigger
    {
        public List<string> Flags = new();
        public bool Passed;
        public bool OnLevel;
        public int FloorNum;

        public CrystalElevatorLevel(EntityData data, Vector2 offset)
    : base(data, offset)
        {
            Flags = data.Attr("flags").Replace(" ","").Split(',').ToList();
        }
    }
}
