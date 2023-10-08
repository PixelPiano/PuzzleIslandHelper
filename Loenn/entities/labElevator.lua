local labElevator = {}
labElevator.justification = { 0, 0 }

labElevator.name = "PuzzleIslandHelper/LabElevator"

labElevator.depth = -8500

labElevator.nodeLimits = {1,-1}
labElevator.nodeLineRenderType = "line"
labElevator.nodeVisibility = "always"
labElevator.canResize = {false, false}
labElevator.nodeLineRenderOffset = {24,0}
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
        }
    }
}

return labElevator