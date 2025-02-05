using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/NoPowerDoor")]
    [Tracked]
    public class NoPowerDoor : Solid
    {
        public NoPowerDoor(EntityData data, Vector2 offset) : base(data.Position + offset, 8, data.Height, false)
        {

        }
    }
}