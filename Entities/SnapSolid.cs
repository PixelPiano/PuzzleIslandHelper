using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/SnapSolid")]
    [TrackedAs(typeof(JumpThru))]
    public class SnapSolid : JumpThru
    {
        public Collider SnapCollider;
        public SnapSolid(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height, true)
        {

        }
        public SnapSolid(Vector2 position, float width, float height, bool safe) : base(position, (int)width, safe)
        {
            SnapCollider = new Hitbox(width, height);
        }
        public override void Update()
        {
            base.Update();
            if (Collidable)
            {
                Collider prev = Collider;
                Collider = SnapCollider;
                if (CollideFirst<Player>() is Player player)
                {
                    if (player.Bottom > Top)
                    {
                        player.Y = Top;
                    }
                }
                Collider = prev;
            }
        }
    }
}