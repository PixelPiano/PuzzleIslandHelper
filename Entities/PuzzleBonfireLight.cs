// PuzzleIslandHelper.PuzzleBonfireLight
using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

[CustomEntity("PuzzleIslandHelper/PuzzleBonfireLight")]
[Tracked]
public class LabFloorLight : Entity
{
    private Sprite sprite;
    public LabFloorLight(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        Depth = -100000;
        Tag |= Tags.TransitionUpdate;
        Add(sprite = new Sprite(GFX.Game, "objects/PuzzleIslandHelper/puzzleBonfireLight/"));
        sprite.Add("idleOn", "lightAnim", 0.1f, "idleOff");
        sprite.Add("idleOff", "lightOff", 0.1f, "idleOn");
        sprite.AddLoop("constant", "lightConstant", 0.1f);
        sprite.Play("constant");
        Depth = -10001;
        Vector2 pos = new Vector2(4, -1);
        Add(new VertexLight(pos, data.HexColor("color"), 1f, data.Int("lightFadeStart"), data.Int("lightFadeEnd")));
        Collider = new Hitbox(sprite.Width, sprite.Height);
    }
}
