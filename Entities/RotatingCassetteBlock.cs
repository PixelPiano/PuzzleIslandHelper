using System.Collections;
using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CommunalHelper.Entities;

[CustomEntity("PuzzleIslandHelper/RotatingCassetteBlock")]
public class RotatingCassetteBlock : CustomCassetteBlock
{

    public RotatingCassetteBlock(Vector2 position, EntityID id, int width, int height, int index, float tempo, Color? overrideColor)
        : base(position, id, width, height, index, tempo, dynamicHitbox: true, overrideColor)
    {
    }

    public RotatingCassetteBlock(EntityData data, Vector2 offset, EntityID id)
        : this(data.Position + offset, id, data.Width, data.Height, data.Int("index"), data.Float("tempo", 1f), data.HexColorNullable("customColor"))
    {
    }

    public override void Render()
    {
        Position += Shake;
        base.Render();
        Position -= Shake;
    }
    private void ShakeSfx()
    {
        Audio.Play(SFX.game_gen_fallblock_shake, Center);
    }

    private void ImpactSfx()
    {
        Audio.Play(SFX.game_gen_fallblock_impact, BottomCenter);
    }
}
