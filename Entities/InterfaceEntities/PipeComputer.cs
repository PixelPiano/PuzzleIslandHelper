using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities
{
    [CustomEntity("PuzzleIslandHelper/PipeComputer")]
    [TrackedAs(typeof(Machine))]
    public class PipeComputer : Machine
    {
        public PipeComputer(EntityData data, Vector2 offset) : base(data.Position + offset, "objects/PuzzleIslandHelper/interface/pipes/machine", Color.Orange)
        {
        }
        public override IEnumerator OnBegin(Player player)
        {
            Interface.StartWithPreset("Pipes");
            yield return base.OnBegin(player);
        }
    }
}