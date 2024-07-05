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
        public Sprite sprite;
        public string ID;
        private string id;

        public DecalEffectTarget(EntityData data, Vector2 offset)
        : base(data.Position + offset)
        {
            Depth = data.Int("depth", 2);
            float delay = 1f / (data.Float("fps") / 2f);
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
            sprite.Rotation = data.Float("rotation").ToRad();
            sprite.CenterOrigin();
            sprite.Position += new Vector2(sprite.Width / 2, sprite.Height / 2);
            sprite.Visible = false;
            Collider = new Hitbox(sprite.Width, sprite.Height, -sprite.Width / 2, -sprite.Height / 2);
            Tag |= Tags.TransitionUpdate;
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            ID = (scene as Level).Session.Level + "_decaltargetgroup_" + id;
            sprite.Play("idle");
        }
    }
}
