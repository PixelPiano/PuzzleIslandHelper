local labElevator = {}
labElevator.justification = { 0, 0 }

labElevator.name = "PuzzleIslandHelper/LabElevator"

labElevator.depth = -8500

labElevator.canResize = {false, false}
labElevator.texture = "objects/PuzzleIslandHelper/labElevator/lonn"

labElevator.placements =
{
    {
        name = "Lab Elevator",
        data = {
            flag = "LabElevatorState",
            moveTime = 7,
            moveSpeed = 3,
            jitterAmount = 1,
            elevatorID = ""
        }
    }
}

return labElevator