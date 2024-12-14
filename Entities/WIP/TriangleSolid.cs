using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.PuzzleIslandHelper.Entities.WIP
{
    [CustomEntity("PuzzleIslandHelper/TriangleSolid")]
    [Tracked]

    public class TriangleSolid : Solid
    {
        public TriangleSolid(EntityData data, Vector2 offset) : base(data.Position + offset, 8, 8, true)
        {

        }
    }
}