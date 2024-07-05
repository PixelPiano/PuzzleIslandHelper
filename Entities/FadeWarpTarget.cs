using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
// PuzzleIslandHelper.FadeWarpTarget
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/FadeWarpTarget")]
    [Tracked]
    public class FadeWarpTarget : Entity
    {
        public string id;
        public bool onGround;
        public FadeWarpTarget(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            id = data.Attr("targetId");
            onGround = data.Bool("placePlayerOnGroundBelow");
            Collider = new Hitbox(16, 24,0,-5);
        }
    }
}