using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;

namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [Tracked]
    [CustomEntity("PuzzleIslandHelper/FirfilOrb")]
    public class FirfilOrbCollectable : Entity
    {
        public FirfilOrbCollectable(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            Depth = 1;
            Image image = new Image(GFX.Game["objects/PuzzleIslandHelper/firfilHat/lonn"]);
            Add(image);
            Collider = image.Collider();
            Add(new GetItemComponent((p) =>
            {
                SceneAs<Level>().Session.DoNotLoad.Add(id);
            }, data.Attr("flag"), true, "You got the sparkly orb!", "The sparkly orb attracts sparkly creatures."));
        }
    }
}