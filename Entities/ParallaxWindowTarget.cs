using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.ComponentModel;

namespace Celeste.Mod.PuzzleIslandHelper.Effects
{
    [CustomEntity("PuzzleIslandHelper/ParallaxWindowTarget")]
    [Tracked]
    public class ParallaxWindowTarget : Entity
    {
        public ParallaxWindowTarget(EntityData data, Vector2 offset)
        :base(data.Position + offset)
        {
            Collider = new Hitbox(data.Width, data.Height);
        }
    }
}

