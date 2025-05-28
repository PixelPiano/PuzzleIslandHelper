using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// PuzzleIslandHelper.SecurityLaser
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/CalidusGuard")]
    [Tracked]
    public class CalidusGuard : Entity
    {
        public CalidusGuard(EntityData data, Vector2 offset) : base(data.Position + offset)
        {

        }
    }
}