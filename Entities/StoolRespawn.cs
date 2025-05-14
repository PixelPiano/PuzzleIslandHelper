using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/StoolRespawn")]
    public class StoolRespawn : Entity
    {
        public StoolRespawn(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            Depth = -100000;
            Collider = new Hitbox(data.Width, 8);
            MTexture tex = GFX.Game["objects/PuzzleIslandHelper/stool/spawnTex"];
            if (tex != null)
            {
                Add(new Image(tex.GetSubtexture(0, 0, 8, 8)));
                for (int x = 8; x < Width - 8; x += 8)
                {
                    Add(new Image(tex.GetSubtexture(8, 0, 8, 8)) { X = x });
                }
                Add(new Image(tex.GetSubtexture(16, 0, 8, 8)){X = Width - 8});
            }
        }
    }
}