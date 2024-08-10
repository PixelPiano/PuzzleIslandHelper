using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/ShaderEntityTest")]
    [TrackedAs(typeof(ShaderEntity))]
    public class ShaderEntityTest : ShaderEntity
    {
        public static MTexture Texture => GFX.Game["objects/PuzzleIslandHelper/invert/glassOrbFilled"];
        public ShaderEntityTest(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Attr("shaderPath"), Texture.Width, Texture.Height)
        {

        }
    }
}

