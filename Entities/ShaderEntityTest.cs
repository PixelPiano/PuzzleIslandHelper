using Celeste.Mod.Entities;
using FMOD;
using FrostHelper;
using FrostHelper.ModIntegration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

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

