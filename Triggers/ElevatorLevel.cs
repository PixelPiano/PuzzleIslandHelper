using Celeste.Mod.Entities;
using Celeste.Mod.PuzzleIslandHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.PuzzleIslandHelper.Triggers
{

    [CustomEntity("PuzzleIslandHelper/SetElevatorFloor")]
    [Tracked]
    public class ElevatorFloorTrigger : Trigger
    {
        public int Floor;
        public string ElevatorID;
        public ElevatorFloorTrigger(EntityData data, Vector2 offset)
    : base(data, offset)
        {
            Tag |= Tags.TransitionUpdate;
            Floor = data.Int("floorNumber");
            ElevatorID = data.Attr("elevatorID");
        }
        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if(CollideFirst<Player>() != null)
            {
                SetFloor();
            }
        }
        public void SetFloor()
        {
            LabElevator.SetFloor(ElevatorID, Floor);
            Triggered = true;
        }
        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            MoveToFloor();
        }
        public void MoveToFloor()
        {
            foreach(LabElevator elevator in Scene.Tracker.GetEntities<LabElevator>())
            {
                if(elevator.CanBeMoved && elevator.ID == ElevatorID && !elevator.Moving && elevator.CurrentFloor != Floor)
                {
                    elevator.Add(new Coroutine(elevator.MoveRoutine(Floor)));
                }
            }
        }

    }
}
