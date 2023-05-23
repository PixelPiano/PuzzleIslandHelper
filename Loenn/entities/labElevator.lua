local labElevator = {}
labElevator.justification = { 0, 0 }

labElevator.name = "PuzzleIslandHelper/LabElevator"

labElevator.depth = -8500

labElevator.texture = "objects/PuzzleIslandHelper/labElevator/lonn"

labElevator.placements =
{
    {
        name = "Lab Elevator",
        data = {
            flag = "LabElevatorState",
            endPosition = 0,
            moveSpeed = 4,
            jitterAmount = 1,
        }
    }
}

return labElevator