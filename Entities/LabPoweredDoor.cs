using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
namespace Celeste.Mod.PuzzleIslandHelper.Entities
{
    [CustomEntity("PuzzleIslandHelper/LabPoweredDoor")]
    [TrackedAs(typeof(CodeDoor))]
    public class LabPoweredDoor : CodeDoor
    {
        public LabPoweredDoor(EntityData data, Vector2 offset) : base(data.Position + offset, "", data.Bool("sideways"), false)
        {
        }
        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (PianoModule.Session.RestoredPower)
            {
                RemoveSelf();
            }
        }
        public override void Update()
        {
            base.Update();
            if (PianoModule.Session.RestoredPower)
            {
                Unlock();
            }
        }
    }
}
