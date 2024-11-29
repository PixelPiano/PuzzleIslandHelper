using Celeste.Mod.Core;
using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Celeste.Mod.PuzzleIslandHelper.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
                        player.MoveToY(Top);
                    }
                }
                Collider = prev;
            }
        }
    }
}