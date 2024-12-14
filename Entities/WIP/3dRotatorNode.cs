using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
// PuzzleIslandHelper.SubWarp
namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/3dRotatorNode")]
    public class Rotator3DNode : Entity
    {
        public Rotator3DNode(EntityData data, Vector2 offset) : base(data.Position + offset)
        {

        }
    }
}