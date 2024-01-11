using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
// PuzzleIslandHelper.ArtifactSlot
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/Marker")]
    [Tracked]
    public class Marker : Entity
    {
        public string ID;
        public Marker(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            ID = data.Attr("markerID");
            Collider = new Hitbox(15,16);
        }
    }
}