using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Loaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.Flora.Passengers
{
    [CustomEntity("PuzzleIslandHelper/PassengerMapProcessorDummy")]
    [Tracked]
    public class PMPDummy : Entity
    {
        public PMPDummy(EntityData data, Vector2 offset) : base(data.Position + offset)
        {

        }
    }
}