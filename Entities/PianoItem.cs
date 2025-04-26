using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities.InterfaceEntities.Transitions;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/PianoItem")]
    [Tracked]
    public class PianoItem : Entity
    {
        public EntityID ID;
        public PianoItem(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
        {
            ID = id;
            Image image = new Image(GFX.Game["objects/PuzzleIslandHelper/pianoSprite"]);
            Add(image);
            Collider = image.Collider();
            TalkComponent talk = new TalkComponent(new Rectangle(0, 0, (int)Width, (int)Height), Collider.HalfSize.XComp(), player =>
            {
                SceneAs<Level>().Session.DoNotLoad.Add(id);
                PianoModule.Session.HasPiano = true;
                RemoveSelf();
            })
            {
                PlayerMustBeFacing = false,
            };
            Add(talk);
        }
    }
}