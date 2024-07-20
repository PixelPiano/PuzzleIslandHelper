using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Marker")]
    [Tracked]
    public class Marker : Entity
    {
        public string ID;
        public static MTexture Texture => GFX.Game["objects/PuzzleIslandHelper/marker/lonn"];
        public Marker(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            ID = data.Attr("markerID");
            Collider = new Hitbox(Texture.Width, Texture.Height);
        }
        public override void DebugRender(Camera camera)
        {
            Texture.Draw(Position);
        }
    }
}