using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Components
{

    [Tracked]
    public class BlockerComponent : Component
    {
        public Collider Collider;
        public BlockerComponent(Collider collider = null) : base(true, false)
        {
            Collider = collider;
        }
        public bool Check(Vector2 position)
        {
            Collider orig = Entity.Collider;
            if (Collider != null)
            {
                Entity.Collider = Collider;
                if (Entity.CollidePoint(position))
                {
                    Entity.Collider = orig;
                    return true;
                }
            }
            Entity.Collider = orig;
            return Entity.CollidePoint(position);
        }
        public bool Check(Entity other)
        {
            if (Entity != other)
            {
                Collider orig = Entity.Collider;
                if (Collider != null)
                {
                    Entity.Collider = Collider;
                    if (Entity.CollideCheck(other))
                    {
                        Entity.Collider = orig;
                        return true;
                    }
                }
                Entity.Collider = orig;
                return Entity.CollideCheck(other);
            }
            return false;
        }
        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            if (Entity != null)
            {
                Collider prev = Entity.Collider;
                if (Collider != null)
                {
                    Entity.Collider = Collider;
                }
                Draw.HollowRect(Entity.Collider, Color.Yellow);
                Draw.Line(Entity.TopLeft, Entity.BottomRight, Color.Yellow);
                Draw.Line(Entity.TopRight, Entity.BottomLeft, Color.Yellow);
                Entity.Collider = prev;
            }
        }
    }
}
