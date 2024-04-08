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
            Depth = data.Int("nDepth", 2);
            float delay = 1f / (data.Float("fps")/2f);
            sprite = new Sprite(GFX.Game, "decals/");
            string path = data.Attr("decalPath");
            if (path.Contains("decals/"))
            {
                path.Replace("decals/", "");
            }
            sprite.AddLoop("idle", data.Attr("decalPath"), 0.1f);
            id = data.Attr("groupId");
            Add(sprite);
            sprite.Scale = new Vector2(data.Float("scaleX", 1), data.Float("scaleY", 1));
            Position = new Vector2(Position.X - (sprite.Width / 2 * sprite.Scale.X), Position.Y - (sprite.Height / 2 * sprite.Scale.Y));
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
