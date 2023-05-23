using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

// PuzzleIslandHelper.DecalEffectTarget
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/DecalEffectTarget")]
    [Tracked]
    public class DecalEffectTarget : Entity
    {

        public Rectangle bounds;
        public Sprite sprite;
        public string id;

        public DecalEffectTarget(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Depth = data.Int("depth", 1);
            float delay = 1f / (data.Float("fps")/2f);
            sprite = new Sprite(GFX.Game, "decals/");
            sprite.AddLoop("idle", data.Attr("decalPath"), delay);
            id = data.Attr("groupId");
            Add(sprite);
            sprite.Visible = false;
            Collider = new Hitbox(sprite.Width, sprite.Height);
            bounds = new Rectangle((int)Position.X, (int)Position.Y, (int)sprite.Width, (int)sprite.Height);
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            sprite.Play("idle");
        }
    }
}
