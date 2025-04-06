using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.PuzzleIslandHelper.Entities.GearEntities
{

    [CustomEntity("PuzzleIslandHelper/GearDoorActivator")]
    [TrackedAs(typeof(GearHolder))]
    public class GearDoorActivator : GearHolder
    {
        private float spins;
        public string DoorID;
        public List<GearDoor> LinkedDoors = new();
        public GearDoorActivator(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Bool("onlyOnce"), Color.MediumPurple, id, 10f)
        {
            spins = data.Float("spins", 1);
            DoorID = data.Attr("doorID");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            foreach (GearDoor door in (scene as Level).Tracker.GetEntities<GearDoor>())
            {
                if (!string.IsNullOrEmpty(door.DoorID) && DoorID == door.DoorID)
                {
                    LinkedDoors.Add(door);
                }
            }
        }
        public override IEnumerator WhileSpinning(Gear gear)
        {
            if (LinkedDoors.Count > 0)
            {
                while (Rotations < spins)
                {
                    yield return null;
                }
                foreach (GearDoor door in LinkedDoors)
                {
                    door.CheckRegister(); //add the door state to a list to prevent open doors from closing if the Player re-enters the room
                }
            }
            yield return 0.2f;
            StopSpinning();
            yield return base.WhileSpinning(gear);
        }
        public override void Update()
        {
            base.Update();
            foreach (GearDoor door in LinkedDoors)
            {
                if (!door.CanMoveBack && door.Amount >= 1) continue;
                door.Amount = Calc.Clamp(Rotations, 0, spins) / spins;
            }
        }
    }
}