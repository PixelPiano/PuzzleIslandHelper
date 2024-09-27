using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

// PuzzleIslandHelper.LabDoor
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/TouchSwitchVerifier")]
    [Tracked]
    public class TouchSwitchVerifier : TouchSwitch
    {
        
        public TouchSwitchVerifier(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
        }
    }
}
